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
}
