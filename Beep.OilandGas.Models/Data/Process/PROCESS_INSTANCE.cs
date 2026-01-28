using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
    public class PROCESS_INSTANCE : ModelEntityBase
    {
        private string? PROCESS_INSTANCE_IDValue;

        public string? PROCESS_INSTANCE_ID

        {

            get { return this.PROCESS_INSTANCE_IDValue; }

            set { SetProperty(ref PROCESS_INSTANCE_IDValue, value); }

        }
        private string? PROCESS_DEFINITION_IDValue;

        public string? PROCESS_DEFINITION_ID

        {

            get { return this.PROCESS_DEFINITION_IDValue; }

            set { SetProperty(ref PROCESS_DEFINITION_IDValue, value); }

        }
        private string? ENTITY_IDValue;

        public string? ENTITY_ID

        {

            get { return this.ENTITY_IDValue; }

            set { SetProperty(ref ENTITY_IDValue, value); }

        }
        private string? ENTITY_TYPEValue;

        public string? ENTITY_TYPE

        {

            get { return this.ENTITY_TYPEValue; }

            set { SetProperty(ref ENTITY_TYPEValue, value); }

        }
        private string? FIELD_IDValue;

        public string? FIELD_ID

        {

            get { return this.FIELD_IDValue; }

            set { SetProperty(ref FIELD_IDValue, value); }

        }
        private string? CURRENT_STATEValue;

        public string? CURRENT_STATE

        {

            get { return this.CURRENT_STATEValue; }

            set { SetProperty(ref CURRENT_STATEValue, value); }

        }
        private string? CURRENT_STEP_IDValue;

        public string? CURRENT_STEP_ID

        {

            get { return this.CURRENT_STEP_IDValue; }

            set { SetProperty(ref CURRENT_STEP_IDValue, value); }

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
        private string? STARTED_BYValue;

        public string? STARTED_BY

        {

            get { return this.STARTED_BYValue; }

            set { SetProperty(ref STARTED_BYValue, value); }

        }
        private string? PROCESS_DATA_JSONValue;

        public string? PROCESS_DATA_JSON

        {

            get { return this.PROCESS_DATA_JSONValue; }

            set { SetProperty(ref PROCESS_DATA_JSONValue, value); }

        }

    }
}
