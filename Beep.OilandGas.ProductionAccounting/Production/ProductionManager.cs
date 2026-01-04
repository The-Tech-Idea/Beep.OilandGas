using Beep.OilandGas.ProductionAccounting.Storage;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.ProductionAccounting.Production
{
    /// <summary>
    /// Manages production data, run tickets, and dispositions.
    /// Uses Entity classes directly with IDataSource - no dictionary conversions.
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
        public async Task<RUN_TICKET> CreateRunTicketAsync(
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

            var ticket = new RUN_TICKET
            {
                RUN_TICKET_ID = Guid.NewGuid().ToString(),
                RUN_TICKET_NUMBER = GenerateRunTicketNumber(),
                TICKET_DATE_TIME = measurement.MeasurementDateTime,
                LEASE_ID = leaseId,
                WELL_ID = wellId,
                TANK_BATTERY_ID = tankBatteryId,
                GROSS_VOLUME = measurement.GrossVolume,
                BSW_VOLUME = measurement.GrossVolume * (measurement.BSW / 100m),
                BSW_PERCENTAGE = measurement.BSW,
                NET_VOLUME = measurement.GrossVolume * (1 - measurement.BSW / 100m),
                TEMPERATURE = measurement.Temperature,
                API_GRAVITY = measurement.ApiGravity,
                DISPOSITION_TYPE = dispositionType.ToString(),
                PURCHASER = purchaser,
                MEASUREMENT_METHOD = measurement.Method?.ToString() ?? string.Empty,
                MEASUREMENT_RECORD_ID = measurement.MeasurementId
            };

            // Prepare for insert and save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForInsert(ticket, userId);
            var result = dataSource.InsertEntity(RUN_TICKET_TABLE, ticket);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create run ticket {TicketNumber}: {Error}", ticket.RUN_TICKET_NUMBER, errorMessage);
                throw new InvalidOperationException($"Failed to save run ticket: {errorMessage}");
            }

            _logger?.LogDebug("Created run ticket {TicketNumber} in database", ticket.RUN_TICKET_NUMBER);
            return ticket;
        }

        /// <summary>
        /// Creates a run ticket (synchronous wrapper).
        /// </summary>
        public RUN_TICKET CreateRunTicket(
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
        public async Task<RUN_TICKET?> GetRunTicketAsync(string runTicketNumber, string? connectionName = null)
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
            return results?.FirstOrDefault() as RUN_TICKET;
        }

        /// <summary>
        /// Gets a run ticket by number (synchronous wrapper).
        /// </summary>
        public RUN_TICKET? GetRunTicket(string runTicketNumber)
        {
            return GetRunTicketAsync(runTicketNumber).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets all run tickets for a lease.
        /// </summary>
        public async Task<IEnumerable<RUN_TICKET>> GetRunTicketsByLeaseAsync(string leaseId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(leaseId))
                return Enumerable.Empty<RUN_TICKET>();

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
                return Enumerable.Empty<RUN_TICKET>();

            return results.Cast<RUN_TICKET>().Where(t => t != null)!;
        }

        /// <summary>
        /// Gets all run tickets for a lease (synchronous wrapper).
        /// </summary>
        public IEnumerable<RUN_TICKET> GetRunTicketsByLease(string leaseId)
        {
            return GetRunTicketsByLeaseAsync(leaseId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets run tickets by date range.
        /// </summary>
        public async Task<IEnumerable<RUN_TICKET>> GetRunTicketsByDateRangeAsync(DateTime startDate, DateTime endDate, string? connectionName = null)
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
                return Enumerable.Empty<RUN_TICKET>();

            return results.Cast<RUN_TICKET>()
                .Where(t => t != null && t.TICKET_DATE_TIME >= startDate && t.TICKET_DATE_TIME <= endDate)!;
        }

        /// <summary>
        /// Gets run tickets by date range (synchronous wrapper).
        /// </summary>
        public IEnumerable<RUN_TICKET> GetRunTicketsByDateRange(DateTime startDate, DateTime endDate)
        {
            return GetRunTicketsByDateRangeAsync(startDate, endDate).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a tank inventory record.
        /// </summary>
        public async Task<TANK_INVENTORY> CreateTankInventoryAsync(
            string tankBatteryId,
            DateTime inventoryDate,
            decimal openingInventory,
            decimal receipts,
            decimal deliveries,
            decimal? actualClosingInventory = null,
            string userId = "system",
            string? connectionName = null)
        {
            var inventory = new TANK_INVENTORY
            {
                TANK_INVENTORY_ID = Guid.NewGuid().ToString(),
                INVENTORY_DATE = inventoryDate,
                TANK_BATTERY_ID = tankBatteryId,
                OPENING_INVENTORY = openingInventory,
                RECEIPTS = receipts,
                DELIVERIES = deliveries,
                ACTUAL_CLOSING_INVENTORY = actualClosingInventory
            };

            // Prepare for insert and save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForInsert(inventory, userId);
            var result = dataSource.InsertEntity(TANK_INVENTORY_TABLE, inventory);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create tank inventory {InventoryId}: {Error}", inventory.TANK_INVENTORY_ID, errorMessage);
                throw new InvalidOperationException($"Failed to save tank inventory: {errorMessage}");
            }

            _logger?.LogDebug("Created tank inventory {InventoryId} in database", inventory.TANK_INVENTORY_ID);
            return inventory;
        }

        /// <summary>
        /// Creates a tank inventory record (synchronous wrapper).
        /// </summary>
        public TANK_INVENTORY CreateTankInventory(
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
        public async Task<TANK_INVENTORY?> GetTankInventoryAsync(string inventoryId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(inventoryId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "TANK_INVENTORY_ID", Operator = "=", FilterValue = inventoryId }
            };

            var results = await dataSource.GetEntityAsync(TANK_INVENTORY_TABLE, filters);
            return results?.FirstOrDefault() as TANK_INVENTORY;
        }

        /// <summary>
        /// Gets tank inventory by ID (synchronous wrapper).
        /// </summary>
        public TANK_INVENTORY? GetTankInventory(string inventoryId)
        {
            return GetTankInventoryAsync(inventoryId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Calculates total production for a lease in a date range.
        /// </summary>
        public decimal CalculateTotalProduction(string leaseId, DateTime startDate, DateTime endDate)
        {
            return GetRunTicketsByLease(leaseId)
                .Where(t => t.TICKET_DATE_TIME >= startDate && t.TICKET_DATE_TIME <= endDate)
                .Sum(t => t.NET_VOLUME ?? 0m);
        }

        /// <summary>
        /// Calculates total dispositions by type.
        /// </summary>
        public Dictionary<DispositionType, decimal> CalculateDispositionsByType(DateTime startDate, DateTime endDate)
        {
            var tickets = GetRunTicketsByDateRange(startDate, endDate);
            return tickets
                .Where(t => !string.IsNullOrEmpty(t.DISPOSITION_TYPE) && t.NET_VOLUME.HasValue)
                .GroupBy(t => Enum.TryParse<DispositionType>(t.DISPOSITION_TYPE, out var dt) ? dt : DispositionType.Sale)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.NET_VOLUME!.Value));
        }

        /// <summary>
        /// Generates a unique run ticket number.
        /// </summary>
        private string GenerateRunTicketNumber()
        {
            return $"RT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }
    }
}
