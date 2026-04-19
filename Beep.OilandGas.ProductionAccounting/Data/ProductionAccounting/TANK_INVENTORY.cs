using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class TANK_INVENTORY : ModelEntityBase {
        private System.String TANK_INVENTORY_IDValue;
        public System.String TANK_INVENTORY_ID
        {
            get { return this.TANK_INVENTORY_IDValue; }
            set { SetProperty(ref TANK_INVENTORY_IDValue, value); }
        }

        private System.DateTime? INVENTORY_DATEValue;
        public System.DateTime? INVENTORY_DATE
        {
            get { return this.INVENTORY_DATEValue; }
            set { SetProperty(ref INVENTORY_DATEValue, value); }
        }

        private System.String TANK_BATTERY_IDValue;
        public System.String TANK_BATTERY_ID
        {
            get { return this.TANK_BATTERY_IDValue; }
            set { SetProperty(ref TANK_BATTERY_IDValue, value); }
        }

        private System.Decimal? OPENING_INVENTORYValue;
        public System.Decimal? OPENING_INVENTORY
        {
            get { return this.OPENING_INVENTORYValue; }
            set { SetProperty(ref OPENING_INVENTORYValue, value); }
        }

        private System.Decimal? RECEIPTSValue;
        public System.Decimal? RECEIPTS
        {
            get { return this.RECEIPTSValue; }
            set { SetProperty(ref RECEIPTSValue, value); }
        }

        private System.Decimal? DELIVERIESValue;
        public System.Decimal? DELIVERIES
        {
            get { return this.DELIVERIESValue; }
            set { SetProperty(ref DELIVERIESValue, value); }
        }

        private System.Decimal? ADJUSTMENTSValue;
        public System.Decimal? ADJUSTMENTS
        {
            get { return this.ADJUSTMENTSValue; }
            set { SetProperty(ref ADJUSTMENTSValue, value); }
        }

        private System.Decimal? SHRINKAGEValue;
        public System.Decimal? SHRINKAGE
        {
            get { return this.SHRINKAGEValue; }
            set { SetProperty(ref SHRINKAGEValue, value); }
        }

        private System.Decimal? THEFT_LOSSValue;
        public System.Decimal? THEFT_LOSS
        {
            get { return this.THEFT_LOSSValue; }
            set { SetProperty(ref THEFT_LOSSValue, value); }
        }

        private System.Decimal? ACTUAL_CLOSING_INVENTORYValue;
        public System.Decimal? ACTUAL_CLOSING_INVENTORY
        {
            get { return this.ACTUAL_CLOSING_INVENTORYValue; }
            set { SetProperty(ref ACTUAL_CLOSING_INVENTORYValue, value); }
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
