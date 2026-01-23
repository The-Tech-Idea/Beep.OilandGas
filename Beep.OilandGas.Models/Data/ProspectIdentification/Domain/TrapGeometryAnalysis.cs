using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class TrapGeometryAnalysis : ModelEntityBase
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
         private string TrapTypeValue = string.Empty;

         public string TrapType

         {

             get { return this.TrapTypeValue; }

             set { SetProperty(ref TrapTypeValue, value); }

         } // Structural, Stratigraphic, Combination
         private decimal ClosureValue;

         public decimal Closure

         {

             get { return this.ClosureValue; }

             set { SetProperty(ref ClosureValue, value); }

         }
         private decimal CrestDepthValue;

         public decimal CrestDepth

         {

             get { return this.CrestDepthValue; }

             set { SetProperty(ref CrestDepthValue, value); }

         }
         private decimal SpillPointDepthValue;

         public decimal SpillPointDepth

         {

             get { return this.SpillPointDepthValue; }

             set { SetProperty(ref SpillPointDepthValue, value); }

         }
         private decimal AreaValue;

         public decimal Area

         {

             get { return this.AreaValue; }

             set { SetProperty(ref AreaValue, value); }

         }
         private string AreaUnitValue = string.Empty;

         public string AreaUnit

         {

             get { return this.AreaUnitValue; }

             set { SetProperty(ref AreaUnitValue, value); }

         }
         private decimal VolumeValue;

         public decimal Volume

         {

             get { return this.VolumeValue; }

             set { SetProperty(ref VolumeValue, value); }

         }
         private string VolumeUnitValue = string.Empty;

         public string VolumeUnit

         {

             get { return this.VolumeUnitValue; }

             set { SetProperty(ref VolumeUnitValue, value); }

         }
         private string TrapGeometryValue = string.Empty;

         public string TrapGeometry

         {

             get { return this.TrapGeometryValue; }

             set { SetProperty(ref TrapGeometryValue, value); }

         }
         private string SourceRockProximityValue = string.Empty;

         public string SourceRockProximity

         {

             get { return this.SourceRockProximityValue; }

             set { SetProperty(ref SourceRockProximityValue, value); }

         }
     }
}
