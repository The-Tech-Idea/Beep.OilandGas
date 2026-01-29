using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class EXCHANGE_PRICING_TERMS : ModelEntityBase {
        private System.String EXCHANGE_PRICING_TERMS_IDValue;
        public System.String EXCHANGE_PRICING_TERMS_ID
        {
            get { return this.EXCHANGE_PRICING_TERMS_IDValue; }
            set { SetProperty(ref EXCHANGE_PRICING_TERMS_IDValue, value); }
        }

        private System.String EXCHANGE_CONTRACT_IDValue;
        public System.String EXCHANGE_CONTRACT_ID
        {
            get { return this.EXCHANGE_CONTRACT_IDValue; }
            set { SetProperty(ref EXCHANGE_CONTRACT_IDValue, value); }
        }

        private System.String BASE_PRICE_INDEXValue;
        public System.String BASE_PRICE_INDEX
        {
            get { return this.BASE_PRICE_INDEXValue; }
            set { SetProperty(ref BASE_PRICE_INDEXValue, value); }
        }

        private System.Decimal  LOCATION_DIFFERENTIALValue;
        public System.Decimal  LOCATION_DIFFERENTIAL
        {
            get { return this.LOCATION_DIFFERENTIALValue; }
            set { SetProperty(ref LOCATION_DIFFERENTIALValue, value); }
        }

        private System.Decimal  QUALITY_DIFFERENTIALValue;
        public System.Decimal  QUALITY_DIFFERENTIAL
        {
            get { return this.QUALITY_DIFFERENTIALValue; }
            set { SetProperty(ref QUALITY_DIFFERENTIALValue, value); }
        }

        private System.Decimal  TIME_DIFFERENTIALValue;
        public System.Decimal  TIME_DIFFERENTIAL
        {
            get { return this.TIME_DIFFERENTIALValue; }
            set { SetProperty(ref TIME_DIFFERENTIALValue, value); }
        }

        // Standard PPDM columns

      

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }

      
    }
}
