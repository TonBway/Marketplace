namespace FarmMarketplace.Portal.Models;

public sealed record SellerDashboardSummaryVm(int ActiveListings, int ReceivedEnquiries, string? ActivePlanName, DateTime? SubscriptionEndDateUtc);

public sealed record SellerProfileVm(Guid UserId, string BusinessName, string? Description, int RegionId, int DistrictId, string ContactMode, string? ProfileImageUrl, bool IsVerified);

public sealed class UpdateSellerProfileVm
{
    public string BusinessName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RegionId { get; set; } = 1;
    public int DistrictId { get; set; } = 1;
    public string ContactMode { get; set; } = "PHONE";
    public string? ProfileImageUrl { get; set; }
}

public sealed record ListingSummaryVm(Guid ListingId, Guid SellerUserId, string Title, decimal Price, decimal Quantity, string UnitName, string StatusCode, DateTime CreatedAtUtc, DateTime? ExpiresAtUtc);

public sealed record ListingImageVm(string ImageUrl, bool IsPrimary, int SortOrder);

public sealed record ListingDetailVm(
    Guid ListingId,
    Guid SellerUserId,
    string Title,
    string Description,
    int CategoryId,
    int ProductTypeId,
    decimal Price,
    decimal Quantity,
    int UnitId,
    int RegionId,
    int DistrictId,
    bool IsLivestock,
    string StatusCode,
    DateTime CreatedAtUtc,
    DateTime? ExpiresAtUtc,
    IReadOnlyList<ListingImageVm> Images);

public sealed record SubscriptionVm(Guid SellerSubscriptionId, int PlanId, string PlanName, DateTime StartDateUtc, DateTime EndDateUtc, string StatusCode);

public sealed record EnquiryVm(Guid EnquiryId, Guid ListingId, Guid BuyerUserId, Guid SellerUserId, string StatusCode, string Message, DateTime CreatedAtUtc);

public sealed class CreateListingVm
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; } = 1;
    public int ProductTypeId { get; set; } = 1;
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public int UnitId { get; set; } = 1;
    public int RegionId { get; set; } = 1;
    public int DistrictId { get; set; } = 1;
    public bool IsLivestock { get; set; }
}

public sealed class UpdateListingVm
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; } = 1;
    public int ProductTypeId { get; set; } = 1;
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public int UnitId { get; set; } = 1;
    public int RegionId { get; set; } = 1;
    public int DistrictId { get; set; } = 1;
    public bool IsLivestock { get; set; }
}
