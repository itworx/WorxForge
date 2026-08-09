using WorxForge.Core.Abstractions;
using WorxForge.Domain.Exceptions;
using WorxForge.Domain.Queries.Admin;

namespace WorxForge.Core.Validators.Admin;

public sealed class GetAdminReportsQueryValidator : IRequestValidator<GetAdminReportsQuery>
{
    public Task ValidateAsync(GetAdminReportsQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Dictionary<string, string[]> errors = [];

        if (request.Page < 1)
        {
            errors[nameof(request.Page)] = ["Page must be greater than or equal to 1."];
        }

        if (request.PageSize is < 1 or > 100)
        {
            errors[nameof(request.PageSize)] = ["PageSize must be between 1 and 100."];
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }

        return Task.CompletedTask;
    }
}
