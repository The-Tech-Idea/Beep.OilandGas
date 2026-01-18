using System;

namespace Beep.OilandGas.ProductionAccounting.Exceptions
{
    /// <summary>
    /// Base exception for production accounting operations.
    /// </summary>
    public class ProductionAccountingException : Exception
    {
        public ProductionAccountingException(string message) : base(message) { }
        public ProductionAccountingException(string message, Exception innerException) : base(message, innerException) { }
    }
}
