namespace Beep.OilandGas.UserManagement.Core.DataAccess
{
    /// <summary>
    /// Interface for controlling access to databases and data sources
    /// </summary>
    public interface IDataSourceAccessControl
    {
        /// <summary>
        /// Gets the list of data sources accessible to a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of accessible data source names</returns>
        Task<IEnumerable<string>> GetAccessibleDataSourcesAsync(string userId);

        /// <summary>
        /// Checks if a user can access a specific data source
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="dataSourceName">The data source name</param>
        /// <returns>True if user can access the data source</returns>
        Task<bool> CanAccessDataSourceAsync(string userId, string dataSourceName);

        /// <summary>
        /// Gets the list of databases accessible to a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of accessible database names</returns>
        Task<IEnumerable<string>> GetAccessibleDatabasesAsync(string userId);

        /// <summary>
        /// Checks if a user can access a specific database
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="databaseName">The database name</param>
        /// <returns>True if user can access the database</returns>
        Task<bool> CanAccessDatabaseAsync(string userId, string databaseName);
    }
}
