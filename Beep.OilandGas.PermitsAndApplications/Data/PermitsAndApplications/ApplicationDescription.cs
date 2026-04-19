using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public class ApplicationDescription : ModelEntityBase
    {
        private string IdValue = string.Empty;
        public string Id
        {
            get { return IdValue; }
            set { SetProperty(ref IdValue, value); }
        }

        private string ApplicationIdValue = string.Empty;
        public string ApplicationId
        {
            get { return ApplicationIdValue; }
            set { SetProperty(ref ApplicationIdValue, value); }
        }

        private string DescriptionTypeValue = string.Empty;
        public string DescriptionType
        {
            get { return DescriptionTypeValue; }
            set { SetProperty(ref DescriptionTypeValue, value); }
        }

        private string DescriptionValue = string.Empty;
        public string Description
        {
            get { return DescriptionValue; }
            set { SetProperty(ref DescriptionValue, value); }
        }

        private DateTime? EffectiveDateValue;
        public DateTime? EffectiveDate
        {
            get { return EffectiveDateValue; }
            set { SetProperty(ref EffectiveDateValue, value); }
        }

        private DateTime? ExpiryDateValue;
        public DateTime? ExpiryDate
        {
            get { return ExpiryDateValue; }
            set { SetProperty(ref ExpiryDateValue, value); }
        }
    }
}
