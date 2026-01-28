using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    public partial class ParameterRanking : ModelEntityBase {
        /// <summary>
        /// Parameter name (e.g., "ReservoirPressure", "ProductivityIndex")
        /// </summary>
        private string _parameterNameValue;
        public string ParameterName
        {
            get { return _parameterNameValue; }
            set { SetProperty(ref _parameterNameValue, value); }
        }

        /// <summary>
        /// Variation magnitude from base case
        /// </summary>
        private double _variationValue;
        public double Variation
        {
            get { return _variationValue; }
            set { SetProperty(ref _variationValue, value); }
        }

        /// <summary>
        /// Impact on flow rate (bbl/day or equivalent)
        /// </summary>
        private double _flowRateImpactValue;
        public double FlowRateImpact
        {
            get { return _flowRateImpactValue; }
            set { SetProperty(ref _flowRateImpactValue, value); }
        }

        /// <summary>
        /// Impact on bottomhole pressure (psi or equivalent)
        /// </summary>
        private double _pressureImpactValue;
        public double PressureImpact
        {
            get { return _pressureImpactValue; }
            set { SetProperty(ref _pressureImpactValue, value); }
        }

        /// <summary>
        /// Combined impact metric (sqrt of flow² + pressure²)
        /// </summary>
        private double _totalImpactValue;
        public double TotalImpact
        {
            get { return _totalImpactValue; }
            set { SetProperty(ref _totalImpactValue, value); }
        }

        /// <summary>
        /// Rank position (1 = most influential)
        /// </summary>
        private int? _rankValue;
        public int? Rank
        {
            get { return _rankValue; }
            set { SetProperty(ref _rankValue, value); }
        }

      
        /// <summary>
        /// Default constructor
        /// </summary>
        public ParameterRanking()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
