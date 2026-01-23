using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class INVOICE_PAYMENT : ModelEntityBase {
        private System.String INVOICE_PAYMENT_IDValue;
        public System.String INVOICE_PAYMENT_ID
        {
            get { return this.INVOICE_PAYMENT_IDValue; }
            set { SetProperty(ref INVOICE_PAYMENT_IDValue, value); }
        }

        private System.String INVOICE_IDValue;
        public System.String INVOICE_ID
        {
            get { return this.INVOICE_IDValue; }
            set { SetProperty(ref INVOICE_IDValue, value); }
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

        private System.String REFERENCE_NUMBERValue;
        public System.String REFERENCE_NUMBER
        {
            get { return this.REFERENCE_NUMBERValue; }
            set { SetProperty(ref REFERENCE_NUMBERValue, value); }
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


