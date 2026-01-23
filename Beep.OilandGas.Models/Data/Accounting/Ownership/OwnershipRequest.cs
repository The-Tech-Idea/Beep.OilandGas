using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting.Ownership
{
    /// <summary>
    /// Request DTO for registering ownership interest
    /// </summary>
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
    }

    /// <summary>
    /// Request DTO for owner information
    /// </summary>
    public class OwnerRequest : ModelEntityBase
    {
        private string OwnerNameValue = string.Empty;

        public string OwnerName

        {

            get { return this.OwnerNameValue; }

            set { SetProperty(ref OwnerNameValue, value); }

        }
        private string? TaxIdValue;

        public string? TaxId

        {

            get { return this.TaxIdValue; }

            set { SetProperty(ref TaxIdValue, value); }

        }
    }
}







