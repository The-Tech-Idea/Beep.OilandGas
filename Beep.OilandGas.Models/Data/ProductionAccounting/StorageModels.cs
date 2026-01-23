using System;
using System.Collections.Generic;
using System.Linq;

using Beep.OilandGas.Models.Data;
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
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the facility name.
        /// </summary>
        private string FacilityNameValue = string.Empty;

        public string FacilityName

        {

            get { return this.FacilityNameValue; }

            set { SetProperty(ref FacilityNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        private string LocationValue = string.Empty;

        public string Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }

        /// <summary>
        /// Gets or sets the total capacity in barrels.
        /// </summary>
        private decimal TotalCapacityValue;

        public decimal TotalCapacity

        {

            get { return this.TotalCapacityValue; }

            set { SetProperty(ref TotalCapacityValue, value); }

        }

        /// <summary>
        /// Gets or sets the tanks in this facility.
        /// </summary>
        private List<Tank> TanksValue = new();

        public List<Tank> Tanks

        {

            get { return this.TanksValue; }

            set { SetProperty(ref TanksValue, value); }

        }

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
        private string TankNumberValue = string.Empty;

        public string TankNumber

        {

            get { return this.TankNumberValue; }

            set { SetProperty(ref TankNumberValue, value); }

        }

        /// <summary>
        /// Gets or sets the tank capacity in barrels.
        /// </summary>
        private decimal CapacityValue;

        public decimal Capacity

        {

            get { return this.CapacityValue; }

            set { SetProperty(ref CapacityValue, value); }

        }

        /// <summary>
        /// Gets or sets the current volume in barrels.
        /// </summary>
        private decimal CurrentVolumeValue;

        public decimal CurrentVolume

        {

            get { return this.CurrentVolumeValue; }

            set { SetProperty(ref CurrentVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the temperature in degrees Fahrenheit.
        /// </summary>
        private decimal TemperatureValue = 60m;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }

        /// <summary>
        /// Gets or sets the BS&W content as percentage (0-100).
        /// </summary>
        private decimal BSWValue;

        public decimal BSW

        {

            get { return this.BSWValue; }

            set { SetProperty(ref BSWValue, value); }

        }

        /// <summary>
        /// Gets or sets the API gravity of the oil in the tank.
        /// </summary>
        private decimal? ApiGravityValue;

        public decimal? ApiGravity

        {

            get { return this.ApiGravityValue; }

            set { SetProperty(ref ApiGravityValue, value); }

        }

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
        private string TankBatteryIdValue = string.Empty;

        public string TankBatteryId

        {

            get { return this.TankBatteryIdValue; }

            set { SetProperty(ref TankBatteryIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the tank battery name.
        /// </summary>
        private string TankBatteryNameValue = string.Empty;

        public string TankBatteryName

        {

            get { return this.TankBatteryNameValue; }

            set { SetProperty(ref TankBatteryNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        private string? PropertyOrLeaseIdValue;

        public string? PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the tanks in this battery.
        /// </summary>
        private List<Tank> TanksValue = new();

        public List<Tank> Tanks

        {

            get { return this.TanksValue; }

            set { SetProperty(ref TanksValue, value); }

        }

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
        private string ServiceUnitIdValue = string.Empty;

        public string ServiceUnitId

        {

            get { return this.ServiceUnitIdValue; }

            set { SetProperty(ref ServiceUnitIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the service unit name.
        /// </summary>
        private string ServiceUnitNameValue = string.Empty;

        public string ServiceUnitName

        {

            get { return this.ServiceUnitNameValue; }

            set { SetProperty(ref ServiceUnitNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        private string? PropertyOrLeaseIdValue;

        public string? PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the participants.
        /// </summary>
        private List<ServiceUnitParticipant> ParticipantsValue = new();

        public List<ServiceUnitParticipant> Participants

        {

            get { return this.ParticipantsValue; }

            set { SetProperty(ref ParticipantsValue, value); }

        }

        /// <summary>
        /// Gets or sets the test separator.
        /// </summary>
        private TestSeparator? TestSeparatorValue;

        public TestSeparator? TestSeparator

        {

            get { return this.TestSeparatorValue; }

            set { SetProperty(ref TestSeparatorValue, value); }

        }

        /// <summary>
        /// Gets or sets the LACT unit.
        /// </summary>
        private LACTUnit? LACTUnitValue;

        public LACTUnit? LACTUnit

        {

            get { return this.LACTUnitValue; }

            set { SetProperty(ref LACTUnitValue, value); }

        }
    }

    /// <summary>
    /// Represents a service unit participant.
    /// </summary>
    public class ServiceUnitParticipant : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the participant identifier.
        /// </summary>
        private string ParticipantIdValue = string.Empty;

        public string ParticipantId

        {

            get { return this.ParticipantIdValue; }

            set { SetProperty(ref ParticipantIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the company name.
        /// </summary>
        private string CompanyNameValue = string.Empty;

        public string CompanyName

        {

            get { return this.CompanyNameValue; }

            set { SetProperty(ref CompanyNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the working interest (decimal, 0-1).
        /// </summary>
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }
    }

    /// <summary>
    /// Represents a test separator.
    /// </summary>
    public class TestSeparator : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the separator identifier.
        /// </summary>
        private string SeparatorIdValue = string.Empty;

        public string SeparatorId

        {

            get { return this.SeparatorIdValue; }

            set { SetProperty(ref SeparatorIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the separator name.
        /// </summary>
        private string SeparatorNameValue = string.Empty;

        public string SeparatorName

        {

            get { return this.SeparatorNameValue; }

            set { SetProperty(ref SeparatorNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the test results.
        /// </summary>
        private List<TestResult> TestResultsValue = new();

        public List<TestResult> TestResults

        {

            get { return this.TestResultsValue; }

            set { SetProperty(ref TestResultsValue, value); }

        }
    }

    /// <summary>
    /// Represents a test result.
    /// </summary>
    public class TestResult : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the test result identifier.
        /// </summary>
        private string TestResultIdValue = string.Empty;

        public string TestResultId

        {

            get { return this.TestResultIdValue; }

            set { SetProperty(ref TestResultIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the test date.
        /// </summary>
        private DateTime TestDateValue;

        public DateTime TestDate

        {

            get { return this.TestDateValue; }

            set { SetProperty(ref TestDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the oil volume in barrels.
        /// </summary>
        private decimal OilVolumeValue;

        public decimal OilVolume

        {

            get { return this.OilVolumeValue; }

            set { SetProperty(ref OilVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the gas volume in MCF.
        /// </summary>
        private decimal GasVolumeValue;

        public decimal GasVolume

        {

            get { return this.GasVolumeValue; }

            set { SetProperty(ref GasVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the water volume in barrels.
        /// </summary>
        private decimal WaterVolumeValue;

        public decimal WaterVolume

        {

            get { return this.WaterVolumeValue; }

            set { SetProperty(ref WaterVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the test duration in hours.
        /// </summary>
        private decimal TestDurationHoursValue;

        public decimal TestDurationHours

        {

            get { return this.TestDurationHoursValue; }

            set { SetProperty(ref TestDurationHoursValue, value); }

        }
    }

    /// <summary>
    /// Represents a LACT (Lease Automatic Custody Transfer) unit.
    /// </summary>
    public class LACTUnit : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the LACT unit identifier.
        /// </summary>
        private string LACTUnitIdValue = string.Empty;

        public string LACTUnitId

        {

            get { return this.LACTUnitIdValue; }

            set { SetProperty(ref LACTUnitIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the LACT unit name.
        /// </summary>
        private string LACTUnitNameValue = string.Empty;

        public string LACTUnitName

        {

            get { return this.LACTUnitNameValue; }

            set { SetProperty(ref LACTUnitNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the meter configuration.
        /// </summary>
        private MeterConfiguration? MeterConfigurationValue;

        public MeterConfiguration? MeterConfiguration

        {

            get { return this.MeterConfigurationValue; }

            set { SetProperty(ref MeterConfigurationValue, value); }

        }

        /// <summary>
        /// Gets or sets the quality measurement system.
        /// </summary>
        private QualityMeasurementSystem? QualityMeasurementSystemValue;

        public QualityMeasurementSystem? QualityMeasurementSystem

        {

            get { return this.QualityMeasurementSystemValue; }

            set { SetProperty(ref QualityMeasurementSystemValue, value); }

        }

        /// <summary>
        /// Gets or sets the transfer records.
        /// </summary>
        private List<LACTTransferRecord> TransferRecordsValue = new();

        public List<LACTTransferRecord> TransferRecords

        {

            get { return this.TransferRecordsValue; }

            set { SetProperty(ref TransferRecordsValue, value); }

        }
    }

    /// <summary>
    /// Represents meter configuration.
    /// </summary>
    public class MeterConfiguration : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the meter identifier.
        /// </summary>
        private string MeterIdValue = string.Empty;

        public string MeterId

        {

            get { return this.MeterIdValue; }

            set { SetProperty(ref MeterIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the meter type.
        /// </summary>
        private string MeterTypeValue = string.Empty;

        public string MeterType

        {

            get { return this.MeterTypeValue; }

            set { SetProperty(ref MeterTypeValue, value); }

        }

        /// <summary>
        /// Gets or sets the meter factor.
        /// </summary>
        private decimal MeterFactorValue = 1.0m;

        public decimal MeterFactor

        {

            get { return this.MeterFactorValue; }

            set { SetProperty(ref MeterFactorValue, value); }

        }

        /// <summary>
        /// Gets or sets the last calibration date.
        /// </summary>
        private DateTime? LastCalibrationDateValue;

        public DateTime? LastCalibrationDate

        {

            get { return this.LastCalibrationDateValue; }

            set { SetProperty(ref LastCalibrationDateValue, value); }

        }
    }

    /// <summary>
    /// Represents quality measurement system.
    /// </summary>
    public class QualityMeasurementSystem : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the system identifier.
        /// </summary>
        private string SystemIdValue = string.Empty;

        public string SystemId

        {

            get { return this.SystemIdValue; }

            set { SetProperty(ref SystemIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the system type.
        /// </summary>
        private string SystemTypeValue = string.Empty;

        public string SystemType

        {

            get { return this.SystemTypeValue; }

            set { SetProperty(ref SystemTypeValue, value); }

        }

        /// <summary>
        /// Gets or sets whether automatic sampling is enabled.
        /// </summary>
        private bool AutomaticSamplingEnabledValue;

        public bool AutomaticSamplingEnabled

        {

            get { return this.AutomaticSamplingEnabledValue; }

            set { SetProperty(ref AutomaticSamplingEnabledValue, value); }

        }
    }

    /// <summary>
    /// Represents a LACT transfer class.
    /// </summary>
    public class LACTTransferRecord : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the transfer class identifier.
        /// </summary>
        private string TransferRecordIdValue = string.Empty;

        public string TransferRecordId

        {

            get { return this.TransferRecordIdValue; }

            set { SetProperty(ref TransferRecordIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the transfer date.
        /// </summary>
        private DateTime TransferDateValue;

        public DateTime TransferDate

        {

            get { return this.TransferDateValue; }

            set { SetProperty(ref TransferDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the volume transferred in barrels.
        /// </summary>
        private decimal VolumeTransferredValue;

        public decimal VolumeTransferred

        {

            get { return this.VolumeTransferredValue; }

            set { SetProperty(ref VolumeTransferredValue, value); }

        }

        /// <summary>
        /// Gets or sets the run ticket number.
        /// </summary>
        private string? RunTicketNumberValue;

        public string? RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
    }
}









