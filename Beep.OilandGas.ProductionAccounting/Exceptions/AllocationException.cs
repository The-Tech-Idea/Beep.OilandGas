using System;

namespace Beep.OilandGas.ProductionAccounting.Exceptions
{
    /// <summary>
    /// Exception for allocation operation failures.
    /// </summary>
    public class AllocationException : ProductionAccountingException
    {
        public AllocationException(string message) : base(message) { }
        public AllocationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
