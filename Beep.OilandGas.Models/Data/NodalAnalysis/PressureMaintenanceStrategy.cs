using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.Models.Data
{
     public class PressureMaintenanceStrategy : ModelEntityBase
     {
         private string StrategyIdValue = string.Empty;

         public string StrategyId

         {

             get { return this.StrategyIdValue; }

             set { SetProperty(ref StrategyIdValue, value); }

         }
         private string WellUWIValue = string.Empty;

         public string WellUWI

         {

             get { return this.WellUWIValue; }

             set { SetProperty(ref WellUWIValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private decimal CurrentReservoirPressureValue;

         public decimal CurrentReservoirPressure

         {

             get { return this.CurrentReservoirPressureValue; }

             set { SetProperty(ref CurrentReservoirPressureValue, value); }

         }
         private decimal MarginToBubblePointValue;

         public decimal MarginToBubblePoint

         {

             get { return this.MarginToBubblePointValue; }

             set { SetProperty(ref MarginToBubblePointValue, value); }

         }
         private string RecommendedStrategyValue = string.Empty;

         public string RecommendedStrategy

         {

             get { return this.RecommendedStrategyValue; }

             set { SetProperty(ref RecommendedStrategyValue, value); }

         }
         private decimal InjectionVolumeRequiredValue;

         public decimal InjectionVolumeRequired

         {

             get { return this.InjectionVolumeRequiredValue; }

             set { SetProperty(ref InjectionVolumeRequiredValue, value); }

         }
     }
}
