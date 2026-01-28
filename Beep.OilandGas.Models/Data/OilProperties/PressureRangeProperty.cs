using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
     public class PressureRangeProperty : ModelEntityBase
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
         private decimal ViscosityValue;

         public decimal Viscosity

         {

             get { return this.ViscosityValue; }

             set { SetProperty(ref ViscosityValue, value); }

         }
         private decimal DensityValue;

         public decimal Density

         {

             get { return this.DensityValue; }

             set { SetProperty(ref DensityValue, value); }

         }
         private decimal FormationVolumeFactorValue;

         public decimal FormationVolumeFactor

         {

             get { return this.FormationVolumeFactorValue; }

             set { SetProperty(ref FormationVolumeFactorValue, value); }

         }
         private decimal CompressibilityValue;

         public decimal Compressibility

         {

             get { return this.CompressibilityValue; }

             set { SetProperty(ref CompressibilityValue, value); }

         }
     }
}
