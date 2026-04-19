using Dapper;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Orders;
using FarmMarketplace.Infrastructure.Data;

namespace FarmMarketplace.Infrastructure.Services;

public sealed class OrderService : IOrderService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly INotificationService _notifications;

    public OrderService(IDbConnectionFactory connectionFactory, INotificationService notifications)
    {
        _connectionFactory = connectionFactory;
        _notifications = notifications;
    }

    private const string SelectOrderSql = @"
select o.order_id as OrderId, o.enquiry_id as EnquiryId,
       o.buyer_user_id as BuyerUserId, bu.full_name as BuyerName,
       o.seller_user_id as SellerUserId, su.full_name as SellerName,
       o.listing_id as ListingId, l.title as ListingTitle,
       o.quantity as Quantity, o.unit_price as UnitPrice, o.total_amount as TotalAmount,
       o.shipping_method_id as ShippingMethodId, sm.method_name as ShippingMethodName,
       o.shipping_cost as ShippingCost, o.order_status as OrderStatus,
       o.buyer_notes as BuyerNotes, o.seller_notes as SellerNotes,
       o.created_at_utc as CreatedAtUtc, o.updated_at_utc as UpdatedAtUtc
from marketplace.orders o
inner join auth.users bu on bu.user_id = o.buyer_user_id
inner join auth.users su on su.user_id = o.seller_user_id
inner join marketplace.listings l on l.listing_id = o.listing_id
left join catalog.shipping_methods sm on sm.shipping_method_id = o.shipping_method_id";

    public async Task<OrderResponse> CreateFromEnquiryAsync(Guid sellerUserId, CreateOrderRequest request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string enquirySql = @"
select e.buyer_user_id as BuyerUserId, e.listing_id as ListingId
from messaging.enquiries e
where e.enquiry_id = @EnquiryId and e.seller_user_id = @SellerUserId";
        var enquiry = await connection.QuerySingleOrDefaultAsync<(Guid BuyerUserId, Guid ListingId)>(
            new CommandDefinition(enquirySql, new { request.EnquiryId, SellerUserId = sellerUserId }, cancellationToken: cancellationToken));

        if (enquiry == default)
            throw new InvalidOperationException("Enquiry not found.");

        var total = request.Quantity * request.UnitPrice + request.ShippingCost;
        var orderId = Guid.NewGuid();

        const string insertSql = @"
insert into marketplace.orders (order_id, enquiry_id, buyer_user_id, seller_user_id, listing_id, quantity, unit_price,
    total_amount, shipping_method_id, shipping_cost, order_status, buyer_notes, created_at_utc)
values (@OrderId, @EnquiryId, @BuyerUserId, @SellerUserId, @ListingId, @Quantity, @UnitPrice,
    @TotalAmount, @ShippingMethodId, @ShippingCost, 'PENDING', @BuyerNotes, now())";

        await connection.ExecuteAsync(new CommandDefinition(insertSql, new
        {
            OrderId = orderId,
            request.EnquiryId,
            enquiry.BuyerUserId,
            SellerUserId = sellerUserId,
            enquiry.ListingId,
            request.Quantity,
            request.UnitPrice,
            TotalAmount = total,
            request.ShippingMethodId,
            request.ShippingCost,
            request.BuyerNotes
        }, cancellationToken: cancellationToken));

        const string histSql = @"
insert into marketplace.order_status_history (order_id, status_code, note, changed_by_user_id, changed_at_utc)
values (@OrderId, 'PENDING', 'Order created', @ActorUserId, now())";
        await connection.ExecuteAsync(new CommandDefinition(histSql,
            new { OrderId = orderId, ActorUserId = sellerUserId }, cancellationToken: cancellationToken));

        await _notifications.CreateNotificationAsync(enquiry.BuyerUserId,
            "Order Placed", "A seller has created an order from your enquiry.", cancellationToken);

        return (await GetByIdAsync(orderId, cancellationToken))!;
    }

    public async Task<IReadOnlyList<OrderResponse>> GetForSellerAsync(Guid sellerUserId, string? statusCode, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = SelectOrderSql + @"
where o.seller_user_id = @SellerUserId
  and (@StatusCode is null or o.order_status = @StatusCode)
order by o.created_at_utc desc";
        var rows = await connection.QueryAsync<OrderResponse>(new CommandDefinition(sql,
            new { SellerUserId = sellerUserId, StatusCode = statusCode }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<IReadOnlyList<OrderResponse>> GetForBuyerAsync(Guid buyerUserId, string? statusCode, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = SelectOrderSql + @"
where o.buyer_user_id = @BuyerUserId
  and (@StatusCode is null or o.order_status = @StatusCode)
order by o.created_at_utc desc";
        var rows = await connection.QueryAsync<OrderResponse>(new CommandDefinition(sql,
            new { BuyerUserId = buyerUserId, StatusCode = statusCode }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = SelectOrderSql + " where o.order_id = @OrderId";
        return await connection.QuerySingleOrDefaultAsync<OrderResponse>(new CommandDefinition(sql,
            new { OrderId = orderId }, cancellationToken: cancellationToken));
    }

    public async Task UpdateStatusAsync(Guid actorUserId, Guid orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string updateSql = @"
update marketplace.orders
set order_status = @StatusCode, updated_at_utc = now()
where order_id = @OrderId and (seller_user_id = @ActorUserId or buyer_user_id = @ActorUserId)";

        var affected = await connection.ExecuteAsync(new CommandDefinition(updateSql,
            new { OrderId = orderId, StatusCode = request.StatusCode, ActorUserId = actorUserId }, cancellationToken: cancellationToken));

        if (affected == 0)
            throw new InvalidOperationException("Order not found.");

        const string histSql = @"
insert into marketplace.order_status_history (order_id, status_code, note, changed_by_user_id, changed_at_utc)
values (@OrderId, @StatusCode, @Note, @ActorUserId, now())";
        await connection.ExecuteAsync(new CommandDefinition(histSql,
            new { OrderId = orderId, StatusCode = request.StatusCode, request.Note, ActorUserId = actorUserId }, cancellationToken: cancellationToken));

        const string partiesSql = "select buyer_user_id as BuyerUserId, seller_user_id as SellerUserId from marketplace.orders where order_id = @OrderId";
        var parties = await connection.QuerySingleOrDefaultAsync<(Guid BuyerUserId, Guid SellerUserId)>(
            new CommandDefinition(partiesSql, new { OrderId = orderId }, cancellationToken: cancellationToken));
        var notifyUserId = parties.SellerUserId == actorUserId ? parties.BuyerUserId : parties.SellerUserId;
        await _notifications.CreateNotificationAsync(notifyUserId,
            "Order Updated", $"Your order status has changed to {request.StatusCode.Replace('_', ' ')}.", cancellationToken);
    }
}
