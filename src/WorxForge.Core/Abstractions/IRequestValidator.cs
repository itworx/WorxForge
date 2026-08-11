namespace WorxForge.Core.Abstractions;

public interface IRequestValidator<in TRequest>
{
    Task ValidateAsync(TRequest request, CancellationToken cancellationToken);
}
