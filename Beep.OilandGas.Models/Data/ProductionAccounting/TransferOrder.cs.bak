using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class TransferOrder : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the transfer order identifier.
        /// </summary>
        private string TransferOrderIdValue = string.Empty;

        public string TransferOrderId

        {

            get { return this.TransferOrderIdValue; }

            set { SetProperty(ref TransferOrderIdValue, value); }

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
        /// Gets or sets the from owner.
        /// </summary>
        private OwnerInformation FromOwnerValue = new();

        public OwnerInformation FromOwner

        {

            get { return this.FromOwnerValue; }

            set { SetProperty(ref FromOwnerValue, value); }

        }

        /// <summary>
        /// Gets or sets the to owner.
        /// </summary>
        private OwnerInformation ToOwnerValue = new();

        public OwnerInformation ToOwner

        {

            get { return this.ToOwnerValue; }

            set { SetProperty(ref ToOwnerValue, value); }

        }

        /// <summary>
        /// Gets or sets the interest transferred (decimal, 0-1).
        /// </summary>
        private decimal InterestTransferredValue;

        public decimal InterestTransferred

        {

            get { return this.InterestTransferredValue; }

            set { SetProperty(ref InterestTransferredValue, value); }

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
        /// Gets or sets the approval status.
        /// </summary>
        private bool IsApprovedValue;

        public bool IsApproved

        {

            get { return this.IsApprovedValue; }

            set { SetProperty(ref IsApprovedValue, value); }

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
    }
}
