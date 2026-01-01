using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Financial.SuccessfulEfforts
{
    /// <summary>
    /// Service for Successful Efforts accounting operations per FASB Statement No. 19.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class SuccessfulEffortsService : ISuccessfulEffortsService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<SuccessfulEffortsService>? _logger;
        private readonly string _connectionName;
        private const string UNPROVED_PROPERTY_TABLE = "UNPROVED_PROPERTY";
        private const string PROVED_PROPERTY_TABLE = "PROVED_PROPERTY";
        private const string EXPLORATION_COSTS_TABLE = "EXPLORATION_COSTS";
        private const string DEVELOPMENT_COSTS_TABLE = "DEVELOPMENT_COSTS";
        private const string PRODUCTION_COSTS_TABLE = "PRODUCTION_COSTS";

        public SuccessfulEffortsService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<SuccessfulEffortsService>? logger = null,
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
        public async Task<UNPROVED_PROPERTY> RecordAcquisitionAsync(UnprovedPropertyDto property, string userId, string? connectionName = null)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            if (string.IsNullOrEmpty(property.PropertyId))
                throw new InvalidAccountingDataException(nameof(property.PropertyId), "Property ID cannot be null or empty.");
            if (property.AcquisitionCost < AccountingConstants.MinCost)
                throw new InvalidAccountingDataException(nameof(property.AcquisitionCost), "Acquisition cost cannot be negative.");

            var connName = connectionName ?? _connectionName;
            var repo = await GetUnprovedPropertyRepositoryAsync(connName);

            var entity = new UNPROVED_PROPERTY
            {
                PROPERTY_ID = property.PropertyId,
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
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(entity);

            _logger?.LogDebug("Recorded unproved property {PropertyId}", property.PropertyId);
            return entity;
        }

        /// <summary>
        /// Records exploration costs. G&G costs are expensed, drilling costs are capitalized.
        /// </summary>
        public async Task<EXPLORATION_COSTS> RecordExplorationCostsAsync(ExplorationCostsDto costs, string userId, string? connectionName = null)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));
            if (string.IsNullOrEmpty(costs.PropertyId))
                throw new InvalidAccountingDataException(nameof(costs.PropertyId), "Property ID cannot be null or empty.");

            var connName = connectionName ?? _connectionName;
            var repo = await GetExplorationCostsRepositoryAsync(connName);

            if (string.IsNullOrEmpty(costs.ExplorationCostId))
                costs.ExplorationCostId = Guid.NewGuid().ToString();

            var entity = new EXPLORATION_COSTS
            {
                EXPLORATION_COST_ID = costs.ExplorationCostId,
                PROPERTY_ID = costs.PropertyId,
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
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(entity);

            _logger?.LogDebug("Recorded exploration costs {ExplorationCostId} for property {PropertyId}",
                 costs.ExplorationCostId, costs.PropertyId);
            return entity;
        }

        /// <summary>
        /// Records development costs. All development costs are capitalized.
        /// </summary>
        public async Task<DEVELOPMENT_COSTS> RecordDevelopmentCostsAsync(DevelopmentCostsDto costs, string userId, string? connectionName = null)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));
            if (string.IsNullOrEmpty(costs.PropertyId))
                throw new InvalidAccountingDataException(nameof(costs.PropertyId), "Property ID cannot be null or empty.");

            var connName = connectionName ?? _connectionName;
            var repo = await GetDevelopmentCostsRepositoryAsync(connName);

            if (string.IsNullOrEmpty(costs.DevelopmentCostId))
                costs.DevelopmentCostId = Guid.NewGuid().ToString();

            var entity = new DEVELOPMENT_COSTS
            {
                DEVELOPMENT_COST_ID = costs.DevelopmentCostId,
                PROPERTY_ID = costs.PropertyId,
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
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(entity);

            _logger?.LogDebug("Recorded development costs {DevelopmentCostId} for property {PropertyId}",
                 costs.DevelopmentCostId, costs.PropertyId);
            return entity;
        }

        /// <summary>
        /// Records production costs (lifting costs). These are expensed as incurred.
        /// </summary>
        public async Task<PRODUCTION_COSTS> RecordProductionCostsAsync(ProductionCostsDto costs, string userId, string? connectionName = null)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));
            if (string.IsNullOrEmpty(costs.PropertyId))
                throw new InvalidAccountingDataException(nameof(costs.PropertyId), "Property ID cannot be null or empty.");

            var connName = connectionName ?? _connectionName;
            var repo = await GetProductionCostsRepositoryAsync(connName);

            if (string.IsNullOrEmpty(costs.ProductionCostId))
                costs.ProductionCostId = Guid.NewGuid().ToString();

            var entity = new PRODUCTION_COSTS
            {
                PRODUCTION_COST_ID = costs.ProductionCostId,
                PROPERTY_ID = costs.PropertyId,
                OPERATING_COSTS = costs.OperatingCosts,
                WORKOVER_COSTS = costs.WorkoverCosts,
                MAINTENANCE_COSTS = costs.MaintenanceCosts,
                TOTAL_PRODUCTION_COSTS = costs.TotalProductionCosts,
                COST_PERIOD = costs.CostPeriod,
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(entity);

            _logger?.LogDebug("Recorded production costs {ProductionCostId} for property {PropertyId}",
                 costs.ProductionCostId, costs.PropertyId);
            return entity;
        }

        /// <summary>
        /// Records a dry hole expense for an exploratory well.
        /// </summary>
        public async Task<EXPLORATION_COSTS> RecordDryHoleAsync(ExplorationCostsDto costs, string userId, string? connectionName = null)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            costs.IsDryHole = true;
            costs.FoundProvedReserves = false;

            // All costs of dry hole are expensed
            return await RecordExplorationCostsAsync(costs, userId, connectionName);
        }

        /// <summary>
        /// Reclassifies an unproved property as proved when reserves are discovered.
        /// </summary>
        public async Task<PROVED_PROPERTY> ReclassifyToProvedPropertyAsync(string propertyId, ProvedReservesDto reserves, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                throw new ArgumentNullException(nameof(propertyId));
            if (reserves == null)
                throw new ArgumentNullException(nameof(reserves));

            var connName = connectionName ?? _connectionName;
            var unprovedRepo = await GetUnprovedPropertyRepositoryAsync(connName);
            var provedRepo = await GetProvedPropertyRepositoryAsync(connName);

            // Get unproved property
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId }
            };

            var unprovedProperties = await unprovedRepo.GetAsync(filters);
            var unprovedProperty = unprovedProperties.Cast<UNPROVED_PROPERTY>().FirstOrDefault();

            if (unprovedProperty == null)
                throw new InvalidAccountingDataException(nameof(propertyId), "Property not found in unproved properties.");

            // Get total costs
            var explorationCosts = await GetTotalExplorationCostsAsync(propertyId, connName);
            var developmentCosts = await GetTotalDevelopmentCostsAsync(propertyId, connName);

            // Create proved property entity
            var provedEntity = new PROVED_PROPERTY
            {
                PROPERTY_ID = propertyId,
                ACQUISITION_COST = unprovedProperty.ACQUISITION_COST,
                EXPLORATION_COSTS = explorationCosts,
                DEVELOPMENT_COSTS = developmentCosts,
                PROVED_RESERVES_OIL = reserves.TotalProvedOilReserves,
                PROVED_RESERVES_GAS = reserves.TotalProvedGasReserves,
                PROVED_RESERVES_BOE = reserves.TotalProvedReservesBOE,
                PROVED_DATE = DateTime.UtcNow,
                ACTIVE_IND = "Y"
            };

            if (provedEntity is IPPDMEntity provedPpdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(provedPpdmEntity, userId, connName);
            }

            await provedRepo.InsertAsync(provedEntity);

            // Update unproved property
            unprovedProperty.IS_PROVED = "Y";
            unprovedProperty.PROVED_DATE = DateTime.UtcNow;

            if (unprovedProperty is IPPDMEntity unprovedPpdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForUpdateAsync(unprovedPpdmEntity, userId, connName);
            }

            await unprovedRepo.UpdateAsync(unprovedProperty);

            _logger?.LogDebug("Reclassified property {PropertyId} as proved", propertyId);
            return provedEntity;
        }

        /// <summary>
        /// Tests impairment of an unproved property.
        /// </summary>
        public async Task<ImpairmentResult> TestImpairmentAsync(string propertyId, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                throw new ArgumentException("Property ID is required.", nameof(propertyId));

            var connName = connectionName ?? _connectionName;
            var repo = await GetUnprovedPropertyRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId }
            };

            var properties = await repo.GetAsync(filters);
            var property = properties.Cast<UNPROVED_PROPERTY>().FirstOrDefault();

            if (property == null)
                throw new InvalidOperationException($"Property {propertyId} not found.");

            var acquisitionCost = property.ACQUISITION_COST ?? 0m;
            var accumulatedImpairment = property.ACCUMULATED_IMPAIRMENT ?? 0m;
            var acquisitionDate = property.ACQUISITION_DATE ?? DateTime.MinValue;
            var yearsSinceAcquisition = (DateTime.UtcNow - acquisitionDate).TotalDays / 365.0;

            // Impairment test: If property is older than 3 years with no activity, consider impairment
            decimal calculatedImpairment = 0m;
            bool impairmentRequired = false;
            string impairmentReason = string.Empty;

            if (yearsSinceAcquisition > 3 && accumulatedImpairment < acquisitionCost)
            {
                var yearsOverThree = (decimal)(yearsSinceAcquisition - 3);
                calculatedImpairment = Math.Min(acquisitionCost * 0.10m * yearsOverThree, acquisitionCost - accumulatedImpairment);

                if (calculatedImpairment > 0.01m)
                {
                    impairmentRequired = true;
                    impairmentReason = $"Property inactive for {yearsSinceAcquisition:F1} years";
                }
            }

            return new ImpairmentResult
            {
                PropertyId = propertyId,
                PropertyName = property.PROPERTY_NAME ?? string.Empty,
                AcquisitionCost = acquisitionCost,
                AccumulatedImpairment = accumulatedImpairment,
                CalculatedImpairment = calculatedImpairment,
                ImpairmentRequired = impairmentRequired,
                ImpairmentReason = impairmentReason,
                TestDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Records impairment of an unproved property.
        /// </summary>
        public async Task<UNPROVED_PROPERTY> RecordImpairmentAsync(string propertyId, decimal impairmentAmount, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                throw new ArgumentNullException(nameof(propertyId));

            var connName = connectionName ?? _connectionName;
            var repo = await GetUnprovedPropertyRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId }
            };

            var properties = await repo.GetAsync(filters);
            var property = properties.Cast<UNPROVED_PROPERTY>().FirstOrDefault();

            if (property == null)
                throw new InvalidAccountingDataException(nameof(propertyId), "Property not found.");

            property.ACCUMULATED_IMPAIRMENT = (property.ACCUMULATED_IMPAIRMENT ?? 0m) + impairmentAmount;

            // Impairment cannot exceed acquisition cost
            if (property.ACCUMULATED_IMPAIRMENT > property.ACQUISITION_COST)
                property.ACCUMULATED_IMPAIRMENT = property.ACQUISITION_COST;

            if (property is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForUpdateAsync(ppdmEntity, userId, connName);
            }

            await repo.UpdateAsync(property);

            _logger?.LogDebug("Recorded impairment {ImpairmentAmount} for property {PropertyId}", impairmentAmount, propertyId);
            return property;
        }

        /// <summary>
        /// Gets all unproved properties.
        /// </summary>
        public async Task<List<UNPROVED_PROPERTY>> GetUnprovedPropertiesAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = await GetUnprovedPropertyRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "IS_PROVED", Operator = "=", FilterValue = "N" },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<UNPROVED_PROPERTY>().ToList();
        }

        /// <summary>
        /// Gets all proved properties.
        /// </summary>
        public async Task<List<PROVED_PROPERTY>> GetProvedPropertiesAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = await GetProvedPropertyRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<PROVED_PROPERTY>().ToList();
        }

        /// <summary>
        /// Gets total exploration costs for a property (only capitalized costs).
        /// </summary>
        public async Task<decimal> GetTotalExplorationCostsAsync(string propertyId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                return 0;

            var connName = connectionName ?? _connectionName;
            var repo = await GetExplorationCostsRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "IS_DRY_HOLE", Operator = "=", FilterValue = "N" },
                new AppFilter { FieldName = "FOUND_PROVED_RESERVES", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            var costs = results.Cast<EXPLORATION_COSTS>().ToList();

            // Only capitalize costs of successful wells (not dry holes)
            return costs
                .Where(c => c.IS_DRY_HOLE != "Y" && c.FOUND_PROVED_RESERVES == "Y")
                .Sum(c => (c.EXPLORATORY_DRILLING_COSTS ?? 0m) + (c.EXPLORATORY_WELL_EQUIPMENT ?? 0m));
        }

        /// <summary>
        /// Gets total development costs for a property.
        /// </summary>
        public async Task<decimal> GetTotalDevelopmentCostsAsync(string propertyId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                return 0;

            var connName = connectionName ?? _connectionName;
            var repo = await GetDevelopmentCostsRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId }
            };

            var results = await repo.GetAsync(filters);
            return results
                .Cast<DEVELOPMENT_COSTS>()
                .Where(c => c != null)
                .Sum(c => c.TOTAL_DEVELOPMENT_COSTS ?? 0m);
        }

        /// <summary>
        /// Gets total G&G costs expensed for a property.
        /// </summary>
        public async Task<decimal> GetTotalGGCostsExpensedAsync(string propertyId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                return 0;

            var connName = connectionName ?? _connectionName;
            var repo = await GetExplorationCostsRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId }
            };

            var results = await repo.GetAsync(filters);

            // G&G costs are always expensed
            return results
                .Cast<EXPLORATION_COSTS>()
                .Where(c => c != null)
                .Sum(c => c.GEOLOGICAL_GEOPHYSICAL_COSTS ?? 0m);
        }

        /// <summary>
        /// Gets total dry hole costs expensed for a property.
        /// </summary>
        public async Task<decimal> GetTotalDryHoleCostsExpensedAsync(string propertyId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(propertyId))
                return 0;

            var connName = connectionName ?? _connectionName;
            var repo = await GetExplorationCostsRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PROPERTY_ID", Operator = "=", FilterValue = propertyId },
                new AppFilter { FieldName = "IS_DRY_HOLE", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results
                .Cast<EXPLORATION_COSTS>()
                .Where(c => c != null && c.IS_DRY_HOLE == "Y")
                .Sum(c => c.TOTAL_EXPLORATION_COSTS ?? 0m);
        }

        /// <summary>
        /// Calculates amortization for a proved property using units-of-production method.
        /// </summary>
        public async Task<decimal> CalculateAmortizationAsync(ProvedPropertyDto property, ProvedReservesDto reserves, ProductionDataDto production)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.Models.Data.ProspectIdentification;
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

        // Repository helper methods
        private async Task<PPDMGenericRepository> GetUnprovedPropertyRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(UNPROVED_PROPERTY_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(UNPROVED_PROPERTY);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, UNPROVED_PROPERTY_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetProvedPropertyRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(PROVED_PROPERTY_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(PROVED_PROPERTY);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, PROVED_PROPERTY_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetExplorationCostsRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(EXPLORATION_COSTS_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(EXPLORATION_COSTS);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, EXPLORATION_COSTS_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetDevelopmentCostsRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(DEVELOPMENT_COSTS_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(DEVELOPMENT_COSTS);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, DEVELOPMENT_COSTS_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetProductionCostsRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(PRODUCTION_COSTS_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(PRODUCTION_COSTS);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, PRODUCTION_COSTS_TABLE,
                null);
        }
    }
}
