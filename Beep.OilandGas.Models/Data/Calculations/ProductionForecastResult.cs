using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ProductionForecastResult : ModelEntityBase
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
        /// Forecast creation date
        /// </summary>
        private DateTime ForecastDateValue;

        public DateTime ForecastDate

        {

            get { return this.ForecastDateValue; }

            set { SetProperty(ref ForecastDateValue, value); }

        }

        /// <summary>
        /// Forecast method used
        /// </summary>
        private ForecastType ForecastMethodValue = ForecastType.Decline;

        public ForecastType ForecastMethod

        {

            get { return this.ForecastMethodValue; }

            set { SetProperty(ref ForecastMethodValue, value); }

        }

        /// <summary>
        /// Forecast points
        /// </summary>
        private List<ProductionForecastPoint> ForecastPointsValue = new();

        public List<ProductionForecastPoint> ForecastPoints

        {

            get { return this.ForecastPointsValue; }

            set { SetProperty(ref ForecastPointsValue, value); }

        }

        /// <summary>
        /// Estimated reserves (STB or MSCF)
        /// </summary>
        private decimal EstimatedReservesValue;

        public decimal EstimatedReserves

        {

            get { return this.EstimatedReservesValue; }

            set { SetProperty(ref EstimatedReservesValue, value); }

        }

        /// <summary>
        /// Forecast status
        /// </summary>
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

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
        /// Forecast confidence interval (%)
        /// </summary>
        private decimal? ConfidenceIntervalValue;

        public decimal? ConfidenceInterval

        {

            get { return this.ConfidenceIntervalValue; }

            set { SetProperty(ref ConfidenceIntervalValue, value); }

        }

        /// <summary>
        /// Economic limit rate (STB/day)
        /// </summary>
        private decimal? EconomicLimitValue;

        public decimal? EconomicLimit

        {

            get { return this.EconomicLimitValue; }

            set { SetProperty(ref EconomicLimitValue, value); }

        }

        /// <summary>
        /// Forecast notes/comments
        /// </summary>
        private string? NotesValue;

        public string? Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }
}
