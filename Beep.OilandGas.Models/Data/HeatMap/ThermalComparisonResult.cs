using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.HeatMap;

namespace Beep.OilandGas.Models.Data
{
     public class ThermalComparisonResult : ModelEntityBase
     {
         private string ComparisonIdValue = string.Empty;

         public string ComparisonId

         {

             get { return this.ComparisonIdValue; }

             set { SetProperty(ref ComparisonIdValue, value); }

         }
         private string LocationIdValue = string.Empty;

         public string LocationId

         {

             get { return this.LocationIdValue; }

             set { SetProperty(ref LocationIdValue, value); }

         }
         private DateTime ComparisonDateValue;

         public DateTime ComparisonDate

         {

             get { return this.ComparisonDateValue; }

             set { SetProperty(ref ComparisonDateValue, value); }

         }
         private DateTime BaselineDateValue;

         public DateTime BaselineDate

         {

             get { return this.BaselineDateValue; }

             set { SetProperty(ref BaselineDateValue, value); }

         }
         private DateTime CurrentDateValue;

         public DateTime CurrentDate

         {

             get { return this.CurrentDateValue; }

             set { SetProperty(ref CurrentDateValue, value); }

         }
         private decimal BaselineAverageTemperatureValue;

         public decimal BaselineAverageTemperature

         {

             get { return this.BaselineAverageTemperatureValue; }

             set { SetProperty(ref BaselineAverageTemperatureValue, value); }

         }
         private decimal CurrentAverageTemperatureValue;

         public decimal CurrentAverageTemperature

         {

             get { return this.CurrentAverageTemperatureValue; }

             set { SetProperty(ref CurrentAverageTemperatureValue, value); }

         }
         private decimal TemperatureChangeValue;

         public decimal TemperatureChange

         {

             get { return this.TemperatureChangeValue; }

             set { SetProperty(ref TemperatureChangeValue, value); }

         }
         private decimal PercentChangeValue;

         public decimal PercentChange

         {

             get { return this.PercentChangeValue; }

             set { SetProperty(ref PercentChangeValue, value); }

         }
         private decimal BaselineStdDevValue;

         public decimal BaselineStdDev

         {

             get { return this.BaselineStdDevValue; }

             set { SetProperty(ref BaselineStdDevValue, value); }

         }
         private decimal CurrentStdDevValue;

         public decimal CurrentStdDev

         {

             get { return this.CurrentStdDevValue; }

             set { SetProperty(ref CurrentStdDevValue, value); }

         }
         private string SignificantChangeValue = string.Empty;

         public string SignificantChange

         {

             get { return this.SignificantChangeValue; }

             set { SetProperty(ref SignificantChangeValue, value); }

         } // Yes, No
         private List<string> ChangePatternsValue = new();

         public List<string> ChangePatterns

         {

             get { return this.ChangePatternsValue; }

             set { SetProperty(ref ChangePatternsValue, value); }

         }
     }
}
