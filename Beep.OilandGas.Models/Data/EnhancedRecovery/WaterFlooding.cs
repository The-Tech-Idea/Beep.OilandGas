using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class WaterFlooding : ModelEntityBase
    {
        private string OperationIdValue = string.Empty;

        public string OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private decimal? WaterInjectionRateValue;

        public decimal? WaterInjectionRate

        {

            get { return this.WaterInjectionRateValue; }

            set { SetProperty(ref WaterInjectionRateValue, value); }

        }
        private string? InjectionRateUnitValue;

        public string? InjectionRateUnit

        {

            get { return this.InjectionRateUnitValue; }

            set { SetProperty(ref InjectionRateUnitValue, value); }

        }
        private string? WaterSourceValue;

        public string? WaterSource

        {

            get { return this.WaterSourceValue; }

            set { SetProperty(ref WaterSourceValue, value); }

        }
        private decimal? ProductionIncreaseValue;

        public decimal? ProductionIncrease

        {

            get { return this.ProductionIncreaseValue; }

            set { SetProperty(ref ProductionIncreaseValue, value); }

        }
        private string? ProductionUnitValue;

        public string? ProductionUnit

        {

            get { return this.ProductionUnitValue; }

            set { SetProperty(ref ProductionUnitValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
    }
}
