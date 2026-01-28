using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
     public class PropertyTrendAnalysis : ModelEntityBase
     {
         private string TrendIdValue = string.Empty;

         public string TrendId

         {

             get { return this.TrendIdValue; }

             set { SetProperty(ref TrendIdValue, value); }

         }
         private string CompositionIdValue = string.Empty;

         public string CompositionId

         {

             get { return this.CompositionIdValue; }

             set { SetProperty(ref CompositionIdValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private string PropertyNameValue = string.Empty;

         public string PropertyName

         {

             get { return this.PropertyNameValue; }

             set { SetProperty(ref PropertyNameValue, value); }

         }
         private List<decimal> PropertyValuesValue = new();

         public List<decimal> PropertyValues

         {

             get { return this.PropertyValuesValue; }

             set { SetProperty(ref PropertyValuesValue, value); }

         }
         private List<decimal> PressureRangeValue = new();

         public List<decimal> PressureRange

         {

             get { return this.PressureRangeValue; }

             set { SetProperty(ref PressureRangeValue, value); }

         }
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

         } // Increasing, Decreasing, Linear
         private decimal RSquaredValue;

         public decimal RSquared

         {

             get { return this.RSquaredValue; }

             set { SetProperty(ref RSquaredValue, value); }

         } // Fit quality
     }
}
