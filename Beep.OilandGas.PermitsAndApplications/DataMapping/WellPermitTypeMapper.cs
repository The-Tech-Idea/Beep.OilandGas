using System;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.DataMapping
{
    public class WellPermitTypeMapper
    {
        public WellPermitTypeInfo MapToDomain(WELL_PERMIT_TYPE data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return new WellPermitTypeInfo
            {
                PermitType = data.PERMIT_TYPE ?? string.Empty,
                Abbreviation = data.ABBREVIATION ?? string.Empty,
                LongName = data.LONG_NAME ?? string.Empty,
                ShortName = data.SHORT_NAME ?? string.Empty,
                GrantedByBaId = data.GRANTED_BY_BA_ID ?? string.Empty,
                RateScheduleId = data.RATE_SCHEDULE_ID ?? string.Empty,
                EffectiveDate = data.EFFECTIVE_DATE,
                ExpiryDate = data.EXPIRY_DATE
            };
        }

        public WELL_PERMIT_TYPE MapToData(WellPermitTypeInfo model, WELL_PERMIT_TYPE? existing = null)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var data = existing ?? new WELL_PERMIT_TYPE();

            data.PERMIT_TYPE = model.PermitType;
            data.ABBREVIATION = model.Abbreviation;
            data.LONG_NAME = model.LongName;
            data.SHORT_NAME = model.ShortName;
            data.GRANTED_BY_BA_ID = model.GrantedByBaId;
            data.RATE_SCHEDULE_ID = model.RateScheduleId;
            data.EFFECTIVE_DATE = model.EffectiveDate;
            data.EXPIRY_DATE = model.ExpiryDate;

            return data;
        }
    }
}
