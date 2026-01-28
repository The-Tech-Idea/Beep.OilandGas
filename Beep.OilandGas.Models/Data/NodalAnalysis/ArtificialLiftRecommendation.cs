using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.Models.Data
{
     public class ArtificialLiftRecommendation : ModelEntityBase
     {
         private string RecommendationIdValue = string.Empty;

         public string RecommendationId

         {

             get { return this.RecommendationIdValue; }

             set { SetProperty(ref RecommendationIdValue, value); }

         }
         private string WellUWIValue = string.Empty;

         public string WellUWI

         {

             get { return this.WellUWIValue; }

             set { SetProperty(ref WellUWIValue, value); }

         }
         private DateTime RecommendationDateValue;

         public DateTime RecommendationDate

         {

             get { return this.RecommendationDateValue; }

             set { SetProperty(ref RecommendationDateValue, value); }

         }
         private string PrimaryRecommendationValue = string.Empty;

         public string PrimaryRecommendation

         {

             get { return this.PrimaryRecommendationValue; }

             set { SetProperty(ref PrimaryRecommendationValue, value); }

         }
         private List<string> AlternativeRecommendationsValue = new();

         public List<string> AlternativeRecommendations

         {

             get { return this.AlternativeRecommendationsValue; }

             set { SetProperty(ref AlternativeRecommendationsValue, value); }

         }
         private decimal RecommendedCapacityValue;

         public decimal RecommendedCapacity

         {

             get { return this.RecommendedCapacityValue; }

             set { SetProperty(ref RecommendedCapacityValue, value); }

         }
         private decimal EstimatedNPVValue;

         public decimal EstimatedNPV

         {

             get { return this.EstimatedNPVValue; }

             set { SetProperty(ref EstimatedNPVValue, value); }

         }
         private List<string> RiskFactorsValue = new();

         public List<string> RiskFactors

         {

             get { return this.RiskFactorsValue; }

             set { SetProperty(ref RiskFactorsValue, value); }

         }
     }
}
