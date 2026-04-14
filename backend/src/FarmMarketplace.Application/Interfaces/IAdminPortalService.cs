using FarmMarketplace.Contracts.Admin;

namespace FarmMarketplace.Application.Interfaces;

public interface IAdminPortalService
{
    Task<AdminDashboardSummaryResponse> GetDashboardSummaryAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<RecentActivityItemResponse>> GetRecentActivityAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminSellerRowResponse>> GetSellersAsync(string? search, string? statusCode, CancellationToken cancellationToken);
    Task<AdminSellerDetailResponse?> GetSellerAsync(Guid sellerUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminBuyerRowResponse>> GetBuyersAsync(string? search, string? statusCode, CancellationToken cancellationToken);
    Task<AdminBuyerDetailResponse?> GetBuyerAsync(Guid buyerUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminListingRowResponse>> GetListingsAsync(string? search, string? statusCode, CancellationToken cancellationToken);
    Task<AdminListingDetailResponse?> GetListingAsync(Guid listingId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminSubscriptionRowResponse>> GetSubscriptionsAsync(string? statusCode, CancellationToken cancellationToken);
    Task<AdminSubscriptionDetailResponse?> GetSubscriptionAsync(Guid sellerSubscriptionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminPaymentRowResponse>> GetPaymentsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminInvoiceRowResponse>> GetInvoicesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminPlanRowResponse>> GetPlansAsync(CancellationToken cancellationToken);
    Task<int> CreatePlanAsync(UpsertPlanRequest request, Guid actorUserId, CancellationToken cancellationToken);
    Task UpdatePlanAsync(int planId, UpsertPlanRequest request, Guid actorUserId, CancellationToken cancellationToken);
    Task DeactivatePlanAsync(int planId, Guid actorUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminEnquiryRowResponse>> GetEnquiriesAsync(string? statusCode, CancellationToken cancellationToken);
    Task<AdminEnquiryDetailResponse?> GetEnquiryAsync(Guid enquiryId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminReferenceItemResponse>> GetReferenceItemsAsync(string referenceType, CancellationToken cancellationToken);
    Task<int> CreateReferenceItemAsync(string referenceType, UpsertReferenceItemRequest request, Guid actorUserId, CancellationToken cancellationToken);
    Task UpdateReferenceItemAsync(string referenceType, int id, UpsertReferenceItemRequest request, Guid actorUserId, CancellationToken cancellationToken);
    Task DeactivateReferenceItemAsync(string referenceType, int id, Guid actorUserId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminSystemSettingResponse>> GetSettingsAsync(CancellationToken cancellationToken);
    Task UpdateSettingAsync(string key, UpdateSystemSettingRequest request, Guid actorUserId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminAuditLogResponse>> GetAuditLogsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminModerationActionResponse>> GetModerationActionsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminNoteResponse>> GetAdminNotesAsync(CancellationToken cancellationToken);

    Task UpdateSellerStatusAsync(Guid sellerUserId, string actionCode, UpdateStatusRequest request, Guid actorUserId, CancellationToken cancellationToken);
    Task UpdateListingStatusAsync(Guid listingId, string actionCode, UpdateStatusRequest request, Guid actorUserId, CancellationToken cancellationToken);
    Task UpdateSubscriptionStatusAsync(Guid sellerSubscriptionId, string actionCode, UpdateStatusRequest request, Guid actorUserId, CancellationToken cancellationToken);
    Task UpdateEnquiryStatusAsync(Guid enquiryId, string actionCode, UpdateStatusRequest request, Guid actorUserId, CancellationToken cancellationToken);

    Task AddSellerNoteAsync(Guid sellerUserId, AddAdminNoteRequest request, Guid actorUserId, CancellationToken cancellationToken);
    Task AddBuyerNoteAsync(Guid buyerUserId, AddAdminNoteRequest request, Guid actorUserId, CancellationToken cancellationToken);
    Task AddListingNoteAsync(Guid listingId, AddAdminNoteRequest request, Guid actorUserId, CancellationToken cancellationToken);
}
