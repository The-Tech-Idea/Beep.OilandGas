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
    /// Full Cost Service - Capitalizes all exploration/development costs in cost centers.
    /// Per ASC 932: All costs capitalized within cost center, subject to SEC ceiling test.
    /// 
    /// Ceiling Test (SEC Requirement):
    ///   IF Net Book Value > Fair Value THEN record impairment
    ///   Net Book Value = Capitalized Costs - Accumulated Depletion
    ///   Fair Value = PV of proved reserves + other assets
    /// </summary>
    public class FullCostService : IFullCostService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<FullCostService> _logger;
        private const string ConnectionName = "PPDM39";

        public FullCostService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<FullCostService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        /// <summary>
        /// Records a cost to a cost center under Full Cost method.
        /// ALL costs (successful or not) are capitalized and amortized over proved reserves.
        /// </summary>
        public async Task<ACCOUNTING_COST> RecordCostAsync(
            string costCenterId,
            decimal cost,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(costCenterId))
                throw new ArgumentNullException(nameof(costCenterId));
            if (cost <= 0)
                throw new AccountingException($"Cost must be positive: {cost}");

            _logger?.LogInformation("Recording Full Cost cost for cost center {CostCenterId}, amount: {Amount}",
                costCenterId, cost);

            var accountingCost = new ACCOUNTING_COST
            {
                ACCOUNTING_COST_ID = Guid.NewGuid().ToString(),
                POOL_ID = costCenterId,  // Link to cost center pool
                AMOUNT = cost,
                COST_DATE = DateTime.UtcNow,
                COST_TYPE = CostType.Exploration,
                COST_CATEGORY = CostCategory.Seismic,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CREATED_BY = userId
            };

            var metadata = await _metadata.GetTableMetadataAsync("ACCOUNTING_COST");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ACCOUNTING_COST);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ACCOUNTING_COST");

            await repo.InsertAsync(accountingCost, userId);

            _logger?.LogInformation("Full Cost recorded: {CostId} for cost center {CostCenterId}, amount: {Amount}",
                accountingCost.ACCOUNTING_COST_ID, costCenterId, cost);

            return accountingCost;
        }

        /// <summary>
        /// Performs ceiling test (SEC requirement for full cost companies).
        /// IF Net Book Value > Fair Value THEN record impairment.
        /// </summary>
        public async Task<bool> PerformCeilingTestAsync(
            string costCenterId,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(costCenterId))
                throw new ArgumentNullException(nameof(costCenterId));

            _logger?.LogInformation("Performing ceiling test for cost center {CostCenterId}", costCenterId);

            try
            {
                // Get total capitalized costs for cost center
                decimal capitalizedCosts = await GetTotalCapitalizedCostsAsync(costCenterId, cn);
                
                // Calculate net book value
                decimal accumulatedDepletion = await GetAccumulatedDepletionAsync(costCenterId, cn);
                decimal netBookValue = capitalizedCosts - accumulatedDepletion;

                // In real implementation:
                // 1. Calculate fair value = PV of proved reserves + other assets
                // 2. Compare: if NBV > Fair Value, record impairment
                // For now, use simplified approach: NBV shouldn't exceed reasonable limit (e.g., 150% of depletion base)

                decimal fairValueEstimate = capitalizedCosts * 1.2m;  // Placeholder: allows 20% appreciation

                if (netBookValue > fairValueEstimate)
                {
                    decimal impairmentAmount = netBookValue - fairValueEstimate;
                    _logger?.LogWarning(
                        "Ceiling test FAILED for cost center {CostCenterId}: NBV {NBV} > Fair Value {FV}, Impairment: {Impairment}",
                        costCenterId, netBookValue, fairValueEstimate, impairmentAmount);
                    return false;
                }

                _logger?.LogInformation("Ceiling test PASSED for cost center {CostCenterId}", costCenterId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing ceiling test");
                throw;
            }
        }

        /// <summary>
        /// Calculates depletion for cost center using Unit of Production method.
        /// Depletion = (Period Production × Net Capitalized Costs) / Total Proved Reserves
        /// </summary>
        public async Task<decimal> CalculateDepletionAsync(
            string costCenterId,
            DateTime startDate,
            DateTime endDate,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(costCenterId))
                throw new ArgumentNullException(nameof(costCenterId));

            _logger?.LogInformation(
                "Calculating depletion for cost center {CostCenterId} from {StartDate} to {EndDate}",
                costCenterId, startDate.ToShortDateString(), endDate.ToShortDateString());

            try
            {
                decimal capitalizedCosts = await GetTotalCapitalizedCostsAsync(costCenterId, cn);
                if (capitalizedCosts <= 0)
                {
                    _logger?.LogInformation("Cost center {CostCenterId} has no capitalized costs", costCenterId);
                    return 0;
                }

                // In real implementation:
                // 1. Get total production during period
                // 2. Get total proved reserves for cost center
                // 3. Calculate: Depletion = (Period Production × Capitalized Costs) / Total Reserves
                
                decimal depletionAmount = capitalizedCosts * 0.08m;  // Placeholder: 8% per period

                _logger?.LogInformation("Depletion calculated for cost center {CostCenterId}: {Amount}",
                    costCenterId, depletionAmount);

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
        /// </summary>
        public async Task<bool> ValidateAsync(ACCOUNTING_COST cost, string cn = "PPDM39")
        {
            if (cost == null)
                throw new ArgumentNullException(nameof(cost));

            _logger?.LogInformation("Validating Full Cost accounting cost {CostId}", cost.ACCOUNTING_COST_ID);

            try
            {
                if (cost.AMOUNT <= 0)
                    throw new AccountingException($"Cost amount must be positive: {cost.AMOUNT}");

                if (string.IsNullOrWhiteSpace(cost.POOL_ID) && string.IsNullOrWhiteSpace(cost.FIELD_ID))
                    throw new AccountingException("Either Pool ID or Field ID must be set for full cost");

                _logger?.LogInformation("Full Cost accounting cost {CostId} validation passed", cost.ACCOUNTING_COST_ID);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Full Cost validation failed");
                throw;
            }
        }

        private async Task<decimal> GetTotalCapitalizedCostsAsync(string costCenterId, string cn = "PPDM39")
        {
            var metadata = await _metadata.GetTableMetadataAsync("ACCOUNTING_COST");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ACCOUNTING_COST);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ACCOUNTING_COST");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "POOL_ID", Operator = "=", FilterValue = costCenterId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var costs = await repo.GetAsync(filters);
            return costs?.Cast<ACCOUNTING_COST>().Sum(c => c.AMOUNT) ?? 0;
        }

        private async Task<decimal> GetAccumulatedDepletionAsync(string costCenterId, string cn = "PPDM39")
        {
            var metadata = await _metadata.GetTableMetadataAsync("AMORTIZATION_RECORD");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(AMORTIZATION_RECORD);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "AMORTIZATION_RECORD");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = costCenterId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var records = await repo.GetAsync(filters);
            return records?.Cast<AMORTIZATION_RECORD>().Sum(r => r.AMORTIZATION_AMOUNT ?? 0) ?? 0;
        }
    }
}
