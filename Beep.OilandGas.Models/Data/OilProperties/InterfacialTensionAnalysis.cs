using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
     public class InterfacialTensionAnalysis : ModelEntityBase
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
         private decimal PressureValue;

         public decimal Pressure

         {

             get { return this.PressureValue; }

             set { SetProperty(ref PressureValue, value); }

         }
         private decimal TemperatureValue;

         public decimal Temperature

         {

             get { return this.TemperatureValue; }

             set { SetProperty(ref TemperatureValue, value); }

         }
         private decimal InterfacialTensionValue;

         public decimal InterfacialTension

         {

             get { return this.InterfacialTensionValue; }

             set { SetProperty(ref InterfacialTensionValue, value); }

         } // dyne/cm
         private string Phase1Value = string.Empty;

         public string Phase1

         {

             get { return this.Phase1Value; }

             set { SetProperty(ref Phase1Value, value); }

         }
         private string Phase2Value = string.Empty;

         public string Phase2

         {

             get { return this.Phase2Value; }

             set { SetProperty(ref Phase2Value, value); }

         }
         private decimal TemperatureDependenceValue;

         public decimal TemperatureDependence

         {

             get { return this.TemperatureDependenceValue; }

             set { SetProperty(ref TemperatureDependenceValue, value); }

         }
     }
}
