using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public class ExpiringPermitRecord : ModelEntityBase
    {
        private string PermitApplicationIdValue = string.Empty;
        public string PermitApplicationId
        {
            get { return this.PermitApplicationIdValue; }
            set { SetProperty(ref PermitApplicationIdValue, value); }
        }

        private string? ApplicationTypeValue;
        public string? ApplicationType
        {
            get { return this.ApplicationTypeValue; }
            set { SetProperty(ref ApplicationTypeValue, value); }
        }

        private string? StatusValue;
        public string? Status
        {
            get { return this.StatusValue; }
            set { SetProperty(ref StatusValue, value); }
        }

        private DateTime? ExpiryDateValue;
        public DateTime? ExpiryDate
        {
            get { return this.ExpiryDateValue; }
            set { SetProperty(ref ExpiryDateValue, value); }
        }

        private string? RegulatoryAuthorityValue;
        public string? RegulatoryAuthority
        {
            get { return this.RegulatoryAuthorityValue; }
            set { SetProperty(ref RegulatoryAuthorityValue, value); }
        }
    }
}
