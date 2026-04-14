namespace FarmMarketplace.Portal.Models;

public sealed record PortalLoginRequest(string EmailOrPhone, string Password);

public sealed record AuthResponsePayload(
    Guid UserId,
    string FullName,
    string Email,
    string RoleCode,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc);

public sealed record PortalUserInfo(Guid UserId, string FullName, string Email, string RoleCode);
