using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TechStore.Api.Infrastructure;
using TechStore.Api.Models;
using TechStore.Application.Common;
using TechStore.Application.EquipmentItems;
using TechStore.Infrastructure.Files;

namespace TechStore.Api.Controllers;

[ApiController]
[Route("api/equipment-items")]
public sealed class EquipmentItemsController : ControllerBase
{
    private const long MaxUploadBytes = 6 * 1024 * 1024; // чуть выше прикладного лимита 5 МБ

    private readonly IEquipmentItemService _service;
    private readonly string _publicPrefix;

    public EquipmentItemsController(IEquipmentItemService service, IOptions<FileStorageOptions> fileOptions)
    {
        _service = service;
        _publicPrefix = fileOptions.Value.PublicPrefix.TrimEnd('/');
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EquipmentItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search([FromQuery] EquipmentItemQuery query, CancellationToken ct)
    {
        var result = await _service.SearchAsync(query, ct);
        if (!result.IsSuccess)
            return result.Error.ToProblem(this);

        var page = result.Value;
        return Ok(new PagedResult<EquipmentItemResponse>(
            page.Items.Select(d => EquipmentItemResponse.From(d, _publicPrefix)).ToList(),
            page.TotalCount,
            page.Page,
            page.PageSize));
    }

    [HttpGet("filter-options")]
    [ProducesResponseType(typeof(FilterOptionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FilterOptions([FromQuery] EquipmentItemQuery query, CancellationToken ct)
    {
        var result = await _service.GetFilterOptionsAsync(query, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error.ToProblem(this);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(EquipmentItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        var result = await _service.GetAsync(id, ct);
        return result.IsSuccess
            ? Ok(EquipmentItemResponse.From(result.Value, _publicPrefix))
            : result.Error.ToProblem(this);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EquipmentItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] EquipmentItemCreateDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id },
                EquipmentItemResponse.From(result.Value, _publicPrefix))
            : result.Error.ToProblem(this);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(EquipmentItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(long id, [FromBody] EquipmentItemUpdateDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.IsSuccess
            ? Ok(EquipmentItemResponse.From(result.Value, _publicPrefix))
            : result.Error.ToProblem(this);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.IsSuccess ? NoContent() : result.Error.ToProblem(this);
    }

    [HttpPost("{id:long}/photo")]
    [RequestSizeLimit(MaxUploadBytes)]
    [ProducesResponseType(typeof(EquipmentItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPhoto(long id, [FromForm] IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return Error.Business(
                    Domain.Common.ErrorCodes.PhotoInvalid, "Приложите файл фотографии.")
                .ToProblem(this);

        await using var stream = file.OpenReadStream();
        var result = await _service.SetPhotoAsync(id, stream, file.FileName, ct);
        return result.IsSuccess
            ? Ok(EquipmentItemResponse.From(result.Value, _publicPrefix))
            : result.Error.ToProblem(this);
    }

    [HttpDelete("{id:long}/photo")]
    [ProducesResponseType(typeof(EquipmentItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClearPhoto(long id, CancellationToken ct)
    {
        var result = await _service.ClearPhotoAsync(id, ct);
        return result.IsSuccess
            ? Ok(EquipmentItemResponse.From(result.Value, _publicPrefix))
            : result.Error.ToProblem(this);
    }
}
