using System;
using System.Collections.Generic;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    /// <summary>
    /// Represents year-by-year VLP degradation forecast.
    /// Shows how well performance deteriorates over time due to erosion, scale, and other effects.
    /// DTO for calculations - Entity class: TIME_STEP_VLP_FORECAST
    /// </summary>
    public partial class TimeStepVLPForecast : ModelEntityBase {
        /// <summary>
        /// Total degradation from year 0 to final year as percentage increase in pressure drop
        /// </summary>
        private double? _totalDegradationPercentageValue;
        public double? TotalDegradationPercentage
        {
            get { return _totalDegradationPercentageValue; }
            set { SetProperty(ref _totalDegradationPercentageValue, value); }
        }

        /// <summary>
        /// Number of years included in forecast
        /// </summary>
        private int? _yearsForecastedValue;
        public int? YearsForecasted
        {
            get { return _yearsForecastedValue; }
            set { SetProperty(ref _yearsForecastedValue, value); }
        }

        /// <summary>
        /// Annual sand production rate used in forecast (tons/year)
        /// </summary>
        private double? _annualSandProductionValue;
        public double? AnnualSandProduction
        {
            get { return _annualSandProductionValue; }
            set { SetProperty(ref _annualSandProductionValue, value); }
        }

        /// <summary>
        /// Annual scale buildup rate used in forecast (fraction of diameter per year)
        /// </summary>
        private double? _annualScaleRateValue;
        public double? AnnualScaleRate
        {
            get { return _annualScaleRateValue; }
            set { SetProperty(ref _annualScaleRateValue, value); }
        }

        /// <summary>
        /// Collection of year-by-year forecast points
        /// </summary>
        private List<VLPForecastTimeStep> _forecastTimeStepsValue;
        public List<VLPForecastTimeStep> ForecastTimeSteps
        {
            get { return _forecastTimeStepsValue ??= new List<VLPForecastTimeStep>(); }
            set { SetProperty(ref _forecastTimeStepsValue, value); }
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
        public TimeStepVLPForecast()
        {
            ForecastTimeSteps = new List<VLPForecastTimeStep>();
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
