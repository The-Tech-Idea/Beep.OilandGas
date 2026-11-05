using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed class RoleWorkspaceTests
{
    private sealed class Authority : IRepositoryAccessService
    {
        public bool Administrator = true;
        public bool Member = true;
        public string[] Roles = ["Accountant"];
        public int Reads;
        public bool RevokeOnClosing;
        public Task<bool> IsAccountActiveAsync(string actor,CancellationToken token=default)=>Task.FromResult(true);
        public Task<RepositoryUserAccess?> GetAccessAsync(string actor, CancellationToken token = default) =>
            Task.FromResult<RepositoryUserAccess?>(new(actor, true, Administrator && !(RevokeOnClosing && ++Reads > 1) ? ["Administrator"] : [], []));
        public Task<CompanyScope[]> GetCompaniesAsync(string actor, CancellationToken token = default) => Task.FromResult<CompanyScope[]>([]);
        public Task<RepositoryCompanyRoleRecipient[]> GetCompanyRoleRecipientsAsync(CompanyScope scope, string role, CancellationToken token = default) =>
            Task.FromResult<RepositoryCompanyRoleRecipient[]>([]);
        public Task<RepositoryCompanyAccess?> GetCompanyAccessAsync(string actor, CompanyScope scope, CancellationToken token = default) =>
            Task.FromResult<RepositoryCompanyAccess?>(Member ? new(scope, Roles, []) : null);
    }
    private sealed class Context(DbContextOptions<Context> options) : RepositoryDbContext(options);
    private sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        public RepositoryDbContext Db { get; }
        public RepositoryPersonaService Service { get; }
        public Authority Access { get; } = new();
        public Fixture()
        {
            connection.Open(); Db = new Context(new DbContextOptionsBuilder<Context>().UseSqlite(connection).Options);
            Db.Database.EnsureCreated(); Db.Users.Add(new OilGasUser { Id="actor", IsActive=true });
            Db.Roles.Add(new IdentityRole { Id="accountant", Name="Accountant", NormalizedName="ACCOUNTANT" });
            Db.Set<AppPersona>().AddRange(new AppPersona { Code="ACCOUNTANT", Name="Accountant" }, new AppPersona { Code="ENGINEER", Name="Engineer" }, new AppPersona { Code="INACTIVE", Name="Inactive", IsActive=false });
            Db.SaveChanges(); Service = new(Db);
        }
        public async Task SaveAsync(string roleId, string code, bool enabled, string actor, Authority access)
        {
            var reviewer = new Authority();
            string revision;
            try { revision = (await Service.ReviewRoleWorkspaceAsync(roleId, actor, reviewer)).Revision; }
            catch (InvalidOperationException) { revision = new string('A',64); }
            await Service.SaveRoleWorkspaceAsync(roleId, code, enabled, revision, actor, access);
        }
        public void Dispose() { Db.Dispose(); connection.Dispose(); }
    }
    [Fact]
    public async Task MappingUsesExistingRoleMetadataAndAuditWithoutGrantingAccess()
    {
        using var f = new Fixture(); await f.SaveAsync("accountant", "ACCOUNTANT", true, "actor", f.Access);
        await f.SaveAsync("accountant", "ACCOUNTANT", true, "actor", f.Access);
        var mapping = Assert.Single(await f.Db.RoleClaims.ToListAsync()); Assert.Equal("workspace-persona", mapping.ClaimType);
        Assert.Single(await f.Db.Set<AppPersonaAudit>().ToListAsync()); Assert.Empty(await f.Db.UserRoles.ToListAsync());
        var review = await f.Service.ReviewWorkspacesAsync("actor", new("tenant","company"), f.Access);
        Assert.Equal("ACCOUNTANT", Assert.Single(review.Personas).Code); Assert.Equal(64, review.Revision.Length);
        await f.SaveAsync("accountant", "ACCOUNTANT", false, "actor", f.Access);
        var removed = await f.Service.ReviewWorkspacesAsync("actor", review.Company, f.Access);
        Assert.Empty(removed.Personas); Assert.NotEqual(review.Revision, removed.Revision); Assert.Equal(2, await f.Db.Set<AppPersonaAudit>().CountAsync());
    }
    [Fact]
    public async Task UnassignedRolesCannotSeeMappedWorkspaces()
    {
        using var f = new Fixture(); await f.SaveAsync("accountant","ACCOUNTANT",true,"actor",f.Access);
        f.Access.Roles = ["Reader"]; Assert.Empty((await f.Service.ReviewWorkspacesAsync("actor",new("tenant","company"),f.Access)).Personas);
    }
    [Fact]
    public async Task MissingMembershipCannotReviewWorkspaces()
    {
        using var f = new Fixture(); f.Access.Member=false;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Service.ReviewWorkspacesAsync("actor",new("tenant","company"),f.Access));
    }
    [Fact]
    public async Task NonAdministratorCannotConfigureGlobalRoleNavigation()
    {
        using var f = new Fixture(); f.Access.Administrator=false;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.SaveAsync("accountant","ACCOUNTANT",true,"actor",f.Access));
        Assert.Empty(await f.Db.RoleClaims.ToListAsync()); Assert.Empty(await f.Db.Set<AppPersonaAudit>().ToListAsync());
    }
    [Fact]
    public async Task ClosingAdministratorRevocationRollsBackMetadataAndAudit()
    {
        using var f = new Fixture(); f.Access.RevokeOnClosing=true;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.SaveAsync("accountant","ACCOUNTANT",true,"actor",f.Access));
        f.Db.ChangeTracker.Clear(); Assert.Empty(await f.Db.RoleClaims.ToListAsync()); Assert.Empty(await f.Db.Set<AppPersonaAudit>().ToListAsync());
    }
    [Fact]
    public async Task InactivePersonaCannotBeEnabled()
    {
        using var f = new Fixture();
        await Assert.ThrowsAsync<ArgumentException>(() => f.SaveAsync("accountant","INACTIVE",true,"actor",f.Access));
        Assert.Empty(await f.Db.RoleClaims.ToListAsync());
    }
    [Fact]
    public async Task DuplicateMappingsRequireReconciliation()
    {
        using var f = new Fixture(); f.Db.RoleClaims.AddRange(Enumerable.Range(0,2).Select(_ => new IdentityRoleClaim<string> { RoleId="accountant",ClaimType="workspace-persona",ClaimValue="ACCOUNTANT" })); await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.SaveAsync("accountant","ACCOUNTANT",false,"actor",f.Access));
        Assert.Equal(2, await f.Db.RoleClaims.CountAsync()); Assert.Empty(await f.Db.Set<AppPersonaAudit>().ToListAsync());
    }

    [Fact]
    public async Task StaleReviewCannotOverwriteAnotherAdministratorsMapping()
    {
        using var f = new Fixture(); var review = await f.Service.ReviewRoleWorkspaceAsync("accountant","actor",f.Access);
        await f.SaveAsync("accountant","ENGINEER",true,"actor",f.Access);
        var audits = await f.Db.Set<AppPersonaAudit>().CountAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Service.SaveRoleWorkspaceAsync("accountant","ACCOUNTANT",true,review.Revision,"actor",f.Access));
        Assert.Equal("ENGINEER", Assert.Single(await f.Db.RoleClaims.ToListAsync()).ClaimValue);
        Assert.Equal(audits, await f.Db.Set<AppPersonaAudit>().CountAsync());
    }
    [Fact]
    public async Task NonAdministratorCannotReadGlobalMappings()
    {
        using var f = new Fixture(); f.Access.Administrator=false;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Service.ReviewRoleWorkspaceAsync("accountant","actor",f.Access));
    }

    private static async Task SeedCompanyAsync(Fixture f, string roleId, string company, string approval = "Approved")
    {
        if (!await f.Db.Set<AppTenant>().AnyAsync()) f.Db.Add(new AppTenant { TenantId="tenant", Name="Tenant" });
        f.Db.Add(new AppCompany { TenantId="tenant", BusinessAssociateId=company });
        f.Db.Add(new AppCompanyMembership { TenantId="tenant", BusinessAssociateId=company, UserId="actor", GrantedByUserId="actor", EffectiveFromUtc=DateTime.UtcNow.AddDays(-1) });
        if (!await f.Db.UserRoles.AnyAsync(row => row.UserId=="actor" && row.RoleId==roleId)) f.Db.UserRoles.Add(new() { UserId="actor", RoleId=roleId });
        f.Db.Add(new AppUserRoleExtension { UserId="actor", RoleId=roleId, TenantId="tenant", BusinessAssociateId=company, ApprovalStatus=approval, EffectiveFromUtc=DateTime.UtcNow.AddDays(-1) });
        await f.Db.SaveChangesAsync();
    }
    [Fact]
    public async Task CanonicalApprovedCompanyRolesResolveWorkspaceWithoutPermissionGrant()
    {
        using var f = new Fixture(); await f.SaveAsync("accountant","ACCOUNTANT",true,"actor",f.Access);
        await SeedCompanyAsync(f,"accountant","A"); await SeedCompanyAsync(f,"accountant","B","Pending");
        var access = new RepositoryAccessService(f.Db);
        var companyA = await f.Service.ReviewWorkspacesAsync("actor",new("tenant","A"),access);
        var companyB = await f.Service.ReviewWorkspacesAsync("actor",new("tenant","B"),access);
        Assert.Equal("ACCOUNTANT",Assert.Single(companyA.Personas).Code); Assert.Empty(companyB.Personas);
        Assert.Empty((await access.GetCompanyAccessAsync("actor",new("tenant","A")))!.Permissions);
        var assignment = await f.Db.Set<AppUserRoleExtension>().SingleAsync(row => row.BusinessAssociateId=="A");
        assignment.ApprovalStatus="Revoked"; await f.Db.SaveChangesAsync();
        var revoked = await f.Service.ReviewWorkspacesAsync("actor",new("tenant","A"),access);
        Assert.Empty(revoked.Personas); Assert.NotEqual(companyA.Revision,revoked.Revision);
    }
    [Fact]
    public async Task CanonicalCompanyOnlyAdministratorCannotConfigureGlobalMappings()
    {
        using var f = new Fixture(); f.Db.Roles.Add(new IdentityRole { Id="admin",Name="Administrator",NormalizedName="ADMINISTRATOR" }); await f.Db.SaveChangesAsync();
        await SeedCompanyAsync(f,"admin","A"); var access = new RepositoryAccessService(f.Db);
        Assert.Contains("Administrator",(await access.GetCompanyAccessAsync("actor",new("tenant","A")))!.Roles);
        Assert.DoesNotContain("Administrator",(await access.GetAccessAsync("actor"))!.Roles);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Service.ReviewRoleWorkspaceAsync("accountant","actor",access));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Service.SaveRoleWorkspaceAsync("accountant","ACCOUNTANT",true,new string('A',64),"actor",access));
        Assert.Empty(await f.Db.RoleClaims.ToListAsync()); Assert.Empty(await f.Db.Set<AppPersonaAudit>().ToListAsync());
    }

    [Theory]
    [InlineData("{\"expectedRevision\":\"revision\"}")]
    [InlineData("{\"enabled\":true}")]
    public void MappingRequestRequiresExplicitStateAndReviewedRevision(string json)
        => Assert.Throws<System.Text.Json.JsonException>(() => System.Text.Json.JsonSerializer.Deserialize<RoleWorkspaceMappingRequest>(json, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)));
}
