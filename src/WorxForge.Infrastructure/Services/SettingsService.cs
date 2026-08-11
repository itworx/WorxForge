using WorxForge.Core.Interfaces.Services;

namespace WorxForge.Infrastructure.Services;

public sealed class SettingsService : ISettingsService
{
    public Task UpdateEmailAddressAsync(Guid userId, string newEmail, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task VerifyUpdatedEmailAsync(Guid userId, string token, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, string newPasswordConfirm, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task DeleteAccountAsync(Guid userId, CancellationToken cancellationToken) => throw new NotImplementedException();
}
