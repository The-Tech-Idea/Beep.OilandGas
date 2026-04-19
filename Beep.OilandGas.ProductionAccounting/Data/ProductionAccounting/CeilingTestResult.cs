using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class CeilingTestResult : ModelEntityBase
    {
        private string CostCenterIdValue = string.Empty;

        public string CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private decimal NetCapitalizedCostsValue;

        public decimal NetCapitalizedCosts

        {

            get { return this.NetCapitalizedCostsValue; }

            set { SetProperty(ref NetCapitalizedCostsValue, value); }

        }
        private decimal PresentValueOfFutureNetRevenuesValue;

        public decimal PresentValueOfFutureNetRevenues

        {

            get { return this.PresentValueOfFutureNetRevenuesValue; }

            set { SetProperty(ref PresentValueOfFutureNetRevenuesValue, value); }

        }
        private decimal CeilingValue;

        public decimal Ceiling

        {

            get { return this.CeilingValue; }

            set { SetProperty(ref CeilingValue, value); }

        }
        private bool ImpairmentNeededValue;

        public bool ImpairmentNeeded

        {

            get { return this.ImpairmentNeededValue; }

            set { SetProperty(ref ImpairmentNeededValue, value); }

        }
        private decimal ImpairmentAmountValue;

        public decimal ImpairmentAmount

        {

            get { return this.ImpairmentAmountValue; }

            set { SetProperty(ref ImpairmentAmountValue, value); }

        }
        private DateTime TestDateValue = DateTime.UtcNow;

        public DateTime TestDate

        {

            get { return this.TestDateValue; }

            set { SetProperty(ref TestDateValue, value); }

        }
    }
}
