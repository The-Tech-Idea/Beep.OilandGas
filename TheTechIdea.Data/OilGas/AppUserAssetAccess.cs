namespace TheTechIdea.Data.OilGas;

// Asset identifiers are meaningful only within the selected module database scope.
public sealed class AppUserAssetAccess
{
    public string Id { get; set; } = "";
    public string UserId { get; set; } = "";
    public string DatabaseScope { get; set; } = "";
    public string AssetType { get; set; } = "";
    public string AssetId { get; set; } = "";
    public string? OrganizationId { get; set; }
    public string AccessLevel { get; set; } = "READ";
    public bool Inherit { get; set; }
    public bool IsActive { get; set; }
    public string CreatedBy { get; set; } = "";
    public string ChangedBy { get; set; } = "";
    public DateTime CreatedUtc { get; set; }
    public DateTime ChangedUtc { get; set; }
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
}
