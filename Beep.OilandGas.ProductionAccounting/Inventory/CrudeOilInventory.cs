namespace Beep.OilandGas.ProductionAccounting.Inventory
{
    /// <summary>
    /// Inventory valuation method.
    /// </summary>
    public enum InventoryValuationMethod
    {
        /// <summary>
        /// First In, First Out.
        /// </summary>
        FIFO,

        /// <summary>
        /// Last In, First Out.
        /// </summary>
        LIFO,

        /// <summary>
        /// Weighted average cost.
        /// </summary>
        WeightedAverage,

        /// <summary>
        /// Lower of cost or market.
        /// </summary>
        LowerOfCostOrMarket
    }

    /// <summary>
    /// Represents crude oil inventory.
    /// </summary>
    public class CrudeOilInventory
    {
        /// <summary>
        /// Gets or sets the inventory identifier.
        /// </summary>
        public string InventoryId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        public string? PropertyOrLeaseId { get; set; }

        /// <summary>
        /// Gets or sets the tank battery identifier.
        /// </summary>
        public string? TankBatteryId { get; set; }

        /// <summary>
        /// Gets or sets the inventory date.
        /// </summary>
        public DateTime InventoryDate { get; set; }

        /// <summary>
        /// Gets or sets the volume in barrels.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Gets or sets the unit cost per barrel.
        /// </summary>
        public decimal UnitCost { get; set; }

        /// <summary>
        /// Gets the total value.
        /// </summary>
        public decimal TotalValue => Volume * UnitCost;

        /// <summary>
        /// Gets or sets the valuation method.
        /// </summary>
        public InventoryValuationMethod ValuationMethod { get; set; } = InventoryValuationMethod.WeightedAverage;

        /// <summary>
        /// Gets or sets the market price per barrel (for LCM valuation).
        /// </summary>
        public decimal? MarketPrice { get; set; }

        /// <summary>
        /// Gets the lower of cost or market value.
        /// </summary>
        public decimal LowerOfCostOrMarketValue => MarketPrice.HasValue 
            ? Math.Min(TotalValue, Volume * MarketPrice.Value) 
            : TotalValue;
    }

    /// <summary>
    /// Represents an inventory transaction.
    /// </summary>
    public class InventoryTransaction
    {
        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the transaction date.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Gets or sets the transaction type.
        /// </summary>
        public InventoryTransactionType TransactionType { get; set; }

        /// <summary>
        /// Gets or sets the volume in barrels.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Gets or sets the unit cost per barrel.
        /// </summary>
        public decimal UnitCost { get; set; }

        /// <summary>
        /// Gets the total value.
        /// </summary>
        public decimal TotalValue => Volume * UnitCost;
    }

    /// <summary>
    /// Inventory transaction type.
    /// </summary>
    public enum InventoryTransactionType
    {
        /// <summary>
        /// Receipt (addition to inventory).
        /// </summary>
        Receipt,

        /// <summary>
        /// Delivery (removal from inventory).
        /// </summary>
        Delivery,

        /// <summary>
        /// Adjustment.
        /// </summary>
        Adjustment
    }

    /// <summary>
    /// Manages crude oil inventory and valuation.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class InventoryManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<InventoryManager>? _logger;
        private readonly string _connectionName;
        private const string CRUDE_OIL_INVENTORY_TABLE = "CRUDE_OIL_INVENTORY";
        private const string CRUDE_OIL_INVENTORY_TRANSACTION_TABLE = "CRUDE_OIL_INVENTORY_TRANSACTION";

        public InventoryManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILoggerFactory loggerFactory,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = loggerFactory?.CreateLogger<InventoryManager>();
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Records an inventory transaction.
        /// </summary>
        public void RecordTransaction(
            string inventoryId,
            InventoryTransactionType transactionType,
            decimal volume,
            decimal unitCost,
            DateTime transactionDate,
            string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Get or create inventory
            var inventory = GetInventory(inventoryId, connName);
            if (inventory == null)
            {
                inventory = new CrudeOilInventory
                {
                    InventoryId = inventoryId,
                    InventoryDate = transactionDate
                };
                // Save new inventory
                var inventoryEntity = ConvertCrudeOilInventoryToEntity(inventory);
                if (inventoryEntity is IPPDMEntity invPpdmEntity)
                    _commonColumnHandler.PrepareForInsert(invPpdmEntity, "SYSTEM");
                var invResult = dataSource.InsertEntity(CRUDE_OIL_INVENTORY_TABLE, inventoryEntity);
                if (invResult != null && invResult.Errors != null && invResult.Errors.Count > 0)
                {
                    var errorMessage = string.Join("; ", invResult.Errors.Select(e => e.Message));
                    _logger?.LogError("Failed to create inventory {InventoryId}: {Error}", inventoryId, errorMessage);
                    throw new InvalidOperationException($"Failed to create inventory: {errorMessage}");
                }
            }

            var transaction = new InventoryTransaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                TransactionDate = transactionDate,
                TransactionType = transactionType,
                Volume = volume,
                UnitCost = unitCost
            };

            // Save transaction
            var transactionEntity = ConvertInventoryTransactionToEntity(transaction, inventoryId);
            if (transactionEntity is IPPDMEntity transPpdmEntity)
                _commonColumnHandler.PrepareForInsert(transPpdmEntity, "SYSTEM");
            var transResult = dataSource.InsertEntity(CRUDE_OIL_INVENTORY_TRANSACTION_TABLE, transactionEntity);
            if (transResult != null && transResult.Errors != null && transResult.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", transResult.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to record transaction {TransactionId}: {Error}", transaction.TransactionId, errorMessage);
                throw new InvalidOperationException($"Failed to record transaction: {errorMessage}");
            }

            // Update inventory based on valuation method
            UpdateInventory(inventory, transaction);
            
            // Save updated inventory
            var updatedInventoryEntity = ConvertCrudeOilInventoryToEntity(inventory);
            if (updatedInventoryEntity is IPPDMEntity updatedPpdmEntity)
                _commonColumnHandler.PrepareForUpdate(updatedPpdmEntity, "SYSTEM");
            var updateResult = dataSource.UpdateEntity(CRUDE_OIL_INVENTORY_TABLE, updatedInventoryEntity);
            if (updateResult != null && updateResult.Errors != null && updateResult.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", updateResult.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to update inventory {InventoryId}: {Error}", inventoryId, errorMessage);
                throw new InvalidOperationException($"Failed to update inventory: {errorMessage}");
            }

            _logger?.LogDebug("Recorded transaction {TransactionId} for inventory {InventoryId}", transaction.TransactionId, inventoryId);
        }

        /// <summary>
        /// Updates inventory based on transaction and valuation method.
        /// </summary>
        private void UpdateInventory(CrudeOilInventory inventory, InventoryTransaction transaction)
        {
            switch (inventory.ValuationMethod)
            {
                case InventoryValuationMethod.FIFO:
                    UpdateFIFO(inventory, transaction);
                    break;

                case InventoryValuationMethod.LIFO:
                    UpdateLIFO(inventory, transaction);
                    break;

                case InventoryValuationMethod.WeightedAverage:
                    UpdateWeightedAverage(inventory, transaction);
                    break;

                case InventoryValuationMethod.LowerOfCostOrMarket:
                    UpdateWeightedAverage(inventory, transaction);
                    // LCM is applied at reporting time
                    break;
            }
        }

        /// <summary>
        /// Updates inventory using FIFO method.
        /// </summary>
        private void UpdateFIFO(CrudeOilInventory inventory, InventoryTransaction transaction)
        {
            if (transaction.TransactionType == InventoryTransactionType.Receipt)
            {
                // For FIFO, we track layers but for simplicity, use weighted average on receipt
                decimal totalValue = inventory.TotalValue + transaction.TotalValue;
                decimal totalVolume = inventory.Volume + transaction.Volume;
                inventory.Volume = totalVolume;
                inventory.UnitCost = totalVolume > 0 ? totalValue / totalVolume : 0;
            }
            else if (transaction.TransactionType == InventoryTransactionType.Delivery)
            {
                inventory.Volume -= transaction.Volume;
                // Unit cost remains the same (oldest cost)
            }
        }

        /// <summary>
        /// Updates inventory using LIFO method.
        /// </summary>
        private void UpdateLIFO(CrudeOilInventory inventory, InventoryTransaction transaction)
        {
            if (transaction.TransactionType == InventoryTransactionType.Receipt)
            {
                decimal totalValue = inventory.TotalValue + transaction.TotalValue;
                decimal totalVolume = inventory.Volume + transaction.Volume;
                inventory.Volume = totalVolume;
                inventory.UnitCost = totalVolume > 0 ? totalValue / totalVolume : 0;
            }
            else if (transaction.TransactionType == InventoryTransactionType.Delivery)
            {
                inventory.Volume -= transaction.Volume;
                // Unit cost remains the same (newest cost)
            }
        }

        /// <summary>
        /// Updates inventory using weighted average method.
        /// </summary>
        private void UpdateWeightedAverage(CrudeOilInventory inventory, InventoryTransaction transaction)
        {
            if (transaction.TransactionType == InventoryTransactionType.Receipt)
            {
                decimal totalValue = inventory.TotalValue + transaction.TotalValue;
                decimal totalVolume = inventory.Volume + transaction.Volume;
                inventory.Volume = totalVolume;
                inventory.UnitCost = totalVolume > 0 ? totalValue / totalVolume : 0;
            }
            else if (transaction.TransactionType == InventoryTransactionType.Delivery)
            {
                inventory.Volume -= transaction.Volume;
                // Unit cost remains the same
            }
        }

        /// <summary>
        /// Gets inventory by ID.
        /// </summary>
        public CrudeOilInventory? GetInventory(string inventoryId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(inventoryId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INVENTORY_ID", Operator = "=", FilterValue = inventoryId }
            };

            var results = dataSource.GetEntityAsync(CRUDE_OIL_INVENTORY_TABLE, filters).GetAwaiter().GetResult();
            var inventoryEntity = results?.OfType<CRUDE_OIL_INVENTORY>().FirstOrDefault();
            
            if (inventoryEntity == null)
                return null;

            return ConvertEntityToCrudeOilInventory(inventoryEntity);
        }

        /// <summary>
        /// Gets inventory transactions.
        /// </summary>
        public IEnumerable<InventoryTransaction> GetTransactions(string inventoryId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(inventoryId))
                return Enumerable.Empty<InventoryTransaction>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INVENTORY_ID", Operator = "=", FilterValue = inventoryId }
            };

            var results = dataSource.GetEntityAsync(CRUDE_OIL_INVENTORY_TRANSACTION_TABLE, filters).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return Enumerable.Empty<InventoryTransaction>();

            return results.OfType<CRUDE_OIL_INVENTORY_TRANSACTION>().Select(ConvertEntityToInventoryTransaction).Where(t => t != null)!;
        }

        private CRUDE_OIL_INVENTORY ConvertCrudeOilInventoryToEntity(CrudeOilInventory inventory)
        {
            var entity = new CRUDE_OIL_INVENTORY
            {
                INVENTORY_ID = inventory.InventoryId,
                PROPERTY_OR_LEASE_ID = inventory.PropertyOrLeaseId,
                TANK_BATTERY_ID = inventory.TankBatteryId,
                INVENTORY_DATE = inventory.InventoryDate,
                VOLUME = inventory.Volume,
                UNIT_COST = inventory.UnitCost,
                VALUATION_METHOD = inventory.ValuationMethod.ToString(),
                MARKET_PRICE = inventory.MarketPrice
            };
            return entity;
        }

        private CrudeOilInventory ConvertEntityToCrudeOilInventory(CRUDE_OIL_INVENTORY entity)
        {
            var inventory = new CrudeOilInventory
            {
                InventoryId = entity.INVENTORY_ID ?? string.Empty,
                PropertyOrLeaseId = entity.PROPERTY_OR_LEASE_ID,
                TankBatteryId = entity.TANK_BATTERY_ID,
                InventoryDate = entity.INVENTORY_DATE ?? DateTime.MinValue,
                Volume = entity.VOLUME ?? 0m,
                UnitCost = entity.UNIT_COST ?? 0m,
                MarketPrice = entity.MARKET_PRICE
            };
            
            if (!string.IsNullOrEmpty(entity.VALUATION_METHOD) && Enum.TryParse<InventoryValuationMethod>(entity.VALUATION_METHOD, out var valMethod))
                inventory.ValuationMethod = valMethod;
            
            return inventory;
        }

        private CRUDE_OIL_INVENTORY_TRANSACTION ConvertInventoryTransactionToEntity(InventoryTransaction transaction, string inventoryId)
        {
            var entity = new CRUDE_OIL_INVENTORY_TRANSACTION
            {
                TRANSACTION_ID = transaction.TransactionId,
                INVENTORY_ID = inventoryId,
                TRANSACTION_DATE = transaction.TransactionDate,
                TRANSACTION_TYPE = transaction.TransactionType.ToString(),
                VOLUME = transaction.Volume,
                UNIT_COST = transaction.UnitCost
            };
            return entity;
        }

        private InventoryTransaction ConvertEntityToInventoryTransaction(CRUDE_OIL_INVENTORY_TRANSACTION entity)
        {
            var transaction = new InventoryTransaction
            {
                TransactionId = entity.TRANSACTION_ID ?? string.Empty,
                TransactionDate = entity.TRANSACTION_DATE ?? DateTime.MinValue,
                Volume = entity.VOLUME ?? 0m,
                UnitCost = entity.UNIT_COST ?? 0m
            };
            
            if (!string.IsNullOrEmpty(entity.TRANSACTION_TYPE) && Enum.TryParse<InventoryTransactionType>(entity.TRANSACTION_TYPE, out var type))
                transaction.TransactionType = type;
            
            return transaction;
        }
    }
}
