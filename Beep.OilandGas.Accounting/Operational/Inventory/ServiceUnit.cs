using System;
using System.Collections.Generic;
using Beep.OilandGas.Accounting.Models;

namespace Beep.OilandGas.Accounting.Operational.Inventory
{
    /// <summary>
    /// Represents a service unit (operating unit).
    /// </summary>
    public class ServiceUnit
    {
        /// <summary>
        /// Gets or sets the unit identifier.
        /// </summary>
        public string UnitId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unit name.
        /// </summary>
        public string UnitName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the operator company.
        /// </summary>
        public string Operator { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the participants in the service unit.
        /// </summary>
        public List<ServiceUnitParticipant> Participants { get; set; } = new();

        /// <summary>
        /// Gets or sets the operating agreement reference.
        /// </summary>
        public string? OperatingAgreementId { get; set; }

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the test separator.
        /// </summary>
        public TestSeparator? TestSeparator { get; set; }

        /// <summary>
        /// Gets or sets the LACT unit.
        /// </summary>
        public LACTUnit? LACTUnit { get; set; }

        /// <summary>
        /// Gets or sets the associated tank battery.
        /// </summary>
        public string? TankBatteryId { get; set; }
    }

    /// <summary>
    /// Represents a participant in a service unit.
    /// </summary>
    public class ServiceUnitParticipant
    {
        /// <summary>
        /// Gets or sets the company name.
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the working interest (decimal, 0-1).
        /// </summary>
        public decimal WorkingInterest { get; set; }

        /// <summary>
        /// Gets or sets whether this participant is the operator.
        /// </summary>
        public bool IsOperator { get; set; }
    }

    /// <summary>
    /// Represents a test separator.
    /// </summary>
    public class TestSeparator
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
        /// Gets or sets the test capacity in barrels per day.
        /// </summary>
        public decimal TestCapacity { get; set; }

        /// <summary>
        /// Gets or sets the operating pressure in psi.
        /// </summary>
        public decimal OperatingPressure { get; set; }

        /// <summary>
        /// Gets or sets the operating temperature in degrees Fahrenheit.
        /// </summary>
        public decimal OperatingTemperature { get; set; } = 60m;

        /// <summary>
        /// Gets or sets the most recent test results.
        /// </summary>
        public List<TestResult> TestResults { get; set; } = new();
    }

    /// <summary>
    /// Represents a test result from a test separator.
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// Gets or sets the test date and time.
        /// </summary>
        public DateTime TestDateTime { get; set; }

        /// <summary>
        /// Gets or sets the well or lease tested.
        /// </summary>
        public string WellOrLeaseId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the oil production rate in barrels per day.
        /// </summary>
        public decimal OilRate { get; set; }

        /// <summary>
        /// Gets or sets the gas production rate in MCF per day.
        /// </summary>
        public decimal GasRate { get; set; }

        /// <summary>
        /// Gets or sets the water production rate in barrels per day.
        /// </summary>
        public decimal WaterRate { get; set; }

        /// <summary>
        /// Gets or sets the BS&W percentage.
        /// </summary>
        public decimal BSW { get; set; }

        /// <summary>
        /// Gets or sets the API gravity.
        /// </summary>
        public decimal? ApiGravity { get; set; }

        /// <summary>
        /// Gets or sets the separator pressure during test.
        /// </summary>
        public decimal SeparatorPressure { get; set; }

        /// <summary>
        /// Gets or sets the separator temperature during test.
        /// </summary>
        public decimal SeparatorTemperature { get; set; }
    }

    /// <summary>
    /// Represents a Lease Automatic Custody Transfer (LACT) unit.
    /// </summary>
    public class LACTUnit
    {
        /// <summary>
        /// Gets or sets the LACT identifier.
        /// </summary>
        public string LACTId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the LACT name.
        /// </summary>
        public string LACTName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the meter configuration.
        /// </summary>
        public MeterConfiguration MeterConfiguration { get; set; } = new();

        /// <summary>
        /// Gets or sets the quality measurement system.
        /// </summary>
        public QualityMeasurementSystem QualityMeasurement { get; set; } = new();

        /// <summary>
        /// Gets or sets the transfer records.
        /// </summary>
        public List<LACTTransferRecord> TransferRecords { get; set; } = new();

        /// <summary>
        /// Gets or sets whether the LACT is currently active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Represents meter configuration for a LACT unit.
    /// </summary>
    public class MeterConfiguration
    {
        /// <summary>
        /// Gets or sets the meter type.
        /// </summary>
        public string MeterType { get; set; } = "Positive Displacement";

        /// <summary>
        /// Gets or sets the meter size.
        /// </summary>
        public string MeterSize { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum flow rate in barrels per hour.
        /// </summary>
        public decimal MaximumFlowRate { get; set; }

        /// <summary>
        /// Gets or sets the meter factor (calibration factor).
        /// </summary>
        public decimal MeterFactor { get; set; } = 1.0m;

        /// <summary>
        /// Gets or sets the last calibration date.
        /// </summary>
        public DateTime? LastCalibrationDate { get; set; }

        /// <summary>
        /// Gets or sets the next calibration due date.
        /// </summary>
        public DateTime? NextCalibrationDate { get; set; }
    }

    /// <summary>
    /// Represents quality measurement system for a LACT unit.
    /// </summary>
    public class QualityMeasurementSystem
    {
        /// <summary>
        /// Gets or sets whether automatic BS&W measurement is enabled.
        /// </summary>
        public bool AutomaticBSWMeasurement { get; set; } = true;

        /// <summary>
        /// Gets or sets whether automatic temperature measurement is enabled.
        /// </summary>
        public bool AutomaticTemperatureMeasurement { get; set; } = true;

        /// <summary>
        /// Gets or sets whether automatic API gravity measurement is enabled.
        /// </summary>
        public bool AutomaticApiGravityMeasurement { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum allowed BS&W percentage.
        /// </summary>
        public decimal MaximumAllowedBSW { get; set; } = 0.5m;

        /// <summary>
        /// Gets or sets whether automatic shutoff is enabled when BS&W exceeds maximum.
        /// </summary>
        public bool AutomaticShutoff { get; set; } = true;
    }

    /// <summary>
    /// Represents a transfer record from a LACT unit.
    /// </summary>
    public class LACTTransferRecord
    {
        /// <summary>
        /// Gets or sets the transfer identifier.
        /// </summary>
        public string TransferId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the transfer date and time.
        /// </summary>
        public DateTime TransferDateTime { get; set; }

        /// <summary>
        /// Gets or sets the gross volume transferred in barrels.
        /// </summary>
        public decimal GrossVolume { get; set; }

        /// <summary>
        /// Gets or sets the BS&W percentage.
        /// </summary>
        public decimal BSW { get; set; }

        /// <summary>
        /// Gets or sets the net volume transferred in barrels.
        /// </summary>
        public decimal NetVolume => GrossVolume * (1m - BSW / 100m);

        /// <summary>
        /// Gets or sets the temperature in degrees Fahrenheit.
        /// </summary>
        public decimal Temperature { get; set; }

        /// <summary>
        /// Gets or sets the API gravity.
        /// </summary>
        public decimal? ApiGravity { get; set; }

        /// <summary>
        /// Gets or sets the purchaser.
        /// </summary>
        public string Purchaser { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the run ticket number.
        /// </summary>
        public string? RunTicketNumber { get; set; }
    }
}

