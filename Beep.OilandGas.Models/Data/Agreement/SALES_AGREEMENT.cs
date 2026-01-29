using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Agreement
{
    public partial class SALES_AGREEMENT : ModelEntityBase
    {
        private System.String AGREEMENT_IDValue;
        public System.String AGREEMENT_ID
        {
            get { return this.AGREEMENT_IDValue; }
            set { SetProperty(ref AGREEMENT_IDValue, value); }
        }

        private System.String AGREEMENT_NAMEValue;
        public System.String AGREEMENT_NAME
        {
            get { return this.AGREEMENT_NAMEValue; }
            set { SetProperty(ref AGREEMENT_NAMEValue, value); }
        }

        private System.String SELLER_BA_IDValue;
        public System.String SELLER_BA_ID
        {
            get { return this.SELLER_BA_IDValue; }
            set { SetProperty(ref SELLER_BA_IDValue, value); }
        }

        private System.String PURCHASER_BA_IDValue;
        public System.String PURCHASER_BA_ID
        {
            get { return this.PURCHASER_BA_IDValue; }
            set { SetProperty(ref PURCHASER_BA_IDValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRATION_DATEValue;
        public System.DateTime? EXPIRATION_DATE
        {
            get { return this.EXPIRATION_DATEValue; }
            set { SetProperty(ref EXPIRATION_DATEValue, value); }
        }

        private System.String PRICING_METHODValue;
        public System.String PRICING_METHOD
        {
            get { return this.PRICING_METHODValue; }
            set { SetProperty(ref PRICING_METHODValue, value); }
        }

        private System.Decimal  BASE_PRICEValue;
        public System.Decimal  BASE_PRICE
        {
            get { return this.BASE_PRICEValue; }
            set { SetProperty(ref BASE_PRICEValue, value); }
        }

        private System.String PRICE_INDEXValue;
        public System.String PRICE_INDEX
        {
            get { return this.PRICE_INDEXValue; }
            set { SetProperty(ref PRICE_INDEXValue, value); }
        }

        private System.Decimal  DIFFERENTIALValue;
        public System.Decimal  DIFFERENTIAL
        {
            get { return this.DIFFERENTIALValue; }
            set { SetProperty(ref DIFFERENTIALValue, value); }
        }

        private System.Int32? PAYMENT_TERMS_DAYSValue;
        public System.Int32? PAYMENT_TERMS_DAYS
        {
            get { return this.PAYMENT_TERMS_DAYSValue; }
            set { SetProperty(ref PAYMENT_TERMS_DAYSValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}
