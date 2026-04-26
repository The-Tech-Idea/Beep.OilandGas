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

       private string LeaseTypeValue = string.Empty;

       public string LeaseType

       {

           get { return this.LeaseTypeValue; }

           set { SetProperty(ref LeaseTypeValue, value); }

       }

       private string FieldIdValue = string.Empty;

       public string FieldId

       {

           get { return this.FieldIdValue; }

           set { SetProperty(ref FieldIdValue, value); }

       }

       private DateTime? EffectiveDateValue;

       public DateTime? EffectiveDate

       {

           get { return this.EffectiveDateValue; }

           set { SetProperty(ref EffectiveDateValue, value); }

       }

       private DateTime? ExpirationDateValue;

       public DateTime? ExpirationDate

       {

           get { return this.ExpirationDateValue; }

           set { SetProperty(ref ExpirationDateValue, value); }

       }

       private int? PrimaryTermMonthsValue;

       public int? PrimaryTermMonths

       {

           get { return this.PrimaryTermMonthsValue; }

           set { SetProperty(ref PrimaryTermMonthsValue, value); }

       }

       private decimal WorkingInterestValue;

       public decimal WorkingInterest

       {

           get { return this.WorkingInterestValue; }

           set { SetProperty(ref WorkingInterestValue, value); }

       }
    }
}
