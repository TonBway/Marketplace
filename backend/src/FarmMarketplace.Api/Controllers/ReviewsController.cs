using FarmMarketplace.Api.Extensions;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmMarketplace.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService) => _reviewService = reviewService;

    [HttpPost]
    [Authorize(Roles = "BUYER")]
    public async Task<ActionResult<ReviewResponse>> Create([FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var review = await _reviewService.CreateAsync(User.GetRequiredUserId(), request, cancellationToken);
        return Ok(review);
    }

    [HttpGet("listing/{listingId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ReviewResponse>>> GetForListing(Guid listingId, CancellationToken cancellationToken)
    {
        var reviews = await _reviewService.GetForListingAsync(listingId, cancellationToken);
        return Ok(reviews);
    }

    [HttpGet("seller/{sellerUserId:guid}/rating")]
    [AllowAnonymous]
    public async Task<ActionResult<SellerRatingSummaryResponse>> GetSellerRating(Guid sellerUserId, CancellationToken cancellationToken)
    {
        var summary = await _reviewService.GetSellerSummaryAsync(sellerUserId, cancellationToken);
        return Ok(summary);
    }
}
