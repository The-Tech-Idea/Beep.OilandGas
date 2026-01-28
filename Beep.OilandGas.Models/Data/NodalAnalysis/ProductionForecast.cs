using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.Models.Data
{
     public class ProductionForecast : ModelEntityBase
     {
         private string ForecastIdValue = string.Empty;

         public string ForecastId

         {

             get { return this.ForecastIdValue; }

             set { SetProperty(ref ForecastIdValue, value); }

         }
         private string WellUWIValue = string.Empty;

         public string WellUWI

         {

             get { return this.WellUWIValue; }

             set { SetProperty(ref WellUWIValue, value); }

         }
         private DateTime ForecastDateValue;

         public DateTime ForecastDate

         {

             get { return this.ForecastDateValue; }

             set { SetProperty(ref ForecastDateValue, value); }

         }
         private int ForecastPeriodMonthsValue;

         public int ForecastPeriodMonths

         {

             get { return this.ForecastPeriodMonthsValue; }

             set { SetProperty(ref ForecastPeriodMonthsValue, value); }

         }
         public Dictionary<int, decimal> MonthlyProduction { get; set; } = new();
         private decimal TotalForecastedVolumeValue;

         public decimal TotalForecastedVolume

         {

             get { return this.TotalForecastedVolumeValue; }

             set { SetProperty(ref TotalForecastedVolumeValue, value); }

         }
         private decimal FinalProductionValue;

         public decimal FinalProduction

         {

             get { return this.FinalProductionValue; }

             set { SetProperty(ref FinalProductionValue, value); }

         }
         private decimal AverageMonthlyProductionValue;

         public decimal AverageMonthlyProduction

         {

             get { return this.AverageMonthlyProductionValue; }

             set { SetProperty(ref AverageMonthlyProductionValue, value); }

         }
         private int? EconomicLimitMonthValue;

         public int? EconomicLimitMonth

         {

             get { return this.EconomicLimitMonthValue; }

             set { SetProperty(ref EconomicLimitMonthValue, value); }

         }
     }
}
