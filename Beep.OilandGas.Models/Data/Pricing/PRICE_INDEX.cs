using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Pricing
{
    public partial class PRICE_INDEX : Entity
    {
        private System.String PRICE_INDEX_IDValue;
        public System.String PRICE_INDEX_ID
        {
            get { return this.PRICE_INDEX_IDValue; }
            set { SetProperty(ref PRICE_INDEX_IDValue, value); }
        }

        private System.String INDEX_NAMEValue;
        public System.String INDEX_NAME
        {
            get { return this.INDEX_NAMEValue; }
            set { SetProperty(ref INDEX_NAMEValue, value); }
        }

        private System.String COMMODITY_TYPEValue;
        public System.String COMMODITY_TYPE
        {
            get { return this.COMMODITY_TYPEValue; }
            set { SetProperty(ref COMMODITY_TYPEValue, value); }
        }

        private System.DateTime? PRICE_DATEValue;
        public System.DateTime? PRICE_DATE
        {
            get { return this.PRICE_DATEValue; }
            set { SetProperty(ref PRICE_DATEValue, value); }
        }

        private System.Decimal? PRICE_VALUEValue;
        public System.Decimal? PRICE_VALUE
        {
            get { return this.PRICE_VALUEValue; }
            set { SetProperty(ref PRICE_VALUEValue, value); }
        }

        private System.String CURRENCY_CODEValue;
        public System.String CURRENCY_CODE
        {
            get { return this.CURRENCY_CODEValue; }
            set { SetProperty(ref CURRENCY_CODEValue, value); }
        }

        private System.String PRICING_POINTValue;
        public System.String PRICING_POINT
        {
            get { return this.PRICING_POINTValue; }
            set { SetProperty(ref PRICING_POINTValue, value); }
        }

        private System.String UNITValue;
        public System.String UNIT
        {
            get { return this.UNITValue; }
            set { SetProperty(ref UNITValue, value); }
        }

        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
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

