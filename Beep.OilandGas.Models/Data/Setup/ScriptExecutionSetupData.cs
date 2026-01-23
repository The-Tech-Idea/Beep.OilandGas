using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Setup
{
    /// <summary>
    /// Data class for script execution metadata and tracking
    /// Tracks which scripts have been executed for a connection/organization
    /// Maps to SETUP_WIZARD_LOG table
    /// </summary>
    public class ScriptExecutionSetupData : ModelEntityBase
    {
        private string SetupLogIdValue = string.Empty;

        [Required]
        public string SetupLogId

        {

            get { return this.SetupLogIdValue; }

            set { SetProperty(ref SetupLogIdValue, value); }

        }

        private string ScriptNameValue = string.Empty;


        [Required]
        public string ScriptName


        {


            get { return this.ScriptNameValue; }


            set { SetProperty(ref ScriptNameValue, value); }


        }

        private string DatabaseTypeValue = string.Empty;


        [Required]
        public string DatabaseType


        {


            get { return this.DatabaseTypeValue; }


            set { SetProperty(ref DatabaseTypeValue, value); }


        }

        private int ExecutionOrderValue;


        [Required]
        public int ExecutionOrder


        {


            get { return this.ExecutionOrderValue; }


            set { SetProperty(ref ExecutionOrderValue, value); }


        }

        private bool RequiredValue = true;


        [Required]
        public bool Required


        {


            get { return this.RequiredValue; }


            set { SetProperty(ref RequiredValue, value); }


        }

        private string? OrganizationIdValue;


        public string? OrganizationId


        {


            get { return this.OrganizationIdValue; }


            set { SetProperty(ref OrganizationIdValue, value); }


        }

        private string? ConnectionNameValue;


        public string? ConnectionName


        {


            get { return this.ConnectionNameValue; }


            set { SetProperty(ref ConnectionNameValue, value); }


        }

        private string? ExecutedByValue;


        public string? ExecutedBy


        {


            get { return this.ExecutedByValue; }


            set { SetProperty(ref ExecutedByValue, value); }


        }

        private DateTime? ExecutionDateValue;


        public DateTime? ExecutionDate


        {


            get { return this.ExecutionDateValue; }


            set { SetProperty(ref ExecutionDateValue, value); }


        }

        private string StatusValue = "PENDING";


        public string Status


        {


            get { return this.StatusValue; }


            set { SetProperty(ref StatusValue, value); }


        } // PENDING, SUCCESS, FAILED

        private string? ErrorMessageValue;


        public string? ErrorMessage


        {


            get { return this.ErrorMessageValue; }


            set { SetProperty(ref ErrorMessageValue, value); }


        }

        private TimeSpan? ExecutionTimeValue;


        public TimeSpan? ExecutionTime


        {


            get { return this.ExecutionTimeValue; }


            set { SetProperty(ref ExecutionTimeValue, value); }


        }

        private string? SetupDataValue;


        public string? SetupData


        {


            get { return this.SetupDataValue; }


            set { SetProperty(ref SetupDataValue, value); }


        } // JSON or structured setup data
    }

    /// <summary>
    /// Collection of script execution records for tracking setup progress
    /// </summary>
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







