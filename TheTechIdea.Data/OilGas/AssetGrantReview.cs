namespace TheTechIdea.Data.OilGas;

// Stored inventory is administrative evidence, never an effective-access decision.
public sealed record AssetGrantReviewPage(CompanyScope Company, string DatabaseScope, string UserId,
    int Offset, int Limit, int Total, List<AppUserAssetAccess> Items);

public sealed record RetireAssetGrantRequest(string ExpectedRevision, string Reason);

public sealed record AssetGrantRetirementPage(CompanyScope Company, string DatabaseScope, string GrantId,
    int Offset, int Limit, int Total, List<AssetGrantRetirement> Items);

// Append-only command evidence. Snapshot uses the canonical grant model, including historical unknown qualifiers.
public sealed class AssetGrantRetirement
{
    public string Revision { get; init; } = "";
    public string GrantId { get; init; } = "";
    public string TenantId { get; init; } = "";
    public string OrganizationId { get; init; } = "";
    public string DatabaseScope { get; init; } = "";
    public string PreviousRevision { get; init; } = "";
    public string SnapshotJson { get; init; } = "";
    public string Reason { get; init; } = "";
    public string ActorUserId { get; init; } = "";
    public DateTime ChangedUtc { get; init; }
}
