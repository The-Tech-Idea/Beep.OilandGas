using System;
using System.Collections.Generic;

namespace Beep.OilandGas.ProductionAccounting.Exceptions
{
    /// <summary>
    /// Exception for data validation failures.
    /// </summary>
    public class ValidationException : ProductionAccountingException
    {
        public List<string> ValidationErrors { get; set; }

        public ValidationException(string message, List<string> errors = null) : base(message)
        {
            ValidationErrors = errors ?? new List<string>();
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
            ValidationErrors = new List<string>();
        }
    }
}
