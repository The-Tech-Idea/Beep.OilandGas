using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class SalesTransaction : ModelEntityBase
    {
        private System.String SALES_TRANSACTION_IDValue;
        public System.String SALES_TRANSACTION_ID
        {
            get { return this.SALES_TRANSACTION_IDValue; }
            set { SetProperty(ref SALES_TRANSACTION_IDValue, value); }
        }

        private System.String RUN_TICKET_IDValue;
        public System.String RUN_TICKET_ID
        {
            get { return this.RUN_TICKET_IDValue; }
            set { SetProperty(ref RUN_TICKET_IDValue, value); }
        }

        private System.String SALES_AGREEMENT_IDValue;
        public System.String SALES_AGREEMENT_ID
        {
            get { return this.SALES_AGREEMENT_IDValue; }
            set { SetProperty(ref SALES_AGREEMENT_IDValue, value); }
        }

        private System.String CUSTOMER_BA_IDValue;
        public System.String CUSTOMER_BA_ID
        {
            get { return this.CUSTOMER_BA_IDValue; }
            set { SetProperty(ref CUSTOMER_BA_IDValue, value); }
        }

        private System.DateTime? SALES_DATEValue;
        public System.DateTime? SALES_DATE
        {
            get { return this.SALES_DATEValue; }
            set { SetProperty(ref SALES_DATEValue, value); }
        }

        private System.Decimal? NET_VOLUMEValue;
        public System.Decimal? NET_VOLUME
        {
            get { return this.NET_VOLUMEValue; }
            set { SetProperty(ref NET_VOLUMEValue, value); }
        }

        private System.Decimal? PRICE_PER_BARRELValue;
        public System.Decimal? PRICE_PER_BARREL
        {
            get { return this.PRICE_PER_BARRELValue; }
            set { SetProperty(ref PRICE_PER_BARRELValue, value); }
        }

        private System.Decimal? TOTAL_AMOUNTValue;
        public System.Decimal? TOTAL_AMOUNT
        {
            get { return this.TOTAL_AMOUNTValue; }
            set { SetProperty(ref TOTAL_AMOUNTValue, value); }
        }

        private System.Decimal? TOTAL_COSTSValue;
        public System.Decimal? TOTAL_COSTS
        {
            get { return this.TOTAL_COSTSValue; }
            set { SetProperty(ref TOTAL_COSTSValue, value); }
        }

        private System.Decimal? TOTAL_TAXESValue;
        public System.Decimal? TOTAL_TAXES
        {
            get { return this.TOTAL_TAXESValue; }
            set { SetProperty(ref TOTAL_TAXESValue, value); }
        }

        private System.Decimal? NET_REVENUEValue;
        public System.Decimal? NET_REVENUE
        {
            get { return this.NET_REVENUEValue; }
            set { SetProperty(ref NET_REVENUEValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.String APPROVAL_STATUSValue;
        public System.String APPROVAL_STATUS
        {
            get { return this.APPROVAL_STATUSValue; }
            set { SetProperty(ref APPROVAL_STATUSValue, value); }
        }

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}


