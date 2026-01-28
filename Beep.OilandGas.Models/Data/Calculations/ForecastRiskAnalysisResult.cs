using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ForecastRiskAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Risk analysis ID
        /// </summary>
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Probability of commercial success (%)
        /// </summary>
        private decimal CommercialSuccessProbabilityValue;

        public decimal CommercialSuccessProbability

        {

            get { return this.CommercialSuccessProbabilityValue; }

            set { SetProperty(ref CommercialSuccessProbabilityValue, value); }

        }

        /// <summary>
        /// Risk factors identified
        /// </summary>
        private List<RiskFactor> RiskFactorsValue = new();

        public List<RiskFactor> RiskFactors

        {

            get { return this.RiskFactorsValue; }

            set { SetProperty(ref RiskFactorsValue, value); }

        }

        /// <summary>
        /// Mitigation strategies
        /// </summary>
        private List<string> MitigationStrategiesValue = new();

        public List<string> MitigationStrategies

        {

            get { return this.MitigationStrategiesValue; }

            set { SetProperty(ref MitigationStrategiesValue, value); }

        }

        /// <summary>
        /// Overall risk rating
        /// </summary>
        private string RiskRatingValue = string.Empty;

        public string RiskRating

        {

            get { return this.RiskRatingValue; }

            set { SetProperty(ref RiskRatingValue, value); }

        }
    }
}
