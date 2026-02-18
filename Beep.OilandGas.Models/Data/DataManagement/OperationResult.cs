using System;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Result for operations that do not return entity data (e.g. Delete).
    /// Uses typed properties only - no Dictionary or untyped object.
    /// </summary>
    public class OperationResult : ModelEntityBase
    {
        private bool _success;
        public bool Success { get => _success; set => SetProperty(ref _success, value); }

        private string _message = string.Empty;
        public string Message { get => _message; set => SetProperty(ref _message, value); }

        private string? _errorMessage;
        public string? ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    }
}
