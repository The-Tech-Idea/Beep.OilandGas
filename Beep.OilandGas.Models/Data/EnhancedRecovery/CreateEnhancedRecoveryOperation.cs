using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class CreateEnhancedRecoveryOperation : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string EORTypeValue = string.Empty;

        public string EORType

        {

            get { return this.EORTypeValue; }

            set { SetProperty(ref EORTypeValue, value); }

        }
        private DateTime? PlannedStartDateValue;

        public DateTime? PlannedStartDate

        {

            get { return this.PlannedStartDateValue; }

            set { SetProperty(ref PlannedStartDateValue, value); }

        }
        private decimal? PlannedInjectionRateValue;

        public decimal? PlannedInjectionRate

        {

            get { return this.PlannedInjectionRateValue; }

            set { SetProperty(ref PlannedInjectionRateValue, value); }

        }
        private string? InjectionRateUnitValue;

        public string? InjectionRateUnit

        {

            get { return this.InjectionRateUnitValue; }

            set { SetProperty(ref InjectionRateUnitValue, value); }

        }

        public string? EorMethod { get; set; }
        public DateTime? StartDate { get; set; }
    }
}
