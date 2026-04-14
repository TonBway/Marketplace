namespace FarmMarketplace.Contracts.Buyer;

public sealed record UpsertBuyerProfileRequest(
    string DisplayName,
    int RegionId,
    int DistrictId,
    bool ReceiveSms,
    bool ReceivePush);

public sealed record BuyerProfileResponse(
    Guid UserId,
    string DisplayName,
    int RegionId,
    int DistrictId,
    bool ReceiveSms,
    bool ReceivePush);
