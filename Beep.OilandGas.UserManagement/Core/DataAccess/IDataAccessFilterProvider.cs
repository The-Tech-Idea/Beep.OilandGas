using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.UserManagement.Core.DataAccess
{
    /// <summary>
    /// Interface for providing combined data access filters (RLS + Source + DataSource)
    /// </summary>
    public interface IDataAccessFilterProvider
    {
        /// <summary>
        /// Gets data source filters for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of accessible data source names</returns>
        Task<IEnumerable<string>> GetDataSourceFiltersAsync(string userId);

        /// <summary>
        /// Gets row-level security filters for a table
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="tableName">The table name</param>
        /// <returns>List of AppFilter objects for RLS</returns>
        Task<List<AppFilter>> GetRowLevelFiltersAsync(string userId, string tableName);

        /// <summary>
        /// Gets source-based filters for a table
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="tableName">The table name</param>
        /// <returns>List of AppFilter objects for source filtering</returns>
        Task<List<AppFilter>> GetSourceFiltersAsync(string userId, string tableName);

        /// <summary>
        /// Combines all access control filters (RLS + Source + DataSource)
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="tableName">The table name</param>
        /// <param name="existingFilters">Existing filters to combine with</param>
        /// <returns>Combined list of filters</returns>
        Task<List<AppFilter>> CombineFiltersAsync(string userId, string tableName, List<AppFilter>? existingFilters = null);
    }
}
