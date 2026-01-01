using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Core.Interfaces.Security;
using Beep.OilandGas.UserManagement.Core.DataAccess;

namespace Beep.OilandGas.UserManagement.Services
{
    /// <summary>
    /// Implementation of source-based access control
    /// Filters data based on source system (e.g., SCADA, ManualEntry, Import)
    /// </summary>
    public class SourceAccessControlService : ISourceAccessControl
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly Dictionary<string, List<string>> _userSourceAccess; // userId -> list of accessible sources

        public SourceAccessControlService(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _userSourceAccess = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sets the accessible sources for a user
        /// </summary>
        public void SetUserSources(string userId, IEnumerable<string> sources)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            _userSourceAccess[userId] = sources?.ToList() ?? new List<string>();
        }

        /// <summary>
        /// Adds a source to a user's accessible sources
        /// </summary>
        public void AddUserSource(string userId, string source)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            if (!_userSourceAccess.ContainsKey(userId))
            {
                _userSourceAccess[userId] = new List<string>();
            }

            if (!string.IsNullOrWhiteSpace(source) && !_userSourceAccess[userId].Contains(source, StringComparer.OrdinalIgnoreCase))
            {
                _userSourceAccess[userId].Add(source);
            }
        }

        /// <summary>
        /// Removes a source from a user's accessible sources
        /// </summary>
        public void RemoveUserSource(string userId, string source)
        {
            if (_userSourceAccess.ContainsKey(userId))
            {
                _userSourceAccess[userId].RemoveAll(s => s.Equals(source, StringComparison.OrdinalIgnoreCase));
            }
        }

        public async Task<IEnumerable<string>> GetAccessibleSourcesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Enumerable.Empty<string>();
            }

            // Check if user has permission to bypass source restrictions
            var canBypass = await _authorizationService.UserHasPermissionAsync(
                userId, 
                "RLS:Source:*:Bypass");

            if (canBypass)
            {
                // User can access all sources
                return new[] { "*" };
            }

            // Get sources from cache/dictionary
            if (_userSourceAccess.ContainsKey(userId))
            {
                return _userSourceAccess[userId];
            }

            // Try to get sources from permissions
            var sources = new List<string>();
            var allSources = new[] { "SCADA", "ManualEntry", "Import", "API", "Legacy", "External" };
            
            foreach (var source in allSources)
            {
                var permission = $"RLS:Source:{source}:Access";
                if (await _authorizationService.UserHasPermissionAsync(userId, permission))
                {
                    sources.Add(source);
                }
            }

            // Cache the result
            if (sources.Any())
            {
                _userSourceAccess[userId] = sources;
            }

            return sources;
        }

        public async Task<bool> CanAccessSourceAsync(string userId, string source)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            // Check if user has permission to bypass source restrictions
            var canBypass = await _authorizationService.UserHasPermissionAsync(
                userId, 
                "RLS:Source:*:Bypass");

            if (canBypass)
            {
                return true;
            }

            var accessibleSources = await GetAccessibleSourcesAsync(userId);
            return accessibleSources.Contains(source, StringComparer.OrdinalIgnoreCase) ||
                   accessibleSources.Contains("*", StringComparer.OrdinalIgnoreCase);
        }

        public async Task<List<AppFilter>> FilterBySourceAsync(string userId, List<AppFilter> filters, string tableName)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return filters ?? new List<AppFilter>();
            }

            var accessibleSources = (await GetAccessibleSourcesAsync(userId)).ToList();

            // If user can access all sources, no filter needed
            if (accessibleSources.Contains("*", StringComparer.OrdinalIgnoreCase) || !accessibleSources.Any())
            {
                return filters ?? new List<AppFilter>();
            }

            var resultFilters = filters?.ToList() ?? new List<AppFilter>();

            // Check if SOURCE column exists in filters already
            var existingSourceFilter = resultFilters.FirstOrDefault(f => 
                f.FieldName.Equals("SOURCE", StringComparison.OrdinalIgnoreCase));

            if (existingSourceFilter != null)
            {
                // Combine with existing source filter
                // This is simplified - in practice you might need more complex logic
                return resultFilters;
            }

            // Add source filter
            if (accessibleSources.Count == 1)
            {
                resultFilters.Add(new AppFilter
                {
                    FieldName = "SOURCE",
                    Operator = "=",
                    FilterValue = accessibleSources[0]
                });
            }
            else if (accessibleSources.Count > 1)
            {
                resultFilters.Add(new AppFilter
                {
                    FieldName = "SOURCE",
                    Operator = "IN",
                    FilterValue = string.Join(",", accessibleSources)
                });
            }

            return resultFilters;
        }
    }
}
