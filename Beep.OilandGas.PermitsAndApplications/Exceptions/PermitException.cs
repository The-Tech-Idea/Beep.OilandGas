using System;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.PermitsAndApplications.Exceptions
{
    /// <summary>
    /// Base exception for permit and application refusals: what the caller asked of a permit application cannot be
    /// done, in a sentence written for them.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01: the family derives from <see cref="RefusalException"/>, so the API answers each with its
    /// <see cref="RefusalException.Kind"/>'s status and its sentence, and reports nothing — a refusal is an answer, not a
    /// failure. Every message passed here is the application's own words; none is built from a caught exception's text.
    /// </remarks>
    public class PermitException : RefusalException
    {
        public PermitException(RefusalKind kind, string message) : base(kind, message)
        {
        }

        public PermitException(RefusalKind kind, string message, Exception innerException) : base(kind, message, innerException)
        {
        }
    }

    /// <summary>
    /// What was sent for a permit application cannot be done as sent (400).
    /// </summary>
    public class InvalidApplicationException : PermitException
    {
        public string? ApplicationId { get; }

        public InvalidApplicationException(string message, string? applicationId = null)
            : base(RefusalKind.Invalid, message)
        {
            ApplicationId = applicationId;
        }
    }

    /// <summary>
    /// The permit application, as it stands, cannot be submitted (409).
    /// </summary>
    public class ApplicationSubmissionException : PermitException
    {
        public string? ApplicationId { get; }

        public ApplicationSubmissionException(string message, string? applicationId = null)
            : base(RefusalKind.Conflict, message)
        {
            ApplicationId = applicationId;
        }
    }

    /// <summary>
    /// The permit or permit application named does not exist (404).
    /// </summary>
    public class PermitNotFoundException : PermitException
    {
        public string? PermitId { get; }

        public PermitNotFoundException(string message, string? permitId = null)
            : base(RefusalKind.NotFound, message)
        {
            PermitId = permitId;
        }
    }

    /// <summary>
    /// The permit has expired, so what was asked of it cannot be done (409).
    /// </summary>
    public class PermitExpiredException : PermitException
    {
        public string? PermitId { get; }
        public DateTime? ExpiryDate { get; }

        public PermitExpiredException(string message, string? permitId = null, DateTime? expiryDate = null)
            : base(RefusalKind.Conflict, message)
        {
            PermitId = permitId;
            ExpiryDate = expiryDate;
        }
    }
}
