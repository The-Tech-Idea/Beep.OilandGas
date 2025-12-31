
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Financial.SuccessfulEfforts
{
    /// <summary>
    /// Provides Successful Efforts accounting calculations per FASB Statement No. 19.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class SuccessfulEffortsAccounting
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<SuccessfulEffortsAccounting>? _logger;
        private readonly string _connectionName;
        private const string UNPROVED_PROPERTY_TABLE = "UNPROVED_PROPERTY";
        private const string PROVED_PROPERTY_TABLE = "PROVED_PROPERTY";
        private const string EXPLORATION_COSTS_TABLE = "EXPLORATION_COSTS";
        private const string DEVELOPMENT_COSTS_TABLE = "DEVELOPMENT_COSTS";
        private const string PRODUCTION_COSTS_TABLE = "PRODUCTION_COSTS";

        public SuccessfulEffortsAccounting(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<SuccessfulEffortsAccounting>? logger = null,
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
        /// Records acquisition of an unproved property.
        /// </summary>
        public void RecordAcquisition(UnprovedPropertyDto property, string? connectionName = null)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (string.IsNullOrEmpty(property.PropertyId))
                throw new InvalidAccountingDataException(nameof(property.PropertyId), "Property ID cannot be null or empty.");

            if (property.AcquisitionCost < AccountingConstants.MinCost)
                throw new InvalidAccountingDataException(nameof(property.AcquisitionCost), "Acquisition cost cannot be negative.");

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var entity = ConvertDtoToUnprovedPropertyEntity(property);
            if (entity is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForInsert(ppdmEntity, "SYSTEM");
            
            var result = dataSource.InsertEntity(UNPROVED_PROPERTY_TABLE, entity);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to record unproved property {PropertyId}: {Error}", property.PropertyId, errorMessage);
                throw new InvalidOperationException($"Failed to save unproved property: {errorMessage}");
            }

            _logger?.LogDebug("Recorded unproved property {PropertyId} in database", property.PropertyId);
        }

        /// <summary>
        /// Records exploration costs. G&G costs are expensed, drilling costs are capitalized.
        /// </summary>
        public void RecordExplorationCosts(ExplorationCostsDto costs, string? connectionName = null)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (string.IsNullOrEmpty(costs.PropertyId))
                throw new InvalidAccountingDataException(nameof(costs.PropertyId), "Property ID cannot be null or empty.");

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            if (string.IsNullOrEmpty(costs.ExplorationCostId))
                costs.ExplorationCostId = Guid.NewGuid().ToString();

            var entity = ConvertDtoToExplorationCostsEntity(costs);
            if (entity is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForInsert(ppdmEntity, "SYSTEM");
            
            var result = dataSource.InsertEntity(EXPLORATION_COSTS_TABLE, entity);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to record exploration costs {ExplorationCostId}: {Error}", costs.ExplorationCostId, errorMessage);
                throw new InvalidOperationException($"Failed to save exploration costs: {errorMessage}");
            }

            _logger?.LogDebug("Recorded exploration costs {ExplorationCostId} for property {PropertyId} in database", 
                costs.ExplorationCostId, costs.PropertyId);
        }

        /// <summary>
        /// Records development costs. All development costs are capitalized.
        /// </summary>
        public void RecordDevelopmentCosts(DevelopmentCostsDto costs, string? connectionName = null)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (string.IsNullOrEmpty(costs.PropertyId))
                throw new InvalidAccountingDataException(nameof(costs.PropertyId), "Property ID cannot be null or empty.");

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            if (string.IsNullOrEmpty(costs.DevelopmentCostId))
                costs.DevelopmentCostId = Guid.NewGuid().ToString();

            var entity = ConvertDtoToDevelopmentCostsEntity(costs);
            if (entity is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForInsert(ppdmEntity, "SYSTEM");
            
            var result = dataSource.InsertEntity(DEVELOPMENT_COSTS_TABLE, entity);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to record development costs {DevelopmentCostId}: {Error}", costs.DevelopmentCostId, errorMessage);
                throw new InvalidOperationException($"Failed to save development costs: {errorMessage}");
            }

            _logger?.LogDebug("Recorded development costs {DevelopmentCostId} for property {PropertyId} in database", 
                costs.DevelopmentCostId, costs.PropertyId);
        }

        /// <summary>
        /// Records production costs (lifting costs). These are expensed as incurred.
        /// </summary>
        public void RecordProductionCosts(ProductionCostsDto costs, string? connectionName = null)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (string.IsNullOrEmpty(costs.PropertyId))
                throw new InvalidAccountingDataException(nameof(costs.PropertyId), "Property ID cannot be null or empty.");

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            if (string.IsNullOrEmpty(costs.ProductionCostId))
                costs.ProductionCostId = Guid.NewGuid().ToString();

            var entity = ConvertDtoToProductionCostsEntity(costs);
            if (entity is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForInsert(ppdmEntity, "SYSTEM");
            
            var result = dataSource.InsertEntity(PRODUCTION_COSTS_TABLE, entity);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to record production costs {ProductionCostId}: {Error}", costs.ProductionCostId, errorMessage);
                throw new InvalidOperationException($"Failed to save production costs: {errorMessage}");
            }

            _logger?.LogDebug("Recorded production costs {ProductionCostId} for property {PropertyId} in database", 
                costs.ProductionCostId, costs.PropertyId);
        }

        /// <summary>
        /// Records a dry hole expense for an exploratory well.
        /// </summary>
        public void RecordDryHole(ExplorationCostsDto costs, string? connectionName = null)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            costs.IsDryHole = true;
            costs.FoundProvedReserves = false;

            // All costs of dry hole are expensed
            RecordExplorationCosts(costs, connectionName);
        }

        /// <summary>
        /// Classifies an unproved property as proved when reserves are discovered.
        /// </summary>
        public void ClassifyAsProved(UnprovedPropertyDto unprovedProperty, ProvedReservesDto reserves, string? connectionName = null)
        {
            if (unprovedProperty == null)
                throw new ArgumentNullException(nameof(unprovedProperty));

            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Verify property exists
            var existingProperty = GetUnprovedProperties(connName).FirstOrDefault(p => p.PropertyId == unprovedProperty.PropertyId);
            if (existingProperty == null)
                throw new InvalidAccountingDataException(nameof(unprovedProperty), "Property not found in unproved properties.");

            // Create proved property entity
            var provedEntity = new PROVED_PROPERTY
            {
                PROPERTY_ID = unprovedProperty.PropertyId,
                ACQUISITION_COST = unprovedProperty.AcquisitionCost,
                EXPLORATION_COSTS = GetTotalExplorationCosts(unprovedProperty.PropertyId, connName),
                DEVELOPMENT_COSTS = GetTotalDevelopmentCosts(unprovedProperty.PropertyId, connName),
                PROVED_DATE = DateTime.Now
            };
            if (provedEntity is IPPDMEntity provedPpdmEntity)
                _commonColumnHandler.PrepareForInsert(provedPpdmEntity, "SYSTEM");

            // Save proved property
            var result = dataSource.InsertEntity(PROVED_PROPERTY_TABLE, provedEntity);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to classify property {PropertyId} as proved: {Error}", unprovedProperty.PropertyId, errorMessage);
                throw new InvalidOperationException($"Failed to save proved property: {errorMessage}");
            }

            // Update unproved property
            unprovedProperty.IsProved = true;
            unprovedProperty.ProvedDate = DateTime.Now;
            var unprovedEntity = ConvertDtoToUnprovedPropertyEntity(unprovedProperty);
            if (unprovedEntity is IPPDMEntity unprovedPpdmEntity)
                _commonColumnHandler.PrepareForUpdate(unprovedPpdmEntity, "SYSTEM");
            
            var updateResult = dataSource.UpdateEntity(UNPROVED_PROPERTY_TABLE, unprovedEntity);
            
            if (updateResult != null && updateResult.Errors != null && updateResult.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", updateResult.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to update unproved property {PropertyId}: {Error}", unprovedProperty.PropertyId, errorMessage);
                throw new InvalidOperationException($"Failed to update unproved property: {errorMessage}");
            }

            _logger?.LogDebug("Classified property {PropertyId} as proved in database", unprovedProperty.PropertyId);
        }

        /// <summary>
        /// Records impairment of an unproved property.
        /// </summary>
        public void RecordImpairment(string propertyId, decimal impairmentAmount, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                throw new ArgumentNullException(nameof(propertyId));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var propertyDto = GetUnprovedProperties(connName).FirstOrDefault(p => p.PropertyId == propertyId);
            if (propertyDto == null)
                throw new InvalidAccountingDataException(nameof(propertyId), "Property not found.");

            propertyDto.AccumulatedImpairment += impairmentAmount;

            // Impairment cannot exceed acquisition cost
            if (propertyDto.AccumulatedImpairment > propertyDto.AcquisitionCost)
                propertyDto.AccumulatedImpairment = propertyDto.AcquisitionCost;

            // Update in database
            var entity = ConvertDtoToUnprovedPropertyEntity(propertyDto);
            if (entity is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForUpdate(ppdmEntity, "SYSTEM");
            var result = dataSource.UpdateEntity(UNPROVED_PROPERTY_TABLE, entity);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to update impairment for property {PropertyId}: {Error}", propertyId, errorMessage);
                throw new InvalidOperationException($"Failed to update impairment: {errorMessage}");
            }

            _logger?.LogDebug("Recorded impairment {ImpairmentAmount} for property {PropertyId} in database", impairmentAmount, propertyId);
        }

        /// <summary>
        /// Calculates amortization for a proved property using units-of-production method.
        /// </summary>
        public decimal CalculateAmortization(ProvedPropertyDto property, ProvedReservesDto reserves, ProductionDataDto production)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));

            if (production == null)
                throw new ArgumentNullException(nameof(production));

            // Calculate net capitalized costs
            decimal netCapitalizedCosts = property.AcquisitionCost + 
                                         property.ExplorationCosts + 
                                         property.DevelopmentCosts - 
                                         property.AccumulatedAmortization;

            if (netCapitalizedCosts <= 0)
                return 0;

            // Calculate total reserves in BOE
            decimal totalReservesBOE = reserves.TotalProvedOilReserves + 
                                      (reserves.TotalProvedGasReserves / AccountingConstants.GasToOilEquivalent);

            if (totalReservesBOE <= 0)
                return 0;

            // Calculate production in BOE
            decimal productionBOE = production.OilProduction + 
                                   (production.GasProduction / AccountingConstants.GasToOilEquivalent);

            // Units-of-production amortization
            decimal amortizationRate = productionBOE / totalReservesBOE;
            decimal amortization = netCapitalizedCosts * amortizationRate;

            return amortization;
        }

        /// <summary>
        /// Calculates interest capitalization for qualifying assets.
        /// </summary>
        public decimal CalculateInterestCapitalization(InterestCapitalizationDataDto data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.InterestRate < AccountingConstants.MinInterestRate || 
                data.InterestRate > AccountingConstants.MaxInterestRate)
                throw new InvalidAccountingDataException(nameof(data.InterestRate), 
                    "Interest rate must be between 0 and 1.");

            // Calculate interest to capitalize
            decimal interestToCapitalize = data.AverageAccumulatedExpenditures * 
                                          data.InterestRate * 
                                          (data.CapitalizationPeriodMonths / 12.0m);

            // Cannot exceed actual interest costs
            return Math.Min(interestToCapitalize, data.ActualInterestCosts);
        }

        /// <summary>
        /// Gets total exploration costs for a property.
        /// </summary>
        public decimal GetTotalExplorationCosts(string propertyId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                return 0;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "IS_DRY_HOLE", Operator = "=", FilterValue = "N" },
                new AppFilter { FieldName = "FOUND_PROVED_RESERVES", Operator = "=", FilterValue = "Y" }
            };

            var results = dataSource.GetEntityAsync(EXPLORATION_COSTS_TABLE, filters).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return 0;

            // Only capitalize costs of successful wells (not dry holes)
            return results
                .OfType<EXPLORATION_COSTS>()
                .Where(c => c != null && c.IS_DRY_HOLE != "Y" && c.FOUND_PROVED_RESERVES == "Y")
                .Sum(c => (c.EXPLORATORY_DRILLING_COSTS ?? 0m) + (c.EXPLORATORY_WELL_EQUIPMENT ?? 0m));
        }

        /// <summary>
        /// Gets total development costs for a property.
        /// </summary>
        public decimal GetTotalDevelopmentCosts(string propertyId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                return 0;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId }
            };

            var results = dataSource.GetEntityAsync(DEVELOPMENT_COSTS_TABLE, filters).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return 0;

            return results
                .OfType<DEVELOPMENT_COSTS>()
                .Where(c => c != null)
                .Sum(c => c.TOTAL_DEVELOPMENT_COSTS ?? 0m);
        }

        /// <summary>
        /// Gets total G&G costs expensed for a property.
        /// </summary>
        public decimal GetTotalGGCostsExpensed(string propertyId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                return 0;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId }
            };

            var results = dataSource.GetEntityAsync(EXPLORATION_COSTS_TABLE, filters).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return 0;

            // G&G costs are always expensed
            return results
                .OfType<EXPLORATION_COSTS>()
                .Where(c => c != null)
                .Sum(c => c.GEOLOGICAL_GEOPHYSICAL_COSTS ?? 0m);
        }

        /// <summary>
        /// Gets total dry hole costs expensed for a property.
        /// </summary>
        public decimal GetTotalDryHoleCostsExpensed(string propertyId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                return 0;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "IS_DRY_HOLE", Operator = "=", FilterValue = "Y" }
            };

            var results = dataSource.GetEntityAsync(EXPLORATION_COSTS_TABLE, filters).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return 0;

            return results
                .OfType<EXPLORATION_COSTS>()
                .Where(c => c != null && c.IS_DRY_HOLE == "Y")
                .Sum(c => c.TOTAL_EXPLORATION_COSTS ?? 0m);
        }

        /// <summary>
        /// Gets all unproved properties.
        /// </summary>
        public IEnumerable<UnprovedPropertyDto> GetUnprovedProperties(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "IS_PROVED", Operator = "=", FilterValue = "N" }
            };

            var results = dataSource.GetEntityAsync(UNPROVED_PROPERTY_TABLE, filters).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return Enumerable.Empty<UnprovedPropertyDto>();

            return results.OfType<UNPROVED_PROPERTY>().Select(ConvertEntityToUnprovedPropertyDto).Where(p => p != null)!;
        }

        /// <summary>
        /// Gets all proved properties.
        /// </summary>
        public IEnumerable<ProvedPropertyDto> GetProvedProperties(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var results = dataSource.GetEntityAsync(PROVED_PROPERTY_TABLE, null).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return Enumerable.Empty<ProvedPropertyDto>();

            return results.OfType<PROVED_PROPERTY>().Select(ConvertEntityToProvedPropertyDto).Where(p => p != null)!;
        }

        // Helper conversion methods: DTO to Entity
        private UNPROVED_PROPERTY ConvertDtoToUnprovedPropertyEntity(UnprovedPropertyDto dto)
        {
            var entity = new UNPROVED_PROPERTY
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
            return entity;
        }

        private UnprovedPropertyDto ConvertEntityToUnprovedPropertyDto(UNPROVED_PROPERTY entity)
        {
            return new UnprovedPropertyDto
            {
                PropertyId = entity.PROPERTY_ID ?? string.Empty,
                PropertyName = entity.PROPERTY_NAME ?? string.Empty,
                AcquisitionCost = entity.ACQUISITION_COST ?? 0m,
                AcquisitionDate = entity.ACQUISITION_DATE ?? DateTime.MinValue,
                PropertyType = Enum.TryParse<PropertyType>(entity.PROPERTY_TYPE, out var propType) ? propType : PropertyType.Lease,
                WorkingInterest = entity.WORKING_INTEREST ?? 1.0m,
                NetRevenueInterest = entity.NET_REVENUE_INTEREST ?? 1.0m,
                AccumulatedImpairment = entity.ACCUMULATED_IMPAIRMENT ?? 0m,
                IsProved = entity.IS_PROVED == "Y",
                ProvedDate = entity.PROVED_DATE
            };
        }

        private PROVED_PROPERTY ConvertDtoToProvedPropertyEntity(ProvedPropertyDto dto)
        {
            var entity = new PROVED_PROPERTY
            {
                PROPERTY_ID = dto.PropertyId,
                ACQUISITION_COST = dto.AcquisitionCost,
                EXPLORATION_COSTS = dto.ExplorationCosts,
                DEVELOPMENT_COSTS = dto.DevelopmentCosts,
                ACCUMULATED_AMORTIZATION = dto.AccumulatedAmortization,
                PROVED_DATE = dto.ProvedDate
            };
            return entity;
        }

        private ProvedPropertyDto ConvertEntityToProvedPropertyDto(PROVED_PROPERTY entity)
        {
            return new ProvedPropertyDto
            {
                PropertyId = entity.PROPERTY_ID ?? string.Empty,
                AcquisitionCost = entity.ACQUISITION_COST ?? 0m,
                ExplorationCosts = entity.EXPLORATION_COSTS ?? 0m,
                DevelopmentCosts = entity.DEVELOPMENT_COSTS ?? 0m,
                AccumulatedAmortization = entity.ACCUMULATED_AMORTIZATION ?? 0m,
                ProvedDate = entity.PROVED_DATE ?? DateTime.MinValue
            };
        }

        private EXPLORATION_COSTS ConvertDtoToExplorationCostsEntity(ExplorationCostsDto dto)
        {
            var entity = new EXPLORATION_COSTS
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
            return entity;
        }

        private DEVELOPMENT_COSTS ConvertDtoToDevelopmentCostsEntity(DevelopmentCostsDto dto)
        {
            var entity = new DEVELOPMENT_COSTS
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
            return entity;
        }

        private PRODUCTION_COSTS ConvertDtoToProductionCostsEntity(ProductionCostsDto dto)
        {
            var entity = new PRODUCTION_COSTS
            {
                PRODUCTION_COST_ID = dto.ProductionCostId,
                PROPERTY_ID = dto.PropertyId,
                OPERATING_COSTS = dto.OperatingCosts,
                WORKOVER_COSTS = dto.WorkoverCosts,
                MAINTENANCE_COSTS = dto.MaintenanceCosts,
                TOTAL_PRODUCTION_COSTS = dto.TotalProductionCosts,
                COST_PERIOD = dto.CostPeriod
            };
            return entity;
        }
    }
}
