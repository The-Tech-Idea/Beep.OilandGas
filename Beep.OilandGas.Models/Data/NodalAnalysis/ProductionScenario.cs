using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
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

     
        /// <summary>
        /// Default constructor
        /// </summary>
        public ProductionScenario()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}


