using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class INVENTORY_ADJUSTMENT : ModelEntityBase {
        private System.String INVENTORY_ADJUSTMENT_IDValue;
        public System.String INVENTORY_ADJUSTMENT_ID
        {
            get { return this.INVENTORY_ADJUSTMENT_IDValue; }
            set { SetProperty(ref INVENTORY_ADJUSTMENT_IDValue, value); }
        }

        private System.String INVENTORY_ITEM_IDValue;
        public System.String INVENTORY_ITEM_ID
        {
            get { return this.INVENTORY_ITEM_IDValue; }
            set { SetProperty(ref INVENTORY_ITEM_IDValue, value); }
        }

        private System.DateTime? ADJUSTMENT_DATEValue;
        public System.DateTime? ADJUSTMENT_DATE
        {
            get { return this.ADJUSTMENT_DATEValue; }
            set { SetProperty(ref ADJUSTMENT_DATEValue, value); }
        }

        private System.String ADJUSTMENT_TYPEValue;
        public System.String ADJUSTMENT_TYPE
        {
            get { return this.ADJUSTMENT_TYPEValue; }
            set { SetProperty(ref ADJUSTMENT_TYPEValue, value); }
        }

        private System.Decimal  QUANTITY_ADJUSTMENTValue;
        public System.Decimal  QUANTITY_ADJUSTMENT
        {
            get { return this.QUANTITY_ADJUSTMENTValue; }
            set { SetProperty(ref QUANTITY_ADJUSTMENTValue, value); }
        }

        private System.Decimal  UNIT_COST_ADJUSTMENTValue;
        public System.Decimal  UNIT_COST_ADJUSTMENT
        {
            get { return this.UNIT_COST_ADJUSTMENTValue; }
            set { SetProperty(ref UNIT_COST_ADJUSTMENTValue, value); }
        }

        private System.String REASONValue;
        public System.String REASON
        {
            get { return this.REASONValue; }
            set { SetProperty(ref REASONValue, value); }
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
