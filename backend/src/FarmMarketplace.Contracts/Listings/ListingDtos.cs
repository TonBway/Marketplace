namespace FarmMarketplace.Contracts.Listings;

public sealed record BrowseListingsRequest(
    string? Search,
    int? RegionId,
    int? DistrictId,
    int? CategoryId,
    int? ProductTypeId,
    decimal? MinPrice,
    decimal? MaxPrice,
    bool? IsLivestock,
    string? SortBy,
    int Page,
    int PageSize);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);

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
    string SellerName,
    string Title,
    string Description,
    decimal Price,
    decimal Quantity,
    string UnitName,
    string StatusCode,
    DateTime CreatedAtUtc,
    DateTime? ExpiresAtUtc,
    string? PrimaryImageUrl);

public sealed record ListingImageResponse(Guid ImageId, string ImageUrl, bool IsPrimary, int SortOrder);

public sealed record ListingDetailResponse(
    Guid ListingId,
    Guid SellerUserId,
    string SellerName,
    string SellerPhone,
    string SellerEmail,
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
