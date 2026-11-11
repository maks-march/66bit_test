using Microsoft.EntityFrameworkCore;
using TechStore.Application.Abstractions;
using TechStore.Domain.Entities;
using TechStore.Infrastructure.Persistence;

namespace TechStore.Infrastructure.Seed;

/// <summary>Демо-данные из коробки. Идемпотентно: повторный запуск ничего не дублирует.</summary>
public sealed class DbSeeder
{
    private const string SeededBy = "seed";

    /// <summary>Фото по типу техники (embedded-ресурсы, по копии файла на каждую позицию).</summary>
    private static readonly Dictionary<string, string> PhotoByType = new()
    {
        ["Телефон"] = "TechStore.Infrastructure.Seed.Photos.phone.jpg",
        ["Телевизор"] = "TechStore.Infrastructure.Seed.Photos.tv.jpg",
        ["Ноутбук"] = "TechStore.Infrastructure.Seed.Photos.laptop.jpg",
        ["Холодильник"] = "TechStore.Infrastructure.Seed.Photos.fridge.jpg",
        ["Наушники"] = "TechStore.Infrastructure.Seed.Photos.headphones.jpg",
    };

    private static readonly (string Name, string Description)[] Manufacturers =
    [
        ("Samsung", "Южнокорейский производитель электроники."),
        ("Apple", "Американский производитель premium-техники."),
        ("LG", "Южнокорейский производитель электроники и бытовой техники."),
        ("Bosch", "Немецкий производитель бытовой техники."),
        ("Sony", "Японский производитель аудио/видео и электроники."),
        ("Xiaomi", "Китайский производитель смартфонов и гаджетов."),
    ];

    private static readonly (string Name, string Description)[] Types =
    [
        ("Телефон", "Смартфоны и мобильные телефоны."),
        ("Телевизор", "Телевизоры и панели."),
        ("Ноутбук", "Портативные компьютеры."),
        ("Холодильник", "Холодильное оборудование."),
        ("Наушники", "Аудиогарнитуры."),
    ];

    private static readonly string[] Countries = ["Китай", "Южная Корея", "Вьетнам", "Германия", "Россия"];

    private static readonly (string Type, string Maker, string Model, string Country, decimal Price)[] Items =
    [
        ("Телефон", "Samsung", "Galaxy S24", "Вьетнам", 79_990m),
        ("Телефон", "Samsung", "Galaxy A55", "Вьетнам", 34_990m),
        ("Телефон", "Apple", "iPhone 16", "Китай", 99_990m),
        ("Телефон", "Apple", "iPhone 15", "Китай", 84_990m),
        ("Телефон", "Xiaomi", "Redmi Note 14", "Китай", 24_990m),
        ("Телефон", "Xiaomi", "14T", "Китай", 59_990m),
        ("Телевизор", "Samsung", "QE55Q80C", "Россия", 74_990m),
        ("Телевизор", "Samsung", "QE77S95D", "Россия", 249_990m),
        ("Телевизор", "LG", "OLED evo C4", "Россия", 149_990m),
        ("Телевизор", "LG", "UR78006", "Россия", 49_990m),
        ("Телевизор", "Sony", "Bravia 7", "Китай", 129_990m),
        ("Ноутбук", "Apple", "MacBook Air M3", "Китай", 149_990m),
        ("Ноутбук", "Samsung", "Galaxy Book4", "Китай", 109_990m),
        ("Ноутбук", "Xiaomi", "RedmiBook 16", "Китай", 64_990m),
        ("Холодильник", "Bosch", "Serie 4 KGV39", "Германия", 89_990m),
        ("Холодильник", "Samsung", "RB38", "Россия", 64_990m),
        ("Холодильник", "LG", "DoorCooling", "Россия", 74_990m),
        ("Наушники", "Sony", "WH-1000XM5", "Китай", 34_990m),
        ("Наушники", "Apple", "AirPods Pro 2", "Вьетнам", 24_990m),
        ("Наушники", "Xiaomi", "Redmi Buds 6", "Китай", 3_490m),
    ];

    private readonly AppDbContext _db;
    private readonly IFileStorage _files;

    public DbSeeder(AppDbContext db, IFileStorage files)
    {
        _db = db;
        _files = files;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.EquipmentItems.AnyAsync(ct))
            return;

        var makers = new Dictionary<string, Manufacturer>();
        foreach (var (name, description) in Manufacturers)
            makers[name] = await EnsureManufacturerAsync(name, description, ct);

        var types = new Dictionary<string, EquipmentType>();
        foreach (var (name, description) in Types)
            types[name] = await EnsureTypeAsync(name, description, ct);

        var countries = new Dictionary<string, SupplyCountry>();
        foreach (var name in Countries)
            countries[name] = await EnsureCountryAsync(name, ct);

        await _db.SaveChangesAsync(ct);

        foreach (var (type, maker, model, country, price) in Items)
        {
            var photoPath = await SaveSeedPhotoAsync(type, ct);
            await _db.EquipmentItems.AddAsync(EquipmentItem.Create(
                types[type].Id, makers[maker].Id, countries[country].Id,
                model, price, photoPath, SeededBy), ct);
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task<string?> SaveSeedPhotoAsync(string typeName, CancellationToken ct)
    {
        if (!PhotoByType.TryGetValue(typeName, out var resource))
            return null;

        // Копия на каждую позицию: удаление одной не уронит фото соседей.
        await using var stream = typeof(DbSeeder).Assembly.GetManifestResourceStream(resource);
        if (stream is null)
            return null; // ресурса нет в сборке — сидируем без фото, но не падаем

        return await _files.SaveAsync(stream, "seed.jpg", ct);
    }

    private async Task<Manufacturer> EnsureManufacturerAsync(string name, string description, CancellationToken ct)
    {
        var existing = await _db.Manufacturers.FirstOrDefaultAsync(x => x.Name == name, ct);
        if (existing is not null)
            return existing;

        var created = Manufacturer.Create(name, description, SeededBy);
        await _db.Manufacturers.AddAsync(created, ct);
        return created;
    }

    private async Task<EquipmentType> EnsureTypeAsync(string name, string description, CancellationToken ct)
    {
        var existing = await _db.EquipmentTypes.FirstOrDefaultAsync(x => x.Name == name, ct);
        if (existing is not null)
            return existing;

        var created = EquipmentType.Create(name, description, SeededBy);
        await _db.EquipmentTypes.AddAsync(created, ct);
        return created;
    }

    private async Task<SupplyCountry> EnsureCountryAsync(string name, CancellationToken ct)
    {
        var existing = await _db.SupplyCountries.FirstOrDefaultAsync(x => x.Name == name, ct);
        if (existing is not null)
            return existing;

        var created = SupplyCountry.Create(name, SeededBy);
        await _db.SupplyCountries.AddAsync(created, ct);
        return created;
    }
}
