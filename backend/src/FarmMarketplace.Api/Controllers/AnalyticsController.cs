using FarmMarketplace.Api.Extensions;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmMarketplace.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public sealed class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService) => _analyticsService = analyticsService;

    [HttpGet("seller")]
    [Authorize(Roles = "SELLER")]
    public async Task<ActionResult<SellerAnalyticsSummaryResponse>> GetSellerSummary(CancellationToken cancellationToken)
    {
        var summary = await _analyticsService.GetSellerSummaryAsync(User.GetRequiredUserId(), cancellationToken);
        return Ok(summary);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<AdminAnalyticsSummaryResponse>> GetAdminSummary(CancellationToken cancellationToken)
    {
        var summary = await _analyticsService.GetAdminSummaryAsync(cancellationToken);
        return Ok(summary);
    }
}
