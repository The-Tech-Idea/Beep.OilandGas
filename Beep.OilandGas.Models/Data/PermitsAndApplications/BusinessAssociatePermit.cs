using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public class BusinessAssociatePermit : ModelEntityBase
    {
        private string BusinessAssociateIdValue = string.Empty;
        public string BusinessAssociateId
        {
            get { return BusinessAssociateIdValue; }
            set { SetProperty(ref BusinessAssociateIdValue, value); }
        }

        private string JurisdictionValue = string.Empty;
        public string Jurisdiction
        {
            get { return JurisdictionValue; }
            set { SetProperty(ref JurisdictionValue, value); }
        }

        private string PermitTypeValue = string.Empty;
        public string PermitType
        {
            get { return PermitTypeValue; }
            set { SetProperty(ref PermitTypeValue, value); }
        }

        private decimal? PermitObservationNumberValue;
        public decimal? PermitObservationNumber
        {
            get { return PermitObservationNumberValue; }
            set { SetProperty(ref PermitObservationNumberValue, value); }
        }

        private string PermitNumberValue = string.Empty;
        public string PermitNumber
        {
            get { return PermitNumberValue; }
            set { SetProperty(ref PermitNumberValue, value); }
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
