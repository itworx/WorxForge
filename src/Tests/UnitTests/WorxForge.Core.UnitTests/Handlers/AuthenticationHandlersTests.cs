using FluentAssertions;
using Moq;
using WorxForge.Core.Abstractions;
using WorxForge.Core.Handlers.Authentication;
using WorxForge.Core.Interfaces.Services;
using WorxForge.Domain.Commands.Authentication;
using WorxForge.Domain.Dtos.Authentication;

namespace WorxForge.Core.UnitTests;

public sealed class AuthenticationHandlersTests
{
    [Fact]
    public async Task LoginHandler_ShouldValidate_ThenAuthenticateWithGoogle()
    {
        var validator = new Mock<IRequestValidator<LoginUserCommand>>();
        var authService = new Mock<IAuthenticationService>();
        var command = new LoginUserCommand { IdToken = "google-id-token" };
        var expected = new AuthTokenDto { AccessToken = "token", ExpiresAt = DateTime.UtcNow.AddHours(1) };

        authService
            .Setup(service => service.AuthenticateWithGoogleAsync(command.IdToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new LoginUserCommandHandler(validator.Object, authService.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        validator.Verify(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        authService.Verify(service => service.AuthenticateWithGoogleAsync(command.IdToken, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogoutHandler_ShouldValidate_ThenRevokeSession()
    {
        var validator = new Mock<IRequestValidator<LogoutUserCommand>>();
        var authService = new Mock<IAuthenticationService>();
        var command = new LogoutUserCommand
        {
            UserId = Guid.NewGuid(),
            SessionId = Guid.NewGuid()
        };

        var handler = new LogoutUserCommandHandler(validator.Object, authService.Object);

        await handler.HandleAsync(command, CancellationToken.None);

        validator.Verify(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        authService.Verify(service => service.RevokeSessionAsync(command.UserId, command.SessionId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

