using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
    public class ProcessHistoryResponse : ModelEntityBase
    {
        private string HistoryIdValue = string.Empty;

        public string HistoryId

        {

            get { return this.HistoryIdValue; }

            set { SetProperty(ref HistoryIdValue, value); }

        }
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        private string StepInstanceIdValue = string.Empty;

        public string StepInstanceId

        {

            get { return this.StepInstanceIdValue; }

            set { SetProperty(ref StepInstanceIdValue, value); }

        }
        private string ActionValue = string.Empty;

        public string Action

        {

            get { return this.ActionValue; }

            set { SetProperty(ref ActionValue, value); }

        }
        private string PreviousStateValue = string.Empty;

        public string PreviousState

        {

            get { return this.PreviousStateValue; }

            set { SetProperty(ref PreviousStateValue, value); }

        }
        private string NewStateValue = string.Empty;

        public string NewState

        {

            get { return this.NewStateValue; }

            set { SetProperty(ref NewStateValue, value); }

        }
        private DateTime TimestampValue;

        public DateTime Timestamp

        {

            get { return this.TimestampValue; }

            set { SetProperty(ref TimestampValue, value); }

        }
        private string PerformedByValue = string.Empty;

        public string PerformedBy

        {

            get { return this.PerformedByValue; }

            set { SetProperty(ref PerformedByValue, value); }

        }
        private string NotesValue = string.Empty;

        public string Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }
}
