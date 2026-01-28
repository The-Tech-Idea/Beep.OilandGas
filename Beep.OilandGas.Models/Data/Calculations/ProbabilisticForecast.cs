using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ProbabilisticForecast : ModelEntityBase
    {
        /// <summary>
        /// Forecast ID
        /// </summary>
        private string ForecastIdValue = string.Empty;

        public string ForecastId

        {

            get { return this.ForecastIdValue; }

            set { SetProperty(ref ForecastIdValue, value); }

        }

        /// <summary>
        /// Well UWI
        /// </summary>
        private string? WellUWIValue;

        public string? WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Field ID
        /// </summary>
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }

        /// <summary>
        /// P10 case forecast (low case)
        /// </summary>
        private ProductionForecastResult P10ForecastValue = new();

        public ProductionForecastResult P10Forecast

        {

            get { return this.P10ForecastValue; }

            set { SetProperty(ref P10ForecastValue, value); }

        }

        /// <summary>
        /// P50 case forecast (most likely)
        /// </summary>
        private ProductionForecastResult P50ForecastValue = new();

        public ProductionForecastResult P50Forecast

        {

            get { return this.P50ForecastValue; }

            set { SetProperty(ref P50ForecastValue, value); }

        }

        /// <summary>
        /// P90 case forecast (high case)
        /// </summary>
        private ProductionForecastResult P90ForecastValue = new();

        public ProductionForecastResult P90Forecast

        {

            get { return this.P90ForecastValue; }

            set { SetProperty(ref P90ForecastValue, value); }

        }

        /// <summary>
        /// Expected value forecast
        /// </summary>
        private ProductionForecastResult ExpectedForecastValue = new();

        public ProductionForecastResult ExpectedForecast

        {

            get { return this.ExpectedForecastValue; }

            set { SetProperty(ref ExpectedForecastValue, value); }

        }

        /// <summary>
        /// Risk analysis results
        /// </summary>
        private ForecastRiskAnalysisResult RiskAnalysisValue = new();

        public ForecastRiskAnalysisResult RiskAnalysis

        {

            get { return this.RiskAnalysisValue; }

            set { SetProperty(ref RiskAnalysisValue, value); }

        }
    }
}
