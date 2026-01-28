using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class SyncResult : ModelEntityBase
    {
        private string SyncIdValue;

        public string SyncId

        {

            get { return this.SyncIdValue; }

            set { SetProperty(ref SyncIdValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private int RecordsProcessedValue;

        public int RecordsProcessed

        {

            get { return this.RecordsProcessedValue; }

            set { SetProperty(ref RecordsProcessedValue, value); }

        }
        private int RecordsCreatedValue;

        public int RecordsCreated

        {

            get { return this.RecordsCreatedValue; }

            set { SetProperty(ref RecordsCreatedValue, value); }

        }
        private int RecordsUpdatedValue;

        public int RecordsUpdated

        {

            get { return this.RecordsUpdatedValue; }

            set { SetProperty(ref RecordsUpdatedValue, value); }

        }
        private int RecordsSkippedValue;

        public int RecordsSkipped

        {

            get { return this.RecordsSkippedValue; }

            set { SetProperty(ref RecordsSkippedValue, value); }

        }
        private int ConflictsFoundValue;

        public int ConflictsFound

        {

            get { return this.ConflictsFoundValue; }

            set { SetProperty(ref ConflictsFoundValue, value); }

        }
        private List<SyncConflict> ConflictsValue = new List<SyncConflict>();

        public List<SyncConflict> Conflicts

        {

            get { return this.ConflictsValue; }

            set { SetProperty(ref ConflictsValue, value); }

        }
        private TimeSpan DurationValue;

        public TimeSpan Duration

        {

            get { return this.DurationValue; }

            set { SetProperty(ref DurationValue, value); }

        }
        private List<string> MessagesValue = new List<string>();

        public List<string> Messages

        {

            get { return this.MessagesValue; }

            set { SetProperty(ref MessagesValue, value); }

        }
    }
}
