using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class CRUDE_OIL_INVENTORY : ModelEntityBase {
        private System.String INVENTORY_IDValue;
        public System.String INVENTORY_ID
        {
            get { return this.INVENTORY_IDValue; }
            set { SetProperty(ref INVENTORY_IDValue, value); }
        }

        private System.String PROPERTY_OR_LEASE_IDValue;
        public System.String PROPERTY_OR_LEASE_ID
        {
            get { return this.PROPERTY_OR_LEASE_IDValue; }
            set { SetProperty(ref PROPERTY_OR_LEASE_IDValue, value); }
        }

        private System.String TANK_BATTERY_IDValue;
        public System.String TANK_BATTERY_ID
        {
            get { return this.TANK_BATTERY_IDValue; }
            set { SetProperty(ref TANK_BATTERY_IDValue, value); }
        }

        private System.DateTime? INVENTORY_DATEValue;
        public System.DateTime? INVENTORY_DATE
        {
            get { return this.INVENTORY_DATEValue; }
            set { SetProperty(ref INVENTORY_DATEValue, value); }
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

        private System.String VALUATION_METHODValue;
        public System.String VALUATION_METHOD
        {
            get { return this.VALUATION_METHODValue; }
            set { SetProperty(ref VALUATION_METHODValue, value); }
        }

        private System.Decimal? MARKET_PRICEValue;
        public System.Decimal? MARKET_PRICE
        {
            get { return this.MARKET_PRICEValue; }
            set { SetProperty(ref MARKET_PRICEValue, value); }
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


