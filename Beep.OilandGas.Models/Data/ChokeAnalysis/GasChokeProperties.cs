using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ChokeAnalysis
{
    /// <summary>
    /// Represents gas properties for choke calculations
    /// DTO for calculations - Entity class: GAS_CHOKE_PROPERTIES
    /// </summary>
    public partial class GasChokeProperties : ModelEntityBase {
        /// <summary>
        /// Gas specific gravity (relative to air)
        /// </summary>
        private decimal _gasSpecificGravityValue;
        public decimal GasSpecificGravity
        {
            get { return _gasSpecificGravityValue; }
            set { SetProperty(ref _gasSpecificGravityValue, value); }
        }

        /// <summary>
        /// Upstream pressure in psia
        /// </summary>
        private decimal _upstreamPressureValue;
        public decimal UpstreamPressure
        {
            get { return _upstreamPressureValue; }
            set { SetProperty(ref _upstreamPressureValue, value); }
        }

        /// <summary>
        /// Downstream pressure in psia
        /// </summary>
        private decimal _downstreamPressureValue;
        public decimal DownstreamPressure
        {
            get { return _downstreamPressureValue; }
            set { SetProperty(ref _downstreamPressureValue, value); }
        }

        /// <summary>
        /// Temperature in Rankine
        /// </summary>
        private decimal _temperatureValue;
        public decimal Temperature
        {
            get { return _temperatureValue; }
            set { SetProperty(ref _temperatureValue, value); }
        }

        /// <summary>
        /// Z-factor (compressibility factor)
        /// </summary>
        private decimal _zFactorValue;
        public decimal ZFactor
        {
            get { return _zFactorValue; }
            set { SetProperty(ref _zFactorValue, value); }
        }

        /// <summary>
        /// Gas flow rate in Mscf/day
        /// </summary>
        private decimal _flowRateValue;
        public decimal FlowRate
        {
            get { return _flowRateValue; }
            set { SetProperty(ref _flowRateValue, value); }
        }

       
    }
}


