using System;
using System.Collections.Generic;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class DatabaseCreationResult : ModelEntityBase
    {
        private string ExecutionIdValue = string.Empty;

        public string ExecutionId

        {

            get { return this.ExecutionIdValue; }

            set { SetProperty(ref ExecutionIdValue, value); }

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
        private DateTime StartTimeValue;

        public DateTime StartTime

        {

            get { return this.StartTimeValue; }

            set { SetProperty(ref StartTimeValue, value); }

        }
        private DateTime EndTimeValue;

        public DateTime EndTime

        {

            get { return this.EndTimeValue; }

            set { SetProperty(ref EndTimeValue, value); }

        }
        private TimeSpan TotalDurationValue;

        public TimeSpan TotalDuration

        {

            get { return this.TotalDurationValue; }

            set { SetProperty(ref TotalDurationValue, value); }

        }
        private int TotalScriptsValue;

        public int TotalScripts

        {

            get { return this.TotalScriptsValue; }

            set { SetProperty(ref TotalScriptsValue, value); }

        }
        private int SuccessfulScriptsValue;

        public int SuccessfulScripts

        {

            get { return this.SuccessfulScriptsValue; }

            set { SetProperty(ref SuccessfulScriptsValue, value); }

        }
        private int FailedScriptsValue;

        public int FailedScripts

        {

            get { return this.FailedScriptsValue; }

            set { SetProperty(ref FailedScriptsValue, value); }

        }
        private int SkippedScriptsValue;

        public int SkippedScripts

        {

            get { return this.SkippedScriptsValue; }

            set { SetProperty(ref SkippedScriptsValue, value); }

        }
        private List<ScriptExecutionResult> ScriptResultsValue = new List<ScriptExecutionResult>();

        public List<ScriptExecutionResult> ScriptResults

        {

            get { return this.ScriptResultsValue; }

            set { SetProperty(ref ScriptResultsValue, value); }

        }
        public Dictionary<string, object> Summary { get; set; } = new Dictionary<string, object>();
        private string? LogFilePathValue;

        public string? LogFilePath

        {

            get { return this.LogFilePathValue; }

            set { SetProperty(ref LogFilePathValue, value); }

        }
    }
}
