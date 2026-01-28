using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class WorkflowProgress : ProgressUpdate
    {
        private string WorkflowNameValue = string.Empty;

        public string WorkflowName

        {

            get { return this.WorkflowNameValue; }

            set { SetProperty(ref WorkflowNameValue, value); }

        }
        private List<OperationProgress> StepsValue = new List<OperationProgress>();

        public List<OperationProgress> Steps

        {

            get { return this.StepsValue; }

            set { SetProperty(ref StepsValue, value); }

        }
        private int CurrentStepIndexValue = -1;

        public int CurrentStepIndex

        {

            get { return this.CurrentStepIndexValue; }

            set { SetProperty(ref CurrentStepIndexValue, value); }

        }
        private int OverallProgressValue;

        public int OverallProgress

        {

            get { return this.OverallProgressValue; }

            set { SetProperty(ref OverallProgressValue, value); }

        }
        private WorkflowStatus StatusValue = WorkflowStatus.NotStarted;

        public WorkflowStatus Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private int TotalStepsValue;

        public int TotalSteps

        {

            get { return this.TotalStepsValue; }

            set { SetProperty(ref TotalStepsValue, value); }

        }
        private int CompletedStepsValue;

        public int CompletedSteps

        {

            get { return this.CompletedStepsValue; }

            set { SetProperty(ref CompletedStepsValue, value); }

        }
        private int FailedStepsValue;

        public int FailedSteps

        {

            get { return this.FailedStepsValue; }

            set { SetProperty(ref FailedStepsValue, value); }

        }
        private DateTime? StartedAtValue;

        public DateTime? StartedAt

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
        private string? CurrentStepNameValue;

        public string? CurrentStepName

        {

            get { return this.CurrentStepNameValue; }

            set { SetProperty(ref CurrentStepNameValue, value); }

        }
    }
}
