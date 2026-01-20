using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.EconomicAnalysis
{
    /// <summary>
    /// Represents results from economic sensitivity analysis.
    /// Shows how project economics respond to parameter variations (price, costs, rates, etc.).
    /// DTO for calculations - Entity class: ECONOMIC_SENSITIVITY_RESULT
    /// </summary>
    public partial class EconomicSensitivityResult : ModelEntityBase {
        /// <summary>
        /// Parameter being analyzed (e.g., "Oil Price", "Operating Cost", "Discount Rate")
        /// </summary>
        private string _parameterNameValue;
        public string ParameterName
        {
            get { return _parameterNameValue; }
            set { SetProperty(ref _parameterNameValue, value); }
        }

        /// <summary>
        /// Unit of measurement for parameter
        /// </summary>
        private string _parameterUnitValue;
        public string ParameterUnit
        {
            get { return _parameterUnitValue; }
            set { SetProperty(ref _parameterUnitValue, value); }
        }

        /// <summary>
        /// Base case parameter value
        /// </summary>
        private decimal? _baseParameterValueValue;
        public decimal? BaseParameterValue
        {
            get { return _baseParameterValueValue; }
            set { SetProperty(ref _baseParameterValueValue, value); }
        }

        /// <summary>
        /// Low case parameter value (for sensitivity range)
        /// </summary>
        private decimal? _lowParameterValueValue;
        public decimal? LowParameterValue
        {
            get { return _lowParameterValueValue; }
            set { SetProperty(ref _lowParameterValueValue, value); }
        }

        /// <summary>
        /// High case parameter value (for sensitivity range)
        /// </summary>
        private decimal? _highParameterValueValue;
        public decimal? HighParameterValue
        {
            get { return _highParameterValueValue; }
            set { SetProperty(ref _highParameterValueValue, value); }
        }

        /// <summary>
        /// NPV at low case parameter value
        /// </summary>
        private decimal? _lowCaseNpvValue;
        public decimal? LowCaseNPV
        {
            get { return _lowCaseNpvValue; }
            set { SetProperty(ref _lowCaseNpvValue, value); }
        }

        /// <summary>
        /// NPV at base case parameter value
        /// </summary>
        private decimal? _baseCaseNpvValue;
        public decimal? BaseCaseNPV
        {
            get { return _baseCaseNpvValue; }
            set { SetProperty(ref _baseCaseNpvValue, value); }
        }

        /// <summary>
        /// NPV at high case parameter value
        /// </summary>
        private decimal? _highCaseNpvValue;
        public decimal? HighCaseNPV
        {
            get { return _highCaseNpvValue; }
            set { SetProperty(ref _highCaseNpvValue, value); }
        }

        /// <summary>
        /// IRR at low case parameter value (percentage)
        /// </summary>
        private double? _lowCaseIrrValue;
        public double? LowCaseIRR
        {
            get { return _lowCaseIrrValue; }
            set { SetProperty(ref _lowCaseIrrValue, value); }
        }

        /// <summary>
        /// IRR at base case parameter value (percentage)
        /// </summary>
        private double? _baseCaseIrrValue;
        public double? BaseCaseIRR
        {
            get { return _baseCaseIrrValue; }
            set { SetProperty(ref _baseCaseIrrValue, value); }
        }

        /// <summary>
        /// IRR at high case parameter value (percentage)
        /// </summary>
        private double? _highCaseIrrValue;
        public double? HighCaseIRR
        {
            get { return _highCaseIrrValue; }
            set { SetProperty(ref _highCaseIrrValue, value); }
        }

        /// <summary>
        /// NPV sensitivity to parameter (slope of NPV vs parameter curve)
        /// How much NPV changes per unit parameter change
        /// </summary>
        private decimal? _npvSensitivityValue;
        public decimal? NPVSensitivity
        {
            get { return _npvSensitivityValue; }
            set { SetProperty(ref _npvSensitivityValue, value); }
        }

        /// <summary>
        /// IRR sensitivity to parameter (slope of IRR vs parameter curve)
        /// </summary>
        private double? _irrSensitivityValue;
        public double? IRRSensitivity
        {
            get { return _irrSensitivityValue; }
            set { SetProperty(ref _irrSensitivityValue, value); }
        }

        /// <summary>
        /// Elasticity - percentage change in NPV per 1% change in parameter
        /// </summary>
        private double? _elasticityValue;
        public double? Elasticity
        {
            get { return _elasticityValue; }
            set { SetProperty(ref _elasticityValue, value); }
        }

        /// <summary>
        /// Parameter range used for analysis (difference between high and low)
        /// </summary>
        private decimal? _parameterRangeValue;
        public decimal? ParameterRange
        {
            get { return _parameterRangeValue; }
            set { SetProperty(ref _parameterRangeValue, value); }
        }

        /// <summary>
        /// Impact ranking (how important this parameter is compared to others)
        /// 1 = most important, higher numbers = less important
        /// </summary>
        private int? _impactRankValue;
        public int? ImpactRank
        {
            get { return _impactRankValue; }
            set { SetProperty(ref _impactRankValue, value); }
        }

        /// <summary>
        /// Sensitivity classification (High, Medium, Low)
        /// </summary>
        private string _sensitivityLevelValue;
        public string SensitivityLevel
        {
            get { return _sensitivityLevelValue; }
            set { SetProperty(ref _sensitivityLevelValue, value); }
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
        public EconomicSensitivityResult()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
