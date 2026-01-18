using System;

namespace Beep.OilandGas.ProductionAccounting.Exceptions
{
    /// <summary>
    /// Exception for accounting operations.
    /// </summary>
    public class AccountingException : ProductionAccountingException
    {
        public AccountingException(string message) : base(message) { }
        public AccountingException(string message, Exception innerException) : base(message, innerException) { }
    }
}
