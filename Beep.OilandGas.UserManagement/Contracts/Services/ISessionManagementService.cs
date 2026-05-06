using System.Threading.Tasks;

namespace Beep.OilandGas.UserManagement.Contracts.Services
{
    /// <summary>
    /// Session management service for tracking and controlling user sessions.
    /// Implements Oil & Gas industry best practices for session security.
    /// </summary>
    public interface ISessionManagementService
    {
        /// <summary>
        /// Creates a new session for a user.
        /// </summary>
        Task<SessionResult> CreateSessionAsync(string userId, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Validates a session token and returns session info.
        /// </summary>
        Task<SessionInfo?> ValidateSessionAsync(string sessionToken);

        /// <summary>
        /// Ends a specific session.
        /// </summary>
        Task<bool> EndSessionAsync(string sessionToken);

        /// <summary>
        /// Ends all sessions for a user (logout from all devices).
        /// </summary>
        Task<int> EndAllSessionsAsync(string userId);

        /// <summary>
        /// Gets all active sessions for a user.
        /// </summary>
        Task<List<SessionInfo>> GetActiveSessionsAsync(string userId);

        /// <summary>
        /// Checks if a user has reached the maximum concurrent session limit.
        /// </summary>
        Task<bool> HasReachedMaxSessionsAsync(string userId);

        /// <summary>
        /// Cleans up expired sessions.
        /// </summary>
        Task<int> CleanupExpiredSessionsAsync();
    }

    /// <summary>
    /// Session creation result.
    /// </summary>
    public record SessionResult
    {
        public bool Success { get; init; }
        public string? SessionToken { get; init; }
        public DateTime ExpiryUtc { get; init; }
        public string? ErrorMessage { get; init; }
    }

    /// <summary>
    /// Session information.
    /// </summary>
    public record SessionInfo
    {
        public string SessionToken { get; init; } = string.Empty;
        public string UserId { get; init; } = string.Empty;
        public string? IpAddress { get; init; }
        public string? UserAgent { get; init; }
        public DateTime CreatedUtc { get; init; }
        public DateTime LastActivityUtc { get; init; }
        public DateTime ExpiryUtc { get; init; }
        public bool IsActive { get; init; }
        public string? DeviceInfo { get; init; }
        public string? Location { get; init; }
    }

    /// <summary>
    /// User session model for persistence.
    /// </summary>
    public class UserSession : Beep.OilandGas.Models.Data.ModelEntityBase
    {
        public string SESSION_ID { get; set; } = Guid.NewGuid().ToString();
        public string USER_ID { get; set; } = string.Empty;
        public string SESSION_TOKEN { get; set; } = string.Empty;
        public string? IP_ADDRESS { get; set; }
        public string? USER_AGENT { get; set; }
        public string? DEVICE_INFO { get; set; }
        public string? LOCATION { get; set; }
        public DateTime CREATED_UTC { get; set; } = DateTime.UtcNow;
        public DateTime LAST_ACTIVITY_UTC { get; set; } = DateTime.UtcNow;
        public DateTime EXPIRY_UTC { get; set; }
    }
}
