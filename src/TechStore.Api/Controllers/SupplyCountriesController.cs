using Microsoft.AspNetCore.Mvc;
using TechStore.Api.Infrastructure;
using TechStore.Application.SupplyCountries;

namespace TechStore.Api.Controllers;

[ApiController]
[Route("api/supply-countries")]
public sealed class SupplyCountriesController : ControllerBase
{
    private readonly ISupplyCountryService _service;

    public SupplyCountriesController(ISupplyCountryService service)
    {
        _service = service;
    }

    /// <summary>Автокомплит для комбобокса страны. Создание стран — через upsert в позициях.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SupplyCountryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Autocomplete(
        [FromQuery] string? search, [FromQuery] int take = 10, CancellationToken ct = default)
    {
        var result = await _service.AutocompleteAsync(search, take, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error.ToProblem(this);
    }
}
