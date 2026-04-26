using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class ProspectRiskAnalysisResult : ModelEntityBase
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
         private string AssessedByValue = string.Empty;

         public string AssessedBy

         {

             get { return this.AssessedByValue; }

             set { SetProperty(ref AssessedByValue, value); }

         }
         private decimal TrapRiskValue;

         public decimal TrapRisk

         {

             get { return this.TrapRiskValue; }

             set { SetProperty(ref TrapRiskValue, value); }

         }
         private decimal SealRiskValue;

         public decimal SealRisk

         {

             get { return this.SealRiskValue; }

             set { SetProperty(ref SealRiskValue, value); }

         }
         private decimal SourceRiskValue;

         public decimal SourceRisk

         {

             get { return this.SourceRiskValue; }

             set { SetProperty(ref SourceRiskValue, value); }

         }
         private decimal MigrationRiskValue;

         public decimal MigrationRisk

         {

             get { return this.MigrationRiskValue; }

             set { SetProperty(ref MigrationRiskValue, value); }

         }
         private decimal CharacterizationRiskValue;

         public decimal CharacterizationRisk

         {

             get { return this.CharacterizationRiskValue; }

             set { SetProperty(ref CharacterizationRiskValue, value); }

         }
         private decimal OverallRiskValue;

         public decimal OverallRisk

         {

             get { return this.OverallRiskValue; }

             set { SetProperty(ref OverallRiskValue, value); }

         }
         private decimal ProbabilityOfSuccessValue;

         public decimal ProbabilityOfSuccess

         {

             get { return this.ProbabilityOfSuccessValue; }

             set { SetProperty(ref ProbabilityOfSuccessValue, value); }

         }
         private string OverallRiskLevelValue = string.Empty;

         public string OverallRiskLevel

         {

             get { return this.OverallRiskLevelValue; }

             set { SetProperty(ref OverallRiskLevelValue, value); }

         }
         private List<RiskCategory> RiskCategoriesValue = new();

         public List<RiskCategory> RiskCategories

         {

             get { return this.RiskCategoriesValue; }

             set { SetProperty(ref RiskCategoriesValue, value); }

         }
     }
}
