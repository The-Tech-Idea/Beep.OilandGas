using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class PeriodClosingStatus : ModelEntityBase
    {
        private string PeriodIdValue = string.Empty;

        public string PeriodId

        {

            get { return this.PeriodIdValue; }

            set { SetProperty(ref PeriodIdValue, value); }

        }
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private PeriodClosingState StateValue;

        public PeriodClosingState State

        {

            get { return this.StateValue; }

            set { SetProperty(ref StateValue, value); }

        }
        private DateTime? OpenedDateValue;

        public DateTime? OpenedDate

        {

            get { return this.OpenedDateValue; }

            set { SetProperty(ref OpenedDateValue, value); }

        }
        private DateTime? ClosedDateValue;

        public DateTime? ClosedDate

        {

            get { return this.ClosedDateValue; }

            set { SetProperty(ref ClosedDateValue, value); }

        }
        private string ClosedByValue = string.Empty;

        public string ClosedBy

        {

            get { return this.ClosedByValue; }

            set { SetProperty(ref ClosedByValue, value); }

        }
        private bool IsLockedValue;

        public bool IsLocked

        {

            get { return this.IsLockedValue; }

            set { SetProperty(ref IsLockedValue, value); }

        }
    }
}
