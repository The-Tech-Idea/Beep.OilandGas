namespace Beep.OilandGas.ProductionAccounting.Pricing
{
    /// <summary>
    /// Manages price indexes.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class PriceIndexManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<PriceIndexManager>? _logger;
        private readonly string _connectionName;
        private const string PRICE_INDEX_TABLE = "PRICE_INDEX";

        public PriceIndexManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PriceIndexManager>? logger = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Registers a price index.
        /// </summary>
        public async Task RegisterIndexAsync(PriceIndex index, string userId = "system", string? connectionName = null)
        {
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            if (string.IsNullOrEmpty(index.IndexName))
                throw new ArgumentException("Index name cannot be null or empty.", nameof(index));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Check if index already exists for this date
            var existingFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INDEX_NAME", Operator = "=", FilterValue = index.IndexName },
                new AppFilter { FieldName = "INDEX_DATE", Operator = "=", FilterValue = index.Date.ToString("yyyy-MM-dd") }
            };

            var existing = await dataSource.GetEntityAsync(PRICE_INDEX_TABLE, existingFilters);
            if (existing != null && existing.Any())
            {
                // Update existing
                var indexData = ConvertPriceIndexToDictionary(index);
                var result = dataSource.UpdateEntity(PRICE_INDEX_TABLE, indexData);
                if (result != null && result.Errors != null && result.Errors.Count > 0)
                {
                    var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                    _logger?.LogError("Failed to update price index {IndexName}: {Error}", index.IndexName, errorMessage);
                    throw new InvalidOperationException($"Failed to update price index: {errorMessage}");
                }
            }
            else
            {
                // Insert new
                var indexData = ConvertPriceIndexToDictionary(index);
                var result = dataSource.InsertEntity(PRICE_INDEX_TABLE, indexData);
                if (result != null && result.Errors != null && result.Errors.Count > 0)
                {
                    var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                    _logger?.LogError("Failed to register price index {IndexName}: {Error}", index.IndexName, errorMessage);
                    throw new InvalidOperationException($"Failed to save price index: {errorMessage}");
                }
            }

            _logger?.LogDebug("Registered price index {IndexName} for date {Date} to database", index.IndexName, index.Date);
        }

        /// <summary>
        /// Registers a price index (synchronous wrapper).
        /// </summary>
        public void RegisterIndex(PriceIndex index)
        {
            RegisterIndexAsync(index).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets the latest price for an index.
        /// </summary>
        public async Task<PriceIndex?> GetLatestPriceAsync(string indexName, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(indexName))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INDEX_NAME", Operator = "=", FilterValue = indexName }
            };

            var results = await dataSource.GetEntityAsync(PRICE_INDEX_TABLE, filters);
            if (results == null || !results.Any())
                return null;

            // Order by date descending and get first
            var prices = results.Cast<PriceIndex>()
                .Where(p => p != null)
                .OrderByDescending(p => p!.Date)
                .ToList();

            return prices.FirstOrDefault();
        }

        /// <summary>
        /// Gets the latest price for an index (synchronous wrapper).
        /// </summary>
        public PriceIndex? GetLatestPrice(string indexName)
        {
            return GetLatestPriceAsync(indexName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets price for an index on a specific date.
        /// </summary>
        public async Task<PriceIndex?> GetPriceAsync(string indexName, DateTime date, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(indexName))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INDEX_NAME", Operator = "=", FilterValue = indexName },
                new AppFilter { FieldName = "INDEX_DATE", Operator = ">=", FilterValue = date.Date.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "INDEX_DATE", Operator = "<", FilterValue = date.Date.AddDays(1).ToString("yyyy-MM-dd") }
            };

            var results = await dataSource.GetEntityAsync(PRICE_INDEX_TABLE, filters);
            if (results == null || !results.Any())
                return null;

            return results.Cast<PriceIndex>()
                .Where(p => p != null && p.Date.Date == date.Date)
                .OrderByDescending(p => p!.Date)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets price for an index on a specific date (synchronous wrapper).
        /// </summary>
        public PriceIndex? GetPrice(string indexName, DateTime date)
        {
            return GetPriceAsync(indexName, date).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets prices for an index in a date range.
        /// </summary>
        public async Task<IEnumerable<PriceIndex>> GetPricesAsync(string indexName, DateTime startDate, DateTime endDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(indexName))
                return Enumerable.Empty<PriceIndex>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INDEX_NAME", Operator = "=", FilterValue = indexName },
                new AppFilter { FieldName = "INDEX_DATE", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "INDEX_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") }
            };

            var results = await dataSource.GetEntityAsync(PRICE_INDEX_TABLE, filters);
            if (results == null || !results.Any())
                return Enumerable.Empty<PriceIndex>();

            return results.Cast<PriceIndex>()
                .Where(p => p != null && p.Date >= startDate && p.Date <= endDate)
                .OrderBy(p => p!.Date)!;
        }

        /// <summary>
        /// Gets prices for an index in a date range (synchronous wrapper).
        /// </summary>
        public IEnumerable<PriceIndex> GetPrices(string indexName, DateTime startDate, DateTime endDate)
        {
            return GetPricesAsync(indexName, startDate, endDate).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets average price for an index in a date range.
        /// </summary>
        public async Task<decimal?> GetAveragePriceAsync(string indexName, DateTime startDate, DateTime endDate, string? connectionName = null)
        {
            var prices = (await GetPricesAsync(indexName, startDate, endDate, connectionName)).ToList();
            if (prices.Count == 0)
                return null;

            return prices.Average(p => p.Price);
        }

        /// <summary>
        /// Gets average price for an index in a date range (synchronous wrapper).
        /// </summary>
        public decimal? GetAveragePrice(string indexName, DateTime startDate, DateTime endDate)
        {
            return GetAveragePriceAsync(indexName, startDate, endDate).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets all available index names.
        /// </summary>
        public async Task<IEnumerable<string>> GetAvailableIndexesAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var results = await dataSource.GetEntityAsync(PRICE_INDEX_TABLE, null);
            if (results == null || !results.Any())
                return Enumerable.Empty<string>();

            return results
                .Where(r => r is Dictionary<string, object> dict && dict.ContainsKey("INDEX_NAME") && dict["INDEX_NAME"] != null)
                .Select(r => ((Dictionary<string, object>)r)["INDEX_NAME"]?.ToString() ?? string.Empty)
                .Distinct()
                .Where(name => !string.IsNullOrEmpty(name));
        }

        /// <summary>
        /// Gets all available index names (synchronous wrapper).
        /// </summary>
        public IEnumerable<string> GetAvailableIndexes()
        {
            return GetAvailableIndexesAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Initializes with standard price indexes.
        /// Checks if indexes already exist before inserting.
        /// </summary>
        public async Task InitializeStandardIndexesAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var today = DateTime.Now.Date;

            var standardIndexes = new[]
            {
                new { Name = "WTI", Point = "Cushing, Oklahoma", Source = "NYMEX" },
                new { Name = "Brent", Point = "North Sea", Source = "ICE" },
                new { Name = "LLS", Point = "Louisiana", Source = "Platts" },
                new { Name = "WCS", Point = "Hardisty, Alberta", Source = "Platts" }
            };

            foreach (var stdIndex in standardIndexes)
            {
                // Check if index already exists for today
                var existing = await GetPriceAsync(stdIndex.Name, today, connName);
                if (existing == null)
                {
                    // Only create if it doesn't exist
                    await RegisterIndexAsync(new PriceIndex
                    {
                        IndexId = Guid.NewGuid().ToString(),
                        IndexName = stdIndex.Name,
                        PricingPoint = stdIndex.Point,
                        Date = today,
                        Price = 0m, // Will be updated with actual prices
                        Source = stdIndex.Source
                    }, "system", connName);
                }
            }
        }

        /// <summary>
        /// Initializes with standard price indexes (synchronous wrapper).
        /// </summary>
        public void InitializeStandardIndexes()
        {
            InitializeStandardIndexesAsync().GetAwaiter().GetResult();
        }

        #region Helper Methods - Model to Dictionary Conversion

        /// <summary>
        /// Converts PriceIndex to dictionary for database storage.
        /// </summary>
        private Dictionary<string, object> ConvertPriceIndexToDictionary(PriceIndex index)
        {
            return new Dictionary<string, object>
            {
                { "PRICE_INDEX_ID", index.IndexId },
                { "INDEX_NAME", index.IndexName ?? string.Empty },
                { "PRICING_POINT", index.PricingPoint ?? string.Empty },
                { "INDEX_DATE", index.Date },
                { "PRICE", index.Price },
                { "SOURCE", index.Source ?? string.Empty },
                { "CURRENCY", index.Currency ?? "USD" },
                { "UNIT", index.Unit ?? "BBL" }
            };
        }

        /// <summary>
        /// Converts dictionary to PriceIndex.
        /// </summary>
        private PriceIndex? ConvertDictionaryToPriceIndex(Dictionary<string, object> dict)
        {
            if (dict == null || !dict.ContainsKey("PRICE_INDEX_ID"))
                return null;

            return new PriceIndex
            {
                IndexId = dict["PRICE_INDEX_ID"]?.ToString() ?? string.Empty,
                IndexName = dict.ContainsKey("INDEX_NAME") ? dict["INDEX_NAME"]?.ToString() ?? string.Empty : string.Empty,
                PricingPoint = dict.ContainsKey("PRICING_POINT") ? dict["PRICING_POINT"]?.ToString() ?? string.Empty : string.Empty,
                Date = dict.ContainsKey("INDEX_DATE") && dict["INDEX_DATE"] != DBNull.Value
                    ? Convert.ToDateTime(dict["INDEX_DATE"])
                    : DateTime.MinValue,
                Price = dict.ContainsKey("PRICE") ? Convert.ToDecimal(dict["PRICE"]) : 0m,
                Source = dict.ContainsKey("SOURCE") ? dict["SOURCE"]?.ToString() ?? string.Empty : string.Empty,
                Currency = dict.ContainsKey("CURRENCY") ? dict["CURRENCY"]?.ToString() ?? "USD" : "USD",
                Unit = dict.ContainsKey("UNIT") ? dict["UNIT"]?.ToString() ?? "BBL" : "BBL"
            };
        }

        #endregion
    }
}
