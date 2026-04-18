using System.Security.Claims;
using FarmMarketplace.Portal.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FarmMarketplace.Portal.Services;

public sealed class AuthPortalService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IJSRuntime _jsRuntime;

    public AuthPortalService(AuthenticationStateProvider authenticationStateProvider, IJSRuntime jsRuntime)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> LoginAsync(PortalLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _jsRuntime.InvokeAsync<LoginResult?>("fmAuth.login", cancellationToken, request);
        if (result is null)
        {
            return null;
        }

        return result?.RoleCode;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        await _jsRuntime.InvokeVoidAsync("fmAuth.logout", cancellationToken);
    }

    public async Task<PortalUserInfo?> GetCurrentUserAsync()
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = state.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userIdRaw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        _ = Guid.TryParse(userIdRaw, out var userId);

        return new PortalUserInfo(
            userId,
            user.FindFirstValue(ClaimTypes.Name) ?? "Unknown",
            user.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            user.FindFirstValue(ClaimTypes.Role) ?? string.Empty);
    }

    private sealed record LoginResult(string RoleCode);
}
