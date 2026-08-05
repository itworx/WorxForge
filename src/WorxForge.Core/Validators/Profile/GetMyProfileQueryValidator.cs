using WorxForge.Core.Abstractions;
using WorxForge.Domain.Exceptions;
using WorxForge.Domain.Queries.Profile;

namespace WorxForge.Core.Validators.Profile;

public sealed class GetMyProfileQueryValidator : IRequestValidator<GetMyProfileQuery>
{
    public Task ValidateAsync(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request is null)
        {
            throw new WorxForgeException("Profile query is required.");
        }

        if (request.UserId == Guid.Empty)
        {
            throw new WorxForgeException("User identifier is required.");
        }

        return Task.CompletedTask;
    }
}