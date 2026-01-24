using System;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.DataMapping
{
    public class ApplicRemarkMapper
    {
        public ApplicationRemark MapToDomain(APPLIC_REMARK data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return new ApplicationRemark
            {
                Id = data.APPLIC_REMARK_ID ?? string.Empty,
                ApplicationId = data.PERMIT_APPLICATION_ID ?? string.Empty,
                RemarkType = data.REMARK_TYPE ?? string.Empty,
                RemarkText = data.REMARK_TEXT ?? string.Empty,
                EffectiveDate = data.EFFECTIVE_DATE,
                ExpiryDate = data.EXPIRY_DATE
            };
        }

        public APPLIC_REMARK MapToData(ApplicationRemark model, APPLIC_REMARK? existing = null)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var data = existing ?? new APPLIC_REMARK();

            data.APPLIC_REMARK_ID = model.Id;
            data.PERMIT_APPLICATION_ID = model.ApplicationId;
            data.REMARK_TYPE = model.RemarkType;
            data.REMARK_TEXT = model.RemarkText;
            data.EFFECTIVE_DATE = model.EffectiveDate;
            data.EXPIRY_DATE = model.ExpiryDate;

            return data;
        }
    }
}
