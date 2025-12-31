using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data
{
    public partial class SALES_CONTRACT : Entity,IPPDMEntity
    {
        private System.String SALES_CONTRACT_IDValue;
        public System.String SALES_CONTRACT_ID
        {
            get { return this.SALES_CONTRACT_IDValue; }
            set { SetProperty(ref SALES_CONTRACT_IDValue, value); }
        }

        private System.String CONTRACT_NUMBERValue;
        public System.String CONTRACT_NUMBER
        {
            get { return this.CONTRACT_NUMBERValue; }
            set { SetProperty(ref CONTRACT_NUMBERValue, value); }
        }

        private System.String BUYER_BA_IDValue;
        public System.String BUYER_BA_ID
        {
            get { return this.BUYER_BA_IDValue; }
            set { SetProperty(ref BUYER_BA_IDValue, value); }
        }

        private System.String SELLER_BA_IDValue;
        public System.String SELLER_BA_ID
        {
            get { return this.SELLER_BA_IDValue; }
            set { SetProperty(ref SELLER_BA_IDValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;
        public System.DateTime? EFFECTIVE_DATE
        {
            get { return this.EFFECTIVE_DATEValue; }
            set { SetProperty(ref EFFECTIVE_DATEValue, value); }
        }

        private System.DateTime? EXPIRY_DATEValue;
        public System.DateTime? EXPIRY_DATE
        {
            get { return this.EXPIRY_DATEValue; }
            set { SetProperty(ref EXPIRY_DATEValue, value); }
        }

        private System.String COMMODITY_TYPEValue;
        public System.String COMMODITY_TYPE
        {
            get { return this.COMMODITY_TYPEValue; }
            set { SetProperty(ref COMMODITY_TYPEValue, value); }
        }

        private System.Decimal? BASE_PRICEValue;
        public System.Decimal? BASE_PRICE
        {
            get { return this.BASE_PRICEValue; }
            set { SetProperty(ref BASE_PRICEValue, value); }
        }

        private System.String PRICING_METHODValue;
        public System.String PRICING_METHOD
        {
            get { return this.PRICING_METHODValue; }
            set { SetProperty(ref PRICING_METHODValue, value); }
        }

        private System.String CURRENCY_CODEValue;
        public System.String CURRENCY_CODE
        {
            get { return this.CURRENCY_CODEValue; }
            set { SetProperty(ref CURRENCY_CODEValue, value); }
        }

        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
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

        private System.String ROW_QUALITYValue;
        public System.String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
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

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}

