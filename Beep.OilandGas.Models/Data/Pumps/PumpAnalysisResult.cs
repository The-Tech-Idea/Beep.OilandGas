using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pumps
{
    public class PumpAnalysisResult : ModelEntityBase
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
        private string? FacilityIdValue;

        public string? FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string? EquipmentIdValue;

        public string? EquipmentId

        {

            get { return this.EquipmentIdValue; }

            set { SetProperty(ref EquipmentIdValue, value); }

        }
        private string PumpTypeValue = string.Empty;

        public string PumpType

        {

            get { return this.PumpTypeValue; }

            set { SetProperty(ref PumpTypeValue, value); }

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
        
        // Performance results
        private decimal? FlowRateValue;

        public decimal? FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        } // GPM or bbl/day
        private decimal? HeadValue;

        public decimal? Head

        {

            get { return this.HeadValue; }

            set { SetProperty(ref HeadValue, value); }

        } // feet
        private decimal? PowerValue;

        public decimal? Power

        {

            get { return this.PowerValue; }

            set { SetProperty(ref PowerValue, value); }

        } // horsepower
        private decimal? EfficiencyValue;

        public decimal? Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        } // fraction 0-1
        private decimal? BestEfficiencyPointValue;

        public decimal? BestEfficiencyPoint

        {

            get { return this.BestEfficiencyPointValue; }

            set { SetProperty(ref BestEfficiencyPointValue, value); }

        } // flow rate at BEP
        
        // Performance curve points
        private List<PumpPerformancePoint>? PerformanceCurveValue;

        public List<PumpPerformancePoint>? PerformanceCurve

        {

            get { return this.PerformanceCurveValue; }

            set { SetProperty(ref PerformanceCurveValue, value); }

        }

        private string? PerformanceCurveJsonValue;

        public string? PerformanceCurveJson

        {

            get { return this.PerformanceCurveJsonValue; }

            set { SetProperty(ref PerformanceCurveJsonValue, value); }

        }
        
        // System analysis
        private decimal? OperatingPointFlowRateValue;

        public decimal? OperatingPointFlowRate

        {

            get { return this.OperatingPointFlowRateValue; }

            set { SetProperty(ref OperatingPointFlowRateValue, value); }

        }
        private decimal? OperatingPointHeadValue;

        public decimal? OperatingPointHead

        {

            get { return this.OperatingPointHeadValue; }

            set { SetProperty(ref OperatingPointHeadValue, value); }

        }
        private decimal? NPSHAvailableValue;

        public decimal? NPSHAvailable

        {

            get { return this.NPSHAvailableValue; }

            set { SetProperty(ref NPSHAvailableValue, value); }

        } // feet
        private decimal? NPSHRequiredValue;

        public decimal? NPSHRequired

        {

            get { return this.NPSHRequiredValue; }

            set { SetProperty(ref NPSHRequiredValue, value); }

        } // feet
        private bool? CavitationRiskValue;

        public bool? CavitationRisk

        {

            get { return this.CavitationRiskValue; }

            set { SetProperty(ref CavitationRiskValue, value); }

        }
        
        // Design results (for ESP)
        private int? RecommendedStagesValue;

        public int? RecommendedStages

        {

            get { return this.RecommendedStagesValue; }

            set { SetProperty(ref RecommendedStagesValue, value); }

        }
        private decimal? RecommendedMotorSizeValue;

        public decimal? RecommendedMotorSize

        {

            get { return this.RecommendedMotorSizeValue; }

            set { SetProperty(ref RecommendedMotorSizeValue, value); }

        } // horsepower
        
        // Additional metadata
        public PumpAnalysisAdditionalResults? AdditionalResults { get; set; }
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
