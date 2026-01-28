using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class WorkflowStep : ModelEntityBase
    {
        private string StepIdValue = Guid.NewGuid().ToString();

        public string StepId

        {

            get { return this.StepIdValue; }

            set { SetProperty(ref StepIdValue, value); }

        }
        private string StepNameValue = string.Empty;

        public string StepName

        {

            get { return this.StepNameValue; }

            set { SetProperty(ref StepNameValue, value); }

        }
        private string? DependsOnValue;

        public string? DependsOn

        {

            get { return this.DependsOnValue; }

            set { SetProperty(ref DependsOnValue, value); }

        } // StepId this step depends on
        private bool CanRunInParallelValue = false;

        public bool CanRunInParallel

        {

            get { return this.CanRunInParallelValue; }

            set { SetProperty(ref CanRunInParallelValue, value); }

        }
        private int EstimatedWeightValue = 1;

        public int EstimatedWeight

        {

            get { return this.EstimatedWeightValue; }

            set { SetProperty(ref EstimatedWeightValue, value); }

        } // Weight for progress calculation
        private string OperationTypeValue = string.Empty;

        public string OperationType

        {

            get { return this.OperationTypeValue; }

            set { SetProperty(ref OperationTypeValue, value); }

        } // "ImportCsv", "Validate", "QualityCheck", etc.
        public Dictionary<string, object>? Parameters { get; set; }
    }
}
