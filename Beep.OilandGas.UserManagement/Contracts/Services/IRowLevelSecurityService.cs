using System.Threading.Tasks;
using Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

namespace Beep.OilandGas.UserManagement.Contracts.Services
{
    /// <summary>
    /// Row-level security service for enforcing data access policies.
    /// Processes row access checks and applies field/asset scope filters.
    /// </summary>
    public interface IRowLevelSecurityService
    {
        /// <summary>
        /// Checks if a user has access to a specific row/entity.
        /// </summary>
        Task<RowAccessResult> CheckRowAccessAsync(CheckRowAccessRequest request);

        /// <summary>
        /// Applies row-level filters to a query based on user's scope assignments.
        /// Returns filter expressions that should be applied to the data query.
        /// </summary>
        Task<RowFilterResult> ApplyRowFiltersAsync(ApplyRowFiltersRequest request);

        /// <summary>
        /// Checks if a user has access to a specific data source.
        /// </summary>
        Task<AccessResult> CheckSourceAccessAsync(CheckSourceAccessRequest request);

        /// <summary>
        /// Checks if a user has access to a specific database.
        /// </summary>
        Task<AccessResult> CheckDatabaseAccessAsync(CheckDatabaseAccessRequest request);

        /// <summary>
        /// Checks if a user has access to a specific data source connection.
        /// </summary>
        Task<AccessResult> CheckDataSourceAccessAsync(CheckDataSourceAccessRequest request);

        /// <summary>
        /// Gets the user's accessible field IDs based on scope assignments.
        /// </summary>
        Task<string[]> GetUserAccessibleFieldsAsync(string userId);

        /// <summary>
        /// Gets the user's accessible asset IDs based on scope assignments.
        /// </summary>
        Task<string[]> GetUserAccessibleAssetsAsync(string userId);

        /// <summary>
        /// Gets the user's accessible organization IDs based on scope assignments.
        /// </summary>
        Task<string[]> GetUserAccessibleOrganizationsAsync(string userId);
    }

    /// <summary>
    /// Result of a row access check.
    /// </summary>
    public record RowAccessResult
    {
        public bool HasAccess { get; init; }
        public string? Reason { get; init; }
        public string? ScopeType { get; init; }
        public string[]? MatchingScopes { get; init; }
    }

    /// <summary>
    /// Result of applying row filters.
    /// </summary>
    public record RowFilterResult
    {
        public string[] FilterExpressions { get; init; } = Array.Empty<string>();
        public string ScopeType { get; init; } = string.Empty;
        public int FilterCount { get; init; }
    }

    /// <summary>
    /// Generic access check result.
    /// </summary>
    public record AccessResult
    {
        public bool HasAccess { get; init; }
        public string? Reason { get; init; }
    }
}
