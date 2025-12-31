using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionAccounting.Allocation
{
    /// <summary>
    /// Service for managing production allocation operations.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class AllocationService : IAllocationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<AllocationService>? _logger;
        private readonly string _connectionName;
        private const string ALLOCATION_RESULT_TABLE = "ALLOCATION_RESULT";
        private const string ALLOCATION_DETAIL_TABLE = "ALLOCATION_DETAIL";

        public AllocationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<AllocationService>? logger = null,
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
        /// Allocates production to wells.
        /// </summary>
        public async Task<ALLOCATION_RESULT> AllocateProductionAsync(
            string runTicketId,
            Beep.OilandGas.Models.DTOs.ProductionAccounting.AllocationMethod method,
            List<WellAllocationDataDto> wells,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(runTicketId))
                throw new ArgumentException("Run ticket ID cannot be null or empty.", nameof(runTicketId));

            // Get run ticket to get total volume
            var connName = connectionName ?? _connectionName;
            var runTicketRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(Beep.OilandGas.Models.Data.RUN_TICKET), connName, "RUN_TICKET",
                null);

            var runTicketFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "RUN_TICKET_ID", Operator = "=", FilterValue = runTicketId }
            };

            var runTickets = await runTicketRepo.GetAsync(runTicketFilters);
            var runTicket = runTickets.Cast<Beep.OilandGas.Models.Data.RUN_TICKET>().FirstOrDefault();

            if (runTicket == null)
                throw new InvalidOperationException($"Run ticket {runTicketId} not found.");
            if (wells == null || wells.Count == 0)
                throw new ArgumentException("Wells list cannot be null or empty.", nameof(wells));

            // Convert DTOs to models for AllocationEngine
            var wellData = wells.Select(w => new WellAllocationData
            {
                WellId = w.WellId,
                LeaseId = w.LeaseId,
                WorkingInterest = w.WorkingInterest,
                NetRevenueInterest = w.NetRevenueInterest,
                MeasuredProduction = w.MeasuredProduction,
                EstimatedProduction = w.EstimatedProduction
            }).ToList();

            // Get total volume from run ticket
            decimal totalVolume = runTicket.NET_VOLUME ?? runTicket.GROSS_VOLUME ?? 0;

            // Call AllocationEngine to perform calculation (convert DTO enum to local enum)
            var localMethod = (Beep.OilandGas.ProductionAccounting.Allocation.AllocationMethod)(int)method;
            var allocationResult = AllocationEngine.AllocateToWells(totalVolume, wellData, localMethod);

            // Convert to entity and save
            return await SaveAllocationResultAsync(allocationResult, runTicketId, userId, connectionName);
        }

        /// <summary>
        /// Allocates production to leases.
        /// </summary>
        public async Task<ALLOCATION_RESULT> AllocateToLeasesAsync(
            string runTicketId,
            Beep.OilandGas.Models.DTOs.ProductionAccounting.AllocationMethod method,
            List<LeaseAllocationDataDto> leases,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(runTicketId))
                throw new ArgumentException("Run ticket ID cannot be null or empty.", nameof(runTicketId));
            if (leases == null || leases.Count == 0)
                throw new ArgumentException("Leases list cannot be null or empty.", nameof(leases));

            // Get run ticket to get total volume
            var connName = connectionName ?? _connectionName;
            var runTicketRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(Beep.OilandGas.Models.Data.RUN_TICKET), connName, "RUN_TICKET",
                null);

            var runTicketFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "RUN_TICKET_ID", Operator = "=", FilterValue = runTicketId }
            };

            var runTickets = await runTicketRepo.GetAsync(runTicketFilters);
            var runTicket = runTickets.Cast<Beep.OilandGas.Models.Data.RUN_TICKET>().FirstOrDefault();

            if (runTicket == null)
                throw new InvalidOperationException($"Run ticket {runTicketId} not found.");

            // Convert DTOs to models for AllocationEngine
            var leaseData = leases.Select(l => new LeaseAllocationData
            {
                LeaseId = l.LeaseId,
                WorkingInterest = l.WorkingInterest,
                NetRevenueInterest = l.NetRevenueInterest,
                MeasuredProduction = l.MeasuredProduction
            }).ToList();

            // Get total volume from run ticket
            decimal totalVolume = runTicket.NET_VOLUME ?? runTicket.GROSS_VOLUME ?? 0;

            // Call AllocationEngine to perform calculation (convert DTO enum to local enum)
            var localMethod = (Beep.OilandGas.ProductionAccounting.Allocation.AllocationMethod)(int)method;
            var allocationResult = AllocationEngine.AllocateToLeases(totalVolume, leaseData, localMethod);

            // Convert to entity and save
            return await SaveAllocationResultAsync(allocationResult, runTicketId, userId, connectionName);
        }

        /// <summary>
        /// Allocates production to tracts.
        /// </summary>
        public async Task<ALLOCATION_RESULT> AllocateToTractsAsync(
            string runTicketId,
            Beep.OilandGas.Models.DTOs.ProductionAccounting.AllocationMethod method,
            List<TractAllocationDataDto> tracts,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(runTicketId))
                throw new ArgumentException("Run ticket ID cannot be null or empty.", nameof(runTicketId));
            if (tracts == null || tracts.Count == 0)
                throw new ArgumentException("Tracts list cannot be null or empty.", nameof(tracts));

            // Get run ticket to get total volume
            var connName = connectionName ?? _connectionName;
            var runTicketRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(Beep.OilandGas.Models.Data.RUN_TICKET), connName, "RUN_TICKET",
                null);

            var runTicketFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "RUN_TICKET_ID", Operator = "=", FilterValue = runTicketId }
            };

            var runTickets = await runTicketRepo.GetAsync(runTicketFilters);
            var runTicket = runTickets.Cast<Beep.OilandGas.Models.Data.RUN_TICKET>().FirstOrDefault();

            if (runTicket == null)
                throw new InvalidOperationException($"Run ticket {runTicketId} not found.");

            // Convert DTOs to models for AllocationEngine
            var tractData = tracts.Select(t => new TractAllocationData
            {
                TractId = t.TractId,
                UnitId = t.UnitId,
                TractParticipation = t.TractParticipation,
                WorkingInterest = t.WorkingInterest,
                NetRevenueInterest = t.NetRevenueInterest
            }).ToList();

            // Get total volume from run ticket
            decimal totalVolume = runTicket.NET_VOLUME ?? runTicket.GROSS_VOLUME ?? 0;

            // Call AllocationEngine to perform calculation (convert DTO enum to local enum)
            var localMethod = (Beep.OilandGas.ProductionAccounting.Allocation.AllocationMethod)(int)method;
            var allocationResult = AllocationEngine.AllocateToTracts(totalVolume, tractData, localMethod);

            // Convert to entity and save
            return await SaveAllocationResultAsync(allocationResult, runTicketId, userId, connectionName);
        }

        /// <summary>
        /// Gets an allocation result by ID.
        /// </summary>
        public async Task<ALLOCATION_RESULT?> GetAllocationResultAsync(string allocationId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(allocationId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ALLOCATION_RESULT), connName, ALLOCATION_RESULT_TABLE,
                null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ALLOCATION_RESULT_ID", Operator = "=", FilterValue = allocationId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<ALLOCATION_RESULT>().FirstOrDefault();
        }

        /// <summary>
        /// Gets allocation history for a run ticket.
        /// </summary>
        public async Task<List<ALLOCATION_RESULT>> GetAllocationHistoryAsync(string runTicketId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(runTicketId))
                return new List<ALLOCATION_RESULT>();

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ALLOCATION_RESULT), connName, ALLOCATION_RESULT_TABLE,
                null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "RUN_TICKET_ID", Operator = "=", FilterValue = runTicketId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<ALLOCATION_RESULT>().OrderByDescending(a => a.ALLOCATION_DATE).ToList();
        }

        /// <summary>
        /// Gets allocation details for an allocation result.
        /// </summary>
        public async Task<List<ALLOCATION_DETAIL>> GetAllocationDetailsAsync(string allocationId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(allocationId))
                return new List<ALLOCATION_DETAIL>();

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ALLOCATION_DETAIL), connName, ALLOCATION_DETAIL_TABLE,
                null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ALLOCATION_RESULT_ID", Operator = "=", FilterValue = allocationId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<ALLOCATION_DETAIL>().ToList();
        }

        /// <summary>
        /// Saves an allocation result and its details to the database.
        /// </summary>
        private async Task<ALLOCATION_RESULT> SaveAllocationResultAsync(
            AllocationResult allocationResult,
            string runTicketId,
            string userId,
            string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;

            // Create ALLOCATION_RESULT entity
            var allocationResultEntity = new ALLOCATION_RESULT
            {
                ALLOCATION_RESULT_ID = _defaults.FormatIdForTable(ALLOCATION_RESULT_TABLE, Guid.NewGuid().ToString()),
                RUN_TICKET_ID = runTicketId,
                ALLOCATION_DATE = allocationResult.AllocationDate,
                ALLOCATION_METHOD = allocationResult.Method.ToString(),
                TOTAL_VOLUME = allocationResult.TotalVolume,
                ALLOCATED_VOLUME = allocationResult.AllocatedVolume,
                ALLOCATION_VARIANCE = allocationResult.AllocationVariance,
                ACTIVE_IND = "Y"
            };

            // Prepare for insert
            if (allocationResultEntity is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);

            // Save allocation result
            var resultRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ALLOCATION_RESULT), connName, ALLOCATION_RESULT_TABLE,
                null);

            await resultRepo.InsertAsync(allocationResultEntity, userId);

            // Save allocation details
            var detailRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ALLOCATION_DETAIL), connName, ALLOCATION_DETAIL_TABLE,
                null);

            foreach (var detail in allocationResult.Details)
            {
                var detailEntity = new ALLOCATION_DETAIL
                {
                    ALLOCATION_DETAIL_ID = _defaults.FormatIdForTable(ALLOCATION_DETAIL_TABLE, Guid.NewGuid().ToString()),
                    ALLOCATION_RESULT_ID = allocationResultEntity.ALLOCATION_RESULT_ID,
                    ENTITY_ID = detail.EntityId,
                    ENTITY_NAME = detail.EntityName,
                    ENTITY_TYPE = "WELL",
                    ALLOCATED_VOLUME = detail.AllocatedVolume,
                    ALLOCATION_PERCENTAGE = detail.AllocationPercentage,
                    ALLOCATION_BASIS = detail.AllocationBasis,
                    ACTIVE_IND = "Y"
                };

                if (detailEntity is IPPDMEntity detailPpdmEntity)
                    _commonColumnHandler.PrepareForInsert(detailPpdmEntity, userId);

                await detailRepo.InsertAsync(detailEntity, userId);
            }

            _logger?.LogDebug("Saved allocation result {AllocationId} with {DetailCount} details",
                allocationResultEntity.ALLOCATION_RESULT_ID, allocationResult.Details.Count);

            return allocationResultEntity;
        }
    }
}
