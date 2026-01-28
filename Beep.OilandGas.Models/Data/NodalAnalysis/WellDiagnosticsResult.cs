using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.Models.Data
{
     public class WellDiagnosticsResult : ModelEntityBase
     {
         private string DiagnosisIdValue = string.Empty;

         public string DiagnosisId

         {

             get { return this.DiagnosisIdValue; }

             set { SetProperty(ref DiagnosisIdValue, value); }

         }
         private string WellUWIValue = string.Empty;

         public string WellUWI

         {

             get { return this.WellUWIValue; }

             set { SetProperty(ref WellUWIValue, value); }

         }
         private DateTime DiagnosisDateValue;

         public DateTime DiagnosisDate

         {

             get { return this.DiagnosisDateValue; }

             set { SetProperty(ref DiagnosisDateValue, value); }

         }
         private decimal ProductionShortfallValue;

         public decimal ProductionShortfall

         {

             get { return this.ProductionShortfallValue; }

             set { SetProperty(ref ProductionShortfallValue, value); }

         }
         private decimal ProductionShortfallPercentValue;

         public decimal ProductionShortfallPercent

         {

             get { return this.ProductionShortfallPercentValue; }

             set { SetProperty(ref ProductionShortfallPercentValue, value); }

         }
         private List<string> IdentifiedIssuesValue = new();

         public List<string> IdentifiedIssues

         {

             get { return this.IdentifiedIssuesValue; }

             set { SetProperty(ref IdentifiedIssuesValue, value); }

         }
         private List<string> RecommendedActionsValue = new();

         public List<string> RecommendedActions

         {

             get { return this.RecommendedActionsValue; }

             set { SetProperty(ref RecommendedActionsValue, value); }

         }
         private string DiagnosisStatusValue = "Analysis Required";

         public string DiagnosisStatus

         {

             get { return this.DiagnosisStatusValue; }

             set { SetProperty(ref DiagnosisStatusValue, value); }

         }
     }
}
