using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class JOURNAL_ENTRY_LINE : Entity,IPPDMEntity
    {
        private System.String JOURNAL_ENTRY_LINE_IDValue;
        public System.String JOURNAL_ENTRY_LINE_ID
        {
            get { return this.JOURNAL_ENTRY_LINE_IDValue; }
            set { SetProperty(ref JOURNAL_ENTRY_LINE_IDValue, value); }
        }

        private System.String JOURNAL_ENTRY_IDValue;
        public System.String JOURNAL_ENTRY_ID
        {
            get { return this.JOURNAL_ENTRY_IDValue; }
            set { SetProperty(ref JOURNAL_ENTRY_IDValue, value); }
        }

        private System.String GL_ACCOUNT_IDValue;
        public System.String GL_ACCOUNT_ID
        {
            get { return this.GL_ACCOUNT_IDValue; }
            set { SetProperty(ref GL_ACCOUNT_IDValue, value); }
        }

        private System.Int32? LINE_NUMBERValue;
        public System.Int32? LINE_NUMBER
        {
            get { return this.LINE_NUMBERValue; }
            set { SetProperty(ref LINE_NUMBERValue, value); }
        }

        private System.Decimal? DEBIT_AMOUNTValue;
        public System.Decimal? DEBIT_AMOUNT
        {
            get { return this.DEBIT_AMOUNTValue; }
            set { SetProperty(ref DEBIT_AMOUNTValue, value); }
        }

        private System.Decimal? CREDIT_AMOUNTValue;
        public System.Decimal? CREDIT_AMOUNT
        {
            get { return this.CREDIT_AMOUNTValue; }
            set { SetProperty(ref CREDIT_AMOUNTValue, value); }
        }

        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
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

