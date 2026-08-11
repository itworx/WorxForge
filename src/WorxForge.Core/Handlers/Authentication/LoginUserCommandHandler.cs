using WorxForge.Core.Abstractions;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Authentication;
using WorxForge.Domain.Dtos.Authentication;

namespace WorxForge.Core.Handlers.Authentication;

public sealed class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, AuthTokenDto>
{
    private readonly IRequestValidator<LoginUserCommand> _validator;
    private readonly IAuthenticationService _authenticationService;

    public LoginUserCommandHandler(
        IRequestValidator<LoginUserCommand> validator,
        IAuthenticationService authenticationService)
    {
        _validator = validator;
        _authenticationService = authenticationService;
    }

    public async Task<AuthTokenDto> HandleAsync(LoginUserCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(command, cancellationToken);
        return await _authenticationService.AuthenticateWithGoogleAsync(command.IdToken, cancellationToken);
    }
}
