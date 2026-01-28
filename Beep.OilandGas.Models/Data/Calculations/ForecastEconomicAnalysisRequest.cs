using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ForecastEconomicAnalysisRequest : ModelEntityBase
    {
        private string ForecastIdValue = string.Empty;

        public string ForecastId

        {

            get { return this.ForecastIdValue; }

            set { SetProperty(ref ForecastIdValue, value); }

        }
        private decimal OilPriceValue;

        public decimal OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }
        private decimal GasPriceValue;

        public decimal GasPrice

        {

            get { return this.GasPriceValue; }

            set { SetProperty(ref GasPriceValue, value); }

        }
        private decimal DiscountRateValue;

        public decimal DiscountRate

        {

            get { return this.DiscountRateValue; }

            set { SetProperty(ref DiscountRateValue, value); }

        }

        private decimal OperatingCostPerBarrelValue = 10m;


        public decimal OperatingCostPerBarrel


        {


            get { return this.OperatingCostPerBarrelValue; }


            set { SetProperty(ref OperatingCostPerBarrelValue, value); }


        }
        private decimal FixedOpexPerPeriodValue = 0m;

        public decimal FixedOpexPerPeriod

        {

            get { return this.FixedOpexPerPeriodValue; }

            set { SetProperty(ref FixedOpexPerPeriodValue, value); }

        }
        private List<CapitalScheduleItem> CapitalScheduleValue = new();

        public List<CapitalScheduleItem> CapitalSchedule

        {

            get { return this.CapitalScheduleValue; }

            set { SetProperty(ref CapitalScheduleValue, value); }

        }
    }
}
