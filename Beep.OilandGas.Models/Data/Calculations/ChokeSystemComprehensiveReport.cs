using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokeSystemComprehensiveReport : ModelEntityBase
    {
        private string ReportIdValue = string.Empty;

        public string ReportId

        {

            get { return this.ReportIdValue; }

            set { SetProperty(ref ReportIdValue, value); }

        }
        private DateTime ReportDateValue;

        public DateTime ReportDate

        {

            get { return this.ReportDateValue; }

            set { SetProperty(ref ReportDateValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string OperatingStatusValue = string.Empty;

        public string OperatingStatus

        {

            get { return this.OperatingStatusValue; }

            set { SetProperty(ref OperatingStatusValue, value); }

        } // Active, Shut-in, Suspended
        
        // Current conditions
        private AdvancedChokeAnalysis CurrentChokeAnalysisValue = new();

        public AdvancedChokeAnalysis CurrentChokeAnalysis

        {

            get { return this.CurrentChokeAnalysisValue; }

            set { SetProperty(ref CurrentChokeAnalysisValue, value); }

        }
        private ChokePerformanceDiagnostics PerformanceDiagnosticsValue = new();

        public ChokePerformanceDiagnostics PerformanceDiagnostics

        {

            get { return this.PerformanceDiagnosticsValue; }

            set { SetProperty(ref PerformanceDiagnosticsValue, value); }

        }
        private ChokeErosionPrediction ErosionPredictionValue = new();

        public ChokeErosionPrediction ErosionPrediction

        {

            get { return this.ErosionPredictionValue; }

            set { SetProperty(ref ErosionPredictionValue, value); }

        }
        private ChokeSandCutRiskAssessment SandRiskAssessmentValue = new();

        public ChokeSandCutRiskAssessment SandRiskAssessment

        {

            get { return this.SandRiskAssessmentValue; }

            set { SetProperty(ref SandRiskAssessmentValue, value); }

        }
        
        // Optimization
        private ChokeBackPressureOptimization OptimizationAnalysisValue = new();

        public ChokeBackPressureOptimization OptimizationAnalysis

        {

            get { return this.OptimizationAnalysisValue; }

            set { SetProperty(ref OptimizationAnalysisValue, value); }

        }
        private ChokeProductionForecast ProductionForecastValue = new();

        public ChokeProductionForecast PRODUCTION_FORECAST

        {

            get { return this.ProductionForecastValue; }

            set { SetProperty(ref ProductionForecastValue, value); }

        }
        
        // Equipment
        private ChokeTrimSelection EquipmentRecommendationValue = new();

        public ChokeTrimSelection EquipmentRecommendation

        {

            get { return this.EquipmentRecommendationValue; }

            set { SetProperty(ref EquipmentRecommendationValue, value); }

        }
        
        // Overall summary
        private decimal CurrentProductionValue;

        public decimal CurrentProduction

        {

            get { return this.CurrentProductionValue; }

            set { SetProperty(ref CurrentProductionValue, value); }

        }
        private decimal OptimizedProductionValue;

        public decimal OptimizedProduction

        {

            get { return this.OptimizedProductionValue; }

            set { SetProperty(ref OptimizedProductionValue, value); }

        }
        private decimal ProductionPotentialValue;

        public decimal ProductionPotential

        {

            get { return this.ProductionPotentialValue; }

            set { SetProperty(ref ProductionPotentialValue, value); }

        } // percent increase
        private string OverallHealthStatusValue = string.Empty;

        public string OverallHealthStatus

        {

            get { return this.OverallHealthStatusValue; }

            set { SetProperty(ref OverallHealthStatusValue, value); }

        } // Excellent, Good, Fair, Poor
        private List<string> KeyFindingsValue = new();

        public List<string> KeyFindings

        {

            get { return this.KeyFindingsValue; }

            set { SetProperty(ref KeyFindingsValue, value); }

        }
        private List<string> PriorityActionsValue = new();

        public List<string> PriorityActions

        {

            get { return this.PriorityActionsValue; }

            set { SetProperty(ref PriorityActionsValue, value); }

        }
        private decimal EstimatedRevenueImpactValue;

        public decimal EstimatedRevenueImpact

        {

            get { return this.EstimatedRevenueImpactValue; }

            set { SetProperty(ref EstimatedRevenueImpactValue, value); }

        } // $/day
    }
}
