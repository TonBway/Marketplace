using FarmMarketplace.Api.Extensions;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Listings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmMarketplace.Api.Controllers;

[ApiController]
[Route("api/listings")]
public sealed class ListingsController : ControllerBase
{
    private readonly IListingService _service;

    public ListingsController(IListingService service) => _service = service;

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

    [HttpPatch("{listingId:guid}/status")]
    [Authorize(Roles = "SELLER")]
    public async Task<IActionResult> UpdateStatus(Guid listingId, [FromBody] UpdateListingStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateStatusAsync(User.GetRequiredUserId(), listingId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("images")]
    [Authorize(Roles = "SELLER")]
    public async Task<IActionResult> AddImage([FromBody] UploadListingImageRequest request, CancellationToken cancellationToken)
    {
        await _service.AddImageAsync(User.GetRequiredUserId(), request, cancellationToken);
        return NoContent();
    }
}
