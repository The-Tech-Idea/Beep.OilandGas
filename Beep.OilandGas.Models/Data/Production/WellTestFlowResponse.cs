using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class WellTestFlowResponse : ModelEntityBase
    {
        private string FlowIdValue = string.Empty;

        public string FlowId

        {

            get { return this.FlowIdValue; }

            set { SetProperty(ref FlowIdValue, value); }

        }
        private string? TestIdValue;

        public string? TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        
        // Flow period
        private DateTime? FlowDateValue;

        public DateTime? FlowDate

        {

            get { return this.FlowDateValue; }

            set { SetProperty(ref FlowDateValue, value); }

        }
        private decimal? FlowDurationValue;

        public decimal? FlowDuration

        {

            get { return this.FlowDurationValue; }

            set { SetProperty(ref FlowDurationValue, value); }

        }
        
        // Flow rates
        private decimal? FlowRateOilValue;

        public decimal? FlowRateOil

        {

            get { return this.FlowRateOilValue; }

            set { SetProperty(ref FlowRateOilValue, value); }

        }
        private decimal? FlowRateGasValue;

        public decimal? FlowRateGas

        {

            get { return this.FlowRateGasValue; }

            set { SetProperty(ref FlowRateGasValue, value); }

        }
        private decimal? FlowRateWaterValue;

        public decimal? FlowRateWater

        {

            get { return this.FlowRateWaterValue; }

            set { SetProperty(ref FlowRateWaterValue, value); }

        }
        private string? FlowRateOuomValue;

        public string? FlowRateOuom

        {

            get { return this.FlowRateOuomValue; }

            set { SetProperty(ref FlowRateOuomValue, value); }

        }
        
        // Choke information
        private decimal? ChokeSizeValue;

        public decimal? ChokeSize

        {

            get { return this.ChokeSizeValue; }

            set { SetProperty(ref ChokeSizeValue, value); }

        }
        private string? ChokeSizeOuomValue;

        public string? ChokeSizeOuom

        {

            get { return this.ChokeSizeOuomValue; }

            set { SetProperty(ref ChokeSizeOuomValue, value); }

        }
        private string? ChokeTypeValue;

        public string? ChokeType

        {

            get { return this.ChokeTypeValue; }

            set { SetProperty(ref ChokeTypeValue, value); }

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
