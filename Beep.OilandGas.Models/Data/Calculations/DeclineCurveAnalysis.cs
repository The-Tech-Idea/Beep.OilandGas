using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DeclineCurveAnalysis : ModelEntityBase
    {
        /// <summary>
        /// Analysis ID
        /// </summary>
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Well UWI
        /// </summary>
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Analysis date
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Decline type (Exponential, Hyperbolic, Harmonic)
        /// </summary>
        private string DeclineTypeValue = string.Empty;

        public string DeclineType

        {

            get { return this.DeclineTypeValue; }

            set { SetProperty(ref DeclineTypeValue, value); }

        }

        /// <summary>
        /// Initial decline rate (1/time)
        /// </summary>
        private decimal DeclineRateValue;

        public decimal DeclineRate

        {

            get { return this.DeclineRateValue; }

            set { SetProperty(ref DeclineRateValue, value); }

        }

        /// <summary>
        /// Hyperbolic b-factor (for hyperbolic decline)
        /// </summary>
        private decimal? BFactorValue;

        public decimal? BFactor

        {

            get { return this.BFactorValue; }

            set { SetProperty(ref BFactorValue, value); }

        }

        /// <summary>
        /// Initial production rate (STB/day or MSCF/day)
        /// </summary>
        private decimal InitialRateValue;

        public decimal InitialRate

        {

            get { return this.InitialRateValue; }

            set { SetProperty(ref InitialRateValue, value); }

        }

        /// <summary>
        /// Estimated ultimate recovery (EUR)
        /// </summary>
        private decimal EstimatedReservesValue;

        public decimal EstimatedReserves

        {

            get { return this.EstimatedReservesValue; }

            set { SetProperty(ref EstimatedReservesValue, value); }

        }

        /// <summary>
        /// R-squared for curve fit quality
        /// </summary>
        private decimal? RSquaredValue;

        public decimal? RSquared

        {

            get { return this.RSquaredValue; }

            set { SetProperty(ref RSquaredValue, value); }

        }

        /// <summary>
        /// Root mean square error
        /// </summary>
        private decimal? RMSEValue;

        public decimal? RMSE

        {

            get { return this.RMSEValue; }

            set { SetProperty(ref RMSEValue, value); }

        }

        /// <summary>
        /// Mean absolute error
        /// </summary>
        private decimal? MAEValue;

        public decimal? MAE

        {

            get { return this.MAEValue; }

            set { SetProperty(ref MAEValue, value); }

        }

        /// <summary>
        /// Akaike information criterion
        /// </summary>
        private decimal? AICValue;

        public decimal? AIC

        {

            get { return this.AICValue; }

            set { SetProperty(ref AICValue, value); }

        }

        /// <summary>
        /// Bayesian information criterion
        /// </summary>
        private decimal? BICValue;

        public decimal? BIC

        {

            get { return this.BICValue; }

            set { SetProperty(ref BICValue, value); }

        }

        /// <summary>
        /// Analysis status
        /// </summary>
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }

        /// <summary>
        /// Analysis notes/comments
        /// </summary>
        private string? NotesValue;

        public string? Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }

        /// <summary>
        /// Historical production data points used in analysis
        /// </summary>
        private List<ForecastProductionDataPoint> HistoricalDataValue = new();

        public List<ForecastProductionDataPoint> HistoricalData

        {

            get { return this.HistoricalDataValue; }

            set { SetProperty(ref HistoricalDataValue, value); }

        }
    }
}
