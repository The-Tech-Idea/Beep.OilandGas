using System;
using System.Collections.Generic;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    public partial class TornadoAnalysisResult : ModelEntityBase {
        /// <summary>
        /// Base operating point used as reference
        /// </summary>
        private double _baseFlowRateValue;
        public double BaseFlowRate
        {
            get { return _baseFlowRateValue; }
            set { SetProperty(ref _baseFlowRateValue, value); }
        }

        /// <summary>
        /// Base bottomhole pressure at reference case
        /// </summary>
        private double _basePressureValue;
        public double BasePressure
        {
            get { return _basePressureValue; }
            set { SetProperty(ref _basePressureValue, value); }
        }

        /// <summary>
        /// Most influential parameter name
        /// </summary>
        private string _mostInfluentialParameterValue;
        public string MostInfluentialParameter
        {
            get { return _mostInfluentialParameterValue; }
            set { SetProperty(ref _mostInfluentialParameterValue, value); }
        }

        /// <summary>
        /// Least influential parameter name
        /// </summary>
        private string _leastInfluentialParameterValue;
        public string LeastInfluentialParameter
        {
            get { return _leastInfluentialParameterValue; }
            set { SetProperty(ref _leastInfluentialParameterValue, value); }
        }

        /// <summary>
        /// Collection of parameter rankings
        /// </summary>
        private List<ParameterRanking> _parameterRankingsValue;
        public List<ParameterRanking> ParameterRankings
        {
            get { return _parameterRankingsValue ??= new List<ParameterRanking>(); }
            set { SetProperty(ref _parameterRankingsValue, value); }
        }
 

        /// <summary>
        /// Default constructor
        /// </summary>
        public TornadoAnalysisResult()
        {
            ParameterRankings = new List<ParameterRanking>();
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
