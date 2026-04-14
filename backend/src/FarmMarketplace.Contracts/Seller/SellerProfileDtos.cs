namespace FarmMarketplace.Contracts.Seller;

public sealed record UpsertSellerProfileRequest(
    string BusinessName,
    string? Description,
    int RegionId,
    int DistrictId,
    string ContactMode,
    string? ProfileImageUrl);

public sealed record SellerProfileResponse(
    Guid UserId,
    string BusinessName,
    string? Description,
    int RegionId,
    int DistrictId,
    string ContactMode,
    string? ProfileImageUrl,
    bool IsVerified);
