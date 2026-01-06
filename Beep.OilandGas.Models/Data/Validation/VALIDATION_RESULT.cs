using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Validation
{
    public partial class VALIDATION_RESULT : Entity
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
        private System.String ACTIVE_INDValue;
        public System.String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private System.String PPDM_GUIDValue;
        public System.String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private System.String REMARKValue;
        public System.String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private System.DateTime? ROW_EFFECTIVE_DATEValue;
        public System.DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private System.DateTime? ROW_EXPIRY_DATEValue;
        public System.DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }
    }
}




