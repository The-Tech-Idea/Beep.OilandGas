using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
    public class ProcessDefinitionResponse : ModelEntityBase
    {
        private string ProcessIdValue = string.Empty;

        public string ProcessId

        {

            get { return this.ProcessIdValue; }

            set { SetProperty(ref ProcessIdValue, value); }

        }
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

        }
        private string EntityTypeValue = string.Empty;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private List<ProcessStepDefinitionResponse> StepsValue = new List<ProcessStepDefinitionResponse>();

        public List<ProcessStepDefinitionResponse> Steps

        {

            get { return this.StepsValue; }

            set { SetProperty(ref StepsValue, value); }

        }
        private bool IsActiveValue;

        public bool IsActive

        {

            get { return this.IsActiveValue; }

            set { SetProperty(ref IsActiveValue, value); }

        }
        private DateTime CreatedDateValue;

        public DateTime CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
        private string CreatedByValue = string.Empty;

        public string CreatedBy

        {

            get { return this.CreatedByValue; }

            set { SetProperty(ref CreatedByValue, value); }

        }
    }
}
