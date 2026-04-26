using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public class WellPermitTypeInfo : ModelEntityBase
    {
        private string PermitTypeValue = string.Empty;
        public string PermitType
        {
            get { return PermitTypeValue; }
            set { SetProperty(ref PermitTypeValue, value); }
        }

        private string AbbreviationValue = string.Empty;
        public string Abbreviation
        {
            get { return AbbreviationValue; }
            set { SetProperty(ref AbbreviationValue, value); }
        }

        private string LongNameValue = string.Empty;
        public string LongName
        {
            get { return LongNameValue; }
            set { SetProperty(ref LongNameValue, value); }
        }

        private string ShortNameValue = string.Empty;
        public string ShortName
        {
            get { return ShortNameValue; }
            set { SetProperty(ref ShortNameValue, value); }
        }

        private string GrantedByBaIdValue = string.Empty;
        public string GrantedByBaId
        {
            get { return GrantedByBaIdValue; }
            set { SetProperty(ref GrantedByBaIdValue, value); }
        }

        private string RateScheduleIdValue = string.Empty;
        public string RateScheduleId
        {
            get { return RateScheduleIdValue; }
            set { SetProperty(ref RateScheduleIdValue, value); }
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
