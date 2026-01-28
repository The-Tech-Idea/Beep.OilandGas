using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
    public class ProcessDefinitionRequest : ModelEntityBase
    {
        private string ProcessNameValue = string.Empty;

        public string ProcessName

        {

            get { return this.ProcessNameValue; }

            set { SetProperty(ref ProcessNameValue, value); }

        }
        private string ProcessTypeValue = string.Empty;

        public string ProcessType

        {

            get { return this.ProcessTypeValue; }

            set { SetProperty(ref ProcessTypeValue, value); }

        } // EXPLORATION, DEVELOPMENT, PRODUCTION, DECOMMISSIONING
        private string EntityTypeValue = string.Empty;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        } // WELL, FIELD, RESERVOIR, PROSPECT, POOL, FACILITY
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private List<ProcessStepDefinitionRequest> StepsValue = new List<ProcessStepDefinitionRequest>();

        public List<ProcessStepDefinitionRequest> Steps

        {

            get { return this.StepsValue; }

            set { SetProperty(ref StepsValue, value); }

        }
        public Dictionary<string, ProcessTransitionRequest> Transitions { get; set; } = new Dictionary<string, ProcessTransitionRequest>();
        public PROCESS_CONFIGURATION Configuration { get; set; } = new PROCESS_CONFIGURATION();
        private bool IsActiveValue = true;

        public bool IsActive

        {

            get { return this.IsActiveValue; }

            set { SetProperty(ref IsActiveValue, value); }

        }
    }
}
