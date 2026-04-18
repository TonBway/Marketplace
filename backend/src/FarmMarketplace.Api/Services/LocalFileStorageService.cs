using FarmMarketplace.Api.Options;
using FarmMarketplace.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace FarmMarketplace.Api.Services;

public sealed class LocalFileStorageService : IFileStorageService
{
    private static readonly Dictionary<string, string> ExtensionByMime = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["image/heic"] = ".heic",
        ["image/heif"] = ".heif"
    };

    private readonly string _rootPath;
    private readonly string _baseUrl;

    public LocalFileStorageService(IWebHostEnvironment environment, IOptions<LocalFileStorageOptions> options)
    {
        var config = options.Value;
        _rootPath = Path.IsPathRooted(config.ListingImagesRoot)
            ? config.ListingImagesRoot
            : Path.Combine(environment.ContentRootPath, config.ListingImagesRoot);

        _baseUrl = "/" + config.ListingImagesBaseUrl.Trim('/');
    }

    public async Task<StoredFileResult> SaveListingImageAsync(Stream content, string contentType, CancellationToken cancellationToken)
    {
        if (!ExtensionByMime.TryGetValue(contentType, out var extension))
        {
            throw new InvalidOperationException("Unsupported content type.");
        }

        Directory.CreateDirectory(_rootPath);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_rootPath, fileName);

        await using var destination = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(destination, cancellationToken);

        var relativeUrl = $"{_baseUrl}/{fileName}";
        return new StoredFileResult(relativeUrl, contentType, destination.Length, fileName);
    }
}
