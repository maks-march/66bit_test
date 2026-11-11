namespace TechStore.Application.Common;

/// <summary>
/// Контракт слоя: сервисы НЕ бросают исключений для ожидаемых случаев
/// (404/409/валидация), а возвращают Failure. DomainException из сущностей
/// транслируется в Failure на границе сервиса. Неожиданное (БД недоступна) —
/// пробрасывается и становится 500.
/// </summary>
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    public bool IsSuccess { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Нет значения: операция завершилась ошибкой.");

    public Error Error => !IsSuccess
        ? _error!
        : throw new InvalidOperationException("Нет ошибки: операция успешна.");

    private Result(bool isSuccess, T? value, Error? error)
    {
        IsSuccess = isSuccess;
        _value = value;
        _error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(Error error) => new(false, default, error);
}

/// <summary>Результат без значения.</summary>
public sealed class Result
{
    private readonly Error? _error;

    public bool IsSuccess { get; }

    public Error Error => !IsSuccess
        ? _error!
        : throw new InvalidOperationException("Нет ошибки: операция успешна.");

    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        _error = error;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(Error error) => new(false, error);
}
