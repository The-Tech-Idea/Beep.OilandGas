using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Entity for storing validation rules configuration
    /// </summary>
    public partial class DATA_VALIDATION_RULE : ModelEntityBase
    {
        private System.String VALIDATION_RULE_IDValue;
        public System.String VALIDATION_RULE_ID
        {
            get { return this.VALIDATION_RULE_IDValue; }
            set { SetProperty(ref VALIDATION_RULE_IDValue, value); }
        }

        private System.String TABLE_NAMEValue;
        public System.String TABLE_NAME
        {
            get { return this.TABLE_NAMEValue; }
            set { SetProperty(ref TABLE_NAMEValue, value); }
        }

        private System.String FIELD_NAMEValue;
        public System.String FIELD_NAME
        {
            get { return this.FIELD_NAMEValue; }
            set { SetProperty(ref FIELD_NAMEValue, value); }
        }

        private System.String RULE_NAMEValue;
        public System.String RULE_NAME
        {
            get { return this.RULE_NAMEValue; }
            set { SetProperty(ref RULE_NAMEValue, value); }
        }

        private System.String RULE_TYPEValue;
        public System.String RULE_TYPE
        {
            get { return this.RULE_TYPEValue; }
            set { SetProperty(ref RULE_TYPEValue, value); }
        }

        private System.String RULE_VALUEValue;
        public System.String RULE_VALUE
        {
            get { return this.RULE_VALUEValue; }
            set { SetProperty(ref RULE_VALUEValue, value); }
        }

        private System.String ERROR_MESSAGEValue;
        public System.String ERROR_MESSAGE
        {
            get { return this.ERROR_MESSAGEValue; }
            set { SetProperty(ref ERROR_MESSAGEValue, value); }
        }

        private System.String SEVERITYValue;
        public System.String SEVERITY
        {
            get { return this.SEVERITYValue; }
            set { SetProperty(ref SEVERITYValue, value); }
        }

        // Standard PPDM columns

        private System.String SOURCEValue;

        private System.String REMARKValue;

        // Optional IPPDMEntity properties
        private System.String AREA_IDValue;
        public System.String AREA_ID
        {
            get { return this.AREA_IDValue; }
            set { SetProperty(ref AREA_IDValue, value); }
        }

        private System.String AREA_TYPEValue;
        public System.String AREA_TYPE
        {
            get { return this.AREA_TYPEValue; }
            set { SetProperty(ref AREA_TYPEValue, value); }
        }

        private System.String BUSINESS_ASSOCIATE_IDValue;
        public System.String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRY_DATEValue;

    }
}


