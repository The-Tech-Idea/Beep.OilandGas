using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.ProductionAccounting.Inventory
{
    /// <summary>
    /// Manages inventory transactions (movements, adjustments).
    /// Uses Entity classes directly with IDataSource - no dictionary conversions.
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
                DESCRIPTION = description
            };

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForInsert(transaction, userId);
            var result = dataSource.InsertEntity(INVENTORY_TRANSACTION_TABLE, transaction);
            
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
    }
}
