namespace FarmMarketplace.Api.Options;

public sealed class LocalFileStorageOptions
{
    public string ListingImagesRoot { get; init; } = "wwwroot/uploads/listings";

    public string ListingImagesBaseUrl { get; init; } = "/uploads/listings";
}
