using TheTechIdea.Beep.Report;
using Beep.OilandGas.UserManagement.Core.DataAccess;

namespace Beep.OilandGas.UserManagement.Services
{
    /// <summary>
    /// Implementation of combined data access filter provider
    /// Combines RLS filters, source filters, and data source filters
    /// </summary>
    public class DataAccessFilterProvider : IDataAccessFilterProvider
    {
        private readonly IRowLevelSecurityProvider _rlsProvider;
        private readonly ISourceAccessControl _sourceAccessControl;
        private readonly IDataSourceAccessControl _dataSourceAccessControl;

        public DataAccessFilterProvider(
            IRowLevelSecurityProvider rlsProvider,
            ISourceAccessControl sourceAccessControl,
            IDataSourceAccessControl dataSourceAccessControl)
        {
            _rlsProvider = rlsProvider ?? throw new ArgumentNullException(nameof(rlsProvider));
            _sourceAccessControl = sourceAccessControl ?? throw new ArgumentNullException(nameof(sourceAccessControl));
            _dataSourceAccessControl = dataSourceAccessControl ?? throw new ArgumentNullException(nameof(dataSourceAccessControl));
        }

        public async Task<IEnumerable<string>> GetDataSourceFiltersAsync(string userId)
        {
            return await _dataSourceAccessControl.GetAccessibleDataSourcesAsync(userId);
        }

        public async Task<List<AppFilter>> GetRowLevelFiltersAsync(string userId, string tableName)
        {
            return await _rlsProvider.GetRowFiltersAsync(userId, tableName);
        }

        public async Task<List<AppFilter>> GetSourceFiltersAsync(string userId, string tableName)
        {
            var filters = new List<AppFilter>();
            var sourceFilters = await _sourceAccessControl.FilterBySourceAsync(userId, filters, tableName);
            return sourceFilters;
        }

        public async Task<List<AppFilter>> CombineFiltersAsync(string userId, string tableName, List<AppFilter>? existingFilters = null)
        {
            var combinedFilters = existingFilters?.ToList() ?? new List<AppFilter>();

            // Add row-level security filters
            var rlsFilters = await _rlsProvider.GetRowFiltersAsync(userId, tableName);
            combinedFilters.AddRange(rlsFilters);

            // Add source-based filters
            var sourceFilters = await _sourceAccessControl.FilterBySourceAsync(userId, new List<AppFilter>(), tableName);
            combinedFilters.AddRange(sourceFilters);

            // Remove duplicates (same field name and operator)
            var uniqueFilters = new List<AppFilter>();
            var seenFilters = new HashSet<string>();

            foreach (var filter in combinedFilters)
            {
                var key = $"{filter.FieldName}:{filter.Operator}";
                if (!seenFilters.Contains(key))
                {
                    seenFilters.Add(key);
                    uniqueFilters.Add(filter);
                }
                else
                {
                    // If duplicate, keep the more restrictive one or combine values
                    var existing = uniqueFilters.First(f => 
                        f.FieldName.Equals(filter.FieldName, StringComparison.OrdinalIgnoreCase) &&
                        f.Operator.Equals(filter.Operator, StringComparison.OrdinalIgnoreCase));
                    
                    // For IN operators, combine values
                    if (filter.Operator.Equals("IN", StringComparison.OrdinalIgnoreCase))
                    {
                        var existingValues = existing.FilterValue?.Split(',') ?? Array.Empty<string>();
                        var newValues = filter.FilterValue?.Split(',') ?? Array.Empty<string>();
                        var combinedValues = existingValues.Union(newValues, StringComparer.OrdinalIgnoreCase).ToList();
                        existing.FilterValue = string.Join(",", combinedValues);
                    }
                }
            }

            return uniqueFilters;
        }
    }
}
