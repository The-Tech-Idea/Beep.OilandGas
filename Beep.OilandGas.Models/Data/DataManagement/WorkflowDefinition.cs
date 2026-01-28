using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class WorkflowDefinition : ModelEntityBase
    {
        private string WorkflowIdValue = Guid.NewGuid().ToString();

        public string WorkflowId

        {

            get { return this.WorkflowIdValue; }

            set { SetProperty(ref WorkflowIdValue, value); }

        }
        private string WorkflowNameValue = string.Empty;

        public string WorkflowName

        {

            get { return this.WorkflowNameValue; }

            set { SetProperty(ref WorkflowNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private List<WorkflowStep> StepsValue = new List<WorkflowStep>();

        public List<WorkflowStep> Steps

        {

            get { return this.StepsValue; }

            set { SetProperty(ref StepsValue, value); }

        }
        private bool StopOnErrorValue = true;

        public bool StopOnError

        {

            get { return this.StopOnErrorValue; }

            set { SetProperty(ref StopOnErrorValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string? PhaseValue;

        public string? Phase

        {

            get { return this.PhaseValue; }

            set { SetProperty(ref PhaseValue, value); }

        } // EXPLORATION, DEVELOPMENT, PRODUCTION, DECOMMISSIONING
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        } // Field this workflow belongs to
        public Dictionary<string, object>? Parameters { get; set; } // Additional parameters including FIELD_ID, PHASE
    }
}
