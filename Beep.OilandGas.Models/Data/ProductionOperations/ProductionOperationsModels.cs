using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionOperations
{
    /// <summary>
    /// DTO for production data.
    /// </summary>
    public class ProductionData : ModelEntityBase
    {
        private string ProductionIdValue = string.Empty;

        public string ProductionId

        {

            get { return this.ProductionIdValue; }

            set { SetProperty(ref ProductionIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime ProductionDateValue;

        public DateTime ProductionDate

        {

            get { return this.ProductionDateValue; }

            set { SetProperty(ref ProductionDateValue, value); }

        }
        private decimal OilVolumeValue;

        public decimal OilVolume

        {

            get { return this.OilVolumeValue; }

            set { SetProperty(ref OilVolumeValue, value); }

        }
        private decimal GasVolumeValue;

        public decimal GasVolume

        {

            get { return this.GasVolumeValue; }

            set { SetProperty(ref GasVolumeValue, value); }

        }
        private decimal WaterVolumeValue;

        public decimal WaterVolume

        {

            get { return this.WaterVolumeValue; }

            set { SetProperty(ref WaterVolumeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    /// <summary>
    /// DTO for production optimization recommendation.
    /// </summary>
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



