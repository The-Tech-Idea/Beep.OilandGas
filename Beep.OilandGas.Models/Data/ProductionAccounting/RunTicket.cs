using System;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// DTO for run ticket data
    /// </summary>
    public class RunTicket : ModelEntityBase
    {
        private string RunTicketNumberValue = string.Empty;

        public string RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
        private DateTime TicketDateTimeValue;

        public DateTime TicketDateTime

        {

            get { return this.TicketDateTimeValue; }

            set { SetProperty(ref TicketDateTimeValue, value); }

        }
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? TankBatteryIdValue;

        public string? TankBatteryId

        {

            get { return this.TankBatteryIdValue; }

            set { SetProperty(ref TankBatteryIdValue, value); }

        }
        private decimal GrossVolumeValue;

        public decimal GrossVolume

        {

            get { return this.GrossVolumeValue; }

            set { SetProperty(ref GrossVolumeValue, value); }

        }
        private decimal BSWVolumeValue;

        public decimal BSWVolume

        {

            get { return this.BSWVolumeValue; }

            set { SetProperty(ref BSWVolumeValue, value); }

        }
        private decimal BSWPercentageValue;

        public decimal BSWPercentage

        {

            get { return this.BSWPercentageValue; }

            set { SetProperty(ref BSWPercentageValue, value); }

        }
        private decimal NetVolumeValue;

        public decimal NetVolume

        {

            get { return this.NetVolumeValue; }

            set { SetProperty(ref NetVolumeValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal? ApiGravityValue;

        public decimal? ApiGravity

        {

            get { return this.ApiGravityValue; }

            set { SetProperty(ref ApiGravityValue, value); }

        }
        private CrudeOilProperties? PropertiesValue;

        public CrudeOilProperties? Properties

        {

            get { return this.PropertiesValue; }

            set { SetProperty(ref PropertiesValue, value); }

        }
        private DispositionType DispositionTypeValue;

        public DispositionType DispositionType

        {

            get { return this.DispositionTypeValue; }

            set { SetProperty(ref DispositionTypeValue, value); }

        }
        private string PurchaserValue = string.Empty;

        public string Purchaser

        {

            get { return this.PurchaserValue; }

            set { SetProperty(ref PurchaserValue, value); }

        }
        private MeasurementMethod? MeasurementMethodValue;

        public MeasurementMethod? MeasurementMethod

        {

            get { return this.MeasurementMethodValue; }

            set { SetProperty(ref MeasurementMethodValue, value); }

        }
    }

    /// <summary>
    /// DTO for measurement method
    /// </summary>


    /// <summary>
    /// Type of disposition.
    /// </summary>
    public enum DispositionType
    {
        /// <summary>
        /// Sale to purchaser.
        /// </summary>
        Sale,

        /// <summary>
        /// Transfer to another location.
        /// </summary>
        Transfer,

        /// <summary>
        /// Exchange transaction.
        /// </summary>
        Exchange,

        /// <summary>
        /// Inventory (not disposed).
        /// </summary>
        Inventory,

        /// <summary>
        /// Royalty in kind.
        /// </summary>
        RoyaltyInKind,

        /// <summary>
        /// Working interest in kind.
        /// </summary>
        WorkingInterestInKind
    }

    /// <summary>
    /// Request to create a run ticket
    /// </summary>
    public class CreateRunTicketRequest : ModelEntityBase
    {
        private string LeaseIdValue = string.Empty;

        [Required]
        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? TankBatteryIdValue;

        public string? TankBatteryId

        {

            get { return this.TankBatteryIdValue; }

            set { SetProperty(ref TankBatteryIdValue, value); }

        }
        private decimal GrossVolumeValue;

        [Required]
        public decimal GrossVolume

        {

            get { return this.GrossVolumeValue; }

            set { SetProperty(ref GrossVolumeValue, value); }

        }
        private decimal BSWPercentageValue;

        [Range(0, 100)]
        public decimal BSWPercentage

        {

            get { return this.BSWPercentageValue; }

            set { SetProperty(ref BSWPercentageValue, value); }

        }
        private decimal TemperatureValue = 60m;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal? ApiGravityValue;

        public decimal? ApiGravity

        {

            get { return this.ApiGravityValue; }

            set { SetProperty(ref ApiGravityValue, value); }

        }
        private CrudeOilProperties? PropertiesValue;

        public CrudeOilProperties? Properties

        {

            get { return this.PropertiesValue; }

            set { SetProperty(ref PropertiesValue, value); }

        }
        private DispositionType DispositionTypeValue;

        [Required]
        public DispositionType DispositionType

        {

            get { return this.DispositionTypeValue; }

            set { SetProperty(ref DispositionTypeValue, value); }

        }
        private string PurchaserValue = string.Empty;

        [Required]
        public string Purchaser

        {

            get { return this.PurchaserValue; }

            set { SetProperty(ref PurchaserValue, value); }

        }
        private DateTime? TicketDateTimeValue;

        public DateTime? TicketDateTime

        {

            get { return this.TicketDateTimeValue; }

            set { SetProperty(ref TicketDateTimeValue, value); }

        }
        private MeasurementMethod MeasurementMethodValue;

        public MeasurementMethod MeasurementMethod

        {

            get { return this.MeasurementMethodValue; }

            set { SetProperty(ref MeasurementMethodValue, value); }

        }
    }
}








