using System;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ProvedProperty : ModelEntityBase
    {
        private string PropertyIdValue = string.Empty;

        public string PropertyId
        {
            get { return this.PropertyIdValue; }
            set { SetProperty(ref PropertyIdValue, value); }
        }

        private decimal AcquisitionCostValue;

        public decimal AcquisitionCost
        {
            get { return this.AcquisitionCostValue; }
            set { SetProperty(ref AcquisitionCostValue, value); }
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

        private decimal AccumulatedAmortizationValue;

        public decimal AccumulatedAmortization
        {
            get { return this.AccumulatedAmortizationValue; }
            set { SetProperty(ref AccumulatedAmortizationValue, value); }
        }

        private ProvedReserves? ReservesValue;

        public ProvedReserves? Reserves
        {
            get { return this.ReservesValue; }
            set { SetProperty(ref ReservesValue, value); }
        }

        private DateTime ProvedDateValue;

        public DateTime ProvedDate
        {
            get { return this.ProvedDateValue; }
            set { SetProperty(ref ProvedDateValue, value); }
        }
    }
}
