using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public partial class CALCULATION_RESULT : Entity
    {
        private System.String CALCULATION_RESULT_IDValue;
        public System.String CALCULATION_RESULT_ID
        {
            get { return this.CALCULATION_RESULT_IDValue; }
            set { SetProperty(ref CALCULATION_RESULT_IDValue, value); }
        }

        private System.String CALCULATION_TYPEValue;
        public System.String CALCULATION_TYPE
        {
            get { return this.CALCULATION_TYPEValue; }
            set { SetProperty(ref CALCULATION_TYPEValue, value); }
        }

        private System.DateTime? CALCULATION_DATEValue;
        public System.DateTime? CALCULATION_DATE
        {
            get { return this.CALCULATION_DATEValue; }
            set { SetProperty(ref CALCULATION_DATEValue, value); }
        }

        private System.String INPUT_DATAValue;
        public System.String INPUT_DATA
        {
            get { return this.INPUT_DATAValue; }
            set { SetProperty(ref INPUT_DATAValue, value); }
        }

        private System.String RESULT_DATAValue;
        public System.String RESULT_DATA
        {
            get { return this.RESULT_DATAValue; }
            set { SetProperty(ref RESULT_DATAValue, value); }
        }

        private System.String CALCULATED_BYValue;
        public System.String CALCULATED_BY
        {
            get { return this.CALCULATED_BYValue; }
            set { SetProperty(ref CALCULATED_BYValue, value); }
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

