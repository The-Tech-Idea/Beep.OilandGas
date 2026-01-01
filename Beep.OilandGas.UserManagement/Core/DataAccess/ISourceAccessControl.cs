using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.UserManagement.Core.DataAccess
{
    /// <summary>
    /// Interface for source-based access control
    /// </summary>
    public interface ISourceAccessControl
    {
        /// <summary>
        /// Gets the list of source systems accessible to a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of accessible source identifiers</returns>
        Task<IEnumerable<string>> GetAccessibleSourcesAsync(string userId);

        /// <summary>
        /// Checks if a user can access data from a specific source
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="source">The source identifier</param>
        /// <returns>True if user can access the source</returns>
        Task<bool> CanAccessSourceAsync(string userId, string source);

        /// <summary>
        /// Adds source-based filters to existing query filters
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="filters">Existing filters</param>
        /// <param name="tableName">The table name (may have SOURCE column)</param>
        /// <returns>Filters with source restrictions applied</returns>
        Task<List<AppFilter>> FilterBySourceAsync(string userId, List<AppFilter> filters, string tableName);
    }
}
