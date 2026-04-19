using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class EXCHANGE_CONTRACT : ModelEntityBase {
        private System.String CONTRACT_IDValue;
        public System.String CONTRACT_ID
        {
            get { return this.CONTRACT_IDValue; }
            set { SetProperty(ref CONTRACT_IDValue, value); }
        }

        private System.String CONTRACT_NAMEValue;
        public System.String CONTRACT_NAME
        {
            get { return this.CONTRACT_NAMEValue; }
            set { SetProperty(ref CONTRACT_NAMEValue, value); }
        }

        private System.String CONTRACT_TYPEValue;
        public System.String CONTRACT_TYPE
        {
            get { return this.CONTRACT_TYPEValue; }
            set { SetProperty(ref CONTRACT_TYPEValue, value); }
        }

        private System.String PARTIES_JSONValue;
        public System.String PARTIES_JSON
        {
            get { return this.PARTIES_JSONValue; }
            set { SetProperty(ref PARTIES_JSONValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRATION_DATEValue;
        public System.DateTime? EXPIRATION_DATE
        {
            get { return this.EXPIRATION_DATEValue; }
            set { SetProperty(ref EXPIRATION_DATEValue, value); }
        }

        private System.String EXCHANGE_TERMS_JSONValue;
        public System.String EXCHANGE_TERMS_JSON
        {
            get { return this.EXCHANGE_TERMS_JSONValue; }
            set { SetProperty(ref EXCHANGE_TERMS_JSONValue, value); }
        }

        private System.String DELIVERY_POINTS_JSONValue;
        public System.String DELIVERY_POINTS_JSON
        {
            get { return this.DELIVERY_POINTS_JSONValue; }
            set { SetProperty(ref DELIVERY_POINTS_JSONValue, value); }
        }

        private System.String PRICING_TERMS_JSONValue;
        public System.String PRICING_TERMS_JSON
        {
            get { return this.PRICING_TERMS_JSONValue; }
            set { SetProperty(ref PRICING_TERMS_JSONValue, value); }
        }

        // Standard PPDM columns

    
        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }

      

        private EXCHANGE_TERMS TERMSValue;
        public EXCHANGE_TERMS TERMS
        {
            get { return this.TERMSValue; }
            set { SetProperty(ref TERMSValue, value); }
        }

        private EXCHANGE_PRICING_TERMS PRICING_TERMSValue;
        public EXCHANGE_PRICING_TERMS PRICING_TERMS
        {
            get { return this.PRICING_TERMSValue; }
            set { SetProperty(ref PRICING_TERMSValue, value); }
        }
    }
}
