using System;
using System.Collections.Generic;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class ScriptExecutionResult : ModelEntityBase
    {
        private string ScriptFileNameValue = string.Empty;

        public string ScriptFileName

        {

            get { return this.ScriptFileNameValue; }

            set { SetProperty(ref ScriptFileNameValue, value); }

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
        private TimeSpan DurationValue;

        public TimeSpan Duration

        {

            get { return this.DurationValue; }

            set { SetProperty(ref DurationValue, value); }

        }
        private int? RowsAffectedValue;

        public int? RowsAffected

        {

            get { return this.RowsAffectedValue; }

            set { SetProperty(ref RowsAffectedValue, value); }

        }
        private string? ExecutionLogValue;

        public string? ExecutionLog

        {

            get { return this.ExecutionLogValue; }

            set { SetProperty(ref ExecutionLogValue, value); }

        }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}
