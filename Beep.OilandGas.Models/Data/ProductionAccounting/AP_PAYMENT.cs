using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class AP_PAYMENT : ModelEntityBase {
        private System.String AP_PAYMENT_IDValue;
        public System.String AP_PAYMENT_ID
        {
            get { return this.AP_PAYMENT_IDValue; }
            set { SetProperty(ref AP_PAYMENT_IDValue, value); }
        }

        private System.String AP_INVOICE_IDValue;
        public System.String AP_INVOICE_ID
        {
            get { return this.AP_INVOICE_IDValue; }
            set { SetProperty(ref AP_INVOICE_IDValue, value); }
        }

        private System.String PAYMENT_NUMBERValue;
        public System.String PAYMENT_NUMBER
        {
            get { return this.PAYMENT_NUMBERValue; }
            set { SetProperty(ref PAYMENT_NUMBERValue, value); }
        }

        private System.DateTime? PAYMENT_DATEValue;
        public System.DateTime? PAYMENT_DATE
        {
            get { return this.PAYMENT_DATEValue; }
            set { SetProperty(ref PAYMENT_DATEValue, value); }
        }

        private System.Decimal? PAYMENT_AMOUNTValue;
        public System.Decimal? PAYMENT_AMOUNT
        {
            get { return this.PAYMENT_AMOUNTValue; }
            set { SetProperty(ref PAYMENT_AMOUNTValue, value); }
        }

        private System.String PAYMENT_METHODValue;
        public System.String PAYMENT_METHOD
        {
            get { return this.PAYMENT_METHODValue; }
            set { SetProperty(ref PAYMENT_METHODValue, value); }
        }

        private System.String CHECK_NUMBERValue;
        public System.String CHECK_NUMBER
        {
            get { return this.CHECK_NUMBERValue; }
            set { SetProperty(ref CHECK_NUMBERValue, value); }
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

        private System.String PAYMENT_STATUSValue = string.Empty;
        public System.String PAYMENT_STATUS
        {
            get { return this.PAYMENT_STATUSValue; }
            set { SetProperty(ref PAYMENT_STATUSValue, value); }
        }
    }
}
