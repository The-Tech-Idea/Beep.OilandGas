using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class SealSourceAssessment : ModelEntityBase
     {
         private string AssessmentIdValue = string.Empty;

         public string AssessmentId

         {

             get { return this.AssessmentIdValue; }

             set { SetProperty(ref AssessmentIdValue, value); }

         }
         private string ProspectIdValue = string.Empty;

         public string ProspectId

         {

             get { return this.ProspectIdValue; }

             set { SetProperty(ref ProspectIdValue, value); }

         }
         private DateTime AssessmentDateValue;

         public DateTime AssessmentDate

         {

             get { return this.AssessmentDateValue; }

             set { SetProperty(ref AssessmentDateValue, value); }

         }
         private string SealRockTypeValue = string.Empty;

         public string SealRockType

         {

             get { return this.SealRockTypeValue; }

             set { SetProperty(ref SealRockTypeValue, value); }

         }
         private decimal SealRockThicknessValue;

         public decimal SealRockThickness

         {

             get { return this.SealRockThicknessValue; }

             set { SetProperty(ref SealRockThicknessValue, value); }

         }
         private string SealQualityValue = string.Empty;

         public string SealQuality

         {

             get { return this.SealQualityValue; }

             set { SetProperty(ref SealQualityValue, value); }

         }
         private decimal SealIntegrityScoreValue;

         public decimal SealIntegrityScore

         {

             get { return this.SealIntegrityScoreValue; }

             set { SetProperty(ref SealIntegrityScoreValue, value); }

         }
         private string SourceRockTypeValue = string.Empty;

         public string SourceRockType

         {

             get { return this.SourceRockTypeValue; }

             set { SetProperty(ref SourceRockTypeValue, value); }

         }
         private decimal SourceRockMaturityValue;

         public decimal SourceRockMaturity

         {

             get { return this.SourceRockMaturityValue; }

             set { SetProperty(ref SourceRockMaturityValue, value); }

         }
         private string GenerationStatusValue = string.Empty;

         public string GenerationStatus

         {

             get { return this.GenerationStatusValue; }

             set { SetProperty(ref GenerationStatusValue, value); }

         }
         private decimal SourceRockProductivityValue;

         public decimal SourceRockProductivity

         {

             get { return this.SourceRockProductivityValue; }

             set { SetProperty(ref SourceRockProductivityValue, value); }

         }
         private string SystemStatusValue = string.Empty;

         public string SystemStatus

         {

             get { return this.SystemStatusValue; }

             set { SetProperty(ref SystemStatusValue, value); }

         } // Active, Inactive, Marginal
     }
}
