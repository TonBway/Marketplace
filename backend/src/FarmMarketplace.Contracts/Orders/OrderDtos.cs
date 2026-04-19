namespace FarmMarketplace.Contracts.Orders;

public sealed record CreateOrderRequest(
    Guid EnquiryId,
    decimal Quantity,
    decimal UnitPrice,
    int? ShippingMethodId,
    decimal ShippingCost,
    string? BuyerNotes);

public sealed record UpdateOrderStatusRequest(string StatusCode, string? Note);

public sealed record OrderResponse(
    Guid OrderId,
    Guid? EnquiryId,
    Guid BuyerUserId,
    string BuyerName,
    Guid SellerUserId,
    string SellerName,
    Guid ListingId,
    string ListingTitle,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalAmount,
    int? ShippingMethodId,
    string? ShippingMethodName,
    decimal ShippingCost,
    string OrderStatus,
    string? BuyerNotes,
    string? SellerNotes,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record OrderStatusHistoryResponse(string StatusCode, string? Note, Guid? ChangedByUserId, DateTime ChangedAtUtc);
