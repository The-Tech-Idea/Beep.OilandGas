using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionAccounting.Financial.Amortization
{
    /// <summary>
    /// Service for amortization operations.
    /// Uses PPDMGenericRepository for database operations and calls AmortizationCalculator for calculations.
    /// </summary>
    public class AmortizationService : IAmortizationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<AmortizationService>? _logger;
        private readonly string _connectionName;
        private const string AMORTIZATION_RECORD_TABLE = "AMORTIZATION_RECORD";

        public AmortizationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<AmortizationService>? logger = null,
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
        /// Calculates and records amortization.
        /// </summary>
        public async Task<AMORTIZATION_RECORD> CalculateAndRecordAmortizationAsync(
            CalculateAmortizationRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.PropertyId) && string.IsNullOrEmpty(request.CostCenterId))
                throw new ArgumentException("Either PropertyId or CostCenterId must be provided.", nameof(request));

            // Calculate amortization using static calculator
            decimal amortizationAmount = AmortizationCalculator.CalculateUnitsOfProduction(
                request.NetCapitalizedCosts,
                request.TotalProvedReservesBOE,
                request.ProductionBOE);


            decimal amortizationRate = request.TotalProvedReservesBOE > 0
                ? request.ProductionBOE / request.TotalProvedReservesBOE
                : 0m;

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(AMORTIZATION_RECORD), connName, AMORTIZATION_RECORD_TABLE, null);

            var entity = new AMORTIZATION_RECORD
            {
                AMORTIZATION_RECORD_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = request.PropertyId,
                COST_CENTER_ID = request.CostCenterId,
                PERIOD_START_DATE = request.PeriodStartDate,
                PERIOD_END_DATE = request.PeriodEndDate,
                NET_CAPITALIZED_COSTS = request.NetCapitalizedCosts,
                TOTAL_RESERVES_BOE = request.TotalProvedReservesBOE,
                PRODUCTION_BOE = request.ProductionBOE,
                AMORTIZATION_AMOUNT = amortizationAmount,
                ACCOUNTING_METHOD = request.AccountingMethod,
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(entity);

            _logger?.LogDebug("Calculated and recorded amortization {Amount} for {Entity} {Id}",
                amortizationAmount,
                string.IsNullOrEmpty(request.PropertyId) ? "Cost Center" : "Property",
                request.PropertyId ?? request.CostCenterId);

            return entity;
        }

        /// <summary>
        /// Gets an amortization record by ID.
        /// </summary>
        public async Task<AMORTIZATION_RECORD?> GetAmortizationRecordAsync(
            string recordId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(recordId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(AMORTIZATION_RECORD), connName, AMORTIZATION_RECORD_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AMORTIZATION_RECORD_ID", Operator = "=", FilterValue = recordId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<AMORTIZATION_RECORD>().FirstOrDefault();
        }

        /// <summary>
        /// Gets amortization history for a property or cost center.
        /// </summary>
        public async Task<List<AMORTIZATION_RECORD>> GetAmortizationHistoryAsync(
            string? propertyId = null,
            string? costCenterId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(AMORTIZATION_RECORD), connName, AMORTIZATION_RECORD_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrEmpty(propertyId))
            {
                filters.Add(new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId });
            }

            if (!string.IsNullOrEmpty(costCenterId))
            {
                filters.Add(new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = costCenterId });
            }

            if (startDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "PERIOD_START_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });
            }

            if (endDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "PERIOD_END_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });
            }

            var results = await repo.GetAsync(filters);
            return results.Cast<AMORTIZATION_RECORD>().OrderBy(r => r.PERIOD_START_DATE).ToList();
        }

        /// <summary>
        /// Generates an amortization schedule.
        /// </summary>
        public async Task<AmortizationSchedule> GenerateAmortizationScheduleAsync(
            GenerateScheduleRequest request,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.PropertyId) && string.IsNullOrEmpty(request.CostCenterId))
                throw new ArgumentException("Either PropertyId or CostCenterId must be provided.", nameof(request));

            var connName = connectionName ?? _connectionName;

            // Get current amortization summary to get beginning balance
            var summary = await GetAmortizationSummaryAsync(request.PropertyId, request.CostCenterId, request.StartDate, connName);
            decimal beginningNetCapitalizedCosts = summary.NetCapitalizedCosts;
            decimal totalReservesBOE = summary.TotalReservesBOE;

            if (totalReservesBOE <= 0)
                throw new InsufficientReservesException("Total reserves must be greater than zero to generate schedule.");

            var schedule = new AmortizationSchedule
            {
                PropertyId = request.PropertyId,
                CostCenterId = request.CostCenterId,
                StartDate = request.StartDate,
                PeriodType = request.PeriodType,
                BeginningNetCapitalizedCosts = beginningNetCapitalizedCosts,
                TotalReservesBOE = totalReservesBOE,
                Periods = new List<AmortizationSchedulePeriod>()
            };

            decimal currentNetCapitalizedCosts = beginningNetCapitalizedCosts;
            DateTime currentDate = request.StartDate;

            int daysPerPeriod = request.PeriodType switch
            {
                "Monthly" => 30,
                "Quarterly" => 90,
                "Annual" => 365,
                _ => 30
            };

            for (int i = 1; i <= request.NumberOfPeriods; i++)
            {
                var periodStart = currentDate;
                var periodEnd = currentDate.AddDays(daysPerPeriod);

                if (currentNetCapitalizedCosts <= 0 || totalReservesBOE <= 0)
                    break;

                decimal amortizationRate = request.EstimatedProductionPerPeriod / totalReservesBOE;
                decimal projectedAmortization = AmortizationCalculator.CalculateUnitsOfProduction(
                    currentNetCapitalizedCosts,
                    totalReservesBOE,
                    request.EstimatedProductionPerPeriod);

                decimal endingNetCapitalizedCosts = currentNetCapitalizedCosts - projectedAmortization;
                totalReservesBOE -= request.EstimatedProductionPerPeriod;

                schedule.Periods.Add(new AmortizationSchedulePeriod
                {
                    PeriodNumber = i,
                    PeriodStartDate = periodStart,
                    PeriodEndDate = periodEnd,
                    BeginningNetCapitalizedCosts = currentNetCapitalizedCosts,
                    EstimatedProductionBOE = request.EstimatedProductionPerPeriod,
                    AmortizationRate = amortizationRate,
                    ProjectedAmortization = projectedAmortization,
                    EndingNetCapitalizedCosts = endingNetCapitalizedCosts
                });

                currentNetCapitalizedCosts = endingNetCapitalizedCosts;
                currentDate = periodEnd;
            }

            schedule.TotalProjectedAmortization = schedule.Periods.Sum(p => p.ProjectedAmortization);
            schedule.EndingNetCapitalizedCosts = schedule.Periods.LastOrDefault()?.EndingNetCapitalizedCosts ?? beginningNetCapitalizedCosts;

            _logger?.LogDebug("Generated amortization schedule with {PeriodCount} periods for {Entity} {Id}",
                schedule.Periods.Count,
                string.IsNullOrEmpty(request.PropertyId) ? "Cost Center" : "Property",
                request.PropertyId ?? request.CostCenterId);

            return schedule;
        }

        /// <summary>
        /// Gets amortization summary for a property or cost center.
        /// </summary>
        public async Task<AmortizationSummary> GetAmortizationSummaryAsync(
            string? propertyId = null,
            string? costCenterId = null,
            DateTime? asOfDate = null,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId) && string.IsNullOrEmpty(costCenterId))
                throw new ArgumentException("Either PropertyId or CostCenterId must be provided.");

            var connName = connectionName ?? _connectionName;
            var history = await GetAmortizationHistoryAsync(propertyId, costCenterId, null, asOfDate, connName);

            if (!history.Any())
            {
                return new AmortizationSummary
                {
                    PropertyId = propertyId,
                    CostCenterId = costCenterId,
                    AsOfDate = asOfDate ?? DateTime.UtcNow,
                    TotalCapitalizedCosts = 0m,
                    AccumulatedAmortization = 0m,
                    NetCapitalizedCosts = 0m,
                    TotalReservesBOE = 0m,
                    RemainingReservesBOE = 0m,
                    AmortizationRate = 0m,
                    NumberOfRecords = 0
                };
            }

            var latestRecord = history.OrderByDescending(r => r.PERIOD_END_DATE).First();
            decimal accumulatedAmortization = history.Sum(r => r.AMORTIZATION_AMOUNT ?? 0m);
            decimal totalCapitalizedCosts = (latestRecord.NET_CAPITALIZED_COSTS ?? 0m) + accumulatedAmortization;
            decimal netCapitalizedCosts = latestRecord.NET_CAPITALIZED_COSTS ?? 0m;
            decimal totalReservesBOE = latestRecord.TOTAL_RESERVES_BOE ?? 0m;
            decimal totalProductionBOE = history.Sum(r => r.PRODUCTION_BOE ?? 0m);
            decimal remainingReservesBOE = totalReservesBOE - totalProductionBOE;
            decimal amortizationRate = totalReservesBOE > 0 ? totalProductionBOE / totalReservesBOE : 0m;

            return new AmortizationSummary
            {
                PropertyId = propertyId,
                CostCenterId = costCenterId,
                AsOfDate = asOfDate ?? DateTime.UtcNow,
                TotalCapitalizedCosts = totalCapitalizedCosts,
                AccumulatedAmortization = accumulatedAmortization,
                NetCapitalizedCosts = netCapitalizedCosts,
                TotalReservesBOE = totalReservesBOE,
                RemainingReservesBOE = remainingReservesBOE,
                AmortizationRate = amortizationRate,
                NumberOfRecords = history.Count,
                FirstAmortizationDate = history.OrderBy(r => r.PERIOD_START_DATE).First().PERIOD_START_DATE,
                LastAmortizationDate = latestRecord.PERIOD_END_DATE
            };
        }
    }
}
