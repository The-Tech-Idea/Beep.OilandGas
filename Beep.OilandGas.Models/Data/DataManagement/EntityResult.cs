using System;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Strongly typed result wrapper for a single entity operation.
    /// Uses typed classes, never Dictionary or untyped object.
    /// </summary>
    public class EntityResult<T> : ModelEntityBase where T : class
    {
        private bool _success;
        public bool Success { get => _success; set => SetProperty(ref _success, value); }

        private T? _data;
        public T? Data { get => _data; set => SetProperty(ref _data, value); }

        private string _message = string.Empty;
        public string Message { get => _message; set => SetProperty(ref _message, value); }

        private string? _errorMessage;
        public string? ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    }
}
