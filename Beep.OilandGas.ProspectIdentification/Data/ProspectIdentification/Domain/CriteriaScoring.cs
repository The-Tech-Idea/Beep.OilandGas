using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class CriteriaScoring : ModelEntityBase
     {
         private string CriteriaNameValue = string.Empty;

         public string CriteriaName

         {

             get { return this.CriteriaNameValue; }

             set { SetProperty(ref CriteriaNameValue, value); }

         }
         private decimal WeightValue;

         public decimal Weight

         {

             get { return this.WeightValue; }

             set { SetProperty(ref WeightValue, value); }

         }
         private decimal RawScoreValue;

         public decimal RawScore

         {

             get { return this.RawScoreValue; }

             set { SetProperty(ref RawScoreValue, value); }

         }
         private decimal WeightedScoreValue;

         public decimal WeightedScore

         {

             get { return this.WeightedScoreValue; }

             set { SetProperty(ref WeightedScoreValue, value); }

         }
         private string CategoryValue = string.Empty;

         public string Category

         {

             get { return this.CategoryValue; }

             set { SetProperty(ref CategoryValue, value); }

         }
     }
}
