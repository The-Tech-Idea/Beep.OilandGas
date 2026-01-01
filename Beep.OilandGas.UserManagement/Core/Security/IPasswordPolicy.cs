namespace Beep.OilandGas.UserManagement.Core.Security
{
    /// <summary>
    /// Result of password validation
    /// </summary>
    public class PasswordValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Interface for password validation and policy enforcement
    /// </summary>
    public interface IPasswordPolicy
    {
        /// <summary>
        /// Validates a password against the policy
        /// </summary>
        /// <param name="password">The password to validate</param>
        /// <param name="username">Optional username (for checks like password cannot equal username)</param>
        /// <returns>Validation result</returns>
        PasswordValidationResult ValidatePassword(string password, string? username = null);

        /// <summary>
        /// Checks if a password has been used recently (for password history)
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="passwordHash">The password hash to check</param>
        /// <returns>True if password was recently used</returns>
        Task<bool> IsPasswordInHistoryAsync(string userId, string passwordHash);

        /// <summary>
        /// Adds a password to the user's password history
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="passwordHash">The password hash</param>
        Task AddToPasswordHistoryAsync(string userId, string passwordHash);

        /// <summary>
        /// Checks if a user's password has expired
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>True if password has expired</returns>
        Task<bool> IsPasswordExpiredAsync(string userId);

        /// <summary>
        /// Gets the minimum password length
        /// </summary>
        int MinimumLength { get; }

        /// <summary>
        /// Gets whether password requires uppercase letters
        /// </summary>
        bool RequireUppercase { get; }

        /// <summary>
        /// Gets whether password requires lowercase letters
        /// </summary>
        bool RequireLowercase { get; }

        /// <summary>
        /// Gets whether password requires digits
        /// </summary>
        bool RequireDigit { get; }

        /// <summary>
        /// Gets whether password requires special characters
        /// </summary>
        bool RequireSpecialCharacter { get; }

        /// <summary>
        /// Gets the maximum password age in days (0 = no expiration)
        /// </summary>
        int MaximumPasswordAgeDays { get; }

        /// <summary>
        /// Gets the number of previous passwords to remember (0 = no history)
        /// </summary>
        int PasswordHistoryCount { get; }
    }
}
