using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.UserManagement.Core.DataAccess
{
    /// <summary>
    /// Interface for providing row-level security filters
    /// </summary>
    public interface IRowLevelSecurityProvider
    {
        /// <summary>
        /// Gets row-level security filters for a specific table and user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="tableName">The table name</param>
        /// <returns>List of AppFilter objects for row-level filtering</returns>
        Task<List<AppFilter>> GetRowFiltersAsync(string userId, string tableName);

        /// <summary>
        /// Checks if a user can access a specific row/entity
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="entity">The entity to check</param>
        /// <returns>True if user can access the row</returns>
        Task<bool> CanAccessRowAsync(string userId, IPPDMEntity entity);

        /// <summary>
        /// Applies row-level security filters to existing query filters
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="tableName">The table name</param>
        /// <param name="existingFilters">Existing filters to combine with RLS filters</param>
        /// <returns>Combined list of filters including RLS filters</returns>
        Task<List<AppFilter>> ApplyRowFiltersAsync(string userId, string tableName, List<AppFilter>? existingFilters = null);
    }
}
