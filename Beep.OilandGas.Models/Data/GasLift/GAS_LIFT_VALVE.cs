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

        private  decimal  DEPTHValue;
        public  decimal  DEPTH
        {
            get { return this.DEPTHValue; }
            set { SetProperty(ref DEPTHValue, value); }
        }

        private  decimal  PORT_SIZEValue;
        public  decimal  PORT_SIZE
        {
            get { return this.PORT_SIZEValue; }
            set { SetProperty(ref PORT_SIZEValue, value); }
        }

        private  decimal  OPENING_PRESSUREValue;
        public  decimal  OPENING_PRESSURE
        {
            get { return this.OPENING_PRESSUREValue; }
            set { SetProperty(ref OPENING_PRESSUREValue, value); }
        }

        private  decimal  CLOSING_PRESSUREValue;
        public  decimal  CLOSING_PRESSURE
        {
            get { return this.CLOSING_PRESSUREValue; }
            set { SetProperty(ref CLOSING_PRESSUREValue, value); }
        }

        private GasLiftValveType VALVE_TYPEValue;
        public GasLiftValveType VALVE_TYPE
        {
            get { return this.VALVE_TYPEValue; }
            set { SetProperty(ref VALVE_TYPEValue, value); }
        }

        private  decimal  TEMPERATUREValue;
        public  decimal  TEMPERATURE
        {
            get { return this.TEMPERATUREValue; }
            set { SetProperty(ref TEMPERATUREValue, value); }
        }

        private  decimal  GAS_INJECTION_RATEValue;
        public  decimal  GAS_INJECTION_RATE
        {
            get { return this.GAS_INJECTION_RATEValue; }
            set { SetProperty(ref GAS_INJECTION_RATEValue, value); }
        }

        // Standard PPDM columns


        private string VALVE_IDValue;
        public string VALVE_ID
        {
            get { return this.VALVE_IDValue; }
            set { SetProperty(ref VALVE_IDValue, value); }
        }
    }
}
