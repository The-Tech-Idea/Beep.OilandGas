using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    /// <summary>
    /// Represents a single time step in VLP degradation forecast.
    /// Shows VLP performance at a specific year in the forecast.
    /// DTO for calculations - Entity class: VLP_FORECAST_TIME_STEP
    /// </summary>
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
        public VLPForecastTimeStep()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
