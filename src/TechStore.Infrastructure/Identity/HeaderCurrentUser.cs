using Microsoft.AspNetCore.Http;
using TechStore.Application.Abstractions;
using TechStore.Domain.Entities;

namespace TechStore.Infrastructure.Identity;

/// <summary>Пользователь из заголовка X-User-Name. Задел под Identity/JWT: заменится реализацией.</summary>
public sealed class HeaderCurrentUser : ICurrentUser
{
    public const string HeaderName = "X-User-Name";
    private const string DefaultName = "anonymous";

    private readonly IHttpContextAccessor _accessor;

    public HeaderCurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string Name
    {
        get
        {
            var value = _accessor.HttpContext?.Request.Headers[HeaderName].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(value))
                return DefaultName;

            var trimmed = value.Trim();
            return trimmed.Length <= AuditableEntity.UpdatedByMaxLength
                ? trimmed
                : trimmed[..AuditableEntity.UpdatedByMaxLength];
        }
    }
}
