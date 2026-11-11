using TechStore.Domain.Exceptions;
using TechStore.Domain.Interfaces;

namespace TechStore.Application.Common;

internal static class UnitOfWorkExtensions
{
    /// <summary>
    /// Сохраняет изменения, транслируя ConflictException из Infrastructure в Result.Failure.
    /// </summary>
    public static async Task<Result<int>> TrySaveAsync(this IUnitOfWork unitOfWork, CancellationToken ct = default)
    {
        try
        {
            return Result<int>.Success(await unitOfWork.SaveChangesAsync(ct));
        }
        catch (ConflictException ex)
        {
            return Result<int>.Failure(Error.FromDomainException(ex));
        }
    }
}
