using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class JOURNAL_ENTRY_LINE : ModelEntityBase {
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

        private System.String REMARKValue;

        private System.String SOURCEValue;

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }

        private System.String COST_CENTER_IDValue;
        public System.String COST_CENTER_ID
        {
            get { return this.COST_CENTER_IDValue; }
            set { SetProperty(ref COST_CENTER_IDValue, value); }
        }

        private System.String CURRENCY_CODEValue;
        public System.String CURRENCY_CODE
        {
            get { return this.CURRENCY_CODEValue; }
            set { SetProperty(ref CURRENCY_CODEValue, value); }
        }

        private System.Decimal? EXCHANGE_RATEValue;
        public System.Decimal? EXCHANGE_RATE
        {
            get { return this.EXCHANGE_RATEValue; }
            set { SetProperty(ref EXCHANGE_RATEValue, value); }
        }

        private System.Decimal? SHADOW_DEBIT_AMOUNTValue;
        public System.Decimal? SHADOW_DEBIT_AMOUNT
        {
            get { return this.SHADOW_DEBIT_AMOUNTValue; }
            set { SetProperty(ref SHADOW_DEBIT_AMOUNTValue, value); }
        }

        private System.Decimal? SHADOW_CREDIT_AMOUNTValue;
        public System.Decimal? SHADOW_CREDIT_AMOUNT
        {
            get { return this.SHADOW_CREDIT_AMOUNTValue; }
            set { SetProperty(ref SHADOW_CREDIT_AMOUNTValue, value); }
        }
    }
}
