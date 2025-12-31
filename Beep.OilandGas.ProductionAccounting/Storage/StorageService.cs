using Beep.OilandGas.Models.Data.Storage;
using Beep.OilandGas.Models.DTOs.Storage;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Storage
{
    /// <summary>
    /// Service for managing storage facilities, tank batteries, and service units.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class StorageService : IStorageService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<StorageService>? _logger;
        private readonly string _connectionName;
        private const string STORAGE_FACILITY_TABLE = "STORAGE_FACILITY";
        private const string TANK_BATTERY_TABLE = "TANK_BATTERY";
        private const string SERVICE_UNIT_TABLE = "SERVICE_UNIT";
        private const string LACT_UNIT_TABLE = "LACT_UNIT";

        public StorageService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<StorageService>? logger = null,
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
        /// Registers a storage facility.
        /// </summary>
        public async Task<STORAGE_FACILITY> RegisterFacilityAsync(CreateStorageFacilityRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = await GetFacilityRepositoryAsync(connName);

            var facility = new STORAGE_FACILITY
            {
                STORAGE_FACILITY_ID = Guid.NewGuid().ToString(),
                FACILITY_NAME = request.FacilityName,
                FACILITY_TYPE = request.FacilityType,
                LOCATION = request.Location,
                CAPACITY = request.Capacity,
                CURRENT_INVENTORY = 0m,
                ACTIVE_IND = "Y"
            };

            if (facility is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(facility);
            _logger?.LogDebug("Registered storage facility {FacilityName}", request.FacilityName);

            return facility;
        }

        /// <summary>
        /// Gets a storage facility by ID.
        /// </summary>
        public async Task<STORAGE_FACILITY?> GetFacilityAsync(string facilityId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(facilityId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = await GetFacilityRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STORAGE_FACILITY_ID", Operator = "=", FilterValue = facilityId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<STORAGE_FACILITY>().FirstOrDefault();
        }

        /// <summary>
        /// Gets all storage facilities.
        /// </summary>
        public async Task<List<STORAGE_FACILITY>> GetFacilitiesAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = await GetFacilityRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<STORAGE_FACILITY>().OrderBy(f => f.FACILITY_NAME).ToList();
        }

        /// <summary>
        /// Registers a tank battery.
        /// </summary>
        public async Task<TANK_BATTERY> RegisterTankBatteryAsync(CreateTankBatteryRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.StorageFacilityId))
                throw new ArgumentException("Storage Facility ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var facility = await GetFacilityAsync(request.StorageFacilityId, connName);

            if (facility == null)
                throw new InvalidOperationException($"Storage facility {request.StorageFacilityId} not found.");

            var batteryRepo = await GetTankBatteryRepositoryAsync(connName);

            var battery = new TANK_BATTERY
            {
                TANK_BATTERY_ID = Guid.NewGuid().ToString(),
                BATTERY_NAME = request.BatteryName,
                STORAGE_FACILITY_ID = request.StorageFacilityId,
                LEASE_ID = request.LeaseId,
                CURRENT_INVENTORY = 0m,
                ACTIVE_IND = "Y"
            };

            if (battery is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await batteryRepo.InsertAsync(battery);
            _logger?.LogDebug("Registered tank battery {BatteryName}", request.BatteryName);

            return battery;
        }

        /// <summary>
        /// Gets a tank battery by ID.
        /// </summary>
        public async Task<TANK_BATTERY?> GetTankBatteryAsync(string tankBatteryId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(tankBatteryId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = await GetTankBatteryRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "TANK_BATTERY_ID", Operator = "=", FilterValue = tankBatteryId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<TANK_BATTERY>().FirstOrDefault();
        }

        /// <summary>
        /// Gets tank batteries by facility.
        /// </summary>
        public async Task<List<TANK_BATTERY>> GetTankBatteriesByFacilityAsync(string facilityId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(facilityId))
                return new List<TANK_BATTERY>();

            var connName = connectionName ?? _connectionName;
            var repo = await GetTankBatteryRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STORAGE_FACILITY_ID", Operator = "=", FilterValue = facilityId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<TANK_BATTERY>().OrderBy(b => b.BATTERY_NAME).ToList();
        }

        /// <summary>
        /// Registers a service unit.
        /// </summary>
        public async Task<SERVICE_UNIT> RegisterServiceUnitAsync(CreateServiceUnitRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = await GetServiceUnitRepositoryAsync(connName);

            var unit = new SERVICE_UNIT
            {
                SERVICE_UNIT_ID = Guid.NewGuid().ToString(),
                UNIT_NAME = request.UnitName,
                UNIT_TYPE = request.UnitType,
                LEASE_ID = request.LeaseId,
                TANK_BATTERY_ID = request.TankBatteryId,
                OPERATOR_BA_ID = request.OperatorBaId,
                EFFECTIVE_DATE = request.EffectiveDate,
                ACTIVE_IND = "Y"
            };

            if (unit is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(unit);
            _logger?.LogDebug("Registered service unit {UnitName}", request.UnitName);

            return unit;
        }

        /// <summary>
        /// Registers a LACT unit.
        /// </summary>
        public async Task<LACT_UNIT> RegisterLACTUnitAsync(CreateLACTUnitRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.ServiceUnitId))
                throw new ArgumentException("Service Unit ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = await GetLACTUnitRepositoryAsync(connName);

            var lactUnit = new LACT_UNIT
            {
                LACT_UNIT_ID = Guid.NewGuid().ToString(),
                LACT_NAME = request.LactName,
                SERVICE_UNIT_ID = request.ServiceUnitId,
                METER_TYPE = request.MeterType,
                MAXIMUM_FLOW_RATE = request.MaximumFlowRate,
                METER_FACTOR = request.MeterFactor,
                IS_ACTIVE = "Y",
                ACTIVE_IND = "Y"
            };

            if (lactUnit is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(lactUnit);
            _logger?.LogDebug("Registered LACT unit {LactName}", request.LactName);

            return lactUnit;
        }

        /// <summary>
        /// Gets storage capacity summary.
        /// </summary>
        public async Task<StorageCapacitySummary> GetStorageCapacityAsync(string facilityId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(facilityId))
                throw new ArgumentException("Facility ID is required.", nameof(facilityId));

            var connName = connectionName ?? _connectionName;
            var facility = await GetFacilityAsync(facilityId, connName);

            if (facility == null)
                throw new InvalidOperationException($"Storage facility {facilityId} not found.");

            var batteries = await GetTankBatteriesByFacilityAsync(facilityId, connName);

            decimal totalCapacity = facility.CAPACITY ?? 0m;
            decimal currentInventory = facility.CURRENT_INVENTORY ?? 0m;
            decimal availableCapacity = totalCapacity - currentInventory;
            decimal utilizationPercentage = totalCapacity > 0 ? (currentInventory / totalCapacity) * 100m : 0m;

            return new StorageCapacitySummary
            {
                StorageFacilityId = facilityId,
                FacilityName = facility.FACILITY_NAME ?? string.Empty,
                TotalCapacity = totalCapacity,
                CurrentInventory = currentInventory,
                AvailableCapacity = availableCapacity,
                UtilizationPercentage = utilizationPercentage,
                TankBatteryCount = batteries.Count
            };
        }

        /// <summary>
        /// Gets storage utilization report.
        /// </summary>
        public async Task<StorageUtilizationReport> GetStorageUtilizationAsync(string facilityId, DateTime? asOfDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(facilityId))
                throw new ArgumentException("Facility ID is required.", nameof(facilityId));

            var connName = connectionName ?? _connectionName;
            var facility = await GetFacilityAsync(facilityId, connName);

            if (facility == null)
                throw new InvalidOperationException($"Storage facility {facilityId} not found.");

            var batteries = await GetTankBatteriesByFacilityAsync(facilityId, connName);
            var reportDate = asOfDate ?? DateTime.UtcNow;

            decimal totalCapacity = facility.CAPACITY ?? 0m;
            decimal currentInventory = facility.CURRENT_INVENTORY ?? 0m;
            decimal availableCapacity = totalCapacity - currentInventory;
            decimal utilizationPercentage = totalCapacity > 0 ? (currentInventory / totalCapacity) * 100m : 0m;

            var details = batteries.Select(b => new StorageUtilizationDetail
            {
                TankBatteryId = b.TANK_BATTERY_ID ?? string.Empty,
                BatteryName = b.BATTERY_NAME ?? string.Empty,
                Capacity = 0m, // Would need to get from tank inventory if available
                CurrentInventory = b.CURRENT_INVENTORY ?? 0m,
                UtilizationPercentage = 0m // Would calculate if capacity available
            }).ToList();

            return new StorageUtilizationReport
            {
                StorageFacilityId = facilityId,
                AsOfDate = reportDate,
                TotalCapacity = totalCapacity,
                CurrentInventory = currentInventory,
                AvailableCapacity = availableCapacity,
                UtilizationPercentage = utilizationPercentage,
                Details = details
            };
        }

        /// <summary>
        /// Gets facilities requiring maintenance.
        /// </summary>
        public async Task<List<STORAGE_FACILITY>> GetFacilitiesRequiringMaintenanceAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var facilities = await GetFacilitiesAsync(connName);

            // For now, return all active facilities (could be enhanced to check last maintenance date)
            return facilities.Where(f => (f.CAPACITY ?? 0m) > 0).ToList();
        }

        // Repository helper methods
        private async Task<PPDMGenericRepository> GetFacilityRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(STORAGE_FACILITY_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Storage.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(STORAGE_FACILITY);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, STORAGE_FACILITY_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetTankBatteryRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(TANK_BATTERY_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Storage.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(TANK_BATTERY);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, TANK_BATTERY_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetServiceUnitRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(SERVICE_UNIT_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Storage.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(SERVICE_UNIT);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, SERVICE_UNIT_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetLACTUnitRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(LACT_UNIT_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Storage.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(LACT_UNIT);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, LACT_UNIT_TABLE,
                null);
        }
    }
}
