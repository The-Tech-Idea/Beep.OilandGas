using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class WorkflowExecutionResult : ModelEntityBase
    {
        private string WorkflowIdValue = string.Empty;

        public string WorkflowId

        {

            get { return this.WorkflowIdValue; }

            set { SetProperty(ref WorkflowIdValue, value); }

        }
        private string OperationIdValue = string.Empty;

        public string OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        public Dictionary<string, object>? StepResults { get; set; }
        private DateTime StartedAtValue;

        public DateTime StartedAt

        {

            get { return this.StartedAtValue; }

            set { SetProperty(ref StartedAtValue, value); }

        }
        private DateTime? CompletedAtValue;

        public DateTime? CompletedAt

        {

            get { return this.CompletedAtValue; }

            set { SetProperty(ref CompletedAtValue, value); }

        }
        private TimeSpan? DurationValue;

        public TimeSpan? Duration

        {

            get { return this.DurationValue; }

            set { SetProperty(ref DurationValue, value); }

        }
    }
}
