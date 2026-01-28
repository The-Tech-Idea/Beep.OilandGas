using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Validation
{
    public class ValidationSummary : ModelEntityBase
    {
        private string EntityTypeValue;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? EndDateValue;

        public DateTime? EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private int TotalValidationsValue;

        public int TotalValidations

        {

            get { return this.TotalValidationsValue; }

            set { SetProperty(ref TotalValidationsValue, value); }

        }
        private int ValidCountValue;

        public int ValidCount

        {

            get { return this.ValidCountValue; }

            set { SetProperty(ref ValidCountValue, value); }

        }
        private int InvalidCountValue;

        public int InvalidCount

        {

            get { return this.InvalidCountValue; }

            set { SetProperty(ref InvalidCountValue, value); }

        }
        private int WarningCountValue;

        public int WarningCount

        {

            get { return this.WarningCountValue; }

            set { SetProperty(ref WarningCountValue, value); }

        }
        private decimal ValidationSuccessRateValue;

        public decimal ValidationSuccessRate

        {

            get { return this.ValidationSuccessRateValue; }

            set { SetProperty(ref ValidationSuccessRateValue, value); }

        }
    }
}
