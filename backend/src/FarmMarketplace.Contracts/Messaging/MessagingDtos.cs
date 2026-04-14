namespace FarmMarketplace.Contracts.Messaging;

public sealed record CreateEnquiryRequest(Guid ListingId, string Message, string PreferredContactMode);

public sealed record UpdateEnquiryStatusRequest(string StatusCode, string? Note);

public sealed record EnquiryResponse(
    Guid EnquiryId,
    Guid ListingId,
    Guid BuyerUserId,
    Guid SellerUserId,
    string StatusCode,
    string Message,
    DateTime CreatedAtUtc);

public sealed record NotificationResponse(Guid NotificationId, Guid UserId, string Title, string Body, bool IsRead, DateTime CreatedAtUtc);
