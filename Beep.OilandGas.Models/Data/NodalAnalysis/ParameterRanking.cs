using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    /// <summary>
    /// Represents a single parameter ranking in tornado sensitivity analysis.
    /// Shows impact of parameter variation on operating point.
    /// DTO for calculations - Entity class: PARAMETER_RANKING
    /// </summary>
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
        public ParameterRanking()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
