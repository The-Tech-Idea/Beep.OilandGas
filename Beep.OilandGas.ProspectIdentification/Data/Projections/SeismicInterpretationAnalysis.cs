using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class SeismicInterpretationAnalysis : ModelEntityBase
     {
         private string AnalysisIdValue = string.Empty;

         public string AnalysisId

         {

             get { return this.AnalysisIdValue; }

             set { SetProperty(ref AnalysisIdValue, value); }

         }
         private string ProspectIdValue = string.Empty;

         public string ProspectId

         {

             get { return this.ProspectIdValue; }

             set { SetProperty(ref ProspectIdValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private string SurveyIdValue = string.Empty;

         public string SurveyId

         {

             get { return this.SurveyIdValue; }

             set { SetProperty(ref SurveyIdValue, value); }

         }
         private int HorizonCountValue;

         public int HorizonCount

         {

             get { return this.HorizonCountValue; }

             set { SetProperty(ref HorizonCountValue, value); }

         }
         private int FaultCountValue;

         public int FaultCount

         {

             get { return this.FaultCountValue; }

             set { SetProperty(ref FaultCountValue, value); }

         }
         private decimal InterpretationConfidenceValue;

         public decimal InterpretationConfidence

         {

             get { return this.InterpretationConfidenceValue; }

             set { SetProperty(ref InterpretationConfidenceValue, value); }

         }
         private string InterpretationStatusValue = string.Empty;

         public string InterpretationStatus

         {

             get { return this.InterpretationStatusValue; }

             set { SetProperty(ref InterpretationStatusValue, value); }

         }

         // Projection-only: populated by service, not persisted to DB
         public List<Horizon> Horizons { get; set; } = new();
         public List<Fault> Faults { get; set; } = new();
     }
}
