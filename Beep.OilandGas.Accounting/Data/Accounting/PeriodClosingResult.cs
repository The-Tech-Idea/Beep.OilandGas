using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class PeriodClosingResult : ModelEntityBase
    {
        private string ClosingIdValue = string.Empty;

        public string ClosingId

        {

            get { return this.ClosingIdValue; }

            set { SetProperty(ref ClosingIdValue, value); }

        }
        private DateTime PeriodEndDateValue;

        public DateTime PeriodEndDate

        {

            get { return this.PeriodEndDateValue; }

            set { SetProperty(ref PeriodEndDateValue, value); }

        }
        private bool IsClosedValue;

        public bool IsClosed

        {

            get { return this.IsClosedValue; }

            set { SetProperty(ref IsClosedValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private int JournalEntriesCreatedValue;

        public int JournalEntriesCreated

        {

            get { return this.JournalEntriesCreatedValue; }

            set { SetProperty(ref JournalEntriesCreatedValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private DateTime ClosingDateValue = DateTime.UtcNow;

        public DateTime ClosingDate

        {

            get { return this.ClosingDateValue; }

            set { SetProperty(ref ClosingDateValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
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
        private DateTime CompletedDateValue;

        public DateTime CompletedDate

        {

            get { return this.CompletedDateValue; }

            set { SetProperty(ref CompletedDateValue, value); }

        }
        private List<string> MessagesValue = new();

        public List<string> Messages

        {

            get { return this.MessagesValue; }

            set { SetProperty(ref MessagesValue, value); }

        }
        private List<string> ErrorsValue = new();

        public List<string> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
    }
}
