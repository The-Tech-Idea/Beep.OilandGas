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
    /// Revenue Service - Recognizes revenue per ASC 606.
    /// Allocates revenue to interest owners based on their ownership percentages.
    /// </summary>
    public class RevenueService : IRevenueService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<RevenueService> _logger;
        private const string ConnectionName = "PPDM39";

        public RevenueService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<RevenueService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        /// <summary>
        /// Recognizes revenue from an allocation detail.
        /// Allocates revenue to interest owners based on their ownership percentages.
        /// </summary>
        public async Task<REVENUE_ALLOCATION> RecognizeRevenueAsync(
            ALLOCATION_DETAIL allocation,
            string userId,
            string cn = "PPDM39")
        {
            if (allocation == null)
                throw new ArgumentNullException(nameof(allocation));

            _logger?.LogInformation("Recognizing revenue for allocation detail {DetailId}", allocation.ALLOCATION_DETAIL_ID);

            try
            {
                // Create revenue allocation record
                // In real implementation: would calculate revenue from volume × price
                var revenueAllocation = new REVENUE_ALLOCATION
                {
                    REVENUE_ALLOCATION_ID = Guid.NewGuid().ToString(),
                    INTEREST_OWNER_BA_ID = allocation.ENTITY_ID,
                    INTEREST_PERCENTAGE = allocation.ALLOCATION_PERCENTAGE ?? 100,
                    ALLOCATED_AMOUNT = 0,  // Would calculate: Volume × Price × Interest %
                    ALLOCATION_METHOD = "PRO-RATA",
                    DESCRIPTION = $"Revenue allocation for {allocation.ENTITY_NAME}",
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    ROW_CREATED_BY = userId
                };

                // Save to database
                var metadata = await _metadata.GetTableMetadataAsync("REVENUE_ALLOCATION");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(REVENUE_ALLOCATION);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "REVENUE_ALLOCATION");

                await repo.InsertAsync(revenueAllocation, userId);

                _logger?.LogInformation(
                    "Revenue allocation created: {AllocationId} for entity {EntityId}",
                    revenueAllocation.REVENUE_ALLOCATION_ID, allocation.ENTITY_ID);

                return revenueAllocation;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recognizing revenue");
                throw;
            }
        }

        /// <summary>
        /// Validates a revenue allocation record.
        /// Checks: amount is positive, interest percentage valid, etc.
        /// </summary>
        public async Task<bool> ValidateAsync(REVENUE_ALLOCATION allocation, string cn = "PPDM39")
        {
            if (allocation == null)
                throw new ArgumentNullException(nameof(allocation));

            _logger?.LogInformation("Validating revenue allocation {AllocationId}", allocation.REVENUE_ALLOCATION_ID);

            try
            {
                // Validation 1: Interest percentage should be between 0 and 100
                if (allocation.INTEREST_PERCENTAGE.HasValue)
                {
                    if (allocation.INTEREST_PERCENTAGE < 0 || allocation.INTEREST_PERCENTAGE > 100)
                    {
                        _logger?.LogWarning(
                            "Revenue allocation {AllocationId}: Invalid interest percentage {Percentage}",
                            allocation.REVENUE_ALLOCATION_ID, allocation.INTEREST_PERCENTAGE);
                        throw new AccountingException(
                            $"Interest percentage must be between 0 and 100: {allocation.INTEREST_PERCENTAGE}");
                    }
                }

                // Validation 2: Allocated amount should be non-negative
                if (allocation.ALLOCATED_AMOUNT.HasValue && allocation.ALLOCATED_AMOUNT < 0)
                {
                    _logger?.LogWarning(
                        "Revenue allocation {AllocationId}: Negative allocated amount {Amount}",
                        allocation.REVENUE_ALLOCATION_ID, allocation.ALLOCATED_AMOUNT);
                    throw new AccountingException(
                        $"Allocated amount cannot be negative: {allocation.ALLOCATED_AMOUNT}");
                }

                // Validation 3: Interest owner ID should be set
                if (string.IsNullOrWhiteSpace(allocation.INTEREST_OWNER_BA_ID))
                {
                    _logger?.LogWarning("Revenue allocation {AllocationId}: Interest owner ID is required",
                        allocation.REVENUE_ALLOCATION_ID);
                    throw new AccountingException("Interest owner ID is required");
                }

                _logger?.LogInformation("Revenue allocation {AllocationId} validation passed",
                    allocation.REVENUE_ALLOCATION_ID);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Revenue allocation validation failed");
                throw;
            }
        }
    }

    /// <summary>
    /// Revenue status constants per ASC 606.
    /// </summary>
    public static class RevenueStatus
    {
        public const string Deferred = "DEFERRED";
        public const string Recognized = "RECOGNIZED";
        public const string Billed = "BILLED";
        public const string Collected = "COLLECTED";
    }

    /// <summary>
    /// Allocation method constants for revenue allocation.
    /// </summary>
    public static class RevenueAllocationMethod
    {
        public const string ProRata = "PRO-RATA";
        public const string Equation = "EQUATION";
        public const string Custom = "CUSTOM";
    }
}
