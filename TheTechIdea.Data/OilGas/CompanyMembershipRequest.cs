namespace TheTechIdea.Data.OilGas;

/// <summary>One original company membership request and its initial scoped role.</summary>
public sealed record CompanyMembershipRequest(string UserId, string RoleId, string Reason, string IntentId)
{
    public void Require()
    {
        static bool Valid(string? value, int limit) => !string.IsNullOrWhiteSpace(value) && value.Length <= limit && value == value.Trim() && !value.Any(char.IsControl);
        if (!Valid(UserId, 128) || !Valid(RoleId, 128) || !Valid(Reason, 1000) ||
            !Guid.TryParseExact(IntentId, "N", out var id) || id == Guid.Empty || id.ToString("N") != IntentId)
            throw new ArgumentException("Select a user, a role and a reason for the company membership.");
    }
}
