using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public class ApplicationBusinessAssociate : ModelEntityBase
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

        private string BusinessAssociateIdValue = string.Empty;
        public string BusinessAssociateId
        {
            get { return BusinessAssociateIdValue; }
            set { SetProperty(ref BusinessAssociateIdValue, value); }
        }

        private string RoleValue = string.Empty;
        public string Role
        {
            get { return RoleValue; }
            set { SetProperty(ref RoleValue, value); }
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

        private string RemarksValue = string.Empty;
        public string Remarks
        {
            get { return RemarksValue; }
            set { SetProperty(ref RemarksValue, value); }
        }
    }
}
