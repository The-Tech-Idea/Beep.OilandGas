using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ProvedProperty : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the acquisition cost.
        /// </summary>
        private decimal AcquisitionCostValue;

        public decimal AcquisitionCost

        {

            get { return this.AcquisitionCostValue; }

            set { SetProperty(ref AcquisitionCostValue, value); }

        }

        /// <summary>
        /// Gets or sets the exploration costs capitalized.
        /// </summary>
        private decimal ExplorationCostsValue;

        public decimal ExplorationCosts

        {

            get { return this.ExplorationCostsValue; }

            set { SetProperty(ref ExplorationCostsValue, value); }

        }

        /// <summary>
        /// Gets or sets the development costs capitalized.
        /// </summary>
        private decimal DevelopmentCostsValue;

        public decimal DevelopmentCosts

        {

            get { return this.DevelopmentCostsValue; }

            set { SetProperty(ref DevelopmentCostsValue, value); }

        }

        /// <summary>
        /// Gets or sets the accumulated amortization.
        /// </summary>
        private decimal AccumulatedAmortizationValue;

        public decimal AccumulatedAmortization

        {

            get { return this.AccumulatedAmortizationValue; }

            set { SetProperty(ref AccumulatedAmortizationValue, value); }

        }

        /// <summary>
        /// Gets or sets the proved reserves.
        /// </summary>
        private ProvedReserves? ReservesValue;

        public ProvedReserves? Reserves

        {

            get { return this.ReservesValue; }

            set { SetProperty(ref ReservesValue, value); }

        }

        /// <summary>
        /// Gets or sets the proved date.
        /// </summary>
        private DateTime ProvedDateValue;

        public DateTime ProvedDate

        {

            get { return this.ProvedDateValue; }

            set { SetProperty(ref ProvedDateValue, value); }

        }
    }
}
