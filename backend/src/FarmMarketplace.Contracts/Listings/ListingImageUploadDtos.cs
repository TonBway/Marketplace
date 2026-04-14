namespace FarmMarketplace.Contracts.Listings;

public sealed record ListingImageUploadResponse(
    Guid ListingId,
    string ImageUrl,
    string ContentType,
    long SizeBytes,
    bool IsPrimary,
    int SortOrder);
