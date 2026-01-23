using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Storage
{
    public partial class SERVICE_UNIT : ModelEntityBase
    {
        private System.String SERVICE_UNIT_IDValue;
        public System.String SERVICE_UNIT_ID
        {
            get { return this.SERVICE_UNIT_IDValue; }
            set { SetProperty(ref SERVICE_UNIT_IDValue, value); }
        }

        private System.String UNIT_NAMEValue;
        public System.String UNIT_NAME
        {
            get { return this.UNIT_NAMEValue; }
            set { SetProperty(ref UNIT_NAMEValue, value); }
        }

        private System.String UNIT_TYPEValue;
        public System.String UNIT_TYPE
        {
            get { return this.UNIT_TYPEValue; }
            set { SetProperty(ref UNIT_TYPEValue, value); }
        }

        private System.String LEASE_IDValue;
        public System.String LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        private System.String TANK_BATTERY_IDValue;
        public System.String TANK_BATTERY_ID
        {
            get { return this.TANK_BATTERY_IDValue; }
            set { SetProperty(ref TANK_BATTERY_IDValue, value); }
        }

        private System.String OPERATOR_BA_IDValue;
        public System.String OPERATOR_BA_ID
        {
            get { return this.OPERATOR_BA_IDValue; }
            set { SetProperty(ref OPERATOR_BA_IDValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}


