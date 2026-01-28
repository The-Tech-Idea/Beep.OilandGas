using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class DivisionOrder : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the division order identifier.
        /// </summary>
        private string DivisionOrderIdValue = string.Empty;

        public string DivisionOrderId

        {

            get { return this.DivisionOrderIdValue; }

            set { SetProperty(ref DivisionOrderIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        private string PropertyOrLeaseIdValue = string.Empty;

        public string PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the owner information.
        /// </summary>
        private OwnerInformation OwnerValue = new();

        public OwnerInformation Owner

        {

            get { return this.OwnerValue; }

            set { SetProperty(ref OwnerValue, value); }

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
        /// Gets or sets the royalty interest (decimal, 0-1).
        /// </summary>
        private decimal? RoyaltyInterestValue;

        public decimal? RoyaltyInterest

        {

            get { return this.RoyaltyInterestValue; }

            set { SetProperty(ref RoyaltyInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the overriding royalty interest (decimal, 0-1).
        /// </summary>
        private decimal? OverridingRoyaltyInterestValue;

        public decimal? OverridingRoyaltyInterest

        {

            get { return this.OverridingRoyaltyInterestValue; }

            set { SetProperty(ref OverridingRoyaltyInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        private DivisionOrderStatus StatusValue = DivisionOrderStatus.Pending;

        public DivisionOrderStatus Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }

        /// <summary>
        /// Gets or sets the approval date.
        /// </summary>
        private DateTime? ApprovalDateValue;

        public DateTime? ApprovalDate

        {

            get { return this.ApprovalDateValue; }

            set { SetProperty(ref ApprovalDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the approved by.
        /// </summary>
        private string? ApprovedByValue;

        public string? ApprovedBy

        {

            get { return this.ApprovedByValue; }

            set { SetProperty(ref ApprovedByValue, value); }

        }

        /// <summary>
        /// Gets or sets any notes or comments.
        /// </summary>
        private string? NotesValue;

        public string? Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }
}
