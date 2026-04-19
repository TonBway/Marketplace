using FarmMarketplace.Api.Extensions;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmMarketplace.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service) => _service = service;

    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> My(CancellationToken cancellationToken)
    {
        var notifications = await _service.GetMyNotificationsAsync(User.GetRequiredUserId(), cancellationToken);
        return Ok(notifications);
    }

    [HttpPatch("my/read-all")]
    public async Task<IActionResult> ReadAll(CancellationToken cancellationToken)
    {
        await _service.MarkAllAsReadAsync(User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPost("device-token")]
    public async Task<IActionResult> RegisterDeviceToken([FromBody] RegisterDeviceTokenRequest request, CancellationToken cancellationToken)
    {
        await _service.RegisterDeviceTokenAsync(User.GetRequiredUserId(), request, cancellationToken);
        return NoContent();
    }
}
