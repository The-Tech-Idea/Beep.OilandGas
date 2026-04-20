using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting.Ownership
{
    public class RegisterOwnershipInterestRequest : ModelEntityBase
    {
        private string PropertyOrLeaseIdValue = string.Empty;

        public string PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }
        private OwnerRequest OwnerValue = new();

        public OwnerRequest Owner

        {

            get { return this.OwnerValue; }

            set { SetProperty(ref OwnerValue, value); }

        }
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }
        private decimal NetRevenueInterestValue;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }

        public OwnerRequest OWNER

        {

            get { return this.OwnerValue; }

            set { SetProperty(ref OwnerValue, value); }

        }

        public string PROPERTY_OR_LEASE_ID

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }

        public decimal WORKING_INTEREST

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }

        public decimal NET_REVENUE_INTEREST

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }

        public new DateTime EFFECTIVE_DATE

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
    }
}
