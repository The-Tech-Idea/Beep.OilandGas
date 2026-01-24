using System;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.DataMapping
{
    public class BaPermitMapper
    {
        public BusinessAssociatePermit MapToDomain(BA_PERMIT data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return new BusinessAssociatePermit
            {
                BusinessAssociateId = data.BUSINESS_ASSOCIATE_ID ?? string.Empty,
                Jurisdiction = data.JURISDICTION ?? string.Empty,
                PermitType = data.PERMIT_TYPE ?? string.Empty,
                PermitObservationNumber = data.PERMIT_OBS_NO,
                PermitNumber = data.PERMIT_NUM ?? string.Empty,
                EffectiveDate = data.EFFECTIVE_DATE,
                ExpiryDate = data.EXPIRY_DATE
            };
        }

        public BA_PERMIT MapToData(BusinessAssociatePermit model, BA_PERMIT? existing = null)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var data = existing ?? new BA_PERMIT();

            data.BUSINESS_ASSOCIATE_ID = model.BusinessAssociateId;
            data.JURISDICTION = model.Jurisdiction;
            data.PERMIT_TYPE = model.PermitType;
            data.PERMIT_OBS_NO = model.PermitObservationNumber;
            data.PERMIT_NUM = model.PermitNumber;
            data.EFFECTIVE_DATE = model.EffectiveDate;
            data.EXPIRY_DATE = model.ExpiryDate;

            return data;
        }
    }
}
