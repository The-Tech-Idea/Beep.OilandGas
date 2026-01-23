using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Validation
{
    public partial class VALIDATION_RESULT : ModelEntityBase
    {
        private System.String VALIDATION_RESULT_IDValue;
        public System.String VALIDATION_RESULT_ID
        {
            get { return this.VALIDATION_RESULT_IDValue; }
            set { SetProperty(ref VALIDATION_RESULT_IDValue, value); }
        }

        private System.String ENTITY_TYPEValue;
        public System.String ENTITY_TYPE
        {
            get { return this.ENTITY_TYPEValue; }
            set { SetProperty(ref ENTITY_TYPEValue, value); }
        }

        private System.String ENTITY_IDValue;
        public System.String ENTITY_ID
        {
            get { return this.ENTITY_IDValue; }
            set { SetProperty(ref ENTITY_IDValue, value); }
        }

        private System.DateTime? VALIDATION_DATEValue;
        public System.DateTime? VALIDATION_DATE
        {
            get { return this.VALIDATION_DATEValue; }
            set { SetProperty(ref VALIDATION_DATEValue, value); }
        }

        private System.String IS_VALIDValue;
        public System.String IS_VALID
        {
            get { return this.IS_VALIDValue; }
            set { SetProperty(ref IS_VALIDValue, value); }
        }

        private System.String VALIDATION_ERRORSValue;
        public System.String VALIDATION_ERRORS
        {
            get { return this.VALIDATION_ERRORSValue; }
            set { SetProperty(ref VALIDATION_ERRORSValue, value); }
        }

        private System.String VALIDATION_WARNINGSValue;
        public System.String VALIDATION_WARNINGS
        {
            get { return this.VALIDATION_WARNINGSValue; }
            set { SetProperty(ref VALIDATION_WARNINGSValue, value); }
        }

        private System.String VALIDATED_BYValue;
        public System.String VALIDATED_BY
        {
            get { return this.VALIDATED_BYValue; }
            set { SetProperty(ref VALIDATED_BYValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}


