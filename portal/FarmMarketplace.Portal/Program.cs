using System.Net.Http.Headers;
using System.Security.Claims;
using FarmMarketplace.Portal.Components;
using FarmMarketplace.Portal.Models;
using FarmMarketplace.Portal.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddCascadingAuthenticationState();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpClient("MarketplaceApi", client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddScoped<PortalApiClient>();
builder.Services.AddScoped<AuthPortalService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapPost("/portal-auth/login", async (
    [FromBody] PortalLoginRequest request,
    IHttpClientFactory httpClientFactory,
    HttpContext context,
    CancellationToken cancellationToken) =>
{
    var client = httpClientFactory.CreateClient("MarketplaceApi");
    var apiResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
        EmailOrPhone = request.EmailOrPhone,
        request.Password
    }, cancellationToken);

    if (!apiResponse.IsSuccessStatusCode)
    {
        return Results.Unauthorized();
    }

    var payload = await apiResponse.Content.ReadFromJsonAsync<AuthResponsePayload>(cancellationToken: cancellationToken);
    if (payload is null)
    {
        return Results.Unauthorized();
    }

    var roleCode = payload.RoleCode.ToUpperInvariant();
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, payload.UserId.ToString()),
        new(ClaimTypes.Name, payload.FullName),
        new(ClaimTypes.Email, payload.Email),
        new(ClaimTypes.Role, roleCode),
        new("access_token", payload.AccessToken),
        new("refresh_token", payload.RefreshToken)
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

    return Results.Ok(new { roleCode });
});

app.MapPost("/portal-auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.NoContent();
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
