using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Ownership
{
    public class OwnershipChangeHistory : ModelEntityBase
    {
        private string ChangeIdValue;

        public string ChangeId

        {

            get { return this.ChangeIdValue; }

            set { SetProperty(ref ChangeIdValue, value); }

        }
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

        }
        private DateTime ChangeDateValue;

        public DateTime ChangeDate

        {

            get { return this.ChangeDateValue; }

            set { SetProperty(ref ChangeDateValue, value); }

        }
        private string OwnerBaIdValue;

        public string OwnerBaId

        {

            get { return this.OwnerBaIdValue; }

            set { SetProperty(ref OwnerBaIdValue, value); }

        }
        private decimal? InterestBeforeValue;

        public decimal? InterestBefore

        {

            get { return this.InterestBeforeValue; }

            set { SetProperty(ref InterestBeforeValue, value); }

        }
        private decimal? InterestAfterValue;

        public decimal? InterestAfter

        {

            get { return this.InterestAfterValue; }

            set { SetProperty(ref InterestAfterValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string ApprovedByValue;

        public string ApprovedBy

        {

            get { return this.ApprovedByValue; }

            set { SetProperty(ref ApprovedByValue, value); }

        }
    }
}
