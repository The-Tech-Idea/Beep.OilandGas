using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.DCA
{
    /// <summary>
    /// Enhanced DCA fit result with industry-standard analysis and PPDM compliance.
    /// DTO for calculations - Entity class: DCA_FIT_RESULT
    /// </summary>
    public partial class DCAFitResultEnhanced : ModelEntityBase {
        /// <summary>
        /// Fitted parameters array [qi, Di, b] for decline models.
        /// </summary>
        private decimal[] _parametersValue;
        public decimal[] Parameters
        {
            get { return _parametersValue; }
            set { SetProperty(ref _parametersValue, value); }
        }

        /// <summary>
        /// Observed production values used in the analysis.
        /// </summary>
        private decimal[] _observedValuesValue;
        public decimal[] ObservedValues
        {
            get { return _observedValuesValue; }
            set { SetProperty(ref _observedValuesValue, value); }
        }

        /// <summary>
        /// Predicted production values from the fitted model.
        /// </summary>
        private decimal[] _predictedValuesValue;
        public decimal[] PredictedValues
        {
            get { return _predictedValuesValue; }
            set { SetProperty(ref _predictedValuesValue, value); }
        }

        /// <summary>
        /// Residuals (observed - predicted) for model evaluation.
        /// </summary>
        private decimal[] _residualsValue;
        public decimal[] Residuals
        {
            get { return _residualsValue; }
            set { SetProperty(ref _residualsValue, value); }
        }

        /// <summary>
        /// Coefficient of determination (R²) for model fit quality.
        /// </summary>
        private decimal _rSquaredValue;
        public decimal RSquared
        {
            get { return _rSquaredValue; }
            set { SetProperty(ref _rSquaredValue, value); }
        }

        /// <summary>
        /// Adjusted R² accounting for degrees of freedom.
        /// </summary>
        private decimal _adjustedRSquaredValue;
        public decimal AdjustedRSquared
        {
            get { return _adjustedRSquaredValue; }
            set { SetProperty(ref _adjustedRSquaredValue, value); }
        }

        /// <summary>
        /// Root Mean Square Error of the fit.
        /// </summary>
        private decimal _rmseValue;
        public decimal RMSE
        {
            get { return _rmseValue; }
            set { SetProperty(ref _rmseValue, value); }
        }

        /// <summary>
        /// Mean Absolute Error of the fit.
        /// </summary>
        private decimal _maeValue;
        public decimal MAE
        {
            get { return _maeValue; }
            set { SetProperty(ref _maeValue, value); }
        }

        /// <summary>
        /// Akaike Information Criterion for model selection.
        /// </summary>
        private decimal _aicValue;
        public decimal AIC
        {
            get { return _aicValue; }
            set { SetProperty(ref _aicValue, value); }
        }

        /// <summary>
        /// Bayesian Information Criterion for model comparison.
        /// </summary>
        private decimal _bicValue;
        public decimal BIC
        {
            get { return _bicValue; }
            set { SetProperty(ref _bicValue, value); }
        }

        /// <summary>
        /// Confidence intervals for fitted parameters.
        /// </summary>
        private (double LowerBound, double UpperBound)[] _confidenceIntervalsValue;
        public (double LowerBound, double UpperBound)[] ConfidenceIntervals
        {
            get { return _confidenceIntervalsValue; }
            set { SetProperty(ref _confidenceIntervalsValue, value); }
        }

        /// <summary>
        /// Number of iterations performed during fitting.
        /// </summary>
        private int _iterationsValue;
        public int Iterations
        {
            get { return _iterationsValue; }
            set { SetProperty(ref _iterationsValue, value); }
        }

        /// <summary>
        /// Whether the optimization converged successfully.
        /// </summary>
        private bool _convergedValue;
        public bool Converged
        {
            get { return _convergedValue; }
            set { SetProperty(ref _convergedValue, value); }
        }

        /// <summary>
        /// Decline curve type used in the analysis.
        /// </summary>
        private string _declineCurveTypeValue;
        public string DeclineCurveType
        {
            get { return _declineCurveTypeValue; }
            set { SetProperty(ref _declineCurveTypeValue, value); }
        }

        /// <summary>
        /// Analysis start date.
        /// </summary>
        private DateTime? _analysisStartDateValue;
        public DateTime? AnalysisStartDate
        {
            get { return _analysisStartDateValue; }
            set { SetProperty(ref _analysisStartDateValue, value); }
        }

        /// <summary>
        /// Analysis end date.
        /// </summary>
        private DateTime? _analysisEndDateValue;
        public DateTime? AnalysisEndDate
        {
            get { return _analysisEndDateValue; }
            set { SetProperty(ref _analysisEndDateValue, value); }
        }

        /// <summary>
        /// Well identifier for this analysis.
        /// </summary>
        private string _wellIdValue;
        public string WellId
        {
            get { return _wellIdValue; }
            set { SetProperty(ref _wellIdValue, value); }
        }

        /// <summary>
        /// Field identifier for this analysis.
        /// </summary>
        private string _fieldIdValue;
        public string FieldId
        {
            get { return _fieldIdValue; }
            set { SetProperty(ref _fieldIdValue, value); }
        }

        /// <summary>
        /// Forecast duration in days.
        /// </summary>
        private int _forecastDurationDaysValue;
        public int ForecastDurationDays
        {
            get { return _forecastDurationDaysValue; }
            set { SetProperty(ref _forecastDurationDaysValue, value); }
        }

        /// <summary>
        /// Estimated ultimate recovery (EUR) for this well.
        /// </summary>
        private decimal _estimatedUltimateRecoveryValue;
        public decimal EstimatedUltimateRecovery
        {
            get { return _estimatedUltimateRecoveryValue; }
            set { SetProperty(ref _estimatedUltimateRecoveryValue, value); }
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

        private string _dcaFitResultIdValue;
        public string DCA_FIT_RESULT_ID
        {
            get { return _dcaFitResultIdValue; }
            set { SetProperty(ref _dcaFitResultIdValue, value); }
        }
    }
}