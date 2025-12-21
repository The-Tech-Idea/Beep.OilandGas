using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Accounting.Exceptions;

namespace Beep.OilandGas.Accounting.Operational.Inventory
{
    /// <summary>
    /// Manages storage facilities, tanks, and service units.
    /// </summary>
    public class StorageManager
    {
        private readonly Dictionary<string, StorageFacility> facilities = new();
        private readonly Dictionary<string, TankBattery> tankBatteries = new();
        private readonly Dictionary<string, ServiceUnit> serviceUnits = new();

        /// <summary>
        /// Registers a storage facility.
        /// </summary>
        public void RegisterFacility(StorageFacility facility)
        {
            if (facility == null)
                throw new ArgumentNullException(nameof(facility));

            if (string.IsNullOrEmpty(facility.FacilityId))
                throw new ArgumentException("Facility ID cannot be null or empty.", nameof(facility));

            facilities[facility.FacilityId] = facility;
        }

        /// <summary>
        /// Gets a storage facility by ID.
        /// </summary>
        public StorageFacility? GetFacility(string facilityId)
        {
            return facilities.TryGetValue(facilityId, out var facility) ? facility : null;
        }

        /// <summary>
        /// Registers a tank battery.
        /// </summary>
        public void RegisterTankBattery(TankBattery battery)
        {
            if (battery == null)
                throw new ArgumentNullException(nameof(battery));

            if (string.IsNullOrEmpty(battery.BatteryId))
                throw new ArgumentException("Battery ID cannot be null or empty.", nameof(battery));

            tankBatteries[battery.BatteryId] = battery;
        }

        /// <summary>
        /// Gets a tank battery by ID.
        /// </summary>
        public TankBattery? GetTankBattery(string batteryId)
        {
            return tankBatteries.TryGetValue(batteryId, out var battery) ? battery : null;
        }

        /// <summary>
        /// Gets tank batteries by lease ID.
        /// </summary>
        public IEnumerable<TankBattery> GetTankBatteriesByLease(string leaseId)
        {
            return tankBatteries.Values.Where(b => b.LeaseId == leaseId);
        }

        /// <summary>
        /// Registers a service unit.
        /// </summary>
        public void RegisterServiceUnit(ServiceUnit unit)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));

            if (string.IsNullOrEmpty(unit.UnitId))
                throw new ArgumentException("Unit ID cannot be null or empty.", nameof(unit));

            serviceUnits[unit.UnitId] = unit;
        }

        /// <summary>
        /// Gets a service unit by ID.
        /// </summary>
        public ServiceUnit? GetServiceUnit(string unitId)
        {
            return serviceUnits.TryGetValue(unitId, out var unit) ? unit : null;
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
        public decimal GetTotalInventory()
        {
            return facilities.Values.Sum(f => f.CurrentInventory);
        }

        /// <summary>
        /// Gets total inventory for a lease.
        /// </summary>
        public decimal GetInventoryByLease(string leaseId)
        {
            return GetTankBatteriesByLease(leaseId).Sum(b => b.CurrentInventory);
        }
    }
}

