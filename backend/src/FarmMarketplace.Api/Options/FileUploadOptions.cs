namespace FarmMarketplace.Api.Options;

public sealed class FileUploadOptions
{
    public long MaxBytes { get; init; } = 5 * 1024 * 1024;

    public string[] AllowedMimeTypes { get; init; } =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/heic",
        "image/heif"
    ];
}
