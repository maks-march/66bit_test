using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TechStore.Web.Models;

namespace TechStore.Web.Services;

/// <summary>
/// Типизированный клиент API. HttpClient уже настроен с BaseAddress = ApiBaseUrl.
/// Числа в query — строго InvariantCulture (UI-культура — ru-RU, там запятая).
/// </summary>
public sealed class TechStoreApi
{
    private readonly HttpClient _http;

    public TechStoreApi(HttpClient http)
    {
        _http = http;
    }

    // ---------- Позиции ----------

    public Task<PagedResult<EquipmentItemModel>> SearchItemsAsync(
        string? search, long? typeId, long? manufacturerId,
        decimal? priceFrom, decimal? priceTo,
        string sortBy, bool sortDesc, int page, int pageSize, CancellationToken ct)
        => GetAsync<PagedResult<EquipmentItemModel>>(
            "api/equipment-items" + BuildItemQuery(search, typeId, manufacturerId, priceFrom, priceTo, sortBy, sortDesc, page, pageSize), ct);

    public Task<FilterOptions> GetFilterOptionsAsync(
        string? search, long? typeId, long? manufacturerId,
        decimal? priceFrom, decimal? priceTo, CancellationToken ct)
        => GetAsync<FilterOptions>(
            "api/equipment-items/filter-options" + BuildItemQuery(search, typeId, manufacturerId, priceFrom, priceTo, "updatedAt", true, 1, 1), ct);

    public Task<EquipmentItemModel> CreateItemAsync(ItemWriteRequest request, CancellationToken ct)
        => PostAsync<ItemWriteRequest, EquipmentItemModel>("api/equipment-items", request, ct);

    public Task<EquipmentItemModel> UpdateItemAsync(long id, ItemWriteRequest request, CancellationToken ct)
        => PutAsync<ItemWriteRequest, EquipmentItemModel>($"api/equipment-items/{id}", request, ct);

    public async Task DeleteItemAsync(long id, CancellationToken ct)
    {
        using var response = await _http.DeleteAsync($"api/equipment-items/{id}", ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task<EquipmentItemModel> SetPhotoAsync(
        long id, Stream content, string fileName, string contentType, CancellationToken ct)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new StreamContent(content);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "file", fileName);
        using var response = await _http.PostAsync($"api/equipment-items/{id}/photo", form, ct);
        return await ReadOrThrowAsync<EquipmentItemModel>(response, ct);
    }

    public async Task<EquipmentItemModel> ClearPhotoAsync(long id, CancellationToken ct)
    {
        using var response = await _http.DeleteAsync($"api/equipment-items/{id}/photo", ct);
        return await ReadOrThrowAsync<EquipmentItemModel>(response, ct);
    }

    // ---------- Справочники ----------

    public Task<PagedResult<ManufacturerModel>> ListManufacturersAsync(int page, int pageSize, CancellationToken ct)
        => GetAsync<PagedResult<ManufacturerModel>>($"api/manufacturers?page={page}&pageSize={pageSize}", ct);

    public Task<ManufacturerModel> CreateManufacturerAsync(DictionaryWriteRequest request, CancellationToken ct)
        => PostAsync<DictionaryWriteRequest, ManufacturerModel>("api/manufacturers", request, ct);

    public Task<ManufacturerModel> UpdateManufacturerAsync(long id, DictionaryWriteRequest request, CancellationToken ct)
        => PutAsync<DictionaryWriteRequest, ManufacturerModel>($"api/manufacturers/{id}", request, ct);

    public async Task DeleteManufacturerAsync(long id, CancellationToken ct)
    {
        using var response = await _http.DeleteAsync($"api/manufacturers/{id}", ct);
        await EnsureSuccessAsync(response, ct);
    }

    public Task<PagedResult<EquipmentTypeModel>> ListEquipmentTypesAsync(int page, int pageSize, CancellationToken ct)
        => GetAsync<PagedResult<EquipmentTypeModel>>($"api/equipment-types?page={page}&pageSize={pageSize}", ct);

    public Task<EquipmentTypeModel> CreateEquipmentTypeAsync(DictionaryWriteRequest request, CancellationToken ct)
        => PostAsync<DictionaryWriteRequest, EquipmentTypeModel>("api/equipment-types", request, ct);

    public Task<EquipmentTypeModel> UpdateEquipmentTypeAsync(long id, DictionaryWriteRequest request, CancellationToken ct)
        => PutAsync<DictionaryWriteRequest, EquipmentTypeModel>($"api/equipment-types/{id}", request, ct);

    public async Task DeleteEquipmentTypeAsync(long id, CancellationToken ct)
    {
        using var response = await _http.DeleteAsync($"api/equipment-types/{id}", ct);
        await EnsureSuccessAsync(response, ct);
    }

    // ---------- Страны (автокомплит) ----------

    public async Task<IReadOnlyList<SupplyCountryModel>> AutocompleteCountriesAsync(string? search, CancellationToken ct)
    {
        var url = "api/supply-countries?take=10"
            + (string.IsNullOrWhiteSpace(search) ? "" : "&search=" + Uri.EscapeDataString(search.Trim()));
        return await GetAsync<IReadOnlyList<SupplyCountryModel>>(url, ct);
    }

    // ---------- Фото ----------

    /// <summary>Относительный PhotoUrl (/uploads/...) → абсолютный URL через BaseAddress API.</summary>
    public string? ResolvePhotoUrl(string? photoUrl)
        => string.IsNullOrEmpty(photoUrl) || _http.BaseAddress is null
            ? null
            : new Uri(_http.BaseAddress, photoUrl.TrimStart('/')).ToString();

    // ---------- Внутреннее ----------

    private static string BuildItemQuery(
        string? search, long? typeId, long? manufacturerId,
        decimal? priceFrom, decimal? priceTo,
        string sortBy, bool sortDesc, int page, int pageSize)
    {
        var sb = new StringBuilder("?");
        void Add(string name, string? value)
        {
            if (string.IsNullOrEmpty(value))
                return;
            if (sb.Length > 1)
                sb.Append('&');
            sb.Append(name).Append('=').Append(Uri.EscapeDataString(value));
        }

        Add("search", string.IsNullOrWhiteSpace(search) ? null : search.Trim());
        Add("equipmentTypeId", typeId?.ToString(CultureInfo.InvariantCulture));
        Add("manufacturerId", manufacturerId?.ToString(CultureInfo.InvariantCulture));
        Add("priceFrom", priceFrom?.ToString(CultureInfo.InvariantCulture));
        Add("priceTo", priceTo?.ToString(CultureInfo.InvariantCulture));
        Add("sortBy", sortBy);
        Add("sortDesc", sortDesc ? "true" : "false");
        Add("page", page.ToString(CultureInfo.InvariantCulture));
        Add("pageSize", pageSize.ToString(CultureInfo.InvariantCulture));
        return sb.ToString();
    }

    private async Task<T> GetAsync<T>(string url, CancellationToken ct)
    {
        using var response = await _http.GetAsync(url, ct);
        return await ReadOrThrowAsync<T>(response, ct);
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct)
    {
        using var response = await _http.PostAsJsonAsync(url, body, ct);
        return await ReadOrThrowAsync<TResponse>(response, ct);
    }

    private async Task<TResponse> PutAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct)
    {
        using var response = await _http.PutAsJsonAsync(url, body, ct);
        return await ReadOrThrowAsync<TResponse>(response, ct);
    }

    private static async Task<T> ReadOrThrowAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct))!;
        throw await ParseErrorAsync(response, ct);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (!response.IsSuccessStatusCode)
            throw await ParseErrorAsync(response, ct);
    }

    private static async Task<ApiException> ParseErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var status = (int)response.StatusCode;
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var root = doc.RootElement;

            string? Text(string name)
                => root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;

            var message = Text("detail") ?? Text("title") ?? $"Запрос завершился с кодом {status}.";
            var code = Text("code");

            Dictionary<string, string[]>? fields = null;
            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                fields = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
                foreach (var prop in errors.EnumerateObject())
                    fields[prop.Name] = prop.Value.ValueKind == JsonValueKind.Array
                        ? prop.Value.EnumerateArray().Select(e => e.GetString() ?? "").ToArray()
                        : [];
            }

            return new ApiException(status, message, code, fields);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return new ApiException(status, $"Запрос завершился с кодом {status}.");
        }
    }
}
