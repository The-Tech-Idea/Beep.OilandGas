using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
    public class PROCESS_STEP_INSTANCE : ModelEntityBase
    {
        private string? PROCESS_STEP_INSTANCE_IDValue;

        public string? PROCESS_STEP_INSTANCE_ID

        {

            get { return this.PROCESS_STEP_INSTANCE_IDValue; }

            set { SetProperty(ref PROCESS_STEP_INSTANCE_IDValue, value); }

        }
        private string? PROCESS_INSTANCE_IDValue;

        public string? PROCESS_INSTANCE_ID

        {

            get { return this.PROCESS_INSTANCE_IDValue; }

            set { SetProperty(ref PROCESS_INSTANCE_IDValue, value); }

        }
        private string? STEP_IDValue;

        public string? STEP_ID

        {

            get { return this.STEP_IDValue; }

            set { SetProperty(ref STEP_IDValue, value); }

        }
        private int? SEQUENCE_NUMBERValue;

        public int? SEQUENCE_NUMBER

        {

            get { return this.SEQUENCE_NUMBERValue; }

            set { SetProperty(ref SEQUENCE_NUMBERValue, value); }

        }
        private string? STATUSValue;

        public string? STATUS

        {

            get { return this.STATUSValue; }

            set { SetProperty(ref STATUSValue, value); }

        }
        private DateTime? START_DATEValue;

        public DateTime? START_DATE

        {

            get { return this.START_DATEValue; }

            set { SetProperty(ref START_DATEValue, value); }

        }
        private DateTime? COMPLETION_DATEValue;

        public DateTime? COMPLETION_DATE

        {

            get { return this.COMPLETION_DATEValue; }

            set { SetProperty(ref COMPLETION_DATEValue, value); }

        }
        private string? COMPLETED_BYValue;

        public string? COMPLETED_BY

        {

            get { return this.COMPLETED_BYValue; }

            set { SetProperty(ref COMPLETED_BYValue, value); }

        }
        private string? STEP_DATA_JSONValue;

        public string? STEP_DATA_JSON

        {

            get { return this.STEP_DATA_JSONValue; }

            set { SetProperty(ref STEP_DATA_JSONValue, value); }

        }
        private string? OUTCOMEValue;

        public string? OUTCOME

        {

            get { return this.OUTCOMEValue; }

            set { SetProperty(ref OUTCOMEValue, value); }

        }
        private string? NOTESValue;

        public string? NOTES

        {

            get { return this.NOTESValue; }

            set { SetProperty(ref NOTESValue, value); }

        }

    }
}
