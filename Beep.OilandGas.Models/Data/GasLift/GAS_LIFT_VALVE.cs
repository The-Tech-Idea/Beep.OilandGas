using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.GasLift
{
    public partial class GAS_LIFT_VALVE : ModelEntityBase {
        private String GAS_LIFT_VALVE_IDValue;
        public String GAS_LIFT_VALVE_ID
        {
            get { return this.GAS_LIFT_VALVE_IDValue; }
            set { SetProperty(ref GAS_LIFT_VALVE_IDValue, value); }
        }

        private String GAS_LIFT_DESIGN_IDValue;
        public String GAS_LIFT_DESIGN_ID
        {
            get { return this.GAS_LIFT_DESIGN_IDValue; }
            set { SetProperty(ref GAS_LIFT_DESIGN_IDValue, value); }
        }

        private Decimal? DEPTHValue;
        public Decimal? DEPTH
        {
            get { return this.DEPTHValue; }
            set { SetProperty(ref DEPTHValue, value); }
        }

        private Decimal? PORT_SIZEValue;
        public Decimal? PORT_SIZE
        {
            get { return this.PORT_SIZEValue; }
            set { SetProperty(ref PORT_SIZEValue, value); }
        }

        private Decimal? OPENING_PRESSUREValue;
        public Decimal? OPENING_PRESSURE
        {
            get { return this.OPENING_PRESSUREValue; }
            set { SetProperty(ref OPENING_PRESSUREValue, value); }
        }

        private Decimal? CLOSING_PRESSUREValue;
        public Decimal? CLOSING_PRESSURE
        {
            get { return this.CLOSING_PRESSUREValue; }
            set { SetProperty(ref CLOSING_PRESSUREValue, value); }
        }

        private String VALVE_TYPEValue;
        public String VALVE_TYPE
        {
            get { return this.VALVE_TYPEValue; }
            set { SetProperty(ref VALVE_TYPEValue, value); }
        }

        private Decimal? TEMPERATUREValue;
        public Decimal? TEMPERATURE
        {
            get { return this.TEMPERATUREValue; }
            set { SetProperty(ref TEMPERATUREValue, value); }
        }

        private Decimal? GAS_INJECTION_RATEValue;
        public Decimal? GAS_INJECTION_RATE
        {
            get { return this.GAS_INJECTION_RATEValue; }
            set { SetProperty(ref GAS_INJECTION_RATEValue, value); }
        }

        // Standard PPDM columns

    }
}


