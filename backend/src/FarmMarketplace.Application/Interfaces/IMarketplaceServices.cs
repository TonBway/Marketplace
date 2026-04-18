using FarmMarketplace.Contracts.Auth;
using FarmMarketplace.Contracts.Billing;
using FarmMarketplace.Contracts.Buyer;
using FarmMarketplace.Contracts.Dashboard;
using FarmMarketplace.Contracts.Listings;
using FarmMarketplace.Contracts.Messaging;
using FarmMarketplace.Contracts.Reference;
using FarmMarketplace.Contracts.Seller;

namespace FarmMarketplace.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
}

public interface ISellerProfileService
{
    Task<SellerProfileResponse?> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task<SellerProfileResponse> UpsertAsync(Guid userId, UpsertSellerProfileRequest request, CancellationToken cancellationToken);
}

public interface IBuyerProfileService
{
    Task<BuyerProfileResponse?> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task<BuyerProfileResponse> UpsertAsync(Guid userId, UpsertBuyerProfileRequest request, CancellationToken cancellationToken);
}

public interface IListingService
{
    Task<Guid> CreateAsync(Guid sellerUserId, CreateListingRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ListingSummaryResponse>> BrowseAsync(string? search, int? regionId, int? categoryId, CancellationToken cancellationToken);
    Task<ListingDetailResponse?> GetPublicAsync(Guid listingId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ListingSummaryResponse>> GetMyListingsAsync(Guid sellerUserId, string? statusCode, CancellationToken cancellationToken);
    Task<ListingDetailResponse?> GetMyListingAsync(Guid sellerUserId, Guid listingId, CancellationToken cancellationToken);
    Task UpdateAsync(Guid sellerUserId, Guid listingId, UpdateListingRequest request, CancellationToken cancellationToken);
    Task UpdateStatusAsync(Guid sellerUserId, Guid listingId, UpdateListingStatusRequest request, CancellationToken cancellationToken);
    Task AddImageAsync(Guid sellerUserId, UploadListingImageRequest request, CancellationToken cancellationToken);
}

public interface ISubscriptionService
{
    Task<IReadOnlyList<SubscriptionPlanResponse>> GetPlansAsync(CancellationToken cancellationToken);
    Task<ActiveSubscriptionResponse?> GetActiveForSellerAsync(Guid sellerUserId, CancellationToken cancellationToken);
    Task<ActiveSubscriptionResponse> SubscribeAsync(Guid sellerUserId, SubscribeRequest request, CancellationToken cancellationToken);
}

public interface IEnquiryService
{
    Task<EnquiryResponse> CreateAsync(Guid buyerUserId, CreateEnquiryRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<EnquiryResponse>> GetForSellerAsync(Guid sellerUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<EnquiryResponse>> GetForBuyerAsync(Guid buyerUserId, CancellationToken cancellationToken);
    Task UpdateStatusAsync(Guid sellerUserId, Guid enquiryId, UpdateEnquiryStatusRequest request, CancellationToken cancellationToken);
}

public interface INotificationService
{
    Task<IReadOnlyList<NotificationResponse>> GetMyNotificationsAsync(Guid userId, CancellationToken cancellationToken);
}

public interface IReferenceDataService
{
    Task<IReadOnlyList<RegionResponse>> GetRegionsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<DistrictResponse>> GetDistrictsAsync(int? regionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ReferenceItemResponse>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductTypeResponse>> GetProductTypesAsync(int? categoryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ReferenceItemResponse>> GetUnitsAsync(CancellationToken cancellationToken);
}

public interface IDashboardService
{
    Task<SellerDashboardSummaryResponse> GetSellerSummaryAsync(Guid sellerUserId, CancellationToken cancellationToken);
    Task<BuyerDashboardSummaryResponse> GetBuyerSummaryAsync(Guid buyerUserId, CancellationToken cancellationToken);
}
