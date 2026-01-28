using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.HeatMap;

namespace Beep.OilandGas.Models.Data
{
     public class ThermalAnalysisResult : ModelEntityBase
     {
         private string AnalysisIdValue = string.Empty;

         public string AnalysisId

         {

             get { return this.AnalysisIdValue; }

             set { SetProperty(ref AnalysisIdValue, value); }

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
         private decimal AverageTemperatureValue;

         public decimal AverageTemperature

         {

             get { return this.AverageTemperatureValue; }

             set { SetProperty(ref AverageTemperatureValue, value); }

         }
         private decimal MaximumTemperatureValue;

         public decimal MaximumTemperature

         {

             get { return this.MaximumTemperatureValue; }

             set { SetProperty(ref MaximumTemperatureValue, value); }

         }
         private decimal MinimumTemperatureValue;

         public decimal MinimumTemperature

         {

             get { return this.MinimumTemperatureValue; }

             set { SetProperty(ref MinimumTemperatureValue, value); }

         }
         private decimal TemperatureGradientValue;

         public decimal TemperatureGradient

         {

             get { return this.TemperatureGradientValue; }

             set { SetProperty(ref TemperatureGradientValue, value); }

         }
         private decimal StandardDeviationValue;

         public decimal StandardDeviation

         {

             get { return this.StandardDeviationValue; }

             set { SetProperty(ref StandardDeviationValue, value); }

         }
         private string ThermalPatternValue = string.Empty;

         public string ThermalPattern

         {

             get { return this.ThermalPatternValue; }

             set { SetProperty(ref ThermalPatternValue, value); }

         } // Hot Spot, Cold Spot, Uniform
         private int DataPointCountValue;

         public int DataPointCount

         {

             get { return this.DataPointCountValue; }

             set { SetProperty(ref DataPointCountValue, value); }

         }
         private decimal TemperatureRangeValue;

         public decimal TemperatureRange

         {

             get { return this.TemperatureRangeValue; }

             set { SetProperty(ref TemperatureRangeValue, value); }

         }
     }
}
