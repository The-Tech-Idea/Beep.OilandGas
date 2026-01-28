using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ROYALTY_PAYMENT_DETAIL : ModelEntityBase {
        private System.String ROYALTY_PAYMENT_DETAIL_IDValue;
        public System.String ROYALTY_PAYMENT_DETAIL_ID
        {
            get { return this.ROYALTY_PAYMENT_DETAIL_IDValue; }
            set { SetProperty(ref ROYALTY_PAYMENT_DETAIL_IDValue, value); }
        }

        private System.String ROYALTY_PAYMENT_IDValue;
        public System.String ROYALTY_PAYMENT_ID
        {
            get { return this.ROYALTY_PAYMENT_IDValue; }
            set { SetProperty(ref ROYALTY_PAYMENT_IDValue, value); }
        }

        private System.String REVENUE_TRANSACTION_IDValue;
        public System.String REVENUE_TRANSACTION_ID
        {
            get { return this.REVENUE_TRANSACTION_IDValue; }
            set { SetProperty(ref REVENUE_TRANSACTION_IDValue, value); }
        }

        private System.Decimal? OIL_VOLUMEValue;
        public System.Decimal? OIL_VOLUME
        {
            get { return this.OIL_VOLUMEValue; }
            set { SetProperty(ref OIL_VOLUMEValue, value); }
        }

        private System.Decimal? GAS_VOLUMEValue;
        public System.Decimal? GAS_VOLUME
        {
            get { return this.GAS_VOLUMEValue; }
            set { SetProperty(ref GAS_VOLUMEValue, value); }
        }

        private System.Decimal? OIL_REVENUEValue;
        public System.Decimal? OIL_REVENUE
        {
            get { return this.OIL_REVENUEValue; }
            set { SetProperty(ref OIL_REVENUEValue, value); }
        }

        private System.Decimal? GAS_REVENUEValue;
        public System.Decimal? GAS_REVENUE
        {
            get { return this.GAS_REVENUEValue; }
            set { SetProperty(ref GAS_REVENUEValue, value); }
        }

        private System.Decimal? ROYALTY_AMOUNTValue;
        public System.Decimal? ROYALTY_AMOUNT
        {
            get { return this.ROYALTY_AMOUNTValue; }
            set { SetProperty(ref ROYALTY_AMOUNTValue, value); }
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
