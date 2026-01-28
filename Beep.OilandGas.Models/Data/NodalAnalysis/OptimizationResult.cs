using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.Models.Data
{
     public class OptimizationResult : ModelEntityBase
     {
         private string OptimizationIdValue = string.Empty;

         public string OptimizationId

         {

             get { return this.OptimizationIdValue; }

             set { SetProperty(ref OptimizationIdValue, value); }

         }
         private string WellUWIValue = string.Empty;

         public string WellUWI

         {

             get { return this.WellUWIValue; }

             set { SetProperty(ref WellUWIValue, value); }

         }
         private DateTime OptimizationDateValue;

         public DateTime OptimizationDate

         {

             get { return this.OptimizationDateValue; }

             set { SetProperty(ref OptimizationDateValue, value); }

         }
         private OperatingPoint RecommendedOperatingPointValue = new();

         public OperatingPoint RecommendedOperatingPoint

         {

             get { return this.RecommendedOperatingPointValue; }

             set { SetProperty(ref RecommendedOperatingPointValue, value); }

         }
         private decimal ImprovementPercentageValue;

         public decimal ImprovementPercentage

         {

             get { return this.ImprovementPercentageValue; }

             set { SetProperty(ref ImprovementPercentageValue, value); }

         }
         private List<string> RecommendationsValue = new();

         public List<string> Recommendations

         {

             get { return this.RecommendationsValue; }

             set { SetProperty(ref RecommendationsValue, value); }

         }
     }
}
