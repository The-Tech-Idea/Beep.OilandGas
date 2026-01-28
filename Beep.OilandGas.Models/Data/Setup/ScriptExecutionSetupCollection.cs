using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Setup
{
    public class ScriptExecutionSetupCollection : ModelEntityBase
    {
        private string OrganizationIdValue = string.Empty;

        [Required]
        public string OrganizationId

        {

            get { return this.OrganizationIdValue; }

            set { SetProperty(ref OrganizationIdValue, value); }

        }

        private string ConnectionNameValue = string.Empty;


        [Required]
        public string ConnectionName


        {


            get { return this.ConnectionNameValue; }


            set { SetProperty(ref ConnectionNameValue, value); }


        }

        private List<ScriptExecutionSetupData> ScriptExecutionsValue = new List<ScriptExecutionSetupData>();


        public List<ScriptExecutionSetupData> ScriptExecutions


        {


            get { return this.ScriptExecutionsValue; }


            set { SetProperty(ref ScriptExecutionsValue, value); }


        }

        private DateTime SetupStartDateValue = DateTime.UtcNow;


        public DateTime SetupStartDate


        {


            get { return this.SetupStartDateValue; }


            set { SetProperty(ref SetupStartDateValue, value); }


        }

        private DateTime? SetupCompletionDateValue;


        public DateTime? SetupCompletionDate


        {


            get { return this.SetupCompletionDateValue; }


            set { SetProperty(ref SetupCompletionDateValue, value); }


        }

        private string OverallStatusValue = "IN_PROGRESS";


        public string OverallStatus


        {


            get { return this.OverallStatusValue; }


            set { SetProperty(ref OverallStatusValue, value); }


        } // IN_PROGRESS, COMPLETED, FAILED
    }
}
