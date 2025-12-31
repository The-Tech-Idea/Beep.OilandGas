using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Royalty
{
    public partial class ROYALTY_STATEMENT : Entity
    {
        private System.String ROYALTY_STATEMENT_IDValue;
        public System.String ROYALTY_STATEMENT_ID
        {
            get { return this.ROYALTY_STATEMENT_IDValue; }
            set { SetProperty(ref ROYALTY_STATEMENT_IDValue, value); }
        }

        private System.String ROYALTY_OWNER_BA_IDValue;
        public System.String ROYALTY_OWNER_BA_ID
        {
            get { return this.ROYALTY_OWNER_BA_IDValue; }
            set { SetProperty(ref ROYALTY_OWNER_BA_IDValue, value); }
        }

        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.DateTime? STATEMENT_PERIOD_STARTValue;
        public System.DateTime? STATEMENT_PERIOD_START
        {
            get { return this.STATEMENT_PERIOD_STARTValue; }
            set { SetProperty(ref STATEMENT_PERIOD_STARTValue, value); }
        }

        private System.DateTime? STATEMENT_PERIOD_ENDValue;
        public System.DateTime? STATEMENT_PERIOD_END
        {
            get { return this.STATEMENT_PERIOD_ENDValue; }
            set { SetProperty(ref STATEMENT_PERIOD_ENDValue, value); }
        }

        private System.Decimal? TOTAL_ROYALTY_AMOUNTValue;
        public System.Decimal? TOTAL_ROYALTY_AMOUNT
        {
            get { return this.TOTAL_ROYALTY_AMOUNTValue; }
            set { SetProperty(ref TOTAL_ROYALTY_AMOUNTValue, value); }
        }

        private System.Decimal? TOTAL_DEDUCTIONSValue;
        public System.Decimal? TOTAL_DEDUCTIONS
        {
            get { return this.TOTAL_DEDUCTIONSValue; }
            set { SetProperty(ref TOTAL_DEDUCTIONSValue, value); }
        }

        private System.Decimal? NET_PAYMENT_AMOUNTValue;
        public System.Decimal? NET_PAYMENT_AMOUNT
        {
            get { return this.NET_PAYMENT_AMOUNTValue; }
            set { SetProperty(ref NET_PAYMENT_AMOUNTValue, value); }
        }

        private System.DateTime? STATEMENT_DATEValue;
        public System.DateTime? STATEMENT_DATE
        {
            get { return this.STATEMENT_DATEValue; }
            set { SetProperty(ref STATEMENT_DATEValue, value); }
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

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
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
    }
}

