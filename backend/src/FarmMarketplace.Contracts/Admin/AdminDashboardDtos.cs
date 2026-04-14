namespace FarmMarketplace.Contracts.Admin;

public sealed record AdminDashboardSummaryResponse(
    int TotalSellers,
    int TotalBuyers,
    int TotalActiveListings,
    int TotalPendingListings,
    int TotalSoldOutListings,
    int TotalActiveSubscriptions,
    int SubscriptionsExpiringSoon,
    int TotalEnquiries);

public sealed record RecentActivityItemResponse(
    string ActivityType,
    string Title,
    DateTime CreatedAtUtc,
    string? RelatedEntityId);
