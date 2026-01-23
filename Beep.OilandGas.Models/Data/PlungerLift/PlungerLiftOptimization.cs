using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.PlungerLift
{
    /// <summary>
    /// Represents plunger lift system optimization analysis and results.
    /// Analyzes operating parameters for optimal well performance and economics.
    /// DTO for calculations - Entity class: PLUNGER_LIFT_OPTIMIZATION
    /// </summary>
    public partial class PlungerLiftOptimization : ModelEntityBase {
        /// <summary>
        /// Optimal opening pressure for casing plunger (psia)
        /// </summary>
        private double? _optimalOpeningPressureValue;
        public double? OptimalOpeningPressure
        {
            get { return _optimalOpeningPressureValue; }
            set { SetProperty(ref _optimalOpeningPressureValue, value); }
        }

        /// <summary>
        /// Optimal plunger cycle time (minutes between plunger launches)
        /// </summary>
        private double? _optimalCycleTimeValue;
        public double? OptimalCycleTime
        {
            get { return _optimalCycleTimeValue; }
            set { SetProperty(ref _optimalCycleTimeValue, value); }
        }

        /// <summary>
        /// Optimal gas injection rate for gas lift assist (MCFD)
        /// </summary>
        private double? _optimalGasInjectionRateValue;
        public double? OptimalGasInjectionRate
        {
            get { return _optimalGasInjectionRateValue; }
            set { SetProperty(ref _optimalGasInjectionRateValue, value); }
        }

        /// <summary>
        /// Expected production rate at optimum operating conditions (bbl/day)
        /// </summary>
        private double? _optimizedFlowRateValue;
        public double? OptimizedFlowRate
        {
            get { return _optimizedFlowRateValue; }
            set { SetProperty(ref _optimizedFlowRateValue, value); }
        }

        /// <summary>
        /// Expected plunger arrival frequency (arrivals per day at optimum settings)
        /// </summary>
        private double? _plungerArrivalFrequencyValue;
        public double? PlungerArrivalFrequency
        {
            get { return _plungerArrivalFrequencyValue; }
            set { SetProperty(ref _plungerArrivalFrequencyValue, value); }
        }

        /// <summary>
        /// Predicted system efficiency at optimal conditions (percentage)
        /// </summary>
        private double? _systemEfficiencyValue;
        public double? SystemEfficiency
        {
            get { return _systemEfficiencyValue; }
            set { SetProperty(ref _systemEfficiencyValue, value); }
        }

        /// <summary>
        /// Expected operating gas volume (MSCF/day) needed for system
        /// </summary>
        private double? _requiredGasVolumeValue;
        public double? RequiredGasVolume
        {
            get { return _requiredGasVolumeValue; }
            set { SetProperty(ref _requiredGasVolumeValue, value); }
        }

        /// <summary>
        /// Estimated annual production increase vs. non-assisted method (bbl)
        /// </summary>
        private double? _annualProductionIncreaseValue;
        public double? AnnualProductionIncrease
        {
            get { return _annualProductionIncreaseValue; }
            set { SetProperty(ref _annualProductionIncreaseValue, value); }
        }

        /// <summary>
        /// Estimated annual cost savings vs. alternative method (dollars)
        /// </summary>
        private decimal? _annualCostSavingsValue;
        public decimal? AnnualCostSavings
        {
            get { return _annualCostSavingsValue; }
            set { SetProperty(ref _annualCostSavingsValue, value); }
        }

        /// <summary>
        /// Estimated payback period for plunger lift equipment (months)
        /// </summary>
        private double? _paybackPeriodValue;
        public double? PaybackPeriod
        {
            get { return _paybackPeriodValue; }
            set { SetProperty(ref _paybackPeriodValue, value); }
        }

        /// <summary>
        /// Recommendation priority (Urgent, High, Medium, Low, Not Recommended)
        /// </summary>
        private string _recommendationPriorityValue;
        public string RecommendationPriority
        {
            get { return _recommendationPriorityValue; }
            set { SetProperty(ref _recommendationPriorityValue, value); }
        }

        /// <summary>
        /// Implementation complexity (Simple, Moderate, Complex, Very Complex)
        /// </summary>
        private string _implementationComplexityValue;
        public string ImplementationComplexity
        {
            get { return _implementationComplexityValue; }
            set { SetProperty(ref _implementationComplexityValue, value); }
        }

        /// <summary>
        /// Risk factors and mitigation strategies
        /// </summary>
        private string _riskAssessmentValue;
        public string RiskAssessment
        {
            get { return _riskAssessmentValue; }
            set { SetProperty(ref _riskAssessmentValue, value); }
        }

        /// <summary>
        /// Technical notes and optimization assumptions
        /// </summary>
        private string _notesValue;
        public string Notes
        {
            get { return _notesValue; }
            set { SetProperty(ref _notesValue, value); }
        }

        

        /// <summary>
        /// Default constructor
        /// </summary>
        public PlungerLiftOptimization()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}


