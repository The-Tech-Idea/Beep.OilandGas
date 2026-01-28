using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
    public class PROCESS_HISTORY : ModelEntityBase
    {
        private string? PROCESS_HISTORY_IDValue;

        public string? PROCESS_HISTORY_ID

        {

            get { return this.PROCESS_HISTORY_IDValue; }

            set { SetProperty(ref PROCESS_HISTORY_IDValue, value); }

        }
        private string? PROCESS_INSTANCE_IDValue;

        public string? PROCESS_INSTANCE_ID

        {

            get { return this.PROCESS_INSTANCE_IDValue; }

            set { SetProperty(ref PROCESS_INSTANCE_IDValue, value); }

        }
        private string? PROCESS_STEP_INSTANCE_IDValue;

        public string? PROCESS_STEP_INSTANCE_ID

        {

            get { return this.PROCESS_STEP_INSTANCE_IDValue; }

            set { SetProperty(ref PROCESS_STEP_INSTANCE_IDValue, value); }

        }
        private string? ACTIONValue;

        public string? ACTION

        {

            get { return this.ACTIONValue; }

            set { SetProperty(ref ACTIONValue, value); }

        }
        private string? PREVIOUS_STATEValue;

        public string? PREVIOUS_STATE

        {

            get { return this.PREVIOUS_STATEValue; }

            set { SetProperty(ref PREVIOUS_STATEValue, value); }

        }
        private string? NEW_STATEValue;

        public string? NEW_STATE

        {

            get { return this.NEW_STATEValue; }

            set { SetProperty(ref NEW_STATEValue, value); }

        }
        private DateTime? ACTION_DATEValue;

        public DateTime? ACTION_DATE

        {

            get { return this.ACTION_DATEValue; }

            set { SetProperty(ref ACTION_DATEValue, value); }

        }
        private string? PERFORMED_BYValue;

        public string? PERFORMED_BY

        {

            get { return this.PERFORMED_BYValue; }

            set { SetProperty(ref PERFORMED_BYValue, value); }

        }
        private string? NOTESValue;

        public string? NOTES

        {

            get { return this.NOTESValue; }

            set { SetProperty(ref NOTESValue, value); }

        }
        private string? ACTION_DATA_JSONValue;

        public string? ACTION_DATA_JSON

        {

            get { return this.ACTION_DATA_JSONValue; }

            set { SetProperty(ref ACTION_DATA_JSONValue, value); }

        }

    }
}
