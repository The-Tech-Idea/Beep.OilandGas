using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokeProductionForecast : ModelEntityBase
    {
        private string ForecastIdValue = string.Empty;

        public string ForecastId

        {

            get { return this.ForecastIdValue; }

            set { SetProperty(ref ForecastIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private decimal CurrentProductionValue;

        public decimal CurrentProduction

        {

            get { return this.CurrentProductionValue; }

            set { SetProperty(ref CurrentProductionValue, value); }

        }
        private decimal CurrentChokeDiameterValue;

        public decimal CurrentChokeDiameter

        {

            get { return this.CurrentChokeDiameterValue; }

            set { SetProperty(ref CurrentChokeDiameterValue, value); }

        }
        private decimal ReservoirDeclineRateValue;

        public decimal ReservoirDeclineRate

        {

            get { return this.ReservoirDeclineRateValue; }

            set { SetProperty(ref ReservoirDeclineRateValue, value); }

        } // decimal/month
        private int ForecastMonthsValue;

        public int ForecastMonths

        {

            get { return this.ForecastMonthsValue; }

            set { SetProperty(ref ForecastMonthsValue, value); }

        }
        private List<ChokeProductionPoint> ProductionScenariosValue = new();

        public List<ChokeProductionPoint> ProductionScenarios

        {

            get { return this.ProductionScenariosValue; }

            set { SetProperty(ref ProductionScenariosValue, value); }

        }
        private List<ChokeOpeningAdjustment> RecommendedAdjustmentsValue = new();

        public List<ChokeOpeningAdjustment> RecommendedAdjustments

        {

            get { return this.RecommendedAdjustmentsValue; }

            set { SetProperty(ref RecommendedAdjustmentsValue, value); }

        }
        private decimal RequiredChokeOpeningByMonth12Value;

        public decimal RequiredChokeOpeningByMonth12

        {

            get { return this.RequiredChokeOpeningByMonth12Value; }

            set { SetProperty(ref RequiredChokeOpeningByMonth12Value, value); }

        }
        private decimal CumulativeProductionGainValue;

        public decimal CumulativeProductionGain

        {

            get { return this.CumulativeProductionGainValue; }

            set { SetProperty(ref CumulativeProductionGainValue, value); }

        } // BOPD-months
        private string StrategyValue = string.Empty;

        public string Strategy

        {

            get { return this.StrategyValue; }

            set { SetProperty(ref StrategyValue, value); }

        } // Conservative, Moderate, Aggressive
    }
}
