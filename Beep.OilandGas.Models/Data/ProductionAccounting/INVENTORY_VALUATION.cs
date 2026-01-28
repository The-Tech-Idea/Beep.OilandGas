using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class INVENTORY_VALUATION : ModelEntityBase {
        private System.String INVENTORY_VALUATION_IDValue;
        public System.String INVENTORY_VALUATION_ID
        {
            get { return this.INVENTORY_VALUATION_IDValue; }
            set { SetProperty(ref INVENTORY_VALUATION_IDValue, value); }
        }

        private System.String INVENTORY_ITEM_IDValue;
        public System.String INVENTORY_ITEM_ID
        {
            get { return this.INVENTORY_ITEM_IDValue; }
            set { SetProperty(ref INVENTORY_ITEM_IDValue, value); }
        }

        private System.DateTime? VALUATION_DATEValue;
        public System.DateTime? VALUATION_DATE
        {
            get { return this.VALUATION_DATEValue; }
            set { SetProperty(ref VALUATION_DATEValue, value); }
        }

        private System.String VALUATION_METHODValue;
        public System.String VALUATION_METHOD
        {
            get { return this.VALUATION_METHODValue; }
            set { SetProperty(ref VALUATION_METHODValue, value); }
        }

        private System.Decimal? QUANTITYValue;
        public System.Decimal? QUANTITY
        {
            get { return this.QUANTITYValue; }
            set { SetProperty(ref QUANTITYValue, value); }
        }

        private System.Decimal? UNIT_COSTValue;
        public System.Decimal? UNIT_COST
        {
            get { return this.UNIT_COSTValue; }
            set { SetProperty(ref UNIT_COSTValue, value); }
        }

        private System.Decimal? TOTAL_VALUEValue;
        public System.Decimal? TOTAL_VALUE
        {
            get { return this.TOTAL_VALUEValue; }
            set { SetProperty(ref TOTAL_VALUEValue, value); }
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
