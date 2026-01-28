using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class GL_ENTRY : ModelEntityBase {
        private System.String GL_ENTRY_IDValue;
        public System.String GL_ENTRY_ID
        {
            get { return this.GL_ENTRY_IDValue; }
            set { SetProperty(ref GL_ENTRY_IDValue, value); }
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

        private System.DateTime? ENTRY_DATEValue;
        public System.DateTime? ENTRY_DATE
        {
            get { return this.ENTRY_DATEValue; }
            set { SetProperty(ref ENTRY_DATEValue, value); }
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

        private System.String REFERENCE_NUMBERValue;
        public System.String REFERENCE_NUMBER
        {
            get { return this.REFERENCE_NUMBERValue; }
            set { SetProperty(ref REFERENCE_NUMBERValue, value); }
        }

        private System.String SOURCE_MODULEValue;
        public System.String SOURCE_MODULE
        {
            get { return this.SOURCE_MODULEValue; }
            set { SetProperty(ref SOURCE_MODULEValue, value); }
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
