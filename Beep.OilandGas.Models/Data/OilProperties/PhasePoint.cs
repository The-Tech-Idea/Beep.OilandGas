using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
     public class PhasePoint : ModelEntityBase
     {
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
         private string PhaseValue = string.Empty;

         public string Phase

         {

             get { return this.PhaseValue; }

             set { SetProperty(ref PhaseValue, value); }

         } // Gas, Oil, Two-Phase
         private decimal DensityValue;

         public decimal Density

         {

             get { return this.DensityValue; }

             set { SetProperty(ref DensityValue, value); }

         }
     }
}
