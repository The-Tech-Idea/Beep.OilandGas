using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class TAX_TRANSACTION : ModelEntityBase {
        private System.String TAX_TRANSACTION_IDValue;
        public System.String TAX_TRANSACTION_ID
        {
            get { return this.TAX_TRANSACTION_IDValue; }
            set { SetProperty(ref TAX_TRANSACTION_IDValue, value); }
        }

        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.String TAX_TYPEValue;
        public System.String TAX_TYPE
        {
            get { return this.TAX_TYPEValue; }
            set { SetProperty(ref TAX_TYPEValue, value); }
        }

        private System.DateTime? TAX_DATEValue;
        public System.DateTime? TAX_DATE
        {
            get { return this.TAX_DATEValue; }
            set { SetProperty(ref TAX_DATEValue, value); }
        }

        private System.Decimal? TAX_AMOUNTValue;
        public System.Decimal? TAX_AMOUNT
        {
            get { return this.TAX_AMOUNTValue; }
            set { SetProperty(ref TAX_AMOUNTValue, value); }
        }

        private System.String TAX_JURISDICTIONValue;
        public System.String TAX_JURISDICTION
        {
            get { return this.TAX_JURISDICTIONValue; }
            set { SetProperty(ref TAX_JURISDICTIONValue, value); }
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


