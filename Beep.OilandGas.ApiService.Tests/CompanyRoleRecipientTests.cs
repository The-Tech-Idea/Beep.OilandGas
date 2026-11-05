using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyRoleRecipientTests
{
    private sealed class Context(DbContextOptions<Context> options) : RepositoryDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);
            model.Entity<IdentityRole>().Property(role => role.Name).UseCollation("NOCASE");
        }
    }
    private sealed class Clock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026,10,25,12,0,0,TimeSpan.Zero);
    }
    private sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        internal Context Db;
        internal readonly CompanyScope Company = new("tenant","company");
        internal readonly Clock Time = new();
        internal RepositoryAccessService Access;
        internal AppCompanyMembership Membership;
        internal AppUserRoleExtension Assignment;
        internal OilGasUser Reviewer;
        internal AppPersona Persona;
        internal Fixture()
        {
            connection.Open();Db=new(new DbContextOptionsBuilder<Context>().UseSqlite(connection).Options);Db.Database.EnsureCreated();
            Db.Add(new AppTenant{TenantId=Company.TenantId});Db.Add(new AppCompany{TenantId=Company.TenantId,BusinessAssociateId=Company.BusinessAssociateId});
            Reviewer=new(){Id="reviewer",IsActive=true};Db.Users.Add(Reviewer);
            Db.Users.Add(new(){Id="administrator",IsActive=true});
            Persona=new(){Code="RESERVOIR_ENGINEER",Name="Reservoir engineer"};Db.Add(Persona);
            Db.Add(new AppUserPersona{UserId=Reviewer.Id,TenantId=Company.TenantId,BusinessAssociateId=Company.BusinessAssociateId,PersonaCode=Persona.Code,ChangedBy="administrator",ChangedUtc=Time.GetUtcNow().UtcDateTime});
            Db.Roles.Add(new(){Id="role",Name="TechnicalReviewer",NormalizedName="TECHNICALREVIEWER"});
            Db.UserRoles.Add(new(){UserId=Reviewer.Id,RoleId="role"});
            Membership=new(){TenantId=Company.TenantId,BusinessAssociateId=Company.BusinessAssociateId,UserId=Reviewer.Id,GrantedByUserId="administrator",EffectiveFromUtc=Time.GetUtcNow().UtcDateTime.AddDays(-1)};
            Assignment=new(){TenantId=Company.TenantId,BusinessAssociateId=Company.BusinessAssociateId,UserId=Reviewer.Id,RoleId="role",ApprovalStatus="Approved",EffectiveFromUtc=Membership.EffectiveFromUtc};
            Db.Add(Membership);Db.Add(Assignment);Db.SaveChanges();Access=new(Db,Time);
        }
        internal Task<RepositoryCompanyRoleRecipient[]> Read()=>Access.GetCompanyRoleRecipientsAsync(Company,"TechnicalReviewer");
        internal async Task<RepositoryRolePersonaReader> Reader()
        {
            var tenant=new RepositoryTenantContext();tenant.SelectCompany((await new RepositoryCompanyContextResolver(Access).ResolveAsync(Reviewer.Id,Company))!);
            return new(Access,tenant);
        }
        public void Dispose(){Db.Dispose();connection.Dispose();}
    }

    [Fact] public async Task ExactCurrentCompanyStandardRoleProjectsActualRecipientsAndFreshRevocation()
    {
        using var f=new Fixture();var recipient=Assert.Single(await f.Read());Assert.Equal(new("reviewer","RESERVOIR_ENGINEER"),recipient);
        Assert.Contains("TechnicalReviewer",(await f.Access.GetCompanyAccessAsync(recipient.UserId,f.Company))!.Roles);
        var reader=await f.Reader();Assert.Equal(new[]{"RESERVOIR_ENGINEER"},await reader.GetActivePersonasAsync("TechnicalReviewer"));
        f.Assignment.ApprovalStatus="Revoked";await f.Db.SaveChangesAsync();Assert.Empty(await reader.GetActivePersonasAsync("TechnicalReviewer"));
    }
    [Fact] public async Task RecipientPersonaComesOnlyFromTheSelectedCompany()
    {
        using var f = new Fixture(); var other = new CompanyScope(f.Company.TenantId, "other");
        f.Db.Add(new AppCompany { TenantId = other.TenantId, BusinessAssociateId = other.BusinessAssociateId });
        f.Db.Add(new AppCompanyMembership { TenantId = other.TenantId, BusinessAssociateId = other.BusinessAssociateId, UserId = f.Reviewer.Id, GrantedByUserId = "administrator", EffectiveFromUtc = f.Time.GetUtcNow().UtcDateTime.AddDays(-1) });
        f.Db.Add(new AppUserRoleExtension { TenantId = other.TenantId, BusinessAssociateId = other.BusinessAssociateId, UserId = f.Reviewer.Id, RoleId = "role", ApprovalStatus = "Approved", EffectiveFromUtc = f.Time.GetUtcNow().UtcDateTime.AddDays(-1) });
        await f.Db.SaveChangesAsync();
        Assert.Empty(await f.Access.GetCompanyRoleRecipientsAsync(other, "TechnicalReviewer"));
        f.Db.Add(new AppUserPersona { UserId = f.Reviewer.Id, TenantId = other.TenantId, BusinessAssociateId = other.BusinessAssociateId, PersonaCode = f.Persona.Code, ChangedBy = "administrator", ChangedUtc = f.Time.GetUtcNow().UtcDateTime });
        await f.Db.SaveChangesAsync();
        Assert.Single(await f.Access.GetCompanyRoleRecipientsAsync(other, "TechnicalReviewer"));
        Assert.Single(await f.Read());
    }
    [Theory]
    [InlineData("inactive-user")][InlineData("inactive-persona")][InlineData("inactive-company")][InlineData("inactive-tenant")]
    [InlineData("future-membership")][InlineData("expired-membership")][InlineData("no-membership")]
    [InlineData("pending-role")][InlineData("future-role")][InlineData("expired-role")][InlineData("global-role-only")][InlineData("metadata-only")][InlineData("no-persona")]
    public async Task GlobalOrStaleGrantsCannotCreateCompanyRecipients(string fault)
    {
        using var f=new Fixture();var now=f.Time.GetUtcNow().UtcDateTime;
        switch(fault)
        {
            case "inactive-user":f.Reviewer.IsActive=false;break;case "inactive-persona":f.Persona.IsActive=false;break;
            case "inactive-company":(await f.Db.Set<AppCompany>().SingleAsync()).IsActive=false;break;
            case "inactive-tenant":(await f.Db.Set<AppTenant>().SingleAsync()).IsActive=false;break;
            case "future-membership":f.Membership.EffectiveFromUtc=now.AddTicks(1);break;
            case "expired-membership":f.Membership.EffectiveToUtc=now;break;case "no-membership":f.Db.Remove(f.Assignment);f.Db.Remove(f.Membership);break;
            case "pending-role":f.Assignment.ApprovalStatus="Pending";break;case "future-role":f.Assignment.EffectiveFromUtc=now.AddTicks(1);break;
            case "expired-role":f.Assignment.EffectiveToUtc=now;break;case "global-role-only":f.Db.Remove(f.Assignment);break;
            case "metadata-only":f.Db.UserRoles.Remove(await f.Db.UserRoles.SingleAsync());break;
            case "no-persona":f.Db.Remove(await f.Db.Set<AppUserPersona>().SingleAsync());break;
        }
        await f.Db.SaveChangesAsync();Assert.Empty(await f.Read());
    }
    [Theory][InlineData("tenant","other-company")][InlineData("other-tenant","company")]
    public async Task AnotherCompanyOrTenantCannotSupplyRoleRecipients(string tenant,string company)
    {
        using var f=new Fixture();Assert.Empty(await f.Access.GetCompanyRoleRecipientsAsync(new(tenant,company),"TechnicalReviewer"));
        // The same account may be a member of both companies; a role grant must belong to the selected company.
        if(tenant!="tenant")f.Db.Add(new AppTenant{TenantId=tenant});
        f.Db.Add(new AppCompany{TenantId=tenant,BusinessAssociateId=company});
        f.Db.Add(new AppCompanyMembership{TenantId=tenant,BusinessAssociateId=company,UserId="reviewer",GrantedByUserId="administrator",EffectiveFromUtc=f.Membership.EffectiveFromUtc});
        await f.Db.SaveChangesAsync();Assert.Empty(await f.Access.GetCompanyRoleRecipientsAsync(new(tenant,company),"TechnicalReviewer"));
        Assert.Single(await f.Read());
    }
    [Fact] public async Task RoleClaimSpellingMustMatchStandardIsInRoleEvenWithCaseInsensitiveDatabase()
    {
        using var f=new Fixture();Assert.Empty(await f.Access.GetCompanyRoleRecipientsAsync(f.Company,"technicalreviewer"));
        Assert.Empty(await f.Access.GetCompanyRoleRecipientsAsync(f.Company,"MissingRole"));Assert.Single(await f.Read());
    }
    [Fact] public async Task EffectiveFromIsInclusiveAndFutureExpiryIsAllowed()
    {
        using var f=new Fixture();var now=f.Time.GetUtcNow().UtcDateTime;f.Membership.EffectiveFromUtc=now;f.Assignment.EffectiveFromUtc=now;
        f.Membership.EffectiveToUtc=now.AddTicks(1);f.Assignment.EffectiveToUtc=now.AddTicks(1);await f.Db.SaveChangesAsync();Assert.Single(await f.Read());
    }
    [Fact] public async Task ReaderRequiresSelectionAndCurrentActorMembershipOnBothSidesOfLookup()
    {
        using var f=new Fixture();await Assert.ThrowsAsync<InvalidOperationException>(()=>new RepositoryRolePersonaReader(f.Access,new()).GetActivePersonasAsync("TechnicalReviewer"));
        var reader=await f.Reader();f.Membership.EffectiveToUtc=f.Time.GetUtcNow().UtcDateTime;await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>reader.GetActivePersonasAsync("TechnicalReviewer"));
        var access=new Mock<IRepositoryAccessService>(MockBehavior.Strict);var tenant=new RepositoryTenantContext();var reads=0;
        access.Setup(x=>x.GetCompanyAccessAsync("reviewer",f.Company,default)).ReturnsAsync(()=>++reads<3?new RepositoryCompanyAccess(f.Company,[],[]):null);
        access.Setup(x=>x.GetCompanyRoleRecipientsAsync(f.Company,"TechnicalReviewer",default)).ReturnsAsync([new("someone","ENGINEER")]);
        tenant.SelectCompany((await new RepositoryCompanyContextResolver(access.Object).ResolveAsync("reviewer",f.Company))!);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>new RepositoryRolePersonaReader(access.Object,tenant).GetActivePersonasAsync("TechnicalReviewer"));
    }
#if WORKFLOW_INBOX_FOCUSED
    [Fact] public async Task NativeTaskRoutingUsesCurrentCompanyRecipientsAndRevocationRollsBackAnotherHandoff()
    {
        using var f=new Fixture();var workflow=new CompanyProcessStartTests.Fixture("TechnicalReviewer");
        workflow.Service=new(workflow.Native.Editor.Object,
            Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<Beep.OilandGas.PPDM39.Core.IPPDM39DefaultsRepository>(),
            Mock.Of<Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataRepository>(),"lifecycle",personaReader:await f.Reader());
        var intent=(await workflow.Intent()) with{ActorUserId="reviewer"};var receipt=await workflow.Start(intent);
        Assert.All(workflow.Native.Rows.OfType<Beep.OilandGas.LifeCycle.Data.Tables.CROSS_PERSONA_TASK>(),task=>Assert.Equal("RESERVOIR_ENGINEER",task.TARGET_PERSONA_CODE));
        Assert.Single(receipt.TaskIds);var recorded=workflow.Native.Rows.Count;
        f.Assignment.ApprovalStatus="Revoked";await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(()=>workflow.Start(intent with{IntentId=Guid.NewGuid().ToString("N"),EntityId="another-invoice"}));
        Assert.Equal(recorded,workflow.Native.Rows.Count);Assert.Equal("rollback",workflow.Native.Transactions.Last());
    }
#endif
}
