using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Unitization
{
    public class CreateUnitAgreementRequest : ModelEntityBase
    {
        private string UnitNameValue;

        public string UnitName

        {

            get { return this.UnitNameValue; }

            set { SetProperty(ref UnitNameValue, value); }

        }
        private string UnitNumberValue;

        public string UnitNumber

        {

            get { return this.UnitNumberValue; }

            set { SetProperty(ref UnitNumberValue, value); }

        }
        private string UnitOperatorBaIdValue;

        public string UnitOperatorBaId

        {

            get { return this.UnitOperatorBaIdValue; }

            set { SetProperty(ref UnitOperatorBaIdValue, value); }

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
        private string TermsAndConditionsValue;

        public string TermsAndConditions

        {

            get { return this.TermsAndConditionsValue; }

            set { SetProperty(ref TermsAndConditionsValue, value); }

        }
    }
}
