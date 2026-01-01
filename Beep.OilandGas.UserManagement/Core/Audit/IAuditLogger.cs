namespace Beep.OilandGas.UserManagement.Core.Audit
{
    /// <summary>
    /// Interface for logging authorization decisions and security events
    /// Platform-agnostic interface (implementations can write to database, file, etc.)
    /// </summary>
    public interface IAuditLogger
    {
        /// <summary>
        /// Logs an authorization decision
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="permission">The permission checked</param>
        /// <param name="result">Whether authorization was granted</param>
        /// <param name="resource">Optional resource identifier</param>
        /// <param name="reason">Optional reason for the decision</param>
        Task LogAuthorizationAsync(string userId, string permission, bool result, string? resource = null, string? reason = null);

        /// <summary>
        /// Logs a data access event
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="action">The action performed (Read, Write, Delete, etc.)</param>
        /// <param name="resourceType">The type of resource (table name, entity type, etc.)</param>
        /// <param name="resourceId">The resource identifier</param>
        /// <param name="success">Whether the action was successful</param>
        Task LogDataAccessAsync(string userId, string action, string resourceType, string? resourceId = null, bool success = true);

        /// <summary>
        /// Logs a user authentication event
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="username">The username</param>
        /// <param name="success">Whether authentication was successful</param>
        /// <param name="reason">Optional reason for failure</param>
        Task LogAuthenticationAsync(string userId, string username, bool success, string? reason = null);

        /// <summary>
        /// Logs a role or permission change
        /// </summary>
        /// <param name="changedByUserId">The user who made the change</param>
        /// <param name="targetUserId">The user whose permissions were changed</param>
        /// <param name="changeType">Type of change (RoleAdded, RoleRemoved, PermissionAdded, etc.)</param>
        /// <param name="changeDetails">Details of the change</param>
        Task LogPermissionChangeAsync(string changedByUserId, string targetUserId, string changeType, string changeDetails);
    }
}
