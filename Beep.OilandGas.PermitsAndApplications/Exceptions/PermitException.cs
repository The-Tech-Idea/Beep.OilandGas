using System;

namespace Beep.OilandGas.PermitsAndApplications.Exceptions
{
    /// <summary>
    /// Base exception for permit and application related errors.
    /// </summary>
    public class PermitException : Exception
    {
        public PermitException(string message) : base(message)
        {
        }

        public PermitException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when application data is invalid.
    /// </summary>
    public class InvalidApplicationException : PermitException
    {
        public string? ApplicationId { get; }

        public InvalidApplicationException(string message, string? applicationId = null) : base(message)
        {
            ApplicationId = applicationId;
        }
    }

    /// <summary>
    /// Exception thrown when permit application submission fails.
    /// </summary>
    public class ApplicationSubmissionException : PermitException
    {
        public string? ApplicationId { get; }

        public ApplicationSubmissionException(string message, string? applicationId = null) : base(message)
        {
            ApplicationId = applicationId;
        }
    }

    /// <summary>
    /// Exception thrown when permit is not found.
    /// </summary>
    public class PermitNotFoundException : PermitException
    {
        public string? PermitId { get; }

        public PermitNotFoundException(string message, string? permitId = null) : base(message)
        {
            PermitId = permitId;
        }
    }

    /// <summary>
    /// Exception thrown when permit has expired.
    /// </summary>
    public class PermitExpiredException : PermitException
    {
        public string? PermitId { get; }
        public DateTime? ExpiryDate { get; }

        public PermitExpiredException(string message, string? permitId = null, DateTime? expiryDate = null) : base(message)
        {
            PermitId = permitId;
            ExpiryDate = expiryDate;
        }
    }
}

