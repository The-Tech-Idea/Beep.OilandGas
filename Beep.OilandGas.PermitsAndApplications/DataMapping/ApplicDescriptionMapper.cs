using System;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.DataMapping
{
    public class ApplicDescriptionMapper
    {
        public ApplicationDescription MapToDomain(APPLIC_DESC data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return new ApplicationDescription
            {
                Id = data.DESCRIPTION_ID ?? string.Empty,
                ApplicationId = data.APPLICATION_ID ?? string.Empty,
                DescriptionType = data.APPLIC_DESC_TYPE ?? string.Empty,
                Description = data.DESCRIPTION ?? string.Empty,
                EffectiveDate = data.EFFECTIVE_DATE,
                ExpiryDate = data.EXPIRY_DATE
            };
        }

        public APPLIC_DESC MapToData(ApplicationDescription model, APPLIC_DESC? existing = null)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var data = existing ?? new APPLIC_DESC();

            data.DESCRIPTION_ID = model.Id;
            data.APPLICATION_ID = model.ApplicationId;
            data.APPLIC_DESC_TYPE = model.DescriptionType;
            data.DESCRIPTION = model.Description;
            data.EFFECTIVE_DATE = model.EffectiveDate;
            data.EXPIRY_DATE = model.ExpiryDate;

            return data;
        }
    }
}
