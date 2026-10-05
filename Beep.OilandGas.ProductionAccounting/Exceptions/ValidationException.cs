using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.ProductionAccounting.Exceptions
{
    /// <summary>
    /// Data the caller sent failed validation (a refusal of kind <see cref="RefusalKind.Invalid"/>).
    /// </summary>
    public class ValidationException : RefusalException
    {
        public List<string> ValidationErrors { get; set; }

        public ValidationException(string message, List<string> errors = null) : base(RefusalKind.Invalid, message)
        {
            ValidationErrors = errors ?? new List<string>();
        }

        public ValidationException(string message, Exception innerException) : base(RefusalKind.Invalid, message, innerException)
        {
            ValidationErrors = new List<string>();
        }
    }
}
