using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ControlParameters : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private List<ControlParam> ParametersValue = new();

        public List<ControlParam> Parameters

        {

            get { return this.ParametersValue; }

            set { SetProperty(ref ParametersValue, value); }

        }
        private string ControlModeValue = string.Empty;

        public string ControlMode

        {

            get { return this.ControlModeValue; }

            set { SetProperty(ref ControlModeValue, value); }

        }
    }
}
