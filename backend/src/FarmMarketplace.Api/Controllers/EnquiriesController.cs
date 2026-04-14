using FarmMarketplace.Api.Extensions;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmMarketplace.Api.Controllers;

[ApiController]
[Route("api/enquiries")]
public sealed class EnquiriesController : ControllerBase
{
    private readonly IEnquiryService _service;

    public EnquiriesController(IEnquiryService service) => _service = service;

    [HttpPost]
    [Authorize(Roles = "BUYER")]
    public async Task<ActionResult<EnquiryResponse>> Create([FromBody] CreateEnquiryRequest request, CancellationToken cancellationToken)
    {
        var enquiry = await _service.CreateAsync(User.GetRequiredUserId(), request, cancellationToken);
        return Ok(enquiry);
    }

    [HttpGet("received")]
    [Authorize(Roles = "SELLER")]
    public async Task<ActionResult<IReadOnlyList<EnquiryResponse>>> Received(CancellationToken cancellationToken)
    {
        var enquiries = await _service.GetForSellerAsync(User.GetRequiredUserId(), cancellationToken);
        return Ok(enquiries);
    }

    [HttpGet("sent")]
    [Authorize(Roles = "BUYER")]
    public async Task<ActionResult<IReadOnlyList<EnquiryResponse>>> Sent(CancellationToken cancellationToken)
    {
        var enquiries = await _service.GetForBuyerAsync(User.GetRequiredUserId(), cancellationToken);
        return Ok(enquiries);
    }

    [HttpPatch("{enquiryId:guid}/status")]
    [Authorize(Roles = "SELLER")]
    public async Task<IActionResult> UpdateStatus(Guid enquiryId, [FromBody] UpdateEnquiryStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateStatusAsync(User.GetRequiredUserId(), enquiryId, request, cancellationToken);
        return NoContent();
    }
}
