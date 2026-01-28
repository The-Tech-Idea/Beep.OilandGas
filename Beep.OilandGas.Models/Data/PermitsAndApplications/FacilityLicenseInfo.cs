using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public class FacilityLicenseInfo : ModelEntityBase
    {
        private string FacilityIdValue = string.Empty;
        public string FacilityId
        {
            get { return FacilityIdValue; }
            set { SetProperty(ref FacilityIdValue, value); }
        }

        private string FacilityTypeValue = string.Empty;
        public string FacilityType
        {
            get { return FacilityTypeValue; }
            set { SetProperty(ref FacilityTypeValue, value); }
        }

        private string LicenseIdValue = string.Empty;
        public string LicenseId
        {
            get { return LicenseIdValue; }
            set { SetProperty(ref LicenseIdValue, value); }
        }

        private string LicenseNumberValue = string.Empty;
        public string LicenseNumber
        {
            get { return LicenseNumberValue; }
            set { SetProperty(ref LicenseNumberValue, value); }
        }

        private string LicenseTypeValue = string.Empty;
        public string LicenseType
        {
            get { return LicenseTypeValue; }
            set { SetProperty(ref LicenseTypeValue, value); }
        }

        private DateTime? GrantedDateValue;
        public DateTime? GrantedDate
        {
            get { return GrantedDateValue; }
            set { SetProperty(ref GrantedDateValue, value); }
        }

        private DateTime? ExpiryDateValue;
        public DateTime? ExpiryDate
        {
            get { return ExpiryDateValue; }
            set { SetProperty(ref ExpiryDateValue, value); }
        }

        private string GrantedByBaIdValue = string.Empty;
        public string GrantedByBaId
        {
            get { return GrantedByBaIdValue; }
            set { SetProperty(ref GrantedByBaIdValue, value); }
        }

        private string GrantedToBaIdValue = string.Empty;
        public string GrantedToBaId
        {
            get { return GrantedToBaIdValue; }
            set { SetProperty(ref GrantedToBaIdValue, value); }
        }

        private bool FeesPaidValue;
        public bool FeesPaid
        {
            get { return FeesPaidValue; }
            set { SetProperty(ref FeesPaidValue, value); }
        }

        private bool ViolationValue;
        public bool Violation
        {
            get { return ViolationValue; }
            set { SetProperty(ref ViolationValue, value); }
        }

        private string DescriptionValue = string.Empty;
        public string Description
        {
            get { return DescriptionValue; }
            set { SetProperty(ref DescriptionValue, value); }
        }

        private string LicenseLocationValue = string.Empty;
        public string LicenseLocation
        {
            get { return LicenseLocationValue; }
            set { SetProperty(ref LicenseLocationValue, value); }
        }
    }
}
