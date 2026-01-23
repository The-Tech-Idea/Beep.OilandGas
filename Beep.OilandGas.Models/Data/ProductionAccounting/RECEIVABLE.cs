using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class RECEIVABLE : ModelEntityBase {
        private System.String RECEIVABLE_IDValue;
        public System.String RECEIVABLE_ID
        {
            get { return this.RECEIVABLE_IDValue; }
            set { SetProperty(ref RECEIVABLE_IDValue, value); }
        }

        private System.String TRANSACTION_IDValue;
        public System.String TRANSACTION_ID
        {
            get { return this.TRANSACTION_IDValue; }
            set { SetProperty(ref TRANSACTION_IDValue, value); }
        }

        private System.String CUSTOMERValue;
        public System.String CUSTOMER
        {
            get { return this.CUSTOMERValue; }
            set { SetProperty(ref CUSTOMERValue, value); }
        }

        private System.String INVOICE_NUMBERValue;
        public System.String INVOICE_NUMBER
        {
            get { return this.INVOICE_NUMBERValue; }
            set { SetProperty(ref INVOICE_NUMBERValue, value); }
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

        private System.Decimal? ORIGINAL_AMOUNTValue;
        public System.Decimal? ORIGINAL_AMOUNT
        {
            get { return this.ORIGINAL_AMOUNTValue; }
            set { SetProperty(ref ORIGINAL_AMOUNTValue, value); }
        }

        private System.Decimal? AMOUNT_PAIDValue;
        public System.Decimal? AMOUNT_PAID
        {
            get { return this.AMOUNT_PAIDValue; }
            set { SetProperty(ref AMOUNT_PAIDValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
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


