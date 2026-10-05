namespace Beep.OilandGas.Models.Core.Refusals
{
    /// <summary>What a refusal refuses — and so the status the API answers it with.</summary>
    public enum RefusalKind
    {
        /// <summary>What was sent cannot be done as sent (400).</summary>
        Invalid,

        /// <summary>What was named does not exist, or is not the caller's to see (404).</summary>
        NotFound,

        /// <summary>The data as it stands does not allow it — already closed, already posted, still in use (409).</summary>
        Conflict,

        /// <summary>The caller may not do it (403).</summary>
        Forbidden,
    }
}
