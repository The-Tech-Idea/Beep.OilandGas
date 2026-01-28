using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ChokeAnalysis
{
    public partial class ChokeFlowResult : ModelEntityBase {
        /// <summary>
        /// Calculated flow rate in Mscf/day
        /// </summary>
        private decimal _flowRateValue;
        public decimal FlowRate
        {
            get { return _flowRateValue; }
            set { SetProperty(ref _flowRateValue, value); }
        }

        /// <summary>
        /// Calculated downstream pressure in psia
        /// </summary>
        private decimal _downstreamPressureValue;
        public decimal DownstreamPressure
        {
            get { return _downstreamPressureValue; }
            set { SetProperty(ref _downstreamPressureValue, value); }
        }

        /// <summary>
        /// Calculated upstream pressure in psia
        /// </summary>
        private decimal _upstreamPressureValue;
        public decimal UpstreamPressure
        {
            get { return _upstreamPressureValue; }
            set { SetProperty(ref _upstreamPressureValue, value); }
        }

        /// <summary>
        /// Pressure ratio (P2/P1)
        /// </summary>
        private decimal _pressureRatioValue;
        public decimal PressureRatio
        {
            get { return _pressureRatioValue; }
            set { SetProperty(ref _pressureRatioValue, value); }
        }

        /// <summary>
        /// Flow regime (subsonic or sonic)
        /// </summary>
        private FlowRegime _flowRegimeValue;
        public FlowRegime FlowRegime
        {
            get { return _flowRegimeValue; }
            set { SetProperty(ref _flowRegimeValue, value); }
        }

        /// <summary>
        /// Critical pressure ratio
        /// </summary>
        private decimal _criticalPressureRatioValue;
        public decimal CriticalPressureRatio
        {
            get { return _criticalPressureRatioValue; }
            set { SetProperty(ref _criticalPressureRatioValue, value); }
        }

      
    }
}
