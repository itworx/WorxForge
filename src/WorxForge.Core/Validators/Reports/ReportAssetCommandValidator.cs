using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Reports;
using WorxForge.Domain.Constants;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Reports;

public sealed class ReportAssetCommandValidator : IRequestValidator<ReportAssetCommand>
{
    public Task ValidateAsync(ReportAssetCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Dictionary<string, string[]> errors = [];

        if (request.UserId == Guid.Empty)
        {
            errors[nameof(request.UserId)] = ["User identifier is required."];
        }

        if (request.AssetId == Guid.Empty)
        {
            errors[nameof(request.AssetId)] = ["Asset identifier is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            errors[nameof(request.Reason)] = ["Reason is required."];
        }
        else if (!ReportReasonConstants.AllowedReasons.Contains(request.Reason))
        {
            errors[nameof(request.Reason)] = [$"Reason must be one of: {string.Join(", ", ReportReasonConstants.AllowedReasons)}."];
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }

        return Task.CompletedTask;
    }
}
