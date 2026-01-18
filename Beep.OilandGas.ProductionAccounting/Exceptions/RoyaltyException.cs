using System;

namespace Beep.OilandGas.ProductionAccounting.Exceptions
{
    /// <summary>
    /// Exception for royalty calculation failures.
    /// </summary>
    public class RoyaltyException : ProductionAccountingException
    {
        public RoyaltyException(string message) : base(message) { }
        public RoyaltyException(string message, Exception innerException) : base(message, innerException) { }
    }
}
