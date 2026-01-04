using Beep.OilandGas.Models.Data.Storage;

namespace Beep.OilandGas.ProductionAccounting.Storage
{
    /// <summary>
    /// Manages storage facilities, tanks, and service units.
    /// Uses Entity classes directly with IDataSource - no dictionary conversions.
    /// </summary>
    public class StorageManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<StorageManager>? _logger;
        private readonly string _connectionName;
        private const string STORAGE_FACILITY_TABLE = "STORAGE_FACILITY";
        private const string TANK_BATTERY_TABLE = "TANK_BATTERY";
        private const string SERVICE_UNIT_TABLE = "SERVICE_UNIT";

        public StorageManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<StorageManager>? logger = null,
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
        public async Task<STORAGE_FACILITY> RegisterFacilityAsync(StorageFacility facility, string userId = "system", string? connectionName = null)
        {
            if (facility == null)
                throw new ArgumentNullException(nameof(facility));

            if (string.IsNullOrEmpty(facility.FacilityId))
                throw new ArgumentException("Facility ID cannot be null or empty.", nameof(facility));

            // Convert StorageFacility model to STORAGE_FACILITY Entity
            var facilityEntity = new STORAGE_FACILITY
            {
                STORAGE_FACILITY_ID = facility.FacilityId,
                FACILITY_NAME = facility.FacilityName,
                FACILITY_TYPE = facility.FacilityType,
                LOCATION = facility.Location,
                CAPACITY = facility.Capacity,
                CURRENT_INVENTORY = facility.CurrentInventory
            };

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForInsert(facilityEntity, userId);
            var result = dataSource.InsertEntity(STORAGE_FACILITY_TABLE, facilityEntity);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to register storage facility {FacilityId}: {Error}", facilityEntity.STORAGE_FACILITY_ID, errorMessage);
                throw new InvalidOperationException($"Failed to save storage facility: {errorMessage}");
            }

            _logger?.LogDebug("Registered storage facility {FacilityId} to database", facilityEntity.STORAGE_FACILITY_ID);
            return facilityEntity;
        }

        /// <summary>
        /// Registers a storage facility (synchronous wrapper).
        /// </summary>
        public STORAGE_FACILITY RegisterFacility(StorageFacility facility)
        {
            return RegisterFacilityAsync(facility).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a storage facility by ID.
        /// </summary>
        public async Task<STORAGE_FACILITY?> GetFacilityAsync(string facilityId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(facilityId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STORAGE_FACILITY_ID", Operator = "=", FilterValue = facilityId }
            };

            var results = await dataSource.GetEntityAsync(STORAGE_FACILITY_TABLE, filters);
            return results?.FirstOrDefault() as STORAGE_FACILITY;
        }

        /// <summary>
        /// Gets a storage facility by ID (synchronous wrapper).
        /// </summary>
        public STORAGE_FACILITY? GetFacility(string facilityId)
        {
            return GetFacilityAsync(facilityId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Registers a tank battery.
        /// </summary>
        public async Task<TANK_BATTERY> RegisterTankBatteryAsync(TankBattery battery, string userId = "system", string? connectionName = null)
        {
            if (battery == null)
                throw new ArgumentNullException(nameof(battery));

            if (string.IsNullOrEmpty(battery.BatteryId))
                throw new ArgumentException("Battery ID cannot be null or empty.", nameof(battery));

            // Convert TankBattery model to TANK_BATTERY Entity
            var batteryEntity = new TANK_BATTERY
            {
                TANK_BATTERY_ID = battery.BatteryId,
                BATTERY_NAME = battery.BatteryName,
                LEASE_ID = battery.LeaseId,
                CURRENT_INVENTORY = battery.CurrentInventory
            };

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForInsert(batteryEntity, userId);
            var result = dataSource.InsertEntity(TANK_BATTERY_TABLE, batteryEntity);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to register tank battery {BatteryId}: {Error}", batteryEntity.TANK_BATTERY_ID, errorMessage);
                throw new InvalidOperationException($"Failed to save tank battery: {errorMessage}");
            }

            _logger?.LogDebug("Registered tank battery {BatteryId} to database", batteryEntity.TANK_BATTERY_ID);
            return batteryEntity;
        }

        /// <summary>
        /// Registers a tank battery (synchronous wrapper).
        /// </summary>
        public TANK_BATTERY RegisterTankBattery(TankBattery battery)
        {
            return RegisterTankBatteryAsync(battery).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a tank battery by ID.
        /// </summary>
        public async Task<TANK_BATTERY?> GetTankBatteryAsync(string batteryId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(batteryId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "TANK_BATTERY_ID", Operator = "=", FilterValue = batteryId }
            };

            var results = await dataSource.GetEntityAsync(TANK_BATTERY_TABLE, filters);
            return results?.FirstOrDefault() as TANK_BATTERY;
        }

        /// <summary>
        /// Gets a tank battery by ID (synchronous wrapper).
        /// </summary>
        public TANK_BATTERY? GetTankBattery(string batteryId)
        {
            return GetTankBatteryAsync(batteryId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets tank batteries by lease ID.
        /// </summary>
        public async Task<IEnumerable<TANK_BATTERY>> GetTankBatteriesByLeaseAsync(string leaseId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(leaseId))
                return Enumerable.Empty<TANK_BATTERY>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId }
            };

            var results = await dataSource.GetEntityAsync(TANK_BATTERY_TABLE, filters);
            if (results == null || !results.Any())
                return Enumerable.Empty<TANK_BATTERY>();

            return results.Cast<TANK_BATTERY>().Where(b => b != null)!;
        }

        /// <summary>
        /// Gets tank batteries by lease ID (synchronous wrapper).
        /// </summary>
        public IEnumerable<TANK_BATTERY> GetTankBatteriesByLease(string leaseId)
        {
            return GetTankBatteriesByLeaseAsync(leaseId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Registers a service unit.
        /// </summary>
        public async Task<SERVICE_UNIT> RegisterServiceUnitAsync(ServiceUnit unit, string userId = "system", string? connectionName = null)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));

            if (string.IsNullOrEmpty(unit.UnitId))
                throw new ArgumentException("Unit ID cannot be null or empty.", nameof(unit));

            // Convert ServiceUnit model to SERVICE_UNIT Entity
            var unitEntity = new SERVICE_UNIT
            {
                SERVICE_UNIT_ID = unit.UnitId,
                UNIT_NAME = unit.UnitName,
                UNIT_TYPE = unit.UnitType,
                LEASE_ID = unit.LeaseId
            };

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForInsert(unitEntity, userId);
            var result = dataSource.InsertEntity(SERVICE_UNIT_TABLE, unitEntity);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to register service unit {UnitId}: {Error}", unitEntity.SERVICE_UNIT_ID, errorMessage);
                throw new InvalidOperationException($"Failed to save service unit: {errorMessage}");
            }

            _logger?.LogDebug("Registered service unit {UnitId} to database", unitEntity.SERVICE_UNIT_ID);
            return unitEntity;
        }

        /// <summary>
        /// Registers a service unit (synchronous wrapper).
        /// </summary>
        public SERVICE_UNIT RegisterServiceUnit(ServiceUnit unit)
        {
            return RegisterServiceUnitAsync(unit).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a service unit by ID.
        /// </summary>
        public async Task<SERVICE_UNIT?> GetServiceUnitAsync(string unitId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(unitId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "SERVICE_UNIT_ID", Operator = "=", FilterValue = unitId }
            };

            var results = await dataSource.GetEntityAsync(SERVICE_UNIT_TABLE, filters);
            return results?.FirstOrDefault() as SERVICE_UNIT;
        }

        /// <summary>
        /// Gets a service unit by ID (synchronous wrapper).
        /// </summary>
        public SERVICE_UNIT? GetServiceUnit(string unitId)
        {
            return GetServiceUnitAsync(unitId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Updates tank inventory.
        /// Note: This method uses model classes for nested properties. Consider refactoring to use separate Entity tables for tanks.
        /// </summary>
        public void UpdateTankInventory(string batteryId, string tankNumber, decimal volume, decimal bsw, decimal? apiGravity = null)
        {
            // Note: Tank inventory updates would need separate TANK Entity table
            // This method may need refactoring to work with Entity classes
            throw new NotImplementedException("UpdateTankInventory needs refactoring to use Entity classes. Tanks should be stored in separate TANK Entity table.");
        }

        /// <summary>
        /// Records a LACT transfer.
        /// Note: This method uses model classes for nested properties. Consider refactoring to use separate Entity tables.
        /// </summary>
        public void RecordLACTTransfer(string unitId, LACTTransferRecord transfer)
        {
            // Note: LACT transfers would need separate LACT_TRANSFER Entity table
            // This method may need refactoring to work with Entity classes
            throw new NotImplementedException("RecordLACTTransfer needs refactoring to use Entity classes. LACT transfers should be stored in separate LACT_TRANSFER Entity table.");
        }

        /// <summary>
        /// Records a test separator result.
        /// Note: This method uses model classes for nested properties. Consider refactoring to use separate Entity tables.
        /// </summary>
        public void RecordTestResult(string unitId, TestResult result)
        {
            // Note: Test results would need separate TEST_RESULT Entity table
            // This method may need refactoring to work with Entity classes
            throw new NotImplementedException("RecordTestResult needs refactoring to use Entity classes. Test results should be stored in separate TEST_RESULT Entity table.");
        }

        /// <summary>
        /// Gets total inventory across all facilities.
        /// </summary>
        public async Task<decimal> GetTotalInventoryAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var results = await dataSource.GetEntityAsync(STORAGE_FACILITY_TABLE, null);
            if (results == null || !results.Any())
                return 0m;

            return results
                .Where(r => r is Dictionary<string, object> dict && dict.ContainsKey("CURRENT_INVENTORY") && dict["CURRENT_INVENTORY"] != DBNull.Value)
                .Sum(r => Convert.ToDecimal(((Dictionary<string, object>)r)["CURRENT_INVENTORY"]));
        }

        /// <summary>
        /// Gets total inventory across all facilities (synchronous wrapper).
        /// </summary>
        public decimal GetTotalInventory()
        {
            return GetTotalInventoryAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets total inventory for a lease.
        /// </summary>
        public async Task<decimal> GetInventoryByLeaseAsync(string leaseId, string? connectionName = null)
        {
            var batteries = await GetTankBatteriesByLeaseAsync(leaseId, connectionName);
            return batteries.Sum(b => b.CURRENT_INVENTORY ?? 0m);
        }

        /// <summary>
        /// Gets total inventory for a lease (synchronous wrapper).
        /// </summary>
        public decimal GetInventoryByLease(string leaseId)
        {
            return GetInventoryByLeaseAsync(leaseId).GetAwaiter().GetResult();
        }

    }
}
