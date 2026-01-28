using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class FieldDevelopmentStrategyResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private double FieldAreaValue;

        public double FieldArea

        {

            get { return this.FieldAreaValue; }

            set { SetProperty(ref FieldAreaValue, value); }

        }
        private double EstimatedReservesValue;

        public double EstimatedReserves

        {

            get { return this.EstimatedReservesValue; }

            set { SetProperty(ref EstimatedReservesValue, value); }

        }
        private string ReservoirTypeValue;

        public string ReservoirType

        {

            get { return this.ReservoirTypeValue; }

            set { SetProperty(ref ReservoirTypeValue, value); }

        }
        private string RecommendedApproachValue;

        public string RecommendedApproach

        {

            get { return this.RecommendedApproachValue; }

            set { SetProperty(ref RecommendedApproachValue, value); }

        }
        private int WellSpacingValue;

        public int WellSpacing

        {

            get { return this.WellSpacingValue; }

            set { SetProperty(ref WellSpacingValue, value); }

        }
        private int PhaseCountValue;

        public int PhaseCount

        {

            get { return this.PhaseCountValue; }

            set { SetProperty(ref PhaseCountValue, value); }

        }
        private double AnnualProductionValue;

        public double AnnualProduction

        {

            get { return this.AnnualProductionValue; }

            set { SetProperty(ref AnnualProductionValue, value); }

        }
        private string ComplexityValue;

        public string Complexity

        {

            get { return this.ComplexityValue; }

            set { SetProperty(ref ComplexityValue, value); }

        }
        private string RiskLevelValue;

        public string RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        }
        private List<string> RecommendedTechnologiesValue;

        public List<string> RecommendedTechnologies

        {

            get { return this.RecommendedTechnologiesValue; }

            set { SetProperty(ref RecommendedTechnologiesValue, value); }

        }
        private List<string> InfrastructureRequirementsValue;

        public List<string> InfrastructureRequirements

        {

            get { return this.InfrastructureRequirementsValue; }

            set { SetProperty(ref InfrastructureRequirementsValue, value); }

        }
    }
}
