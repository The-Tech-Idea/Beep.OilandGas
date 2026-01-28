using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class CEILING_TEST_CALCULATION : ModelEntityBase {
        private System.String CEILING_TEST_CALCULATION_IDValue;
        public System.String CEILING_TEST_CALCULATION_ID
        {
            get { return this.CEILING_TEST_CALCULATION_IDValue; }
            set { SetProperty(ref CEILING_TEST_CALCULATION_IDValue, value); }
        }

        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.DateTime? CALCULATION_DATEValue;
        public System.DateTime? CALCULATION_DATE
        {
            get { return this.CALCULATION_DATEValue; }
            set { SetProperty(ref CALCULATION_DATEValue, value); }
        }

        private System.Decimal? NET_CAPITALIZED_COSTValue;
        public System.Decimal? NET_CAPITALIZED_COST
        {
            get { return this.NET_CAPITALIZED_COSTValue; }
            set { SetProperty(ref NET_CAPITALIZED_COSTValue, value); }
        }

        private System.Decimal? DISCOUNTED_FUTURE_NET_CASH_FLOWSValue;
        public System.Decimal? DISCOUNTED_FUTURE_NET_CASH_FLOWS
        {
            get { return this.DISCOUNTED_FUTURE_NET_CASH_FLOWSValue; }
            set { SetProperty(ref DISCOUNTED_FUTURE_NET_CASH_FLOWSValue, value); }
        }

        private System.Decimal? CEILING_VALUEValue;
        public System.Decimal? CEILING_VALUE
        {
            get { return this.CEILING_VALUEValue; }
            set { SetProperty(ref CEILING_VALUEValue, value); }
        }

        private System.Decimal? IMPAIRMENT_AMOUNTValue;
        public System.Decimal? IMPAIRMENT_AMOUNT
        {
            get { return this.IMPAIRMENT_AMOUNTValue; }
            set { SetProperty(ref IMPAIRMENT_AMOUNTValue, value); }
        }

        private System.Decimal? DISCOUNT_RATEValue;
        public System.Decimal? DISCOUNT_RATE
        {
            get { return this.DISCOUNT_RATEValue; }
            set { SetProperty(ref DISCOUNT_RATEValue, value); }
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
