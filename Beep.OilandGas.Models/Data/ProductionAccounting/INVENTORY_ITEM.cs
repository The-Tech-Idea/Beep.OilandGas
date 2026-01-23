using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class INVENTORY_ITEM : ModelEntityBase {
        private System.String INVENTORY_ITEM_IDValue;
        public System.String INVENTORY_ITEM_ID
        {
            get { return this.INVENTORY_ITEM_IDValue; }
            set { SetProperty(ref INVENTORY_ITEM_IDValue, value); }
        }

        private System.String ITEM_NUMBERValue;
        public System.String ITEM_NUMBER
        {
            get { return this.ITEM_NUMBERValue; }
            set { SetProperty(ref ITEM_NUMBERValue, value); }
        }

        private System.String ITEM_NAMEValue;
        public System.String ITEM_NAME
        {
            get { return this.ITEM_NAMEValue; }
            set { SetProperty(ref ITEM_NAMEValue, value); }
        }

        private System.String ITEM_TYPEValue;
        public System.String ITEM_TYPE
        {
            get { return this.ITEM_TYPEValue; }
            set { SetProperty(ref ITEM_TYPEValue, value); }
        }

        private System.String UNIT_OF_MEASUREValue;
        public System.String UNIT_OF_MEASURE
        {
            get { return this.UNIT_OF_MEASUREValue; }
            set { SetProperty(ref UNIT_OF_MEASUREValue, value); }
        }

        private System.Decimal? QUANTITY_ON_HANDValue;
        public System.Decimal? QUANTITY_ON_HAND
        {
            get { return this.QUANTITY_ON_HANDValue; }
            set { SetProperty(ref QUANTITY_ON_HANDValue, value); }
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

         private System.String VALUATION_METHODValue;
         public System.String VALUATION_METHOD
         {
             get { return this.VALUATION_METHODValue; }
             set { SetProperty(ref VALUATION_METHODValue, value); }
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


