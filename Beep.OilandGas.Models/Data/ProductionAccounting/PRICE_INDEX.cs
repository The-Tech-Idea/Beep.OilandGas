using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class PRICE_INDEX : ModelEntityBase {
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

        private System.String UNIT_OF_MEASUREValue;
        public System.String UNIT_OF_MEASURE
        {
            get { return this.UNIT_OF_MEASUREValue; }
            set { SetProperty(ref UNIT_OF_MEASUREValue, value); }
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


