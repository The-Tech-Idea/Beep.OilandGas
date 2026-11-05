namespace Beep.OilandGas.Models.Data.Security;

/// <summary>One current configuration aggregate in the app-owned access repository, never a PPDM module table.</summary>
public sealed class ORGANIZATION_HIERARCHY_CONFIG
{
    public string CONFIG_ID { get; set; } = string.Empty;
    public string TENANT_ID { get; set; } = string.Empty;
    public string ORGANIZATION_ID { get; set; } = string.Empty;
    public string LEVELS_JSON { get; set; } = string.Empty;
    public string REVISION { get; set; } = string.Empty;
    public string CREATED_BY { get; set; } = string.Empty;
    public DateTime CREATED_UTC { get; set; }
    public string CHANGED_BY { get; set; } = string.Empty;
    public DateTime CHANGED_UTC { get; set; }
}

/// <summary>Immutable revision evidence written in the same transaction as its configuration aggregate.</summary>
public sealed class OrganizationHierarchyConfigRevision
{
    public string Revision { get; set; } = string.Empty;
    public string ConfigId { get; set; } = string.Empty;
    public string? PreviousRevision { get; set; }
    public string LevelsJson { get; set; } = string.Empty;
    public string ActorUserId { get; set; } = string.Empty;
    public DateTime ChangedUtc { get; set; }
}
