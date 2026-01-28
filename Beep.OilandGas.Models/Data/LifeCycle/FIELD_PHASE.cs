using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public partial class FIELD_PHASE : ModelEntityBase
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

        private System.String REMARKValue;

        private System.String SOURCEValue;

        public FIELD_PHASE() { }
    }
}
