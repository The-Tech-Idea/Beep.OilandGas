using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.GasLift
{
    public partial class GAS_LIFT_VALVE_DESIGN_RESULT : ModelEntityBase {
        private String GAS_LIFT_VALVE_DESIGN_RESULT_IDValue;
        public String GAS_LIFT_VALVE_DESIGN_RESULT_ID
        {
            get { return this.GAS_LIFT_VALVE_DESIGN_RESULT_IDValue; }
            set { SetProperty(ref GAS_LIFT_VALVE_DESIGN_RESULT_IDValue, value); }
        }

        private String GAS_LIFT_WELL_PROPERTIES_IDValue;
        public String GAS_LIFT_WELL_PROPERTIES_ID
        {
            get { return this.GAS_LIFT_WELL_PROPERTIES_IDValue; }
            set { SetProperty(ref GAS_LIFT_WELL_PROPERTIES_IDValue, value); }
        }

        private Decimal? TOTAL_GAS_INJECTION_RATEValue;
        public Decimal? TOTAL_GAS_INJECTION_RATE
        {
            get { return this.TOTAL_GAS_INJECTION_RATEValue; }
            set { SetProperty(ref TOTAL_GAS_INJECTION_RATEValue, value); }
        }

        // Standard PPDM columns

        // Optional PPDM properties
        private String AREA_IDValue;
        public String AREA_ID
        {
            get { return this.AREA_IDValue; }
            set { SetProperty(ref AREA_IDValue, value); }
        }

        private String AREA_TYPEValue;
        public String AREA_TYPE
        {
            get { return this.AREA_TYPEValue; }
            set { SetProperty(ref AREA_TYPEValue, value); }
        }

        private String BUSINESS_ASSOCIATE_IDValue;
        public String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

    }
}
