using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class SalesApprovalResult : ModelEntityBase
    {
        private string SalesTransactionIdValue;

        public string SalesTransactionId

        {

            get { return this.SalesTransactionIdValue; }

            set { SetProperty(ref SalesTransactionIdValue, value); }

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
        private string? RejectionReasonValue;

        public string? RejectionReason

        {

            get { return this.RejectionReasonValue; }

            set { SetProperty(ref RejectionReasonValue, value); }

        }
    }
}
