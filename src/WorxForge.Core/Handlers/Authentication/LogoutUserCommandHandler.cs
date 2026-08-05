using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Authentication;

namespace WorxForge.Core.Handlers.Authentication;

public sealed class LogoutUserCommandHandler : ICommandHandler<LogoutUserCommand>
{
    private readonly IRequestValidator<LogoutUserCommand> _validator;
    private readonly IAuthenticationService _authenticationService;

    public LogoutUserCommandHandler(
        IRequestValidator<LogoutUserCommand> validator,
        IAuthenticationService authenticationService)
    {
        _validator = validator;
        _authenticationService = authenticationService;
    }

    public async Task HandleAsync(LogoutUserCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(command, cancellationToken);
        await _authenticationService.RevokeSessionAsync(command.UserId, command.SessionId, cancellationToken);
    }
}
