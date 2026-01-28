using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
     public class PhaseDiagramAnalysis : ModelEntityBase
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
         private decimal CriticalTemperatureValue;

         public decimal CriticalTemperature

         {

             get { return this.CriticalTemperatureValue; }

             set { SetProperty(ref CriticalTemperatureValue, value); }

         }
         private decimal CriticalPressureValue;

         public decimal CriticalPressure

         {

             get { return this.CriticalPressureValue; }

             set { SetProperty(ref CriticalPressureValue, value); }

         }
         private decimal CriticalDensityValue;

         public decimal CriticalDensity

         {

             get { return this.CriticalDensityValue; }

             set { SetProperty(ref CriticalDensityValue, value); }

         }
         private decimal TriplePointTemperatureValue;

         public decimal TriplePointTemperature

         {

             get { return this.TriplePointTemperatureValue; }

             set { SetProperty(ref TriplePointTemperatureValue, value); }

         }
         private decimal TriplePointPressureValue;

         public decimal TriplePointPressure

         {

             get { return this.TriplePointPressureValue; }

             set { SetProperty(ref TriplePointPressureValue, value); }

         }
         private List<PhasePoint> PhasePointsValue = new();

         public List<PhasePoint> PhasePoints

         {

             get { return this.PhasePointsValue; }

             set { SetProperty(ref PhasePointsValue, value); }

         }
         private string PhaseValue = string.Empty;

         public string Phase

         {

             get { return this.PhaseValue; }

             set { SetProperty(ref PhaseValue, value); }

         } // Single Phase, Two-Phase, Three-Phase
     }
}
