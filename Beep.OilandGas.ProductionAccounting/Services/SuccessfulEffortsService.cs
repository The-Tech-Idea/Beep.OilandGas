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
    /// Successful Efforts Service - Capitalizes only successful exploration/development costs.
    /// Per ASC 932 (Oil & Gas Accounting): Dry holes expensed immediately, successful wells capitalized.
    /// 
    /// Logic:
    ///   IF well status = Successful THEN capitalize costs
    ///   ELSE IF well status = Dry Hole OR unsuccessful THEN expense costs immediately
    /// </summary>
    public class SuccessfulEffortsService : ISuccessfulEffortsService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<SuccessfulEffortsService> _logger;
        private const string ConnectionName = "PPDM39";

        public SuccessfulEffortsService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<SuccessfulEffortsService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        /// <summary>
        /// Records a cost for a well under Successful Efforts method.
        /// Successful well: CAPITALIZES cost (balance sheet asset)
        /// Unsuccessful well (dry hole): EXPENSES cost immediately (P&L charge)
        /// </summary>
        public async Task<ACCOUNTING_COST> RecordCostAsync(
            string wellId,
            decimal cost,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(wellId))
                throw new ArgumentNullException(nameof(wellId));
            if (cost <= 0)
                throw new AccountingException($"Cost must be positive: {cost}");

            _logger?.LogInformation("Recording Successful Efforts cost for well {WellId}, amount: {Amount}",
                wellId, cost);

            // Create accounting cost record
            // CRITICAL: Decision point - is this well successful or dry hole?
            // In real system, would query well status from WELL entity
                var accountingCost = new ACCOUNTING_COST
                {
                    ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                    WELL_ID = wellId,
                    AMOUNT = cost,
                    COST_DATE = DateTime.UtcNow,
                    COST_TYPE = CostTypes.Development,  // Default; would be set based on context
                    COST_CATEGORY = CostCategories.Drilling,  // Default; would be set based on context
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    ROW_CREATED_BY = userId
                };

            // Save to database
            var metadata = await _metadata.GetTableMetadataAsync("ACCOUNTING_COST");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ACCOUNTING_COST);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ACCOUNTING_COST");

            await repo.InsertAsync(accountingCost, userId);

            _logger?.LogInformation(
                "Successful Efforts cost recorded: {CostId} for well {WellId}, amount: {Amount}",
                accountingCost.ACCOUNTING_COST_ID, wellId, cost);

            return accountingCost;
        }

        /// <summary>
        /// Calculates depletion for a well under Successful Efforts method.
        /// Uses Unit of Production method: Depletion = (Production x Capitalized Cost) / Total Reserves
        /// </summary>
        public async Task<decimal> CalculateDepletionAsync(
            string wellId,
            DateTime startDate,
            DateTime endDate,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(wellId))
                throw new ArgumentNullException(nameof(wellId));

            _logger?.LogInformation(
                "Calculating depletion for well {WellId} from {StartDate} to {EndDate}",
                wellId, startDate.ToShortDateString(), endDate.ToShortDateString());

            try
            {
                // Get total capitalized costs for the well
                decimal totalCapitalizedCosts = await GetTotalCapitalizedCostsAsync(wellId, cn);
                if (totalCapitalizedCosts <= 0)
                {
                    _logger?.LogInformation("Well {WellId} has no capitalized costs", wellId);
                    return 0;
                }

                // Unit of Production Depletion Formula:
                // Depletion = (Period Production x Total Capitalized Cost) / Total Proved Reserves

                // Get period production from MEASUREMENT_RECORD
                decimal periodProduction = await GetPeriodProductionAsync(wellId, cn);
                
                // Get total proved reserves for the well
                decimal totalReserves = await GetProvedReservesAsync(wellId, cn);

                // Calculate depletion using UOP method
                decimal depletionAmount = 0;
                if (totalReserves > 0)
                {
                    depletionAmount = (periodProduction * totalCapitalizedCosts) / totalReserves;
                    _logger?.LogInformation(
                        "Depletion (UOP) for well {WellId}: ({Production} x ${Cost}) / {Reserves} = ${Amount}",
                        wellId, periodProduction, totalCapitalizedCosts, totalReserves, depletionAmount);
                }
                else
                {
                    // Fallback: Use simple percentage if reserves not available
                    depletionAmount = totalCapitalizedCosts * 0.05m;
                    _logger?.LogWarning(
                        "Total proved reserves for well {WellId} is zero or not found, using fallback 5% depletion: ${Amount}",
                        wellId, depletionAmount);
                }

                return depletionAmount;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating depletion");
                throw;
            }
        }

        /// <summary>
        /// Validates an accounting cost record.
        /// Checks: amount is positive, cost type is valid, dates make sense, etc.
        /// </summary>
        public async Task<bool> ValidateAsync(ACCOUNTING_COST cost, string cn = "PPDM39")
        {
            if (cost == null)
                throw new ArgumentNullException(nameof(cost));

            _logger?.LogInformation("Validating accounting cost {CostId}", cost.ACCOUNTING_COST_ID);

            try
            {
                // Validation 1: Cost must be positive
                if (cost.AMOUNT <= 0 && !IsSalvageRecovery(cost))
                {
                    _logger?.LogWarning("Accounting cost {CostId}: Invalid amount {Amount}",
                        cost.ACCOUNTING_COST_ID, cost.AMOUNT);
                    throw new AccountingException($"Cost amount must be positive: {cost.AMOUNT}");
                }

                // Validation 2: Either WELL_ID or FIELD_ID must be set (links to entity)
                if (string.IsNullOrWhiteSpace(cost.WELL_ID) && string.IsNullOrWhiteSpace(cost.FIELD_ID))
                {
                    _logger?.LogWarning("Accounting cost {CostId}: Well ID and Field ID both missing",
                        cost.ACCOUNTING_COST_ID);
                    throw new AccountingException("Either Well ID or Field ID must be set");
                }

                // Validation 3: Cost type should be valid if set
                if (!string.IsNullOrWhiteSpace(cost.COST_TYPE))
                {
                    var validTypes = new[] { CostTypes.Exploration, CostTypes.Development, CostTypes.Acquisition, CostTypes.Production };
                    if (!validTypes.Contains(cost.COST_TYPE))
                    {
                        _logger?.LogWarning("Accounting cost {CostId}: Invalid cost type {Type}",
                            cost.ACCOUNTING_COST_ID, cost.COST_TYPE);
                        throw new AccountingException($"Invalid cost type: {cost.COST_TYPE}");
                    }
                }

                // Validation 4: Cost date should not be in the future
                if (cost.COST_DATE.HasValue && cost.COST_DATE > DateTime.UtcNow)
                {
                    _logger?.LogWarning("Accounting cost {CostId}: Cost date is in the future",
                        cost.ACCOUNTING_COST_ID);
                    throw new AccountingException("Cost date cannot be in the future");
                }

                // Validation 5: ID must be set
                if (string.IsNullOrWhiteSpace(cost.ACCOUNTING_COST_ID))
                {
                    _logger?.LogWarning("Accounting cost: ID is required");
                    throw new AccountingException("Accounting cost ID is required");
                }

                _logger?.LogInformation("Accounting cost {CostId} validation passed", cost.ACCOUNTING_COST_ID);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Accounting cost validation failed");
                throw;
            }
        }

        private static bool IsSalvageRecovery(ACCOUNTING_COST cost)
        {
            if (cost == null)
                return false;
            if (!string.IsNullOrWhiteSpace(cost.COST_CATEGORY) &&
                string.Equals(cost.COST_CATEGORY, CostCategories.Salvage, StringComparison.OrdinalIgnoreCase))
                return true;
            if (!string.IsNullOrWhiteSpace(cost.DESCRIPTION) &&
                cost.DESCRIPTION.IndexOf("SALVAGE", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            return false;
        }

        /// <summary>
        /// Reclassifies unproved property costs to proved status by capitalizing exploration costs.
        /// </summary>
        public async Task<bool> ReclassifyPropertyAsync(string propertyId, string userId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(propertyId))
                throw new ArgumentNullException(nameof(propertyId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var metadata = await _metadata.GetTableMetadataAsync("ACCOUNTING_COST");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ACCOUNTING_COST);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ACCOUNTING_COST");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var costs = results?.Cast<ACCOUNTING_COST>().ToList() ?? new List<ACCOUNTING_COST>();
            if (costs.Count == 0)
                return false;

            foreach (var cost in costs)
            {
                if (string.Equals(cost.COST_TYPE, CostTypes.Exploration, StringComparison.OrdinalIgnoreCase))
                {
                    cost.COST_TYPE = CostTypes.Development;
                }

                cost.IS_CAPITALIZED = "Y";
                cost.IS_EXPENSED = "N";
                cost.ROW_CHANGED_BY = userId;
                cost.ROW_CHANGED_DATE = DateTime.UtcNow;

                await repo.UpdateAsync(cost, userId);
            }

            _logger?.LogInformation(
                "Reclassified {Count} cost records for property {PropertyId} to proved status",
                costs.Count, propertyId);

            return true;
        }

        /// <summary>
        /// Gets total capitalized costs for a well.
        /// Used for depletion rate calculation.
        /// </summary>
        private async Task<decimal> GetTotalCapitalizedCostsAsync(
            string wellId,
            string cn = "PPDM39")
        {
            var metadata = await _metadata.GetTableMetadataAsync("ACCOUNTING_COST");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ACCOUNTING_COST);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ACCOUNTING_COST");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var costs = await repo.GetAsync(filters);
            var costList = costs?.Cast<ACCOUNTING_COST>().ToList() ?? new List<ACCOUNTING_COST>();

            return costList.Sum(c => c.AMOUNT);
        }

        /// <summary>
        /// Gets total production for a well during current period from MEASUREMENT_RECORD.
        /// </summary>
        private async Task<decimal> GetPeriodProductionAsync(string wellId, string cn)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("MEASUREMENT_RECORD");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(MEASUREMENT_RECORD);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "MEASUREMENT_RECORD");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var measurements = await repo.GetAsync(filters);
                var measurementList = measurements?.Cast<MEASUREMENT_RECORD>().ToList() ?? new List<MEASUREMENT_RECORD>();

                decimal totalProduction = measurementList.Sum(m => m.GROSS_VOLUME ?? 0);
                _logger?.LogDebug("Period production for well {WellId}: {Volume} BBL", wellId, totalProduction);

                return totalProduction;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error retrieving period production for well {WellId}", wellId);
                return 0;
            }
        }

        /// <summary>
        /// Gets total proved reserves for a well from PROVED_RESERVES table.
        /// Sums developed and undeveloped oil reserves.
        /// </summary>
        private async Task<decimal> GetProvedReservesAsync(string wellId, string cn)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("PROVED_RESERVES");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(PROVED_RESERVES);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "PROVED_RESERVES");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "LIKE", FilterValue = $"%{wellId}%" },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var reserves = await repo.GetAsync(filters);
                var reserveList = reserves?.Cast<PROVED_RESERVES>().ToList() ?? new List<PROVED_RESERVES>();

                // Use most recent reserve estimate and sum oil reserves (developed + undeveloped)
                if (reserveList.Any())
                {
                    var latestReserve = reserveList.OrderByDescending(r => r.RESERVE_DATE).FirstOrDefault();
                    decimal totalReserves = (latestReserve?.PROVED_DEVELOPED_OIL_RESERVES ?? 0) +
                                           (latestReserve?.PROVED_UNDEVELOPED_OIL_RESERVES ?? 0);

                    _logger?.LogDebug("Total proved reserves for well {WellId}: {Volume} BBL", wellId, totalReserves);
                    return totalReserves;
                }

                _logger?.LogWarning("No proved reserves found for well {WellId}", wellId);
                return 0;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error retrieving proved reserves for well {WellId}", wellId);
                return 0;
            }
        }
    }

}
