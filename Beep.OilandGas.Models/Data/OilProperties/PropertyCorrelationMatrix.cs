using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
     public class PropertyCorrelationMatrix : ModelEntityBase
     {
         private string MatrixIdValue = string.Empty;

         public string MatrixId

         {

             get { return this.MatrixIdValue; }

             set { SetProperty(ref MatrixIdValue, value); }

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
         private List<PressureRangeProperty> PropertyByPressureValue = new();

         public List<PressureRangeProperty> PropertyByPressure

         {

             get { return this.PropertyByPressureValue; }

             set { SetProperty(ref PropertyByPressureValue, value); }

         }
         private List<TemperatureRangeProperty> PropertyByTemperatureValue = new();

         public List<TemperatureRangeProperty> PropertyByTemperature

         {

             get { return this.PropertyByTemperatureValue; }

             set { SetProperty(ref PropertyByTemperatureValue, value); }

         }
         public Dictionary<string, decimal> CorrelationCoefficients { get; set; } = new();
     }
}
