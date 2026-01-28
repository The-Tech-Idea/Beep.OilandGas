using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.HeatMap;

namespace Beep.OilandGas.Models.Data
{
     public class ThermalImageQuality : ModelEntityBase
     {
         private string QualityIdValue = string.Empty;

         public string QualityId

         {

             get { return this.QualityIdValue; }

             set { SetProperty(ref QualityIdValue, value); }

         }
         private string ImageIdValue = string.Empty;

         public string ImageId

         {

             get { return this.ImageIdValue; }

             set { SetProperty(ref ImageIdValue, value); }

         }
         private DateTime AssessmentDateValue;

         public DateTime AssessmentDate

         {

             get { return this.AssessmentDateValue; }

             set { SetProperty(ref AssessmentDateValue, value); }

         }
         private decimal ClarityValue;

         public decimal Clarity

         {

             get { return this.ClarityValue; }

             set { SetProperty(ref ClarityValue, value); }

         } // 0-100
         private decimal NoiseLevelValue;

         public decimal NoiseLevel

         {

             get { return this.NoiseLevelValue; }

             set { SetProperty(ref NoiseLevelValue, value); }

         } // 0-100
         private decimal ContrastValue;

         public decimal Contrast

         {

             get { return this.ContrastValue; }

             set { SetProperty(ref ContrastValue, value); }

         } // 0-100
         private decimal OverallQualityScoreValue;

         public decimal OverallQualityScore

         {

             get { return this.OverallQualityScoreValue; }

             set { SetProperty(ref OverallQualityScoreValue, value); }

         } // 0-100
         private string QualityRatingValue = string.Empty;

         public string QualityRating

         {

             get { return this.QualityRatingValue; }

             set { SetProperty(ref QualityRatingValue, value); }

         } // Excellent, Good, Fair, Poor
         private List<string> QualityIssuesValue = new();

         public List<string> QualityIssues

         {

             get { return this.QualityIssuesValue; }

             set { SetProperty(ref QualityIssuesValue, value); }

         }
         private List<string> RecommendedImprovementsValue = new();

         public List<string> RecommendedImprovements

         {

             get { return this.RecommendedImprovementsValue; }

             set { SetProperty(ref RecommendedImprovementsValue, value); }

         }
     }
}
