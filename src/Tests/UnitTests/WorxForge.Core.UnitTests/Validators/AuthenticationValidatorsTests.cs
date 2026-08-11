using FluentAssertions;
using WorxForge.Core.Validators.Authentication;
using WorxForge.Domain.Commands.Authentication;
using WorxForge.Domain.Exceptions;

namespace WorxForge.Core.UnitTests;

public sealed class AuthenticationValidatorsTests
{
    [Fact]
    public async Task LoginValidator_ShouldThrow_WhenIdTokenIsMissing()
    {
        var validator = new LoginUserCommandValidator();
        var command = new LoginUserCommand { IdToken = " " };

        var act = () => validator.ValidateAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<WorxForgeException>()
            .WithMessage("IdToken is required.");
    }

    [Fact]
    public async Task LogoutValidator_ShouldPass_WhenPayloadIsValid()
    {
        var validator = new LogoutUserCommandValidator();
        var command = new LogoutUserCommand
        {
            UserId = Guid.NewGuid(),
            SessionId = Guid.NewGuid()
        };

        var act = () => validator.ValidateAsync(command, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}

