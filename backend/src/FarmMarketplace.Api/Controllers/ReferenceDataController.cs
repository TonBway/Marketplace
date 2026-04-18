using FarmMarketplace.Application.Interfaces;
using FarmMarketplace.Contracts.Reference;
using Microsoft.AspNetCore.Mvc;

namespace FarmMarketplace.Api.Controllers;

[ApiController]
[Route("api/reference")]
public sealed class ReferenceDataController : ControllerBase
{
    private readonly IReferenceDataService _service;

    public ReferenceDataController(IReferenceDataService service) => _service = service;

    [HttpGet("regions")]
    public async Task<ActionResult<IReadOnlyList<RegionResponse>>> Regions(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetRegionsAsync(cancellationToken));
    }

    [HttpGet("districts")]
    public async Task<ActionResult<IReadOnlyList<DistrictResponse>>> Districts([FromQuery] int? regionId, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetDistrictsAsync(regionId, cancellationToken));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<ReferenceItemResponse>>> Categories(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetCategoriesAsync(cancellationToken));
    }

    [HttpGet("product-types")]
    public async Task<ActionResult<IReadOnlyList<ProductTypeResponse>>> ProductTypes([FromQuery] int? categoryId, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetProductTypesAsync(categoryId, cancellationToken));
    }

    [HttpGet("units")]
    public async Task<ActionResult<IReadOnlyList<ReferenceItemResponse>>> Units(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetUnitsAsync(cancellationToken));
    }
}
