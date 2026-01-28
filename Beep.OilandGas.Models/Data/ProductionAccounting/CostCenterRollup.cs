using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class CostCenterRollup : ModelEntityBase
    {
        private string CostCenterIdValue = string.Empty;

        public string CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private string CostCenterNameValue = string.Empty;

        public string CostCenterName

        {

            get { return this.CostCenterNameValue; }

            set { SetProperty(ref CostCenterNameValue, value); }

        }
        private decimal AcquisitionCostsValue;

        public decimal AcquisitionCosts

        {

            get { return this.AcquisitionCostsValue; }

            set { SetProperty(ref AcquisitionCostsValue, value); }

        }
        private decimal ExplorationCostsValue;

        public decimal ExplorationCosts

        {

            get { return this.ExplorationCostsValue; }

            set { SetProperty(ref ExplorationCostsValue, value); }

        }
        private decimal DevelopmentCostsValue;

        public decimal DevelopmentCosts

        {

            get { return this.DevelopmentCostsValue; }

            set { SetProperty(ref DevelopmentCostsValue, value); }

        }
        private decimal TotalCapitalizedCostsValue;

        public decimal TotalCapitalizedCosts

        {

            get { return this.TotalCapitalizedCostsValue; }

            set { SetProperty(ref TotalCapitalizedCostsValue, value); }

        }
        private decimal AccumulatedAmortizationValue;

        public decimal AccumulatedAmortization

        {

            get { return this.AccumulatedAmortizationValue; }

            set { SetProperty(ref AccumulatedAmortizationValue, value); }

        }
        private decimal NetCapitalizedCostsValue;

        public decimal NetCapitalizedCosts

        {

            get { return this.NetCapitalizedCostsValue; }

            set { SetProperty(ref NetCapitalizedCostsValue, value); }

        }
        private DateTime? AsOfDateValue;

        public DateTime? AsOfDate

        {

            get { return this.AsOfDateValue; }

            set { SetProperty(ref AsOfDateValue, value); }

        }
    }
}
