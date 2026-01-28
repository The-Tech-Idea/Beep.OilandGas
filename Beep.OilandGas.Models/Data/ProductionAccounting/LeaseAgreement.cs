using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public abstract class LeaseAgreement : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the lease identifier.
        /// </summary>
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the lease name or description.
        /// </summary>
        private string LeaseNameValue = string.Empty;

        public string LeaseName

        {

            get { return this.LeaseNameValue; }

            set { SetProperty(ref LeaseNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the lease type.
        /// </summary>
        private LeaseType LeaseTypeValue;

        public LeaseType LeaseType

        {

            get { return this.LeaseTypeValue; }

            set { SetProperty(ref LeaseTypeValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective date of the lease.
        /// </summary>
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the expiration date of the lease.
        /// </summary>
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the primary term in months.
        /// </summary>
        private int PrimaryTermMonthsValue;

        public int PrimaryTermMonths

        {

            get { return this.PrimaryTermMonthsValue; }

            set { SetProperty(ref PrimaryTermMonthsValue, value); }

        }

        /// <summary>
        /// Gets or sets the working interest (decimal, 0-1).
        /// </summary>
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the net revenue interest (decimal, 0-1).
        /// </summary>
        private decimal NetRevenueInterestValue;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the royalty rate (decimal, 0-1).
        /// </summary>
        private decimal RoyaltyRateValue;

        public decimal RoyaltyRate

        {

            get { return this.RoyaltyRateValue; }

            set { SetProperty(ref RoyaltyRateValue, value); }

        }

        /// <summary>
        /// Gets or sets the lease provisions.
        /// </summary>
        private LeaseProvisions ProvisionsValue = new();

        public LeaseProvisions Provisions

        {

            get { return this.ProvisionsValue; }

            set { SetProperty(ref ProvisionsValue, value); }

        }

        /// <summary>
        /// Gets or sets the location information.
        /// </summary>
        private LeaseLocation LocationValue = new();

        public LeaseLocation Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }
    }
}
