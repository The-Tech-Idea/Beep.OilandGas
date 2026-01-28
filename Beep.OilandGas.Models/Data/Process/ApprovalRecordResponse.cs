using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
    public class ApprovalRecordResponse : ModelEntityBase
    {
        private string ApprovalIdValue = string.Empty;

        public string ApprovalId

        {

            get { return this.ApprovalIdValue; }

            set { SetProperty(ref ApprovalIdValue, value); }

        }
        private string ApprovalTypeValue = string.Empty;

        public string ApprovalType

        {

            get { return this.ApprovalTypeValue; }

            set { SetProperty(ref ApprovalTypeValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // PENDING, APPROVED, REJECTED
        private DateTime RequestedDateValue;

        public DateTime RequestedDate

        {

            get { return this.RequestedDateValue; }

            set { SetProperty(ref RequestedDateValue, value); }

        }
        private string RequestedByValue = string.Empty;

        public string RequestedBy

        {

            get { return this.RequestedByValue; }

            set { SetProperty(ref RequestedByValue, value); }

        }
        private DateTime? ApprovedDateValue;

        public DateTime? ApprovedDate

        {

            get { return this.ApprovedDateValue; }

            set { SetProperty(ref ApprovedDateValue, value); }

        }
        private string ApprovedByValue = string.Empty;

        public string ApprovedBy

        {

            get { return this.ApprovedByValue; }

            set { SetProperty(ref ApprovedByValue, value); }

        }
        private string NotesValue = string.Empty;

        public string Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }
}
