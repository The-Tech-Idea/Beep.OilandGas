using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public class ApplicationRemark : ModelEntityBase
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

        private string RemarkTypeValue = string.Empty;
        public string RemarkType
        {
            get { return RemarkTypeValue; }
            set { SetProperty(ref RemarkTypeValue, value); }
        }

        private string RemarkTextValue = string.Empty;
        public string RemarkText
        {
            get { return RemarkTextValue; }
            set { SetProperty(ref RemarkTextValue, value); }
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
