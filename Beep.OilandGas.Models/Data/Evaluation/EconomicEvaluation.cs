using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class EconomicEvaluation : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private DateTime EvaluationDateValue = DateTime.UtcNow;

        public DateTime EvaluationDate

        {

            get { return this.EvaluationDateValue; }

            set { SetProperty(ref EvaluationDateValue, value); }

        }
        private decimal NPVValue;

        public decimal NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }
        private decimal IRRValue;

        public decimal IRR

        {

            get { return this.IRRValue; }

            set { SetProperty(ref IRRValue, value); }

        }
        private decimal PaybackYearsValue;

        public decimal PaybackYears

        {

            get { return this.PaybackYearsValue; }

            set { SetProperty(ref PaybackYearsValue, value); }

        }
    }
}
