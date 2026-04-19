namespace FarmMarketplace.Contracts.Auth;

public sealed record RegisterRequest(string FullName, string Email, string Phone, string Password, string RoleCode);

public sealed record LoginRequest(string EmailOrPhone, string Password);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record ForgotPasswordRequest(string EmailOrPhone, string NewPassword);

public sealed record AuthUserProfileResponse(Guid UserId, string FullName, string Email, string Phone, string RoleCode);

public sealed record AuthResponse(Guid UserId, string FullName, string Email, string RoleCode, string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);
