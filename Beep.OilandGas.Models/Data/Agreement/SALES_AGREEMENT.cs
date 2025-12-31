using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Agreement
{
    public partial class SALES_AGREEMENT : Entity
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
        public System.DateTime? EFFECTIVE_DATE
        {
            get { return this.EFFECTIVE_DATEValue; }
            set { SetProperty(ref EFFECTIVE_DATEValue, value); }
        }

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

        private System.Decimal? BASE_PRICEValue;
        public System.Decimal? BASE_PRICE
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

        private System.Decimal? DIFFERENTIALValue;
        public System.Decimal? DIFFERENTIAL
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

