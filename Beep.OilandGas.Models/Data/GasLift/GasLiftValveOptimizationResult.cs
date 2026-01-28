using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.GasLift
{
    public class GasLiftValveOptimizationResult : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the number of valves in this configuration
        /// </summary>
        private int NumberOfValvesValue;

        public int NumberOfValves

        {

            get { return this.NumberOfValvesValue; }

            set { SetProperty(ref NumberOfValvesValue, value); }

        }

        /// <summary>
        /// Gets or sets the total gas injection rate (Mscf/day)
        /// </summary>
        private decimal TotalGasInjectionRateValue;

        public decimal TotalGasInjectionRate

        {

            get { return this.TotalGasInjectionRateValue; }

            set { SetProperty(ref TotalGasInjectionRateValue, value); }

        }

        /// <summary>
        /// Gets or sets the valve spacing (feet)
        /// </summary>
        private decimal ValveSpacingValue;

        public decimal ValveSpacing

        {

            get { return this.ValveSpacingValue; }

            set { SetProperty(ref ValveSpacingValue, value); }

        }

        /// <summary>
        /// Gets or sets the design quality score (0-100%)
        /// </summary>
        private decimal DesignQualityValue;

        public decimal DesignQuality

        {

            get { return this.DesignQualityValue; }

            set { SetProperty(ref DesignQualityValue, value); }

        }

        /// <summary>
        /// Gets or sets the cost-effectiveness score (0-100%)
        /// </summary>
        private decimal CostEffectivenessValue;

        public decimal CostEffectiveness

        {

            get { return this.CostEffectivenessValue; }

            set { SetProperty(ref CostEffectivenessValue, value); }

        }
    }
}
