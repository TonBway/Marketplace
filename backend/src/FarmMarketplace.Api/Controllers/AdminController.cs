using FarmMarketplace.Api.Extensions;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmMarketplace.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN,SUPER_ADMIN")]
[Route("api/admin")]
public sealed class AdminController : ControllerBase
{
    private readonly IAdminPortalService _service;

    public AdminController(IAdminPortalService service)
    {
        _service = service;
    }

    [HttpGet("dashboard/summary")]
    public async Task<ActionResult<AdminDashboardSummaryResponse>> DashboardSummary(CancellationToken cancellationToken)
        => Ok(await _service.GetDashboardSummaryAsync(cancellationToken));

    [HttpGet("dashboard/recent-activity")]
    public async Task<ActionResult<IReadOnlyList<RecentActivityItemResponse>>> RecentActivity(CancellationToken cancellationToken)
        => Ok(await _service.GetRecentActivityAsync(cancellationToken));

    [HttpGet("sellers")]
    public async Task<ActionResult<IReadOnlyList<AdminSellerRowResponse>>> Sellers([FromQuery] string? search, [FromQuery] string? statusCode, CancellationToken cancellationToken)
        => Ok(await _service.GetSellersAsync(search, statusCode, cancellationToken));

    [HttpGet("sellers/{id:guid}")]
    public async Task<ActionResult<AdminSellerDetailResponse>> Seller(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetSellerAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("sellers/{id:guid}/approve")]
    public async Task<IActionResult> ApproveSeller(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateSellerStatusAsync(id, "APPROVE", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("sellers/{id:guid}/reject")]
    public async Task<IActionResult> RejectSeller(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateSellerStatusAsync(id, "REJECT", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("sellers/{id:guid}/suspend")]
    public async Task<IActionResult> SuspendSeller(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateSellerStatusAsync(id, "SUSPEND", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("sellers/{id:guid}/reactivate")]
    public async Task<IActionResult> ReactivateSeller(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateSellerStatusAsync(id, "REACTIVATE", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPost("sellers/{id:guid}/notes")]
    public async Task<IActionResult> AddSellerNote(Guid id, [FromBody] AddAdminNoteRequest request, CancellationToken cancellationToken)
    {
        await _service.AddSellerNoteAsync(id, request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpGet("buyers")]
    public async Task<ActionResult<IReadOnlyList<AdminBuyerRowResponse>>> Buyers([FromQuery] string? search, [FromQuery] string? statusCode, CancellationToken cancellationToken)
        => Ok(await _service.GetBuyersAsync(search, statusCode, cancellationToken));

    [HttpGet("buyers/{id:guid}")]
    public async Task<ActionResult<AdminBuyerDetailResponse>> Buyer(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetBuyerAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("buyers/{id:guid}/suspend")]
    public async Task<IActionResult> SuspendBuyer(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateSellerStatusAsync(id, "SUSPEND", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("buyers/{id:guid}/reactivate")]
    public async Task<IActionResult> ReactivateBuyer(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateSellerStatusAsync(id, "REACTIVATE", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPost("buyers/{id:guid}/notes")]
    public async Task<IActionResult> AddBuyerNote(Guid id, [FromBody] AddAdminNoteRequest request, CancellationToken cancellationToken)
    {
        await _service.AddBuyerNoteAsync(id, request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpGet("listings")]
    public async Task<ActionResult<IReadOnlyList<AdminListingRowResponse>>> Listings([FromQuery] string? search, [FromQuery] string? statusCode, CancellationToken cancellationToken)
        => Ok(await _service.GetListingsAsync(search, statusCode, cancellationToken));

    [HttpGet("listings/{id:guid}")]
    public async Task<ActionResult<AdminListingDetailResponse>> Listing(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetListingAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("listings/{id:guid}/approve")]
    public async Task<IActionResult> ApproveListing(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateListingStatusAsync(id, "APPROVE", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("listings/{id:guid}/reject")]
    public async Task<IActionResult> RejectListing(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateListingStatusAsync(id, "REJECT", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("listings/{id:guid}/suspend")]
    public async Task<IActionResult> SuspendListing(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateListingStatusAsync(id, "SUSPEND", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("listings/{id:guid}/reactivate")]
    public async Task<IActionResult> ReactivateListing(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateListingStatusAsync(id, "REACTIVATE", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("listings/{id:guid}/feature")]
    public async Task<IActionResult> FeatureListing(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateListingStatusAsync(id, "FEATURE", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("listings/{id:guid}/unfeature")]
    public async Task<IActionResult> UnfeatureListing(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateListingStatusAsync(id, "UNFEATURE", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPost("listings/{id:guid}/notes")]
    public async Task<IActionResult> AddListingNote(Guid id, [FromBody] AddAdminNoteRequest request, CancellationToken cancellationToken)
    {
        await _service.AddListingNoteAsync(id, request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpGet("subscriptions")]
    public async Task<ActionResult<IReadOnlyList<AdminSubscriptionRowResponse>>> Subscriptions([FromQuery] string? statusCode, CancellationToken cancellationToken)
        => Ok(await _service.GetSubscriptionsAsync(statusCode, cancellationToken));

    [HttpGet("subscriptions/{id:guid}")]
    public async Task<ActionResult<AdminSubscriptionDetailResponse>> Subscription(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetSubscriptionAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("payments")]
    public async Task<ActionResult<IReadOnlyList<AdminPaymentRowResponse>>> Payments(CancellationToken cancellationToken)
        => Ok(await _service.GetPaymentsAsync(cancellationToken));

    [HttpGet("invoices")]
    public async Task<ActionResult<IReadOnlyList<AdminInvoiceRowResponse>>> Invoices(CancellationToken cancellationToken)
        => Ok(await _service.GetInvoicesAsync(cancellationToken));

    [HttpGet("plans")]
    public async Task<ActionResult<IReadOnlyList<AdminPlanRowResponse>>> Plans(CancellationToken cancellationToken)
        => Ok(await _service.GetPlansAsync(cancellationToken));

    [HttpPost("plans")]
    [Authorize(Roles = "SUPER_ADMIN")]
    public async Task<ActionResult<int>> CreatePlan([FromBody] UpsertPlanRequest request, CancellationToken cancellationToken)
        => Ok(await _service.CreatePlanAsync(request, User.GetRequiredUserId(), cancellationToken));

    [HttpPut("plans/{id:int}")]
    [Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> UpdatePlan(int id, [FromBody] UpsertPlanRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdatePlanAsync(id, request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("plans/{id:int}/deactivate")]
    [Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> DeactivatePlan(int id, CancellationToken cancellationToken)
    {
        await _service.DeactivatePlanAsync(id, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("subscriptions/{id:guid}/activate")]
    public async Task<IActionResult> ActivateSubscription(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateSubscriptionStatusAsync(id, "ACTIVATE", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("subscriptions/{id:guid}/extend")]
    public async Task<IActionResult> ExtendSubscription(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateSubscriptionStatusAsync(id, "EXTEND", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("subscriptions/{id:guid}/cancel")]
    public async Task<IActionResult> CancelSubscription(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateSubscriptionStatusAsync(id, "CANCEL", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("subscriptions/{id:guid}/suspend")]
    public async Task<IActionResult> SuspendSubscription(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateSubscriptionStatusAsync(id, "SUSPEND", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpGet("enquiries")]
    public async Task<ActionResult<IReadOnlyList<AdminEnquiryRowResponse>>> Enquiries([FromQuery] string? statusCode, CancellationToken cancellationToken)
        => Ok(await _service.GetEnquiriesAsync(statusCode, cancellationToken));

    [HttpGet("enquiries/{id:guid}")]
    public async Task<ActionResult<AdminEnquiryDetailResponse>> Enquiry(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetEnquiryAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("enquiries/{id:guid}/close")]
    public async Task<IActionResult> CloseEnquiry(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateEnquiryStatusAsync(id, "CLOSE", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("enquiries/{id:guid}/review")]
    public async Task<IActionResult> ReviewEnquiry(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateEnquiryStatusAsync(id, "REVIEW", request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpGet("settings")]
    [Authorize(Roles = "SUPER_ADMIN")]
    public async Task<ActionResult<IReadOnlyList<AdminSystemSettingResponse>>> Settings(CancellationToken cancellationToken)
        => Ok(await _service.GetSettingsAsync(cancellationToken));

    [HttpPut("settings/{key}")]
    [Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSystemSettingRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateSettingAsync(key, request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<IReadOnlyList<AdminAuditLogResponse>>> AuditLogs(CancellationToken cancellationToken)
        => Ok(await _service.GetAuditLogsAsync(cancellationToken));

    [HttpGet("reference/{type}")]
    public async Task<ActionResult<IReadOnlyList<AdminReferenceItemResponse>>> ReferenceList(string type, CancellationToken cancellationToken)
        => Ok(await _service.GetReferenceItemsAsync(type, cancellationToken));

    [HttpPost("reference/{type}")]
    public async Task<ActionResult<int>> CreateReferenceItem(string type, [FromBody] UpsertReferenceItemRequest request, CancellationToken cancellationToken)
        => Ok(await _service.CreateReferenceItemAsync(type, request, User.GetRequiredUserId(), cancellationToken));

    [HttpPut("reference/{type}/{id:int}")]
    public async Task<IActionResult> UpdateReferenceItem(string type, int id, [FromBody] UpsertReferenceItemRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateReferenceItemAsync(type, id, request, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpPatch("reference/{type}/{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateReferenceItem(string type, int id, CancellationToken cancellationToken)
    {
        await _service.DeactivateReferenceItemAsync(type, id, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }

    [HttpGet("moderation-actions")]
    public async Task<ActionResult<IReadOnlyList<AdminModerationActionResponse>>> ModerationActions(CancellationToken cancellationToken)
        => Ok(await _service.GetModerationActionsAsync(cancellationToken));

    [HttpGet("admin-notes")]
    public async Task<ActionResult<IReadOnlyList<AdminNoteResponse>>> AdminNotes(CancellationToken cancellationToken)
        => Ok(await _service.GetAdminNotesAsync(cancellationToken));
}
