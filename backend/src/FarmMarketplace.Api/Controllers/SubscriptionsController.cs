using FarmMarketplace.Api.Extensions;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Billing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmMarketplace.Api.Controllers;

[ApiController]
[Authorize(Roles = "SELLER")]
[Route("api/subscriptions")]
public sealed class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _service;

    public SubscriptionsController(ISubscriptionService service) => _service = service;

    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<SubscriptionPlanResponse>>> Plans(CancellationToken cancellationToken)
    {
        var plans = await _service.GetPlansAsync(cancellationToken);
        return Ok(plans);
    }

    [HttpGet("active")]
    public async Task<ActionResult<ActiveSubscriptionResponse>> Active(CancellationToken cancellationToken)
    {
        var active = await _service.GetActiveForSellerAsync(User.GetRequiredUserId(), cancellationToken);
        if (active is null)
        {
            return NotFound();
        }

        return Ok(active);
    }

    [HttpPost]
    public async Task<ActionResult<ActiveSubscriptionResponse>> Subscribe([FromBody] SubscribeRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.SubscribeAsync(User.GetRequiredUserId(), request, cancellationToken);
        return Ok(result);
    }
}
