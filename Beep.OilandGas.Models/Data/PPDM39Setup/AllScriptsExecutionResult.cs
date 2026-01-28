using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Data
{
    public class AllScriptsExecutionResult : ModelEntityBase
    {
        /// <summary>
        /// Unique execution identifier
        /// </summary>
        private string ExecutionIdValue = string.Empty;

        public string ExecutionId

        {

            get { return this.ExecutionIdValue; }

            set { SetProperty(ref ExecutionIdValue, value); }

        }

        /// <summary>
        /// Overall success flag
        /// </summary>
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }

        /// <summary>
        /// Error message if the operation failed
        /// </summary>
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }

        /// <summary>
        /// Start time of the operation
        /// </summary>
        private DateTime StartTimeValue;

        public DateTime StartTime

        {

            get { return this.StartTimeValue; }

            set { SetProperty(ref StartTimeValue, value); }

        }

        /// <summary>
        /// End time of the operation
        /// </summary>
        private DateTime EndTimeValue;

        public DateTime EndTime

        {

            get { return this.EndTimeValue; }

            set { SetProperty(ref EndTimeValue, value); }

        }

        /// <summary>
        /// Total duration of the operation
        /// </summary>
        public TimeSpan TotalDuration => EndTime - StartTime;

        /// <summary>
        /// Individual script execution results (DTO)
        /// </summary>
        private List<ScriptExecutionResult> ResultsValue = new List<ScriptExecutionResult>();

        public List<ScriptExecutionResult> Results

        {

            get { return this.ResultsValue; }

            set { SetProperty(ref ResultsValue, value); }

        }

        /// <summary>
        /// Total number of scripts attempted
        /// </summary>
        private int TotalScriptsValue;

        public int TotalScripts

        {

            get { return this.TotalScriptsValue; }

            set { SetProperty(ref TotalScriptsValue, value); }

        }

        /// <summary>
        /// Number of successful scripts
        /// </summary>
        private int SuccessfulScriptsValue;

        public int SuccessfulScripts

        {

            get { return this.SuccessfulScriptsValue; }

            set { SetProperty(ref SuccessfulScriptsValue, value); }

        }

        /// <summary>
        /// Number of failed scripts
        /// </summary>
        private int FailedScriptsValue;

        public int FailedScripts

        {

            get { return this.FailedScriptsValue; }

            set { SetProperty(ref FailedScriptsValue, value); }

        }

        /// <summary>
        /// Flag indicating whether all scripts succeeded
        /// </summary>
        private bool AllSucceededValue;

        public bool AllSucceeded

        {

            get { return this.AllSucceededValue; }

            set { SetProperty(ref AllSucceededValue, value); }

        }

        /// <summary>
        /// Optional summary dictionary
        /// </summary>
        public Dictionary<string, object>? Summary { get; set; }

        /// <summary>
        /// Optional log file path where execution log was written
        /// </summary>
        private string? LogFilePathValue;

        public string? LogFilePath

        {

            get { return this.LogFilePathValue; }

            set { SetProperty(ref LogFilePathValue, value); }

        }
    }
}
