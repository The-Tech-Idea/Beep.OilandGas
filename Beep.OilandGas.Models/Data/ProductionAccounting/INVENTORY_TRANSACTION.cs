using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class INVENTORY_TRANSACTION : ModelEntityBase {
        private System.String INVENTORY_TRANSACTION_IDValue;
        public System.String INVENTORY_TRANSACTION_ID
        {
            get { return this.INVENTORY_TRANSACTION_IDValue; }
            set { SetProperty(ref INVENTORY_TRANSACTION_IDValue, value); }
        }

        private System.String INVENTORY_ITEM_IDValue;
        public System.String INVENTORY_ITEM_ID
        {
            get { return this.INVENTORY_ITEM_IDValue; }
            set { SetProperty(ref INVENTORY_ITEM_IDValue, value); }
        }

        private System.String TRANSACTION_TYPEValue;
        public System.String TRANSACTION_TYPE
        {
            get { return this.TRANSACTION_TYPEValue; }
            set { SetProperty(ref TRANSACTION_TYPEValue, value); }
        }

        private System.DateTime? TRANSACTION_DATEValue;
        public System.DateTime? TRANSACTION_DATE
        {
            get { return this.TRANSACTION_DATEValue; }
            set { SetProperty(ref TRANSACTION_DATEValue, value); }
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

        private System.Decimal? TOTAL_COSTValue;
        public System.Decimal? TOTAL_COST
        {
            get { return this.TOTAL_COSTValue; }
            set { SetProperty(ref TOTAL_COSTValue, value); }
        }

        private System.String REFERENCE_NUMBERValue;
        public System.String REFERENCE_NUMBER
        {
            get { return this.REFERENCE_NUMBERValue; }
            set { SetProperty(ref REFERENCE_NUMBERValue, value); }
        }

         private System.String DESCRIPTIONValue;
         public System.String DESCRIPTION
         {
             get { return this.DESCRIPTIONValue; }
             set { SetProperty(ref DESCRIPTIONValue, value); }
         }

         private System.String NOTESValue;
         public System.String NOTES
         {
             get { return this.NOTESValue; }
             set { SetProperty(ref NOTESValue, value); }
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
