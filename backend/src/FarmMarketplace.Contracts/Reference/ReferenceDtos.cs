namespace FarmMarketplace.Contracts.Reference;

public sealed record ReferenceItemResponse(int Id, string Code, string Name);

public sealed record RegionResponse(int RegionId, string RegionName);

public sealed record DistrictResponse(int DistrictId, int RegionId, string DistrictName);
