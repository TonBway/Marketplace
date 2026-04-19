using FarmMarketplace.Api.Extensions;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmMarketplace.Api.Controllers;

[ApiController]
[Route("api/conversations")]
[Authorize]
public sealed class ConversationsController : ControllerBase
{
    private readonly IMessagingService _messagingService;

    public ConversationsController(IMessagingService messagingService) => _messagingService = messagingService;

    [HttpPost]
    public async Task<ActionResult<ConversationResponse>> Start([FromBody] StartConversationRequest request, CancellationToken cancellationToken)
    {
        var conversation = await _messagingService.StartConversationAsync(request.EnquiryId, cancellationToken);
        return Ok(conversation);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConversationResponse>>> GetMine(CancellationToken cancellationToken)
    {
        var conversations = await _messagingService.GetMyConversationsAsync(User.GetRequiredUserId(), cancellationToken);
        return Ok(conversations);
    }

    [HttpGet("{conversationId:guid}/messages")]
    public async Task<ActionResult<IReadOnlyList<MessageResponse>>> GetMessages(Guid conversationId, CancellationToken cancellationToken)
    {
        var messages = await _messagingService.GetMessagesAsync(conversationId, User.GetRequiredUserId(), cancellationToken);
        return Ok(messages);
    }

    [HttpPost("{conversationId:guid}/messages")]
    public async Task<ActionResult<MessageResponse>> SendMessage(Guid conversationId, [FromBody] SendMessageRequest request, CancellationToken cancellationToken)
    {
        var message = await _messagingService.SendMessageAsync(conversationId, User.GetRequiredUserId(), request, cancellationToken);
        return Ok(message);
    }

    [HttpPatch("{conversationId:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid conversationId, CancellationToken cancellationToken)
    {
        await _messagingService.MarkConversationReadAsync(conversationId, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }
}
