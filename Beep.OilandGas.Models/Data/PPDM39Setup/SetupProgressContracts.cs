using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// Progress snapshot for a long-running setup operation tracked by an operationId.
    /// </summary>
    public class OperationProgressResult : ModelEntityBase
    {
        private string operationIdValue = string.Empty;
        public string OperationId { get => operationIdValue; set => SetProperty(ref operationIdValue, value); }

        private string statusValue = string.Empty;
        public string Status { get => statusValue; set => SetProperty(ref statusValue, value); }

        private int percentCompleteValue;
        public int PercentComplete { get => percentCompleteValue; set => SetProperty(ref percentCompleteValue, value); }

        private string messageValue = string.Empty;
        public string Message { get => messageValue; set => SetProperty(ref messageValue, value); }

        private bool isCompletedValue;
        public bool IsCompleted { get => isCompletedValue; set => SetProperty(ref isCompletedValue, value); }

        private bool hasFailedValue;
        public bool HasFailed { get => hasFailedValue; set => SetProperty(ref hasFailedValue, value); }

        private string? errorMessageValue;
        public string? ErrorMessage { get => errorMessageValue; set => SetProperty(ref errorMessageValue, value); }
    }

    /// <summary>
    /// Progress snapshot for a database creation execution tracked by an executionId.
    /// </summary>
    public class CreationProgressResult : ModelEntityBase
    {
        private string executionIdValue = string.Empty;
        public string ExecutionId { get => executionIdValue; set => SetProperty(ref executionIdValue, value); }

        private string statusValue = string.Empty;
        public string Status { get => statusValue; set => SetProperty(ref statusValue, value); }

        private int percentCompleteValue;
        public int PercentComplete { get => percentCompleteValue; set => SetProperty(ref percentCompleteValue, value); }

        private bool isCompletedValue;
        public bool IsCompleted { get => isCompletedValue; set => SetProperty(ref isCompletedValue, value); }

        private bool hasFailedValue;
        public bool HasFailed { get => hasFailedValue; set => SetProperty(ref hasFailedValue, value); }

        private string messageValue = string.Empty;
        public string Message { get => messageValue; set => SetProperty(ref messageValue, value); }

        private List<string> stepsCompletedValue = new();
        public List<string> StepsCompleted { get => stepsCompletedValue; set => SetProperty(ref stepsCompletedValue, value); }
    }

    /// <summary>
    /// Result of a PPDM SQL script generation operation.
    /// </summary>
    public class ScriptGenerationResult : ModelEntityBase
    {
        private bool successValue;
        public bool Success { get => successValue; set => SetProperty(ref successValue, value); }

        private string messageValue = string.Empty;
        public string Message { get => messageValue; set => SetProperty(ref messageValue, value); }

        private int scriptsGeneratedValue;
        public int ScriptsGenerated { get => scriptsGeneratedValue; set => SetProperty(ref scriptsGeneratedValue, value); }

        private string? outputPathValue;
        public string? OutputPath { get => outputPathValue; set => SetProperty(ref outputPathValue, value); }

        private List<string> generatedFilesValue = new();
        public List<string> GeneratedFiles { get => generatedFilesValue; set => SetProperty(ref generatedFilesValue, value); }

        private List<string> errorsValue = new();
        public List<string> Errors { get => errorsValue; set => SetProperty(ref errorsValue, value); }
    }
}
