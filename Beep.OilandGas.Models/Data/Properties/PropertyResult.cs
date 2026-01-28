using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PropertyResult : ModelEntityBase
    {
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        private string PropertyTypeValue = string.Empty;

        public string PropertyType

        {

            get { return this.PropertyTypeValue; }

            set { SetProperty(ref PropertyTypeValue, value); }

        }
        private decimal ValueValue;

        public decimal Value

        {

            get { return this.ValueValue; }

            set { SetProperty(ref ValueValue, value); }

        }
        private string UnitValue = string.Empty;

        public string Unit

        {

            get { return this.UnitValue; }

            set { SetProperty(ref UnitValue, value); }

        }
        private string CorrelationUsedValue = string.Empty;

        public string CorrelationUsed

        {

            get { return this.CorrelationUsedValue; }

            set { SetProperty(ref CorrelationUsedValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
        public Dictionary<string, decimal> InputParameters { get; set; } = new();
        private decimal? UncertaintyValue;

        public decimal? Uncertainty

        {

            get { return this.UncertaintyValue; }

            set { SetProperty(ref UncertaintyValue, value); }

        }
        private string NotesValue = string.Empty;

        public string Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }
}
