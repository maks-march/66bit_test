using Microsoft.AspNetCore.Mvc;
using TechStore.Api.Infrastructure;
using TechStore.Application.Common;
using TechStore.Application.EquipmentTypes;

namespace TechStore.Api.Controllers;

[ApiController]
[Route("api/equipment-types")]
public sealed class EquipmentTypesController : ControllerBase
{
    private readonly IEquipmentTypeService _service;

    public EquipmentTypesController(IEquipmentTypeService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EquipmentTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List([FromQuery] PagedQuery query, CancellationToken ct)
    {
        var result = await _service.ListAsync(query, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error.ToProblem(this);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(EquipmentTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        var result = await _service.GetAsync(id, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error.ToProblem(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EquipmentTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] EquipmentTypeCreateDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : result.Error.ToProblem(this);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(EquipmentTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(long id, [FromBody] EquipmentTypeUpdateDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error.ToProblem(this);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.IsSuccess ? NoContent() : result.Error.ToProblem(this);
    }
}
