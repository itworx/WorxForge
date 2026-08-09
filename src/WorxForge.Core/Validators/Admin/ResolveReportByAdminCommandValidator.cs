using WorxForge.Core.Abstractions;
using WorxForge.Domain.Commands.Admin;
using WorxForge.Domain.Enums;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.Validators.Admin;

public sealed class ResolveReportByAdminCommandValidator : IRequestValidator<ResolveReportByAdminCommand>
{
    public Task ValidateAsync(ResolveReportByAdminCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Dictionary<string, string[]> errors = [];

        if (request.ReportId == Guid.Empty)
        {
            errors[nameof(request.ReportId)] = ["Report identifier is required."];
        }

        if (request.AdminUserId == Guid.Empty)
        {
            errors[nameof(request.AdminUserId)] = ["Admin user identifier is required."];
        }

        // Open is the initial state; it is not a valid transition target via admin resolution.
        if (request.NewStatus == ReportStatus.Open)
        {
            errors[nameof(request.NewStatus)] = ["NewStatus must be InReview, Resolved, or Rejected."];
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }

        return Task.CompletedTask;
    }
}
