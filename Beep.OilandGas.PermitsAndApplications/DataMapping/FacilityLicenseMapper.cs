using System;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.DataMapping
{
    public class FacilityLicenseMapper
    {
        public FacilityLicenseInfo MapToDomain(FACILITY_LICENSE data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return new FacilityLicenseInfo
            {
                FacilityId = data.FACILITY_ID ?? string.Empty,
                FacilityType = data.FACILITY_TYPE ?? string.Empty,
                LicenseId = data.LICENSE_ID ?? string.Empty,
                LicenseNumber = data.LICENSE_NUM ?? string.Empty,
                LicenseType = data.FACILITY_LICENSE_TYPE ?? string.Empty,
                GrantedDate = data.GRANTED_DATE,
                ExpiryDate = data.EXPIRY_DATE,
                GrantedByBaId = data.GRANTED_BY_BA_ID ?? string.Empty,
                GrantedToBaId = data.GRANTED_TO_BA_ID ?? string.Empty,
                FeesPaid = string.Equals(data.FEES_PAID_IND, "Y", StringComparison.OrdinalIgnoreCase),
                Violation = string.Equals(data.VIOLATION_IND, "Y", StringComparison.OrdinalIgnoreCase),
                Description = data.DESCRIPTION ?? string.Empty,
                LicenseLocation = data.LICENSE_LOCATION ?? string.Empty
            };
        }

        public FACILITY_LICENSE MapToData(FacilityLicenseInfo model, FACILITY_LICENSE? existing = null)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var data = existing ?? new FACILITY_LICENSE();

            data.FACILITY_ID = model.FacilityId;
            data.FACILITY_TYPE = model.FacilityType;
            data.LICENSE_ID = model.LicenseId;
            data.LICENSE_NUM = model.LicenseNumber;
            data.FACILITY_LICENSE_TYPE = model.LicenseType;
            data.GRANTED_DATE = model.GrantedDate;
            data.EXPIRY_DATE = model.ExpiryDate;
            data.GRANTED_BY_BA_ID = model.GrantedByBaId;
            data.GRANTED_TO_BA_ID = model.GrantedToBaId;
            data.FEES_PAID_IND = model.FeesPaid ? "Y" : "N";
            data.VIOLATION_IND = model.Violation ? "Y" : "N";
            data.DESCRIPTION = model.Description;
            data.LICENSE_LOCATION = model.LicenseLocation;

            return data;
        }
    }
}
