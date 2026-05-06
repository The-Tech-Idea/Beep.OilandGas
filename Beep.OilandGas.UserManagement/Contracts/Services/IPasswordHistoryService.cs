using System.Threading.Tasks;

namespace Beep.OilandGas.UserManagement.Contracts.Services
{
    /// <summary>
    /// Password history service for enforcing password rotation and preventing reuse.
    /// Implements Oil & Gas industry best practices for password security.
    /// </summary>
    public interface IPasswordHistoryService
    {
        /// <summary>
        /// Records a password change in the history.
        /// </summary>
        Task RecordPasswordChangeAsync(string userId, string passwordHash);

        /// <summary>
        /// Checks if a password has been used recently (within the history window).
        /// </summary>
        Task<bool> IsPasswordRecentlyUsedAsync(string userId, string passwordHash);

        /// <summary>
        /// Gets the number of password changes in the history.
        /// </summary>
        Task<int> GetPasswordHistoryCountAsync(string userId);

        /// <summary>
        /// Checks if a user is due for password rotation.
        /// </summary>
        Task<PasswordRotationResult> CheckPasswordRotationAsync(string userId);

        /// <summary>
        /// Clears old password history entries beyond the retention period.
        /// </summary>
        Task<int> CleanupOldHistoryAsync(int retentionDays = 365);
    }

    /// <summary>
    /// Password rotation check result.
    /// </summary>
    public record PasswordRotationResult
    {
        public bool RequiresRotation { get; init; }
        public int DaysSinceLastChange { get; init; }
        public int MaxPasswordAgeDays { get; init; }
        public DateTime? LastPasswordChangeUtc { get; init; }
    }

    /// <summary>
    /// Password history model for persistence.
    /// </summary>
    public class PasswordHistory : Beep.OilandGas.Models.Data.ModelEntityBase
    {
        public string PASSWORD_HISTORY_ID { get; set; } = Guid.NewGuid().ToString();
        public string USER_ID { get; set; } = string.Empty;
        public string PASSWORD_HASH { get; set; } = string.Empty;
        public DateTime CHANGED_DATE_UTC { get; set; } = DateTime.UtcNow;
        public string CHANGED_BY { get; set; } = string.Empty;
        public string? IP_ADDRESS { get; set; }
    }
}
