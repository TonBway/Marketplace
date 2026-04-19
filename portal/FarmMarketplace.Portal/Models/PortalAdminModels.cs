namespace FarmMarketplace.Portal.Models;

public sealed record AdminDashboardSummaryVm(
    int TotalSellers,
    int TotalBuyers,
    int TotalActiveListings,
    int TotalPendingListings,
    int TotalSoldOutListings,
    int TotalActiveSubscriptions,
    int SubscriptionsExpiringSoon,
    int TotalEnquiries);

public sealed record RecentActivityVm(string ActivityType, string Title, DateTime CreatedAtUtc, string? RelatedEntityId);

public sealed record AdminSellerVm(Guid UserId, string FullName, string Email, string Phone, bool IsActive, bool IsVerified, string? SubscriptionStatus, int ActiveListingCount, DateTime CreatedAtUtc);
public sealed record AdminSellerDetailVm(Guid UserId, string FullName, string Email, string Phone, bool IsActive, bool IsVerified, string? BusinessName, string? Description, int? RegionId, int? DistrictId, string? ContactMode, string? ProfileImageUrl, int TotalListings, int TotalEnquiries, DateTime CreatedAtUtc);
public sealed record AdminBuyerVm(Guid UserId, string FullName, string Email, string Phone, bool IsActive, int FavoritesCount, int EnquiryCount, DateTime CreatedAtUtc);
public sealed record AdminBuyerDetailVm(Guid UserId, string FullName, string Email, string Phone, bool IsActive, string? DisplayName, int? RegionId, int? DistrictId, bool ReceiveSms, bool ReceivePush, int FavoritesCount, int EnquiryCount, DateTime CreatedAtUtc);
public sealed record AdminListingVm(Guid ListingId, string Title, Guid SellerUserId, string SellerName, decimal Price, decimal Quantity, string StatusCode, DateTime CreatedAtUtc);
public sealed record AdminListingImageVm(string ImageUrl, bool IsPrimary, int SortOrder);
public sealed record AdminListingStatusHistoryVm(string StatusCode, string? Note, Guid? ChangedByUserId, DateTime ChangedAtUtc);
public sealed record AdminListingDetailVm(Guid ListingId, string Title, string Description, Guid SellerUserId, string SellerName, int CategoryId, int ProductTypeId, decimal Price, decimal Quantity, int UnitId, int RegionId, int DistrictId, string StatusCode, bool IsLivestock, DateTime CreatedAtUtc, DateTime? ExpiresAtUtc, int EnquiryCount, int ViewCount, IReadOnlyList<AdminListingImageVm> Images, IReadOnlyList<AdminListingStatusHistoryVm> StatusHistory);
public sealed record AdminSubscriptionVm(Guid SellerSubscriptionId, Guid SellerUserId, string SellerName, string PlanName, string StatusCode, DateTime StartDateUtc, DateTime EndDateUtc);
public sealed record AdminSubscriptionDetailVm(Guid SellerSubscriptionId, Guid SellerUserId, string SellerName, int PlanId, string PlanName, string StatusCode, DateTime StartDateUtc, DateTime EndDateUtc, decimal TotalPaidAmount);
public sealed record AdminEnquiryVm(Guid EnquiryId, Guid ListingId, Guid BuyerUserId, Guid SellerUserId, string StatusCode, DateTime CreatedAtUtc);
public sealed record AdminEnquiryDetailVm(Guid EnquiryId, Guid ListingId, Guid BuyerUserId, Guid SellerUserId, string StatusCode, string Message, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);
public sealed record AdminSettingVm(string Key, string Value, DateTime UpdatedAtUtc);
public sealed record AdminAuditLogVm(Guid AuditLogId, Guid? ActorUserId, string Action, string EntityType, string? EntityId, DateTime OccurredAtUtc);
public sealed record AdminModerationActionVm(Guid ModerationActionId, string TargetType, string TargetId, string ActionCode, string? Reason, Guid? ActedByUserId, DateTime ActedAtUtc);
public sealed record AdminNoteVm(Guid AdminNoteId, Guid? TargetUserId, string Note, Guid? CreatedByUserId, DateTime CreatedAtUtc);
public sealed record AdminPaymentVm(Guid SubscriptionPaymentId, Guid SellerSubscriptionId, decimal Amount, string PaymentMethod, string PaymentStatus, DateTime? PaidAtUtc, DateTime CreatedAtUtc);
public sealed record AdminInvoiceVm(Guid InvoiceId, Guid SellerSubscriptionId, string InvoiceNumber, decimal Amount, DateTime IssuedAtUtc, DateTime? DueAtUtc);
public sealed record AdminPlanVm(int PlanId, string PlanName, decimal PriceAmount, int DurationDays, int MaxActiveListings, bool IsActive, int SortOrder);
public sealed class UpsertPlanVm
{
    public string PlanName { get; set; } = string.Empty;
    public decimal PriceAmount { get; set; }
    public int DurationDays { get; set; } = 30;
    public int MaxActiveListings { get; set; } = 10;
    public int SortOrder { get; set; }
}
public sealed record AdminReferenceItemVm(int Id, string Code, string Name, bool IsActive);
public sealed record UpsertReferenceItemVm(string? Code, string Name);

// ── New feature models ────────────────────────────────────────────────────────

public sealed record OrderVm(
    Guid OrderId, Guid? EnquiryId,
    Guid BuyerUserId, string BuyerName,
    Guid SellerUserId, string SellerName,
    Guid ListingId, string ListingTitle,
    decimal Quantity, decimal UnitPrice, decimal TotalAmount,
    int? ShippingMethodId, string? ShippingMethodName, decimal ShippingCost,
    string OrderStatus, string? BuyerNotes, string? SellerNotes,
    DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);

public sealed record ReviewVm(
    Guid ReviewId, Guid ListingId,
    Guid ReviewerUserId, string ReviewerName,
    int Rating, string? Comment, DateTime CreatedAtUtc);

public sealed record SellerRatingSummaryVm(double AverageRating, int TotalReviews, IReadOnlyList<ReviewVm> RecentReviews);

public sealed record AnalyticsSummaryVm(
    int NewSellersThisMonth, int NewBuyersThisMonth, int NewListingsThisMonth,
    int TotalOrdersThisMonth, decimal TotalRevenueThisMonth,
    IReadOnlyList<DailyActivityVm> DailyActivity);

public sealed record DailyActivityVm(DateOnly Date, int NewListings, int NewEnquiries, int NewOrders);

public sealed record ListingAnalyticsVm(
    Guid ListingId, string Title,
    int ViewCount, int FavoriteCount, int EnquiryCount, int OrderCount, decimal TotalRevenue);

public sealed record ShippingMethodVm(int ShippingMethodId, string MethodCode, string MethodName, string? Description);

