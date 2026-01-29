using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Storage
{
    public partial class LACT_UNIT : ModelEntityBase
    {
        private System.String LACT_UNIT_IDValue;
        public System.String LACT_UNIT_ID
        {
            get { return this.LACT_UNIT_IDValue; }
            set { SetProperty(ref LACT_UNIT_IDValue, value); }
        }

        private System.String LACT_NAMEValue;
        public System.String LACT_NAME
        {
            get { return this.LACT_NAMEValue; }
            set { SetProperty(ref LACT_NAMEValue, value); }
        }

        private System.String SERVICE_UNIT_IDValue;
        public System.String SERVICE_UNIT_ID
        {
            get { return this.SERVICE_UNIT_IDValue; }
            set { SetProperty(ref SERVICE_UNIT_IDValue, value); }
        }

        private System.String METER_TYPEValue;
        public System.String METER_TYPE
        {
            get { return this.METER_TYPEValue; }
            set { SetProperty(ref METER_TYPEValue, value); }
        }

        private System.Decimal  MAXIMUM_FLOW_RATEValue;
        public System.Decimal  MAXIMUM_FLOW_RATE
        {
            get { return this.MAXIMUM_FLOW_RATEValue; }
            set { SetProperty(ref MAXIMUM_FLOW_RATEValue, value); }
        }

        private System.Decimal  METER_FACTORValue;
        public System.Decimal  METER_FACTOR
        {
            get { return this.METER_FACTORValue; }
            set { SetProperty(ref METER_FACTORValue, value); }
        }

        private System.DateTime? LAST_CALIBRATION_DATEValue;
        public System.DateTime? LAST_CALIBRATION_DATE
        {
            get { return this.LAST_CALIBRATION_DATEValue; }
            set { SetProperty(ref LAST_CALIBRATION_DATEValue, value); }
        }

        private System.DateTime? NEXT_CALIBRATION_DATEValue;
        public System.DateTime? NEXT_CALIBRATION_DATE
        {
            get { return this.NEXT_CALIBRATION_DATEValue; }
            set { SetProperty(ref NEXT_CALIBRATION_DATEValue, value); }
        }

        private System.String IS_ACTIVEValue;
        public System.String IS_ACTIVE
        {
            get { return this.IS_ACTIVEValue; }
            set { SetProperty(ref IS_ACTIVEValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}
