using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ProductionResponse : ModelEntityBase
    {
        private string ProductionIdValue = string.Empty;

        public string ProductionId

        {

            get { return this.ProductionIdValue; }

            set { SetProperty(ref ProductionIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? PoolIdValue;

        public string? PoolId

        {

            get { return this.PoolIdValue; }

            set { SetProperty(ref PoolIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        
        // Production date and period
        private DateTime? ProductionDateValue;

        public DateTime? ProductionDate

        {

            get { return this.ProductionDateValue; }

            set { SetProperty(ref ProductionDateValue, value); }

        }
        private DateTime? ProductionPeriodStartValue;

        public DateTime? ProductionPeriodStart

        {

            get { return this.ProductionPeriodStartValue; }

            set { SetProperty(ref ProductionPeriodStartValue, value); }

        }
        private DateTime? ProductionPeriodEndValue;

        public DateTime? ProductionPeriodEnd

        {

            get { return this.ProductionPeriodEndValue; }

            set { SetProperty(ref ProductionPeriodEndValue, value); }

        }
        private int? ProductionDaysValue;

        public int? ProductionDays

        {

            get { return this.ProductionDaysValue; }

            set { SetProperty(ref ProductionDaysValue, value); }

        }
        
        // Volumes
        private decimal? OilVolumeValue;

        public decimal? OilVolume

        {

            get { return this.OilVolumeValue; }

            set { SetProperty(ref OilVolumeValue, value); }

        }
        private decimal? GasVolumeValue;

        public decimal? GasVolume

        {

            get { return this.GasVolumeValue; }

            set { SetProperty(ref GasVolumeValue, value); }

        }
        private decimal? WaterVolumeValue;

        public decimal? WaterVolume

        {

            get { return this.WaterVolumeValue; }

            set { SetProperty(ref WaterVolumeValue, value); }

        }
        private decimal? CondensateVolumeValue;

        public decimal? CondensateVolume

        {

            get { return this.CondensateVolumeValue; }

            set { SetProperty(ref CondensateVolumeValue, value); }

        }
        private string? VolumeOuomValue;

        public string? VolumeOuom

        {

            get { return this.VolumeOuomValue; }

            set { SetProperty(ref VolumeOuomValue, value); }

        }
        
        // Rates
        private decimal? OilRateValue;

        public decimal? OilRate

        {

            get { return this.OilRateValue; }

            set { SetProperty(ref OilRateValue, value); }

        }
        private decimal? GasRateValue;

        public decimal? GasRate

        {

            get { return this.GasRateValue; }

            set { SetProperty(ref GasRateValue, value); }

        }
        private decimal? WaterRateValue;

        public decimal? WaterRate

        {

            get { return this.WaterRateValue; }

            set { SetProperty(ref WaterRateValue, value); }

        }
        private string? RateOuomValue;

        public string? RateOuom

        {

            get { return this.RateOuomValue; }

            set { SetProperty(ref RateOuomValue, value); }

        }
        
        // Pressures
        private decimal? FlowingPressureValue;

        public decimal? FlowingPressure

        {

            get { return this.FlowingPressureValue; }

            set { SetProperty(ref FlowingPressureValue, value); }

        }
        private decimal? StaticPressureValue;

        public decimal? StaticPressure

        {

            get { return this.StaticPressureValue; }

            set { SetProperty(ref StaticPressureValue, value); }

        }
        private decimal? BottomHolePressureValue;

        public decimal? BottomHolePressure

        {

            get { return this.BottomHolePressureValue; }

            set { SetProperty(ref BottomHolePressureValue, value); }

        }
        private string? PressureOuomValue;

        public string? PressureOuom

        {

            get { return this.PressureOuomValue; }

            set { SetProperty(ref PressureOuomValue, value); }

        }
        
        // Production classification
        private string? ProductionTypeValue;

        public string? ProductionType

        {

            get { return this.ProductionTypeValue; }

            set { SetProperty(ref ProductionTypeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        
        // Common PPDM fields
        private string? ActiveIndValue;

        public string? ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private string? RowQualityValue;

        public string? RowQuality

        {

            get { return this.RowQualityValue; }

            set { SetProperty(ref RowQualityValue, value); }

        }
        private string? PreferredIndValue;

        public string? PreferredInd

        {

            get { return this.PreferredIndValue; }

            set { SetProperty(ref PreferredIndValue, value); }

        }
        
        // Audit fields
        private DateTime? CreateDateValue;

        public DateTime? CreateDate

        {

            get { return this.CreateDateValue; }

            set { SetProperty(ref CreateDateValue, value); }

        }
        private string? CreateUserValue;

        public string? CreateUser

        {

            get { return this.CreateUserValue; }

            set { SetProperty(ref CreateUserValue, value); }

        }
        private DateTime? UpdateDateValue;

        public DateTime? UpdateDate

        {

            get { return this.UpdateDateValue; }

            set { SetProperty(ref UpdateDateValue, value); }

        }
        private string? UpdateUserValue;

        public string? UpdateUser

        {

            get { return this.UpdateUserValue; }

            set { SetProperty(ref UpdateUserValue, value); }

        }
    }
}
