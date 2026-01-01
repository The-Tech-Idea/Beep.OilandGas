using TheTechIdea.Beep.Report;
using Beep.OilandGas.UserManagement.Core.Authorization;

namespace Beep.OilandGas.UserManagement.Core.DataAccess
{
    /// <summary>
    /// Context for row-level security evaluation
    /// </summary>
    public class RowLevelSecurityContext
    {
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user roles
        /// </summary>
        public IEnumerable<string> UserRoles { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Gets or sets the user permissions
        /// </summary>
        public IEnumerable<string> UserPermissions { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Gets or sets the tenant ID
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the table name
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entity type
        /// </summary>
        public Type? EntityType { get; set; }

        /// <summary>
        /// Gets or sets existing filters
        /// </summary>
        public List<AppFilter> ExistingFilters { get; set; } = new List<AppFilter>();

        /// <summary>
        /// Gets or sets the data source name
        /// </summary>
        public string? DataSourceName { get; set; }

        /// <summary>
        /// Gets or sets the database name
        /// </summary>
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Creates a context from AuthorizationContext
        /// </summary>
        public static RowLevelSecurityContext FromAuthorizationContext(
            AuthorizationContext authContext,
            string tableName,
            List<AppFilter>? existingFilters = null)
        {
            return new RowLevelSecurityContext
            {
                UserId = authContext.UserId,
                UserRoles = authContext.Roles,
                UserPermissions = authContext.Permissions,
                TenantId = authContext.TenantId,
                TableName = tableName,
                ExistingFilters = existingFilters ?? new List<AppFilter>()
            };
        }
    }
}
