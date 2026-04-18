using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FarmMarketplace.Portal.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace FarmMarketplace.Portal.Services;

public sealed class PortalApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public PortalApiClient(IHttpClientFactory httpClientFactory, AuthenticationStateProvider authenticationStateProvider)
    {
        _httpClientFactory = httpClientFactory;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<AdminDashboardSummaryVm?> GetAdminDashboardSummaryAsync(CancellationToken cancellationToken)
        => await SendAsync<AdminDashboardSummaryVm>(HttpMethod.Get, "/api/admin/dashboard/summary", null, cancellationToken);

    public async Task<IReadOnlyList<RecentActivityVm>> GetRecentActivityAsync(CancellationToken cancellationToken)
        => await SendListAsync<RecentActivityVm>(HttpMethod.Get, "/api/admin/dashboard/recent-activity", cancellationToken);

    public async Task<IReadOnlyList<AdminSellerVm>> GetAdminSellersAsync(string? search, string? statusCode, CancellationToken cancellationToken)
    {
        var route = BuildRoute("/api/admin/sellers", ("search", search), ("statusCode", statusCode));
        return await SendListAsync<AdminSellerVm>(HttpMethod.Get, route, cancellationToken);
    }

    public async Task<AdminSellerDetailVm?> GetAdminSellerAsync(Guid id, CancellationToken cancellationToken)
        => await SendAsync<AdminSellerDetailVm>(HttpMethod.Get, $"/api/admin/sellers/{id}", null, cancellationToken);

    public async Task<IReadOnlyList<AdminBuyerVm>> GetAdminBuyersAsync(string? search, string? statusCode, CancellationToken cancellationToken)
    {
        var route = BuildRoute("/api/admin/buyers", ("search", search), ("statusCode", statusCode));
        return await SendListAsync<AdminBuyerVm>(HttpMethod.Get, route, cancellationToken);
    }

    public async Task<AdminBuyerDetailVm?> GetAdminBuyerAsync(Guid id, CancellationToken cancellationToken)
        => await SendAsync<AdminBuyerDetailVm>(HttpMethod.Get, $"/api/admin/buyers/{id}", null, cancellationToken);

    public async Task<IReadOnlyList<AdminListingVm>> GetAdminListingsAsync(string? search, string? statusCode, CancellationToken cancellationToken)
    {
        var route = BuildRoute("/api/admin/listings", ("search", search), ("statusCode", statusCode));
        return await SendListAsync<AdminListingVm>(HttpMethod.Get, route, cancellationToken);
    }

    public async Task<AdminListingDetailVm?> GetAdminListingAsync(Guid id, CancellationToken cancellationToken)
        => await SendAsync<AdminListingDetailVm>(HttpMethod.Get, $"/api/admin/listings/{id}", null, cancellationToken);

    public async Task<IReadOnlyList<AdminSubscriptionVm>> GetAdminSubscriptionsAsync(string? statusCode, CancellationToken cancellationToken)
    {
        var route = BuildRoute("/api/admin/subscriptions", ("statusCode", statusCode));
        return await SendListAsync<AdminSubscriptionVm>(HttpMethod.Get, route, cancellationToken);
    }

    public async Task<AdminSubscriptionDetailVm?> GetAdminSubscriptionAsync(Guid id, CancellationToken cancellationToken)
        => await SendAsync<AdminSubscriptionDetailVm>(HttpMethod.Get, $"/api/admin/subscriptions/{id}", null, cancellationToken);

    public async Task<IReadOnlyList<AdminPaymentVm>> GetAdminPaymentsAsync(CancellationToken cancellationToken)
        => await SendListAsync<AdminPaymentVm>(HttpMethod.Get, "/api/admin/payments", cancellationToken);

    public async Task<IReadOnlyList<AdminInvoiceVm>> GetAdminInvoicesAsync(CancellationToken cancellationToken)
        => await SendListAsync<AdminInvoiceVm>(HttpMethod.Get, "/api/admin/invoices", cancellationToken);

    public async Task<IReadOnlyList<AdminPlanVm>> GetAdminPlansAsync(CancellationToken cancellationToken)
        => await SendListAsync<AdminPlanVm>(HttpMethod.Get, "/api/admin/plans", cancellationToken);

    public async Task<IReadOnlyList<AdminEnquiryVm>> GetAdminEnquiriesAsync(string? statusCode, CancellationToken cancellationToken)
    {
        var route = BuildRoute("/api/admin/enquiries", ("statusCode", statusCode));
        return await SendListAsync<AdminEnquiryVm>(HttpMethod.Get, route, cancellationToken);
    }

    public async Task<AdminEnquiryDetailVm?> GetAdminEnquiryAsync(Guid id, CancellationToken cancellationToken)
        => await SendAsync<AdminEnquiryDetailVm>(HttpMethod.Get, $"/api/admin/enquiries/{id}", null, cancellationToken);

    public async Task<IReadOnlyList<AdminSettingVm>> GetAdminSettingsAsync(CancellationToken cancellationToken)
        => await SendListAsync<AdminSettingVm>(HttpMethod.Get, "/api/admin/settings", cancellationToken);

    public async Task<IReadOnlyList<AdminReferenceItemVm>> GetReferenceAsync(string type, CancellationToken cancellationToken)
        => await SendListAsync<AdminReferenceItemVm>(HttpMethod.Get, $"/api/admin/reference/{type}", cancellationToken);

    public async Task<IReadOnlyList<AdminAuditLogVm>> GetAuditLogsAsync(CancellationToken cancellationToken)
        => await SendListAsync<AdminAuditLogVm>(HttpMethod.Get, "/api/admin/audit-logs", cancellationToken);

    public async Task<IReadOnlyList<AdminModerationActionVm>> GetModerationActionsAsync(CancellationToken cancellationToken)
        => await SendListAsync<AdminModerationActionVm>(HttpMethod.Get, "/api/admin/moderation-actions", cancellationToken);

    public async Task<IReadOnlyList<AdminNoteVm>> GetAdminNotesAsync(CancellationToken cancellationToken)
        => await SendListAsync<AdminNoteVm>(HttpMethod.Get, "/api/admin/admin-notes", cancellationToken);

    public async Task<SellerDashboardSummaryVm?> GetSellerDashboardSummaryAsync(CancellationToken cancellationToken)
        => await SendAsync<SellerDashboardSummaryVm>(HttpMethod.Get, "/api/dashboard/seller-summary", null, cancellationToken);

    public async Task<SellerProfileVm?> GetSellerProfileAsync(CancellationToken cancellationToken)
        => await SendAsync<SellerProfileVm>(HttpMethod.Get, "/api/seller/profile/me", null, cancellationToken);

    public async Task UpdateSellerProfileAsync(UpdateSellerProfileVm request, CancellationToken cancellationToken)
        => await SendAsync<object>(HttpMethod.Put, "/api/seller/profile/me", request, cancellationToken);

    public async Task<IReadOnlyList<ListingSummaryVm>> GetMyListingsAsync(string? statusCode, CancellationToken cancellationToken)
    {
        var route = BuildRoute("/api/listings/my", ("statusCode", statusCode));
        return await SendListAsync<ListingSummaryVm>(HttpMethod.Get, route, cancellationToken);
    }

    public async Task<ListingDetailVm?> GetMyListingAsync(Guid listingId, CancellationToken cancellationToken)
        => await SendAsync<ListingDetailVm>(HttpMethod.Get, $"/api/listings/my/{listingId}", null, cancellationToken);

    public async Task<Guid?> CreateListingAsync(CreateListingVm request, CancellationToken cancellationToken)
        => await SendAsync<Guid>(HttpMethod.Post, "/api/listings", request, cancellationToken);

    public async Task UpdateListingAsync(Guid listingId, UpdateListingVm request, CancellationToken cancellationToken)
        => await SendAsync<object>(HttpMethod.Put, $"/api/listings/{listingId}", request, cancellationToken);

    public async Task<IReadOnlyList<EnquiryVm>> GetReceivedEnquiriesAsync(CancellationToken cancellationToken)
        => await SendListAsync<EnquiryVm>(HttpMethod.Get, "/api/enquiries/received", cancellationToken);

    public async Task<SubscriptionVm?> GetActiveSubscriptionAsync(CancellationToken cancellationToken)
        => await SendAsync<SubscriptionVm>(HttpMethod.Get, "/api/subscriptions/active", null, cancellationToken);

    public async Task<bool> UploadListingImageAsync(Guid listingId, Stream fileStream, string fileName, string contentType, bool isPrimary, int sortOrder, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("MarketplaceApi");
        var token = await GetAccessTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        content.Add(streamContent, "file", fileName);
        content.Add(new StringContent(isPrimary ? "true" : "false", Encoding.UTF8), "isPrimary");
        content.Add(new StringContent(sortOrder.ToString(), Encoding.UTF8), "sortOrder");

        using var response = await client.PostAsync($"/api/listings/{listingId}/images/upload", content, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task PatchAsync(string route, object request, CancellationToken cancellationToken)
        => await SendAsync<object>(HttpMethod.Patch, route, request, cancellationToken);

    public async Task PostAsync(string route, object request, CancellationToken cancellationToken)
        => await SendAsync<object>(HttpMethod.Post, route, request, cancellationToken);

    public async Task PutAsync(string route, object request, CancellationToken cancellationToken)
        => await SendAsync<object>(HttpMethod.Put, route, request, cancellationToken);

    private async Task<T?> SendAsync<T>(HttpMethod method, string route, object? body, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("MarketplaceApi");
        var token = await GetAccessTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var request = new HttpRequestMessage(method, route);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        if (typeof(T) == typeof(object) || response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    private async Task<IReadOnlyList<T>> SendListAsync<T>(HttpMethod method, string route, CancellationToken cancellationToken)
    {
        var result = await SendAsync<List<T>>(method, route, null, cancellationToken);
        return result ?? [];
    }

    private async Task<string?> GetAccessTokenAsync()
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return state.User.FindFirstValue("access_token");
    }

    private static string BuildRoute(string path, params (string Key, string? Value)[] query)
    {
        var parts = query
            .Where(q => !string.IsNullOrWhiteSpace(q.Value))
            .Select(q => $"{q.Key}={Uri.EscapeDataString(q.Value!)}")
            .ToArray();

        return parts.Length == 0 ? path : $"{path}?{string.Join("&", parts)}";
    }
}
