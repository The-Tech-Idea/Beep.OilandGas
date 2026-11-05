using System.Threading.Tasks;
using TheTechIdea.Beep.Report;
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
        Task<RowAccessResult> CheckRowAccessAsync(string userId, CheckRowAccessRequest request);

        /// <summary>
        /// Applies row-level filters to a query based on user's scope assignments.
        /// Returns the current read predicates and validated SDK filters; never SQL text.
        /// </summary>
        Task<RowFilterResult> ApplyRowFiltersAsync(string userId, ApplyRowFiltersRequest request);

        /// <summary>
        /// Checks if a user has access to a specific data source.
        /// </summary>
        Task<AccessResult> CheckSourceAccessAsync(string userId, CheckSourceAccessRequest request);

        /// <summary>
        /// Checks if a user has access to a specific database.
        /// </summary>
        Task<AccessResult> CheckDatabaseAccessAsync(string userId, CheckDatabaseAccessRequest request);

        /// <summary>
        /// Checks if a user has access to a specific data source connection.
        /// </summary>
        Task<AccessResult> CheckDataSourceAccessAsync(string userId, CheckDataSourceAccessRequest request);

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
        // OR between groups; AND their filters, every company constraint and supplied filter within each group.
        // No groups denies all rows; constraints do not independently grant access.
        public IReadOnlyList<IReadOnlyList<AppFilter>> ReadConjunctions { get; init; } = Array.Empty<IReadOnlyList<AppFilter>>();
        public IReadOnlyList<AppFilter> CompanyConstraints { get; init; } = Array.Empty<AppFilter>();
        public IReadOnlyList<AppFilter> Filters { get; init; } = Array.Empty<AppFilter>();
        public bool DenyAll => ReadConjunctions.Count == 0;
        public string ScopeType { get; init; } = string.Empty;
        public int FilterCount => ReadConjunctions.Sum(group => group.Count);
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
