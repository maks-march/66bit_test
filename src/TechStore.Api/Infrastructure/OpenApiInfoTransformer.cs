using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TechStore.Api.Infrastructure;

/// <summary>Шапка OpenAPI-документа.</summary>
internal sealed class OpenApiInfoTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken ct)
    {
        document.Info.Title = "TechStore API";
        document.Info.Version = "v1";
        document.Info.Description = "Склад техники: справочники, позиции, фото, realtime-уведомления.";
        return Task.CompletedTask;
    }
}
