using System;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;

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
                Id = data.APPLICATION_BA_ID ?? string.Empty,
                ApplicationId = data.APPLICATION_ID ?? string.Empty,
                BusinessAssociateId = data.CONTACT_BA_ID ?? string.Empty,
                Role = data.APPLICATION_BA_ROLE ?? string.Empty,
                EffectiveDate = data.EFFECTIVE_DATE,
                ExpiryDate = data.EXPIRY_DATE,
                Remarks = data.REMARK ?? string.Empty
            };
        }

        public APPLIC_BA MapToData(ApplicationBusinessAssociate model, APPLIC_BA? existing = null)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var data = existing ?? new APPLIC_BA();

            data.APPLICATION_BA_ID = model.Id;
            data.APPLICATION_ID = model.ApplicationId;
            data.CONTACT_BA_ID = model.BusinessAssociateId;
            data.APPLICATION_BA_ROLE = model.Role;
            data.EFFECTIVE_DATE = model.EffectiveDate;
            data.EXPIRY_DATE = model.ExpiryDate;
            data.REMARK = model.Remarks;

            return data;
        }
    }
}
