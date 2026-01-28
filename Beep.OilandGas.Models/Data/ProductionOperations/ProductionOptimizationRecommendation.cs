using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionOperations
{
    public class ProductionOptimizationRecommendation : ModelEntityBase
    {
        private string RecommendationIdValue = string.Empty;

        public string RecommendationId

        {

            get { return this.RecommendationIdValue; }

            set { SetProperty(ref RecommendationIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string RecommendationTypeValue = string.Empty;

        public string RecommendationType

        {

            get { return this.RecommendationTypeValue; }

            set { SetProperty(ref RecommendationTypeValue, value); }

        }
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private decimal ExpectedImprovementValue;

        public decimal ExpectedImprovement

        {

            get { return this.ExpectedImprovementValue; }

            set { SetProperty(ref ExpectedImprovementValue, value); }

        }
        private string PriorityValue = "Medium";

        public string Priority

        {

            get { return this.PriorityValue; }

            set { SetProperty(ref PriorityValue, value); }

        }
    }
}
