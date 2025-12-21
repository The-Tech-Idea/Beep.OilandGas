using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Accounting.Models;

namespace Beep.OilandGas.Accounting.Operational.Inventory
{
    /// <summary>
    /// Represents a storage facility for crude oil.
    /// </summary>
    public class StorageFacility
    {
        /// <summary>
        /// Gets or sets the facility identifier.
        /// </summary>
        public string FacilityId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the facility name.
        /// </summary>
        public string FacilityName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total capacity in barrels.
        /// </summary>
        public decimal TotalCapacity { get; set; }

        /// <summary>
        /// Gets or sets the tanks in this facility.
        /// </summary>
        public List<Tank> Tanks { get; set; } = new();

        /// <summary>
        /// Gets the current total inventory in barrels.
        /// </summary>
        public decimal CurrentInventory => Tanks.Sum(t => t.CurrentVolume);

        /// <summary>
        /// Gets the available capacity in barrels.
        /// </summary>
        public decimal AvailableCapacity => TotalCapacity - CurrentInventory;

        /// <summary>
        /// Gets the utilization percentage (0-100).
        /// </summary>
        public decimal UtilizationPercentage => TotalCapacity > 0 
            ? (CurrentInventory / TotalCapacity) * 100m 
            : 0m;
    }

    /// <summary>
    /// Represents a storage tank.
    /// </summary>
    public class Tank
    {
        /// <summary>
        /// Gets or sets the tank number.
        /// </summary>
        public string TankNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tank capacity in barrels.
        /// </summary>
        public decimal Capacity { get; set; }

        /// <summary>
        /// Gets or sets the current volume in barrels.
        /// </summary>
        public decimal CurrentVolume { get; set; }

        /// <summary>
        /// Gets or sets the temperature in degrees Fahrenheit.
        /// </summary>
        public decimal Temperature { get; set; } = 60m;

        /// <summary>
        /// Gets or sets the BS&W content as percentage (0-100).
        /// </summary>
        public decimal BSW { get; set; }

        /// <summary>
        /// Gets or sets the API gravity of the oil in the tank.
        /// </summary>
        public decimal? ApiGravity { get; set; }

        /// <summary>
        /// Gets the available capacity in barrels.
        /// </summary>
        public decimal AvailableCapacity => Capacity - CurrentVolume;

        /// <summary>
        /// Gets the utilization percentage (0-100).
        /// </summary>
        public decimal UtilizationPercentage => Capacity > 0 
            ? (CurrentVolume / Capacity) * 100m 
            : 0m;

        /// <summary>
        /// Gets the net volume (gross - BS&W).
        /// </summary>
        public decimal NetVolume => CurrentVolume * (1m - BSW / 100m);
    }

    /// <summary>
    /// Represents a tank battery (group of tanks).
    /// </summary>
    public class TankBattery
    {
        /// <summary>
        /// Gets or sets the battery identifier.
        /// </summary>
        public string BatteryId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the battery name.
        /// </summary>
        public string BatteryName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the associated lease or property.
        /// </summary>
        public string? LeaseId { get; set; }

        /// <summary>
        /// Gets or sets the tanks in this battery.
        /// </summary>
        public List<Tank> Tanks { get; set; } = new();

        /// <summary>
        /// Gets the total capacity of all tanks in barrels.
        /// </summary>
        public decimal TotalCapacity => Tanks.Sum(t => t.Capacity);

        /// <summary>
        /// Gets the current total inventory in barrels.
        /// </summary>
        public decimal CurrentInventory => Tanks.Sum(t => t.CurrentVolume);

        /// <summary>
        /// Gets the available capacity in barrels.
        /// </summary>
        public decimal AvailableCapacity => TotalCapacity - CurrentInventory;

        /// <summary>
        /// Gets the utilization percentage (0-100).
        /// </summary>
        public decimal UtilizationPercentage => TotalCapacity > 0 
            ? (CurrentInventory / TotalCapacity) * 100m 
            : 0m;

        /// <summary>
        /// Gets the total net volume (gross - BS&W).
        /// </summary>
        public decimal TotalNetVolume => Tanks.Sum(t => t.NetVolume);
    }
}

