namespace TheTechIdea.Data.OilGas;

/// <summary>Append-only evidence that a particular company membership was ended.</summary>
public sealed class CompanyMembershipRetirement
{
    public string IntentId { get; set; } = "";
    public string TenantId { get; set; } = "";
    public string BusinessAssociateId { get; set; } = "";
    public string UserId { get; set; } = "";
    public string ActorUserId { get; set; } = "";
    public string Reason { get; set; } = "";
    public string PreviousRevision { get; set; } = "";
    public string ResultRevision { get; set; } = "";
    public DateTime EffectiveFromUtc { get; set; }
    public DateTime? PreviousEffectiveToUtc { get; set; }
    public DateTime EffectiveToUtc { get; set; }
    public DateTime RetiredUtc { get; set; }
}

public sealed record CompanyMembershipRetirementRequest(string IntentId, string ExpectedRevision, string Reason)
{
    public void Require()
    {
        if (IntentId is null || !Guid.TryParseExact(IntentId, "N", out _) ||
            string.IsNullOrWhiteSpace(ExpectedRevision) || ExpectedRevision.Length > 36 || ExpectedRevision != ExpectedRevision.Trim() || ExpectedRevision.Any(char.IsControl) ||
            string.IsNullOrWhiteSpace(Reason) || Reason.Length > 1000 || Reason != Reason.Trim() || Reason.Any(char.IsControl))
            throw new ArgumentException("A retirement reference, reviewed membership revision and reason are required.");
    }
}

/// <summary>Stored retirement evidence for one selected company and account.</summary>
public sealed record CompanyMembershipRetirementPage(CompanyScope Company, string UserId, int Offset, int Limit, int Total, CompanyMembershipRetirement[] Items);
