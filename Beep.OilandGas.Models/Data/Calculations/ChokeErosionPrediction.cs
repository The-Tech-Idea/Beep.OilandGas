using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokeErosionPrediction : ModelEntityBase
    {
        private string PredictionIdValue = string.Empty;

        public string PredictionId

        {

            get { return this.PredictionIdValue; }

            set { SetProperty(ref PredictionIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal SandProductionRateValue;

        public decimal SandProductionRate

        {

            get { return this.SandProductionRateValue; }

            set { SetProperty(ref SandProductionRateValue, value); }

        } // lb/day
        private decimal SandParticleSizeValue;

        public decimal SandParticleSize

        {

            get { return this.SandParticleSizeValue; }

            set { SetProperty(ref SandParticleSizeValue, value); }

        } // microns
        private decimal ParticleVelocityValue;

        public decimal ParticleVelocity

        {

            get { return this.ParticleVelocityValue; }

            set { SetProperty(ref ParticleVelocityValue, value); }

        } // ft/sec
        private decimal ChokeMaterialValue;

        public decimal ChokeMaterial

        {

            get { return this.ChokeMaterialValue; }

            set { SetProperty(ref ChokeMaterialValue, value); }

        } // Hardness rating
        private decimal ErosionRateValue;

        public decimal ErosionRate

        {

            get { return this.ErosionRateValue; }

            set { SetProperty(ref ErosionRateValue, value); }

        } // mils/year (0.001 inch/year)
        private decimal EstimatedChokeLifeValue;

        public decimal EstimatedChokeLife

        {

            get { return this.EstimatedChokeLifeValue; }

            set { SetProperty(ref EstimatedChokeLifeValue, value); }

        } // years
        private int DaysUntilReplacementValue;

        public int DaysUntilReplacement

        {

            get { return this.DaysUntilReplacementValue; }

            set { SetProperty(ref DaysUntilReplacementValue, value); }

        }
        private decimal CumulativeWearDepthValue;

        public decimal CumulativeWearDepth

        {

            get { return this.CumulativeWearDepthValue; }

            set { SetProperty(ref CumulativeWearDepthValue, value); }

        } // mils
        private string WearStatusValue = string.Empty;

        public string WearStatus

        {

            get { return this.WearStatusValue; }

            set { SetProperty(ref WearStatusValue, value); }

        } // Good, Fair, Poor, Critical
        private decimal ErosionSeverityValue;

        public decimal ErosionSeverity

        {

            get { return this.ErosionSeverityValue; }

            set { SetProperty(ref ErosionSeverityValue, value); }

        } // 0-100 scale
        private List<string> RecommendedActionsValue = new();

        public List<string> RecommendedActions

        {

            get { return this.RecommendedActionsValue; }

            set { SetProperty(ref RecommendedActionsValue, value); }

        }
    }
}
