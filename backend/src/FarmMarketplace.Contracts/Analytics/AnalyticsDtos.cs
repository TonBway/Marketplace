namespace FarmMarketplace.Contracts.Analytics;

public sealed record ListingAnalyticsResponse(
    Guid ListingId,
    string Title,
    int ViewCount,
    int FavoriteCount,
    int EnquiryCount,
    int OrderCount,
    decimal TotalRevenue);

public sealed record SellerAnalyticsSummaryResponse(
    int TotalViews,
    int TotalFavorites,
    int TotalEnquiries,
    int TotalOrders,
    decimal TotalRevenue,
    IReadOnlyList<ListingAnalyticsResponse> TopListings);

public sealed record DailyActivityResponse(DateOnly Date, int NewListings, int NewEnquiries, int NewOrders);

public sealed record AdminAnalyticsSummaryResponse(
    int NewSellersThisMonth,
    int NewBuyersThisMonth,
    int NewListingsThisMonth,
    int TotalOrdersThisMonth,
    decimal TotalRevenueThisMonth,
    IReadOnlyList<DailyActivityResponse> DailyActivity);
