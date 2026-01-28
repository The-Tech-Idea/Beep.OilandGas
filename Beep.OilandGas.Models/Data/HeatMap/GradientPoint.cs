using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.HeatMap;

namespace Beep.OilandGas.Models.Data
{
     public class GradientPoint : ModelEntityBase
     {
         private decimal DistanceValue;

         public decimal Distance

         {

             get { return this.DistanceValue; }

             set { SetProperty(ref DistanceValue, value); }

         }
         private decimal TemperatureValue;

         public decimal Temperature

         {

             get { return this.TemperatureValue; }

             set { SetProperty(ref TemperatureValue, value); }

         }
         private decimal LocalGradientValue;

         public decimal LocalGradient

         {

             get { return this.LocalGradientValue; }

             set { SetProperty(ref LocalGradientValue, value); }

         }
     }
}
