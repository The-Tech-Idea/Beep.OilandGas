using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class RECEIVABLE : Entity,IPPDMEntity
    {
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
        private System.String ACTIVE_INDValue;
        public System.String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private System.String PPDM_GUIDValue;
        public System.String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private System.String REMARKValue;
        public System.String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private System.String ROW_QUALITYValue;
        public System.String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private System.DateTime? ROW_EFFECTIVE_DATEValue;
        public System.DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private System.DateTime? ROW_EXPIRY_DATEValue;
        public System.DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}

