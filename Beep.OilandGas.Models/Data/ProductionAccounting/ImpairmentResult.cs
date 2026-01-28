using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ImpairmentResult : ModelEntityBase
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
        /// Gets or sets the property name.
        /// </summary>
        private string PropertyNameValue = string.Empty;

        public string PropertyName

        {

            get { return this.PropertyNameValue; }

            set { SetProperty(ref PropertyNameValue, value); }

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
        /// Gets or sets the current accumulated impairment.
        /// </summary>
        private decimal AccumulatedImpairmentValue;

        public decimal AccumulatedImpairment

        {

            get { return this.AccumulatedImpairmentValue; }

            set { SetProperty(ref AccumulatedImpairmentValue, value); }

        }

        /// <summary>
        /// Gets or sets the calculated impairment amount.
        /// </summary>
        private decimal CalculatedImpairmentValue;

        public decimal CalculatedImpairment

        {

            get { return this.CalculatedImpairmentValue; }

            set { SetProperty(ref CalculatedImpairmentValue, value); }

        }

        /// <summary>
        /// Gets or sets whether impairment is required.
        /// </summary>
        private bool ImpairmentRequiredValue;

        public bool ImpairmentRequired

        {

            get { return this.ImpairmentRequiredValue; }

            set { SetProperty(ref ImpairmentRequiredValue, value); }

        }

        /// <summary>
        /// Gets or sets the impairment reason.
        /// </summary>
        private string ImpairmentReasonValue = string.Empty;

        public string ImpairmentReason

        {

            get { return this.ImpairmentReasonValue; }

            set { SetProperty(ref ImpairmentReasonValue, value); }

        }

        /// <summary>
        /// Gets or sets the test date.
        /// </summary>
        private DateTime TestDateValue = DateTime.UtcNow;

        public DateTime TestDate

        {

            get { return this.TestDateValue; }

            set { SetProperty(ref TestDateValue, value); }

        }
    }
}
