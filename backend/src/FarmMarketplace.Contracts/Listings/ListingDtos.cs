namespace FarmMarketplace.Contracts.Listings;

public sealed record CreateListingRequest(
    string Title,
    string Description,
    int CategoryId,
    int ProductTypeId,
    decimal Price,
    decimal Quantity,
    int UnitId,
    int RegionId,
    int DistrictId,
    bool IsLivestock);

public sealed record UpdateListingRequest(
    string Title,
    string Description,
    int CategoryId,
    int ProductTypeId,
    decimal Price,
    decimal Quantity,
    int UnitId,
    int RegionId,
    int DistrictId,
    bool IsLivestock);

public sealed record UpdateListingStatusRequest(string StatusCode);

public sealed record ListingSummaryResponse(
    Guid ListingId,
    Guid SellerUserId,
    string Title,
    decimal Price,
    decimal Quantity,
    string UnitName,
    string StatusCode,
    DateTime CreatedAtUtc,
    DateTime? ExpiresAtUtc);

public sealed record ListingImageResponse(string ImageUrl, bool IsPrimary, int SortOrder);

public sealed record ListingDetailResponse(
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
    IReadOnlyList<ListingImageResponse> Images);

public sealed record UploadListingImageRequest(Guid ListingId, string ImageUrl, bool IsPrimary, int SortOrder);
