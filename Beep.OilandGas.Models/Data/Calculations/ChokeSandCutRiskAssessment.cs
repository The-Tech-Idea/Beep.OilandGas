using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokeSandCutRiskAssessment : ModelEntityBase
    {
        private string AssessmentIdValue = string.Empty;

        public string AssessmentId

        {

            get { return this.AssessmentIdValue; }

            set { SetProperty(ref AssessmentIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private decimal EstimatedSandRateValue;

        public decimal EstimatedSandRate

        {

            get { return this.EstimatedSandRateValue; }

            set { SetProperty(ref EstimatedSandRateValue, value); }

        } // lb/day
        private decimal SandGrainSizeValue;

        public decimal SandGrainSize

        {

            get { return this.SandGrainSizeValue; }

            set { SetProperty(ref SandGrainSizeValue, value); }

        } // microns
        private decimal WellProdDepthValue;

        public decimal WellProdDepth

        {

            get { return this.WellProdDepthValue; }

            set { SetProperty(ref WellProdDepthValue, value); }

        }
        private decimal ChokeDepthValue;

        public decimal ChokeDepth

        {

            get { return this.ChokeDepthValue; }

            set { SetProperty(ref ChokeDepthValue, value); }

        }
        private decimal SettlingVelocityValue;

        public decimal SettlingVelocity

        {

            get { return this.SettlingVelocityValue; }

            set { SetProperty(ref SettlingVelocityValue, value); }

        } // ft/sec
        private decimal FlowVelocityValue;

        public decimal FlowVelocity

        {

            get { return this.FlowVelocityValue; }

            set { SetProperty(ref FlowVelocityValue, value); }

        } // ft/sec
        private decimal SandMigrationRiskValue;

        public decimal SandMigrationRisk

        {

            get { return this.SandMigrationRiskValue; }

            set { SetProperty(ref SandMigrationRiskValue, value); }

        } // 0-100 scale
        private string SandStatusValue = string.Empty;

        public string SandStatus

        {

            get { return this.SandStatusValue; }

            set { SetProperty(ref SandStatusValue, value); }

        } // Low, Moderate, High, Severe
        private List<string> SandMigrationPointsValue = new();

        public List<string> SandMigrationPoints

        {

            get { return this.SandMigrationPointsValue; }

            set { SetProperty(ref SandMigrationPointsValue, value); }

        }
        private decimal PredictedChokeDamageRateValue;

        public decimal PredictedChokeDamageRate

        {

            get { return this.PredictedChokeDamageRateValue; }

            set { SetProperty(ref PredictedChokeDamageRateValue, value); }

        } // mils/year
        private int DaysUntilChokeReplacementValue;

        public int DaysUntilChokeReplacement

        {

            get { return this.DaysUntilChokeReplacementValue; }

            set { SetProperty(ref DaysUntilChokeReplacementValue, value); }

        }
        private string RecommendationValue = string.Empty;

        public string Recommendation

        {

            get { return this.RecommendationValue; }

            set { SetProperty(ref RecommendationValue, value); }

        }
    }
}
