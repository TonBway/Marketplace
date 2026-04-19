namespace FarmMarketplace.Contracts.Messaging;

public sealed record CreateEnquiryRequest(Guid ListingId, string Message, string PreferredContactMode);

public sealed record UpdateEnquiryStatusRequest(string StatusCode, string? Note);

public sealed record EnquiryResponse(
    Guid EnquiryId,
    Guid ListingId,
    string ListingTitle,
    Guid BuyerUserId,
    string BuyerName,
    string? BuyerEmail,
    string? BuyerPhone,
    Guid SellerUserId,
    string SellerName,
    string StatusCode,
    string PreferredContactMode,
    string Message,
    DateTime CreatedAtUtc);

public sealed record NotificationResponse(Guid NotificationId, Guid UserId, string Title, string Body, bool IsRead, DateTime CreatedAtUtc);

// ── Conversations & Messages ───────────────────────────────────────────────────

public sealed record ConversationResponse(
    Guid ConversationId,
    Guid? EnquiryId,
    Guid BuyerUserId,
    string BuyerName,
    Guid SellerUserId,
    string SellerName,
    DateTime CreatedAtUtc,
    MessageResponse? LastMessage);

public sealed record MessageResponse(
    Guid MessageId,
    Guid ConversationId,
    Guid SenderUserId,
    string SenderName,
    string MessageBody,
    bool IsRead,
    DateTime SentAtUtc);

public sealed record SendMessageRequest(string MessageBody);

public sealed record StartConversationRequest(Guid EnquiryId);

// ── Device Tokens (Push Notifications) ────────────────────────────────────────

public sealed record RegisterDeviceTokenRequest(string Platform, string Token);
