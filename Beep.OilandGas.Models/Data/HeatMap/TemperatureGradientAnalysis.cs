using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.HeatMap;

namespace Beep.OilandGas.Models.Data
{
     public class TemperatureGradientAnalysis : ModelEntityBase
     {
         private string GradientIdValue = string.Empty;

         public string GradientId

         {

             get { return this.GradientIdValue; }

             set { SetProperty(ref GradientIdValue, value); }

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
         private decimal AverageGradientValue;

         public decimal AverageGradient

         {

             get { return this.AverageGradientValue; }

             set { SetProperty(ref AverageGradientValue, value); }

         } // °C per unit distance
         private decimal MaxGradientValue;

         public decimal MaxGradient

         {

             get { return this.MaxGradientValue; }

             set { SetProperty(ref MaxGradientValue, value); }

         }
         private decimal MinGradientValue;

         public decimal MinGradient

         {

             get { return this.MinGradientValue; }

             set { SetProperty(ref MinGradientValue, value); }

         }
         private decimal HorizontalGradientValue;

         public decimal HorizontalGradient

         {

             get { return this.HorizontalGradientValue; }

             set { SetProperty(ref HorizontalGradientValue, value); }

         }
         private decimal VerticalGradientValue;

         public decimal VerticalGradient

         {

             get { return this.VerticalGradientValue; }

             set { SetProperty(ref VerticalGradientValue, value); }

         }
         private string GradientPatternValue = string.Empty;

         public string GradientPattern

         {

             get { return this.GradientPatternValue; }

             set { SetProperty(ref GradientPatternValue, value); }

         } // Linear, Exponential, Nonlinear
         private List<GradientPoint> GradientPointsValue = new();

         public List<GradientPoint> GradientPoints

         {

             get { return this.GradientPointsValue; }

             set { SetProperty(ref GradientPointsValue, value); }

         }
     }
}
