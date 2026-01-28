using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class InspectionExecutionRequest : ModelEntityBase
    {
        private string InspectionIdValue = string.Empty;

        public string InspectionId

        {

            get { return this.InspectionIdValue; }

            set { SetProperty(ref InspectionIdValue, value); }

        }
        private DateTime ExecutionDateValue;

        public DateTime ExecutionDate

        {

            get { return this.ExecutionDateValue; }

            set { SetProperty(ref ExecutionDateValue, value); }

        }
        private string? InspectorValue;

        public string? Inspector

        {

            get { return this.InspectorValue; }

            set { SetProperty(ref InspectorValue, value); }

        }
        private string? FindingsValue;

        public string? Findings

        {

            get { return this.FindingsValue; }

            set { SetProperty(ref FindingsValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        public Dictionary<string, object>? InspectionData { get; set; }
    }
}
