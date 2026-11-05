using Microsoft.AspNetCore.Identity;

namespace TheTechIdea.Data.OilGas;

// Local authorization account. External sign-in links belong in AspNetUserLogins.
public sealed class OilGasUser : IdentityUser
{
    public bool IsActive { get; set; } = true;
}

public sealed class RepositoryBootstrap
{
    public int Id { get; set; }
    public string AdministratorUserId { get; set; } = "";
    public DateTime CompletedAtUtc { get; set; }
}

// ConnectionName references BeepDM ConfigEditor; never store credentials here.
public sealed class ModuleDatabaseBinding
{
    public long BindingId { get; set; }
    // Existing bindings stay unowned until an administrator supplies reviewed ownership evidence.
    public string? TenantId { get; set; }
    public string ModuleId { get; set; } = "";
    public string ConnectionName { get; set; } = "";
    public string? ConnectionKey { get; set; }
    public string? OwnershipAssignedByUserId { get; set; }
    public DateTime? OwnershipAssignedUtc { get; set; }
    public string? OwnershipEvidence { get; set; }
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>One tenant owns a BeepDM connection name; several of its modules may share that connection.</summary>
public sealed class AppModuleConnection
{
    public string ConnectionKey { get; set; } = "";
    public string TenantId { get; set; } = "";
    public string AssignedByUserId { get; set; } = "";
    public DateTime AssignedUtc { get; set; }
    // Historical name-only ownership must be explicitly revalidated before routing.
    public string? PhysicalTargetKey { get; set; }
    public string? TargetVerifiedByUserId { get; set; }
    public DateTime? TargetVerifiedUtc { get; set; }
}
