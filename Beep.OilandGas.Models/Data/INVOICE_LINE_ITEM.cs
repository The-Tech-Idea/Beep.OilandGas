using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data
{
    public partial class INVOICE_LINE_ITEM : Entity
    {
        private System.String INVOICE_LINE_ITEM_IDValue;
        public System.String INVOICE_LINE_ITEM_ID
        {
            get { return this.INVOICE_LINE_ITEM_IDValue; }
            set { SetProperty(ref INVOICE_LINE_ITEM_IDValue, value); }
        }

        private System.String INVOICE_IDValue;
        public System.String INVOICE_ID
        {
            get { return this.INVOICE_IDValue; }
            set { SetProperty(ref INVOICE_IDValue, value); }
        }

        private System.Int32? LINE_NUMBERValue;
        public System.Int32? LINE_NUMBER
        {
            get { return this.LINE_NUMBERValue; }
            set { SetProperty(ref LINE_NUMBERValue, value); }
        }

        private System.String ITEM_DESCRIPTIONValue;
        public System.String ITEM_DESCRIPTION
        {
            get { return this.ITEM_DESCRIPTIONValue; }
            set { SetProperty(ref ITEM_DESCRIPTIONValue, value); }
        }

        private System.Decimal? QUANTITYValue;
        public System.Decimal? QUANTITY
        {
            get { return this.QUANTITYValue; }
            set { SetProperty(ref QUANTITYValue, value); }
        }

        private System.Decimal? UNIT_PRICEValue;
        public System.Decimal? UNIT_PRICE
        {
            get { return this.UNIT_PRICEValue; }
            set { SetProperty(ref UNIT_PRICEValue, value); }
        }

        private System.Decimal? LINE_TOTALValue;
        public System.Decimal? LINE_TOTAL
        {
            get { return this.LINE_TOTALValue; }
            set { SetProperty(ref LINE_TOTALValue, value); }
        }

        private System.String GL_ACCOUNT_IDValue;
        public System.String GL_ACCOUNT_ID
        {
            get { return this.GL_ACCOUNT_IDValue; }
            set { SetProperty(ref GL_ACCOUNT_IDValue, value); }
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

