using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Storage
{
    /// <summary>
    /// Manages storage facilities, tanks, and service units.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
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
            ILoggerFactory loggerFactory,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = loggerFactory?.CreateLogger<StorageManager>();
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Registers a storage facility.
        /// </summary>
        public async Task RegisterFacilityAsync(StorageFacility facility, string userId = "system", string? connectionName = null)
        {
            if (facility == null)
                throw new ArgumentNullException(nameof(facility));

            if (string.IsNullOrEmpty(facility.FacilityId))
                throw new ArgumentException("Facility ID cannot be null or empty.", nameof(facility));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var facilityData = ConvertStorageFacilityToDictionary(facility);
            var result = dataSource.InsertEntity(STORAGE_FACILITY_TABLE, facilityData);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to register storage facility {FacilityId}: {Error}", facility.FacilityId, errorMessage);
                throw new InvalidOperationException($"Failed to save storage facility: {errorMessage}");
            }

            _logger?.LogDebug("Registered storage facility {FacilityId} to database", facility.FacilityId);
        }

        /// <summary>
        /// Registers a storage facility (synchronous wrapper).
        /// </summary>
        public void RegisterFacility(StorageFacility facility)
        {
            RegisterFacilityAsync(facility).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a storage facility by ID.
        /// </summary>
        public async Task<StorageFacility?> GetFacilityAsync(string facilityId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(facilityId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = facilityId }
            };

            var results = await dataSource.GetEntityAsync(STORAGE_FACILITY_TABLE, filters);
            var facilityData = results?.FirstOrDefault();
            
            if (facilityData == null)
                return null;

            return facilityData as StorageFacility;
        }

        /// <summary>
        /// Gets a storage facility by ID (synchronous wrapper).
        /// </summary>
        public StorageFacility? GetFacility(string facilityId)
        {
            return GetFacilityAsync(facilityId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Registers a tank battery.
        /// </summary>
        public async Task RegisterTankBatteryAsync(TankBattery battery, string userId = "system", string? connectionName = null)
        {
            if (battery == null)
                throw new ArgumentNullException(nameof(battery));

            if (string.IsNullOrEmpty(battery.BatteryId))
                throw new ArgumentException("Battery ID cannot be null or empty.", nameof(battery));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var batteryData = ConvertTankBatteryToDictionary(battery);
            var result = dataSource.InsertEntity(TANK_BATTERY_TABLE, batteryData);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to register tank battery {BatteryId}: {Error}", battery.BatteryId, errorMessage);
                throw new InvalidOperationException($"Failed to save tank battery: {errorMessage}");
            }

            _logger?.LogDebug("Registered tank battery {BatteryId} to database", battery.BatteryId);
        }

        /// <summary>
        /// Registers a tank battery (synchronous wrapper).
        /// </summary>
        public void RegisterTankBattery(TankBattery battery)
        {
            RegisterTankBatteryAsync(battery).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a tank battery by ID.
        /// </summary>
        public async Task<TankBattery?> GetTankBatteryAsync(string batteryId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(batteryId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "BATTERY_ID", Operator = "=", FilterValue = batteryId }
            };

            var results = await dataSource.GetEntityAsync(TANK_BATTERY_TABLE, filters);
            var batteryData = results?.FirstOrDefault();
            
            if (batteryData == null)
                return null;

            return batteryData as TankBattery;
        }

        /// <summary>
        /// Gets a tank battery by ID (synchronous wrapper).
        /// </summary>
        public TankBattery? GetTankBattery(string batteryId)
        {
            return GetTankBatteryAsync(batteryId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets tank batteries by lease ID.
        /// </summary>
        public async Task<IEnumerable<TankBattery>> GetTankBatteriesByLeaseAsync(string leaseId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(leaseId))
                return Enumerable.Empty<TankBattery>();

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
                return Enumerable.Empty<TankBattery>();

            return results.Cast<TankBattery>().Where(b => b != null)!;
        }

        /// <summary>
        /// Gets tank batteries by lease ID (synchronous wrapper).
        /// </summary>
        public IEnumerable<TankBattery> GetTankBatteriesByLease(string leaseId)
        {
            return GetTankBatteriesByLeaseAsync(leaseId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Registers a service unit.
        /// </summary>
        public async Task RegisterServiceUnitAsync(ServiceUnit unit, string userId = "system", string? connectionName = null)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));

            if (string.IsNullOrEmpty(unit.UnitId))
                throw new ArgumentException("Unit ID cannot be null or empty.", nameof(unit));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var unitData = ConvertServiceUnitToDictionary(unit);
            var result = dataSource.InsertEntity(SERVICE_UNIT_TABLE, unitData);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to register service unit {UnitId}: {Error}", unit.UnitId, errorMessage);
                throw new InvalidOperationException($"Failed to save service unit: {errorMessage}");
            }

            _logger?.LogDebug("Registered service unit {UnitId} to database", unit.UnitId);
        }

        /// <summary>
        /// Registers a service unit (synchronous wrapper).
        /// </summary>
        public void RegisterServiceUnit(ServiceUnit unit)
        {
            RegisterServiceUnitAsync(unit).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a service unit by ID.
        /// </summary>
        public async Task<ServiceUnit?> GetServiceUnitAsync(string unitId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(unitId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "UNIT_ID", Operator = "=", FilterValue = unitId }
            };

            var results = await dataSource.GetEntityAsync(SERVICE_UNIT_TABLE, filters);
            var unitData = results?.FirstOrDefault();
            
            if (unitData == null)
                return null;

            return unitData as ServiceUnit;
        }

        /// <summary>
        /// Gets a service unit by ID (synchronous wrapper).
        /// </summary>
        public ServiceUnit? GetServiceUnit(string unitId)
        {
            return GetServiceUnitAsync(unitId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Updates tank inventory.
        /// </summary>
        public void UpdateTankInventory(string batteryId, string tankNumber, decimal volume, decimal bsw, decimal? apiGravity = null)
        {
            var battery = GetTankBattery(batteryId);
            if (battery == null)
                throw new ArgumentException($"Tank battery {batteryId} not found.", nameof(batteryId));

            var tank = battery.Tanks.FirstOrDefault(t => t.TankNumber == tankNumber);
            if (tank == null)
                throw new ArgumentException($"Tank {tankNumber} not found in battery {batteryId}.", nameof(tankNumber));

            if (volume < 0 || volume > tank.Capacity)
                throw new ArgumentException($"Volume {volume} is invalid for tank capacity {tank.Capacity}.", nameof(volume));

            tank.CurrentVolume = volume;
            tank.BSW = bsw;
            tank.ApiGravity = apiGravity;
        }

        /// <summary>
        /// Records a LACT transfer.
        /// </summary>
        public void RecordLACTTransfer(string unitId, LACTTransferRecord transfer)
        {
            var unit = GetServiceUnit(unitId);
            if (unit == null)
                throw new ArgumentException($"Service unit {unitId} not found.", nameof(unitId));

            if (unit.LACTUnit == null)
                throw new InvalidOperationException($"Service unit {unitId} does not have a LACT unit.");

            unit.LACTUnit.TransferRecords.Add(transfer);
        }

        /// <summary>
        /// Records a test separator result.
        /// </summary>
        public void RecordTestResult(string unitId, TestResult result)
        {
            var unit = GetServiceUnit(unitId);
            if (unit == null)
                throw new ArgumentException($"Service unit {unitId} not found.", nameof(unitId));

            if (unit.TestSeparator == null)
                throw new InvalidOperationException($"Service unit {unitId} does not have a test separator.");

            unit.TestSeparator.TestResults.Add(result);
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
            return batteries.Sum(b => b.CurrentInventory);
        }

        /// <summary>
        /// Gets total inventory for a lease (synchronous wrapper).
        /// </summary>
        public decimal GetInventoryByLease(string leaseId)
        {
            return GetInventoryByLeaseAsync(leaseId).GetAwaiter().GetResult();
        }

        #region Helper Methods - Model to Dictionary Conversion

        private Dictionary<string, object> ConvertStorageFacilityToDictionary(StorageFacility facility)
        {
            return new Dictionary<string, object>
            {
                { "FACILITY_ID", facility.FacilityId },
                { "FACILITY_NAME", facility.FacilityName ?? string.Empty },
                { "FACILITY_TYPE", facility.FacilityType ?? string.Empty },
                { "LOCATION", facility.Location ?? string.Empty },
                { "CAPACITY", facility.Capacity },
                { "CURRENT_INVENTORY", facility.CurrentInventory }
            };
        }


        private Dictionary<string, object> ConvertTankBatteryToDictionary(TankBattery battery)
        {
            return new Dictionary<string, object>
            {
                { "BATTERY_ID", battery.BatteryId },
                { "BATTERY_NAME", battery.BatteryName ?? string.Empty },
                { "LEASE_ID", battery.LeaseId ?? string.Empty },
                { "CURRENT_INVENTORY", battery.CurrentInventory }
            };
        }


        private Dictionary<string, object> ConvertServiceUnitToDictionary(ServiceUnit unit)
        {
            return new Dictionary<string, object>
            {
                { "UNIT_ID", unit.UnitId },
                { "UNIT_NAME", unit.UnitName ?? string.Empty },
                { "UNIT_TYPE", unit.UnitType ?? string.Empty },
                { "LEASE_ID", unit.LeaseId ?? string.Empty }
            };
        }

        private ServiceUnit? ConvertDictionaryToServiceUnit(Dictionary<string, object> dict)
        {
            if (dict == null || !dict.ContainsKey("UNIT_ID"))
                return null;

            return new ServiceUnit
            {
                UnitId = dict["UNIT_ID"]?.ToString() ?? string.Empty,
                UnitName = dict.ContainsKey("UNIT_NAME") ? dict["UNIT_NAME"]?.ToString() ?? string.Empty : string.Empty,
                UnitType = dict.ContainsKey("UNIT_TYPE") ? dict["UNIT_TYPE"]?.ToString() : null,
                LeaseId = dict.ContainsKey("LEASE_ID") ? dict["LEASE_ID"]?.ToString() : null
            };
        }

        #endregion
    }
}

