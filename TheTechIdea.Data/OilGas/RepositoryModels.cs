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
    public string ModuleId { get; set; } = "";
    public string ConnectionName { get; set; } = "";
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
}
