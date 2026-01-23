using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Ownership
{
    public class CreateDivisionOrderRequest : ModelEntityBase
    {
        private string PropertyIdValue;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string OwnerBaIdValue;

        public string OwnerBaId

        {

            get { return this.OwnerBaIdValue; }

            set { SetProperty(ref OwnerBaIdValue, value); }

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
        private decimal? RoyaltyInterestValue;

        public decimal? RoyaltyInterest

        {

            get { return this.RoyaltyInterestValue; }

            set { SetProperty(ref RoyaltyInterestValue, value); }

        }
        private decimal? OverridingRoyaltyInterestValue;

        public decimal? OverridingRoyaltyInterest

        {

            get { return this.OverridingRoyaltyInterestValue; }

            set { SetProperty(ref OverridingRoyaltyInterestValue, value); }

        }
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }
        private string NotesValue;

        public string Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }

    public class CreateTransferOrderRequest : ModelEntityBase
    {
        private string PropertyIdValue;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string FromOwnerBaIdValue;

        public string FromOwnerBaId

        {

            get { return this.FromOwnerBaIdValue; }

            set { SetProperty(ref FromOwnerBaIdValue, value); }

        }
        private string ToOwnerBaIdValue;

        public string ToOwnerBaId

        {

            get { return this.ToOwnerBaIdValue; }

            set { SetProperty(ref ToOwnerBaIdValue, value); }

        }
        private decimal InterestTransferredValue;

        public decimal InterestTransferred

        {

            get { return this.InterestTransferredValue; }

            set { SetProperty(ref InterestTransferredValue, value); }

        }
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
    }

    public class CreateOwnershipInterestRequest : ModelEntityBase
    {
        private string PropertyIdValue;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string OwnerBaIdValue;

        public string OwnerBaId

        {

            get { return this.OwnerBaIdValue; }

            set { SetProperty(ref OwnerBaIdValue, value); }

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
        private decimal? RoyaltyInterestValue;

        public decimal? RoyaltyInterest

        {

            get { return this.RoyaltyInterestValue; }

            set { SetProperty(ref RoyaltyInterestValue, value); }

        }
        private decimal? OverridingRoyaltyInterestValue;

        public decimal? OverridingRoyaltyInterest

        {

            get { return this.OverridingRoyaltyInterestValue; }

            set { SetProperty(ref OverridingRoyaltyInterestValue, value); }

        }
        private DateTime EffectiveStartDateValue;

        public DateTime EffectiveStartDate

        {

            get { return this.EffectiveStartDateValue; }

            set { SetProperty(ref EffectiveStartDateValue, value); }

        }
        private DateTime? EffectiveEndDateValue;

        public DateTime? EffectiveEndDate

        {

            get { return this.EffectiveEndDateValue; }

            set { SetProperty(ref EffectiveEndDateValue, value); }

        }
        private string DivisionOrderIdValue;

        public string DivisionOrderId

        {

            get { return this.DivisionOrderIdValue; }

            set { SetProperty(ref DivisionOrderIdValue, value); }

        }
    }

    public class OwnershipChangeRequest : ModelEntityBase
    {
        private string PropertyIdValue;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string ChangeTypeValue;

        public string ChangeType

        {

            get { return this.ChangeTypeValue; }

            set { SetProperty(ref ChangeTypeValue, value); }

        } // DivisionOrder, TransferOrder
        private string ChangeIdValue;

        public string ChangeId

        {

            get { return this.ChangeIdValue; }

            set { SetProperty(ref ChangeIdValue, value); }

        }
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    public class OwnershipChangeResult : ModelEntityBase
    {
        private string ChangeIdValue;

        public string ChangeId

        {

            get { return this.ChangeIdValue; }

            set { SetProperty(ref ChangeIdValue, value); }

        }
        private string PropertyIdValue;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string ChangeTypeValue;

        public string ChangeType

        {

            get { return this.ChangeTypeValue; }

            set { SetProperty(ref ChangeTypeValue, value); }

        }
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        private bool IsApprovedValue;

        public bool IsApproved

        {

            get { return this.IsApprovedValue; }

            set { SetProperty(ref IsApprovedValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    public class OwnershipApprovalResult : ModelEntityBase
    {
        private string ChangeIdValue;

        public string ChangeId

        {

            get { return this.ChangeIdValue; }

            set { SetProperty(ref ChangeIdValue, value); }

        }
        private bool IsApprovedValue;

        public bool IsApproved

        {

            get { return this.IsApprovedValue; }

            set { SetProperty(ref IsApprovedValue, value); }

        }
        private string ApproverIdValue;

        public string ApproverId

        {

            get { return this.ApproverIdValue; }

            set { SetProperty(ref ApproverIdValue, value); }

        }
        private DateTime ApprovalDateValue = DateTime.UtcNow;

        public DateTime ApprovalDate

        {

            get { return this.ApprovalDateValue; }

            set { SetProperty(ref ApprovalDateValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string CommentsValue;

        public string Comments

        {

            get { return this.CommentsValue; }

            set { SetProperty(ref CommentsValue, value); }

        }
    }

    public class OwnershipChangeHistory : ModelEntityBase
    {
        private string ChangeIdValue;

        public string ChangeId

        {

            get { return this.ChangeIdValue; }

            set { SetProperty(ref ChangeIdValue, value); }

        }
        private string PropertyIdValue;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string ChangeTypeValue;

        public string ChangeType

        {

            get { return this.ChangeTypeValue; }

            set { SetProperty(ref ChangeTypeValue, value); }

        }
        private DateTime ChangeDateValue;

        public DateTime ChangeDate

        {

            get { return this.ChangeDateValue; }

            set { SetProperty(ref ChangeDateValue, value); }

        }
        private string OwnerBaIdValue;

        public string OwnerBaId

        {

            get { return this.OwnerBaIdValue; }

            set { SetProperty(ref OwnerBaIdValue, value); }

        }
        private decimal? InterestBeforeValue;

        public decimal? InterestBefore

        {

            get { return this.InterestBeforeValue; }

            set { SetProperty(ref InterestBeforeValue, value); }

        }
        private decimal? InterestAfterValue;

        public decimal? InterestAfter

        {

            get { return this.InterestAfterValue; }

            set { SetProperty(ref InterestAfterValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string ApprovedByValue;

        public string ApprovedBy

        {

            get { return this.ApprovedByValue; }

            set { SetProperty(ref ApprovedByValue, value); }

        }
    }
}








