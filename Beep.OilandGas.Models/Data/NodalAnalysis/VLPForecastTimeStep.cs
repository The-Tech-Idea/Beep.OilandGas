using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    public partial class VLPForecastTimeStep : ModelEntityBase {
        /// <summary>
        /// Year number in forecast (0 = baseline, 1+ = future years)
        /// </summary>
        private int? _yearValue;
        public int? Year
        {
            get { return _yearValue; }
            set { SetProperty(ref _yearValue, value); }
        }

        /// <summary>
        /// Cumulative sand concentration at this time step (mg/L)
        /// </summary>
        private double? _sandConcentrationValue;
        public double? SandConcentration
        {
            get { return _sandConcentrationValue; }
            set { SetProperty(ref _sandConcentrationValue, value); }
        }

        /// <summary>
        /// Cumulative scale buildup fraction at this time step (0-1)
        /// </summary>
        private double? _scaleBuildupFractionValue;
        public double? ScaleBuildupFraction
        {
            get { return _scaleBuildupFractionValue; }
            set { SetProperty(ref _scaleBuildupFractionValue, value); }
        }

        /// <summary>
        /// Low flow rate required BHP at this time step (psi)
        /// </summary>
        private double? _lowFlowRequiredBhpValue;
        public double? LowFlowRequiredBHP
        {
            get { return _lowFlowRequiredBhpValue; }
            set { SetProperty(ref _lowFlowRequiredBhpValue, value); }
        }

        /// <summary>
        /// Mid flow rate required BHP at this time step (psi)
        /// </summary>
        private double? _midFlowRequiredBhpValue;
        public double? MidFlowRequiredBHP
        {
            get { return _midFlowRequiredBhpValue; }
            set { SetProperty(ref _midFlowRequiredBhpValue, value); }
        }

        /// <summary>
        /// High flow rate required BHP at this time step (psi)
        /// </summary>
        private double? _highFlowRequiredBhpValue;
        public double? HighFlowRequiredBHP
        {
            get { return _highFlowRequiredBhpValue; }
            set { SetProperty(ref _highFlowRequiredBhpValue, value); }
        }

        /// <summary>
        /// Total degradation percentage from baseline at this time step
        /// </summary>
        private double? _degradationPercentageValue;
        public double? DegradationPercentage
        {
            get { return _degradationPercentageValue; }
            set { SetProperty(ref _degradationPercentageValue, value); }
        }

        
        /// <summary>
        /// Default constructor
        /// </summary>
        public VLPForecastTimeStep()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
