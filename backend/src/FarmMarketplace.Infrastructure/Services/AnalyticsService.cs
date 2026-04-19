using Dapper;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Analytics;
using FarmMarketplace.Infrastructure.Data;

namespace FarmMarketplace.Infrastructure.Services;

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AnalyticsService(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<SellerAnalyticsSummaryResponse> GetSellerSummaryAsync(Guid sellerUserId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string totSql = @"
select
    (select count(1) from marketplace.listing_views v inner join marketplace.listings l on l.listing_id = v.listing_id where l.seller_user_id = @UserId) as TotalViews,
    (select count(1) from marketplace.listing_favorites f inner join marketplace.listings l on l.listing_id = f.listing_id where l.seller_user_id = @UserId) as TotalFavorites,
    (select count(1) from messaging.enquiries where seller_user_id = @UserId) as TotalEnquiries,
    (select count(1) from marketplace.orders where seller_user_id = @UserId) as TotalOrders,
    (select coalesce(sum(total_amount), 0) from marketplace.orders where seller_user_id = @UserId and order_status not in ('CANCELLED')) as TotalRevenue";

        var totals = await connection.QuerySingleAsync<(long TotalViews, long TotalFavorites, long TotalEnquiries, long TotalOrders, decimal TotalRevenue)>(
            new CommandDefinition(totSql, new { UserId = sellerUserId }, cancellationToken: cancellationToken));

        const string topSql = @"
select l.listing_id as ListingId, l.title as Title,
       (select count(1) from marketplace.listing_views v where v.listing_id = l.listing_id) as ViewCount,
       (select count(1) from marketplace.listing_favorites f where f.listing_id = l.listing_id) as FavoriteCount,
       (select count(1) from messaging.enquiries e where e.listing_id = l.listing_id) as EnquiryCount,
       (select count(1) from marketplace.orders o where o.listing_id = l.listing_id and o.order_status != 'CANCELLED') as OrderCount,
       (select coalesce(sum(o.total_amount), 0) from marketplace.orders o where o.listing_id = l.listing_id and o.order_status != 'CANCELLED') as TotalRevenue
from marketplace.listings l
where l.seller_user_id = @UserId
order by ViewCount desc
limit 10";
        var top = (await connection.QueryAsync<ListingAnalyticsResponse>(new CommandDefinition(topSql,
            new { UserId = sellerUserId }, cancellationToken: cancellationToken))).ToList();

        return new SellerAnalyticsSummaryResponse(
            (int)totals.TotalViews, (int)totals.TotalFavorites, (int)totals.TotalEnquiries,
            (int)totals.TotalOrders, totals.TotalRevenue, top);
    }

    public async Task<AdminAnalyticsSummaryResponse> GetAdminSummaryAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string totSql = @"
select
    (select count(1) from auth.users u inner join auth.roles r on r.role_id = u.role_id where r.role_code = 'SELLER' and date_trunc('month', u.created_at_utc) = date_trunc('month', now())) as NewSellersThisMonth,
    (select count(1) from auth.users u inner join auth.roles r on r.role_id = u.role_id where r.role_code = 'BUYER' and date_trunc('month', u.created_at_utc) = date_trunc('month', now())) as NewBuyersThisMonth,
    (select count(1) from marketplace.listings where date_trunc('month', created_at_utc) = date_trunc('month', now())) as NewListingsThisMonth,
    (select count(1) from marketplace.orders where date_trunc('month', created_at_utc) = date_trunc('month', now())) as TotalOrdersThisMonth,
    (select coalesce(sum(total_amount), 0) from marketplace.orders where date_trunc('month', created_at_utc) = date_trunc('month', now()) and order_status != 'CANCELLED') as TotalRevenueThisMonth";

        var totals = await connection.QuerySingleAsync<(int NewSellersThisMonth, int NewBuyersThisMonth, int NewListingsThisMonth, int TotalOrdersThisMonth, decimal TotalRevenueThisMonth)>(
            new CommandDefinition(totSql, cancellationToken: cancellationToken));

        const string dailySql = @"
select
    date_trunc('day', gs.day)::date as Date,
    (select count(1) from marketplace.listings where date_trunc('day', created_at_utc) = date_trunc('day', gs.day)) as NewListings,
    (select count(1) from messaging.enquiries where date_trunc('day', created_at_utc) = date_trunc('day', gs.day)) as NewEnquiries,
    (select count(1) from marketplace.orders where date_trunc('day', created_at_utc) = date_trunc('day', gs.day)) as NewOrders
from generate_series(now() - interval '29 days', now(), interval '1 day') gs(day)
order by gs.day";

        var daily = (await connection.QueryAsync<DailyActivityRow>(new CommandDefinition(dailySql,
            cancellationToken: cancellationToken)))
            .Select(r => new DailyActivityResponse(r.Date, r.NewListings, r.NewEnquiries, r.NewOrders))
            .ToList();

        return new AdminAnalyticsSummaryResponse(
            totals.NewSellersThisMonth, totals.NewBuyersThisMonth, totals.NewListingsThisMonth,
            totals.TotalOrdersThisMonth, totals.TotalRevenueThisMonth, daily);
    }

    private sealed record DailyActivityRow(DateOnly Date, int NewListings, int NewEnquiries, int NewOrders);
}
