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
}
