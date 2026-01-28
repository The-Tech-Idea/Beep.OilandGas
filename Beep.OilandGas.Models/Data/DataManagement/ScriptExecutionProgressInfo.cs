using System;
using System.Collections.Generic;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class ScriptExecutionProgressInfo : ModelEntityBase
    {
        private string ExecutionIdValue = string.Empty;

        public string ExecutionId

        {

            get { return this.ExecutionIdValue; }

            set { SetProperty(ref ExecutionIdValue, value); }

        }
        private int TotalScriptsValue;

        public int TotalScripts

        {

            get { return this.TotalScriptsValue; }

            set { SetProperty(ref TotalScriptsValue, value); }

        }
        private int CompletedScriptsValue;

        public int CompletedScripts

        {

            get { return this.CompletedScriptsValue; }

            set { SetProperty(ref CompletedScriptsValue, value); }

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
        private decimal ProgressPercentageValue;

        public decimal ProgressPercentage

        {

            get { return this.ProgressPercentageValue; }

            set { SetProperty(ref ProgressPercentageValue, value); }

        }
        private string CurrentScriptValue = string.Empty;

        public string CurrentScript

        {

            get { return this.CurrentScriptValue; }

            set { SetProperty(ref CurrentScriptValue, value); }

        }
        private DateTime StartTimeValue;

        public DateTime StartTime

        {

            get { return this.StartTimeValue; }

            set { SetProperty(ref StartTimeValue, value); }

        }
        private DateTime? EstimatedCompletionTimeValue;

        public DateTime? EstimatedCompletionTime

        {

            get { return this.EstimatedCompletionTimeValue; }

            set { SetProperty(ref EstimatedCompletionTimeValue, value); }

        }
        private string StatusValue = "Not Started";

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }
}
