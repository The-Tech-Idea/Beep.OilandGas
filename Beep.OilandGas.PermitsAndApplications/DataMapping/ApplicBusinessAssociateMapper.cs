using System;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.DataMapping
{
    public class ApplicBusinessAssociateMapper
    {
        public ApplicationBusinessAssociate MapToDomain(APPLIC_BA data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return new ApplicationBusinessAssociate
            {
                Id = data.APPLIC_BA_ID ?? string.Empty,
                ApplicationId = data.PERMIT_APPLICATION_ID ?? string.Empty,
                BusinessAssociateId = data.BUSINESS_ASSOCIATE_ID ?? string.Empty,
                Role = data.BA_ROLE ?? string.Empty,
                EffectiveDate = data.EFFECTIVE_DATE,
                ExpiryDate = data.EXPIRY_DATE,
                Remarks = data.REMARKS ?? string.Empty
            };
        }

        public APPLIC_BA MapToData(ApplicationBusinessAssociate model, APPLIC_BA? existing = null)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var data = existing ?? new APPLIC_BA();

            data.APPLIC_BA_ID = model.Id;
            data.PERMIT_APPLICATION_ID = model.ApplicationId;
            data.BUSINESS_ASSOCIATE_ID = model.BusinessAssociateId;
            data.BA_ROLE = model.Role;
            data.EFFECTIVE_DATE = model.EffectiveDate;
            data.EXPIRY_DATE = model.ExpiryDate;
            data.REMARKS = model.Remarks;

            return data;
        }
    }
}
