using System;
using System.Collections.Generic;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.NodalAnalysis
{
    /// <summary>
    /// Represents tornado sensitivity analysis results showing parameter rankings by impact.
    /// DTO for calculations - Entity class: TORNADO_ANALYSIS_RESULT
    /// </summary>
    public partial class TornadoAnalysisResult : Entity, IPPDMEntity
    {
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

        // PPDM Entity Properties

        private string _activeIndValue = "Y";
        public string ACTIVE_IND
        {
            get { return _activeIndValue; }
            set { SetProperty(ref _activeIndValue, value); }
        }

        private string _rowCreatedByValue;
        public string ROW_CREATED_BY
        {
            get { return _rowCreatedByValue; }
            set { SetProperty(ref _rowCreatedByValue, value); }
        }

        private DateTime? _rowCreatedDateValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return _rowCreatedDateValue; }
            set { SetProperty(ref _rowCreatedDateValue, value); }
        }

        private string _rowChangedByValue;
        public string ROW_CHANGED_BY
        {
            get { return _rowChangedByValue; }
            set { SetProperty(ref _rowChangedByValue, value); }
        }

        private DateTime? _rowChangedDateValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return _rowChangedDateValue; }
            set { SetProperty(ref _rowChangedDateValue, value); }
        }

        private DateTime? _rowEffectiveDateValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return _rowEffectiveDateValue; }
            set { SetProperty(ref _rowEffectiveDateValue, value); }
        }

        private DateTime? _rowExpiryDateValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return _rowExpiryDateValue; }
            set { SetProperty(ref _rowExpiryDateValue, value); }
        }

        private string _rowQualityValue;
        public string ROW_QUALITY
        {
            get { return _rowQualityValue; }
            set { SetProperty(ref _rowQualityValue, value); }
        }

        private string _ppdmGuidValue;
        public string PPDM_GUID
        {
            get { return _ppdmGuidValue; }
            set { SetProperty(ref _ppdmGuidValue, value); }
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
