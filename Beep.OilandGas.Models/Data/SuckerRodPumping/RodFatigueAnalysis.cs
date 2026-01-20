using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    /// <summary>
    /// Represents rod string fatigue and stress analysis results.
    /// Calculates remaining service life and fatigue margin based on load history.
    /// DTO for calculations - Entity class: ROD_FATIGUE_ANALYSIS
    /// </summary>
    public partial class RodFatigueAnalysis : ModelEntityBase {
        /// <summary>
        /// Maximum rod stress during operating cycle (psi)
        /// </summary>
        private double? _maximumStressValue;
        public double? MaximumStress
        {
            get { return _maximumStressValue; }
            set { SetProperty(ref _maximumStressValue, value); }
        }

        /// <summary>
        /// Minimum rod stress during operating cycle (psi)
        /// </summary>
        private double? _minimumStressValue;
        public double? MinimumStress
        {
            get { return _minimumStressValue; }
            set { SetProperty(ref _minimumStressValue, value); }
        }

        /// <summary>
        /// Mean (average) stress level (psi)
        /// </summary>
        private double? _meanStressValue;
        public double? MeanStress
        {
            get { return _meanStressValue; }
            set { SetProperty(ref _meanStressValue, value); }
        }

        /// <summary>
        /// Stress range (max - min) (psi)
        /// </summary>
        private double? _stressRangeValue;
        public double? StressRange
        {
            get { return _stressRangeValue; }
            set { SetProperty(ref _stressRangeValue, value); }
        }

        /// <summary>
        /// Rod material yield strength (psi)
        /// </summary>
        private double? _yieldStrengthValue;
        public double? YieldStrength
        {
            get { return _yieldStrengthValue; }
            set { SetProperty(ref _yieldStrengthValue, value); }
        }

        /// <summary>
        /// Fatigue strength (endurance limit) for material (psi)
        /// </summary>
        private double? _fatigueStrengthValue;
        public double? FatigueStrength
        {
            get { return _fatigueStrengthValue; }
            set { SetProperty(ref _fatigueStrengthValue, value); }
        }

        /// <summary>
        /// Stress concentration factor (Kt) at critical section
        /// </summary>
        private double? _stressConcentrationFactorValue;
        public double? StressConcentrationFactor
        {
            get { return _stressConcentrationFactorValue; }
            set { SetProperty(ref _stressConcentrationFactorValue, value); }
        }

        /// <summary>
        /// Fatigue margin (safety factor against fatigue failure)
        /// Greater than 1.0 means safe; less than 1.0 means unsafe
        /// </summary>
        private double? _fatigueMarginValue;
        public double? FatigueMargin
        {
            get { return _fatigueMarginValue; }
            set { SetProperty(ref _fatigueMarginValue, value); }
        }

        /// <summary>
        /// Estimated remaining service life (months)
        /// </summary>
        private double? _remainingServiceLifeValue;
        public double? RemainingServiceLife
        {
            get { return _remainingServiceLifeValue; }
            set { SetProperty(ref _remainingServiceLifeValue, value); }
        }

        /// <summary>
        /// Estimated cycles to failure based on fatigue analysis
        /// </summary>
        private double? _cyclesToFailureValue;
        public double? CyclesToFailure
        {
            get { return _cyclesToFailureValue; }
            set { SetProperty(ref _cyclesToFailureValue, value); }
        }

        /// <summary>
        /// Current cumulative stress cycles (approx)
        /// </summary>
        private double? _cumulativeCyclesValue;
        public double? CumulativeCycles
        {
            get { return _cumulativeCyclesValue; }
            set { SetProperty(ref _cumulativeCyclesValue, value); }
        }

        /// <summary>
        /// Percentage of fatigue life consumed (0-100%)
        /// </summary>
        private double? _lifeConsumedPercentValue;
        public double? LifeConsumedPercent
        {
            get { return _lifeConsumedPercentValue; }
            set { SetProperty(ref _lifeConsumedPercentValue, value); }
        }

        /// <summary>
        /// Fatigue risk assessment (Low, Medium, High, Critical)
        /// </summary>
        private string _riskAssessmentValue;
        public string RiskAssessment
        {
            get { return _riskAssessmentValue; }
            set { SetProperty(ref _riskAssessmentValue, value); }
        }

        /// <summary>
        /// Recommended action (Continue Operating, Schedule Replacement, Replace Immediately)
        /// </summary>
        private string _recommendedActionValue;
        public string RecommendedAction
        {
            get { return _recommendedActionValue; }
            set { SetProperty(ref _recommendedActionValue, value); }
        }

        /// <summary>
        /// Analysis method used (Goodman, Soderberg, Morrow, etc.)
        /// </summary>
        private string _analysisMethodValue;
        public string AnalysisMethod
        {
            get { return _analysisMethodValue; }
            set { SetProperty(ref _analysisMethodValue, value); }
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
        public RodFatigueAnalysis()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
