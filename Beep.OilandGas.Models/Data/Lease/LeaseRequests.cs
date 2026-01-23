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

    public class CreateFeeMineralLeaseRequest : CreateLease 
    {
    }

    public class CreateGovernmentLeaseRequest : CreateLease 
    {
    }

    public class LeaseRenewalRequest : ModelEntityBase 
    {
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private DateTime NewExpirationDateValue;

        public DateTime NewExpirationDate

        {

            get { return this.NewExpirationDateValue; }

            set { SetProperty(ref NewExpirationDateValue, value); }

        }
        private string RenewalTermsValue = string.Empty;

        public string RenewalTerms

        {

            get { return this.RenewalTermsValue; }

            set { SetProperty(ref RenewalTermsValue, value); }

        }
    }

    public class LeaseRenewalResult : ModelEntityBase 
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private DateTime NewExpirationDateValue;

        public DateTime NewExpirationDate

        {

            get { return this.NewExpirationDateValue; }

            set { SetProperty(ref NewExpirationDateValue, value); }

        }
    }

    public class LeaseExpirationAlert : ModelEntityBase 
    {
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private DateTime ExpirationDateValue;

        public DateTime ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }
        private int DaysUntilExpirationValue;

        public int DaysUntilExpiration

        {

            get { return this.DaysUntilExpirationValue; }

            set { SetProperty(ref DaysUntilExpirationValue, value); }

        }
        private string AlertMessageValue = string.Empty;

        public string AlertMessage

        {

            get { return this.AlertMessageValue; }

            set { SetProperty(ref AlertMessageValue, value); }

        }
    }
}



