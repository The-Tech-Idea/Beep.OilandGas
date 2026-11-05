using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Utilities;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentDraftAuthorityTests
{
    internal sealed class ContextDb(DbContextOptions<ContextDb> options) : RepositoryDbContext(options);
    internal sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection database=new("Data Source=:memory:");
        internal ContextDb Db;
        internal DevelopmentPlanNativeDraftTests.Fixture Drafts=new();
        internal Mock<IRepositoryAccessService> Accounts=new();
        internal Mock<ICompanyFieldSelectionService> Fields=new();
        internal Mock<ICompanyFieldSelectionAuthority> Preference=new();
        internal Mock<IAccessControlService> Assets=new();
        internal Mock<IUserAssetAccessStore> Grants=new();
        internal DevelopmentPlanRequestAuthority Authority=null!;
        internal RepositoryRequestAccount Account=null!;
        internal CompanyModuleConnection Company=null!;
        internal RepositoryTenantContext Tenant=null!;
        internal ModuleConnectionResolver Modules=null!;
        internal HttpContextAccessor Http=new();
        internal ClaimsPrincipal User=new(new ClaimsIdentity([new Claim("party_id","planner"),new Claim(ClaimTypes.Role,"ConfiguredPlanner")],"fixture"));
        internal string[] Roles=["ConfiguredPlanner"],Permissions=[PermissionConstants.DevelopmentPlanning.View,PermissionConstants.DevelopmentPlanning.Create];
        internal bool Active=true,Available=true,Stored=true,FieldAccess=true,GrantChanged;
        internal int FieldReads,AssetReads;
        internal FieldSelectionReview Context=null!;
        internal AssetDatabaseScope Planning=null!;
        internal Fixture()
        {
            database.Open();Db=new(new DbContextOptionsBuilder<ContextDb>().UseSqlite(database).Options);Db.Database.EnsureCreated();
            var planning=(ConnectionProperties)Drafts.Native.Source.Object.Dataconnection.ConnectionProp;
            planning.DatabaseType=DataSourceType.SqlLite;planning.ConnectionString="Data Source="+Path.Combine(Path.GetTempPath(),"fdp-native-fixture.db");
            var core=new ConnectionProperties{ConnectionName="core",DatabaseType=DataSourceType.SqlLite,ConnectionString="Data Source="+Path.Combine(Path.GetTempPath(),"fdp-core-fixture.db")};
            var config=new Mock<IConfigEditor>();config.SetupGet(value=>value.DataConnections).Returns(new List<ConnectionProperties>{core,planning});Drafts.Native.Editor.SetupGet(e=>e.ConfigEditor).Returns(config.Object);
            Db.Add(new AppTenant{TenantId="tenant"});
            foreach(var (module,connection) in new[]{("PPDM_CORE",core),("DEVELOPMENT_PLANNING",planning)})
            {
                Db.Add(new AppModuleConnection{TenantId="tenant",ConnectionKey=connection.ConnectionName.ToUpperInvariant(),AssignedByUserId="administrator",PhysicalTargetKey=ModuleDatabaseTarget.Key(Drafts.Native.Editor.Object,connection)});
                Db.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId=module,ConnectionName=connection.ConnectionName,ConnectionKey=connection.ConnectionName.ToUpperInvariant(),ConcurrencyStamp="original"});
            }
            Db.SaveChanges();
            Accounts.Setup(a=>a.IsAccountActiveAsync("planner",It.IsAny<CancellationToken>())).ReturnsAsync(()=>Active);
            Accounts.Setup(a=>a.GetCompanyAccessAsync("planner",It.IsAny<CompanyScope>(),It.IsAny<CancellationToken>())).ReturnsAsync((string actor,CompanyScope company,CancellationToken token)=>Available?new(company,Roles,Permissions):null);
            var companies=new RepositoryCompanyContextResolver(Accounts.Object);var tenant=new RepositoryTenantContext();tenant.SelectCompany(companies.ResolveAsync("planner",new("tenant","company")).GetAwaiter().GetResult()!);
            var modules=new ModuleConnectionResolver(Db,Drafts.Native.Editor.Object,tenant);var company=new CompanyModuleConnection(modules,tenant,companies);
            Context=new(company.Company.Scope,modules.ResolveAssetScopeAsync().GetAwaiter().GetResult(),"planner","preference","field");
            Planning=new(modules.ResolveAsync("DEVELOPMENT_PLANNING").GetAwaiter().GetResult(),modules.GetMigrationBindingFingerprintAsync(["DEVELOPMENT_PLANNING"],"planning").GetAwaiter().GetResult());
            Drafts.Request=Drafts.Request with{SelectionContext=Context,PlanningDatabase=Planning};
            Http.HttpContext=new DefaultHttpContext{User=User};var account=new RepositoryRequestAccount(Http,user=>user.FindFirst("party_id")?.Value,Accounts.Object,null!);
            Fields.Setup(f=>f.GetCurrentFieldSelectionAsync()).ReturnsAsync(()=>{
                Assert.DoesNotContain("begin",Drafts.Native.Transactions.TakeLast(1));FieldReads++;
                return new(){FieldId=Context.FieldId!,FieldName="Current recorded FIELD",SelectionAvailable=true,SelectionContext=Context};});
            Preference.Setup(p=>p.RequireStoredSelectionAsync(It.IsAny<FieldSelectionReview>())).Returns((FieldSelectionReview current)=>{
                if(!Stored || current!=Context)throw new UnauthorizedAccessException("Stored FIELD preference changed");return Task.CompletedTask;});
            Assets.Setup(a=>a.CheckAssetAccessAsync("planner","field","FIELD",It.IsAny<string?>(),null)).ReturnsAsync(()=>{AssetReads++;return new(){HasAccess=FieldAccess};});
            Grants.Setup(g=>g.ReadAsync("planner",Context.Database.Fingerprint,"company")).ReturnsAsync(()=>new List<AppUserAssetAccess>{new(){Id="grant",UserId="planner",DatabaseScope=Context.Database.Fingerprint,
                OrganizationId="company",AssetType="FIELD",AssetId="field",AccessLevel="WRITE",IsActive=true,ConcurrencyStamp=GrantChanged?"changed":"original"}});
            Account=account;Company=company;Tenant=tenant;Modules=modules;Authority=new(account,company,modules,Fields.Object,Preference.Object,Assets.Object,Grants.Object);
        }
        internal Task<DevelopmentPlanRequestScope> Capture(bool write=true)=>Authority.CaptureAsync(User,PermissionConstants.DevelopmentPlanning.Create,write,Drafts.Request.SelectionContext,write);
        public void Dispose(){Db.Dispose();database.Dispose();}
    }
    [Fact] public async Task ActualAuthorityBracketsSeparateCoreAndPlanningSourcesWithoutNestedFieldReads()
    {
        using var f=new Fixture();var scope=await f.Capture();var initial=f.FieldReads;
        var saved=await f.Drafts.Service.CreateDevelopmentPlanAsync(f.Drafts.Request,scope.CheckCurrentAsync);Assert.Equal(initial,f.FieldReads);
        await scope.ConfirmAsync();Assert.True(f.FieldReads>initial);Assert.True(f.AssetReads>=2);Assert.Equal("company",saved.BUSINESS_ASSOCIATE_ID);
        Assert.NotEqual(scope.Context.Database.ConnectionName,scope.PlanningDatabase.ConnectionName);Assert.Equal(1,f.Drafts.Native.Writes);
    }
    [Theory][InlineData("account")][InlineData("company")][InlineData("role")][InlineData("claim")][InlineData("permission")][InlineData("field")][InlineData("stored")]
    public async Task MissingCurrentAppAuthorityNeverStartsNativeDraftWrite(string fault)
    {
        using var f=new Fixture();switch(fault){case "account":f.Active=false;break;case "company":f.Available=false;break;case "role":f.Roles=[];break;
            case "claim":f.User=new(new ClaimsIdentity([new Claim("party_id","planner")],"fixture"));f.Http.HttpContext!.User=f.User;break;
            case "permission":f.Permissions=[];break;case "field":f.FieldAccess=false;break;case "stored":f.Stored=false;break;}
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Capture());Assert.Equal(0,f.Drafts.Native.Writes);Assert.Empty(f.Drafts.Native.Transactions);
    }
    [Theory][InlineData("roles")][InlineData("permissions")][InlineData("grants")][InlineData("stored")][InlineData("planning")][InlineData("core")]
    public async Task ClosingAppAuthorityOrBindingChangeRollsBackActualNativeDraft(string fault)
    {
        using var f=new Fixture();var scope=await f.Capture();f.Drafts.OnInsert=()=>{
            switch(fault){case "roles":f.Roles=["DifferentRole"];break;case "permissions":f.Permissions=[];break;case "grants":f.GrantChanged=true;break;case "stored":f.Stored=false;break;
                default:var module=fault=="core"?"PPDM_CORE":"DEVELOPMENT_PLANNING";f.Db.ModuleDatabases.Single(row=>row.ModuleId==module).ConcurrencyStamp="changed";f.Db.SaveChanges();break;}};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Drafts.Service.CreateDevelopmentPlanAsync(f.Drafts.Request,scope.CheckCurrentAsync));
        Assert.Empty(f.Drafts.Native.Rows.OfType<Beep.OilandGas.Models.Data.DevelopmentPlanning.FIELD_DEVELOPMENT_PLAN>());Assert.Contains("rollback",f.Drafts.Native.Transactions);
    }
    [Fact] public async Task RecoveryMayUseNewPreferenceRevisionOnlyForSameOriginalResources()
    {
        using var f=new Fixture();await f.Capture();f.Context=f.Context with{Revision="new-current-review"};
        var scope=await f.Authority.CaptureAsync(f.User,PermissionConstants.DevelopmentPlanning.Create,false,f.Drafts.Request.SelectionContext,false);
        Assert.Null((await f.Drafts.Service.RecoverDevelopmentPlanAsync(f.Drafts.Request,scope.Context,scope.PlanningDatabase,scope.CheckCurrentAsync)).Plan);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Capture());f.Context=f.Context with{FieldId="other"};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Authority.CaptureAsync(f.User,PermissionConstants.DevelopmentPlanning.Create,false,f.Drafts.Request.SelectionContext,false));
    }
}
#endif
