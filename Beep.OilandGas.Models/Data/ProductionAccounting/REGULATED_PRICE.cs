using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class REGULATED_PRICE : ModelEntityBase {
        private System.String REGULATED_PRICE_IDValue;
        public System.String REGULATED_PRICE_ID
        {
            get { return this.REGULATED_PRICE_IDValue; }
            set { SetProperty(ref REGULATED_PRICE_IDValue, value); }
        }

        private System.String REGULATORY_AUTHORITYValue;
        public System.String REGULATORY_AUTHORITY
        {
            get { return this.REGULATORY_AUTHORITYValue; }
            set { SetProperty(ref REGULATORY_AUTHORITYValue, value); }
        }

        private System.String PRICE_FORMULAValue;
        public System.String PRICE_FORMULA
        {
            get { return this.PRICE_FORMULAValue; }
            set { SetProperty(ref PRICE_FORMULAValue, value); }
        }

        private System.DateTime? EFFECTIVE_START_DATEValue;
        public System.DateTime? EFFECTIVE_START_DATE
        {
            get { return this.EFFECTIVE_START_DATEValue; }
            set { SetProperty(ref EFFECTIVE_START_DATEValue, value); }
        }

        private System.DateTime? EFFECTIVE_END_DATEValue;
        public System.DateTime? EFFECTIVE_END_DATE
        {
            get { return this.EFFECTIVE_END_DATEValue; }
            set { SetProperty(ref EFFECTIVE_END_DATEValue, value); }
        }

        private System.Decimal? PRICE_CAPValue;
        public System.Decimal? PRICE_CAP
        {
            get { return this.PRICE_CAPValue; }
            set { SetProperty(ref PRICE_CAPValue, value); }
        }

        private System.Decimal? PRICE_FLOORValue;
        public System.Decimal? PRICE_FLOOR
        {
            get { return this.PRICE_FLOORValue; }
            set { SetProperty(ref PRICE_FLOORValue, value); }
        }

        private System.Decimal? BASE_PRICEValue;
        public System.Decimal? BASE_PRICE
        {
            get { return this.BASE_PRICEValue; }
            set { SetProperty(ref BASE_PRICEValue, value); }
        }

        private System.String ADJUSTMENT_FACTORS_JSONValue;
        public System.String ADJUSTMENT_FACTORS_JSON
        {
            get { return this.ADJUSTMENT_FACTORS_JSONValue; }
            set { SetProperty(ref ADJUSTMENT_FACTORS_JSONValue, value); }
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


