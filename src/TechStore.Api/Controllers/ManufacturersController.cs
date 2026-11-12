using Microsoft.AspNetCore.Mvc;
using TechStore.Api.Infrastructure;
using TechStore.Application.Common;
using TechStore.Application.Manufacturers;

namespace TechStore.Api.Controllers;

[ApiController]
[Route("api/manufacturers")]
public sealed class ManufacturersController : ControllerBase
{
    private readonly IManufacturerService _service;

    public ManufacturersController(IManufacturerService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ManufacturerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List([FromQuery] PagedQuery query, CancellationToken ct)
    {
        var result = await _service.ListAsync(query, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error.ToProblem(this);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ManufacturerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        var result = await _service.GetAsync(id, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error.ToProblem(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ManufacturerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] ManufacturerCreateDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : result.Error.ToProblem(this);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ManufacturerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(long id, [FromBody] ManufacturerUpdateDto dto, CancellationToken ct)
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
