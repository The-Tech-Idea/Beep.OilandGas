using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PumpSizingRequest : ModelEntityBase
    {
        private decimal DesiredFlowRateValue;

        public decimal DesiredFlowRate

        {

            get { return this.DesiredFlowRateValue; }

            set { SetProperty(ref DesiredFlowRateValue, value); }

        }
        private decimal MaxOperatingPressureValue;

        public decimal MaxOperatingPressure

        {

            get { return this.MaxOperatingPressureValue; }

            set { SetProperty(ref MaxOperatingPressureValue, value); }

        }
        private decimal FluidViscosityValue;

        public decimal FluidViscosity

        {

            get { return this.FluidViscosityValue; }

            set { SetProperty(ref FluidViscosityValue, value); }

        }
    }
}
