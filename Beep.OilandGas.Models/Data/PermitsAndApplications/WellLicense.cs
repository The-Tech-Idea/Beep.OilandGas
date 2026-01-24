using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public class WellLicense : ModelEntityBase
    {
        private string LicenseIdValue = string.Empty;

        public string LicenseId

        {

            get { return this.LicenseIdValue; }

            set { SetProperty(ref LicenseIdValue, value); }

        }
        private string? ApplicationIdValue;

        public string? ApplicationId

        {

            get { return this.ApplicationIdValue; }

            set { SetProperty(ref ApplicationIdValue, value); }

        }
        private string? LicenseTypeValue;

        public string? LicenseType

        {

            get { return this.LicenseTypeValue; }

            set { SetProperty(ref LicenseTypeValue, value); }

        }
        private string? UwiValue;

        public string? Uwi

        {

            get { return this.UwiValue; }

            set { SetProperty(ref UwiValue, value); }

        }
        private DateTime? GrantedDateValue;

        public DateTime? GrantedDate

        {

            get { return this.GrantedDateValue; }

            set { SetProperty(ref GrantedDateValue, value); }

        }
        private DateTime? ExpiryDateValue;

        public DateTime? ExpiryDate

        {

            get { return this.ExpiryDateValue; }

            set { SetProperty(ref ExpiryDateValue, value); }

        }
    }
}
