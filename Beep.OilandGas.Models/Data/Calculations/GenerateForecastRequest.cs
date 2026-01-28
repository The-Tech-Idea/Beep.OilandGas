using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class GenerateForecastRequest : ModelEntityBase
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier) - optional if FieldId is provided
        /// </summary>
        private string? WellUWIValue;

        public string? WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Field identifier - optional if WellUWI is provided
        /// </summary>
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }

        /// <summary>
        /// Forecast method (e.g., "DCA", "ARPS", "HYP")
        /// </summary>
        private ForecastType ForecastMethodValue = ForecastType.None;

        [Required(ErrorMessage = "ForecastMethod is required")]
        public ForecastType ForecastMethod

        {

            get { return this.ForecastMethodValue; }

            set { SetProperty(ref ForecastMethodValue, value); }

        }

        /// <summary>
        /// Forecast period in months
        /// </summary>
        private int ForecastPeriodValue;

        [Required]
        [Range(1, 600, ErrorMessage = "ForecastPeriod must be between 1 and 600 months")]
        public int ForecastPeriod

        {

            get { return this.ForecastPeriodValue; }

            set { SetProperty(ref ForecastPeriodValue, value); }

        }
        /// <summary>
        /// Variable operating cost per barrel (USD/STB)
        /// </summary>
        private decimal OperatingCostPerBarrelValue = 10m;

        public decimal OperatingCostPerBarrel

        {

            get { return this.OperatingCostPerBarrelValue; }

            set { SetProperty(ref OperatingCostPerBarrelValue, value); }

        }

        /// <summary>
        /// Fixed OPEX per period (USD)
        /// </summary>
        private decimal FixedOpexPerPeriodValue = 0m;

        public decimal FixedOpexPerPeriod

        {

            get { return this.FixedOpexPerPeriodValue; }

            set { SetProperty(ref FixedOpexPerPeriodValue, value); }

        }

        /// <summary>
        /// Optional capital schedule (date, amount) to apply per period
        /// </summary>
        private List<CapitalScheduleItem> CapitalScheduleValue = new();

        public List<CapitalScheduleItem> CapitalSchedule

        {

            get { return this.CapitalScheduleValue; }

            set { SetProperty(ref CapitalScheduleValue, value); }

        }
    }
}
