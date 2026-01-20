using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    /// <summary>
    /// Represents a production scenario (Best/Base/Worst case).
    /// Used for scenario analysis of well performance.
    /// DTO for calculations - Entity class: PRODUCTION_SCENARIO
    /// </summary>
    public partial class ProductionScenario : ModelEntityBase {
        /// <summary>
        /// Scenario name (e.g., "Best Case", "Base Case", "Worst Case")
        /// </summary>
        private string _scenarioNameValue;
        public string ScenarioName
        {
            get { return _scenarioNameValue; }
            set { SetProperty(ref _scenarioNameValue, value); }
        }

        /// <summary>
        /// Detailed description of scenario assumptions
        /// </summary>
        private string _descriptionValue;
        public string Description
        {
            get { return _descriptionValue; }
            set { SetProperty(ref _descriptionValue, value); }
        }

        /// <summary>
        /// Expected flow rate under this scenario (bbl/day or equivalent)
        /// </summary>
        private double _flowRateValue;
        public double FlowRate
        {
            get { return _flowRateValue; }
            set { SetProperty(ref _flowRateValue, value); }
        }

        /// <summary>
        /// Expected bottomhole pressure (psi or equivalent)
        /// </summary>
        private double _bottomholePressureValue;
        public double BottomholePressure
        {
            get { return _bottomholePressureValue; }
            set { SetProperty(ref _bottomholePressureValue, value); }
        }

        /// <summary>
        /// Probability of this scenario occurring (0-1 fraction)
        /// </summary>
        private double? _probabilityValue;
        public double? Probability
        {
            get { return _probabilityValue; }
            set { SetProperty(ref _probabilityValue, value); }
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
        public ProductionScenario()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
