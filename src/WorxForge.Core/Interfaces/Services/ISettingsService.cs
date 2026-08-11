namespace WorxForge.Core.Interfaces.Services;

public interface ISettingsService
{
    Task UpdateEmailAddressAsync(Guid userId, string newEmail, CancellationToken cancellationToken);
    Task VerifyUpdatedEmailAsync(Guid userId, string token, CancellationToken cancellationToken);
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, string newPasswordConfirm, CancellationToken cancellationToken);
    Task DeleteAccountAsync(Guid userId, CancellationToken cancellationToken);
}

