using WorxForge.Core.Abstractions;
using WorxForge.Domain.Exceptions;
using WorxForge.Domain.Queries.Profile;

namespace WorxForge.Core.Validators.Profile;

public sealed class GetMyProfileStatisticsQueryValidator : IRequestValidator<GetMyProfileStatisticsQuery>
{
    public Task ValidateAsync(GetMyProfileStatisticsQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request is null)
        {
            throw new WorxForgeException("Profile statistics query is required.");
        }

        if (request.UserId == Guid.Empty)
        {
            throw new WorxForgeException("User identifier is required.");
        }

        return Task.CompletedTask;
    }
}
