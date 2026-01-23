using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.GasLift
{
    /// <summary>
    /// Risk analysis result for gas lift operations
    /// </summary>
    public class GasLiftRiskAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the analysis date
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the user who performed the analysis
        /// </summary>
        private string AnalyzedByUserValue = string.Empty;

        public string AnalyzedByUser

        {

            get { return this.AnalyzedByUserValue; }

            set { SetProperty(ref AnalyzedByUserValue, value); }

        }

        /// <summary>
        /// Gets or sets the well UWI
        /// </summary>
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Gets or sets the tubing/casing stress risk (0-100%)
        /// </summary>
        private decimal TubingCasingStressRiskValue;

        public decimal TubingCasingStressRisk

        {

            get { return this.TubingCasingStressRiskValue; }

            set { SetProperty(ref TubingCasingStressRiskValue, value); }

        }

        /// <summary>
        /// Gets or sets the scale/corrosion risk (0-100%)
        /// </summary>
        private decimal ScaleCorrosionRiskValue;

        public decimal ScaleCorrosionRisk

        {

            get { return this.ScaleCorrosionRiskValue; }

            set { SetProperty(ref ScaleCorrosionRiskValue, value); }

        }

        /// <summary>
        /// Gets or sets the valve reliability risk (0-100%)
        /// </summary>
        private decimal ValveReliabilityRiskValue;

        public decimal ValveReliabilityRisk

        {

            get { return this.ValveReliabilityRiskValue; }

            set { SetProperty(ref ValveReliabilityRiskValue, value); }

        }

        /// <summary>
        /// Gets or sets the gas supply interruption risk (0-100%)
        /// </summary>
        private decimal GasSupplyInterruptionRiskValue;

        public decimal GasSupplyInterruptionRisk

        {

            get { return this.GasSupplyInterruptionRiskValue; }

            set { SetProperty(ref GasSupplyInterruptionRiskValue, value); }

        }

        /// <summary>
        /// Gets or sets the overall risk rating (0-100%)
        /// </summary>
        private decimal OverallRiskRatingValue;

        public decimal OverallRiskRating

        {

            get { return this.OverallRiskRatingValue; }

            set { SetProperty(ref OverallRiskRatingValue, value); }

        }

        /// <summary>
        /// Gets or sets the risk level (Low, Medium, High, Critical)
        /// </summary>
        private string RiskLevelValue = "Medium";

        public string RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        }

        /// <summary>
        /// Gets or sets the recommended mitigation actions
        /// </summary>
        private List<string> RecommendedMitigationActionsValue = new();

        public List<string> RecommendedMitigationActions

        {

            get { return this.RecommendedMitigationActionsValue; }

            set { SetProperty(ref RecommendedMitigationActionsValue, value); }

        }
    }
}



