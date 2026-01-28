using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PressureDynamics : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private decimal AveragePressureValue;

        public decimal AveragePressure

        {

            get { return this.AveragePressureValue; }

            set { SetProperty(ref AveragePressureValue, value); }

        }
    }
}
