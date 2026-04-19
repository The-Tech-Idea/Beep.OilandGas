using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class ProspectEvaluation : ModelEntityBase
    {
        private string EvaluationIdValue = string.Empty;

        public string EvaluationId

        {

            get { return this.EvaluationIdValue; }

            set { SetProperty(ref EvaluationIdValue, value); }

        }
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private DateTime EvaluationDateValue;

        public DateTime EvaluationDate

        {

            get { return this.EvaluationDateValue; }

            set { SetProperty(ref EvaluationDateValue, value); }

        }
        private string EvaluatedByValue = string.Empty;

        public string EvaluatedBy

        {

            get { return this.EvaluatedByValue; }

            set { SetProperty(ref EvaluatedByValue, value); }

        }
        private decimal? EstimatedOilResourcesValue;

        public decimal? EstimatedOilResources

        {

            get { return this.EstimatedOilResourcesValue; }

            set { SetProperty(ref EstimatedOilResourcesValue, value); }

        }
        private decimal? EstimatedGasResourcesValue;

        public decimal? EstimatedGasResources

        {

            get { return this.EstimatedGasResourcesValue; }

            set { SetProperty(ref EstimatedGasResourcesValue, value); }

        }
        private string? ResourceUnitValue;

        public string? ResourceUnit

        {

            get { return this.ResourceUnitValue; }

            set { SetProperty(ref ResourceUnitValue, value); }

        }
        private decimal? ProbabilityOfSuccessValue;

        public decimal? ProbabilityOfSuccess

        {

            get { return this.ProbabilityOfSuccessValue; }

            set { SetProperty(ref ProbabilityOfSuccessValue, value); }

        }
        private decimal? RiskScoreValue;

        public decimal? RiskScore

        {

            get { return this.RiskScoreValue; }

            set { SetProperty(ref RiskScoreValue, value); }

        }
        private string? RiskLevelValue;

        public string? RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        }
        private string? RecommendationValue;

        public string? Recommendation

        {

            get { return this.RecommendationValue; }

            set { SetProperty(ref RecommendationValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
        private List<RiskFactor> RiskFactorsValue = new();

        public List<RiskFactor> RiskFactors

        {

            get { return this.RiskFactorsValue; }

            set { SetProperty(ref RiskFactorsValue, value); }

        }

        public string Potential { get; set; }
        public string FieldId { get; set; }
    }
}
