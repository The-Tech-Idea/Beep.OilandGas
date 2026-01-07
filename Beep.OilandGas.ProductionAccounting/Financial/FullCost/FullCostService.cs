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
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionAccounting.Financial.FullCost
{
    /// <summary>
    /// Service for Full Cost accounting operations per industry practice.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class FullCostService : IFullCostService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<FullCostService>? _logger;
        private readonly string _connectionName;
        private const string COST_CENTER_TABLE = "COST_CENTER";
        private const string EXPLORATION_COSTS_TABLE = "EXPLORATION_COSTS";
        private const string DEVELOPMENT_COSTS_TABLE = "DEVELOPMENT_COSTS";
        private const string UNPROVED_PROPERTY_TABLE = "UNPROVED_PROPERTY";
        private const string CEILING_TEST_CALCULATION_TABLE = "CEILING_TEST_CALCULATION";

        public FullCostService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<FullCostService>? logger = null,
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
        /// Records exploration costs to a cost center. All costs are capitalized.
        /// </summary>
        public async Task<EXPLORATION_COSTS> RecordExplorationCostsAsync(
            string propertyId,
            string costCenterId,
            ExplorationCostsDto costs,
            string userId,
            string? connectionName = null)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));
            if (string.IsNullOrEmpty(costCenterId))
                throw new ArgumentException("Cost center ID is required.", nameof(costCenterId));

            var connName = connectionName ?? _connectionName;

            // Ensure cost center exists
            await GetOrCreateCostCenterAsync(costCenterId, connName);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(EXPLORATION_COSTS), connName, EXPLORATION_COSTS_TABLE, null);

            if (string.IsNullOrEmpty(costs.ExplorationCostId))
                costs.ExplorationCostId = Guid.NewGuid().ToString();

            var entity = new EXPLORATION_COSTS
            {
                EXPLORATION_COST_ID = costs.ExplorationCostId,
                PROPERTY_ID = propertyId,
                COST_CENTER_ID = costCenterId,
                GEOLOGICAL_GEOPHYSICAL_COSTS = costs.GeologicalGeophysicalCosts,
                EXPLORATORY_DRILLING_COSTS = costs.ExploratoryDrillingCosts,
                EXPLORATORY_WELL_EQUIPMENT = costs.ExploratoryWellEquipment,
                TOTAL_EXPLORATION_COSTS = costs.TotalExplorationCosts,
                COST_DATE = costs.CostDate,
                WELL_ID = costs.WellId,
                IS_DRY_HOLE = costs.IsDryHole ? "Y" : "N",
                FOUND_PROVED_RESERVES = costs.FoundProvedReserves ? "Y" : "N",
                IS_DEFERRED_CLASSIFICATION = costs.IsDeferredClassification ? "Y" : "N",
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(entity);

            _logger?.LogDebug("Recorded exploration costs {ExplorationCostId} to cost center {CostCenterId}",
                costs.ExplorationCostId, costCenterId);

            return entity;
        }

        /// <summary>
        /// Records development costs to a cost center. All costs are capitalized.
        /// </summary>
        public async Task<DEVELOPMENT_COSTS> RecordDevelopmentCostsAsync(
            string propertyId,
            string costCenterId,
            DevelopmentCostsDto costs,
            string userId,
            string? connectionName = null)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));
            if (string.IsNullOrEmpty(costCenterId))
                throw new ArgumentException("Cost center ID is required.", nameof(costCenterId));

            var connName = connectionName ?? _connectionName;

            // Ensure cost center exists
            await GetOrCreateCostCenterAsync(costCenterId, connName);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(DEVELOPMENT_COSTS), connName, DEVELOPMENT_COSTS_TABLE, null);

            if (string.IsNullOrEmpty(costs.DevelopmentCostId))
                costs.DevelopmentCostId = Guid.NewGuid().ToString();

            var entity = new DEVELOPMENT_COSTS
            {
                DEVELOPMENT_COST_ID = costs.DevelopmentCostId,
                PROPERTY_ID = propertyId,
                COST_CENTER_ID = costCenterId,
                DEVELOPMENT_WELL_DRILLING_COSTS = costs.DevelopmentWellDrillingCosts,
                DEVELOPMENT_WELL_EQUIPMENT = costs.DevelopmentWellEquipment,
                SUPPORT_EQUIPMENT_AND_FACILITIES = costs.SupportEquipmentAndFacilities,
                SERVICE_WELL_COSTS = costs.ServiceWellCosts,
                TOTAL_DEVELOPMENT_COSTS = costs.TotalDevelopmentCosts,
                COST_DATE = costs.CostDate,
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(entity);

            _logger?.LogDebug("Recorded development costs {DevelopmentCostId} to cost center {CostCenterId}",
                costs.DevelopmentCostId, costCenterId);

            return entity;
        }

        /// <summary>
        /// Records acquisition costs to a cost center. All costs are capitalized.
        /// </summary>
        public async Task<UNPROVED_PROPERTY> RecordAcquisitionCostsAsync(
            string costCenterId,
            UnprovedPropertyDto property,
            string userId,
            string? connectionName = null)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            if (string.IsNullOrEmpty(costCenterId))
                throw new ArgumentException("Cost center ID is required.", nameof(costCenterId));

            var connName = connectionName ?? _connectionName;

            // Ensure cost center exists
            await GetOrCreateCostCenterAsync(costCenterId, connName);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(UNPROVED_PROPERTY), connName, UNPROVED_PROPERTY_TABLE, null);

            var entity = new UNPROVED_PROPERTY
            {
                PROPERTY_ID = property.PropertyId,
                COST_CENTER_ID = costCenterId,
                PROPERTY_NAME = property.PropertyName,
                ACQUISITION_COST = property.AcquisitionCost,
                ACQUISITION_DATE = property.AcquisitionDate,
                PROPERTY_TYPE = property.PropertyType.ToString(),
                WORKING_INTEREST = property.WorkingInterest,
                NET_REVENUE_INTEREST = property.NetRevenueInterest,
                ACCUMULATED_IMPAIRMENT = property.AccumulatedImpairment,
                IS_PROVED = property.IsProved ? "Y" : "N",
                PROVED_DATE = property.ProvedDate,
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(entity);

            _logger?.LogDebug("Recorded acquisition costs for property {PropertyId} to cost center {CostCenterId}",
                property.PropertyId, costCenterId);

            return entity;
        }

        /// <summary>
        /// Creates a cost center.
        /// </summary>
        public async Task<COST_CENTER> CreateCostCenterAsync(
            CreateCostCenterRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.CostCenterId))
                throw new ArgumentException("Cost center ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(COST_CENTER), connName, COST_CENTER_TABLE, null);

            // Check if cost center already exists
            var existingFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = request.CostCenterId }
            };
            var existing = await repo.GetAsync(existingFilters);
            if (existing.Any())
            {
                return existing.Cast<COST_CENTER>().First();
            }

            var costCenter = new COST_CENTER
            {
                COST_CENTER_ID = request.CostCenterId,
                COST_CENTER_NAME = request.CostCenterName,
                ACCOUNTING_METHOD = "Full Cost",
                ACTIVE_IND = "Y"
            };

            if (costCenter is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(costCenter);

            _logger?.LogDebug("Created cost center {CostCenterId}", request.CostCenterId);
            return costCenter;
        }

        /// <summary>
        /// Gets a cost center by ID.
        /// </summary>
        public async Task<COST_CENTER?> GetCostCenterAsync(string costCenterId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(costCenterId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(COST_CENTER), connName, COST_CENTER_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = costCenterId }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<COST_CENTER>().FirstOrDefault();
        }

        /// <summary>
        /// Gets all cost centers.
        /// </summary>
        public async Task<List<COST_CENTER>> GetCostCentersAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(COST_CENTER), connName, COST_CENTER_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<COST_CENTER>().ToList();
        }

        /// <summary>
        /// Performs ceiling test to determine if impairment is needed.
        /// </summary>
        public async Task<CEILING_TEST_CALCULATION> PerformCeilingTestAsync(
            string costCenterId,
            DateTime testDate,
            ProvedReservesDto reserves,
            decimal discountRate,
            string userId,
            string? connectionName = null)
        {
            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));

            var connName = connectionName ?? _connectionName;
            decimal totalCapitalizedCosts = await CalculateTotalCapitalizedCostsAsync(costCenterId, connName);
            decimal accumulatedAmortization = await GetAccumulatedAmortizationAsync(costCenterId, connName);
            decimal netCapitalizedCosts = totalCapitalizedCosts - accumulatedAmortization;

            // Calculate present value of future net revenues
            decimal futureNetRevenues = CalculateFutureNetRevenues(reserves, discountRate);

            // Ceiling is the lower of: (1) net capitalized costs, (2) present value of future net revenues
            decimal ceiling = Math.Min(netCapitalizedCosts, futureNetRevenues);
            bool impairmentNeeded = netCapitalizedCosts > ceiling;
            decimal impairmentAmount = impairmentNeeded ? netCapitalizedCosts - ceiling : 0;

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(CEILING_TEST_CALCULATION), connName, CEILING_TEST_CALCULATION_TABLE, null);

            var entity = new CEILING_TEST_CALCULATION
            {
                CEILING_TEST_CALCULATION_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = costCenterId, // Using PROPERTY_ID to store cost center ID
                CALCULATION_DATE = testDate,
                NET_CAPITALIZED_COST = netCapitalizedCosts,
                DISCOUNTED_FUTURE_NET_CASH_FLOWS = futureNetRevenues,
                CEILING_VALUE = ceiling,
                IMPAIRMENT_AMOUNT = impairmentAmount,
                DISCOUNT_RATE = discountRate,
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(entity);

            _logger?.LogDebug("Performed ceiling test for cost center {CostCenterId}, impairment needed: {ImpairmentNeeded}",
                costCenterId, impairmentNeeded);

            return entity;
        }

        /// <summary>
        /// Gets ceiling test history for a cost center.
        /// </summary>
        public async Task<List<CEILING_TEST_CALCULATION>> GetCeilingTestHistoryAsync(
            string costCenterId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(costCenterId))
                return new List<CEILING_TEST_CALCULATION>();

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(CEILING_TEST_CALCULATION), connName, CEILING_TEST_CALCULATION_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = costCenterId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<CEILING_TEST_CALCULATION>().OrderByDescending(c => c.CALCULATION_DATE).ToList();
        }

        /// <summary>
        /// Calculates total capitalized costs for a cost center.
        /// </summary>
        public async Task<decimal> CalculateTotalCapitalizedCostsAsync(string costCenterId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(costCenterId))
                return 0;

            var connName = connectionName ?? _connectionName;

            // Get acquisition costs
            var unprovedRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(UNPROVED_PROPERTY), connName, UNPROVED_PROPERTY_TABLE, null);

            var acquisitionFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = costCenterId }
            };
            var acquisitionResults = await unprovedRepo.GetAsync(acquisitionFilters);
            decimal acquisition = acquisitionResults
                .Cast<UNPROVED_PROPERTY>()
                .Where(p => p != null)
                .Sum(p => p.ACQUISITION_COST ?? 0m);

            // Get exploration costs
            var explorationRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(EXPLORATION_COSTS), connName, EXPLORATION_COSTS_TABLE, null);

            var explorationFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = costCenterId }
            };
            var explorationResults = await explorationRepo.GetAsync(explorationFilters);
            decimal exploration = explorationResults
                .Cast<EXPLORATION_COSTS>()
                .Where(c => c != null)
                .Sum(c => c.TOTAL_EXPLORATION_COSTS ?? 0m);

            // Get development costs
            var developmentRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(DEVELOPMENT_COSTS), connName, DEVELOPMENT_COSTS_TABLE, null);

            var developmentFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = costCenterId }
            };
            var developmentResults = await developmentRepo.GetAsync(developmentFilters);
            decimal development = developmentResults
                .Cast<DEVELOPMENT_COSTS>()
                .Where(c => c != null)
                .Sum(c => c.TOTAL_DEVELOPMENT_COSTS ?? 0m);

            return acquisition + exploration + development;
        }

        /// <summary>
        /// Gets accumulated amortization for a cost center.
        /// </summary>
        public async Task<decimal> GetAccumulatedAmortizationAsync(string costCenterId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(costCenterId))
                return 0;

            var costCenter = await GetCostCenterAsync(costCenterId, connectionName);
            return costCenter?.ACCUMULATED_AMORTIZATION ?? 0m;
        }

        /// <summary>
        /// Records amortization for a cost center.
        /// </summary>
        public async Task<COST_CENTER> RecordAmortizationAsync(
            string costCenterId,
            decimal amortizationAmount,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(costCenterId))
                throw new ArgumentException("Cost center ID is required.", nameof(costCenterId));

            var connName = connectionName ?? _connectionName;
            var costCenter = await GetOrCreateCostCenterAsync(costCenterId, connName);

            costCenter.ACCUMULATED_AMORTIZATION = (costCenter.ACCUMULATED_AMORTIZATION ?? 0m) + amortizationAmount;

            if (costCenter is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(ppdmEntity, userId);
            }

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(COST_CENTER), connName, COST_CENTER_TABLE, null);

            await repo.UpdateAsync(costCenter);

            _logger?.LogDebug("Recorded amortization {AmortizationAmount} for cost center {CostCenterId}",
                amortizationAmount, costCenterId);

            return costCenter;
        }

        /// <summary>
        /// Calculates amortization using units-of-production method.
        /// </summary>
        public async Task<decimal> CalculateAmortizationAsync(
            string costCenterId,
            ProvedReservesDto reserves,
            ProductionDataDto production,
            string? connectionName = null)
        {
            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));

            if (production == null)
                throw new ArgumentNullException(nameof(production));

            var connName = connectionName ?? _connectionName;
            decimal totalCapitalizedCosts = await CalculateTotalCapitalizedCostsAsync(costCenterId, connName);
            decimal accumulatedAmortization = await GetAccumulatedAmortizationAsync(costCenterId, connName);
            decimal netCapitalizedCosts = totalCapitalizedCosts - accumulatedAmortization;

            if (netCapitalizedCosts <= 0)
                return 0;

            // Calculate total reserves in BOE
            decimal totalReservesBOE = reserves.TotalProvedOilReserves +
                (reserves.TotalProvedGasReserves / AccountingConstants.GasToOilEquivalent);

            if (totalReservesBOE <= 0)
                throw new InsufficientReservesException("Total reserves must be greater than zero for amortization.");

            // Calculate production in BOE
            decimal productionBOE = production.OilProduction +
                (production.GasProduction / AccountingConstants.GasToOilEquivalent);

            // Units-of-production amortization
            decimal amortizationRate = productionBOE / totalReservesBOE;
            decimal amortization = netCapitalizedCosts * amortizationRate;

            return amortization;
        }

        /// <summary>
        /// Gets cost center rollup summary.
        /// </summary>
        public async Task<CostCenterRollup> GetCostCenterRollupAsync(
            string costCenterId,
            DateTime? asOfDate,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(costCenterId))
                throw new ArgumentException("Cost center ID is required.", nameof(costCenterId));

            var connName = connectionName ?? _connectionName;
            var costCenter = await GetCostCenterAsync(costCenterId, connName);
            if (costCenter == null)
                throw new InvalidOperationException($"Cost center {costCenterId} not found.");

            // Get costs by category
            var unprovedRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(UNPROVED_PROPERTY), connName, UNPROVED_PROPERTY_TABLE, null);

            var acquisitionFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = costCenterId }
            };
            var acquisitionResults = await unprovedRepo.GetAsync(acquisitionFilters);
            decimal acquisitionCosts = acquisitionResults
                .Cast<UNPROVED_PROPERTY>()
                .Sum(p => p.ACQUISITION_COST ?? 0m);

            var explorationRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(EXPLORATION_COSTS), connName, EXPLORATION_COSTS_TABLE, null);

            var explorationFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = costCenterId }
            };
            var explorationResults = await explorationRepo.GetAsync(explorationFilters);
            decimal explorationCosts = explorationResults
                .Cast<EXPLORATION_COSTS>()
                .Sum(c => c.TOTAL_EXPLORATION_COSTS ?? 0m);

            var developmentRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(DEVELOPMENT_COSTS), connName, DEVELOPMENT_COSTS_TABLE, null);

            var developmentFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = costCenterId }
            };
            var developmentResults = await developmentRepo.GetAsync(developmentFilters);
            decimal developmentCosts = developmentResults
                .Cast<DEVELOPMENT_COSTS>()
                .Sum(c => c.TOTAL_DEVELOPMENT_COSTS ?? 0m);

            decimal totalCapitalizedCosts = acquisitionCosts + explorationCosts + developmentCosts;
            decimal accumulatedAmortization = await GetAccumulatedAmortizationAsync(costCenterId, connName);
            decimal netCapitalizedCosts = totalCapitalizedCosts - accumulatedAmortization;

            return new CostCenterRollup
            {
                CostCenterId = costCenterId,
                CostCenterName = costCenter.COST_CENTER_NAME ?? string.Empty,
                AcquisitionCosts = acquisitionCosts,
                ExplorationCosts = explorationCosts,
                DevelopmentCosts = developmentCosts,
                TotalCapitalizedCosts = totalCapitalizedCosts,
                AccumulatedAmortization = accumulatedAmortization,
                NetCapitalizedCosts = netCapitalizedCosts,
                AsOfDate = asOfDate ?? DateTime.UtcNow
            };
        }

        /// <summary>
        /// Calculates impairment based on ceiling test.
        /// </summary>
        public async Task<ImpairmentResult> CalculateImpairmentAsync(
            string costCenterId,
            DateTime testDate,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(costCenterId))
                throw new ArgumentException("Cost center ID is required.", nameof(costCenterId));

            var connName = connectionName ?? _connectionName;
            var costCenter = await GetCostCenterAsync(costCenterId, connName);
            if (costCenter == null)
                throw new InvalidOperationException($"Cost center {costCenterId} not found.");

            // Get most recent ceiling test
            var ceilingTests = await GetCeilingTestHistoryAsync(costCenterId, connName);
            var latestTest = ceilingTests.FirstOrDefault();

            if (latestTest == null)
            {
                return new ImpairmentResult
                {
                    PropertyId = costCenterId,
                    PropertyName = costCenter.COST_CENTER_NAME ?? string.Empty,
                    AcquisitionCost = 0m,
                    AccumulatedImpairment = 0m,
                    CalculatedImpairment = 0m,
                    ImpairmentRequired = false,
                    ImpairmentReason = "No ceiling test performed",
                    TestDate = testDate
                };
            }

            return new ImpairmentResult
            {
                PropertyId = costCenterId,
                PropertyName = costCenter.COST_CENTER_NAME ?? string.Empty,
                AcquisitionCost = latestTest.NET_CAPITALIZED_COST ?? 0m,
                AccumulatedImpairment = 0m, // Full Cost doesn't track accumulated impairment separately
                CalculatedImpairment = latestTest.IMPAIRMENT_AMOUNT ?? 0m,
                ImpairmentRequired = (latestTest.IMPAIRMENT_AMOUNT ?? 0m) > 0.01m,
                ImpairmentReason = latestTest.IMPAIRMENT_AMOUNT > 0 ? "Ceiling test exceeded" : "No impairment needed",
                TestDate = latestTest.CALCULATION_DATE ?? testDate
            };
        }

        /// <summary>
        /// Calculates present value of future net revenues.
        /// </summary>
        private decimal CalculateFutureNetRevenues(ProvedReservesDto reserves, decimal discountRate)
        {
            // Simplified calculation - full implementation would use production forecast
            decimal oilRevenue = reserves.TotalProvedOilReserves * reserves.OilPrice;
            decimal gasRevenue = (reserves.TotalProvedGasReserves / AccountingConstants.GasToOilEquivalent) * reserves.GasPrice;
            decimal totalRevenue = oilRevenue + gasRevenue;

            // Apply discount (simplified - assumes immediate production)
            // Full implementation would discount over production period
            decimal presentValue = totalRevenue / (1 + discountRate);
            return presentValue;
        }

        /// <summary>
        /// Gets or creates a cost center.
        /// </summary>
        private async Task<COST_CENTER> GetOrCreateCostCenterAsync(string costCenterId, string connectionName)
        {
            var costCenter = await GetCostCenterAsync(costCenterId, connectionName);
            if (costCenter != null)
                return costCenter;

            var request = new CreateCostCenterRequest
            {
                CostCenterId = costCenterId,
                CostCenterName = costCenterId
            };

            return await CreateCostCenterAsync(request, "SYSTEM", connectionName);
        }
    }
}
