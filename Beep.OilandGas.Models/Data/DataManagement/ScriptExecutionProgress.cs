using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class ScriptExecutionProgress : ProgressUpdate
    {
        private string ScriptNameValue = string.Empty;

        public string ScriptName

        {

            get { return this.ScriptNameValue; }

            set { SetProperty(ref ScriptNameValue, value); }

        }
        private int CurrentScriptIndexValue;

        public int CurrentScriptIndex

        {

            get { return this.CurrentScriptIndexValue; }

            set { SetProperty(ref CurrentScriptIndexValue, value); }

        }
        private int TotalScriptsValue;

        public int TotalScripts

        {

            get { return this.TotalScriptsValue; }

            set { SetProperty(ref TotalScriptsValue, value); }

        }
        private int StatementsExecutedValue;

        public int StatementsExecuted

        {

            get { return this.StatementsExecutedValue; }

            set { SetProperty(ref StatementsExecutedValue, value); }

        }
        private int TotalStatementsValue;

        public int TotalStatements

        {

            get { return this.TotalStatementsValue; }

            set { SetProperty(ref TotalStatementsValue, value); }

        }
        private TimeSpan? ElapsedTimeValue;

        public TimeSpan? ElapsedTime

        {

            get { return this.ElapsedTimeValue; }

            set { SetProperty(ref ElapsedTimeValue, value); }

        }
        private TimeSpan? EstimatedTimeRemainingValue;

        public TimeSpan? EstimatedTimeRemaining

        {

            get { return this.EstimatedTimeRemainingValue; }

            set { SetProperty(ref EstimatedTimeRemainingValue, value); }

        }
    }
}
