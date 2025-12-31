using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionAccounting.Production
{
    /// <summary>
    /// Service for managing production data, run tickets, and dispositions.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class ProductionService : IProductionService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<ProductionService>? _logger;
        private readonly string _connectionName;
        private const string RUN_TICKET_TABLE = "RUN_TICKET";
        private const string TANK_INVENTORY_TABLE = "TANK_INVENTORY";

        public ProductionService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<ProductionService>? logger = null,
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
        /// Creates a run ticket.
        /// </summary>
        public async Task<RUN_TICKET> CreateRunTicketAsync(
            CreateRunTicketRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.LeaseId))
                throw new ArgumentException("Lease ID cannot be null or empty.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var runTicketNumber = GenerateRunTicketNumber();

            // Calculate volumes
            decimal bswVolume = request.GrossVolume * (request.BSWPercentage / 100m);
            decimal netVolume = request.GrossVolume - bswVolume;

            // Create entity
            var runTicket = new RUN_TICKET
            {
                RUN_TICKET_ID = _defaults.FormatIdForTable(RUN_TICKET_TABLE, Guid.NewGuid().ToString()),
                RUN_TICKET_NUMBER = runTicketNumber,
                TICKET_DATE_TIME = request.TicketDateTime ?? DateTime.UtcNow,
                LEASE_ID = request.LeaseId,
                WELL_ID = request.WellId,
                TANK_BATTERY_ID = request.TankBatteryId,
                GROSS_VOLUME = request.GrossVolume,
                BSW_VOLUME = bswVolume,
                BSW_PERCENTAGE = request.BSWPercentage,
                NET_VOLUME = netVolume,
                TEMPERATURE = request.Temperature,
                API_GRAVITY = request.ApiGravity,
                DISPOSITION_TYPE = request.DispositionType.ToString(),
                PURCHASER = request.Purchaser,
                MEASUREMENT_METHOD = request.MeasurementMethod ?? string.Empty,
                ACTIVE_IND = "Y"
            };

            // Prepare for insert
            if (runTicket is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);

            // Save to database
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(RUN_TICKET), connName, RUN_TICKET_TABLE,
                null);

            await repo.InsertAsync(runTicket, userId);
            _logger?.LogDebug("Created run ticket {TicketNumber} in database", runTicketNumber);

            return runTicket;
        }

        /// <summary>
        /// Gets a run ticket by number.
        /// </summary>
        public async Task<RUN_TICKET?> GetRunTicketAsync(string runTicketNumber, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(runTicketNumber))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(RUN_TICKET), connName, RUN_TICKET_TABLE,
                null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "RUN_TICKET_NUMBER", Operator = "=", FilterValue = runTicketNumber }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<RUN_TICKET>().FirstOrDefault();
        }

        /// <summary>
        /// Gets all run tickets for a lease.
        /// </summary>
        public async Task<List<RUN_TICKET>> GetRunTicketsByLeaseAsync(string leaseId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(leaseId))
                return new List<RUN_TICKET>();

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(RUN_TICKET), connName, RUN_TICKET_TABLE,
                null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<RUN_TICKET>().ToList();
        }

        /// <summary>
        /// Gets run tickets by date range.
        /// </summary>
        public async Task<List<RUN_TICKET>> GetRunTicketsByDateRangeAsync(DateTime startDate, DateTime endDate, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(RUN_TICKET), connName, RUN_TICKET_TABLE,
                null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "TICKET_DATE_TIME", Operator = ">=", FilterValue = startDate },
                new AppFilter { FieldName = "TICKET_DATE_TIME", Operator = "<=", FilterValue = endDate }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<RUN_TICKET>()
                .Where(t => t.TICKET_DATE_TIME.HasValue &&
                            t.TICKET_DATE_TIME.Value >= startDate &&
                            t.TICKET_DATE_TIME.Value <= endDate)
                .ToList();
        }

        /// <summary>
        /// Creates a tank inventory record.
        /// </summary>
        public async Task<TANK_INVENTORY> CreateTankInventoryAsync(
            CreateTankInventoryRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.TankBatteryId))
                throw new ArgumentException("Tank battery ID cannot be null or empty.", nameof(request));

            var connName = connectionName ?? _connectionName;

            // Create entity
            var inventory = new TANK_INVENTORY
            {
                TANK_INVENTORY_ID = _defaults.FormatIdForTable(TANK_INVENTORY_TABLE, Guid.NewGuid().ToString()),
                INVENTORY_DATE = request.InventoryDate,
                TANK_BATTERY_ID = request.TankBatteryId,
                OPENING_INVENTORY = request.OpeningInventory,
                RECEIPTS = request.Receipts,
                DELIVERIES = request.Deliveries,
                ADJUSTMENTS = request.Adjustments,
                SHRINKAGE = request.Shrinkage,
                THEFT_LOSS = request.TheftLoss,
                ACTUAL_CLOSING_INVENTORY = request.ActualClosingInventory,
                ACTIVE_IND = "Y"
            };

            // Prepare for insert
            if (inventory is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);

            // Save to database
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(TANK_INVENTORY), connName, TANK_INVENTORY_TABLE,
                null);

            await repo.InsertAsync(inventory, userId);
            _logger?.LogDebug("Created tank inventory {InventoryId} in database", inventory.TANK_INVENTORY_ID);

            return inventory;
        }

        /// <summary>
        /// Gets tank inventory by ID.
        /// </summary>
        public async Task<TANK_INVENTORY?> GetTankInventoryAsync(string inventoryId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(inventoryId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(TANK_INVENTORY), connName, TANK_INVENTORY_TABLE,
                null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "TANK_INVENTORY_ID", Operator = "=", FilterValue = inventoryId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<TANK_INVENTORY>().FirstOrDefault();
        }

        /// <summary>
        /// Calculates total production for a lease in a date range.
        /// </summary>
        public async Task<decimal> CalculateTotalProductionAsync(string leaseId, DateTime startDate, DateTime endDate, string? connectionName = null)
        {
            var tickets = await GetRunTicketsByLeaseAsync(leaseId, connectionName);
            return tickets
                .Where(t => t.TICKET_DATE_TIME.HasValue &&
                            t.TICKET_DATE_TIME.Value >= startDate &&
                            t.TICKET_DATE_TIME.Value <= endDate &&
                            t.NET_VOLUME.HasValue)
                .Sum(t => t.NET_VOLUME!.Value);
        }

        /// <summary>
        /// Calculates dispositions by type.
        /// </summary>
        public async Task<Dictionary<DispositionType, decimal>> CalculateDispositionsByTypeAsync(DateTime startDate, DateTime endDate, string? connectionName = null)
        {
            var tickets = await GetRunTicketsByDateRangeAsync(startDate, endDate, connectionName);
            return tickets
                .Where(t => t.NET_VOLUME.HasValue && !string.IsNullOrEmpty(t.DISPOSITION_TYPE))
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
