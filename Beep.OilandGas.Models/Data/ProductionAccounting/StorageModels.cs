using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Represents a storage facility for crude oil (DTO for calculations/reporting).
    /// </summary>
    public class StorageFacility : ModelEntityBase
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
    public class Tank : ModelEntityBase
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
    }

    /// <summary>
    /// Represents a tank battery.
    /// </summary>
    public class TankBattery : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the tank battery identifier.
        /// </summary>
        public string TankBatteryId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tank battery name.
        /// </summary>
        public string TankBatteryName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        public string? PropertyOrLeaseId { get; set; }

        /// <summary>
        /// Gets or sets the tanks in this battery.
        /// </summary>
        public List<Tank> Tanks { get; set; } = new();

        /// <summary>
        /// Gets the total capacity in barrels.
        /// </summary>
        public decimal TotalCapacity => Tanks.Sum(t => t.Capacity);

        /// <summary>
        /// Gets the current total inventory in barrels.
        /// </summary>
        public decimal CurrentInventory => Tanks.Sum(t => t.CurrentVolume);
    }

    /// <summary>
    /// Represents a service unit.
    /// </summary>
    public class ServiceUnit : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the service unit identifier.
        /// </summary>
        public string ServiceUnitId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service unit name.
        /// </summary>
        public string ServiceUnitName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        public string? PropertyOrLeaseId { get; set; }

        /// <summary>
        /// Gets or sets the participants.
        /// </summary>
        public List<ServiceUnitParticipant> Participants { get; set; } = new();

        /// <summary>
        /// Gets or sets the test separator.
        /// </summary>
        public TestSeparator? TestSeparator { get; set; }

        /// <summary>
        /// Gets or sets the LACT unit.
        /// </summary>
        public LACTUnit? LACTUnit { get; set; }
    }

    /// <summary>
    /// Represents a service unit participant.
    /// </summary>
    public class ServiceUnitParticipant : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the participant identifier.
        /// </summary>
        public string ParticipantId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the company name.
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the working interest (decimal, 0-1).
        /// </summary>
        public decimal WorkingInterest { get; set; }
    }

    /// <summary>
    /// Represents a test separator.
    /// </summary>
    public class TestSeparator : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the separator identifier.
        /// </summary>
        public string SeparatorId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the separator name.
        /// </summary>
        public string SeparatorName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the test results.
        /// </summary>
        public List<TestResult> TestResults { get; set; } = new();
    }

    /// <summary>
    /// Represents a test result.
    /// </summary>
    public class TestResult : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the test result identifier.
        /// </summary>
        public string TestResultId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the test date.
        /// </summary>
        public DateTime TestDate { get; set; }

        /// <summary>
        /// Gets or sets the oil volume in barrels.
        /// </summary>
        public decimal OilVolume { get; set; }

        /// <summary>
        /// Gets or sets the gas volume in MCF.
        /// </summary>
        public decimal GasVolume { get; set; }

        /// <summary>
        /// Gets or sets the water volume in barrels.
        /// </summary>
        public decimal WaterVolume { get; set; }

        /// <summary>
        /// Gets or sets the test duration in hours.
        /// </summary>
        public decimal TestDurationHours { get; set; }
    }

    /// <summary>
    /// Represents a LACT (Lease Automatic Custody Transfer) unit.
    /// </summary>
    public class LACTUnit : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the LACT unit identifier.
        /// </summary>
        public string LACTUnitId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the LACT unit name.
        /// </summary>
        public string LACTUnitName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the meter configuration.
        /// </summary>
        public MeterConfiguration? MeterConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the quality measurement system.
        /// </summary>
        public QualityMeasurementSystem? QualityMeasurementSystem { get; set; }

        /// <summary>
        /// Gets or sets the transfer records.
        /// </summary>
        public List<LACTTransferRecord> TransferRecords { get; set; } = new();
    }

    /// <summary>
    /// Represents meter configuration.
    /// </summary>
    public class MeterConfiguration : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the meter identifier.
        /// </summary>
        public string MeterId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the meter type.
        /// </summary>
        public string MeterType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the meter factor.
        /// </summary>
        public decimal MeterFactor { get; set; } = 1.0m;

        /// <summary>
        /// Gets or sets the last calibration date.
        /// </summary>
        public DateTime? LastCalibrationDate { get; set; }
    }

    /// <summary>
    /// Represents quality measurement system.
    /// </summary>
    public class QualityMeasurementSystem : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the system identifier.
        /// </summary>
        public string SystemId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the system type.
        /// </summary>
        public string SystemType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether automatic sampling is enabled.
        /// </summary>
        public bool AutomaticSamplingEnabled { get; set; }
    }

    /// <summary>
    /// Represents a LACT transfer class.
    /// </summary>
    public class LACTTransferRecord : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the transfer class identifier.
        /// </summary>
        public string TransferRecordId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the transfer date.
        /// </summary>
        public DateTime TransferDate { get; set; }

        /// <summary>
        /// Gets or sets the volume transferred in barrels.
        /// </summary>
        public decimal VolumeTransferred { get; set; }

        /// <summary>
        /// Gets or sets the run ticket number.
        /// </summary>
        public string? RunTicketNumber { get; set; }
    }
}






