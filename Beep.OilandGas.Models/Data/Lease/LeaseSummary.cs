using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    public class LeaseSummary : ModelEntityBase 
    {
       private string LeaseIdValue = string.Empty;

       public string LeaseId

       {

           get { return this.LeaseIdValue; }

           set { SetProperty(ref LeaseIdValue, value); }

       }
       private string LeaseNumberValue = string.Empty;

       public string LeaseNumber

       {

           get { return this.LeaseNumberValue; }

           set { SetProperty(ref LeaseNumberValue, value); }

       }
       private string LeaseNameValue = string.Empty;

       public string LeaseName

       {

           get { return this.LeaseNameValue; }

           set { SetProperty(ref LeaseNameValue, value); }

       }
       private string StatusValue = string.Empty;

       public string Status

       {

           get { return this.StatusValue; }

           set { SetProperty(ref StatusValue, value); }

       }
    }
}
