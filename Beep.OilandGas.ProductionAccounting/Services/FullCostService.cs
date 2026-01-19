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
        private readonly IReserveAccountingService _reserveAccountingService;
        private readonly ILogger<FullCostService> _logger;
        private const string ConnectionName = "PPDM39";

        public FullCostService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<FullCostService> logger = null,
            IReserveAccountingService reserveAccountingService = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _reserveAccountingService = reserveAccountingService;
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
                COST_TYPE = CostTypes.Exploration,
                COST_CATEGORY = CostCategories.Seismic,
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

                // SEC Ceiling Test: Fair Value = PV of proved reserves
                // Use proved reserves valuation at current commodity prices
                decimal fairValueEstimate = await GetProvedReservesValueAsync(costCenterId, cn);
                
                if (fairValueEstimate <= 0)
                {
                    // Fallback: Use simplified approach if reserves valuation not available
                    fairValueEstimate = capitalizedCosts * 1.2m;  // Allow 20% appreciation
                    _logger?.LogWarning(
                        "Could not calculate fair value from proved reserves, using fallback 1.2x capitalized costs");
                }

                if (netBookValue > fairValueEstimate)
                {
                    decimal impairmentAmount = netBookValue - fairValueEstimate;
                    _logger?.LogWarning(
                        "Ceiling test FAILED for cost center {CostCenterId}: NBV {NBV} > Fair Value {FV}, Impairment: {Impairment}",
                        costCenterId, netBookValue, fairValueEstimate, impairmentAmount);
                    await RecordImpairmentAsync(
                        costCenterId,
                        impairmentAmount,
                        "SEC ceiling test impairment",
                        userId,
                        cn);
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
        /// Depletion = (Period Production x Net Capitalized Costs) / Total Proved Reserves
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

                // Unit of Production Depletion Formula:
                // Depletion = (Period Production x Capitalized Costs) / Total Proved Reserves
                
                decimal periodProduction = await GetPeriodProductionAsync(costCenterId, cn);
                decimal totalReserves = await GetTotalReservesAsync(costCenterId, cn);

                decimal depletionAmount = 0;
                if (totalReserves > 0)
                {
                    depletionAmount = (periodProduction * capitalizedCosts) / totalReserves;
                    _logger?.LogInformation(
                        "Depletion (UOP) for cost center {CostCenterId}: ({Production} x ${Cost}) / {Reserves} = ${Amount}",
                        costCenterId, periodProduction, capitalizedCosts, totalReserves, depletionAmount);
                }
                else
                {
                    // Fallback: Use simple percentage if reserves not available
                    depletionAmount = capitalizedCosts * 0.08m;
                    _logger?.LogWarning(
                        "Total proved reserves for cost center {CostCenterId} is zero or not found, using fallback 8% depletion: ${Amount}",
                        costCenterId, depletionAmount);
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

        /// <summary>
        /// Records an impairment for a cost center.
        /// </summary>
        public async Task<IMPAIRMENT_RECORD> RecordImpairmentAsync(
            string costCenterId,
            decimal impairmentAmount,
            string reason,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(costCenterId))
                throw new ArgumentNullException(nameof(costCenterId));
            if (impairmentAmount <= 0)
                throw new AccountingException("Impairment amount must be positive");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var impairment = new IMPAIRMENT_RECORD
            {
                IMPAIRMENT_RECORD_ID = Guid.NewGuid().ToString(),
                COST_CENTER_ID = costCenterId,
                IMPAIRMENT_DATE = DateTime.UtcNow,
                IMPAIRMENT_AMOUNT = impairmentAmount,
                IMPAIRMENT_TYPE = "CEILING_TEST",
                REASON = reason,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CREATED_BY = userId
            };

            var metadata = await _metadata.GetTableMetadataAsync("IMPAIRMENT_RECORD");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(IMPAIRMENT_RECORD);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "IMPAIRMENT_RECORD");

            await repo.InsertAsync(impairment, userId);

            _logger?.LogInformation(
                "Impairment recorded for cost center {CostCenterId}: {Amount}",
                costCenterId, impairmentAmount);

            return impairment;
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

        /// <summary>
        /// Gets total production for cost center during current period from MEASUREMENT_RECORD.
        /// </summary>
        private async Task<decimal> GetPeriodProductionAsync(string costCenterId, string cn)
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
                    new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = costCenterId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var measurements = await repo.GetAsync(filters);
                var measurementList = measurements?.Cast<MEASUREMENT_RECORD>().ToList() ?? new List<MEASUREMENT_RECORD>();

                decimal totalProduction = measurementList.Sum(m => m.GROSS_VOLUME ?? 0);
                _logger?.LogDebug("Period production for cost center {CostCenterId}: {Volume} BBL", costCenterId, totalProduction);

                return totalProduction;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error retrieving period production for cost center {CostCenterId}", costCenterId);
                return 0;
            }
        }

        /// <summary>
        /// Gets total proved reserves for cost center from PROVED_RESERVES table.
        /// </summary>
        private async Task<decimal> GetTotalReservesAsync(string costCenterId, string cn)
        {
            try
            {
                if (_reserveAccountingService != null)
                {
                    var reserves = await _reserveAccountingService.GetLatestReservesAsync(costCenterId, DateTime.UtcNow, cn);
                    if (reserves != null)
                    {
                        return (reserves.PROVED_DEVELOPED_OIL_RESERVES ?? 0) +
                               (reserves.PROVED_UNDEVELOPED_OIL_RESERVES ?? 0) +
                               (reserves.PROVED_DEVELOPED_GAS_RESERVES ?? 0) +
                               (reserves.PROVED_UNDEVELOPED_GAS_RESERVES ?? 0);
                    }
                }

                var metadata = await _metadata.GetTableMetadataAsync("PROVED_RESERVES");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(PROVED_RESERVES);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "PROVED_RESERVES");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "COST_CENTER_ID", Operator = "LIKE", FilterValue = $"%{costCenterId}%" },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var reserves = await repo.GetAsync(filters);
                var reserveList = reserves?.Cast<PROVED_RESERVES>().ToList() ?? new List<PROVED_RESERVES>();

                // Sum total proved reserves (developed + undeveloped oil)
                if (reserveList.Any())
                {
                    var latestReserve = reserveList.OrderByDescending(r => r.RESERVE_DATE).FirstOrDefault();
                    decimal totalReserves = (latestReserve?.PROVED_DEVELOPED_OIL_RESERVES ?? 0) +
                                           (latestReserve?.PROVED_UNDEVELOPED_OIL_RESERVES ?? 0);

                    _logger?.LogDebug("Total proved reserves for cost center {CostCenterId}: {Volume} BBL", costCenterId, totalReserves);
                    return totalReserves;
                }

                _logger?.LogWarning("No proved reserves found for cost center {CostCenterId}", costCenterId);
                return 0;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error retrieving proved reserves for cost center {CostCenterId}", costCenterId);
                return 0;
            }
        }

        /// <summary>
        /// Gets fair value of proved reserves using PV calculation.
        /// Fair Value = Sum of (Reserve Volume x Commodity Price x Discount Factor)
        /// </summary>
        private async Task<decimal> GetProvedReservesValueAsync(string costCenterId, string cn)
        {
            try
            {
                if (_reserveAccountingService != null)
                {
                    var pv = await _reserveAccountingService.CalculatePresentValueAsync(costCenterId, DateTime.UtcNow, cn);
                    if (pv > 0m)
                        return pv;
                }

                var metadata = await _metadata.GetTableMetadataAsync("PROVED_RESERVES");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(PROVED_RESERVES);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "PROVED_RESERVES");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "COST_CENTER_ID", Operator = "LIKE", FilterValue = $"%{costCenterId}%" },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var reserves = await repo.GetAsync(filters);
                var reserveList = reserves?.Cast<PROVED_RESERVES>().ToList() ?? new List<PROVED_RESERVES>();

                // Calculate PV of proved reserves using current commodity prices
                if (reserveList.Any())
                {
                    var latestReserve = reserveList.OrderByDescending(r => r.RESERVE_DATE).FirstOrDefault();
                    return CalculateReserveFairValue(latestReserve);
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error retrieving proved reserves fair value for cost center {CostCenterId}", costCenterId);
                return 0;
            }
        }

        private decimal CalculateReserveFairValue(PROVED_RESERVES reserves)
        {
            if (reserves == null)
                return 0m;

            decimal oilVolume = (reserves.PROVED_DEVELOPED_OIL_RESERVES ?? 0) +
                               (reserves.PROVED_UNDEVELOPED_OIL_RESERVES ?? 0);
            decimal oilPrice = reserves.OIL_PRICE ?? 0;

            decimal gasVolume = (reserves.PROVED_DEVELOPED_GAS_RESERVES ?? 0) +
                               (reserves.PROVED_UNDEVELOPED_GAS_RESERVES ?? 0);
            decimal gasPrice = reserves.GAS_PRICE ?? 0;

            decimal discountFactor = 0.9091m;

            decimal oilValue = (oilVolume * oilPrice) * discountFactor;
            decimal gasValue = (gasVolume * gasPrice) * discountFactor;
            var fairValue = oilValue + gasValue;

            _logger?.LogDebug(
                "Fair value (PV): Oil({OilVol}x${OilPrice}x{DF}) + Gas({GasVol}x${GasPrice}x{DF}) = ${FairValue}",
                oilVolume, oilPrice, discountFactor, gasVolume, gasPrice, discountFactor, fairValue);

            return fairValue;
        }
    }
}
