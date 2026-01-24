using System;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.PermitsAndApplications.DataMapping
{
    /// <summary>
    /// Maps between PPDM39 WELL_LICENSE and WellLicense domain model.
    /// </summary>
    public class WellLicenseMapper
    {
        public WellLicense MapToDomain(WELL_LICENSE ppdmLicense)
        {
            if (ppdmLicense == null)
                throw new ArgumentNullException(nameof(ppdmLicense));

            return new WellLicense
            {
                LicenseId = ppdmLicense.LICENSE_ID ?? string.Empty,
                ApplicationId = ppdmLicense.APPLICATION_ID,
                Uwi = ppdmLicense.UWI,
                GrantedDate = ppdmLicense.LICENSE_DATE ?? ppdmLicense.EFFECTIVE_DATE,
                ExpiryDate = ppdmLicense.EXPIRY_DATE,
                LicenseType = ExtractLicenseType(ppdmLicense.REMARK)
            };
        }

        public WELL_LICENSE MapToPPDM39(WellLicense license, WELL_LICENSE? existing = null)
        {
            if (license == null)
                throw new ArgumentNullException(nameof(license));

            var ppdm = existing ?? new WELL_LICENSE();

            ppdm.LICENSE_ID = license.LicenseId;
            ppdm.APPLICATION_ID = license.ApplicationId;
            ppdm.UWI = license.Uwi;
            ppdm.LICENSE_DATE = license.GrantedDate;
            ppdm.EFFECTIVE_DATE = license.GrantedDate;
            ppdm.EXPIRY_DATE = license.ExpiryDate;

            if (!string.IsNullOrWhiteSpace(license.LicenseType))
            {
                ppdm.REMARK = string.IsNullOrWhiteSpace(ppdm.REMARK)
                    ? $"LicenseType:{license.LicenseType}"
                    : ppdm.REMARK;
            }

            return ppdm;
        }

        private string? ExtractLicenseType(string? remark)
        {
            if (string.IsNullOrWhiteSpace(remark))
                return null;

            const string prefix = "LicenseType:";
            var index = remark.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return null;

            var value = remark.Substring(index + prefix.Length).Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}
