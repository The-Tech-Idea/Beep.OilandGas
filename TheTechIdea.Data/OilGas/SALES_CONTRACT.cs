using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class SALES_CONTRACT : ModelEntityBase {
        private string? termsWorkflowJson;
        [MaxLength(TheTechIdea.Data.OilGas.SalesContractTermsNativeProof.MaximumLength)]
        public string? TERMS_WORKFLOW_JSON { get => termsWorkflowJson; set => SetProperty(ref termsWorkflowJson,value); }
        private System.String SALES_CONTRACT_IDValue = string.Empty;
        [Key,MaxLength(128)] public System.String SALES_CONTRACT_ID
        {
            get { return this.SALES_CONTRACT_IDValue; }
            set { SetProperty(ref SALES_CONTRACT_IDValue, value); }
        }

        private System.String CONTRACT_NUMBERValue = string.Empty;
        public System.String CONTRACT_NUMBER
        {
            get { return this.CONTRACT_NUMBERValue; }
            set { SetProperty(ref CONTRACT_NUMBERValue, value); }
        }

        private System.String BUYER_BA_IDValue = string.Empty;
        public System.String BUYER_BA_ID
        {
            get { return this.BUYER_BA_IDValue; }
            set { SetProperty(ref BUYER_BA_IDValue, value); }
        }

        private System.String SELLER_BA_IDValue = string.Empty;
        public System.String SELLER_BA_ID
        {
            get { return this.SELLER_BA_IDValue; }
            set { SetProperty(ref SELLER_BA_IDValue, value); }
        }





        private System.String COMMODITY_TYPEValue = string.Empty;
        public System.String COMMODITY_TYPE
        {
            get { return this.COMMODITY_TYPEValue; }
            set { SetProperty(ref COMMODITY_TYPEValue, value); }
        }

        private System.Decimal?  BASE_PRICEValue;
        public System.Decimal?  BASE_PRICE
        {
            get { return this.BASE_PRICEValue; }
            set { SetProperty(ref BASE_PRICEValue, value); }
        }

        private System.String PRICING_METHODValue = string.Empty;
        public System.String PRICING_METHOD
        {
            get { return this.PRICING_METHODValue; }
            set { SetProperty(ref PRICING_METHODValue, value); }
        }

        private System.String CURRENCY_CODEValue = string.Empty;
        public System.String CURRENCY_CODE
        {
            get { return this.CURRENCY_CODEValue; }
            set { SetProperty(ref CURRENCY_CODEValue, value); }
        }

        private System.String DESCRIPTIONValue = string.Empty;
        public System.String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

        // Standard PPDM columns





        private System.String ROW_IDValue = string.Empty;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
