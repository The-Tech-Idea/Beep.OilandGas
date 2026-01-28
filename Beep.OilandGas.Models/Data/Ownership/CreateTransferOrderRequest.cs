using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Ownership
{
    public class CreateTransferOrderRequest : ModelEntityBase
    {
        private string PropertyIdValue;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string FromOwnerBaIdValue;

        public string FromOwnerBaId

        {

            get { return this.FromOwnerBaIdValue; }

            set { SetProperty(ref FromOwnerBaIdValue, value); }

        }
        private string ToOwnerBaIdValue;

        public string ToOwnerBaId

        {

            get { return this.ToOwnerBaIdValue; }

            set { SetProperty(ref ToOwnerBaIdValue, value); }

        }
        private decimal InterestTransferredValue;

        public decimal InterestTransferred

        {

            get { return this.InterestTransferredValue; }

            set { SetProperty(ref InterestTransferredValue, value); }

        }
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
    }
}
