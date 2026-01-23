using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class Horizon : ModelEntityBase
     {
         private string HorizonIdValue = string.Empty;

         public string HorizonId

         {

             get { return this.HorizonIdValue; }

             set { SetProperty(ref HorizonIdValue, value); }

         }
         private string HorizonNameValue = string.Empty;

         public string HorizonName

         {

             get { return this.HorizonNameValue; }

             set { SetProperty(ref HorizonNameValue, value); }

         }
         private string GeologicalAgeValue = string.Empty;

         public string GeologicalAge

         {

             get { return this.GeologicalAgeValue; }

             set { SetProperty(ref GeologicalAgeValue, value); }

         }
         private decimal DepthValue;

         public decimal Depth

         {

             get { return this.DepthValue; }

             set { SetProperty(ref DepthValue, value); }

         }
         private decimal ThicknessValue;

         public decimal Thickness

         {

             get { return this.ThicknessValue; }

             set { SetProperty(ref ThicknessValue, value); }

         }
         private string LithologyTypeValue = string.Empty;

         public string LithologyType

         {

             get { return this.LithologyTypeValue; }

             set { SetProperty(ref LithologyTypeValue, value); }

         }
         private string ReservoirQualityValue = string.Empty;

         public string ReservoirQuality

         {

             get { return this.ReservoirQualityValue; }

             set { SetProperty(ref ReservoirQualityValue, value); }

         }
     }
}
