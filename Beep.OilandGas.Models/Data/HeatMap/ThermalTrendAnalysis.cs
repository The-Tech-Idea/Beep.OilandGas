using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.HeatMap;

namespace Beep.OilandGas.Models.Data
{
     public class ThermalTrendAnalysis : ModelEntityBase
     {
         private string TrendIdValue = string.Empty;

         public string TrendId

         {

             get { return this.TrendIdValue; }

             set { SetProperty(ref TrendIdValue, value); }

         }
         private string LocationIdValue = string.Empty;

         public string LocationId

         {

             get { return this.LocationIdValue; }

             set { SetProperty(ref LocationIdValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private int MonthsAnalyzedValue;

         public int MonthsAnalyzed

         {

             get { return this.MonthsAnalyzedValue; }

             set { SetProperty(ref MonthsAnalyzedValue, value); }

         }
         private decimal TemperatureTrendValue;

         public decimal TemperatureTrend

         {

             get { return this.TemperatureTrendValue; }

             set { SetProperty(ref TemperatureTrendValue, value); }

         } // °C/month
         private decimal TrendSlopeValue;

         public decimal TrendSlope

         {

             get { return this.TrendSlopeValue; }

             set { SetProperty(ref TrendSlopeValue, value); }

         }
         private string TrendDirectionValue = string.Empty;

         public string TrendDirection

         {

             get { return this.TrendDirectionValue; }

             set { SetProperty(ref TrendDirectionValue, value); }

         } // Increasing, Decreasing, Stable
         private decimal PercentChangeValue;

         public decimal PercentChange

         {

             get { return this.PercentChangeValue; }

             set { SetProperty(ref PercentChangeValue, value); }

         }
         private List<decimal> HistoricalTemperaturesValue = new();

         public List<decimal> HistoricalTemperatures

         {

             get { return this.HistoricalTemperaturesValue; }

             set { SetProperty(ref HistoricalTemperaturesValue, value); }

         }
         private decimal PredictedTemperatureValue;

         public decimal PredictedTemperature

         {

             get { return this.PredictedTemperatureValue; }

             set { SetProperty(ref PredictedTemperatureValue, value); }

         }
         private int PredictionMonthsValue;

         public int PredictionMonths

         {

             get { return this.PredictionMonthsValue; }

             set { SetProperty(ref PredictionMonthsValue, value); }

         }
         private decimal RSquaredValue;

         public decimal RSquared

         {

             get { return this.RSquaredValue; }

             set { SetProperty(ref RSquaredValue, value); }

         } // Trend confidence
     }
}
