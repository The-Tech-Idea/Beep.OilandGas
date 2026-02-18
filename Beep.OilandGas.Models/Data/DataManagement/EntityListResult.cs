using System.Collections.Generic;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Strongly typed result wrapper for entity list operations.
    /// Uses typed classes, never Dictionary or untyped object.
    /// </summary>
    public class EntityListResult<T> : ModelEntityBase where T : class
    {
        private bool _success;
        public bool Success { get => _success; set => SetProperty(ref _success, value); }

        private List<T> _data = new List<T>();
        public List<T> Data { get => _data; set => SetProperty(ref _data, value); }

        private int _count;
        public int Count { get => _count; set => SetProperty(ref _count, value); }

        private string? _errorMessage;
        public string? ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    }
}
