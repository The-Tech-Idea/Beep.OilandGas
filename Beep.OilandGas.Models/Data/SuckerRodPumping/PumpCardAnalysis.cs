using System;
using System.Collections.Generic;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    /// <summary>
    /// Represents detailed pump card analysis results.
    /// Analyzes pump efficiency, card shape, and pump performance characteristics.
    /// DTO for calculations - Entity class: PUMP_CARD_ANALYSIS
    /// </summary>
    public partial class PumpCardAnalysis : ModelEntityBase {
        /// <summary>
        /// Pump card shape classification (Ideal, Normal, Gas Interference, Traveling Valve Leak, etc.)
        /// </summary>
        private string _cardShapeValue;
        public string CardShape
        {
            get { return _cardShapeValue; }
            set { SetProperty(ref _cardShapeValue, value); }
        }

        /// <summary>
        /// Pump efficiency percentage (0-100)
        /// </summary>
        private double? _pumpEfficiencyValue;
        public double? PumpEfficiency
        {
            get { return _pumpEfficiencyValue; }
            set { SetProperty(ref _pumpEfficiencyValue, value); }
        }

        /// <summary>
        /// Volumetric efficiency percentage (actual flow / theoretical flow)
        /// </summary>
        private double? _volumetricEfficiencyValue;
        public double? VolumetricEfficiency
        {
            get { return _volumetricEfficiencyValue; }
            set { SetProperty(ref _volumetricEfficiencyValue, value); }
        }

        /// <summary>
        /// Mechanical efficiency percentage
        /// </summary>
        private double? _mechanicalEfficiencyValue;
        public double? MechanicalEfficiency
        {
            get { return _mechanicalEfficiencyValue; }
            set { SetProperty(ref _mechanicalEfficiencyValue, value); }
        }

        /// <summary>
        /// Maximum pump pressure (psi)
        /// </summary>
        private double? _maximumPressureValue;
        public double? MaximumPressure
        {
            get { return _maximumPressureValue; }
            set { SetProperty(ref _maximumPressureValue, value); }
        }

        /// <summary>
        /// Minimum pump pressure (psi)
        /// </summary>
        private double? _minimumPressureValue;
        public double? MinimumPressure
        {
            get { return _minimumPressureValue; }
            set { SetProperty(ref _minimumPressureValue, value); }
        }

        /// <summary>
        /// Pressure differential across pump (psi)
        /// </summary>
        private double? _pressureDifferentialValue;
        public double? PressureDifferential
        {
            get { return _pressureDifferentialValue; }
            set { SetProperty(ref _pressureDifferentialValue, value); }
        }

        /// <summary>
        /// Pump card area (indicator card area, proportional to work done)
        /// </summary>
        private double? _cardAreaValue;
        public double? CardArea
        {
            get { return _cardAreaValue; }
            set { SetProperty(ref _cardAreaValue, value); }
        }

        /// <summary>
        /// Indicates presence of gas interference on pump card
        /// </summary>
        private bool? _hasGasInterferenceValue;
        public bool? HasGasInterference
        {
            get { return _hasGasInterferenceValue; }
            set { SetProperty(ref _hasGasInterferenceValue, value); }
        }

        /// <summary>
        /// Indicates traveling valve leak condition
        /// </summary>
        private bool? _hasTravelingValveLeakValue;
        public bool? HasTravelingValveLeak
        {
            get { return _hasTravelingValveLeakValue; }
            set { SetProperty(ref _hasTravelingValveLeakValue, value); }
        }

        /// <summary>
        /// Indicates standing valve leak condition
        /// </summary>
        private bool? _hasStandingValveLeakValue;
        public bool? HasStandingValveLeak
        {
            get { return _hasStandingValveLeakValue; }
            set { SetProperty(ref _hasStandingValveLeakValue, value); }
        }

        /// <summary>
        /// Pump condition assessment (Excellent, Good, Fair, Poor)
        /// </summary>
        private string _conditionAssessmentValue;
        public string ConditionAssessment
        {
            get { return _conditionAssessmentValue; }
            set { SetProperty(ref _conditionAssessmentValue, value); }
        }

        /// <summary>
        /// Recommended action (Run As-Is, Monitor, Schedule Repair, Replace Immediately)
        /// </summary>
        private string _recommendedActionValue;
        public string RecommendedAction
        {
            get { return _recommendedActionValue; }
            set { SetProperty(ref _recommendedActionValue, value); }
        }

        /// <summary>
        /// Analysis notes and observations
        /// </summary>
        private string _analysisNotesValue;
        public string AnalysisNotes
        {
            get { return _analysisNotesValue; }
            set { SetProperty(ref _analysisNotesValue, value); }
        }

      
        /// <summary>
        /// Default constructor
        /// </summary>
        public PumpCardAnalysis()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}


