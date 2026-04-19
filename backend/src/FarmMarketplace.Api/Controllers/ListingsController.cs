using FarmMarketplace.Api.Extensions;
using FarmMarketplace.Api.Options;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Listings;
using FarmMarketplace.Contracts.Shipping;
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

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ListingSummaryResponse>>> Browse(
        [FromQuery] BrowseListingsRequest request,
        CancellationToken cancellationToken)
    {
        var listings = await _service.BrowseAsync(request, cancellationToken);
        return Ok(listings);
    }

    [HttpGet("{listingId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ListingDetailResponse>> GetPublicListing(Guid listingId, CancellationToken cancellationToken)
    {
        var listing = await _service.GetPublicAsync(listingId, cancellationToken);
        if (listing is null) return NotFound();
        _ = _service.TrackViewAsync(listingId, null, cancellationToken);
        return Ok(listing);
    }

    [HttpGet("{listingId:guid}/shipping")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ListingShippingOptionResponse>>> GetShipping(Guid listingId, CancellationToken cancellationToken)
    {
        var options = await _service.GetShippingOptionsAsync(listingId, cancellationToken);
        return Ok(options);
    }

    [HttpPut("{listingId:guid}/shipping")]
    [Authorize(Roles = "SELLER")]
    public async Task<IActionResult> UpsertShipping(Guid listingId, [FromBody] UpsertListingShippingOptionsRequest request, CancellationToken cancellationToken)
    {
        await _service.UpsertShippingOptionsAsync(User.GetRequiredUserId(), listingId, request, cancellationToken);
        return NoContent();
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

    // Swashbuckle 9.x requires IFormFile and companion [FromForm] fields to be
    // wrapped in a single model bound with [FromForm] — individual [FromForm]
    // parameters alongside IFormFile cause SwaggerGeneratorException.
    public sealed class UploadImageForm
    {
        public IFormFile File { get; set; } = null!;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

    [HttpPost("{listingId:guid}/images/upload")]
    [Authorize(Roles = "SELLER")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ListingImageUploadResponse>> UploadImage(
        Guid listingId,
        [FromForm] UploadImageForm form,
        CancellationToken cancellationToken)
    {
        var file = form.File;

        if (file.Length <= 0)
        {
            return BadRequest(new { error = "File is required." });
        }

        if (file.Length > _fileUploadOptions.MaxBytes)
        {
            return BadRequest(new { error = $"File exceeds maximum allowed size of {_fileUploadOptions.MaxBytes} bytes." });
        }

        if (form.SortOrder < 0)
        {
            return BadRequest(new { error = "Sort order must be zero or greater." });
        }

        var allowedMimeTypes = _fileUploadOptions.AllowedMimeTypes
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var effectiveContentType = NormalizeContentType(file.ContentType, file.FileName);
        var detectedContentType = await DetectContentTypeFromSignatureAsync(file, cancellationToken);

        if (detectedContentType is null)
        {
            return BadRequest(new { error = "Unsupported file type." });
        }

        // Trust actual file bytes first. Some devices keep .heic file names while
        // transcoding bytes to JPEG during image picking/compression.
        var finalContentType = detectedContentType;

        if (!allowedMimeTypes.Contains(finalContentType))
        {
            return BadRequest(new { error = "Unsupported file type." });
        }

        if (!string.Equals(effectiveContentType, finalContentType, StringComparison.OrdinalIgnoreCase))
        {
            // Keep warning visible in logs while still accepting valid file bytes.
            Console.WriteLine($"[UPLOAD] Content type normalized to '{effectiveContentType}', detected from bytes as '{finalContentType}' for file '{file.FileName}'.");
        }

        await using var fileStream = file.OpenReadStream();
        var storedFile = await _fileStorageService.SaveListingImageAsync(fileStream, finalContentType, cancellationToken);

        var addImageRequest = new UploadListingImageRequest(listingId, storedFile.PublicUrl, form.IsPrimary, form.SortOrder);
        await _service.AddImageAsync(User.GetRequiredUserId(), addImageRequest, cancellationToken);

        return Ok(new ListingImageUploadResponse(listingId, storedFile.PublicUrl, storedFile.ContentType, storedFile.SizeBytes, form.IsPrimary, form.SortOrder));
    }

    private static async Task<string?> DetectContentTypeFromSignatureAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var maxSignatureLength = 12;
        var signature = new byte[maxSignatureLength];

        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(signature.AsMemory(0, maxSignatureLength), cancellationToken);
        if (read <= 0)
        {
            return null;
        }

        if (read >= 3 && signature[0] == 0xFF && signature[1] == 0xD8 && signature[2] == 0xFF)
        {
            return "image/jpeg";
        }

        if (read >= 8 && signature[0] == 0x89 && signature[1] == 0x50 && signature[2] == 0x4E && signature[3] == 0x47 && signature[4] == 0x0D && signature[5] == 0x0A && signature[6] == 0x1A && signature[7] == 0x0A)
        {
            return "image/png";
        }

        if (read >= 12 && signature[0] == 0x52 && signature[1] == 0x49 && signature[2] == 0x46 && signature[3] == 0x46 && signature[8] == 0x57 && signature[9] == 0x45 && signature[10] == 0x42 && signature[11] == 0x50)
        {
            return "image/webp";
        }

        if (IsHeicOrHeifSignature(signature, read))
        {
            return "image/heic";
        }

        return null;
    }

    [HttpDelete("{listingId:guid}/images/{imageId:guid}")]
    [Authorize(Roles = "SELLER")]
    public async Task<IActionResult> DeleteImage(Guid listingId, Guid imageId, CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteImageAsync(User.GetRequiredUserId(), listingId, imageId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("favorites")]
    [Authorize(Roles = "BUYER")]
    public async Task<ActionResult<IReadOnlyList<ListingSummaryResponse>>> GetFavorites(CancellationToken cancellationToken)
    {
        var listings = await _service.GetFavoritesAsync(User.GetRequiredUserId(), cancellationToken);
        return Ok(listings);
    }

    [HttpPost("{listingId:guid}/favorite")]
    [Authorize(Roles = "BUYER")]
    public async Task<IActionResult> AddFavorite(Guid listingId, CancellationToken cancellationToken)
    {
        await _service.AddFavoriteAsync(User.GetRequiredUserId(), listingId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{listingId:guid}/favorite")]
    [Authorize(Roles = "BUYER")]
    public async Task<IActionResult> RemoveFavorite(Guid listingId, CancellationToken cancellationToken)
    {
        await _service.RemoveFavoriteAsync(User.GetRequiredUserId(), listingId, cancellationToken);
        return NoContent();
    }

    private static bool IsHeicOrHeifSignature(byte[] signature, int read)
    {
        // ISO Base Media File format headers for HEIC/HEIF commonly include:
        // bytes [4..7] == 'ftyp' and bytes [8..11] one of heic/heix/hevc/hevx/mif1/msf1
        if (read < 12)
        {
            return false;
        }

        var hasFtyp = signature[4] == 0x66 && signature[5] == 0x74 && signature[6] == 0x79 && signature[7] == 0x70;
        if (!hasFtyp)
        {
            return false;
        }

        var brand = new string(new[]
        {
            (char)signature[8],
            (char)signature[9],
            (char)signature[10],
            (char)signature[11]
        }).ToLowerInvariant();

        return brand is "heic" or "heix" or "hevc" or "hevx" or "heim" or "heis" or "hevm" or "hevs" or "mif1" or "msf1";
    }

    private static string NormalizeContentType(string? contentType, string? fileName)
    {
        var normalized = (contentType ?? string.Empty).Trim().ToLowerInvariant();

        if (normalized == "image/jpg")
        {
            return "image/jpeg";
        }

        if (normalized == "image/heic-sequence")
        {
            return "image/heic";
        }

        if (normalized == "image/heif-sequence")
        {
            return "image/heif";
        }

        if (!string.IsNullOrWhiteSpace(normalized) && normalized != "application/octet-stream")
        {
            return normalized;
        }

        var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".heic" => "image/heic",
            ".heif" => "image/heif",
            _ => normalized
        };
    }
}
