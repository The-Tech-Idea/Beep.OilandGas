using Beep.OilandGas.Models.Core.Interfaces.Security;
using Beep.OilandGas.UserManagement.Core.DataAccess;

namespace Beep.OilandGas.UserManagement.Services
{
    /// <summary>
    /// Implementation of data source and database-level access control
    /// </summary>
    public class DataSourceAccessControlService : IDataSourceAccessControl
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly Dictionary<string, List<string>> _userDataSourceAccess; // userId -> list of accessible data sources
        private readonly Dictionary<string, List<string>> _userDatabaseAccess; // userId -> list of accessible databases

        public DataSourceAccessControlService(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _userDataSourceAccess = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            _userDatabaseAccess = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sets the accessible data sources for a user
        /// </summary>
        public void SetUserDataSources(string userId, IEnumerable<string> dataSources)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            _userDataSourceAccess[userId] = dataSources?.ToList() ?? new List<string>();
        }

        /// <summary>
        /// Sets the accessible databases for a user
        /// </summary>
        public void SetUserDatabases(string userId, IEnumerable<string> databases)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            _userDatabaseAccess[userId] = databases?.ToList() ?? new List<string>();
        }

        public async Task<IEnumerable<string>> GetAccessibleDataSourcesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Enumerable.Empty<string>();
            }

            // Check if user has permission to access all data sources
            var canAccessAll = await _authorizationService.UserHasPermissionAsync(
                userId, 
                "DataSource:*:Access");

            if (canAccessAll)
            {
                return new[] { "*" };
            }

            // Get from cache/dictionary
            if (_userDataSourceAccess.ContainsKey(userId))
            {
                return _userDataSourceAccess[userId];
            }

            // Try to get from permissions
            var dataSources = new List<string>();
            // In a real implementation, you would query permissions or a database
            // For now, we'll check common data source permissions
            var commonDataSources = new[] { "ProductionDB", "DevelopmentDB", "TestDB", "ArchiveDB" };

            foreach (var dataSource in commonDataSources)
            {
                var permission = $"DataSource:{dataSource}:Access";
                if (await _authorizationService.UserHasPermissionAsync(userId, permission))
                {
                    dataSources.Add(dataSource);
                }
            }

            // Cache the result
            if (dataSources.Any())
            {
                _userDataSourceAccess[userId] = dataSources;
            }

            return dataSources;
        }

        public async Task<bool> CanAccessDataSourceAsync(string userId, string dataSourceName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(dataSourceName))
            {
                return false;
            }

            // Check if user has permission to access all data sources
            var canAccessAll = await _authorizationService.UserHasPermissionAsync(
                userId, 
                "DataSource:*:Access");

            if (canAccessAll)
            {
                return true;
            }

            var accessibleDataSources = await GetAccessibleDataSourcesAsync(userId);
            return accessibleDataSources.Contains(dataSourceName, StringComparer.OrdinalIgnoreCase) ||
                   accessibleDataSources.Contains("*", StringComparer.OrdinalIgnoreCase);
        }

        public async Task<IEnumerable<string>> GetAccessibleDatabasesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Enumerable.Empty<string>();
            }

            // Check if user has permission to access all databases
            var canAccessAll = await _authorizationService.UserHasPermissionAsync(
                userId, 
                "Database:*:Access");

            if (canAccessAll)
            {
                return new[] { "*" };
            }

            // Get from cache/dictionary
            if (_userDatabaseAccess.ContainsKey(userId))
            {
                return _userDatabaseAccess[userId];
            }

            // Try to get from permissions
            var databases = new List<string>();
            // In a real implementation, you would query permissions or a database
            var commonDatabases = new[] { "PPDM39", "Production", "Development", "Test" };

            foreach (var database in commonDatabases)
            {
                var permission = $"Database:{database}:Access";
                if (await _authorizationService.UserHasPermissionAsync(userId, permission))
                {
                    databases.Add(database);
                }
            }

            // Cache the result
            if (databases.Any())
            {
                _userDatabaseAccess[userId] = databases;
            }

            return databases;
        }

        public async Task<bool> CanAccessDatabaseAsync(string userId, string databaseName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(databaseName))
            {
                return false;
            }

            // Check if user has permission to access all databases
            var canAccessAll = await _authorizationService.UserHasPermissionAsync(
                userId, 
                "Database:*:Access");

            if (canAccessAll)
            {
                return true;
            }

            var accessibleDatabases = await GetAccessibleDatabasesAsync(userId);
            return accessibleDatabases.Contains(databaseName, StringComparer.OrdinalIgnoreCase) ||
                   accessibleDatabases.Contains("*", StringComparer.OrdinalIgnoreCase);
        }
    }
}
