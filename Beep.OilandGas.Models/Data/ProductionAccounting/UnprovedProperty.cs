using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class UnprovedProperty : ModelEntityBase
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
        /// Gets or sets the property name or description.
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
        /// Gets or sets the acquisition date.
        /// </summary>
        private DateTime AcquisitionDateValue;

        public DateTime AcquisitionDate

        {

            get { return this.AcquisitionDateValue; }

            set { SetProperty(ref AcquisitionDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the property type (lease, concession, fee interest, etc.).
        /// </summary>
        private PropertyType PropertyTypeValue = PropertyType.Lease;

        public PropertyType PropertyType

        {

            get { return this.PropertyTypeValue; }

            set { SetProperty(ref PropertyTypeValue, value); }

        }

        /// <summary>
        /// Gets or sets the working interest percentage.
        /// </summary>
        private decimal WorkingInterestValue = 1.0m;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the net revenue interest percentage.
        /// </summary>
        private decimal NetRevenueInterestValue = 1.0m;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the accumulated impairment.
        /// </summary>
        private decimal AccumulatedImpairmentValue;

        public decimal AccumulatedImpairment

        {

            get { return this.AccumulatedImpairmentValue; }

            set { SetProperty(ref AccumulatedImpairmentValue, value); }

        }

        /// <summary>
        /// Gets or sets whether the property has been classified as proved.
        /// </summary>
        private bool IsProvedValue;

        public bool IsProved

        {

            get { return this.IsProvedValue; }

            set { SetProperty(ref IsProvedValue, value); }

        }

        /// <summary>
        /// Gets or sets the date when property was classified as proved.
        /// </summary>
        private DateTime? ProvedDateValue;

        public DateTime? ProvedDate

        {

            get { return this.ProvedDateValue; }

            set { SetProperty(ref ProvedDateValue, value); }

        }
    }
}
