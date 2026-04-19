using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class RiskCategory : ModelEntityBase
     {
         private string CategoryNameValue = string.Empty;

         public string CategoryName

         {

             get { return this.CategoryNameValue; }

             set { SetProperty(ref CategoryNameValue, value); }

         }
         private decimal RiskScoreValue;

         public decimal RiskScore

         {

             get { return this.RiskScoreValue; }

             set { SetProperty(ref RiskScoreValue, value); }

         }
         private string RiskLevelValue = string.Empty;

         public string RiskLevel

         {

             get { return this.RiskLevelValue; }

             set { SetProperty(ref RiskLevelValue, value); }

         }
         private List<string> MitigationStrategiesValue = new();

         public List<string> MitigationStrategies

         {

             get { return this.MitigationStrategiesValue; }

             set { SetProperty(ref MitigationStrategiesValue, value); }

         }
     }
}
