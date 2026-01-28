using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class SyncStatus : ModelEntityBase
    {
        private string SyncIdValue;

        public string SyncId

        {

            get { return this.SyncIdValue; }

            set { SetProperty(ref SyncIdValue, value); }

        }
        private SyncStatusType StatusValue;

        public SyncStatusType Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private DateTime StartTimeValue;

        public DateTime StartTime

        {

            get { return this.StartTimeValue; }

            set { SetProperty(ref StartTimeValue, value); }

        }
        private DateTime? EndTimeValue;

        public DateTime? EndTime

        {

            get { return this.EndTimeValue; }

            set { SetProperty(ref EndTimeValue, value); }

        }
        private int ProgressPercentageValue;

        public int ProgressPercentage

        {

            get { return this.ProgressPercentageValue; }

            set { SetProperty(ref ProgressPercentageValue, value); }

        }
        private string CurrentOperationValue;

        public string CurrentOperation

        {

            get { return this.CurrentOperationValue; }

            set { SetProperty(ref CurrentOperationValue, value); }

        }
        private List<string> ErrorsValue = new List<string>();

        public List<string> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
    }
}
