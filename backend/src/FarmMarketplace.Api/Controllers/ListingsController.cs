using FarmMarketplace.Api.Extensions;
using FarmMarketplace.Api.Options;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Listings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FarmMarketplace.Api.Controllers;

[ApiController]
[Route("api/listings")]
public sealed class ListingsController : ControllerBase
{
    private readonly IListingService _service;
    private readonly IFileStorageService _fileStorageService;
    private readonly FileUploadOptions _fileUploadOptions;

    public ListingsController(
        IListingService service,
        IFileStorageService fileStorageService,
        IOptions<FileUploadOptions> fileUploadOptions)
    {
        _service = service;
        _fileStorageService = fileStorageService;
        _fileUploadOptions = fileUploadOptions.Value;
    }

    [HttpPost]
    [Authorize(Roles = "SELLER")]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateListingRequest request, CancellationToken cancellationToken)
    {
        var listingId = await _service.CreateAsync(User.GetRequiredUserId(), request, cancellationToken);
        return Ok(listingId);
    }

    [HttpGet("my")]
    [Authorize(Roles = "SELLER")]
    public async Task<ActionResult<IReadOnlyList<ListingSummaryResponse>>> GetMy([FromQuery] string? statusCode, CancellationToken cancellationToken)
    {
        var listings = await _service.GetMyListingsAsync(User.GetRequiredUserId(), statusCode, cancellationToken);
        return Ok(listings);
    }

    [HttpGet("my/{listingId:guid}")]
    [Authorize(Roles = "SELLER")]
    public async Task<ActionResult<ListingDetailResponse>> GetMyListing(Guid listingId, CancellationToken cancellationToken)
    {
        var listing = await _service.GetMyListingAsync(User.GetRequiredUserId(), listingId, cancellationToken);
        return listing is null ? NotFound() : Ok(listing);
    }

    [HttpPut("{listingId:guid}")]
    [Authorize(Roles = "SELLER")]
    public async Task<IActionResult> Update(Guid listingId, [FromBody] UpdateListingRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateAsync(User.GetRequiredUserId(), listingId, request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{listingId:guid}/status")]
    [Authorize(Roles = "SELLER")]
    public async Task<IActionResult> UpdateStatus(Guid listingId, [FromBody] UpdateListingStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateStatusAsync(User.GetRequiredUserId(), listingId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{listingId:guid}/images/upload")]
    [Authorize(Roles = "SELLER")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<ListingImageUploadResponse>> UploadImage(
        Guid listingId,
        [FromForm] IFormFile file,
        [FromForm] bool isPrimary,
        [FromForm] int sortOrder,
        CancellationToken cancellationToken)
    {
        if (file.Length <= 0)
        {
            return BadRequest(new { error = "File is required." });
        }

        if (file.Length > _fileUploadOptions.MaxBytes)
        {
            return BadRequest(new { error = $"File exceeds maximum allowed size of {_fileUploadOptions.MaxBytes} bytes." });
        }

        if (sortOrder < 0)
        {
            return BadRequest(new { error = "Sort order must be zero or greater." });
        }

        var allowedMimeTypes = _fileUploadOptions.AllowedMimeTypes
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!allowedMimeTypes.Contains(file.ContentType))
        {
            return BadRequest(new { error = "Unsupported file type." });
        }

        if (!await HasValidSignatureAsync(file, file.ContentType, cancellationToken))
        {
            return BadRequest(new { error = "File signature does not match content type." });
        }

        await using var fileStream = file.OpenReadStream();
        var storedFile = await _fileStorageService.SaveListingImageAsync(fileStream, file.ContentType, cancellationToken);

        var addImageRequest = new UploadListingImageRequest(listingId, storedFile.PublicUrl, isPrimary, sortOrder);
        await _service.AddImageAsync(User.GetRequiredUserId(), addImageRequest, cancellationToken);

        return Ok(new ListingImageUploadResponse(listingId, storedFile.PublicUrl, storedFile.ContentType, storedFile.SizeBytes, isPrimary, sortOrder));
    }

    private static async Task<bool> HasValidSignatureAsync(IFormFile file, string contentType, CancellationToken cancellationToken)
    {
        var maxSignatureLength = 12;
        var signature = new byte[maxSignatureLength];

        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(signature.AsMemory(0, maxSignatureLength), cancellationToken);
        if (read <= 0)
        {
            return false;
        }

        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => read >= 3 && signature[0] == 0xFF && signature[1] == 0xD8 && signature[2] == 0xFF,
            "image/png" => read >= 8 && signature[0] == 0x89 && signature[1] == 0x50 && signature[2] == 0x4E && signature[3] == 0x47 && signature[4] == 0x0D && signature[5] == 0x0A && signature[6] == 0x1A && signature[7] == 0x0A,
            "image/webp" => read >= 12 && signature[0] == 0x52 && signature[1] == 0x49 && signature[2] == 0x46 && signature[3] == 0x46 && signature[8] == 0x57 && signature[9] == 0x45 && signature[10] == 0x42 && signature[11] == 0x50,
            _ => false
        };
    }
}
