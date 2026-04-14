using FarmMarketplace.Api.Extensions;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Seller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmMarketplace.Api.Controllers;

[ApiController]
[Authorize(Roles = "SELLER")]
[Route("api/seller/profile")]
public sealed class SellerProfilesController : ControllerBase
{
    private readonly ISellerProfileService _service;

    public SellerProfilesController(ISellerProfileService service) => _service = service;

    [HttpGet("me")]
    public async Task<ActionResult<SellerProfileResponse>> GetMe(CancellationToken cancellationToken)
    {
        var result = await _service.GetAsync(User.GetRequiredUserId(), cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<ActionResult<SellerProfileResponse>> UpsertMe([FromBody] UpsertSellerProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpsertAsync(User.GetRequiredUserId(), request, cancellationToken);
        return Ok(result);
    }
}
