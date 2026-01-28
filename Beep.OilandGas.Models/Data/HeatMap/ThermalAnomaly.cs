using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.HeatMap;

namespace Beep.OilandGas.Models.Data
{
     public class ThermalAnomaly : ModelEntityBase
     {
         private string AnomalyIdValue = string.Empty;

         public string AnomalyId

         {

             get { return this.AnomalyIdValue; }

             set { SetProperty(ref AnomalyIdValue, value); }

         }
         private string LocationIdValue = string.Empty;

         public string LocationId

         {

             get { return this.LocationIdValue; }

             set { SetProperty(ref LocationIdValue, value); }

         }
         private DateTime DetectionDateValue;

         public DateTime DetectionDate

         {

             get { return this.DetectionDateValue; }

             set { SetProperty(ref DetectionDateValue, value); }

         }
         private decimal AnomalyTemperatureValue;

         public decimal AnomalyTemperature

         {

             get { return this.AnomalyTemperatureValue; }

             set { SetProperty(ref AnomalyTemperatureValue, value); }

         }
         private decimal ExpectedTemperatureValue;

         public decimal ExpectedTemperature

         {

             get { return this.ExpectedTemperatureValue; }

             set { SetProperty(ref ExpectedTemperatureValue, value); }

         }
         private decimal TemperatureDeviationValue;

         public decimal TemperatureDeviation

         {

             get { return this.TemperatureDeviationValue; }

             set { SetProperty(ref TemperatureDeviationValue, value); }

         }
         private decimal DeviationPercentValue;

         public decimal DeviationPercent

         {

             get { return this.DeviationPercentValue; }

             set { SetProperty(ref DeviationPercentValue, value); }

         }
         private string AnomalyTypeValue = string.Empty;

         public string AnomalyType

         {

             get { return this.AnomalyTypeValue; }

             set { SetProperty(ref AnomalyTypeValue, value); }

         } // Hot Anomaly, Cold Anomaly, Gradient Anomaly
         private string SeverityValue = string.Empty;

         public string Severity

         {

             get { return this.SeverityValue; }

             set { SetProperty(ref SeverityValue, value); }

         } // Low, Medium, High, Critical
         private decimal XValue;

         public decimal X

         {

             get { return this.XValue; }

             set { SetProperty(ref XValue, value); }

         }
         private decimal YValue;

         public decimal Y

         {

             get { return this.YValue; }

             set { SetProperty(ref YValue, value); }

         }
         private string DescriptionValue = string.Empty;

         public string Description

         {

             get { return this.DescriptionValue; }

             set { SetProperty(ref DescriptionValue, value); }

         }
         private List<string> RecommendedActionsValue = new();

         public List<string> RecommendedActions

         {

             get { return this.RecommendedActionsValue; }

             set { SetProperty(ref RecommendedActionsValue, value); }

         }
     }
}
