using FarmMarketplace.Api.Extensions;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Buyer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmMarketplace.Api.Controllers;

[ApiController]
[Authorize(Roles = "BUYER")]
[Route("api/buyer/profile")]
public sealed class BuyerProfilesController : ControllerBase
{
    private readonly IBuyerProfileService _service;

    public BuyerProfilesController(IBuyerProfileService service) => _service = service;

    [HttpGet("me")]
    public async Task<ActionResult<BuyerProfileResponse>> GetMe(CancellationToken cancellationToken)
    {
        var result = await _service.GetAsync(User.GetRequiredUserId(), cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<ActionResult<BuyerProfileResponse>> UpsertMe([FromBody] UpsertBuyerProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpsertAsync(User.GetRequiredUserId(), request, cancellationToken);
        return Ok(result);
    }
}
