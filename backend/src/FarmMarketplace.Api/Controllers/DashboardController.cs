using FarmMarketplace.Api.Extensions;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmMarketplace.Api.Controllers;

[ApiController]
[Authorize(Roles = "SELLER")]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    [HttpGet("seller-summary")]
    public async Task<ActionResult<SellerDashboardSummaryResponse>> SellerSummary(CancellationToken cancellationToken)
    {
        var summary = await _service.GetSellerSummaryAsync(User.GetRequiredUserId(), cancellationToken);
        return Ok(summary);
    }
}
