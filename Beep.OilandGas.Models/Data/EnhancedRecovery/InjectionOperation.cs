using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class InjectionOperation : ModelEntityBase
    {
        private string OperationIdValue = string.Empty;

        public string OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string InjectionTypeValue = string.Empty;

        public string InjectionType

        {

            get { return this.InjectionTypeValue; }

            set { SetProperty(ref InjectionTypeValue, value); }

        }
        private DateTime OperationDateValue;

        public DateTime OperationDate

        {

            get { return this.OperationDateValue; }

            set { SetProperty(ref OperationDateValue, value); }

        }
        private decimal? InjectionRateValue;

        public decimal? InjectionRate

        {

            get { return this.InjectionRateValue; }

            set { SetProperty(ref InjectionRateValue, value); }

        }
        private string? InjectionRateUnitValue;

        public string? InjectionRateUnit

        {

            get { return this.InjectionRateUnitValue; }

            set { SetProperty(ref InjectionRateUnitValue, value); }

        }
        private decimal? InjectionPressureValue;

        public decimal? InjectionPressure

        {

            get { return this.InjectionPressureValue; }

            set { SetProperty(ref InjectionPressureValue, value); }

        }
        private string? PressureUnitValue;

        public string? PressureUnit

        {

            get { return this.PressureUnitValue; }

            set { SetProperty(ref PressureUnitValue, value); }

        }
        private decimal? CumulativeInjectionValue;

        public decimal? CumulativeInjection

        {

            get { return this.CumulativeInjectionValue; }

            set { SetProperty(ref CumulativeInjectionValue, value); }

        }
        private string? CumulativeUnitValue;

        public string? CumulativeUnit

        {

            get { return this.CumulativeUnitValue; }

            set { SetProperty(ref CumulativeUnitValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
    }
}
