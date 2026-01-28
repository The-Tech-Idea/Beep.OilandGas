using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class AP_CREDIT_MEMO : ModelEntityBase {
        private System.String AP_CREDIT_MEMO_IDValue;
        public System.String AP_CREDIT_MEMO_ID
        {
            get { return this.AP_CREDIT_MEMO_IDValue; }
            set { SetProperty(ref AP_CREDIT_MEMO_IDValue, value); }
        }

        private System.String AP_INVOICE_IDValue;
        public System.String AP_INVOICE_ID
        {
            get { return this.AP_INVOICE_IDValue; }
            set { SetProperty(ref AP_INVOICE_IDValue, value); }
        }

        private System.String CREDIT_MEMO_NUMBERValue;
        public System.String CREDIT_MEMO_NUMBER
        {
            get { return this.CREDIT_MEMO_NUMBERValue; }
            set { SetProperty(ref CREDIT_MEMO_NUMBERValue, value); }
        }

        private System.DateTime? CREDIT_MEMO_DATEValue;
        public System.DateTime? CREDIT_MEMO_DATE
        {
            get { return this.CREDIT_MEMO_DATEValue; }
            set { SetProperty(ref CREDIT_MEMO_DATEValue, value); }
        }

        private System.Decimal? CREDIT_AMOUNTValue;
        public System.Decimal? CREDIT_AMOUNT
        {
            get { return this.CREDIT_AMOUNTValue; }
            set { SetProperty(ref CREDIT_AMOUNTValue, value); }
        }

        private System.String REASONValue;
        public System.String REASON
        {
            get { return this.REASONValue; }
            set { SetProperty(ref REASONValue, value); }
        }

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
