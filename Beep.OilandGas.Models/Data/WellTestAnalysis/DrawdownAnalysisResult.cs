using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    /// <summary>
    /// Represents results from drawdown test analysis.
    /// Includes early-time, middle-time, and late-time analysis results.
    /// DTO for calculations - Entity class: DRAWDOWN_ANALYSIS_RESULT
    /// </summary>
    public partial class DrawdownAnalysisResult : ModelEntityBase {
        /// <summary>
        /// Analysis method used (e.g., "Constant Rate Drawdown", "Type Curve Matching")
        /// </summary>
        private string _analysisMethodValue;
        public string AnalysisMethod
        {
            get { return _analysisMethodValue; }
            set { SetProperty(ref _analysisMethodValue, value); }
        }

        /// <summary>
        /// Calculated permeability (millidarcies)
        /// </summary>
        private double? _permeabilityValue;
        public double? Permeability
        {
            get { return _permeabilityValue; }
            set { SetProperty(ref _permeabilityValue, value); }
        }

        /// <summary>
        /// Skin factor (dimensionless)
        /// </summary>
        private double? _skinFactorValue;
        public double? SkinFactor
        {
            get { return _skinFactorValue; }
            set { SetProperty(ref _skinFactorValue, value); }
        }

        /// <summary>
        /// Initial reservoir pressure (psia)
        /// </summary>
        private double? _initialReservoirPressureValue;
        public double? InitialReservoirPressure
        {
            get { return _initialReservoirPressureValue; }
            set { SetProperty(ref _initialReservoirPressureValue, value); }
        }

        /// <summary>
        /// Productivity index (STB/day/psi)
        /// </summary>
        private double? _productivityIndexValue;
        public double? ProductivityIndex
        {
            get { return _productivityIndexValue; }
            set { SetProperty(ref _productivityIndexValue, value); }
        }

        /// <summary>
        /// Flow efficiency (fraction, 0-1)
        /// </summary>
        private double? _flowEfficiencyValue;
        public double? FlowEfficiency
        {
            get { return _flowEfficiencyValue; }
            set { SetProperty(ref _flowEfficiencyValue, value); }
        }

        /// <summary>
        /// Damage ratio (dimensionless)
        /// </summary>
        private double? _damageRatioValue;
        public double? DamageRatio
        {
            get { return _damageRatioValue; }
            set { SetProperty(ref _damageRatioValue, value); }
        }

        /// <summary>
        /// Radius of investigation (feet)
        /// </summary>
        private double? _radiusOfInvestigationValue;
        public double? RadiusOfInvestigation
        {
            get { return _radiusOfInvestigationValue; }
            set { SetProperty(ref _radiusOfInvestigationValue, value); }
        }

        /// <summary>
        /// Identified flow regime type
        /// </summary>
        private string _flowRegimeValue;
        public string FlowRegime
        {
            get { return _flowRegimeValue; }
            set { SetProperty(ref _flowRegimeValue, value); }
        }

        /// <summary>
        /// R-squared goodness of fit (0-1)
        /// </summary>
        private double? _rSquaredValue;
        public double? RSquared
        {
            get { return _rSquaredValue; }
            set { SetProperty(ref _rSquaredValue, value); }
        }

        /// <summary>
        /// Analysis quality rating (Excellent/Good/Fair/Poor)
        /// </summary>
        private string _qualityRatingValue;
        public string QualityRating
        {
            get { return _qualityRatingValue; }
            set { SetProperty(ref _qualityRatingValue, value); }
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
        public DrawdownAnalysisResult()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
