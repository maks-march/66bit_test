using Microsoft.AspNetCore.Mvc;
using TechStore.Application.Common;
using TechStore.Domain.Common;

namespace TechStore.Api.Infrastructure;

/// <summary>Перевод Application-Error в ProblemDetails. Код ошибки определяет статус.</summary>
internal static class ResultMapping
{
    public static IActionResult ToProblem(this Error error, ControllerBase controller)
    {
        var traceId = controller.HttpContext.TraceIdentifier;

        if (error.FieldErrors is not null)
        {
            var validation = new HttpValidationProblemDetails(new Dictionary<string, string[]>(error.FieldErrors))
            {
                Detail = error.Message,
                Status = StatusCodes.Status400BadRequest,
                Title = "Ошибка валидации",
            };
            validation.Extensions["traceId"] = traceId;
            return new BadRequestObjectResult(validation);
        }

        var status = error.Code switch
        {
            ErrorCodes.NotFound => StatusCodes.Status404NotFound,
            ErrorCodes.NameDuplicate or ErrorCodes.ReferenceInUse => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };

        var problem = new ProblemDetails
        {
            Detail = error.Message,
            Status = status,
            Title = status switch
            {
                StatusCodes.Status404NotFound => "Не найдено",
                StatusCodes.Status409Conflict => "Конфликт",
                _ => "Некорректный запрос",
            },
        };
        problem.Extensions["code"] = error.Code;
        problem.Extensions["traceId"] = traceId;

        if (error.Details is not null)
        {
            foreach (var (key, value) in error.Details)
                problem.Extensions[key] = value;
        }

        return new ObjectResult(problem) { StatusCode = status };
    }
}
