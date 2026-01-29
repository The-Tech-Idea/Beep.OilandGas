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
    /// Imbalance Service - Manages over/under-production situations and inventory reconciliation.
    /// Tracks cumulative imbalance, calculates monetary settlement value, handles imbalance reserves.
    /// 
    /// Formula:
    ///   Imbalance = |Produced Volume - Sold Volume|
    ///   Imbalance Liability = Imbalance Volume × Settlement Price
    ///   Settlement = Imbalance Volume × Settlement Price
    /// </summary>
    public class ImbalanceService : IImbalanceService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<ImbalanceService> _logger;
        private const string ConnectionName = "PPDM39";

        public ImbalanceService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<ImbalanceService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        /// <summary>
        /// Records an imbalance for a lease.
        /// Imbalance = Produced Volume - Sold Volume (can be positive or negative).
        /// </summary>
        public async Task<IMBALANCE_ADJUSTMENT> RecordImbalanceAsync(
            string leaseId,
            decimal volume,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            _logger?.LogInformation("Recording imbalance for lease {LeaseId}, volume: {Volume}",
                leaseId, volume);

            // Create imbalance adjustment record
            var adjustment = new IMBALANCE_ADJUSTMENT
            {
                IMBALANCE_ADJUSTMENT_ID = Guid.NewGuid().ToString(),
                ADJUSTMENT_AMOUNT = Math.Abs(volume),  // Store absolute value, track direction in REASON
                ADJUSTMENT_TYPE = volume > 0 ? ImbalanceType.Overproduced : ImbalanceType.Underproduced,
                PROPERTY_OR_LEASE_ID = leaseId,
                REASON = $"Imbalance for lease {leaseId}",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CREATED_BY = userId
            };

            // Save to database
            var metadata = await _metadata.GetTableMetadataAsync("IMBALANCE_ADJUSTMENT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(IMBALANCE_ADJUSTMENT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "IMBALANCE_ADJUSTMENT");

            await repo.InsertAsync(adjustment, userId);

            _logger?.LogInformation(
                "Imbalance recorded: {AdjustmentId} for lease {LeaseId}, type: {Type}, volume: {Volume}",
                adjustment.IMBALANCE_ADJUSTMENT_ID, leaseId, adjustment.ADJUSTMENT_TYPE, volume);

            return adjustment;
        }

        /// <summary>
        /// Reconciles imbalances for a lease over a date range.
        /// Calculates cumulative imbalance and prepares for settlement.
        /// </summary>
        public async Task<bool> ReconcileAsync(
            string leaseId,
            DateTime startDate,
            DateTime endDate,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));

            _logger?.LogInformation("Reconciling imbalances for lease {LeaseId} from {StartDate} to {EndDate}",
                leaseId, startDate.ToShortDateString(), endDate.ToShortDateString());

            try
            {
                // Get all imbalance adjustments for the lease in the date range
                var adjustments = await GetImbalanceAdjustmentsAsync(leaseId, startDate, endDate, cn);

                if (!adjustments.Any())
                {
                    _logger?.LogInformation("No imbalances found for lease {LeaseId} in period", leaseId);
                    return true;
                }

                // Calculate cumulative imbalance
                decimal cumulativeOverproduction = 0m;
                decimal cumulativeUnderproduction = 0m;

                foreach (var adj in adjustments)
                {
                    if (adj.ADJUSTMENT_TYPE == ImbalanceType.Overproduced)
                        cumulativeOverproduction += ((decimal?)adj.ADJUSTMENT_AMOUNT) ?? 0m;
                    else
                        cumulativeUnderproduction += ((decimal?)adj.ADJUSTMENT_AMOUNT) ?? 0m;
                }

                // Net imbalance (positive = over, negative = under)
                decimal netImbalance = cumulativeOverproduction - cumulativeUnderproduction;

                _logger?.LogInformation(
                    "Lease {LeaseId} imbalance reconciliation: Over={Over}, Under={Under}, Net={Net}",
                    leaseId, cumulativeOverproduction, cumulativeUnderproduction, netImbalance);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error reconciling imbalances");
                throw;
            }
        }

        /// <summary>
        /// Gets the outstanding (cumulative) imbalance for a lease.
        /// Returns positive for overproduction, negative for underproduction.
        /// </summary>
        public async Task<decimal> GetOutstandingImbalanceAsync(string leaseId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));

            _logger?.LogInformation("Getting outstanding imbalance for lease {LeaseId}", leaseId);

            try
            {
                // Get all active imbalance adjustments
                var adjustments = await GetAllImbalanceAdjustmentsAsync(leaseId, cn);

                // Calculate net outstanding imbalance
                decimal overproduced = adjustments
                    .Where(a => a.ADJUSTMENT_TYPE == ImbalanceType.Overproduced)
                    .Sum(a => ((decimal?)a.ADJUSTMENT_AMOUNT) ?? 0m);

                decimal underproduced = adjustments
                    .Where(a => a.ADJUSTMENT_TYPE == ImbalanceType.Underproduced)
                    .Sum(a => ((decimal?)a.ADJUSTMENT_AMOUNT) ?? 0m);

                decimal netImbalance = overproduced - underproduced;

                _logger?.LogInformation("Lease {LeaseId} outstanding imbalance: {Imbalance}", leaseId, netImbalance);
                return netImbalance;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting outstanding imbalance");
                throw;
            }
        }

        /// <summary>
        /// Validates an imbalance adjustment record.
        /// Checks: amount is positive, type is valid, etc.
        /// </summary>
        public async Task<bool> ValidateAsync(IMBALANCE_ADJUSTMENT imbalance, string cn = "PPDM39")
        {
            if (imbalance == null)
                throw new ArgumentNullException(nameof(imbalance));

            _logger?.LogInformation("Validating imbalance {AdjustmentId}", imbalance.IMBALANCE_ADJUSTMENT_ID);

            try
            {
                // Validation 1: Amount must be positive
                if (imbalance.ADJUSTMENT_AMOUNT == null || imbalance.ADJUSTMENT_AMOUNT <= 0)
                {
                    _logger?.LogWarning("Imbalance {AdjustmentId}: Invalid amount {Amount}",
                        imbalance.IMBALANCE_ADJUSTMENT_ID, imbalance.ADJUSTMENT_AMOUNT);
                    throw new AllocationException($"Imbalance amount must be positive: {imbalance.ADJUSTMENT_AMOUNT}");
                }

                // Validation 2: Type must be valid
                if (string.IsNullOrWhiteSpace(imbalance.ADJUSTMENT_TYPE) ||
                    !new[] { ImbalanceType.Overproduced, ImbalanceType.Underproduced }.Contains(imbalance.ADJUSTMENT_TYPE))
                {
                    _logger?.LogWarning("Imbalance {AdjustmentId}: Invalid adjustment type {Type}",
                        imbalance.IMBALANCE_ADJUSTMENT_ID, imbalance.ADJUSTMENT_TYPE);
                    throw new AllocationException($"Invalid adjustment type: {imbalance.ADJUSTMENT_TYPE}");
                }

                // Validation 3: Adjustment ID must be set
                if (string.IsNullOrWhiteSpace(imbalance.IMBALANCE_ADJUSTMENT_ID))
                {
                    _logger?.LogWarning("Imbalance: ID is required");
                    throw new AllocationException("Imbalance ID is required");
                }

                _logger?.LogInformation("Imbalance {AdjustmentId} validation passed", imbalance.IMBALANCE_ADJUSTMENT_ID);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Imbalance validation failed");
                throw;
            }
        }

        /// <summary>
        /// Gets imbalance adjustments for a lease in a date range.
        /// </summary>
        private async Task<List<IMBALANCE_ADJUSTMENT>> GetImbalanceAdjustmentsAsync(
            string leaseId,
            DateTime startDate,
            DateTime endDate,
            string cn = "PPDM39")
        {
            var metadata = await _metadata.GetTableMetadataAsync("IMBALANCE_ADJUSTMENT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(IMBALANCE_ADJUSTMENT);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "IMBALANCE_ADJUSTMENT");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = leaseId },
                new AppFilter { FieldName = "ROW_CREATED_DATE", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ROW_CREATED_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<IMBALANCE_ADJUSTMENT>().ToList() ?? new List<IMBALANCE_ADJUSTMENT>();
        }

        /// <summary>
        /// Gets all active imbalance adjustments for a lease.
        /// </summary>
        private async Task<List<IMBALANCE_ADJUSTMENT>> GetAllImbalanceAdjustmentsAsync(
            string leaseId,
            string cn = "PPDM39")
        {
            // Avoid DateTime.MinValue/MaxValue which some providers cannot translate.
            return await GetImbalanceAdjustmentsAsync(leaseId, new DateTime(1900, 1, 1), DateTime.UtcNow.Date.AddDays(1), cn);
        }
    }

    /// <summary>
    /// Imbalance adjustment type constants.
    /// </summary>
    public static class ImbalanceType
    {
        public const string Overproduced = "OVER-PRODUCED";
        public const string Underproduced = "UNDER-PRODUCED";
    }

    /// <summary>
    /// Imbalance status constants per PPDM39 standards.
    /// </summary>
    public static class ImbalanceStatus
    {
        public const string Pending = "PENDING";
        public const string Reconciled = "RECONCILED";
        public const string Settled = "SETTLED";
        public const string Void = "VOID";
    }
}
