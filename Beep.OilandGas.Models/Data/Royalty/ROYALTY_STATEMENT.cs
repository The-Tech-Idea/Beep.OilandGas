using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Royalty
{
    public partial class ROYALTY_STATEMENT : ModelEntityBase
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

        private System.Decimal  TOTAL_ROYALTY_AMOUNTValue;
        public System.Decimal  TOTAL_ROYALTY_AMOUNT
        {
            get { return this.TOTAL_ROYALTY_AMOUNTValue; }
            set { SetProperty(ref TOTAL_ROYALTY_AMOUNTValue, value); }
        }

        private System.Decimal  TOTAL_DEDUCTIONSValue;
        public System.Decimal  TOTAL_DEDUCTIONS
        {
            get { return this.TOTAL_DEDUCTIONSValue; }
            set { SetProperty(ref TOTAL_DEDUCTIONSValue, value); }
        }

        private System.Decimal  NET_PAYMENT_AMOUNTValue;
        public System.Decimal  NET_PAYMENT_AMOUNT
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

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}
