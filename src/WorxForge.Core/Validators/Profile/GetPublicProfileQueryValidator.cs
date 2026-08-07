using WorxForge.Core.Abstractions;
using WorxForge.Domain.Exceptions;
using WorxForge.Domain.Queries.Profile;

namespace WorxForge.Core.Validators.Profile;

public sealed class GetPublicProfileQueryValidator : IRequestValidator<GetPublicProfileQuery>
{
    public Task ValidateAsync(GetPublicProfileQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request is null)
        {
            throw new WorxForgeException("Public profile query is required.");
        }

        if (request.TargetUserId == Guid.Empty)
        {
            throw new WorxForgeException("Target user identifier is required.");
        }

        return Task.CompletedTask;
    }
}
