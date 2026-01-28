using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
     public class FluidBehaviorAnalysis : ModelEntityBase
     {
         private string AnalysisIdValue = string.Empty;

         public string AnalysisId

         {

             get { return this.AnalysisIdValue; }

             set { SetProperty(ref AnalysisIdValue, value); }

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
         private string FluidTypeValue = string.Empty;

         public string FluidType

         {

             get { return this.FluidTypeValue; }

             set { SetProperty(ref FluidTypeValue, value); }

         } // Black Oil, Volatile Oil, Condensate, etc.
         private decimal BubblePointPressureValue;

         public decimal BubblePointPressure

         {

             get { return this.BubblePointPressureValue; }

             set { SetProperty(ref BubblePointPressureValue, value); }

         }
         private decimal DewPointPressureValue;

         public decimal DewPointPressure

         {

             get { return this.DewPointPressureValue; }

             set { SetProperty(ref DewPointPressureValue, value); }

         }
         private decimal CriticalSolveGORValue;

         public decimal CriticalSolveGOR

         {

             get { return this.CriticalSolveGORValue; }

             set { SetProperty(ref CriticalSolveGORValue, value); }

         }
         private decimal DissolvedGORValue;

         public decimal DissolvedGOR

         {

             get { return this.DissolvedGORValue; }

             set { SetProperty(ref DissolvedGORValue, value); }

         }
         private string CharacteristicsValue = string.Empty;

         public string Characteristics

         {

             get { return this.CharacteristicsValue; }

             set { SetProperty(ref CharacteristicsValue, value); }

         }
         private List<string> BehaviorClassificationsValue = new();

         public List<string> BehaviorClassifications

         {

             get { return this.BehaviorClassificationsValue; }

             set { SetProperty(ref BehaviorClassificationsValue, value); }

         }
     }
}
