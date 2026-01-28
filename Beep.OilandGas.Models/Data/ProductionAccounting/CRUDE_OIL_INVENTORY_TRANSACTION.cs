using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class CRUDE_OIL_INVENTORY_TRANSACTION : ModelEntityBase {
        private System.String TRANSACTION_IDValue;
        public System.String TRANSACTION_ID
        {
            get { return this.TRANSACTION_IDValue; }
            set { SetProperty(ref TRANSACTION_IDValue, value); }
        }

        private System.String INVENTORY_IDValue;
        public System.String INVENTORY_ID
        {
            get { return this.INVENTORY_IDValue; }
            set { SetProperty(ref INVENTORY_IDValue, value); }
        }

        private System.DateTime? TRANSACTION_DATEValue;
        public System.DateTime? TRANSACTION_DATE
        {
            get { return this.TRANSACTION_DATEValue; }
            set { SetProperty(ref TRANSACTION_DATEValue, value); }
        }

        private System.String TRANSACTION_TYPEValue;
        public System.String TRANSACTION_TYPE
        {
            get { return this.TRANSACTION_TYPEValue; }
            set { SetProperty(ref TRANSACTION_TYPEValue, value); }
        }

        private System.Decimal? VOLUMEValue;
        public System.Decimal? VOLUME
        {
            get { return this.VOLUMEValue; }
            set { SetProperty(ref VOLUMEValue, value); }
        }

        private System.Decimal? UNIT_COSTValue;
        public System.Decimal? UNIT_COST
        {
            get { return this.UNIT_COSTValue; }
            set { SetProperty(ref UNIT_COSTValue, value); }
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
