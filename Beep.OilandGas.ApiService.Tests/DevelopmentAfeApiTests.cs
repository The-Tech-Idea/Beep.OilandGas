#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentAfeApiTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DevelopmentAfeNativeTests.Fixture Native=new();
        internal DevelopmentAssignedReviewTests.Fixture Receiver=>Native.Planning.Decision.Receiver;
        internal RepositoryDbContext Db=>Receiver.Sender.Catalog.Draft.Db;
        internal string Process=>Native.Basis.Source.Review.InstanceId;
        internal CompanyDevelopmentAfeService Service=>Build(Receiver.Http);
        internal async Task Prepare(bool finance=false)
        {
            await Native.Prepare();var shared=Receiver.Sender.Catalog.Draft.Drafts.Native.Editor;
            var connection=Native.Native.Editor.Object.ConfigEditor.DataConnections.Single();shared.Object.ConfigEditor.DataConnections.Add(connection);
            shared.Setup(editor=>editor.GetDataSource("production")).Returns(Native.Native.Source.Object);
            Db.Add(new AppModuleConnection{TenantId="tenant",ConnectionKey="PRODUCTION",AssignedByUserId="administrator",PhysicalTargetKey=ModuleDatabaseTarget.Key(shared.Object,connection)});
            Db.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="PRODUCTION",ConnectionName="production",ConnectionKey="PRODUCTION",ConcurrencyStamp="production-original"});
            if(finance)
            {
                Db.Users.Add(new(){Id="accountant",IsActive=true});Db.Roles.Add(new(){Id="finance-role",Name="FinanceRole",NormalizedName="FINANCEROLE"});
                Db.Add(new AppCompanyMembership{TenantId="tenant",BusinessAssociateId="company",UserId="accountant",GrantedByUserId="administrator",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
                Db.UserRoles.Add(new(){UserId="accountant",RoleId="finance-role"});Db.Add(new AppUserRoleExtension{UserId="accountant",RoleId="finance-role",TenantId="tenant",BusinessAssociateId="company",ApprovalStatus="Approved",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
                Db.Add(new AppUserAssetAccess{Id="finance-field",UserId="accountant",DatabaseScope=Native.Basis.Source.Review.Scope.CoreDatabase.Fingerprint,OrganizationId="company",AssetType="FIELD",AssetId="field",AccessLevel="WRITE",IsActive=true});
            }
            var role=finance?"finance-role":"review-role";
            foreach(var permission in new[]{"Accounting.View",CompanyDevelopmentAfeService.PreparePermission,"DevelopmentPlanning.View","Workflow.View"})
                if(!await Db.RoleClaims.AnyAsync(row=>row.RoleId==role && row.ClaimValue==permission))Db.RoleClaims.Add(new(){RoleId=role,ClaimType="permission",ClaimValue=permission});
            if(!finance)(await Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="reviewer")).AccessLevel="WRITE";
            await Db.SaveChangesAsync();await Receiver.SelectReceiver(finance?"accountant":"reviewer",role:finance?"FinanceRole":"ConfiguredReviewRole");
            Receiver.Resources.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD","WRITE",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=true});
        }
        internal CompanyDevelopmentAfeService Build(IHttpContextAccessor http)=>new(new RepositoryRequestAccount(http,user=>user.FindFirst("party_id")?.Value,Receiver.Accounts,null!),Receiver.Company,Receiver.Modules,Receiver.Grants,Receiver.Resources.Object,Receiver.Build(http),Native.Books,Native.Owner,Receiver.Sender.Catalog.Draft.Drafts.Native.Editor.Object,new Beep.OilandGas.ProductionAccounting.Services.AuthorizationWorkflowService(Native.Native.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<Beep.OilandGas.PPDM39.Core.IPPDM39DefaultsRepository>(),Native.Metadata));
        internal async Task<DevelopmentAfeCommand> Command()=>new(await Service.ReviewAsync(Receiver.User,Process),Guid.NewGuid(),"statutory","AFE-finance-1","Recorded approved development request",235.125m,new(2027,2,3),"Explicit functional-currency budget basis");
        public void Dispose()=>Native.Dispose();
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task CurrentTechnicalRecipientOrSeparateFinanceActorCanRequestFromApprovedBaseline(bool finance)
    {
        using var f=new Fixture();await f.Prepare(finance);var command=await f.Command();var result=await f.Service.RecordAsync(f.Receiver.User,command);result.Require(command);
        Assert.Equal(finance?"accountant":"reviewer",result.Recorded!.ROW_CREATED_BY);Assert.Equal("DRAFT",result.Recorded.STATUS);Assert.Equal(1,f.Native.Native.Writes);
        Assert.Single((await f.Service.ReviewAsync(f.Receiver.User,f.Process)).Afes);await f.Service.RecordAsync(f.Receiver.User,command,true);Assert.Equal(1,f.Native.Native.Writes);
        if(finance)await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Receiver.Service.ReadProgressAsync(f.Receiver.User,f.Process));
    }
    [Theory][InlineData("permission")][InlineData("role")][InlineData("grant")][InlineData("resource")][InlineData("binding")][InlineData("source")][InlineData("company")][InlineData("actor")]
    public async Task CurrentOriginalCompanyActorRoleSourceAndFieldCloseBeforeAnyWrite(string fault)
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();
        if(fault=="permission")f.Db.Remove(await f.Db.RoleClaims.SingleAsync(row=>row.ClaimValue==CompanyDevelopmentAfeService.PreparePermission));
        if(fault=="role")(await f.Db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId=="reviewer")).ApprovalStatus="Revoked";
        if(fault=="grant")(await f.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="reviewer")).AccessLevel="READ";
        if(fault=="resource")f.Receiver.Resources.Setup(value=>value.CheckAssetAccessAsync("reviewer","field","FIELD","WRITE",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=false});
        if(fault=="binding")(await f.Db.Set<ModuleDatabaseBinding>().SingleAsync(row=>row.ModuleId=="PRODUCTION")).ConcurrencyStamp="changed";
        if(fault=="source")f.Native.Planning.Planning.Native.Rows.OfType<Beep.OilandGas.Models.Data.DevelopmentPlanning.FIELD_DEVELOPMENT_PLAN>().Single(row=>row.FDP_STATUS=="APPROVED").FDP_NAME="changed";
        if(fault=="company")command=command with{Review=command.Review with{Source=command.Review.Source with{Review=command.Review.Source.Review with{Scope=command.Review.Source.Review.Scope with{Company=new("tenant","foreign")}}}}};
        await f.Db.SaveChangesAsync();var user=fault=="actor"?new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id","someone-else"),new Claim(ClaimTypes.Role,"ConfiguredReviewRole")],"fixture")):f.Receiver.User;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.RecordAsync(user,command));Assert.Equal(0,f.Native.Native.Writes);
    }
    [Fact] public async Task RecoveryUsesReadViewAfterPrepareAndFieldWriteRevocationWithoutNewBookQualification()
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();await f.Service.RecordAsync(f.Receiver.User,command);
        f.Db.Remove(await f.Db.RoleClaims.SingleAsync(row=>row.ClaimValue==CompanyDevelopmentAfeService.PreparePermission));(await f.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="reviewer")).AccessLevel="READ";await f.Db.SaveChangesAsync();await f.Receiver.SelectReceiver();
        f.Native.Native.Rows.OfType<COMPANY_LEDGER_BOOK>().Single().ACTIVE_IND="N";
        Assert.NotNull((await f.Service.RecordAsync(f.Receiver.User,command,true)).Recorded);Assert.Equal(1,f.Native.Native.Writes);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.RecordAsync(f.Receiver.User,command));Assert.Equal(1,f.Native.Native.Writes);
    }
    [Fact] public async Task ApprovedFinancialSourceNeverExposesDraftOrMissingBaseline()
    {
        using var f=new Fixture();await f.Prepare(true);f.Native.Planning.Planning.Native.Rows.RemoveAll(row=>row is Beep.OilandGas.Models.Data.DevelopmentPlanning.FIELD_DEVELOPMENT_PLAN plan && plan.FDP_STATUS=="APPROVED");
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.ReviewAsync(f.Receiver.User,f.Process));Assert.Equal(0,f.Native.Native.Writes);
    }

    [Fact] public async Task FreshAppPermissionRevocationInsideOwnedInsertRollsBackTheDraftRequest()
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();
        f.Native.OnInsert=()=>{f.Db.Remove(f.Db.RoleClaims.Single(row=>row.ClaimValue==CompanyDevelopmentAfeService.PreparePermission));f.Db.SaveChanges();};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.RecordAsync(f.Receiver.User,command));Assert.Empty(f.Native.Native.Rows.OfType<Beep.OilandGas.Models.Data.ProductionAccounting.AFE>());Assert.Equal("rollback",f.Native.Native.Transactions.Last());
    }
    [Fact] public async Task LaterSourceChangeLeavesCommittedOriginalUnconfirmedUntilOriginalEvidenceIsAuthorized()
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();var baseline=f.Native.Planning.Planning.Native.Rows.OfType<Beep.OilandGas.Models.Data.DevelopmentPlanning.FIELD_DEVELOPMENT_PLAN>().Single(row=>row.FDP_STATUS=="APPROVED");var name=baseline.FDP_NAME;
        f.Native.OnInsert=()=>baseline.FDP_NAME="Changed after native financial insert";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.RecordAsync(f.Receiver.User,command));Assert.Single(f.Native.Native.Rows.OfType<Beep.OilandGas.Models.Data.ProductionAccounting.AFE>());Assert.Equal(1,f.Native.Native.Writes);
        // A failed owned planning read restores detached fixture rows; reacquire the current native header.
        f.Native.Planning.Planning.Native.Rows.OfType<Beep.OilandGas.Models.Data.DevelopmentPlanning.FIELD_DEVELOPMENT_PLAN>().Single(row=>row.FDP_STATUS=="APPROVED").FDP_NAME=name;f.Native.OnInsert=null;Assert.NotNull((await f.Service.RecordAsync(f.Receiver.User,command,true)).Recorded);Assert.Equal(1,f.Native.Native.Writes);
    }
    [Fact] public async Task ActualTypedHttpRecordsRecoversAndRemovesUnqualifiedLegacyRoutes()
    {
        using var f=new Fixture();await f.Prepare(true);await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>{
            services.AddScoped<CompanyDevelopmentAfeService>(provider=>f.Build(provider.GetRequiredService<IHttpContextAccessor>()));services.AddSingleton(Mock.Of<IFailureReporter>());
        });host.Client.DefaultRequestHeaders.Add("X-Test-Actor","accountant");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","FinanceRole");
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();company.TrySelect(company.Clear(),"accountant",new(f.Receiver.Tenant.Company!.Scope,f.Receiver.Tenant.Company.Roles.ToArray(),f.Receiver.Tenant.Company.Permissions.ToArray()));
        var client=new Beep.OilandGas.Web.Services.AfeServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company));var review=await client.ReviewDevelopmentAsync(f.Process);var command=new DevelopmentAfeCommand(review,Guid.NewGuid(),"statutory","AFE-HTTP","Typed financial request",1.125m,new(2027,2,3),"Explicit HTTP budget basis");
        Assert.NotNull((await client.RecordDevelopmentAsync(command)).Recorded);Assert.NotNull((await client.RecoverDevelopmentAsync(command)).Recorded);Assert.Single((await client.ReviewDevelopmentAsync(f.Process)).Afes);Assert.Equal(1,f.Native.Native.Writes);
        Assert.Equal(HttpStatusCode.NotFound,(await host.Client.GetAsync("/api/accounting/cost/afe?connectionName=foreign")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await host.Client.PostAsync("/api/accounting/cost/afe",null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,(await host.Client.PatchAsync("/api/accounting/cost/afe/anything/approve",null)).StatusCode);
        host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.GetAsync("/api/accounting/cost/afe/development?processId="+f.Process)).StatusCode);
    }
}
#endif
