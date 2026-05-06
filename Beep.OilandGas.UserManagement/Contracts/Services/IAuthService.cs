using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Security;

namespace Beep.OilandGas.UserManagement.Contracts.Services
{
    /// <summary>
    /// Authentication service for login, token generation, and session management.
    /// Implements Oil & Gas industry best practices for secure authentication.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user and returns a JWT token with claims.
        /// Records login event in audit trail.
        /// </summary>
        Task<LoginResult> LoginAsync(LoginRequest request, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Validates a refresh token and issues a new access token.
        /// </summary>
        Task<LoginResult> RefreshTokenAsync(string refreshToken, string? ipAddress = null);

        /// <summary>
        /// Revokes a refresh token (logout).
        /// </summary>
        Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null);

        /// <summary>
        /// Revokes all refresh tokens for a user (logout from all devices).
        /// </summary>
        Task<bool> RevokeAllTokensAsync(string userId);

        /// <summary>
        /// Changes a user's password. Requires current password for verification.
        /// Enforces password history and rotation policies.
        /// </summary>
        Task<PasswordChangeResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

        /// <summary>
        /// Initiates password reset flow. Sends reset token to user's email.
        /// </summary>
        Task<bool> RequestPasswordResetAsync(string emailOrUsername);

        /// <summary>
        /// Completes password reset using a valid reset token.
        /// </summary>
        Task<bool> ResetPasswordAsync(string resetToken, string newPassword);

        /// <summary>
        /// Records a failed login attempt. Locks account after threshold.
        /// </summary>
        Task RecordFailedLoginAsync(string username, string? ipAddress = null);

        /// <summary>
        /// Unlocks a locked user account. Requires admin privileges.
        /// </summary>
        Task<bool> UnlockAccountAsync(string userId, string unlockedByUserId);
    }

    /// <summary>
    /// Login request with username and password.
    /// </summary>
    public record LoginRequest(string Username, string Password, bool RememberMe = false);

    /// <summary>
    /// Login result with access and refresh tokens.
    /// </summary>
    public record LoginResult
    {
        public bool Success { get; init; }
        public string? AccessToken { get; init; }
        public string? RefreshToken { get; init; }
        public DateTime AccessTokenExpiry { get; init; }
        public DateTime RefreshTokenExpiry { get; init; }
        public string? UserId { get; init; }
        public string? Username { get; init; }
        public string? ErrorMessage { get; init; }
        public string[]? Roles { get; init; }
        public string[]? Permissions { get; init; }
        public string? DefaultFieldId { get; init; }
        public string? PersonaCode { get; init; }
        public bool RequiresPasswordChange { get; init; }
        public bool IsLocked { get; init; }
    }

    /// <summary>
    /// Password change result.
    /// </summary>
    public record PasswordChangeResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }
        public bool RequiresPasswordChange { get; init; }
    }
}
