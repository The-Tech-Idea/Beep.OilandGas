using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Storage
{
    public partial class TANK_BATTERY : ModelEntityBase
    {
        private System.String TANK_BATTERY_IDValue;
        public System.String TANK_BATTERY_ID
        {
            get { return this.TANK_BATTERY_IDValue; }
            set { SetProperty(ref TANK_BATTERY_IDValue, value); }
        }

        private System.String BATTERY_NAMEValue;
        public System.String BATTERY_NAME
        {
            get { return this.BATTERY_NAMEValue; }
            set { SetProperty(ref BATTERY_NAMEValue, value); }
        }

        private System.String STORAGE_FACILITY_IDValue;
        public System.String STORAGE_FACILITY_ID
        {
            get { return this.STORAGE_FACILITY_IDValue; }
            set { SetProperty(ref STORAGE_FACILITY_IDValue, value); }
        }

        private System.String LEASE_IDValue;
        public System.String LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        private System.Decimal? CURRENT_INVENTORYValue;
        public System.Decimal? CURRENT_INVENTORY
        {
            get { return this.CURRENT_INVENTORYValue; }
            set { SetProperty(ref CURRENT_INVENTORYValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}
