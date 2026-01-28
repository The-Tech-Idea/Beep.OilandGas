using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class GasLiftAnalysisResult : ModelEntityBase
    {
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string AnalysisTypeValue = string.Empty;

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
        
        // Optimal results
        private decimal OptimalGasInjectionRateValue;

        public decimal OptimalGasInjectionRate

        {

            get { return this.OptimalGasInjectionRateValue; }

            set { SetProperty(ref OptimalGasInjectionRateValue, value); }

        } // Mscf/day
        private decimal MaximumProductionRateValue;

        public decimal MaximumProductionRate

        {

            get { return this.MaximumProductionRateValue; }

            set { SetProperty(ref MaximumProductionRateValue, value); }

        } // bbl/day
        private decimal OptimalGasLiquidRatioValue;

        public decimal OptimalGasLiquidRatio

        {

            get { return this.OptimalGasLiquidRatioValue; }

            set { SetProperty(ref OptimalGasLiquidRatioValue, value); }

        }
        
        // Performance curve points
        private List<GasLiftPerformancePoint> PerformancePointsValue = new List<GasLiftPerformancePoint>();

        public List<GasLiftPerformancePoint> PerformancePoints

        {

            get { return this.PerformancePointsValue; }

            set { SetProperty(ref PerformancePointsValue, value); }

        }

        private string? PerformancePointsJsonValue;

        public string? PerformancePointsJson

        {

            get { return this.PerformancePointsJsonValue; }

            set { SetProperty(ref PerformancePointsJsonValue, value); }

        }
        
        // Valve design results (if AnalysisType is VALVE_DESIGN)
        private List<GasLiftValveData>? ValvesValue;

        public List<GasLiftValveData>? Valves

        {

            get { return this.ValvesValue; }

            set { SetProperty(ref ValvesValue, value); }

        }

        private string? ValvesJsonValue;

        public string? ValvesJson

        {

            get { return this.ValvesJsonValue; }

            set { SetProperty(ref ValvesJsonValue, value); }

        }
        private decimal? TotalGasInjectionRateValue;

        public decimal? TotalGasInjectionRate

        {

            get { return this.TotalGasInjectionRateValue; }

            set { SetProperty(ref TotalGasInjectionRateValue, value); }

        }
        private decimal? ExpectedProductionRateValue;

        public decimal? ExpectedProductionRate

        {

            get { return this.ExpectedProductionRateValue; }

            set { SetProperty(ref ExpectedProductionRateValue, value); }

        }
        private decimal? SystemEfficiencyValue;

        public decimal? SystemEfficiency

        {

            get { return this.SystemEfficiencyValue; }

            set { SetProperty(ref SystemEfficiencyValue, value); }

        }
        
        // Valve spacing results (if AnalysisType is VALVE_SPACING)
        private List<decimal>? ValveDepthsValue;

        public List<decimal>? ValveDepths

        {

            get { return this.ValveDepthsValue; }

            set { SetProperty(ref ValveDepthsValue, value); }

        } // feet

        private string? ValveDepthsJsonValue;

        public string? ValveDepthsJson

        {

            get { return this.ValveDepthsJsonValue; }

            set { SetProperty(ref ValveDepthsJsonValue, value); }

        }
        private List<decimal>? OpeningPressuresValue;

        public List<decimal>? OpeningPressures

        {

            get { return this.OpeningPressuresValue; }

            set { SetProperty(ref OpeningPressuresValue, value); }

        } // psia
        private int? NumberOfValvesValue;

        public int? NumberOfValves

        {

            get { return this.NumberOfValvesValue; }

            set { SetProperty(ref NumberOfValvesValue, value); }

        }
        private decimal? TotalDepthCoverageValue;

        public decimal? TotalDepthCoverage

        {

            get { return this.TotalDepthCoverageValue; }

            set { SetProperty(ref TotalDepthCoverageValue, value); }

        } // feet
        
        // Additional metadata
        public GasLiftAnalysisAdditionalResults? AdditionalResults { get; set; }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // SUCCESS, FAILED
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
