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
    /// Amortization Service - Allocates capitalized costs over asset life using Unit of Production method.
    /// Per ASC 932: Oil & Gas assets depleted using UOP based on proved reserves.
    /// 
    /// Formula:
    ///   UOP Rate = Capitalized Cost / Total Proved Reserves
    ///   Period Depletion = Period Production x UOP Rate
    /// </summary>
    public class AmortizationService : IAmortizationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IReserveAccountingService _reserveAccountingService;
        private readonly ILogger<AmortizationService> _logger;
        private const string ConnectionName = "PPDM39";

        public AmortizationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<AmortizationService> logger = null,
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
        /// Calculates amortization/depletion for an asset in a period.
        /// Uses Unit of Production: Period Depletion = (Period Production x Cap Cost) / Proved Reserves
        /// </summary>
        public async Task<AMORTIZATION_RECORD> CalculateAsync(
            string assetId,
            DateTime period,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(assetId))
                throw new ArgumentNullException(nameof(assetId));

            _logger?.LogInformation("Calculating amortization for asset {AssetId}, period {Period}",
                assetId, period.ToShortDateString());

            try
            {
                // Get capitalized costs for asset
                decimal capitalizedCosts = await GetCapitalizedCostsAsync(assetId, cn);
                if (capitalizedCosts <= 0)
                {
                    _logger?.LogInformation("Asset {AssetId} has no capitalized costs", assetId);
                    return null;
                }

                // Unit of Production Depletion Formula:
                // Depletion = (Period Production x Capitalized Costs) / Total Proved Reserves

                decimal periodProduction = await GetPeriodProductionAsync(assetId, cn);
                decimal totalReserves = await GetProvedReservesAsync(assetId, cn);

                decimal periodDepletion = 0;
                if (_reserveAccountingService != null)
                {
                    var rate = await _reserveAccountingService.CalculateDepletionRateAsync(
                        assetId, capitalizedCosts, period, cn);
                    if (rate > 0m)
                    {
                        periodDepletion = periodProduction * rate;
                        _logger?.LogDebug(
                            "Depletion (Reserve rate) for asset {AssetId}: {Production} x {Rate} = ${Amount}",
                            assetId, periodProduction, rate, periodDepletion);
                    }
                }

                if (periodDepletion <= 0m && totalReserves > 0)
                {
                    periodDepletion = (periodProduction * capitalizedCosts) / totalReserves;
                    _logger?.LogDebug(
                        "Depletion (UOP) for asset {AssetId}: ({Production} x ${Cost}) / {Reserves} = ${Amount}",
                        assetId, periodProduction, capitalizedCosts, totalReserves, periodDepletion);
                }
                else
                {
                    // Fallback: Use simple percentage if reserves not available
                    periodDepletion = capitalizedCosts * 0.06m;
                    _logger?.LogWarning(
                        "Total proved reserves for asset {AssetId} is zero or not found, using fallback 6% depletion: ${Amount}",
                        assetId, periodDepletion);
                }

                // Create amortization record
                var record = new AMORTIZATION_RECORD
                {
                    AMORTIZATION_RECORD_ID = Guid.NewGuid().ToString(),
                    PROPERTY_ID = assetId,
                    PERIOD_START_DATE = period,
                    PERIOD_END_DATE = period.AddMonths(1).AddDays(-1),
                    NET_CAPITALIZED_COSTS = capitalizedCosts,
                    AMORTIZATION_AMOUNT = periodDepletion,
                    ACCOUNTING_METHOD = AmortizationMethods.UnitOfProduction,
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    ROW_CREATED_BY = userId
                };

                // Save to database
                var metadata = await _metadata.GetTableMetadataAsync("AMORTIZATION_RECORD");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(AMORTIZATION_RECORD);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "AMORTIZATION_RECORD");

                await repo.InsertAsync(record, userId);

                _logger?.LogInformation("Amortization calculated: {RecordId} for asset {AssetId}, amount: {Amount}",
                    record.AMORTIZATION_RECORD_ID, assetId, periodDepletion);

                return record;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating amortization");
                throw;
            }
        }

        /// <summary>
        /// Calculates fieldwide DD&amp;A by aggregating capitalized costs and reserves across a field.
        /// </summary>
        public async Task<AMORTIZATION_RECORD> CalculateFieldwideAsync(
            string fieldId,
            DateTime period,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));

            var capitalizedCosts = await GetFieldCapitalizedCostsAsync(fieldId, cn);
            if (capitalizedCosts <= 0m)
                return null;

            var production = await GetFieldProductionAsync(fieldId, cn);
            var reserves = await GetFieldReservesAsync(fieldId, cn);
            var depletion = reserves > 0m ? (production * capitalizedCosts) / reserves : 0m;

            var record = new AMORTIZATION_RECORD
            {
                AMORTIZATION_RECORD_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = fieldId,
                PERIOD_START_DATE = period,
                PERIOD_END_DATE = period.AddMonths(1).AddDays(-1),
                NET_CAPITALIZED_COSTS = capitalizedCosts,
                AMORTIZATION_AMOUNT = depletion,
                ACCOUNTING_METHOD = AmortizationMethods.UnitOfProduction,
                REMARK = $"FIELD_ID={fieldId}",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CREATED_BY = userId
            };

            var metadata = await _metadata.GetTableMetadataAsync("AMORTIZATION_RECORD");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(AMORTIZATION_RECORD);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "AMORTIZATION_RECORD");

            await repo.InsertAsync(record, userId);
            return record;
        }

        /// <summary>
        /// Calculates oil/gas and working/non-working DD&amp;A splits for a property.
        /// </summary>
        public async Task<AMORTIZATION_SPLIT> CalculateSplitAsync(
            string propertyId,
            DateTime periodEnd,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(propertyId))
                throw new ArgumentNullException(nameof(propertyId));

            var capitalizedCosts = await GetCapitalizedCostsAsync(propertyId, cn);
            if (capitalizedCosts <= 0m)
                return null;

            var (oilVolume, gasVolume) = await GetRevenueVolumesAsync(propertyId, periodEnd, cn);
            var totalVolume = oilVolume + gasVolume;
            if (totalVolume <= 0m)
                totalVolume = 1m;

            var totalReserves = await GetProvedReservesAsync(propertyId, cn);
            var production = await GetPeriodProductionAsync(propertyId, cn);
            var depletion = totalReserves > 0m ? (production * capitalizedCosts) / totalReserves : 0m;

            var oilDepletion = depletion * (oilVolume / totalVolume);
            var gasDepletion = depletion * (gasVolume / totalVolume);

            var workingInterest = await GetWorkingInterestAsync(propertyId, cn);
            var workingDepletion = depletion * workingInterest;
            var nonWorkingDepletion = depletion - workingDepletion;

            var split = new AMORTIZATION_SPLIT
            {
                AMORTIZATION_SPLIT_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = propertyId,
                PERIOD_END_DATE = periodEnd,
                OIL_DEPLETION = oilDepletion,
                GAS_DEPLETION = gasDepletion,
                WORKING_INTEREST_DEPLETION = workingDepletion,
                NON_WORKING_DEPLETION = nonWorkingDepletion,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var splitRepo = await CreateRepoAsync<AMORTIZATION_SPLIT>("AMORTIZATION_SPLIT", cn);
            await splitRepo.InsertAsync(split, userId);

            return split;
        }

        /// <summary>
        /// Gets accumulated depletion for an asset.
        /// Sum of all AMORTIZATION_AMOUNT records for the asset.
        /// </summary>
        public async Task<decimal> GetAccumulatedDepletionAsync(string assetId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(assetId))
                throw new ArgumentNullException(nameof(assetId));

            _logger?.LogInformation("Getting accumulated depletion for asset {AssetId}", assetId);

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("AMORTIZATION_RECORD");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(AMORTIZATION_RECORD);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "AMORTIZATION_RECORD");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = assetId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var records = await repo.GetAsync(filters);
                decimal accumulated = records?.Cast<AMORTIZATION_RECORD>().Sum(r => r.AMORTIZATION_AMOUNT ?? 0) ?? 0;

                _logger?.LogInformation("Accumulated depletion for asset {AssetId}: {Amount}",
                    assetId, accumulated);

                return accumulated;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting accumulated depletion");
                throw;
            }
        }

        /// <summary>
        /// Validates an amortization record.
        /// Checks: amount is positive, dates are valid, capitalized cost set, etc.
        /// </summary>
        public async Task<bool> ValidateAsync(AMORTIZATION_RECORD record, string cn = "PPDM39")
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            _logger?.LogInformation("Validating amortization record {RecordId}", record.AMORTIZATION_RECORD_ID);

            try
            {
                // Validation 1: Amount should be non-negative
                if (record.AMORTIZATION_AMOUNT.HasValue && record.AMORTIZATION_AMOUNT < 0)
                {
                    _logger?.LogWarning("Amortization record {RecordId}: Negative amount {Amount}",
                        record.AMORTIZATION_RECORD_ID, record.AMORTIZATION_AMOUNT);
                    throw new AccountingException($"Amortization amount cannot be negative: {record.AMORTIZATION_AMOUNT}");
                }

                // Validation 2: Capitalized costs should be positive
                if (record.NET_CAPITALIZED_COSTS == null || record.NET_CAPITALIZED_COSTS <= 0)
                {
                    _logger?.LogWarning("Amortization record {RecordId}: Invalid capitalized costs {Costs}",
                        record.AMORTIZATION_RECORD_ID, record.NET_CAPITALIZED_COSTS);
                    throw new AccountingException($"Capitalized costs must be positive: {record.NET_CAPITALIZED_COSTS}");
                }

                // Validation 3: Period dates should be valid
                if (record.PERIOD_START_DATE.HasValue && record.PERIOD_END_DATE.HasValue)
                {
                    if (record.PERIOD_END_DATE <= record.PERIOD_START_DATE)
                    {
                        _logger?.LogWarning("Amortization record {RecordId}: Invalid period dates",
                            record.AMORTIZATION_RECORD_ID);
                        throw new AccountingException("Period end date must be after start date");
                    }
                }

                // Validation 4: Depletion shouldn't exceed capitalized costs
                if (record.AMORTIZATION_AMOUNT > record.NET_CAPITALIZED_COSTS)
                {
                    _logger?.LogWarning("Amortization record {RecordId}: Depletion exceeds capitalized costs",
                        record.AMORTIZATION_RECORD_ID);
                    throw new AccountingException("Depletion amount cannot exceed capitalized costs");
                }

                _logger?.LogInformation("Amortization record {RecordId} validation passed", record.AMORTIZATION_RECORD_ID);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Amortization validation failed");
                throw;
            }
        }

        /// <summary>
        /// Generates a depletion rollforward for a property over a period.
        /// Opening + Additions - Depletion - Impairment = Closing.
        /// </summary>
        public async Task<DEPLETION_ROLLFORWARD> GenerateRollforwardAsync(
            string propertyId,
            DateTime periodStart,
            DateTime periodEnd,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(propertyId))
                throw new ArgumentNullException(nameof(propertyId));
            if (periodEnd < periodStart)
                throw new ArgumentException("periodEnd must be >= periodStart", nameof(periodEnd));

            var openingCosts = await GetCapitalizedCostsAsync(propertyId, cn, periodStart);
            var openingDepletion = await GetDepletionTotalAsync(propertyId, cn, periodStart);
            var openingImpairment = await GetImpairmentTotalAsync(propertyId, cn, periodStart);
            var openingNet = openingCosts - openingDepletion - openingImpairment;

            var additions = await GetCapitalizedCostsAsync(propertyId, cn, periodStart, periodEnd);
            var depletion = await GetDepletionTotalAsync(propertyId, cn, periodEnd, periodStart);
            var impairment = await GetImpairmentTotalAsync(propertyId, cn, periodEnd, periodStart);
            var closingNet = openingNet + additions - depletion - impairment;

            var openingReserves = await GetReservesAsOfAsync(propertyId, periodStart, cn);
            var closingReserves = await GetReservesAsOfAsync(propertyId, periodEnd, cn);
            var reserveAdjustments = closingReserves - openingReserves;

            var rollforward = new DEPLETION_ROLLFORWARD
            {
                DEPLETION_ROLLFORWARD_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = propertyId,
                PERIOD_START_DATE = periodStart,
                PERIOD_END_DATE = periodEnd,
                OPENING_NET_CAPITALIZED = openingNet,
                ADDITIONS = additions,
                DEPLETION = depletion,
                IMPAIRMENT = impairment,
                CLOSING_NET_CAPITALIZED = closingNet,
                OPENING_RESERVES = openingReserves,
                RESERVE_ADJUSTMENTS = reserveAdjustments,
                CLOSING_RESERVES = closingReserves,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CREATED_BY = userId
            };

            var metadata = await _metadata.GetTableMetadataAsync("DEPLETION_ROLLFORWARD");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(DEPLETION_ROLLFORWARD);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "DEPLETION_ROLLFORWARD");

            await repo.InsertAsync(rollforward, userId);
            return rollforward;
        }

        private async Task<decimal> GetCapitalizedCostsAsync(string assetId, string cn = "PPDM39")
        {
            var metadata = await _metadata.GetTableMetadataAsync("ACCOUNTING_COST");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(ACCOUNTING_COST);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "ACCOUNTING_COST");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = assetId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var costs = await repo.GetAsync(filters);
            return costs?.Cast<ACCOUNTING_COST>().Sum(c => c.AMOUNT) ?? 0;
        }

        private async Task<decimal> GetCapitalizedCostsAsync(string propertyId, string cn, DateTime asOfDate)
        {
            return await GetCapitalizedCostsAsync(propertyId, cn, null, asOfDate);
        }

        private async Task<decimal> GetCapitalizedCostsAsync(string propertyId, string cn, DateTime? start, DateTime? end)
        {
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

            if (start.HasValue)
                filters.Add(new AppFilter { FieldName = "COST_DATE", Operator = ">=", FilterValue = start.Value.ToString("yyyy-MM-dd") });
            if (end.HasValue)
                filters.Add(new AppFilter { FieldName = "COST_DATE", Operator = "<=", FilterValue = end.Value.ToString("yyyy-MM-dd") });

            var costs = await repo.GetAsync(filters);
            return costs?.Cast<ACCOUNTING_COST>().Sum(c => c.AMOUNT) ?? 0;
        }

        private async Task<decimal> GetDepletionTotalAsync(string propertyId, string cn, DateTime asOfDate)
        {
            return await GetDepletionTotalAsync(propertyId, cn, null, asOfDate);
        }

        private async Task<decimal> GetDepletionTotalAsync(string propertyId, string cn, DateTime? start, DateTime? end)
        {
            var metadata = await _metadata.GetTableMetadataAsync("AMORTIZATION_RECORD");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(AMORTIZATION_RECORD);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "AMORTIZATION_RECORD");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (start.HasValue)
                filters.Add(new AppFilter { FieldName = "PERIOD_START_DATE", Operator = ">=", FilterValue = start.Value.ToString("yyyy-MM-dd") });
            if (end.HasValue)
                filters.Add(new AppFilter { FieldName = "PERIOD_END_DATE", Operator = "<=", FilterValue = end.Value.ToString("yyyy-MM-dd") });

            var records = await repo.GetAsync(filters);
            return records?.Cast<AMORTIZATION_RECORD>().Sum(r => r.AMORTIZATION_AMOUNT ?? 0m) ?? 0m;
        }

        private async Task<decimal> GetImpairmentTotalAsync(string propertyId, string cn, DateTime asOfDate)
        {
            return await GetImpairmentTotalAsync(propertyId, cn, null, asOfDate);
        }

        private async Task<decimal> GetImpairmentTotalAsync(string propertyId, string cn, DateTime? start, DateTime? end)
        {
            var metadata = await _metadata.GetTableMetadataAsync("IMPAIRMENT_RECORD");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(IMPAIRMENT_RECORD);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "IMPAIRMENT_RECORD");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (start.HasValue)
                filters.Add(new AppFilter { FieldName = "IMPAIRMENT_DATE", Operator = ">=", FilterValue = start.Value.ToString("yyyy-MM-dd") });
            if (end.HasValue)
                filters.Add(new AppFilter { FieldName = "IMPAIRMENT_DATE", Operator = "<=", FilterValue = end.Value.ToString("yyyy-MM-dd") });

            var records = await repo.GetAsync(filters);
            return records?.Cast<IMPAIRMENT_RECORD>().Sum(r => r.IMPAIRMENT_AMOUNT ?? 0m) ?? 0m;
        }

        private async Task<decimal> GetReservesAsOfAsync(string propertyId, DateTime asOfDate, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync("PROVED_RESERVES");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(PROVED_RESERVES);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "PROVED_RESERVES");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "RESERVE_DATE", Operator = "<=", FilterValue = asOfDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var reserves = await repo.GetAsync(filters);
            var reserveList = reserves?.Cast<PROVED_RESERVES>().ToList() ?? new List<PROVED_RESERVES>();
            if (!reserveList.Any())
                return 0m;

            var latestReserve = reserveList.OrderByDescending(r => r.RESERVE_DATE).FirstOrDefault();
            return (latestReserve?.PROVED_DEVELOPED_OIL_RESERVES ?? 0m) +
                   (latestReserve?.PROVED_UNDEVELOPED_OIL_RESERVES ?? 0m);
        }

        /// <summary>
        /// Gets total production for an asset during current period from MEASUREMENT_RECORD.
        /// </summary>
        private async Task<decimal> GetPeriodProductionAsync(string assetId, string cn)
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
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = assetId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var measurements = await repo.GetAsync(filters);
                var measurementList = measurements?.Cast<MEASUREMENT_RECORD>().ToList() ?? new List<MEASUREMENT_RECORD>();

                decimal totalProduction = measurementList.Sum(m => m.GROSS_VOLUME ?? 0);
                _logger?.LogDebug("Period production for asset {AssetId}: {Volume} BBL", assetId, totalProduction);

                return totalProduction;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error retrieving period production for asset {AssetId}", assetId);
                return 0;
            }
        }

        private async Task<decimal> GetFieldCapitalizedCostsAsync(string fieldId, string cn)
        {
            var repo = await CreateRepoAsync<ACCOUNTING_COST>("ACCOUNTING_COST", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var costs = await repo.GetAsync(filters);
            return costs?.Cast<ACCOUNTING_COST>().Sum(c => c.AMOUNT) ?? 0m;
        }

        private async Task<decimal> GetFieldProductionAsync(string fieldId, string cn)
        {
            var repo = await CreateRepoAsync<MEASUREMENT_RECORD>("MEASUREMENT_RECORD", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = fieldId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var records = await repo.GetAsync(filters);
            return records?.Cast<MEASUREMENT_RECORD>().Sum(m => m.NET_VOLUME ?? m.GROSS_VOLUME ?? 0m) ?? 0m;
        }

        private async Task<decimal> GetFieldReservesAsync(string fieldId, string cn)
        {
            var repo = await CreateRepoAsync<PROVED_RESERVES>("PROVED_RESERVES", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AREA_ID", Operator = "=", FilterValue = fieldId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var reserves = await repo.GetAsync(filters);
            var latest = reserves?.Cast<PROVED_RESERVES>().OrderByDescending(r => r.RESERVE_DATE).FirstOrDefault();
            if (latest == null)
                return 0m;

            return (latest.PROVED_DEVELOPED_OIL_RESERVES ?? 0m) +
                   (latest.PROVED_UNDEVELOPED_OIL_RESERVES ?? 0m) +
                   (latest.PROVED_DEVELOPED_GAS_RESERVES ?? 0m) +
                   (latest.PROVED_UNDEVELOPED_GAS_RESERVES ?? 0m);
        }

        private async Task<(decimal oilVolume, decimal gasVolume)> GetRevenueVolumesAsync(string propertyId, DateTime periodEnd, string cn)
        {
            var repo = await CreateRepoAsync<REVENUE_TRANSACTION>("REVENUE_TRANSACTION", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var revenues = results?.Cast<REVENUE_TRANSACTION>().ToList() ?? new List<REVENUE_TRANSACTION>();
            var oil = revenues.Sum(r => r.OIL_VOLUME ?? 0m);
            var gas = revenues.Sum(r => r.GAS_VOLUME ?? 0m);
            return (oil, gas);
        }

        private async Task<decimal> GetWorkingInterestAsync(string propertyId, string cn)
        {
            var repo = await CreateRepoAsync<OWNERSHIP_INTEREST>("OWNERSHIP_INTEREST", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_OR_LEASE_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var interests = results?.Cast<OWNERSHIP_INTEREST>().ToList() ?? new List<OWNERSHIP_INTEREST>();
            if (interests.Count == 0)
                return 1m;

            var wi = interests.Average(i => (i.WORKING_INTEREST ?? 100m) / 100m);
            if (wi <= 0m)
                wi = 1m;
            return wi;
        }

        private async Task<PPDMGenericRepository> CreateRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(T);

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, tableName);
        }

        /// <summary>
        /// Gets total proved reserves for an asset from PROVED_RESERVES table.
        /// </summary>
        private async Task<decimal> GetProvedReservesAsync(string assetId, string cn)
        {
            try
            {
                if (_reserveAccountingService != null)
                {
                    var latestReserves = await _reserveAccountingService.GetLatestReservesAsync(assetId, DateTime.UtcNow, cn);
                    if (latestReserves != null)
                    {
                        return (latestReserves.PROVED_DEVELOPED_OIL_RESERVES ?? 0) +
                               (latestReserves.PROVED_UNDEVELOPED_OIL_RESERVES ?? 0) +
                               (latestReserves.PROVED_DEVELOPED_GAS_RESERVES ?? 0) +
                               (latestReserves.PROVED_UNDEVELOPED_GAS_RESERVES ?? 0);
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
                    new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = assetId },
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

                    _logger?.LogDebug("Total proved reserves for asset {AssetId}: {Volume} BBL", assetId, totalReserves);
                    return totalReserves;
                }

                _logger?.LogWarning("No proved reserves found for asset {AssetId}", assetId);
                return 0;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error retrieving proved reserves for asset {AssetId}", assetId);
                return 0;
            }
        }
    }

}
