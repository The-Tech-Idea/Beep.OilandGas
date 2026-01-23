using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ACCOUNTING_METHOD : ModelEntityBase {
        private System.String ACCOUNTING_METHOD_IDValue;
        public System.String ACCOUNTING_METHOD_ID
        {
            get { return this.ACCOUNTING_METHOD_IDValue; }
            set { SetProperty(ref ACCOUNTING_METHOD_IDValue, value); }
        }
        private System.String FIELD_IDValue;
        public System.String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private System.String COST_CENTER_IDValue;
        public System.String COST_CENTER_ID
        {
            get { return this.COST_CENTER_IDValue; }
            set { SetProperty(ref COST_CENTER_IDValue, value); }
        }

        private System.String METHOD_TYPEValue;
        public System.String METHOD_TYPE
        {
            get { return this.METHOD_TYPEValue; }
            set { SetProperty(ref METHOD_TYPEValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRY_DATEValue;

        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

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


