using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
    public class PROCESS_APPROVAL : ModelEntityBase
    {
        private string? PROCESS_APPROVAL_IDValue;

        public string? PROCESS_APPROVAL_ID

        {

            get { return this.PROCESS_APPROVAL_IDValue; }

            set { SetProperty(ref PROCESS_APPROVAL_IDValue, value); }

        }
        private string? PROCESS_STEP_INSTANCE_IDValue;

        public string? PROCESS_STEP_INSTANCE_ID

        {

            get { return this.PROCESS_STEP_INSTANCE_IDValue; }

            set { SetProperty(ref PROCESS_STEP_INSTANCE_IDValue, value); }

        }
        private string? APPROVAL_TYPEValue;

        public string? APPROVAL_TYPE

        {

            get { return this.APPROVAL_TYPEValue; }

            set { SetProperty(ref APPROVAL_TYPEValue, value); }

        }
        private string? APPROVAL_STATUSValue;

        public string? APPROVAL_STATUS

        {

            get { return this.APPROVAL_STATUSValue; }

            set { SetProperty(ref APPROVAL_STATUSValue, value); }

        }
        private DateTime? REQUESTED_DATEValue;

        public DateTime? REQUESTED_DATE

        {

            get { return this.REQUESTED_DATEValue; }

            set { SetProperty(ref REQUESTED_DATEValue, value); }

        }
        private string? REQUESTED_BYValue;

        public string? REQUESTED_BY

        {

            get { return this.REQUESTED_BYValue; }

            set { SetProperty(ref REQUESTED_BYValue, value); }

        }
        private DateTime? APPROVED_DATEValue;

        public DateTime? APPROVED_DATE

        {

            get { return this.APPROVED_DATEValue; }

            set { SetProperty(ref APPROVED_DATEValue, value); }

        }
        private string? APPROVED_BYValue;

        public string? APPROVED_BY

        {

            get { return this.APPROVED_BYValue; }

            set { SetProperty(ref APPROVED_BYValue, value); }

        }
        private string? APPROVAL_NOTESValue;

        public string? APPROVAL_NOTES

        {

            get { return this.APPROVAL_NOTESValue; }

            set { SetProperty(ref APPROVAL_NOTESValue, value); }

        }

    }
}
