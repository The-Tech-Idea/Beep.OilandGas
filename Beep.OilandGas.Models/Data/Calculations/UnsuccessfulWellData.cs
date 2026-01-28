using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class UnsuccessfulWellData : ModelEntityBase
    {
        /// <summary>
        /// Well identifier
        /// </summary>
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }

        /// <summary>
        /// Total drilling and completion costs (to be expensed)
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
        /// Date well determined unsuccessful
        /// </summary>
        private DateTime DateDeterminedValue;

        public DateTime DateDetermined

        {

            get { return this.DateDeterminedValue; }

            set { SetProperty(ref DateDeterminedValue, value); }

        }

        /// <summary>
        /// Reason well was unsuccessful
        /// </summary>
        private string ReasonValue = string.Empty;

        public string Reason

        {

            get { return this.ReasonValue; }

            set { SetProperty(ref ReasonValue, value); }

        }
    }
}
