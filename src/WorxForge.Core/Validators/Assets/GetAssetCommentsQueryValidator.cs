using WorxForge.Core.Abstractions;
using WorxForge.Domain.Queries.Assets;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Assets;

public sealed class GetAssetCommentsQueryValidator : IRequestValidator<GetAssetCommentsQuery>
{
    private const int MaxPageSize = 100;

    public Task ValidateAsync(GetAssetCommentsQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.AssetId == Guid.Empty)
            throw new ValidationException("Asset identifier is required.");

        if (request.Page < 1)
            throw new ValidationException("Page must be greater than zero.");

        if (request.PageSize < 1)
            throw new ValidationException("Page size must be greater than zero.");

        if (request.PageSize > MaxPageSize)
            throw new ValidationException($"Page size must not exceed {MaxPageSize}.");

        return Task.CompletedTask;
    }
}
