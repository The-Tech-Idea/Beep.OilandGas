using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    /// <summary>
    /// Represents a detected flow regime segment in pressure transient data.
    /// Used to identify and characterize different flow periods during well testing.
    /// DTO for calculations - Entity class: FLOW_REGIME_INDICATOR
    /// </summary>
    public partial class FlowRegimeIndicator : ModelEntityBase {
        /// <summary>
        /// Flow regime type (e.g., "Storage", "Wellbore Skin", "Infinite Acting", "Boundary Dominated")
        /// </summary>
        private string _regimeTypeValue;
        public string RegimeType
        {
            get { return _regimeTypeValue; }
            set { SetProperty(ref _regimeTypeValue, value); }
        }

        /// <summary>
        /// Start time of this flow regime (hours)
        /// </summary>
        private double? _startTimeValue;
        public double? StartTime
        {
            get { return _startTimeValue; }
            set { SetProperty(ref _startTimeValue, value); }
        }

        /// <summary>
        /// End time of this flow regime (hours)
        /// </summary>
        private double? _endTimeValue;
        public double? EndTime
        {
            get { return _endTimeValue; }
            set { SetProperty(ref _endTimeValue, value); }
        }

        /// <summary>
        /// Characteristic slope or gradient of this regime on diagnostic plot
        /// </summary>
        private double? _slopeValue;
        public double? Slope
        {
            get { return _slopeValue; }
            set { SetProperty(ref _slopeValue, value); }
        }

        /// <summary>
        /// Intercept or reference pressure value for this regime
        /// </summary>
        private double? _interceptValue;
        public double? Intercept
        {
            get { return _interceptValue; }
            set { SetProperty(ref _interceptValue, value); }
        }

        /// <summary>
        /// Goodness of fit (R²) for this regime segment
        /// </summary>
        private double? _rSquaredValue;
        public double? RSquared
        {
            get { return _rSquaredValue; }
            set { SetProperty(ref _rSquaredValue, value); }
        }

        /// <summary>
        /// Confidence level in regime identification (Low/Medium/High)
        /// </summary>
        private string _confidenceLevelValue;
        public string ConfidenceLevel
        {
            get { return _confidenceLevelValue; }
            set { SetProperty(ref _confidenceLevelValue, value); }
        }

        /// <summary>
        /// Physical interpretation or meaning of this regime
        /// </summary>
        private string _interpretationValue;
        public string Interpretation
        {
            get { return _interpretationValue; }
            set { SetProperty(ref _interpretationValue, value); }
        }

        /// <summary>
        /// Sequence order in the test (1 = first flow regime)
        /// </summary>
        private int? _sequenceValue;
        public int? Sequence
        {
            get { return _sequenceValue; }
            set { SetProperty(ref _sequenceValue, value); }
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
        public FlowRegimeIndicator()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
