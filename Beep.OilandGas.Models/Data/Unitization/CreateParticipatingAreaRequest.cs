using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Unitization
{
    public class CreateParticipatingAreaRequest : ModelEntityBase
    {
        private string UnitAgreementIdValue;

        public string UnitAgreementId

        {

            get { return this.UnitAgreementIdValue; }

            set { SetProperty(ref UnitAgreementIdValue, value); }

        }
        private string AreaNameValue;

        public string AreaName

        {

            get { return this.AreaNameValue; }

            set { SetProperty(ref AreaNameValue, value); }

        }
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        private DateTime? ExpiryDateValue;

        public DateTime? ExpiryDate

        {

            get { return this.ExpiryDateValue; }

            set { SetProperty(ref ExpiryDateValue, value); }

        }
    }
}
