
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Financial.FullCost
{
    /// <summary>
    /// Provides Full Cost accounting calculations per industry practice.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class FullCostAccounting
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<FullCostAccounting>? _logger;
        private readonly string _connectionName;
        private const string COST_CENTER_TABLE = "COST_CENTER";
        private const string EXPLORATION_COSTS_TABLE = "EXPLORATION_COSTS";
        private const string DEVELOPMENT_COSTS_TABLE = "DEVELOPMENT_COSTS";
        private const string UNPROVED_PROPERTY_TABLE = "UNPROVED_PROPERTY";

        public FullCostAccounting(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<FullCostAccounting>? logger = null,
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
        /// Creates or gets a cost center.
        /// </summary>
        public COST_CENTER GetOrCreateCostCenter(string costCenterId, string name = "", string? connectionName = null)
        {
            if (string.IsNullOrEmpty(costCenterId))
                throw new ArgumentNullException(nameof(costCenterId));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Try to get existing cost center
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = costCenterId }
            };

            var results = dataSource.GetEntityAsync(COST_CENTER_TABLE, filters).GetAwaiter().GetResult();
            if (results != null && results.Any())
            {
                return results.OfType<COST_CENTER>().FirstOrDefault() ?? throw new InvalidOperationException($"Cost center {costCenterId} not found");
            }

            // Create new cost center
            var costCenter = new COST_CENTER
            {
                COST_CENTER_ID = costCenterId,
                COST_CENTER_NAME = name
            };
            if (costCenter is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForInsert(ppdmEntity, "SYSTEM");
            
            var result = dataSource.InsertEntity(COST_CENTER_TABLE, costCenter);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create cost center {CostCenterId}: {Error}", costCenterId, errorMessage);
                throw new InvalidOperationException($"Failed to save cost center: {errorMessage}");
            }

            _logger?.LogDebug("Created cost center {CostCenterId} in database", costCenterId);
            return costCenter;
        }

        /// <summary>
        /// Records exploration costs to a cost center. All costs are capitalized.
        /// </summary>
        public void RecordExplorationCosts(string costCenterId, ExplorationCostsDto costs, string? connectionName = null)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Ensure cost center exists
            GetOrCreateCostCenter(costCenterId, connectionName: connName);

            if (string.IsNullOrEmpty(costs.ExplorationCostId))
                costs.ExplorationCostId = Guid.NewGuid().ToString();

            // Convert DTO to Entity
            var entity = ConvertDtoToExplorationCostsEntity(costs);
            entity.COST_CENTER_ID = costCenterId;
            if (entity is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForInsert(ppdmEntity, "SYSTEM");

            var result = dataSource.InsertEntity(EXPLORATION_COSTS_TABLE, entity);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to record exploration costs {ExplorationCostId}: {Error}", costs.ExplorationCostId, errorMessage);
                throw new InvalidOperationException($"Failed to save exploration costs: {errorMessage}");
            }

            _logger?.LogDebug("Recorded exploration costs {ExplorationCostId} to cost center {CostCenterId} in database", 
                costs.ExplorationCostId, costCenterId);
        }

        /// <summary>
        /// Records development costs to a cost center. All costs are capitalized.
        /// </summary>
        public void RecordDevelopmentCosts(string costCenterId, DevelopmentCostsDto costs, string? connectionName = null)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Ensure cost center exists
            GetOrCreateCostCenter(costCenterId, connectionName: connName);

            if (string.IsNullOrEmpty(costs.DevelopmentCostId))
                costs.DevelopmentCostId = Guid.NewGuid().ToString();

            // Convert DTO to Entity
            var entity = ConvertDtoToDevelopmentCostsEntity(costs);
            entity.COST_CENTER_ID = costCenterId;
            if (entity is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForInsert(ppdmEntity, "SYSTEM");

            var result = dataSource.InsertEntity(DEVELOPMENT_COSTS_TABLE, entity);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to record development costs {DevelopmentCostId}: {Error}", costs.DevelopmentCostId, errorMessage);
                throw new InvalidOperationException($"Failed to save development costs: {errorMessage}");
            }

            _logger?.LogDebug("Recorded development costs {DevelopmentCostId} to cost center {CostCenterId} in database", 
                costs.DevelopmentCostId, costCenterId);
        }

        /// <summary>
        /// Records acquisition costs to a cost center. All costs are capitalized.
        /// </summary>
        public void RecordAcquisitionCosts(string costCenterId, UnprovedPropertyDto property, string? connectionName = null)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Ensure cost center exists
            GetOrCreateCostCenter(costCenterId, connectionName: connName);

            // Convert DTO to Entity
            var entity = ConvertDtoToUnprovedPropertyEntity(property);
            entity.COST_CENTER_ID = costCenterId;
            if (entity is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForInsert(ppdmEntity, "SYSTEM");

            var result = dataSource.InsertEntity(UNPROVED_PROPERTY_TABLE, entity);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to record acquisition costs for property {PropertyId}: {Error}", property.PropertyId, errorMessage);
                throw new InvalidOperationException($"Failed to save acquisition costs: {errorMessage}");
            }

            _logger?.LogDebug("Recorded acquisition costs for property {PropertyId} to cost center {CostCenterId} in database", 
                property.PropertyId, costCenterId);
        }

        /// <summary>
        /// Calculates total capitalized costs for a cost center.
        /// </summary>
        public decimal CalculateTotalCapitalizedCosts(string costCenterId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(costCenterId))
                return 0;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Get acquisition costs
            var acquisitionFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = costCenterId }
            };
            var acquisitionResults = dataSource.GetEntityAsync(UNPROVED_PROPERTY_TABLE, acquisitionFilters).GetAwaiter().GetResult();
            decimal acquisition = acquisitionResults != null && acquisitionResults.Any()
                ? acquisitionResults.OfType<UNPROVED_PROPERTY>()
                    .Where(p => p != null)
                    .Sum(p => p.ACQUISITION_COST ?? 0m)
                : 0m;

            // Get exploration costs
            var explorationFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = costCenterId }
            };
            var explorationResults = dataSource.GetEntityAsync(EXPLORATION_COSTS_TABLE, explorationFilters).GetAwaiter().GetResult();
            decimal exploration = explorationResults != null && explorationResults.Any()
                ? explorationResults.OfType<EXPLORATION_COSTS>()
                    .Where(c => c != null)
                    .Sum(c => c.TOTAL_EXPLORATION_COSTS ?? 0m)
                : 0m;

            // Get development costs
            var developmentFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COST_CENTER_ID", Operator = "=", FilterValue = costCenterId }
            };
            var developmentResults = dataSource.GetEntityAsync(DEVELOPMENT_COSTS_TABLE, developmentFilters).GetAwaiter().GetResult();
            decimal development = developmentResults != null && developmentResults.Any()
                ? developmentResults.OfType<DEVELOPMENT_COSTS>()
                    .Where(c => c != null)
                    .Sum(c => c.TOTAL_DEVELOPMENT_COSTS ?? 0m)
                : 0m;

            return acquisition + exploration + development;
        }

        /// <summary>
        /// Calculates amortization using units-of-production method.
        /// </summary>
        public decimal CalculateAmortization(string costCenterId, ProvedReservesDto reserves, ProductionDataDto production, string? connectionName = null)
        {
            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.ProspectIdentification;

            if (production == null)
                throw new ArgumentNullException(nameof(production));

            var connName = connectionName ?? _connectionName;
            decimal totalCapitalizedCosts = CalculateTotalCapitalizedCosts(costCenterId, connName);
            decimal accumulatedAmortization = GetAccumulatedAmortization(costCenterId, connName);
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
        /// Performs ceiling test to determine if impairment is needed.
        /// </summary>
        public CeilingTestResult PerformCeilingTest(string costCenterId, ProvedReservesDto reserves, decimal discountRate = 0.10m, string? connectionName = null)
        {
            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));

            var connName = connectionName ?? _connectionName;
            decimal totalCapitalizedCosts = CalculateTotalCapitalizedCosts(costCenterId, connName);
            decimal accumulatedAmortization = GetAccumulatedAmortization(costCenterId, connName);
            decimal netCapitalizedCosts = totalCapitalizedCosts - accumulatedAmortization;

            // Calculate present value of future net revenues
            decimal futureNetRevenues = CalculateFutureNetRevenues(reserves, discountRate);

            // Ceiling is the lower of: (1) net capitalized costs, (2) present value of future net revenues
            decimal ceiling = Math.Min(netCapitalizedCosts, futureNetRevenues);

            bool impairmentNeeded = netCapitalizedCosts > ceiling;
            decimal impairmentAmount = impairmentNeeded ? netCapitalizedCosts - ceiling : 0;

            return new CeilingTestResult
            {
                CostCenterId = costCenterId,
                NetCapitalizedCosts = netCapitalizedCosts,
                PresentValueOfFutureNetRevenues = futureNetRevenues,
                Ceiling = ceiling,
                ImpairmentNeeded = impairmentNeeded,
                ImpairmentAmount = impairmentAmount
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
        /// Gets accumulated amortization for a cost center.
        /// </summary>
        public decimal GetAccumulatedAmortization(string costCenterId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(costCenterId))
                return 0;

            var costCenter = GetOrCreateCostCenter(costCenterId, connectionName: connectionName);
            return costCenter.ACCUMULATED_AMORTIZATION ?? 0m;
        }

        /// <summary>
        /// Records amortization for a cost center.
        /// </summary>
        public void RecordAmortization(string costCenterId, decimal amortizationAmount, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var costCenter = GetOrCreateCostCenter(costCenterId, connectionName: connName);
            costCenter.ACCUMULATED_AMORTIZATION = (costCenter.ACCUMULATED_AMORTIZATION ?? 0m) + amortizationAmount;

            // Update in database
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            if (costCenter is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForUpdate(ppdmEntity, "SYSTEM");
            var result = dataSource.UpdateEntity(COST_CENTER_TABLE, costCenter);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to update amortization for cost center {CostCenterId}: {Error}", costCenterId, errorMessage);
                throw new InvalidOperationException($"Failed to update amortization: {errorMessage}");
            }

            _logger?.LogDebug("Recorded amortization {AmortizationAmount} for cost center {CostCenterId} in database", 
                amortizationAmount, costCenterId);
        }

        /// <summary>
        /// Gets all cost centers.
        /// </summary>
        public IEnumerable<COST_CENTER> GetCostCenters(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var results = dataSource.GetEntityAsync(COST_CENTER_TABLE, null).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return Enumerable.Empty<COST_CENTER>();

            return results.OfType<COST_CENTER>();
        }

        // Helper conversion methods: DTO to Entity
        private UNPROVED_PROPERTY ConvertDtoToUnprovedPropertyEntity(UnprovedPropertyDto dto)
        {
            return new UNPROVED_PROPERTY
            {
                PROPERTY_ID = dto.PropertyId,
                PROPERTY_NAME = dto.PropertyName,
                ACQUISITION_COST = dto.AcquisitionCost,
                ACQUISITION_DATE = dto.AcquisitionDate,
                PROPERTY_TYPE = dto.PropertyType.ToString(),
                WORKING_INTEREST = dto.WorkingInterest,
                NET_REVENUE_INTEREST = dto.NetRevenueInterest,
                ACCUMULATED_IMPAIRMENT = dto.AccumulatedImpairment,
                IS_PROVED = dto.IsProved ? "Y" : "N",
                PROVED_DATE = dto.ProvedDate
            };
        }

        private EXPLORATION_COSTS ConvertDtoToExplorationCostsEntity(ExplorationCostsDto dto)
        {
            return new EXPLORATION_COSTS
            {
                EXPLORATION_COST_ID = dto.ExplorationCostId,
                PROPERTY_ID = dto.PropertyId,
                GEOLOGICAL_GEOPHYSICAL_COSTS = dto.GeologicalGeophysicalCosts,
                EXPLORATORY_DRILLING_COSTS = dto.ExploratoryDrillingCosts,
                EXPLORATORY_WELL_EQUIPMENT = dto.ExploratoryWellEquipment,
                TOTAL_EXPLORATION_COSTS = dto.TotalExplorationCosts,
                COST_DATE = dto.CostDate,
                WELL_ID = dto.WellId,
                IS_DRY_HOLE = dto.IsDryHole ? "Y" : "N",
                FOUND_PROVED_RESERVES = dto.FoundProvedReserves ? "Y" : "N",
                IS_DEFERRED_CLASSIFICATION = dto.IsDeferredClassification ? "Y" : "N"
            };
        }

        private DEVELOPMENT_COSTS ConvertDtoToDevelopmentCostsEntity(DevelopmentCostsDto dto)
        {
            return new DEVELOPMENT_COSTS
            {
                DEVELOPMENT_COST_ID = dto.DevelopmentCostId,
                PROPERTY_ID = dto.PropertyId,
                DEVELOPMENT_WELL_DRILLING_COSTS = dto.DevelopmentWellDrillingCosts,
                DEVELOPMENT_WELL_EQUIPMENT = dto.DevelopmentWellEquipment,
                SUPPORT_EQUIPMENT_AND_FACILITIES = dto.SupportEquipmentAndFacilities,
                SERVICE_WELL_COSTS = dto.ServiceWellCosts,
                TOTAL_DEVELOPMENT_COSTS = dto.TotalDevelopmentCosts,
                COST_DATE = dto.CostDate
            };
        }
    }


    /// <summary>
    /// Represents the result of a ceiling test.
    /// </summary>
    public class CeilingTestResult
    {
        /// <summary>
        /// Gets or sets the cost center identifier.
        /// </summary>
        public string CostCenterId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the net capitalized costs.
        /// </summary>
        public decimal NetCapitalizedCosts { get; set; }

        /// <summary>
        /// Gets or sets the present value of future net revenues.
        /// </summary>
        public decimal PresentValueOfFutureNetRevenues { get; set; }

        /// <summary>
        /// Gets or sets the ceiling amount.
        /// </summary>
        public decimal Ceiling { get; set; }

        /// <summary>
        /// Gets or sets whether impairment is needed.
        /// </summary>
        public bool ImpairmentNeeded { get; set; }

        /// <summary>
        /// Gets or sets the impairment amount.
        /// </summary>
        public decimal ImpairmentAmount { get; set; }
    }
}
