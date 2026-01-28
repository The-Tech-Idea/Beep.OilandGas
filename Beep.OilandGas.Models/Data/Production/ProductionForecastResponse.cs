using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ProductionForecastResponse : ModelEntityBase
    {
        private string ForecastIdValue = string.Empty;

        public string ForecastId

        {

            get { return this.ForecastIdValue; }

            set { SetProperty(ref ForecastIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? PoolIdValue;

        public string? PoolId

        {

            get { return this.PoolIdValue; }

            set { SetProperty(ref PoolIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        
        // Forecast classification
        private string? ForecastTypeValue;

        public string? ForecastType

        {

            get { return this.ForecastTypeValue; }

            set { SetProperty(ref ForecastTypeValue, value); }

        }
        private string? ForecastMethodValue;

        public string? ForecastMethod

        {

            get { return this.ForecastMethodValue; }

            set { SetProperty(ref ForecastMethodValue, value); }

        }
        private string? ForecastNameValue;

        public string? ForecastName

        {

            get { return this.ForecastNameValue; }

            set { SetProperty(ref ForecastNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Forecast period
        private DateTime? ForecastStartDateValue;

        public DateTime? ForecastStartDate

        {

            get { return this.ForecastStartDateValue; }

            set { SetProperty(ref ForecastStartDateValue, value); }

        }
        private DateTime? ForecastEndDateValue;

        public DateTime? ForecastEndDate

        {

            get { return this.ForecastEndDateValue; }

            set { SetProperty(ref ForecastEndDateValue, value); }

        }
        private int? ForecastPeriodMonthsValue;

        public int? ForecastPeriodMonths

        {

            get { return this.ForecastPeriodMonthsValue; }

            set { SetProperty(ref ForecastPeriodMonthsValue, value); }

        }
        
        // Forecast parameters
        private decimal? InitialRateValue;

        public decimal? InitialRate

        {

            get { return this.InitialRateValue; }

            set { SetProperty(ref InitialRateValue, value); }

        }
        private decimal? DeclineRateValue;

        public decimal? DeclineRate

        {

            get { return this.DeclineRateValue; }

            set { SetProperty(ref DeclineRateValue, value); }

        }
        private decimal? DeclineConstantValue;

        public decimal? DeclineConstant

        {

            get { return this.DeclineConstantValue; }

            set { SetProperty(ref DeclineConstantValue, value); }

        }
        private decimal? HyperbolicExponentValue;

        public decimal? HyperbolicExponent

        {

            get { return this.HyperbolicExponentValue; }

            set { SetProperty(ref HyperbolicExponentValue, value); }

        }
        
        // Forecasted volumes
        private decimal? ForecastOilVolumeValue;

        public decimal? ForecastOilVolume

        {

            get { return this.ForecastOilVolumeValue; }

            set { SetProperty(ref ForecastOilVolumeValue, value); }

        }
        private decimal? ForecastGasVolumeValue;

        public decimal? ForecastGasVolume

        {

            get { return this.ForecastGasVolumeValue; }

            set { SetProperty(ref ForecastGasVolumeValue, value); }

        }
        private string? ForecastVolumeOuomValue;

        public string? ForecastVolumeOuom

        {

            get { return this.ForecastVolumeOuomValue; }

            set { SetProperty(ref ForecastVolumeOuomValue, value); }

        }
        
        // Confidence levels
        private decimal? P10ForecastValue;

        public decimal? P10Forecast

        {

            get { return this.P10ForecastValue; }

            set { SetProperty(ref P10ForecastValue, value); }

        }
        private decimal? P50ForecastValue;

        public decimal? P50Forecast

        {

            get { return this.P50ForecastValue; }

            set { SetProperty(ref P50ForecastValue, value); }

        }
        private decimal? P90ForecastValue;

        public decimal? P90Forecast

        {

            get { return this.P90ForecastValue; }

            set { SetProperty(ref P90ForecastValue, value); }

        }
        
        // Status
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
