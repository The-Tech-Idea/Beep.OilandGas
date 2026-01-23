using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class JOINT_OPERATING_AGREEMENT : ModelEntityBase {
        private System.String JOA_IDValue;
        public System.String JOA_ID
        {
            get { return this.JOA_IDValue; }
            set { SetProperty(ref JOA_IDValue, value); }
        }

        private System.String JOA_NUMBERValue;
        public System.String JOA_NUMBER
        {
            get { return this.JOA_NUMBERValue; }
            set { SetProperty(ref JOA_NUMBERValue, value); }
        }

        private System.String JOA_NAMEValue;
        public System.String JOA_NAME
        {
            get { return this.JOA_NAMEValue; }
            set { SetProperty(ref JOA_NAMEValue, value); }
        }

        private System.String FIELD_IDValue;
        public System.String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private System.String OPERATOR_BA_IDValue;
        public System.String OPERATOR_BA_ID
        {
            get { return this.OPERATOR_BA_IDValue; }
            set { SetProperty(ref OPERATOR_BA_IDValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRY_DATEValue;

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}


