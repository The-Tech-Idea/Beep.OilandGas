#if WORKFLOW_INBOX_FOCUSED
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentAssignedReviewTests
{
    internal sealed class FixedHttp : IHttpContextAccessor { public HttpContext? HttpContext{get;set;} }
    internal sealed class Fixture : IDisposable
    {
        internal DevelopmentReviewHandoffTests.Fixture Sender=new();
        internal RepositoryAccessService Accounts=null!;
        internal RepositoryTenantContext Tenant=null!;
        internal RepositoryPersonaService Workspaces=null!;
        internal CompanyModuleConnection Company=null!;
        internal ModuleConnectionResolver Modules=null!;
        internal RepositoryAssetAccessStore Grants=null!;
        internal Mock<IAccessControlService> Resources=new();
        internal ClaimsPrincipal User=new(new ClaimsIdentity([new Claim("party_id","reviewer"),new Claim(ClaimTypes.Role,"ConfiguredReviewRole")],"fixture"));
        internal FixedHttp Http=new();
        internal bool FieldAccess=true;
        internal int FieldReads;
        internal Action? OnFieldRead;
        internal BoundProcessService Processes=null!;
        internal DevelopmentAssignedReviewService Service=null!;
        internal async Task Prepare(int stages=1)
        {
            await Sender.Prepare(stages);await Sender.Start();await SelectReceiver();
        }
        internal async Task SelectReceiver(string actor="reviewer",CompanyScope? selected=null,string role="ConfiguredReviewRole")
        {
            var catalog=Sender.Catalog;
            User=new(new ClaimsIdentity([new Claim("party_id",actor),new Claim(ClaimTypes.Role,role)],"fixture"));
            Accounts=new(catalog.Draft.Db);var resolver=new RepositoryCompanyContextResolver(Accounts);Tenant=new();
            Tenant.SelectCompany((await resolver.ResolveAsync(actor,selected??catalog.Draft.Context.Company))!);
            Modules=new(catalog.Draft.Db,catalog.Draft.Drafts.Native.Editor.Object,Tenant);Company=new(Modules,Tenant,resolver);
            Workspaces=new(catalog.Draft.Db);Grants=new(catalog.Draft.Db,Company,Mock.Of<IAssetGrantTargetVerifier>());
            Processes=new(()=>Task.FromResult("lifecycle"),_=>catalog.Workflow.Service);
            Http.HttpContext=new DefaultHttpContext{User=User};
            Resources.Setup(resource=>resource.CheckAssetAccessAsync(actor,"field","FIELD","READ",null)).ReturnsAsync(()=>{
                Assert.DoesNotContain("begin",catalog.Workflow.Native.Transactions.TakeLast(1));FieldReads++;OnFieldRead?.Invoke();return new Beep.OilandGas.Models.Data.AccessCheckResponse{HasAccess=FieldAccess};});
            Service=Build(Http);
        }
        internal DevelopmentAssignedReviewService Build(IHttpContextAccessor http)=>new(new RepositoryRequestAccount(http,user=>user.FindFirst("party_id")?.Value,Accounts,null!),Company,Modules,
            Sender.Catalog.Draft.Db,Accounts,Workspaces,Grants,Resources.Object,Sender.Catalog.Draft.Drafts.Service,Processes,Sender.Catalog.Draft.Drafts.Native.Editor.Object,new DevelopmentReviewRecipientAuthority(Accounts,Workspaces,Grants));
        internal Task<DevelopmentAssignedReview> Read()=>Service.ReadAsync(User,Sender.Original.InstanceId());
        internal WorkflowTasksController Inbox(IDevelopmentAssignedReviewReader? reader=null,IDevelopmentInvestmentAssignedReader? investments=null,IDrillingProgramAssignedReviewReader? programs=null)=>new(Sender.Catalog.Draft.Db,Sender.Catalog.Workflow.Native.Router,Tenant,Accounts,Workspaces,
            Resources.Object,Processes,null!,reader??Service,investments??new Mock<IDevelopmentInvestmentAssignedReader>(MockBehavior.Strict).Object,programs??new Mock<IDrillingProgramAssignedReviewReader>(MockBehavior.Strict).Object,Mock.Of<IFailureReporter>()){ControllerContext=new(){HttpContext=new DefaultHttpContext{User=User}}};
        public void Dispose()=>Sender.Dispose();
    }
    [Fact] public async Task RecipientReadsExactRecordedSourceAndInboxWithoutChangingAnUnrelatedActiveField()
    {
        using var f=new Fixture();await f.Prepare();var db=f.Sender.Catalog.Draft.Db;
        f.Sender.Catalog.Draft.Context=f.Sender.Catalog.Draft.Context with{FieldId="unrelated-active-field",Revision="unrelated-preference"};
        var writes=f.Sender.Catalog.Workflow.Native.Writes;var review=await f.Read();review.Require(new("tenant","company"),"reviewer",f.Sender.Original.InstanceId());
        Assert.Equal("field",review.Scope.FieldId);Assert.Equal("reviewer",review.Scope.ActorUserId);Assert.Equal("planner",review.Plan.ROW_CREATED_BY);
        Assert.Equal(f.Sender.Original.Source.Revision,review.SourceRevision);Assert.Equal(writes,f.Sender.Catalog.Workflow.Native.Writes);
        var result=Assert.IsType<UnifiedInbox>(Assert.IsType<OkObjectResult>(await f.Inbox().Inbox(null,default)).Value);
        var task=Assert.Single(result.CriticalTasks.Concat(result.HighPriorityTasks).Concat(result.NormalTasks));
        Assert.Equal(review.Revision,task.DevelopmentReviewRevision);Assert.Equal("/development/fdp/assigned?processId="+Uri.EscapeDataString(review.InstanceId),task.Route);
        Assert.Equal(review.Plan.FDP_NAME,task.EntityDescription);
        Assert.Equal("Independent technical review",task.StepDisplayName);Assert.Equal(1,result.Counts.Approvals);Assert.Equal(writes,f.Sender.Catalog.Workflow.Native.Writes);
        Assert.DoesNotContain(db.ChangeTracker.Entries(),entry=>entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
    }
    [Theory][InlineData("role")][InlineData("permission")][InlineData("grant")][InlineData("persona")][InlineData("workspace")][InlineData("resource")][InlineData("source")][InlineData("definition")][InlineData("binding")][InlineData("task")]
    public async Task ChangedReviewerOrRecordedEvidenceCannotExposeAssignedSource(string fault)
    {
        using var f=new Fixture();await f.Prepare();var db=f.Sender.Catalog.Draft.Db;
        Assert.NotNull(await f.Read());
        switch(fault)
        {
            case "role":(await db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId=="reviewer")).ApprovalStatus="Revoked";break;
            case "permission":db.Remove(await db.RoleClaims.SingleAsync(row=>row.ClaimValue=="Workflow.Approve"));break;
            case "grant":(await db.Set<AppUserAssetAccess>().SingleAsync()).IsActive=false;break;
            case "persona":(await db.Set<AppPersona>().SingleAsync(row=>row.Code=="RESERVOIR_ENGINEER")).IsActive=false;break;
            case "workspace":db.Remove(await db.RoleClaims.SingleAsync(row=>row.ClaimType==RepositoryPersonaService.WorkspaceClaimType));break;
            case "resource":f.FieldAccess=false;break;
            case "source":f.Sender.Catalog.Draft.Drafts.Native.Rows.OfType<Beep.OilandGas.Models.Data.DevelopmentPlanning.FIELD_DEVELOPMENT_PLAN>().Single().FDP_NAME="changed";break;
            case "definition":f.Sender.Catalog.Workflow.Definition.VERSION="changed";break;
            case "binding":f.Sender.Catalog.Binding.ConcurrencyStamp="changed";break;
            case "task":f.Sender.Catalog.Workflow.Native.Rows.OfType<CROSS_PERSONA_TASK>().Single().TASK_STATUS="COMPLETED";break;
        }
        await db.SaveChangesAsync();await Assert.ThrowsAnyAsync<Exception>(()=>f.Read());Assert.Equal(4,f.Sender.Catalog.Workflow.Native.Writes);
    }
    [Fact] public async Task ClosingNativeSourceChangeCannotPublishEarlierAssignedReview()
    {
        using var f=new Fixture();await f.Prepare();f.OnFieldRead=()=>{if(f.FieldReads==2)f.Sender.Catalog.Draft.Drafts.Native.Rows.OfType<Beep.OilandGas.Models.Data.DevelopmentPlanning.FIELD_DEVELOPMENT_PLAN>().Single().FDP_NAME="changed after first source read";};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Read());Assert.Equal(4,f.Sender.Catalog.Workflow.Native.Writes);
    }
    [Fact] public async Task MakerCannotReceiveEvenAfterBeingGivenTheSameCurrentApprovalRoleAndWorkspace()
    {
        using var f=new Fixture();await f.Prepare();var db=f.Sender.Catalog.Draft.Db;
        db.UserRoles.Add(new(){UserId="planner",RoleId="review-role"});
        db.Add(new AppUserRoleExtension{UserId="planner",RoleId="review-role",TenantId="tenant",BusinessAssociateId="company",ApprovalStatus="Approved",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
        db.Add(new AppUserPersona{UserId="planner",TenantId="tenant",BusinessAssociateId="company",PersonaCode="RESERVOIR_ENGINEER"});await db.SaveChangesAsync();await f.SelectReceiver("planner");
        var denied=await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Read());Assert.Contains("independent recipient",denied.Message);Assert.Equal(0,f.FieldReads);
    }
    [Fact] public async Task AnotherAuthorizedCompanyCannotReadOrCountTheSamePersonaTask()
    {
        using var f=new Fixture();await f.Prepare();var db=f.Sender.Catalog.Draft.Db;
        db.Add(new AppCompanyMembership{TenantId="tenant",BusinessAssociateId="foreign",UserId="reviewer",GrantedByUserId="administrator",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
        db.Add(new AppUserRoleExtension{UserId="reviewer",RoleId="review-role",TenantId="tenant",BusinessAssociateId="foreign",ApprovalStatus="Approved",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
        await db.SaveChangesAsync();await f.SelectReceiver(selected:new("tenant","foreign"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Read());
        var reader=new Mock<IDevelopmentAssignedReviewReader>(MockBehavior.Strict);var response=Assert.IsType<UnifiedInbox>(Assert.IsType<OkObjectResult>(await f.Inbox(reader.Object).Inbox(null,default)).Value);
        Assert.Equal(0,response.Counts.TotalPending);Assert.Empty(reader.Invocations);Assert.Equal(0,f.FieldReads);
    }
    [Fact] public async Task ActualTypedInboxAndSourceHttpUseSameQualifiedNativeAssignment()
    {
        using var f=new Fixture();await f.Prepare();await using var host=await AccountingSourceHttpTests.Start(configure:(services,context)=>{
            services.AddSingleton<RepositoryDbContext>(f.Sender.Catalog.Draft.Db);services.AddSingleton<IRepositoryAccessService>(f.Accounts);services.AddSingleton(f.Tenant);services.AddSingleton(f.Workspaces);
            services.AddSingleton(f.Resources.Object);services.AddSingleton<IProcessService>(f.Processes);services.AddSingleton<ICrossPersonaTaskRouter>(f.Sender.Catalog.Workflow.Native.Router);
            services.AddSingleton<Beep.OilandGas.DevelopmentPlanning.Services.IDevelopmentPlanService>(f.Sender.Catalog.Draft.Drafts.Service);
            services.AddSingleton<IDevelopmentPlanRequestAuthority>(f.Sender.Catalog.Draft.Authority);
            services.AddScoped<IDevelopmentAssignedReviewReader>(provider=>f.Build(provider.GetRequiredService<IHttpContextAccessor>()));services.AddSingleton(Mock.Of<IFailureReporter>());
        });
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","reviewer");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredReviewRole");
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();company.TrySelect(company.Clear(),"reviewer",new(f.Tenant.Company!.Scope,f.Tenant.Company.Roles.ToArray(),f.Tenant.Company.Permissions.ToArray()));
        var api=new Beep.OilandGas.Web.Services.ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);
        var client=new Beep.OilandGas.Web.Services.DevelopmentServiceClient(api,Microsoft.Extensions.Logging.Abstractions.NullLogger<Beep.OilandGas.Web.Services.DevelopmentServiceClient>.Instance);
        var review=await client.GetAssignedDraftReviewAsync(f.Sender.Original.InstanceId());review.Require(company.Selection!.Scope,"reviewer",review.InstanceId);
        var inbox=await new Beep.OilandGas.Web.Services.UnifiedTaskInboxService(api).GetInboxAsync("RESERVOIR_ENGINEER");
        Assert.Equal(review.Revision,Assert.Single(inbox.CriticalTasks.Concat(inbox.HighPriorityTasks).Concat(inbox.NormalTasks)).DevelopmentReviewRevision);Assert.Equal(4,f.Sender.Catalog.Workflow.Native.Writes);
    }
}
#endif
