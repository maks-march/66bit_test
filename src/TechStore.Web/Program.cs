using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TechStore.Web;
using TechStore.Web.Services;

// Интерфейс русскоязычный: даты и числа (ввод цены через запятую) — по ru-RU.
var culture = new CultureInfo("ru-RU");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = builder.Configuration.GetValue<string>("ApiBaseUrl")
    ?? throw new InvalidOperationException("Не задан ApiBaseUrl в wwwroot/appsettings.json.");
apiBase = apiBase.TrimEnd('/') + "/";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBase, UriKind.Absolute) });
builder.Services.AddScoped<TechStoreApi>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<InventoryHubClient>();

await builder.Build().RunAsync();
