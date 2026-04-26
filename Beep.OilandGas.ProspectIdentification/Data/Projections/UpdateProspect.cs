using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class UpdateProspect : ModelEntityBase
     {
         private string? ProspectNameValue;

         public string? ProspectName

         {

             get { return this.ProspectNameValue; }

             set { SetProperty(ref ProspectNameValue, value); }

         }
         private string? DescriptionValue;

         public string? Description

         {

             get { return this.DescriptionValue; }

             set { SetProperty(ref DescriptionValue, value); }

         }
         private string? StatusValue;

         public string? Status

         {

             get { return this.StatusValue; }

             set { SetProperty(ref StatusValue, value); }

         }
         private decimal? EstimatedResourcesValue;

         public decimal? EstimatedResources

         {

             get { return this.EstimatedResourcesValue; }

             set { SetProperty(ref EstimatedResourcesValue, value); }

         }
         private string? ResourceUnitValue;

         public string? ResourceUnit

         {

             get { return this.ResourceUnitValue; }

             set { SetProperty(ref ResourceUnitValue, value); }

         }
     }
}
