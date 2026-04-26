using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class ResourceEstimationResult : ModelEntityBase
     {
         private string EstimationIdValue = string.Empty;

         public string EstimationId

         {

             get { return this.EstimationIdValue; }

             set { SetProperty(ref EstimationIdValue, value); }

         }
         private string ProspectIdValue = string.Empty;

         public string ProspectId

         {

             get { return this.ProspectIdValue; }

             set { SetProperty(ref ProspectIdValue, value); }

         }
         private DateTime EstimationDateValue;

         public DateTime EstimationDate

         {

             get { return this.EstimationDateValue; }

             set { SetProperty(ref EstimationDateValue, value); }

         }
         private string EstimatedByValue = string.Empty;

         public string EstimatedBy

         {

             get { return this.EstimatedByValue; }

             set { SetProperty(ref EstimatedByValue, value); }

         }
         private decimal GrossRockVolumeValue;

         public decimal GrossRockVolume

         {

             get { return this.GrossRockVolumeValue; }

             set { SetProperty(ref GrossRockVolumeValue, value); }

         }
         private decimal NetRockVolumeValue;

         public decimal NetRockVolume

         {

             get { return this.NetRockVolumeValue; }

             set { SetProperty(ref NetRockVolumeValue, value); }

         }
         private decimal PorosityValue;

         public decimal Porosity

         {

             get { return this.PorosityValue; }

             set { SetProperty(ref PorosityValue, value); }

         }
         private decimal WaterSaturationValue;

         public decimal WaterSaturation

         {

             get { return this.WaterSaturationValue; }

             set { SetProperty(ref WaterSaturationValue, value); }

         }
         private decimal OilRecoveryFactorValue;

         public decimal OilRecoveryFactor

         {

             get { return this.OilRecoveryFactorValue; }

             set { SetProperty(ref OilRecoveryFactorValue, value); }

         }
         private decimal GasRecoveryFactorValue;

         public decimal GasRecoveryFactor

         {

             get { return this.GasRecoveryFactorValue; }

             set { SetProperty(ref GasRecoveryFactorValue, value); }

         }
         private decimal EstimatedOilVolumeValue;

         public decimal EstimatedOilVolume

         {

             get { return this.EstimatedOilVolumeValue; }

             set { SetProperty(ref EstimatedOilVolumeValue, value); }

         }
         private decimal EstimatedGasVolumeValue;

         public decimal EstimatedGasVolume

         {

             get { return this.EstimatedGasVolumeValue; }

             set { SetProperty(ref EstimatedGasVolumeValue, value); }

         }
         private string VolumeUnitValue = string.Empty;

         public string VolumeUnit

         {

             get { return this.VolumeUnitValue; }

             set { SetProperty(ref VolumeUnitValue, value); }

         }
         private string EstimationMethodValue = string.Empty;

         public string EstimationMethod

         {

             get { return this.EstimationMethodValue; }

             set { SetProperty(ref EstimationMethodValue, value); }

         }
         private List<string> AssumptionsAndLimitationsValue = new();

         public List<string> AssumptionsAndLimitations

         {

             get { return this.AssumptionsAndLimitationsValue; }

             set { SetProperty(ref AssumptionsAndLimitationsValue, value); }

         }
     }
}
