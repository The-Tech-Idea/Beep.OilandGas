using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CalculationResultResponse : ModelEntityBase
    {
        private string CalculationIdValue;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        private string CalculationTypeValue;

        public string CalculationType

        {

            get { return this.CalculationTypeValue; }

            set { SetProperty(ref CalculationTypeValue, value); }

        }
        private DateTime CalculationDateValue = DateTime.UtcNow;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
        private object ResultValue;

        public object Result

        {

            get { return this.ResultValue; }

            set { SetProperty(ref ResultValue, value); }

        }
        public List<CalculationInputParameter> InputParameters { get; set; } = new();
    }

    public class CalculateDeclineRateRequest : ModelEntityBase
    {
        private decimal InitialRateValue;

        public decimal InitialRate

        {

            get { return this.InitialRateValue; }

            set { SetProperty(ref InitialRateValue, value); }

        }
        private decimal CurrentRateValue;

        public decimal CurrentRate

        {

            get { return this.CurrentRateValue; }

            set { SetProperty(ref CurrentRateValue, value); }

        }
        private decimal TimePeriodValue;

        public decimal TimePeriod

        {

            get { return this.TimePeriodValue; }

            set { SetProperty(ref TimePeriodValue, value); }

        }
        private string DeclineTypeValue = "Exponential";

        public string DeclineType

        {

            get { return this.DeclineTypeValue; }

            set { SetProperty(ref DeclineTypeValue, value); }

        }
    }

    public class CalculateVolumeRequest : ModelEntityBase
    {
        private decimal? GrossVolumeValue;

        public decimal? GrossVolume

        {

            get { return this.GrossVolumeValue; }

            set { SetProperty(ref GrossVolumeValue, value); }

        }
        private decimal? NetVolumeValue;

        public decimal? NetVolume

        {

            get { return this.NetVolumeValue; }

            set { SetProperty(ref NetVolumeValue, value); }

        }
        private decimal BswPercentageValue;

        public decimal BswPercentage

        {

            get { return this.BswPercentageValue; }

            set { SetProperty(ref BswPercentageValue, value); }

        }
    }

    public class CalculateApiGravityRequest : ModelEntityBase
    {
        private decimal? SpecificGravityValue;

        public decimal? SpecificGravity

        {

            get { return this.SpecificGravityValue; }

            set { SetProperty(ref SpecificGravityValue, value); }

        }
        private decimal? ApiGravityValue;

        public decimal? ApiGravity

        {

            get { return this.ApiGravityValue; }

            set { SetProperty(ref ApiGravityValue, value); }

        }
    }
}








