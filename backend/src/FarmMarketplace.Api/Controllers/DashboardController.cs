using FarmMarketplace.Api.Extensions;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmMarketplace.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    [HttpGet("seller-summary")]
    [Authorize(Roles = "SELLER")]
    public async Task<ActionResult<SellerDashboardSummaryResponse>> SellerSummary(CancellationToken cancellationToken)
    {
        var summary = await _service.GetSellerSummaryAsync(User.GetRequiredUserId(), cancellationToken);
        return Ok(summary);
    }

    [HttpGet("buyer-summary")]
    [Authorize(Roles = "BUYER")]
    public async Task<ActionResult<BuyerDashboardSummaryResponse>> BuyerSummary(CancellationToken cancellationToken)
    {
        var summary = await _service.GetBuyerSummaryAsync(User.GetRequiredUserId(), cancellationToken);
        return Ok(summary);
    }
}
