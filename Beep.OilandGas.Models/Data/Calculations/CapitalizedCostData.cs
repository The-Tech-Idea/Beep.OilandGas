using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CapitalizedCostData : ModelEntityBase
    {
        /// <summary>
        /// Well or project identifier
        /// </summary>
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }

        /// <summary>
        /// Drilling and completion costs
        /// </summary>
        private decimal DrillingCompletionCostsValue;

        public decimal DrillingCompletionCosts

        {

            get { return this.DrillingCompletionCostsValue; }

            set { SetProperty(ref DrillingCompletionCostsValue, value); }

        }

        /// <summary>
        /// Acquisition costs
        /// </summary>
        private decimal AcquisitionCostsValue;

        public decimal AcquisitionCosts

        {

            get { return this.AcquisitionCostsValue; }

            set { SetProperty(ref AcquisitionCostsValue, value); }

        }

        /// <summary>
        /// Capitalized interest
        /// </summary>
        private decimal CapitalizedInterestValue;

        public decimal CapitalizedInterest

        {

            get { return this.CapitalizedInterestValue; }

            set { SetProperty(ref CapitalizedInterestValue, value); }

        }

        /// <summary>
        /// Total capitalized costs before amortization
        /// </summary>
        private decimal TotalCapitalizedCostsValue;

        public decimal TotalCapitalizedCosts

        {

            get { return this.TotalCapitalizedCostsValue; }

            set { SetProperty(ref TotalCapitalizedCostsValue, value); }

        }

        /// <summary>
        /// Date capitalized
        /// </summary>
        private DateTime DateCapitalizedValue;

        public DateTime DateCapitalized

        {

            get { return this.DateCapitalizedValue; }

            set { SetProperty(ref DateCapitalizedValue, value); }

        }
    }
}
