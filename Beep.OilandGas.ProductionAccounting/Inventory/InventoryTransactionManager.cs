namespace Beep.OilandGas.ProductionAccounting.Inventory
{
    /// <summary>
    /// Manages inventory transactions (movements, adjustments).
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class InventoryTransactionManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<InventoryTransactionManager>? _logger;
        private readonly string _connectionName;
        private const string INVENTORY_TRANSACTION_TABLE = "INVENTORY_TRANSACTION";

        public InventoryTransactionManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<InventoryTransactionManager>? logger = null,
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
        /// Creates an inventory transaction.
        /// </summary>
        public INVENTORY_TRANSACTION CreateTransaction(
            string inventoryItemId,
            string transactionType,
            DateTime transactionDate,
            decimal quantity,
            decimal? unitCost,
            string description,
            string userId,
            string? connectionName = null)
        {
            var transaction = new INVENTORY_TRANSACTION
            {
                INVENTORY_TRANSACTION_ID = Guid.NewGuid().ToString(),
                INVENTORY_ITEM_ID = inventoryItemId,
                TRANSACTION_TYPE = transactionType,
                TRANSACTION_DATE = transactionDate,
                QUANTITY = quantity,
                UNIT_COST = unitCost,
                TOTAL_COST = quantity * (unitCost ?? 0m),
                DESCRIPTION = description,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var transactionData = ConvertInventoryTransactionToDictionary(transaction);
            var result = dataSource.InsertEntity(INVENTORY_TRANSACTION_TABLE, transactionData);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create inventory transaction {TransactionId}: {Error}", transaction.INVENTORY_TRANSACTION_ID, errorMessage);
                throw new InvalidOperationException($"Failed to save inventory transaction: {errorMessage}");
            }

            _logger?.LogDebug("Created inventory transaction {TransactionId} in database", transaction.INVENTORY_TRANSACTION_ID);
            return transaction;
        }

        /// <summary>
        /// Gets a transaction by ID.
        /// </summary>
        public INVENTORY_TRANSACTION? GetTransaction(string transactionId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(transactionId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INVENTORY_TRANSACTION_ID", Operator = "=", FilterValue = transactionId }
            };

            var results = dataSource.GetEntityAsync(INVENTORY_TRANSACTION_TABLE, filters).GetAwaiter().GetResult();
            var transactionData = results?.FirstOrDefault();
            
            if (transactionData == null)
                return null;

            return transactionData as INVENTORY_TRANSACTION;
        }

        private Dictionary<string, object> ConvertInventoryTransactionToDictionary(INVENTORY_TRANSACTION transaction)
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(transaction.INVENTORY_TRANSACTION_ID)) dict["INVENTORY_TRANSACTION_ID"] = transaction.INVENTORY_TRANSACTION_ID;
            if (!string.IsNullOrEmpty(transaction.INVENTORY_ITEM_ID)) dict["INVENTORY_ITEM_ID"] = transaction.INVENTORY_ITEM_ID;
            if (!string.IsNullOrEmpty(transaction.TRANSACTION_TYPE)) dict["TRANSACTION_TYPE"] = transaction.TRANSACTION_TYPE;
            if (transaction.TRANSACTION_DATE.HasValue) dict["TRANSACTION_DATE"] = transaction.TRANSACTION_DATE.Value;
            if (transaction.QUANTITY.HasValue) dict["QUANTITY"] = transaction.QUANTITY.Value;
            if (transaction.UNIT_COST.HasValue) dict["UNIT_COST"] = transaction.UNIT_COST.Value;
            if (transaction.TOTAL_COST.HasValue) dict["TOTAL_COST"] = transaction.TOTAL_COST.Value;
            if (!string.IsNullOrEmpty(transaction.DESCRIPTION)) dict["DESCRIPTION"] = transaction.DESCRIPTION;
            if (!string.IsNullOrEmpty(transaction.ACTIVE_IND)) dict["ACTIVE_IND"] = transaction.ACTIVE_IND;
            if (!string.IsNullOrEmpty(transaction.ROW_CREATED_BY)) dict["ROW_CREATED_BY"] = transaction.ROW_CREATED_BY;
            if (transaction.ROW_CREATED_DATE.HasValue) dict["ROW_CREATED_DATE"] = transaction.ROW_CREATED_DATE.Value;
            if (!string.IsNullOrEmpty(transaction.ROW_CHANGED_BY)) dict["ROW_CHANGED_BY"] = transaction.ROW_CHANGED_BY;
            if (transaction.ROW_CHANGED_DATE.HasValue) dict["ROW_CHANGED_DATE"] = transaction.ROW_CHANGED_DATE.Value;
            return dict;
        }

        private INVENTORY_TRANSACTION ConvertDictionaryToInventoryTransaction(Dictionary<string, object> dict)
        {
            var transaction = new INVENTORY_TRANSACTION();
            if (dict.TryGetValue("INVENTORY_TRANSACTION_ID", out var transactionId)) transaction.INVENTORY_TRANSACTION_ID = transactionId?.ToString();
            if (dict.TryGetValue("INVENTORY_ITEM_ID", out var itemId)) transaction.INVENTORY_ITEM_ID = itemId?.ToString();
            if (dict.TryGetValue("TRANSACTION_TYPE", out var transactionType)) transaction.TRANSACTION_TYPE = transactionType?.ToString();
            if (dict.TryGetValue("TRANSACTION_DATE", out var transactionDate)) transaction.TRANSACTION_DATE = transactionDate != null ? Convert.ToDateTime(transactionDate) : (DateTime?)null;
            if (dict.TryGetValue("QUANTITY", out var quantity)) transaction.QUANTITY = quantity != null ? Convert.ToDecimal(quantity) : (decimal?)null;
            if (dict.TryGetValue("UNIT_COST", out var unitCost)) transaction.UNIT_COST = unitCost != null ? Convert.ToDecimal(unitCost) : (decimal?)null;
            if (dict.TryGetValue("TOTAL_COST", out var totalCost)) transaction.TOTAL_COST = totalCost != null ? Convert.ToDecimal(totalCost) : (decimal?)null;
            if (dict.TryGetValue("DESCRIPTION", out var description)) transaction.DESCRIPTION = description?.ToString();
            if (dict.TryGetValue("ACTIVE_IND", out var activeInd)) transaction.ACTIVE_IND = activeInd?.ToString();
            if (dict.TryGetValue("ROW_CREATED_BY", out var createdBy)) transaction.ROW_CREATED_BY = createdBy?.ToString();
            if (dict.TryGetValue("ROW_CREATED_DATE", out var createdDate)) transaction.ROW_CREATED_DATE = createdDate != null ? Convert.ToDateTime(createdDate) : (DateTime?)null;
            if (dict.TryGetValue("ROW_CHANGED_BY", out var changedBy)) transaction.ROW_CHANGED_BY = changedBy?.ToString();
            if (dict.TryGetValue("ROW_CHANGED_DATE", out var changedDate)) transaction.ROW_CHANGED_DATE = changedDate != null ? Convert.ToDateTime(changedDate) : (DateTime?)null;
            return transaction;
        }
    }
}
