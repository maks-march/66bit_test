using Microsoft.Extensions.FileProviders;
using Scalar.AspNetCore;
using TechStore.Api.Infrastructure;
using TechStore.Api.Middleware;
using TechStore.Application;
using TechStore.Infrastructure;
using TechStore.Infrastructure.Files;
using TechStore.Infrastructure.Notifications;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<OpenApiInfoTransformer>());
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Default")!);

// CORS: массив (Cors:AllowedOrigins:0..n) или одна строка через ';'.
var corsSection = builder.Configuration.GetSection("Cors:AllowedOrigins");
string[] corsOrigins = corsSection.GetChildren().Any()
    ? corsSection.Get<string[]>() ?? []
    : corsSection.Value?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

builder.Services.AddCors(options => options.AddPolicy("Web", policy =>
{
    if (corsOrigins.Length == 0 && builder.Environment.IsDevelopment())
        policy.SetIsOriginAllowed(_ => true);
    else
        policy.WithOrigins(corsOrigins);

    policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));

var app = builder.Build();

app.UseMiddleware<DomainExceptionMiddleware>();

var fileOptions = app.Configuration.GetSection(FileStorageOptions.SectionName).Get<FileStorageOptions>() ?? new();
var uploadsRoot = Path.IsPathRooted(fileOptions.RootPath)
    ? fileOptions.RootPath
    : Path.Combine(app.Environment.ContentRootPath, fileOptions.RootPath);
Directory.CreateDirectory(uploadsRoot);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = fileOptions.PublicPrefix,
    ServeUnknownFileTypes = false,
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "public,max-age=3600",
});

if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("Web");

app.MapControllers();
app.MapHub<InventoryHub>("/hubs/inventory");
app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("TechStore API"));
app.MapHealthChecks("/health");

await app.ApplyMigrationsAndSeedAsync();

app.Run();
