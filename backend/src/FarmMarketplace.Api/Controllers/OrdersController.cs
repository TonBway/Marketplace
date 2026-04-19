using FarmMarketplace.Api.Extensions;
using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmMarketplace.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService) => _orderService = orderService;

    [HttpPost]
    [Authorize(Roles = "SELLER")]
    public async Task<ActionResult<OrderResponse>> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _orderService.CreateFromEnquiryAsync(User.GetRequiredUserId(), request, cancellationToken);
        return Ok(order);
    }

    [HttpGet("selling")]
    [Authorize(Roles = "SELLER")]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetSelling([FromQuery] string? status, CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetForSellerAsync(User.GetRequiredUserId(), status, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("buying")]
    [Authorize(Roles = "BUYER")]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> GetBuying([FromQuery] string? status, CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetForBuyerAsync(User.GetRequiredUserId(), status, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(orderId, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPatch("{orderId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        await _orderService.UpdateStatusAsync(User.GetRequiredUserId(), orderId, request, cancellationToken);
        return NoContent();
    }
}
