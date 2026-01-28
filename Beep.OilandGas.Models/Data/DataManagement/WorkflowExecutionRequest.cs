using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class WorkflowExecutionRequest : ModelEntityBase
    {
        private WorkflowDefinition WorkflowValue = new WorkflowDefinition();

        public WorkflowDefinition Workflow

        {

            get { return this.WorkflowValue; }

            set { SetProperty(ref WorkflowValue, value); }

        }
        private string? OperationIdValue;

        public string? OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
    }
}
