using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.DrillingAndConstruction.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Utilities;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingProgramApiTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DevelopmentInvestmentHandoffApiTests.Fixture Funding=new();internal DrillingProgramDraftNativeTests.Fixture Store=new();internal PersonaTaskRoutingTests.Fixture Core=new();
        internal WELL Well=null!;internal WELL_TEST? AcceptedPhysicalTest;internal bool WellAccess=true;internal Beep.OilandGas.Repository.IRepositoryAccessService? RecipientAccounts;
        internal async Task Prepare()
        {
            await Funding.Prepare();await Funding.Start();await Funding.Routing.Afe.Receiver.SelectReceiver("investor",role:"InvestmentReviewer");var routing=Funding.Routing.Service();var assigned=await routing.ReadAssignedAsync(Funding.Routing.Afe.Receiver.User,Funding.Original.InstanceId());await routing.DecideAssignedStageAsync(Funding.Routing.Afe.Receiver.User,new(assigned,Guid.NewGuid(),"APPROVE","Confirmed investment supports program drafting"));
            await Store.Prepare();var native=Store.Store.Native;native.Connection="programs";native.Source.SetupGet(s=>s.DatasourceName).Returns("programs");var execution=(ConnectionProperties)native.Source.Object.Dataconnection.ConnectionProp;execution.ConnectionName="programs";execution.DatabaseType=DataSourceType.SqlLite;execution.ConnectionString="Data Source="+Path.Combine(Path.GetTempPath(),"drilling-program-api-fixture.db");execution.Host="";execution.Database="";native.Source.SetupGet(s=>s.DatasourceType).Returns(DataSourceType.SqlLite);
            var editor=Funding.Routing.Catalog.Draft.Drafts.Native.Editor;editor.Setup(e=>e.GetDataSource("programs")).Returns(native.Source.Object);editor.Object.ConfigEditor.DataConnections.Add(execution);
            var db=Funding.Routing.Afe.Db;db.Add(new AppModuleConnection{TenantId="tenant",ConnectionKey="PROGRAMS",AssignedByUserId="administrator",PhysicalTargetKey=ModuleDatabaseTarget.Key(editor.Object,execution)});db.Add(new ModuleDatabaseBinding{TenantId="tenant",ModuleId="DRILLING_EXECUTION",ConnectionName="programs",ConnectionKey="PROGRAMS",ConcurrencyStamp="original-drilling"});
            foreach(var permission in new[]{"Drilling.View","Drilling.Create"})db.RoleClaims.Add(new(){RoleId="finance-role",ClaimType="permission",ClaimValue=permission});await db.SaveChangesAsync();await Select();
            var core=editor.Object.ConfigEditor.DataConnections.Single(c=>c.ConnectionName=="core");Core.Connection="core";Core.Rows.Clear();Core.Source.SetupGet(s=>s.DatasourceName).Returns("core");Core.Source.SetupGet(s=>s.DatasourceType).Returns(core.DatabaseType);Mock.Get(Core.Source.Object.Dataconnection).SetupGet(c=>c.ConnectionProp).Returns(core);editor.Setup(e=>e.GetDataSource("core")).Returns(Core.Source.Object);
            Well=new(){UWI="well/one%2F",ASSIGNED_FIELD="field",WELL_NAME="Recorded development target",ACTIVE_IND="Y"};Core.Rows.Add(Well);
        }
        internal async Task Select(string actor="accountant",string role="FinanceRole",CompanyScope? selected=null)
        {
            await Funding.Routing.Afe.Receiver.SelectReceiver(actor,selected,role);Funding.Routing.Afe.Receiver.Resources.Setup(r=>r.CheckAssetAccessAsync(actor,"well/one%2F","WELL","READ",null)).ReturnsAsync(()=>new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=WellAccess});
        }
        internal CompanyDrillingProgramService Build(Microsoft.AspNetCore.Http.IHttpContextAccessor? http=null)
        {
            var receiver=Funding.Routing.Afe.Receiver;var editor=Funding.Routing.Catalog.Draft.Drafts.Native.Editor.Object;return new(new RepositoryRequestAccount(http??receiver.Http,u=>u.FindFirst("party_id")?.Value,receiver.Accounts,null!),receiver.Company,receiver.Modules,receiver.Grants,receiver.Resources.Object,Funding.Routing.Service(),new DrillingOperationService(editor,new CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>()),editor,new Beep.OilandGas.LifeCycle.Services.Processes.BoundProcessService(()=>Task.FromResult("lifecycle"),_=>Funding.Routing.Catalog.Workflow.Service),new DevelopmentReviewRecipientAuthority(RecipientAccounts??new Beep.OilandGas.Repository.RepositoryAccessService(Funding.Routing.Afe.Db),new Beep.OilandGas.Repository.RepositoryPersonaService(Funding.Routing.Afe.Db),new RepositoryAssetAccessStore(Funding.Routing.Afe.Db,receiver.Company,Mock.Of<IAssetGrantTargetVerifier>())),Funding.Routing.Afe.Db,receiver.Accounts,new Beep.OilandGas.Repository.RepositoryPersonaService(Funding.Routing.Afe.Db));
        }
        internal Task<DrillingProgramBasis> Basis()=>Build().ReviewAsync(Funding.Routing.Afe.Receiver.User,Funding.Routing.Afe.Process,Funding.Original.Review.Source.AfeId,Well.UWI);
        public void Dispose(){Store.Dispose();Funding.Dispose();}
    }
    [Fact]public async Task ProtectedTypedBasisCreationAndReadOnlyRecoveryUseTheSameApprovedSourceAndNativeProgram()
    {
        using var f=new Fixture();await f.Prepare();await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>{services.AddScoped<CompanyDrillingProgramService>(p=>f.Build(p.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()));services.AddSingleton(Mock.Of<IFailureReporter>());});host.Client.DefaultRequestHeaders.Add("X-Test-Actor","accountant");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","FinanceRole");
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();void Selection(){var current=f.Funding.Routing.Afe.Receiver.Tenant.Company!;company.TrySelect(company.Clear(),"accountant",new(current.Scope,current.Roles.ToArray(),current.Permissions.ToArray()));}Selection();
        var client=new Beep.OilandGas.Web.Services.DrillingServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company));var basis=await client.ReviewProgramBasisAsync(f.Funding.Routing.Afe.Process,f.Funding.Original.Review.Source.AfeId,f.Well.UWI);Assert.Equal(f.Well.UWI,basis.Well.Uwi);Assert.Equal("APPROVED",basis.Funding.Progress!.State);
        var original=f.Store.Command with{Basis=basis,IntentId=Guid.NewGuid()};var saved=await client.RecordProgramDraftAsync(original);Assert.Equal("DRAFT",saved.Recorded!.PROGRAM_STATUS);Assert.Equal(basis.Funding.Source.Afe().RequestedAmount,saved.Recorded.AFE_COST);Assert.Equal(1,f.Store.Store.Native.Writes);Assert.Equal(original.ProgramId(),Assert.Single((await client.GetProgramDraftsAsync(basis.Funding.Source.Current.Source.Review.InstanceId,basis.Funding.Source.AfeId,basis.Well.Uwi)).Drafts).PROGRAM_ID);Assert.Null(f.Well.SPUD_DATE);Assert.Equal("Y",f.Well.ACTIVE_IND);
        var db=f.Funding.Routing.Afe.Db;db.Remove(await db.RoleClaims.SingleAsync(r=>r.RoleId=="finance-role" && r.ClaimValue=="Drilling.Create"));(await db.Set<AppUserAssetAccess>().SingleAsync(r=>r.UserId=="accountant")).AccessLevel="READ";await db.SaveChangesAsync();await f.Select();Selection();
        Assert.NotNull((await client.RecoverProgramDraftAsync(original)).Recorded);Assert.Equal(1,f.Store.Store.Native.Writes);await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Build().RecordAsync(f.Funding.Routing.Afe.Receiver.User,original));
        foreach(var route in new[]{"/api/drillingoperation/operations","/api/field/current/drilling/operations","/api/field/current/drilling/operations/old/reports"})Assert.Equal(HttpStatusCode.NotFound,(await host.Client.PostAsync(route,System.Net.Http.Json.JsonContent.Create(new{WellUWI=f.Well.UWI}))).StatusCode);
        host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.PostAsync("/api/drilling/programs/drafts/recover",System.Net.Http.Json.JsonContent.Create(original))).StatusCode);Assert.Equal(1,f.Store.Store.Native.Writes);
    }
    [Fact]public async Task ProtectedProgramReviewReadsExactSavedSourceAndActualIndependentCompanyRecipientsWithoutWrites()
    {
        using var f=new Fixture();await f.Prepare();var db=f.Funding.Routing.Afe.Db;foreach(var permission in new[]{"Drilling.View","Drilling.Approve"})db.RoleClaims.Add(new(){RoleId="investment-role",ClaimType="permission",ClaimValue=permission});await db.SaveChangesAsync();await f.Select();
        var definition=CompanyInventorySourceReview.Copy(f.Funding.Routing.Definition);definition.PROCESS_DEFINITION_ID="recorded-program-review";definition.PROCESS_TYPE="DRILLING";definition.ENTITY_TYPE="DRILLING_PROGRAM";definition.PROCESS_NAME="Independent drilling program review";f.Funding.Routing.Catalog.Workflow.Native.Rows.Add(definition);
        var basis=await f.Basis();var command=f.Store.Command with{Basis=basis,IntentId=Guid.NewGuid()};var saved=await f.Build().RecordAsync(f.Funding.Routing.Afe.Receiver.User,command);var lifecycle=System.Text.Json.JsonSerializer.Serialize(f.Funding.Routing.Catalog.Workflow.Native.Rows);var writes=f.Funding.Routing.Afe.Native.Native.Writes;
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,_)=>{services.AddScoped<CompanyDrillingProgramService>(p=>f.Build(p.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()));services.AddSingleton(Mock.Of<IFailureReporter>());});host.Client.DefaultRequestHeaders.Add("X-Test-Actor","accountant");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","FinanceRole");
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();var access=f.Funding.Routing.Afe.Receiver.Tenant.Company!;company.TrySelect(company.Clear(),"accountant",new(access.Scope,access.Roles.ToArray(),access.Permissions.ToArray()));var client=new Beep.OilandGas.Web.Services.DrillingServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company));
        var row=saved.Recorded!;var review=await client.ReviewProgramRoutingAsync(f.Funding.Routing.Afe.Process,command.Basis.Funding.Source.AfeId,f.Well.UWI,row.PROGRAM_ID,row.PROGRAM_DRAFT_REVISION);Assert.Equal(row.PROGRAM_ID,review.Source.Program.PROGRAM_ID);Assert.Equal(row.PROGRAM_DRAFT_REVISION,review.Source.Program.PROGRAM_DRAFT_REVISION);Assert.Equal("DRAFT",review.Source.Program.PROGRAM_STATUS);var option=Assert.Single(review.Options);Assert.True(option.Ready);Assert.Equal("investor",Assert.Single(Assert.Single(option.Stages).Authority.Recipients).UserId);Assert.Equal("Independent investment review",option.Stages[0].Definition.StepName);
        var path="/api/drilling/programs/review?processId="+Uri.EscapeDataString(f.Funding.Routing.Afe.Process)+"&afeId="+Uri.EscapeDataString(command.Basis.Funding.Source.AfeId)+"&wellUwi="+Uri.EscapeDataString(f.Well.UWI)+"&programId="+Uri.EscapeDataString(row.PROGRAM_ID)+"&programRevision="+row.PROGRAM_DRAFT_REVISION;
        Assert.Equal(HttpStatusCode.Conflict,(await host.Client.GetAsync(path.Replace(row.PROGRAM_DRAFT_REVISION,new string('F',64),StringComparison.Ordinal))).StatusCode);
        var approval=await db.RoleClaims.SingleAsync(claim=>claim.RoleId=="investment-role" && claim.ClaimValue=="Drilling.Approve");db.Remove(approval);await db.SaveChangesAsync();Assert.False(Assert.Single((await client.ReviewProgramRoutingAsync(f.Funding.Routing.Afe.Process,command.Basis.Funding.Source.AfeId,f.Well.UWI,row.PROGRAM_ID,row.PROGRAM_DRAFT_REVISION)).Options).Ready);approval.Id=0;db.RoleClaims.Add(approval);await db.SaveChangesAsync();
        f.Funding.Routing.Catalog.Workflow.Native.Rows.Remove(definition);Assert.Empty((await client.ReviewProgramRoutingAsync(f.Funding.Routing.Afe.Process,command.Basis.Funding.Source.AfeId,f.Well.UWI,row.PROGRAM_ID,row.PROGRAM_DRAFT_REVISION)).Options);f.Funding.Routing.Catalog.Workflow.Native.Rows.Add(definition);
        var originalVersion=definition.VERSION;var accounts=new Beep.OilandGas.Repository.RepositoryAccessService(db);var changed=new Mock<Beep.OilandGas.Repository.IRepositoryAccessService>(MockBehavior.Strict);var lookups=0;
        changed.Setup(value=>value.GetCompanyAccessAsync(It.IsAny<string>(),It.IsAny<CompanyScope>(),It.IsAny<CancellationToken>())).Returns((string actor,CompanyScope scope,CancellationToken token)=>accounts.GetCompanyAccessAsync(actor,scope,token));
        changed.Setup(value=>value.GetCompanyRoleRecipientsAsync(It.IsAny<CompanyScope>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns((CompanyScope scope,string role,CancellationToken token)=>{if(++lookups==1)definition.VERSION="changed-during-review";return accounts.GetCompanyRoleRecipientsAsync(scope,role,token);});f.RecipientAccounts=changed.Object;Assert.Equal(HttpStatusCode.Conflict,(await host.Client.GetAsync(path)).StatusCode);Assert.Equal(2,lookups);definition.VERSION=originalVersion;f.RecipientAccounts=null;

        db.Remove(await db.RoleClaims.SingleAsync(claim=>claim.RoleId=="finance-role" && claim.ClaimValue=="Drilling.Create"));await db.SaveChangesAsync();await f.Select();Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.GetAsync(path)).StatusCode);host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");Assert.Equal(HttpStatusCode.Unauthorized,(await host.Client.GetAsync(path)).StatusCode);
        Assert.Equal(lifecycle,System.Text.Json.JsonSerializer.Serialize(f.Funding.Routing.Catalog.Workflow.Native.Rows));Assert.Equal(writes,f.Funding.Routing.Afe.Native.Native.Writes);Assert.Equal(1,f.Store.Store.Native.Writes);Assert.Null(f.Well.SPUD_DATE);
    }
    [Fact]public async Task RetiredOperationAndReportRoutesHaveNoHttpAdmission()
    {
        await using var host=await AccountingSourceHttpTests.Start();
        foreach(var route in new[]{"/api/drillingoperation/operations","/api/drillingoperation/operations/old/reports","/api/field/current/drilling/operations","/api/field/current/drilling/operations/old/reports"})
            Assert.Equal(HttpStatusCode.NotFound,(await host.Client.PostAsync(route,System.Net.Http.Json.JsonContent.Create(new{WellUWI="well"}))).StatusCode);
    }
    [Fact]public async Task WellMembershipResourceAndCurrentDrillingAuthorityCannotBeBorrowedFromFundingApproval()
    {
        using var f=new Fixture();await f.Prepare();Assert.NotNull(await f.Basis());var writes=f.Store.Store.Native.Writes;f.Well.ASSIGNED_FIELD="foreign-field";await Assert.ThrowsAnyAsync<Exception>(()=>f.Basis());f.Well.ASSIGNED_FIELD="field";f.Well.ACTIVE_IND="N";await Assert.ThrowsAnyAsync<Exception>(()=>f.Basis());f.Well.ACTIVE_IND="Y";f.WellAccess=false;await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Basis());f.WellAccess=true;
        var db=f.Funding.Routing.Afe.Db;db.Remove(await db.RoleClaims.SingleAsync(r=>r.RoleId=="finance-role" && r.ClaimValue=="Drilling.View"));await db.SaveChangesAsync();await f.Select();await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Basis());Assert.Equal(writes,f.Store.Store.Native.Writes);Assert.Equal(2,f.Funding.Routing.Afe.Native.Native.Writes);
    }
}
#endif
