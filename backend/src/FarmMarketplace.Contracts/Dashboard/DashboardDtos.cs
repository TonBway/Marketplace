namespace FarmMarketplace.Contracts.Dashboard;

public sealed record SellerDashboardSummaryResponse(int ActiveListings, int ReceivedEnquiries, string? ActivePlanName, DateTime? SubscriptionEndDateUtc);

public sealed record BuyerDashboardSummaryResponse(int FavoriteCount, int SentEnquiries, int AvailableCredits);
