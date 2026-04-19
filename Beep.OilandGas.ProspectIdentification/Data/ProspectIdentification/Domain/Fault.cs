using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class Fault : ModelEntityBase
     {
         private string FaultIdValue = string.Empty;

         public string FaultId

         {

             get { return this.FaultIdValue; }

             set { SetProperty(ref FaultIdValue, value); }

         }
         private string FaultNameValue = string.Empty;

         public string FaultName

         {

             get { return this.FaultNameValue; }

             set { SetProperty(ref FaultNameValue, value); }

         }
         private decimal ThrowValue;

         public decimal Throw

         {

             get { return this.ThrowValue; }

             set { SetProperty(ref ThrowValue, value); }

         }
         private string FaultTypeValue = string.Empty;

         public string FaultType

         {

             get { return this.FaultTypeValue; }

             set { SetProperty(ref FaultTypeValue, value); }

         }
         private string SealingPotentialValue = string.Empty;

         public string SealingPotential

         {

             get { return this.SealingPotentialValue; }

             set { SetProperty(ref SealingPotentialValue, value); }

         }
     }
}
