using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class PO_LINE_ITEM : ModelEntityBase {
        private System.String PO_LINE_ITEM_IDValue;
        public System.String PO_LINE_ITEM_ID
        {
            get { return this.PO_LINE_ITEM_IDValue; }
            set { SetProperty(ref PO_LINE_ITEM_IDValue, value); }
        }

        private System.String PURCHASE_ORDER_IDValue;
        public System.String PURCHASE_ORDER_ID
        {
            get { return this.PURCHASE_ORDER_IDValue; }
            set { SetProperty(ref PURCHASE_ORDER_IDValue, value); }
        }

        private System.Int32? LINE_NUMBERValue;
        public System.Int32? LINE_NUMBER
        {
            get { return this.LINE_NUMBERValue; }
            set { SetProperty(ref LINE_NUMBERValue, value); }
        }

        private System.String ITEM_DESCRIPTIONValue;
        public System.String ITEM_DESCRIPTION
        {
            get { return this.ITEM_DESCRIPTIONValue; }
            set { SetProperty(ref ITEM_DESCRIPTIONValue, value); }
        }

        private System.Decimal? QUANTITYValue;
        public System.Decimal? QUANTITY
        {
            get { return this.QUANTITYValue; }
            set { SetProperty(ref QUANTITYValue, value); }
        }

        private System.Decimal? UNIT_PRICEValue;
        public System.Decimal? UNIT_PRICE
        {
            get { return this.UNIT_PRICEValue; }
            set { SetProperty(ref UNIT_PRICEValue, value); }
        }

        private System.Decimal? LINE_TOTALValue;
        public System.Decimal? LINE_TOTAL
        {
            get { return this.LINE_TOTALValue; }
            set { SetProperty(ref LINE_TOTALValue, value); }
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
