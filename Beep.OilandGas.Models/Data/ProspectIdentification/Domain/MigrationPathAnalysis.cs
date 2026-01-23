using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class MigrationPathAnalysis : ModelEntityBase
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
         private string SourceRockIdValue = string.Empty;

         public string SourceRockId

         {

             get { return this.SourceRockIdValue; }

             set { SetProperty(ref SourceRockIdValue, value); }

         }
         private decimal SourceRockMaturityLevelValue;

         public decimal SourceRockMaturityLevel

         {

             get { return this.SourceRockMaturityLevelValue; }

             set { SetProperty(ref SourceRockMaturityLevelValue, value); }

         }
         private string MigrationPathwayValue = string.Empty;

         public string MigrationPathway

         {

             get { return this.MigrationPathwayValue; }

             set { SetProperty(ref MigrationPathwayValue, value); }

         }
         private decimal MigrationDistanceValue;

         public decimal MigrationDistance

         {

             get { return this.MigrationDistanceValue; }

             set { SetProperty(ref MigrationDistanceValue, value); }

         }
         private string DistanceUnitValue = string.Empty;

         public string DistanceUnit

         {

             get { return this.DistanceUnitValue; }

             set { SetProperty(ref DistanceUnitValue, value); }

         }
         private decimal MigrationEfficiencyValue;

         public decimal MigrationEfficiency

         {

             get { return this.MigrationEfficiencyValue; }

             set { SetProperty(ref MigrationEfficiencyValue, value); }

         }
         private string SealIntegrityValue = string.Empty;

         public string SealIntegrity

         {

             get { return this.SealIntegrityValue; }

             set { SetProperty(ref SealIntegrityValue, value); }

         }
         private string LateralMigrationRiskValue = string.Empty;

         public string LateralMigrationRisk

         {

             get { return this.LateralMigrationRiskValue; }

             set { SetProperty(ref LateralMigrationRiskValue, value); }

         }
         private List<string> MigrationBarriersValue = new();

         public List<string> MigrationBarriers

         {

             get { return this.MigrationBarriersValue; }

             set { SetProperty(ref MigrationBarriersValue, value); }

         }
     }
}
