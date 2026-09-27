namespace TheTechIdea.Data.OilGas;

/// <summary>
/// The titles <c>GET /api/auth/repository/me</c> refuses with — the API's answer, and what the Web reads to decide
/// whether it still admits the person.
/// </summary>
public static class RepositoryAccountRefusals
{
    /// <summary>403: an application acting for itself, which has no OilGas account.</summary>
    public const string NotAPerson = "not_a_person";

    /// <summary>503: the caller's account could not be resolved just now — an outage, not a refusal.</summary>
    public const string Unresolved = "account_unresolved";

    /// <summary>403: the account no longer exists.</summary>
    public const string Removed = "account_removed";

    /// <summary>403: the account has been switched off.</summary>
    public const string Deactivated = "account_deactivated";
}
