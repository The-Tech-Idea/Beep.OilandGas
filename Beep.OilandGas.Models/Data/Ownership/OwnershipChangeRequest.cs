using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Ownership
{
    public class OwnershipChangeRequest : ModelEntityBase
    {
        private string PropertyIdValue;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string ChangeTypeValue;

        public string ChangeType

        {

            get { return this.ChangeTypeValue; }

            set { SetProperty(ref ChangeTypeValue, value); }

        } // DIVISION_ORDER, TRANSFER_ORDER
        private string ChangeIdValue;

        public string ChangeId

        {

            get { return this.ChangeIdValue; }

            set { SetProperty(ref ChangeIdValue, value); }

        }
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
