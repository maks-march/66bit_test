using TechStore.Domain.Exceptions;

namespace TechStore.Api.Middleware;

/// <summary>
/// Отлов исключений. Всё остальное → 500 без деталей.
/// </summary>
public sealed class DomainExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DomainExceptionMiddleware> _logger;

    public DomainExceptionMiddleware(RequestDelegate next, ILogger<DomainExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            var status = ex switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ConflictException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest,
            };

            var extensions = new Dictionary<string, object?> { ["code"] = ex.Code };
            if (ex is ConflictException conflict && conflict.Details is not null)
            {
                foreach (var (key, value) in conflict.Details)
                    extensions[key] = value;
            }

            await Results.Problem(detail: ex.Message, statusCode: status, extensions: extensions)
                .ExecuteAsync(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необработанная ошибка");

            await Results.Problem(
                    detail: "Внутренняя ошибка сервера.",
                    statusCode: StatusCodes.Status500InternalServerError)
                .ExecuteAsync(context);
        }
    }
}
