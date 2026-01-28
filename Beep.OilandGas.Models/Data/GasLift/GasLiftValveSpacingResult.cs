using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.GasLift
{
    public class GasLiftValveSpacingResult : ModelEntityBase
    {
        /// <summary>
        /// Number of valves in the design.
        /// </summary>
        private int NumberOfValvesValue;

        public int NumberOfValves

        {

            get { return this.NumberOfValvesValue; }

            set { SetProperty(ref NumberOfValvesValue, value); }

        }

        /// <summary>
        /// Depths of each valve in feet.
        /// </summary>
        private List<decimal> ValveDepthsValue = new List<decimal>();

        public List<decimal> ValveDepths

        {

            get { return this.ValveDepthsValue; }

            set { SetProperty(ref ValveDepthsValue, value); }

        }

        /// <summary>
        /// Opening pressures for each valve in psia.
        /// </summary>
        private List<decimal> OpeningPressuresValue = new List<decimal>();

        public List<decimal> OpeningPressures

        {

            get { return this.OpeningPressuresValue; }

            set { SetProperty(ref OpeningPressuresValue, value); }

        }

        /// <summary>
        /// Total depth coverage from first to last valve in feet.
        /// </summary>
        private decimal TotalDepthCoverageValue;

        public decimal TotalDepthCoverage

        {

            get { return this.TotalDepthCoverageValue; }

            set { SetProperty(ref TotalDepthCoverageValue, value); }

        }
    }
}
