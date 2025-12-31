
using Beep.OilandGas.ProductionAccounting.Storage;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.ProductionAccounting.Production
{
    /// <summary>
    /// Manages production data, run tickets, and dispositions.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class ProductionManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<ProductionManager>? _logger;
        private readonly string _connectionName;
        private const string RUN_TICKET_TABLE = "RUN_TICKET";
        private const string TANK_INVENTORY_TABLE = "TANK_INVENTORY";

        public ProductionManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<ProductionManager>? logger = null,
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
        /// Creates a run ticket from a measurement record.
        /// </summary>
        public async Task<RunTicket> CreateRunTicketAsync(
            string leaseId,
            string? wellId,
            string? tankBatteryId,
            Measurement.MeasurementRecord measurement,
            DispositionType dispositionType,
            string purchaser,
            string userId = "system",
            string? connectionName = null)
        {
            if (measurement == null)
                throw new ArgumentNullException(nameof(measurement));

            if (string.IsNullOrEmpty(leaseId))
                throw new ArgumentException("Lease ID cannot be null or empty.", nameof(leaseId));

            var ticket = new RunTicket
            {
                RunTicketNumber = GenerateRunTicketNumber(),
                TicketDateTime = measurement.MeasurementDateTime,
                LeaseId = leaseId,
                WellId = wellId,
                TankBatteryId = tankBatteryId,
                GrossVolume = measurement.GrossVolume,
                BSWVolume = measurement.GrossVolume * (measurement.BSW / 100m),
                BSWPercentage = measurement.BSW,
                Temperature = measurement.Temperature,
                ApiGravity = measurement.ApiGravity,
                Properties = measurement.Properties,
                DispositionType = dispositionType,
                Purchaser = purchaser,
                MeasurementMethod = measurement.Method,
                MeasurementRecordId = measurement.MeasurementId
            };

            // Save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var ticketData = ConvertRunTicketToDictionary(ticket);
            var result = dataSource.InsertEntity(RUN_TICKET_TABLE, ticketData);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create run ticket {TicketNumber}: {Error}", ticket.RunTicketNumber, errorMessage);
                throw new InvalidOperationException($"Failed to save run ticket: {errorMessage}");
            }

            _logger?.LogDebug("Created run ticket {TicketNumber} in database", ticket.RunTicketNumber);
            return ticket;
        }

        /// <summary>
        /// Creates a run ticket (synchronous wrapper).
        /// </summary>
        public RunTicket CreateRunTicket(
            string leaseId,
            string? wellId,
            string? tankBatteryId,
            Measurement.MeasurementRecord measurement,
            DispositionType dispositionType,
            string purchaser)
        {
            return CreateRunTicketAsync(leaseId, wellId, tankBatteryId, measurement, dispositionType, purchaser).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a run ticket by number.
        /// </summary>
        public async Task<RunTicket?> GetRunTicketAsync(string runTicketNumber, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(runTicketNumber))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "RUN_TICKET_NUMBER", Operator = "=", FilterValue = runTicketNumber }
            };

            var results = await dataSource.GetEntityAsync(RUN_TICKET_TABLE, filters);
            var ticketData = results?.FirstOrDefault();
            
            if (ticketData == null)
                return null;

            return ticketData as RunTicket;
        }

        /// <summary>
        /// Gets a run ticket by number (synchronous wrapper).
        /// </summary>
        public RunTicket? GetRunTicket(string runTicketNumber)
        {
            return GetRunTicketAsync(runTicketNumber).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets all run tickets for a lease.
        /// </summary>
        public async Task<IEnumerable<RunTicket>> GetRunTicketsByLeaseAsync(string leaseId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(leaseId))
                return Enumerable.Empty<RunTicket>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId }
            };

            var results = await dataSource.GetEntityAsync(RUN_TICKET_TABLE, filters);
            if (results == null || !results.Any())
                return Enumerable.Empty<RunTicket>();

            return results.Cast<RunTicket>().Where(t => t != null)!;
        }

        /// <summary>
        /// Gets all run tickets for a lease (synchronous wrapper).
        /// </summary>
        public IEnumerable<RunTicket> GetRunTicketsByLease(string leaseId)
        {
            return GetRunTicketsByLeaseAsync(leaseId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets run tickets by date range.
        /// </summary>
        public async Task<IEnumerable<RunTicket>> GetRunTicketsByDateRangeAsync(DateTime startDate, DateTime endDate, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "TICKET_DATE_TIME", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd HH:mm:ss") },
                new AppFilter { FieldName = "TICKET_DATE_TIME", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd HH:mm:ss")}
            };

            var results = await dataSource.GetEntityAsync(RUN_TICKET_TABLE, filters);
            if (results == null || !results.Any())
                return Enumerable.Empty<RunTicket>();

            return results.Cast<RunTicket>()
                .Where(t => t != null && t.TicketDateTime >= startDate && t.TicketDateTime <= endDate)!;
        }

        /// <summary>
        /// Gets run tickets by date range (synchronous wrapper).
        /// </summary>
        public IEnumerable<RunTicket> GetRunTicketsByDateRange(DateTime startDate, DateTime endDate)
        {
            return GetRunTicketsByDateRangeAsync(startDate, endDate).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a tank inventory record.
        /// </summary>
        public async Task<TankInventory> CreateTankInventoryAsync(
            string tankBatteryId,
            DateTime inventoryDate,
            decimal openingInventory,
            decimal receipts,
            decimal deliveries,
            decimal? actualClosingInventory = null,
            string userId = "system",
            string? connectionName = null)
        {
            var inventory = new TankInventory
            {
                InventoryId = Guid.NewGuid().ToString(),
                InventoryDate = inventoryDate,
                TankBatteryId = tankBatteryId,
                OpeningInventory = openingInventory,
                Receipts = receipts,
                Deliveries = deliveries,
                ActualClosingInventory = actualClosingInventory
            };

            // Save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var inventoryData = ConvertTankInventoryToDictionary(inventory);
            var result = dataSource.InsertEntity(TANK_INVENTORY_TABLE, inventoryData);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create tank inventory {InventoryId}: {Error}", inventory.InventoryId, errorMessage);
                throw new InvalidOperationException($"Failed to save tank inventory: {errorMessage}");
            }

            _logger?.LogDebug("Created tank inventory {InventoryId} in database", inventory.InventoryId);
            return inventory;
        }

        /// <summary>
        /// Creates a tank inventory record (synchronous wrapper).
        /// </summary>
        public TankInventory CreateTankInventory(
            string tankBatteryId,
            DateTime inventoryDate,
            decimal openingInventory,
            decimal receipts,
            decimal deliveries,
            decimal? actualClosingInventory = null)
        {
            return CreateTankInventoryAsync(tankBatteryId, inventoryDate, openingInventory, receipts, deliveries, actualClosingInventory).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets tank inventory by ID.
        /// </summary>
        public async Task<TankInventory?> GetTankInventoryAsync(string inventoryId, string? connectionName = null)
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

            var results = await dataSource.GetEntityAsync(TANK_INVENTORY_TABLE, filters);
            var inventoryData = results?.FirstOrDefault();
            
            if (inventoryData == null)
                return null;

            return inventoryData as TankInventory;
        }

        /// <summary>
        /// Gets tank inventory by ID (synchronous wrapper).
        /// </summary>
        public TankInventory? GetTankInventory(string inventoryId)
        {
            return GetTankInventoryAsync(inventoryId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Calculates total production for a lease in a date range.
        /// </summary>
        public decimal CalculateTotalProduction(string leaseId, DateTime startDate, DateTime endDate)
        {
            return GetRunTicketsByLease(leaseId)
                .Where(t => t.TicketDateTime >= startDate && t.TicketDateTime <= endDate)
                .Sum(t => t.NetVolume);
        }

        /// <summary>
        /// Calculates total dispositions by type.
        /// </summary>
        public Dictionary<DispositionType, decimal> CalculateDispositionsByType(DateTime startDate, DateTime endDate)
        {
            var tickets = GetRunTicketsByDateRange(startDate, endDate);
            return tickets
                .GroupBy(t => t.DispositionType)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.NetVolume));
        }

        /// <summary>
        /// Generates a unique run ticket number.
        /// </summary>
        private string GenerateRunTicketNumber()
        {
            return $"RT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        #region Helper Methods - Model to Dictionary Conversion

        /// <summary>
        /// Converts RunTicket to dictionary for database storage.
        /// </summary>
        private Dictionary<string, object> ConvertRunTicketToDictionary(RunTicket ticket)
        {
            return new Dictionary<string, object>
            {
                { "RUN_TICKET_NUMBER", ticket.RunTicketNumber },
                { "TICKET_DATE_TIME", ticket.TicketDateTime },
                { "LEASE_ID", ticket.LeaseId },
                { "WELL_ID", ticket.WellId ?? (object)DBNull.Value },
                { "TANK_BATTERY_ID", ticket.TankBatteryId ?? (object)DBNull.Value },
                { "GROSS_VOLUME", ticket.GrossVolume },
                { "BSW_VOLUME", ticket.BSWVolume },
                { "BSW_PERCENTAGE", ticket.BSWPercentage },
                { "NET_VOLUME", ticket.NetVolume },
                { "TEMPERATURE", ticket.Temperature },
                { "API_GRAVITY", ticket.ApiGravity ?? (object)DBNull.Value },
                { "DISPOSITION_TYPE", ticket.DispositionType.ToString() },
                { "PURCHASER", ticket.Purchaser ?? string.Empty },
                { "PRICE_PER_BARREL", ticket.PricePerBarrel ?? (object)DBNull.Value },
                { "MEASUREMENT_METHOD", ticket.MeasurementMethod.ToString() },
                { "MEASUREMENT_RECORD_ID", ticket.MeasurementRecordId ?? (object)DBNull.Value },
                { "NOTES", ticket.Notes ?? string.Empty },
                { "IS_PROCESSED", ticket.IsProcessed },
                { "PROCESSED_DATE", ticket.ProcessedDate ?? (object)DBNull.Value }
            };
        }

        /// <summary>
        /// Converts dictionary to RunTicket.
        /// </summary>
        private RunTicket? ConvertDictionaryToRunTicket(Dictionary<string, object> dict)
        {
            if (dict == null || !dict.ContainsKey("RUN_TICKET_NUMBER"))
                return null;

            var ticket = new RunTicket
            {
                RunTicketNumber = dict["RUN_TICKET_NUMBER"]?.ToString() ?? string.Empty,
                TicketDateTime = dict.ContainsKey("TICKET_DATE_TIME") && dict["TICKET_DATE_TIME"] != DBNull.Value
                    ? Convert.ToDateTime(dict["TICKET_DATE_TIME"])
                    : DateTime.MinValue,
                LeaseId = dict.ContainsKey("LEASE_ID") ? dict["LEASE_ID"]?.ToString() ?? string.Empty : string.Empty,
                WellId = dict.ContainsKey("WELL_ID") && dict["WELL_ID"] != DBNull.Value ? dict["WELL_ID"]?.ToString() : null,
                TankBatteryId = dict.ContainsKey("TANK_BATTERY_ID") && dict["TANK_BATTERY_ID"] != DBNull.Value ? dict["TANK_BATTERY_ID"]?.ToString() : null,
                GrossVolume = dict.ContainsKey("GROSS_VOLUME") ? Convert.ToDecimal(dict["GROSS_VOLUME"]) : 0m,
                BSWVolume = dict.ContainsKey("BSW_VOLUME") ? Convert.ToDecimal(dict["BSW_VOLUME"]) : 0m,
                BSWPercentage = dict.ContainsKey("BSW_PERCENTAGE") ? Convert.ToDecimal(dict["BSW_PERCENTAGE"]) : 0m,
                Temperature = dict.ContainsKey("TEMPERATURE") ? Convert.ToDecimal(dict["TEMPERATURE"]) : 60m,
                ApiGravity = dict.ContainsKey("API_GRAVITY") && dict["API_GRAVITY"] != DBNull.Value ? Convert.ToDecimal(dict["API_GRAVITY"]) : null,
                Purchaser = dict.ContainsKey("PURCHASER") ? dict["PURCHASER"]?.ToString() ?? string.Empty : string.Empty,
                PricePerBarrel = dict.ContainsKey("PRICE_PER_BARREL") && dict["PRICE_PER_BARREL"] != DBNull.Value ? Convert.ToDecimal(dict["PRICE_PER_BARREL"]) : null,
                MeasurementRecordId = dict.ContainsKey("MEASUREMENT_RECORD_ID") && dict["MEASUREMENT_RECORD_ID"] != DBNull.Value ? dict["MEASUREMENT_RECORD_ID"]?.ToString() : null,
                Notes = dict.ContainsKey("NOTES") ? dict["NOTES"]?.ToString() : null,
                IsProcessed = dict.ContainsKey("IS_PROCESSED") && Convert.ToBoolean(dict["IS_PROCESSED"]),
                ProcessedDate = dict.ContainsKey("PROCESSED_DATE") && dict["PROCESSED_DATE"] != DBNull.Value ? Convert.ToDateTime(dict["PROCESSED_DATE"]) : null
            };

            if (dict.ContainsKey("DISPOSITION_TYPE") && Enum.TryParse<DispositionType>(dict["DISPOSITION_TYPE"]?.ToString(), out var dispositionType))
                ticket.DispositionType = dispositionType;
            if (dict.ContainsKey("MEASUREMENT_METHOD") && Enum.TryParse<Measurement.MeasurementMethod>(dict["MEASUREMENT_METHOD"]?.ToString(), out var method))
                ticket.MeasurementMethod = method;

            return ticket;
        }

        /// <summary>
        /// Converts TankInventory to dictionary for database storage.
        /// </summary>
        private Dictionary<string, object> ConvertTankInventoryToDictionary(TankInventory inventory)
        {
            return new Dictionary<string, object>
            {
                { "INVENTORY_ID", inventory.InventoryId },
                { "INVENTORY_DATE", inventory.InventoryDate },
                { "TANK_BATTERY_ID", inventory.TankBatteryId },
                { "OPENING_INVENTORY", inventory.OpeningInventory },
                { "RECEIPTS", inventory.Receipts },
                { "DELIVERIES", inventory.Deliveries },
                { "ADJUSTMENTS", inventory.Adjustments },
                { "SHRINKAGE", inventory.Shrinkage },
                { "THEFT_LOSS", inventory.TheftLoss },
                { "ACTUAL_CLOSING_INVENTORY", inventory.ActualClosingInventory ?? (object)DBNull.Value }
            };
        }

        /// <summary>
        /// Converts dictionary to TankInventory.
        /// </summary>
        private TankInventory? ConvertDictionaryToTankInventory(Dictionary<string, object> dict)
        {
            if (dict == null || !dict.ContainsKey("INVENTORY_ID"))
                return null;

            return new TankInventory
            {
                InventoryId = dict["INVENTORY_ID"]?.ToString() ?? string.Empty,
                InventoryDate = dict.ContainsKey("INVENTORY_DATE") && dict["INVENTORY_DATE"] != DBNull.Value
                    ? Convert.ToDateTime(dict["INVENTORY_DATE"])
                    : DateTime.MinValue,
                TankBatteryId = dict.ContainsKey("TANK_BATTERY_ID") ? dict["TANK_BATTERY_ID"]?.ToString() ?? string.Empty : string.Empty,
                OpeningInventory = dict.ContainsKey("OPENING_INVENTORY") ? Convert.ToDecimal(dict["OPENING_INVENTORY"]) : 0m,
                Receipts = dict.ContainsKey("RECEIPTS") ? Convert.ToDecimal(dict["RECEIPTS"]) : 0m,
                Deliveries = dict.ContainsKey("DELIVERIES") ? Convert.ToDecimal(dict["DELIVERIES"]) : 0m,
                Adjustments = dict.ContainsKey("ADJUSTMENTS") ? Convert.ToDecimal(dict["ADJUSTMENTS"]) : 0m,
                Shrinkage = dict.ContainsKey("SHRINKAGE") ? Convert.ToDecimal(dict["SHRINKAGE"]) : 0m,
                TheftLoss = dict.ContainsKey("THEFT_LOSS") ? Convert.ToDecimal(dict["THEFT_LOSS"]) : 0m,
                ActualClosingInventory = dict.ContainsKey("ACTUAL_CLOSING_INVENTORY") && dict["ACTUAL_CLOSING_INVENTORY"] != DBNull.Value
                    ? Convert.ToDecimal(dict["ACTUAL_CLOSING_INVENTORY"])
                    : null
            };
        }

        #endregion
    }
}
