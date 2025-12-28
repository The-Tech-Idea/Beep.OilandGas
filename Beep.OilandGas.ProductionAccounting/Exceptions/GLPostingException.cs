using System;

namespace Beep.OilandGas.ProductionAccounting.Exceptions
{
    /// <summary>
    /// Exception thrown when GL posting fails.
    /// </summary>
    public class GLPostingException : Exception
    {
        public string TransactionId { get; }
        public string SourceModule { get; }
        public string? GlAccountId { get; }

        public GLPostingException(string message) : base(message)
        {
        }

        public GLPostingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public GLPostingException(string message, string transactionId, string sourceModule, string? glAccountId = null) 
            : base(message)
        {
            TransactionId = transactionId;
            SourceModule = sourceModule;
            GlAccountId = glAccountId;
        }

        public GLPostingException(string message, string transactionId, string sourceModule, Exception innerException, string? glAccountId = null) 
            : base(message, innerException)
        {
            TransactionId = transactionId;
            SourceModule = sourceModule;
            GlAccountId = glAccountId;
        }
    }
}

