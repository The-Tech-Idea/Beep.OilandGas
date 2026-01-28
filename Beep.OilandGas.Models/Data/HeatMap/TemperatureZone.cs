using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.HeatMap;

namespace Beep.OilandGas.Models.Data
{
     public class TemperatureZone : ModelEntityBase
     {
         private string ZoneIdValue = string.Empty;

         public string ZoneId

         {

             get { return this.ZoneIdValue; }

             set { SetProperty(ref ZoneIdValue, value); }

         }
         private string LocationIdValue = string.Empty;

         public string LocationId

         {

             get { return this.LocationIdValue; }

             set { SetProperty(ref LocationIdValue, value); }

         }
         private DateTime IdentificationDateValue;

         public DateTime IdentificationDate

         {

             get { return this.IdentificationDateValue; }

             set { SetProperty(ref IdentificationDateValue, value); }

         }
         private decimal MinTemperatureValue;

         public decimal MinTemperature

         {

             get { return this.MinTemperatureValue; }

             set { SetProperty(ref MinTemperatureValue, value); }

         }
         private decimal MaxTemperatureValue;

         public decimal MaxTemperature

         {

             get { return this.MaxTemperatureValue; }

             set { SetProperty(ref MaxTemperatureValue, value); }

         }
         private decimal AverageTemperatureValue;

         public decimal AverageTemperature

         {

             get { return this.AverageTemperatureValue; }

             set { SetProperty(ref AverageTemperatureValue, value); }

         }
         private string ZoneClassificationValue = string.Empty;

         public string ZoneClassification

         {

             get { return this.ZoneClassificationValue; }

             set { SetProperty(ref ZoneClassificationValue, value); }

         } // Hot Zone, Normal Zone, Cold Zone
         private decimal AreaValue;

         public decimal Area

         {

             get { return this.AreaValue; }

             set { SetProperty(ref AreaValue, value); }

         }
         private int PointCountValue;

         public int PointCount

         {

             get { return this.PointCountValue; }

             set { SetProperty(ref PointCountValue, value); }

         }
         private List<decimal> BoundaryCoordinatesValue = new();

         public List<decimal> BoundaryCoordinates

         {

             get { return this.BoundaryCoordinatesValue; }

             set { SetProperty(ref BoundaryCoordinatesValue, value); }

         }
     }
}
