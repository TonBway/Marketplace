namespace FarmMarketplace.Contracts.Shipping;

public sealed record ShippingMethodResponse(int ShippingMethodId, string MethodCode, string MethodName, string? Description);

public sealed record ListingShippingOptionResponse(
    int ShippingMethodId,
    string MethodName,
    int? EstimatedDays,
    decimal? Cost,
    string? Notes);

public sealed record ShippingOptionItem(int ShippingMethodId, int? EstimatedDays, decimal? Cost, string? Notes);

public sealed record UpsertListingShippingOptionsRequest(IReadOnlyList<ShippingOptionItem> Options);
