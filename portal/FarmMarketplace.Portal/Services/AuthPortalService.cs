using System.Security.Claims;
using FarmMarketplace.Portal.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace FarmMarketplace.Portal.Services;

public sealed class AuthPortalService
{
    private readonly NavigationManager _navigationManager;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AuthPortalService(NavigationManager navigationManager, AuthenticationStateProvider authenticationStateProvider)
    {
        _navigationManager = navigationManager;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<string?> LoginAsync(PortalLoginRequest request, CancellationToken cancellationToken)
    {
        using var client = new HttpClient { BaseAddress = new Uri(_navigationManager.BaseUri) };
        var response = await client.PostAsJsonAsync("/portal-auth/login", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResult>(cancellationToken: cancellationToken);
        return result?.RoleCode;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        using var client = new HttpClient { BaseAddress = new Uri(_navigationManager.BaseUri) };
        await client.PostAsync("/portal-auth/logout", null, cancellationToken);
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
