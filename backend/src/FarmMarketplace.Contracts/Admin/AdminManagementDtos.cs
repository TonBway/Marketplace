namespace FarmMarketplace.Contracts.Admin;

public sealed record AdminSellerRowResponse(
    Guid UserId,
    string FullName,
    string Email,
    string Phone,
    bool IsActive,
    bool IsVerified,
    string? SubscriptionStatus,
    int ActiveListingCount,
    DateTime CreatedAtUtc);

public sealed record AdminSellerDetailResponse(
    Guid UserId,
    string FullName,
    string Email,
    string Phone,
    bool IsActive,
    bool IsVerified,
    string? BusinessName,
    string? Description,
    int? RegionId,
    int? DistrictId,
    string? ContactMode,
    string? ProfileImageUrl,
    int TotalListings,
    int TotalEnquiries,
    DateTime CreatedAtUtc);

public sealed record AdminBuyerRowResponse(
    Guid UserId,
    string FullName,
    string Email,
    string Phone,
    bool IsActive,
    int FavoritesCount,
    int EnquiryCount,
    DateTime CreatedAtUtc);

public sealed record AdminBuyerDetailResponse(
    Guid UserId,
    string FullName,
    string Email,
    string Phone,
    bool IsActive,
    string? DisplayName,
    int? RegionId,
    int? DistrictId,
    bool ReceiveSms,
    bool ReceivePush,
    int FavoritesCount,
    int EnquiryCount,
    DateTime CreatedAtUtc);

public sealed record AdminListingRowResponse(
    Guid ListingId,
    string Title,
    Guid SellerUserId,
    string SellerName,
    decimal Price,
    decimal Quantity,
    string StatusCode,
    DateTime CreatedAtUtc);

public sealed record AdminListingImageResponse(string ImageUrl, bool IsPrimary, int SortOrder);

public sealed record AdminListingStatusHistoryResponse(string StatusCode, string? Note, Guid? ChangedByUserId, DateTime ChangedAtUtc);

public sealed record AdminListingDetailResponse(
    Guid ListingId,
    string Title,
    string Description,
    Guid SellerUserId,
    string SellerName,
    int CategoryId,
    int ProductTypeId,
    decimal Price,
    decimal Quantity,
    int UnitId,
    int RegionId,
    int DistrictId,
    string StatusCode,
    bool IsLivestock,
    DateTime CreatedAtUtc,
    DateTime? ExpiresAtUtc,
    int EnquiryCount,
    int ViewCount,
    IReadOnlyList<AdminListingImageResponse> Images,
    IReadOnlyList<AdminListingStatusHistoryResponse> StatusHistory);

public sealed record AdminSubscriptionRowResponse(
    Guid SellerSubscriptionId,
    Guid SellerUserId,
    string SellerName,
    string PlanName,
    string StatusCode,
    DateTime StartDateUtc,
    DateTime EndDateUtc);

public sealed record AdminSubscriptionDetailResponse(
    Guid SellerSubscriptionId,
    Guid SellerUserId,
    string SellerName,
    int PlanId,
    string PlanName,
    string StatusCode,
    DateTime StartDateUtc,
    DateTime EndDateUtc,
    decimal TotalPaidAmount);

public sealed record AdminEnquiryRowResponse(
    Guid EnquiryId,
    Guid ListingId,
    Guid BuyerUserId,
    Guid SellerUserId,
    string StatusCode,
    DateTime CreatedAtUtc);

public sealed record AdminEnquiryDetailResponse(
    Guid EnquiryId,
    Guid ListingId,
    Guid BuyerUserId,
    Guid SellerUserId,
    string StatusCode,
    string Message,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record AdminPaymentRowResponse(
    Guid SubscriptionPaymentId,
    Guid SellerSubscriptionId,
    decimal Amount,
    string PaymentMethod,
    string PaymentStatus,
    DateTime? PaidAtUtc,
    DateTime CreatedAtUtc);

public sealed record AdminInvoiceRowResponse(
    Guid InvoiceId,
    Guid SellerSubscriptionId,
    string InvoiceNumber,
    decimal Amount,
    DateTime IssuedAtUtc,
    DateTime? DueAtUtc);

public sealed record AdminPlanRowResponse(
    int PlanId,
    string PlanName,
    decimal PriceAmount,
    int DurationDays,
    int MaxActiveListings,
    bool IsActive,
    int SortOrder);

public sealed record UpsertPlanRequest(
    string PlanName,
    decimal PriceAmount,
    int DurationDays,
    int MaxActiveListings,
    int SortOrder);

public sealed record AdminReferenceItemResponse(int Id, string Code, string Name, bool IsActive);

public sealed record UpsertReferenceItemRequest(string? Code, string Name);

public sealed record AdminSystemSettingResponse(string Key, string Value, DateTime UpdatedAtUtc);

public sealed record AdminAuditLogResponse(
    Guid AuditLogId,
    Guid? ActorUserId,
    string Action,
    string EntityType,
    string? EntityId,
    DateTime OccurredAtUtc);

public sealed record AdminModerationActionResponse(
    Guid ModerationActionId,
    string TargetType,
    string TargetId,
    string ActionCode,
    string? Reason,
    Guid? ActedByUserId,
    DateTime ActedAtUtc);

public sealed record AdminNoteResponse(
    Guid AdminNoteId,
    Guid? TargetUserId,
    string Note,
    Guid? CreatedByUserId,
    DateTime CreatedAtUtc);

public sealed record AddAdminNoteRequest(string Note);

public sealed record UpdateSystemSettingRequest(string Value);

public sealed record UpdateStatusRequest(string StatusCode, string? Reason);
