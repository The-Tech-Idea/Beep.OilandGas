using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class INVOICE : ModelEntityBase {
        private System.String INVOICE_IDValue;
        public System.String INVOICE_ID
        {
            get { return this.INVOICE_IDValue; }
            set { SetProperty(ref INVOICE_IDValue, value); }
        }

        private System.String INVOICE_NUMBERValue;
        public System.String INVOICE_NUMBER
        {
            get { return this.INVOICE_NUMBERValue; }
            set { SetProperty(ref INVOICE_NUMBERValue, value); }
        }

        private System.String CUSTOMER_BA_IDValue;
        public System.String CUSTOMER_BA_ID
        {
            get { return this.CUSTOMER_BA_IDValue; }
            set { SetProperty(ref CUSTOMER_BA_IDValue, value); }
        }

        private System.DateTime? INVOICE_DATEValue;
        public System.DateTime? INVOICE_DATE
        {
            get { return this.INVOICE_DATEValue; }
            set { SetProperty(ref INVOICE_DATEValue, value); }
        }

        private System.DateTime? DUE_DATEValue;
        public System.DateTime? DUE_DATE
        {
            get { return this.DUE_DATEValue; }
            set { SetProperty(ref DUE_DATEValue, value); }
        }

        private System.Decimal  SUBTOTALValue;
        public System.Decimal  SUBTOTAL
        {
            get { return this.SUBTOTALValue; }
            set { SetProperty(ref SUBTOTALValue, value); }
        }

        private System.Decimal  TAX_AMOUNTValue;
        public System.Decimal  TAX_AMOUNT
        {
            get { return this.TAX_AMOUNTValue; }
            set { SetProperty(ref TAX_AMOUNTValue, value); }
        }

        private System.Decimal  TOTAL_AMOUNTValue;
        public System.Decimal  TOTAL_AMOUNT
        {
            get { return this.TOTAL_AMOUNTValue; }
            set { SetProperty(ref TOTAL_AMOUNTValue, value); }
        }

        private System.Decimal  PAID_AMOUNTValue;
        public System.Decimal  PAID_AMOUNT
        {
            get { return this.PAID_AMOUNTValue; }
            set { SetProperty(ref PAID_AMOUNTValue, value); }
        }

        private System.Decimal  BALANCE_DUEValue;
        public System.Decimal  BALANCE_DUE
        {
            get { return this.BALANCE_DUEValue; }
            set { SetProperty(ref BALANCE_DUEValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.String CURRENCY_CODEValue;
        public System.String CURRENCY_CODE
        {
            get { return this.CURRENCY_CODEValue; }
            set { SetProperty(ref CURRENCY_CODEValue, value); }
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
