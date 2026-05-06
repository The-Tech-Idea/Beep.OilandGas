using System.Threading.Tasks;

namespace Beep.OilandGas.UserManagement.Contracts.Services
{
    /// <summary>
    /// Multi-Factor Authentication service for TOTP-based 2FA.
    /// Implements Oil & Gas industry best practices for secure authentication.
    /// </summary>
    public interface IMfaService
    {
        /// <summary>
        /// Enables MFA for a user and returns the secret key and QR code URI.
        /// </summary>
        Task<MfaSetupResult> EnableMfaAsync(string userId);

        /// <summary>
        /// Verifies the TOTP code during MFA setup and activates MFA for the user.
        /// </summary>
        Task<bool> VerifyMfaSetupAsync(string userId, string totpCode);

        /// <summary>
        /// Disables MFA for a user. Requires current password verification.
        /// </summary>
        Task<bool> DisableMfaAsync(string userId, string currentPassword);

        /// <summary>
        /// Verifies a TOTP code for login.
        /// </summary>
        Task<bool> VerifyTotpAsync(string userId, string totpCode);

        /// <summary>
        /// Generates backup codes for account recovery.
        /// </summary>
        Task<BackupCodesResult> GenerateBackupCodesAsync(string userId);

        /// <summary>
        /// Uses a backup code for login. Consumes the code (one-time use).
        /// </summary>
        Task<bool> UseBackupCodeAsync(string userId, string backupCode);

        /// <summary>
        /// Gets MFA status for a user.
        /// </summary>
        Task<MfaStatusResult> GetMfaStatusAsync(string userId);
    }

    /// <summary>
    /// Result of MFA setup initiation.
    /// </summary>
    public record MfaSetupResult
    {
        public bool Success { get; init; }
        public string? SecretKey { get; init; }
        public string? QrCodeUri { get; init; }
        public string? ErrorMessage { get; init; }
    }

    /// <summary>
    /// Result of backup code generation.
    /// </summary>
    public record BackupCodesResult
    {
        public bool Success { get; init; }
        public string[]? BackupCodes { get; init; }
        public string? ErrorMessage { get; init; }
    }

    /// <summary>
    /// MFA status for a user.
    /// </summary>
    public record MfaStatusResult
    {
        public bool IsEnabled { get; init; }
        public bool IsVerified { get; init; }
        public int RemainingBackupCodes { get; init; }
        public DateTime? LastUsedUtc { get; init; }
        public string? Method { get; init; }
    }

    /// <summary>
    /// MFA model for persistence.
    /// </summary>
    public class UserMfaConfig : Beep.OilandGas.Models.Data.ModelEntityBase
    {
        public string USER_MFA_ID { get; set; } = Guid.NewGuid().ToString();
        public string USER_ID { get; set; } = string.Empty;
        public string? MFA_SECRET_KEY { get; set; }
        public string MFA_METHOD { get; set; } = "TOTP";
        public string MFA_ENABLED_IND { get; set; } = "N";
        public string MFA_VERIFIED_IND { get; set; } = "N";
        public string? BACKUP_CODES_JSON { get; set; }
        public int REMAINING_BACKUP_CODES { get; set; }
        public DateTime? LAST_MFA_USED_UTC { get; set; }
        public DateTime? MFA_ENABLED_DATE_UTC { get; set; }
    }
}
