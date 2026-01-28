using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
    public class PROCESS_DEFINITION : ModelEntityBase
    {
        private string? PROCESS_DEFINITION_IDValue;

        public string? PROCESS_DEFINITION_ID

        {

            get { return this.PROCESS_DEFINITION_IDValue; }

            set { SetProperty(ref PROCESS_DEFINITION_IDValue, value); }

        }
        private string? PROCESS_NAMEValue;

        public string? PROCESS_NAME

        {

            get { return this.PROCESS_NAMEValue; }

            set { SetProperty(ref PROCESS_NAMEValue, value); }

        }
        private string? PROCESS_TYPEValue;

        public string? PROCESS_TYPE

        {

            get { return this.PROCESS_TYPEValue; }

            set { SetProperty(ref PROCESS_TYPEValue, value); }

        }
        private string? ENTITY_TYPEValue;

        public string? ENTITY_TYPE

        {

            get { return this.ENTITY_TYPEValue; }

            set { SetProperty(ref ENTITY_TYPEValue, value); }

        }
        private string? DESCRIPTIONValue;

        public string? DESCRIPTION

        {

            get { return this.DESCRIPTIONValue; }

            set { SetProperty(ref DESCRIPTIONValue, value); }

        }
        private string? PROCESS_CONFIG_JSONValue;

        public string? PROCESS_CONFIG_JSON

        {

            get { return this.PROCESS_CONFIG_JSONValue; }

            set { SetProperty(ref PROCESS_CONFIG_JSONValue, value); }

        }

    }
}
