using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class RiskAssessmentResponse : ModelEntityBase
    {
        private string AssessmentIdValue = string.Empty;

        public string AssessmentId

        {

            get { return this.AssessmentIdValue; }

            set { SetProperty(ref AssessmentIdValue, value); }

        }
        private string? ProspectIdValue;

        public string? ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? RiskModelValue;

        public string? RiskModel

        {

            get { return this.RiskModelValue; }

            set { SetProperty(ref RiskModelValue, value); }

        }
        private DateTime AssessmentDateValue;

        public DateTime AssessmentDate

        {

            get { return this.AssessmentDateValue; }

            set { SetProperty(ref AssessmentDateValue, value); }

        }
        
        // Risk probabilities
        private decimal? TrapRiskValue;

        public decimal? TrapRisk

        {

            get { return this.TrapRiskValue; }

            set { SetProperty(ref TrapRiskValue, value); }

        }
        private decimal? ReservoirRiskValue;

        public decimal? ReservoirRisk

        {

            get { return this.ReservoirRiskValue; }

            set { SetProperty(ref ReservoirRiskValue, value); }

        }
        private decimal? SealRiskValue;

        public decimal? SealRisk

        {

            get { return this.SealRiskValue; }

            set { SetProperty(ref SealRiskValue, value); }

        }
        private decimal? SourceRiskValue;

        public decimal? SourceRisk

        {

            get { return this.SourceRiskValue; }

            set { SetProperty(ref SourceRiskValue, value); }

        }
        private decimal? TimingRiskValue;

        public decimal? TimingRisk

        {

            get { return this.TimingRiskValue; }

            set { SetProperty(ref TimingRiskValue, value); }

        }
        private decimal? OverallGeologicalRiskValue;

        public decimal? OverallGeologicalRisk

        {

            get { return this.OverallGeologicalRiskValue; }

            set { SetProperty(ref OverallGeologicalRiskValue, value); }

        } // Product of all risks
        
        // Risked volumes (unrisked volume * overall risk)
        private decimal? RiskedOilVolumeValue;

        public decimal? RiskedOilVolume

        {

            get { return this.RiskedOilVolumeValue; }

            set { SetProperty(ref RiskedOilVolumeValue, value); }

        }
        private decimal? RiskedGasVolumeValue;

        public decimal? RiskedGasVolume

        {

            get { return this.RiskedGasVolumeValue; }

            set { SetProperty(ref RiskedGasVolumeValue, value); }

        }
        private decimal? UnriskedOilVolumeValue;

        public decimal? UnriskedOilVolume

        {

            get { return this.UnriskedOilVolumeValue; }

            set { SetProperty(ref UnriskedOilVolumeValue, value); }

        }
        private decimal? UnriskedGasVolumeValue;

        public decimal? UnriskedGasVolume

        {

            get { return this.UnriskedGasVolumeValue; }

            set { SetProperty(ref UnriskedGasVolumeValue, value); }

        }
        
        // Volume estimates
        private decimal? LowEstimateOilValue;

        public decimal? LowEstimateOil

        {

            get { return this.LowEstimateOilValue; }

            set { SetProperty(ref LowEstimateOilValue, value); }

        }
        private decimal? BestEstimateOilValue;

        public decimal? BestEstimateOil

        {

            get { return this.BestEstimateOilValue; }

            set { SetProperty(ref BestEstimateOilValue, value); }

        }
        private decimal? HighEstimateOilValue;

        public decimal? HighEstimateOil

        {

            get { return this.HighEstimateOilValue; }

            set { SetProperty(ref HighEstimateOilValue, value); }

        }
        private decimal? LowEstimateGasValue;

        public decimal? LowEstimateGas

        {

            get { return this.LowEstimateGasValue; }

            set { SetProperty(ref LowEstimateGasValue, value); }

        }
        private decimal? BestEstimateGasValue;

        public decimal? BestEstimateGas

        {

            get { return this.BestEstimateGasValue; }

            set { SetProperty(ref BestEstimateGasValue, value); }

        }
        private decimal? HighEstimateGasValue;

        public decimal? HighEstimateGas

        {

            get { return this.HighEstimateGasValue; }

            set { SetProperty(ref HighEstimateGasValue, value); }

        }
        
        // Economic assessment (if provided)
        private decimal? RiskedNPVValue;

        public decimal? RiskedNPV

        {

            get { return this.RiskedNPVValue; }

            set { SetProperty(ref RiskedNPVValue, value); }

        }
        private decimal? UnriskedNPVValue;

        public decimal? UnriskedNPV

        {

            get { return this.UnriskedNPVValue; }

            set { SetProperty(ref UnriskedNPVValue, value); }

        }
        private decimal? ExpectedMonetaryValueValue;

        public decimal? ExpectedMonetaryValue

        {

            get { return this.ExpectedMonetaryValueValue; }

            set { SetProperty(ref ExpectedMonetaryValueValue, value); }

        }
        
        // Risk classification
        private string? RiskCategoryValue;

        public string? RiskCategory

        {

            get { return this.RiskCategoryValue; }

            set { SetProperty(ref RiskCategoryValue, value); }

        } // e.g., "LOW", "MEDIUM", "HIGH", "VERY_HIGH"
        private List<string> RiskFactorsValue = new List<string>();

        public List<string> RiskFactors

        {

            get { return this.RiskFactorsValue; }

            set { SetProperty(ref RiskFactorsValue, value); }

        } // List of key risk factors
        private List<string> RecommendationsValue = new List<string>();

        public List<string> Recommendations

        {

            get { return this.RecommendationsValue; }

            set { SetProperty(ref RecommendationsValue, value); }

        } // Risk mitigation recommendations
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
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
