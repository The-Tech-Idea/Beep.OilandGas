using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class EnhancedRecoveryOperation : ModelEntityBase
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
        private string EORTypeValue = string.Empty;

        public string EORType

        {

            get { return this.EORTypeValue; }

            set { SetProperty(ref EORTypeValue, value); }

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
        private decimal? EfficiencyValue;

        public decimal? Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
        private List<InjectionWell> InjectionWellsValue = new();

        public List<InjectionWell> InjectionWells

        {

            get { return this.InjectionWellsValue; }

            set { SetProperty(ref InjectionWellsValue, value); }

        }

        public string? EorMethod { get; set; }
    }
}
