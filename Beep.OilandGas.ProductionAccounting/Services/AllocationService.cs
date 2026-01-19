using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Allocation Service - Orchestrates production allocation operations.
    /// Allocates production volumes to wells, leases, tracts, and working interests.
    /// Per PPDM39 standards and industry accounting requirements.
    /// </summary>
    public class AllocationService : IAllocationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IAllocationEngine _allocationEngine;
        private readonly ILogger<AllocationService> _logger;
        private const string ConnectionName = "PPDM39";

        public AllocationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            IAllocationEngine allocationEngine,
            ILogger<AllocationService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _allocationEngine = allocationEngine ?? throw new ArgumentNullException(nameof(allocationEngine));
            _logger = logger;
        }

        /// <summary>
        /// Allocates production from a run ticket using specified method.
        /// Creates ALLOCATION_RESULT with detailed breakdown.
        /// </summary>
        public async Task<ALLOCATION_RESULT> AllocateAsync(
            RUN_TICKET runTicket,
            string method,
            string userId,
            string cn = "PPDM39")
        {
            if (runTicket == null)
                throw new AllocationException("Run ticket cannot be null");
            if (string.IsNullOrWhiteSpace(method))
                throw new AllocationException("Allocation method required");

            _logger?.LogInformation("Starting allocation for run ticket {RunTicketId} using method {Method}",
                runTicket.RUN_TICKET_ID, method);

            // Delegate to allocation engine for core calculation
            var normalizedMethod = AllocationMethods.AllMethods
                .FirstOrDefault(m => string.Equals(m, method, StringComparison.OrdinalIgnoreCase))
                ?? method;

            var allocationResult = await _allocationEngine.AllocateAsync(runTicket, normalizedMethod, userId, cn);

            _logger?.LogInformation("Allocation completed: {AllocationResultId}", allocationResult.ALLOCATION_RESULT_ID);
            return allocationResult;
        }

        /// <summary>
        /// Retrieves an allocation result by ID.
        /// </summary>
        public async Task<ALLOCATION_RESULT?> GetAsync(string allocationId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(allocationId))
                throw new ArgumentNullException(nameof(allocationId));

            var metadata = await _metadata.GetTableMetadataAsync("ALLOCATION_RESULT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ALLOCATION_RESULT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ALLOCATION_RESULT");

            var result = await repo.GetByIdAsync(allocationId);
            return result as ALLOCATION_RESULT;
        }

        /// <summary>
        /// Retrieves all details for an allocation result.
        /// </summary>
        public async Task<List<ALLOCATION_DETAIL>> GetDetailsAsync(string allocationId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(allocationId))
                throw new ArgumentNullException(nameof(allocationId));

            var metadata = await _metadata.GetTableMetadataAsync("ALLOCATION_DETAIL");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ALLOCATION_DETAIL);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ALLOCATION_DETAIL");

            // Filter by allocation result ID
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ALLOCATION_RESULT_ID", Operator = "=", FilterValue = allocationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var details = await repo.GetAsync(filters);
            return details?.Cast<ALLOCATION_DETAIL>().ToList() ?? new List<ALLOCATION_DETAIL>();
        }

        /// <summary>
        /// Gets allocation history for a run ticket (all previous allocations).
        /// </summary>
        public async Task<List<ALLOCATION_RESULT>> GetHistoryAsync(string runTicketId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(runTicketId))
                throw new ArgumentNullException(nameof(runTicketId));

            var metadata = await _metadata.GetTableMetadataAsync("ALLOCATION_RESULT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ALLOCATION_RESULT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ALLOCATION_RESULT");

            // Filter by run ticket ID
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ALLOCATION_REQUEST_ID", Operator = "=", FilterValue = runTicketId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<ALLOCATION_RESULT>().ToList() ?? new List<ALLOCATION_RESULT>();
        }

        /// <summary>
        /// Validates an allocation result.
        /// Checks: total volume match, percentage sum to 100%, all positive, etc.
        /// </summary>
        public async Task<bool> ValidateAsync(ALLOCATION_RESULT allocation, string cn = "PPDM39")
        {
            if (allocation == null)
                throw new ArgumentNullException(nameof(allocation));

            _logger?.LogInformation("Validating allocation {AllocationResultId}", allocation.ALLOCATION_RESULT_ID);

            try
            {
                // Validation 1: Total volume must be positive
                if (allocation.TOTAL_VOLUME == null || allocation.TOTAL_VOLUME <= 0)
                {
                    _logger?.LogWarning("Allocation {AllocationResultId}: Invalid total volume {Volume}",
                        allocation.ALLOCATION_RESULT_ID, allocation.TOTAL_VOLUME);
                    throw new AllocationException($"Invalid total volume: {allocation.TOTAL_VOLUME}");
                }

                // Validation 2: Allocated volume must not exceed total volume
                if (allocation.ALLOCATED_VOLUME.GetValueOrDefault(0) > allocation.TOTAL_VOLUME)
                {
                    _logger?.LogWarning("Allocation {AllocationResultId}: Allocated volume exceeds total",
                        allocation.ALLOCATION_RESULT_ID);
                    throw new AllocationException("Allocated volume cannot exceed total volume");
                }

                // Validation 3: Get details and validate
                var details = await GetDetailsAsync(allocation.ALLOCATION_RESULT_ID, cn);
                if (details.Count == 0)
                {
                    _logger?.LogWarning("Allocation {AllocationResultId}: No allocation details found",
                        allocation.ALLOCATION_RESULT_ID);
                    throw new AllocationException("Allocation has no details");
                }

                // Validation 4: All details must have positive volumes
                var invalidDetails = details.Where(d => d.ALLOCATED_VOLUME == null || d.ALLOCATED_VOLUME <= 0).ToList();
                if (invalidDetails.Any())
                {
                    _logger?.LogWarning("Allocation {AllocationResultId}: Found {Count} details with invalid volume",
                        allocation.ALLOCATION_RESULT_ID, invalidDetails.Count);
                    throw new AllocationException("All allocation details must have positive volumes");
                }

                // Validation 5: Sum of details should match allocated volume (within tolerance)
                var detailSum = details.Sum(d => d.ALLOCATED_VOLUME ?? 0);
                var tolerance = allocation.TOTAL_VOLUME * 0.0001m;  // 0.01% tolerance
                if (Math.Abs(detailSum - (allocation.ALLOCATED_VOLUME ?? 0)) > tolerance)
                {
                    _logger?.LogWarning("Allocation {AllocationResultId}: Detail sum {DetailSum} != allocated {Allocated}",
                        allocation.ALLOCATION_RESULT_ID, detailSum, allocation.ALLOCATED_VOLUME);
                    throw new AllocationException($"Detail volumes ({detailSum}) do not match allocated volume ({allocation.ALLOCATED_VOLUME})");
                }

                // Validation 6: If allocation method uses percentages, verify they sum to 100%
                if (allocation.ALLOCATION_METHOD == AllocationMethods.ProRata)
                {
                    var percentageDetails = details
                        .Where(d => d.ALLOCATION_PERCENTAGE.HasValue && d.ALLOCATION_PERCENTAGE > 0)
                        .ToList();

                    if (percentageDetails.Any())
                    {
                        decimal percentageSum = percentageDetails.Sum(d => d.ALLOCATION_PERCENTAGE ?? 0);

                        var pctTolerance = 0.01m;  // 0.01% tolerance
                        if (Math.Abs(percentageSum - 100) > pctTolerance)
                        {
                            _logger?.LogWarning("Allocation {AllocationResultId}: Percentages sum to {Sum}%, expected 100%",
                                allocation.ALLOCATION_RESULT_ID, percentageSum);
                            throw new AllocationException($"Allocation percentages must sum to 100% (got {percentageSum}%)");
                        }
                    }
                }

                _logger?.LogInformation("Allocation {AllocationResultId} validation passed",
                    allocation.ALLOCATION_RESULT_ID);
                return true;
            }
            catch (AllocationException ex)
            {
                _logger?.LogError(ex, "Allocation validation failed");
                throw;
            }
        }

        /// <summary>
        /// Reverses an allocation (sets to inactive) and logs the reversal.
        /// </summary>
        public async Task ReverseAsync(string allocationId, string userId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(allocationId))
                throw new ArgumentNullException(nameof(allocationId));

            _logger?.LogInformation("Reversing allocation {AllocationResultId}", allocationId);

            // Get the allocation
            var allocation = await GetAsync(allocationId, cn);
            if (allocation == null)
                throw new AllocationException($"Allocation {allocationId} not found");

            // Get metadata and repository
            var metadata = await _metadata.GetTableMetadataAsync("ALLOCATION_RESULT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ALLOCATION_RESULT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ALLOCATION_RESULT");

            // Soft delete the allocation (sets ACTIVE_IND to 'N')
            await repo.SoftDeleteAsync(allocationId, userId);

            // Also reverse all details
            var details = await GetDetailsAsync(allocationId, cn);
            if (details.Any())
            {
                var detailMetadata = await _metadata.GetTableMetadataAsync("ALLOCATION_DETAIL");
                var detailEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{detailMetadata.EntityTypeName}")
                    ?? typeof(ALLOCATION_DETAIL);

                var detailRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    detailEntityType, cn, "ALLOCATION_DETAIL");

                foreach (var detail in details)
                {
                    await detailRepo.SoftDeleteAsync(detail.ALLOCATION_DETAIL_ID, userId);
                }
            }

            _logger?.LogInformation("Allocation {AllocationResultId} reversed successfully", allocationId);
        }
    }
}
