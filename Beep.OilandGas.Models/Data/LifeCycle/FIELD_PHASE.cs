using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public partial class FIELD_PHASE : Entity,IPPDMEntity
    {
        private System.String FIELD_PHASE_IDValue;
        public System.String FIELD_PHASE_ID
        {
            get
            {
                return this.FIELD_PHASE_IDValue;
            }
            set { SetProperty(ref FIELD_PHASE_IDValue, value); }
        }

        private System.String FIELD_IDValue;
        public System.String FIELD_ID
        {
            get
            {
                return this.FIELD_IDValue;
            }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private System.String PHASEValue;
        public System.String PHASE
        {
            get
            {
                return this.PHASEValue;
            }
            set { SetProperty(ref PHASEValue, value); }
        }

        private System.DateTime? PHASE_START_DATEValue;
        public System.DateTime? PHASE_START_DATE
        {
            get
            {
                return this.PHASE_START_DATEValue;
            }
            set { SetProperty(ref PHASE_START_DATEValue, value); }
        }

        private System.DateTime? PHASE_END_DATEValue;
        public System.DateTime? PHASE_END_DATE
        {
            get
            {
                return this.PHASE_END_DATEValue;
            }
            set { SetProperty(ref PHASE_END_DATEValue, value); }
        }

        private System.String PHASE_STATUSValue;
        public System.String PHASE_STATUS
        {
            get
            {
                return this.PHASE_STATUSValue;
            }
            set { SetProperty(ref PHASE_STATUSValue, value); }
        }

        private System.String TRANSITION_REASONValue;
        public System.String TRANSITION_REASON
        {
            get
            {
                return this.TRANSITION_REASONValue;
            }
            set { SetProperty(ref TRANSITION_REASONValue, value); }
        }

        private System.String ACTIVE_INDValue;
        public System.String ACTIVE_IND
        {
            get
            {
                return this.ACTIVE_INDValue;
            }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private System.String PPDM_GUIDValue;
        public System.String PPDM_GUID
        {
            get
            {
                return this.PPDM_GUIDValue;
            }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private System.String REMARKValue;
        public System.String REMARK
        {
            get
            {
                return this.REMARKValue;
            }
            set { SetProperty(ref REMARKValue, value); }
        }

        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get
            {
                return this.SOURCEValue;
            }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get
            {
                return this.ROW_CHANGED_BYValue;
            }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get
            {
                return this.ROW_CHANGED_DATEValue;
            }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get
            {
                return this.ROW_CREATED_BYValue;
            }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get
            {
                return this.ROW_CREATED_DATEValue;
            }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.DateTime? ROW_EFFECTIVE_DATEValue;
        public System.DateTime? ROW_EFFECTIVE_DATE
        {
            get
            {
                return this.ROW_EFFECTIVE_DATEValue;
            }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private System.DateTime? ROW_EXPIRY_DATEValue;
        public System.DateTime? ROW_EXPIRY_DATE
        {
            get
            {
                return this.ROW_EXPIRY_DATEValue;
            }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private System.String ROW_QUALITYValue;
        public System.String ROW_QUALITY
        {
            get
            {
                return this.ROW_QUALITYValue;
            }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

        public FIELD_PHASE() { }
    }
}




