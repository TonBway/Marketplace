namespace FarmMarketplace.Application.Interfaces;

public sealed record StoredFileResult(string PublicUrl, string ContentType, long SizeBytes, string StoredFileName);

public interface IFileStorageService
{
    Task<StoredFileResult> SaveListingImageAsync(Stream content, string contentType, CancellationToken cancellationToken);
}
