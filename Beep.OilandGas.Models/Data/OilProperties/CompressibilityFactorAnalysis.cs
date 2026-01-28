using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
     public class CompressibilityFactorAnalysis : ModelEntityBase
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
         private decimal CompressibilityFactorValue;

         public decimal CompressibilityFactor

         {

             get { return this.CompressibilityFactorValue; }

             set { SetProperty(ref CompressibilityFactorValue, value); }

         }
         private decimal ReducedPressureValue;

         public decimal ReducedPressure

         {

             get { return this.ReducedPressureValue; }

             set { SetProperty(ref ReducedPressureValue, value); }

         }
         private decimal ReducedTemperatureValue;

         public decimal ReducedTemperature

         {

             get { return this.ReducedTemperatureValue; }

             set { SetProperty(ref ReducedTemperatureValue, value); }

         }
         private string CorrelationMethodValue = string.Empty;

         public string CorrelationMethod

         {

             get { return this.CorrelationMethodValue; }

             set { SetProperty(ref CorrelationMethodValue, value); }

         }
         private decimal DeviationFromIdealValue;

         public decimal DeviationFromIdeal

         {

             get { return this.DeviationFromIdealValue; }

             set { SetProperty(ref DeviationFromIdealValue, value); }

         }
     }
}
