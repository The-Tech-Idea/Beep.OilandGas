namespace Beep.OilandGas.UserManagement.Core.Session
{
    /// <summary>
    /// Interface for managing user sessions
    /// Platform-agnostic (can be implemented for desktop or web)
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>
        /// Creates a new session for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="additionalData">Additional session data</param>
        /// <returns>Session ID</returns>
        Task<string> CreateSessionAsync(string userId, Dictionary<string, string>? additionalData = null);

        /// <summary>
        /// Validates a session
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        /// <returns>User ID if session is valid, null otherwise</returns>
        Task<string?> ValidateSessionAsync(string sessionId);

        /// <summary>
        /// Invalidates a session
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        Task InvalidateSessionAsync(string sessionId);

        /// <summary>
        /// Invalidates all sessions for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        Task InvalidateUserSessionsAsync(string userId);

        /// <summary>
        /// Gets session data
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        /// <returns>Session data dictionary</returns>
        Task<Dictionary<string, string>?> GetSessionDataAsync(string sessionId);

        /// <summary>
        /// Updates session data
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        /// <param name="data">Data to update</param>
        Task UpdateSessionDataAsync(string sessionId, Dictionary<string, string> data);

        /// <summary>
        /// Extends the session expiration
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        /// <param name="expirationMinutes">Minutes to extend</param>
        Task ExtendSessionAsync(string sessionId, int expirationMinutes = 30);
    }
}
