using FarmMarketplace.Contracts.Analytics;
using FarmMarketplace.Contracts.Auth;
using FarmMarketplace.Contracts.Billing;
using FarmMarketplace.Contracts.Buyer;
using FarmMarketplace.Contracts.Dashboard;
using FarmMarketplace.Contracts.Listings;
using FarmMarketplace.Contracts.Messaging;
using FarmMarketplace.Contracts.Orders;
using FarmMarketplace.Contracts.Reference;
using FarmMarketplace.Contracts.Reviews;
using FarmMarketplace.Contracts.Seller;
using FarmMarketplace.Contracts.Shipping;

namespace FarmMarketplace.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);
    Task<AuthUserProfileResponse?> GetMeAsync(Guid userId, CancellationToken cancellationToken);
    Task<RequestOtpResponse> RequestOtpAsync(RequestOtpRequest request, CancellationToken cancellationToken);
    Task ResetPasswordWithOtpAsync(ResetPasswordWithOtpRequest request, CancellationToken cancellationToken);
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
    Task<PagedResult<ListingSummaryResponse>> BrowseAsync(BrowseListingsRequest request, CancellationToken cancellationToken);
    Task<ListingDetailResponse?> GetPublicAsync(Guid listingId, CancellationToken cancellationToken);
    Task TrackViewAsync(Guid listingId, Guid? viewerUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ListingSummaryResponse>> GetMyListingsAsync(Guid sellerUserId, string? statusCode, CancellationToken cancellationToken);
    Task<ListingDetailResponse?> GetMyListingAsync(Guid sellerUserId, Guid listingId, CancellationToken cancellationToken);
    Task UpdateAsync(Guid sellerUserId, Guid listingId, UpdateListingRequest request, CancellationToken cancellationToken);
    Task UpdateStatusAsync(Guid sellerUserId, Guid listingId, UpdateListingStatusRequest request, CancellationToken cancellationToken);
    Task AddImageAsync(Guid sellerUserId, UploadListingImageRequest request, CancellationToken cancellationToken);
    Task DeleteImageAsync(Guid sellerUserId, Guid listingId, Guid imageId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ListingSummaryResponse>> GetFavoritesAsync(Guid buyerUserId, CancellationToken cancellationToken);
    Task AddFavoriteAsync(Guid buyerUserId, Guid listingId, CancellationToken cancellationToken);
    Task RemoveFavoriteAsync(Guid buyerUserId, Guid listingId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ListingShippingOptionResponse>> GetShippingOptionsAsync(Guid listingId, CancellationToken cancellationToken);
    Task UpsertShippingOptionsAsync(Guid sellerUserId, Guid listingId, UpsertListingShippingOptionsRequest request, CancellationToken cancellationToken);
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
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken);
    Task CreateNotificationAsync(Guid userId, string title, string body, CancellationToken cancellationToken);
    Task RegisterDeviceTokenAsync(Guid userId, RegisterDeviceTokenRequest request, CancellationToken cancellationToken);
}

public interface IReferenceDataService
{
    Task<IReadOnlyList<RegionResponse>> GetRegionsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<DistrictResponse>> GetDistrictsAsync(int? regionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ReferenceItemResponse>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductTypeResponse>> GetProductTypesAsync(int? categoryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ReferenceItemResponse>> GetUnitsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ShippingMethodResponse>> GetShippingMethodsAsync(CancellationToken cancellationToken);
}

public interface IDashboardService
{
    Task<SellerDashboardSummaryResponse> GetSellerSummaryAsync(Guid sellerUserId, CancellationToken cancellationToken);
    Task<BuyerDashboardSummaryResponse> GetBuyerSummaryAsync(Guid buyerUserId, CancellationToken cancellationToken);
}

// ── New feature interfaces ──────────────────────────────────────────────────────

public interface IShippingService
{
    Task<IReadOnlyList<ShippingMethodResponse>> GetMethodsAsync(CancellationToken cancellationToken);
}

public interface IOrderService
{
    Task<OrderResponse> CreateFromEnquiryAsync(Guid sellerUserId, CreateOrderRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderResponse>> GetForSellerAsync(Guid sellerUserId, string? statusCode, CancellationToken cancellationToken);
    Task<IReadOnlyList<OrderResponse>> GetForBuyerAsync(Guid buyerUserId, string? statusCode, CancellationToken cancellationToken);
    Task<OrderResponse?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);
    Task UpdateStatusAsync(Guid actorUserId, Guid orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken);
}

public interface IReviewService
{
    Task<ReviewResponse> CreateAsync(Guid buyerUserId, CreateReviewRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ReviewResponse>> GetForListingAsync(Guid listingId, CancellationToken cancellationToken);
    Task<SellerRatingSummaryResponse> GetSellerSummaryAsync(Guid sellerUserId, CancellationToken cancellationToken);
}

public interface IMessagingService
{
    Task<ConversationResponse> StartConversationAsync(Guid enquiryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ConversationResponse>> GetMyConversationsAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<MessageResponse>> GetMessagesAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken);
    Task<MessageResponse> SendMessageAsync(Guid conversationId, Guid senderUserId, SendMessageRequest request, CancellationToken cancellationToken);
    Task MarkConversationReadAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken);
}

public interface IAnalyticsService
{
    Task<SellerAnalyticsSummaryResponse> GetSellerSummaryAsync(Guid sellerUserId, CancellationToken cancellationToken);
    Task<AdminAnalyticsSummaryResponse> GetAdminSummaryAsync(CancellationToken cancellationToken);
}

public interface IPushNotificationService
{
    Task SendToUserAsync(Guid userId, string title, string body, CancellationToken cancellationToken);
}
