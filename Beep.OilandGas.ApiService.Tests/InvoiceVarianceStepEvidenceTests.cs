using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.LifeCycle.Services.Processes;
using System.Text.Json.Nodes;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InvoiceVarianceStepEvidenceTests
{
    internal sealed class Fixture:IAsyncDisposable
    {
        public InvoiceVarianceDecisionCommandTests.Fixture Invoice=null!;
        public CompanyProcessStartTests.Fixture Process=null!;
        public AccountingSourceHttpTests.Host Host=null!;
        public CompanyClientContext Company=new();
        public BusinessProcessServiceClient Client=null!;
        public string Id="";
        public WorkflowInboxHttpTests.Fixture? Inbox;
        public async Task Prepare(bool decided=true,string actor="reviewer",object? configured=null,bool withInbox=false,bool correctionWorkflow=false,bool acceptCorrection=false,bool unpaired=false,bool acknowledgement=false)
        {
            Invoice=new();await Invoice.Prepare(!correctionWorkflow || acceptCorrection);var n=Invoice.Existing.Existing.ReadOwner.Native;Process=new("ConfiguredControllerRole");Process.Native.Destinations=["ACCOUNTANT"];
            if(correctionWorkflow)Process.Native.Personas.Setup(value=>value.GetActivePersonasAsync("ConfiguredAccountantRole")).ReturnsAsync(()=>Process.Native.Destinations.ToList());
            var config=JsonNode.Parse(Process.Definition.PROCESS_CONFIG_JSON!)!;
            if(!correctionWorkflow)config["Steps"]!.AsArray().RemoveAt(1);
            else{var next=config["Steps"]![1]!;next["RequiredRoles"]=new JsonArray("ConfiguredAccountantRole");next["StepType"]="ACTION";next["StepName"]="Correct requested invoice";next["RequiresApproval"]=false;next["StepConfiguration"]=new JsonObject{[CompanyAccountingStepEvidence.ConfigurationKey]=CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork};}
            if(acknowledgement)
            {
                var work=config["Steps"]![1]!;var ack=work.DeepClone();work["NextStepId"]="acknowledge";
                ack["StepId"]="acknowledge";ack["StepName"]="Acknowledge latest corrected invoice";ack["SequenceNumber"]=3;ack["NextStepId"]="";
                ack["RequiredRoles"]=new JsonArray("ConfiguredControllerRole");ack["StepType"]="APPROVAL";ack["RequiresApproval"]=true;
                ack["StepConfiguration"]=new JsonObject{[CompanyAccountingStepEvidence.ConfigurationKey]=CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement};config["Steps"]!.AsArray().Add(ack);
            }
            config["Steps"]![0]!["StepType"]="APPROVAL";config["Steps"]![0]!["StepConfiguration"]=new JsonObject{[CompanyAccountingStepEvidence.ConfigurationKey]=JsonValue.Create(configured??(correctionWorkflow?CompanyAccountingStepEvidence.InvoiceVarianceCorrectionRequest:CompanyAccountingStepEvidence.InvoiceVarianceDisposition))};
            if(unpaired)config["Steps"]![1]!["StepConfiguration"]=new JsonObject();
            Process.Definition.PROCESS_CONFIG_JSON=config.ToJsonString();CompanyStepCommandTests.ConfigureUpdates(Process);
            if(withInbox)Inbox=new(assignedRole:"ConfiguredControllerRole",actor:actor);
            Host=await AccountingSourceHttpTests.Start(configure:(services,context)=>{
                Inbox?.Configure(services,context);context.FixedProcess=Process.Service;context.StartBinding=new string('B',64);
                if(Inbox is not null){services.AddSingleton<IProcessService>(Process.Service);var router=new Mock<ICrossPersonaTaskRouter>();router.Setup(value=>value.GetTasksForPersonaAsync("ACCOUNTANT")).ReturnsAsync(()=>{Inbox.OnTaskRead?.Invoke(++Inbox.TaskReads);return Process.Native.Rows.OfType<CROSS_PERSONA_TASK>().Where(row=>row.TASK_STATUS=="PENDING").Select(row=>JsonSerializer.Deserialize<CROSS_PERSONA_TASK>(JsonSerializer.Serialize(row))!).ToList();});services.AddSingleton(router.Object);}
            },configureAccounting:(services,_)=>{services.AddSingleton(Invoice.Existing.Existing.Owner);services.AddSingleton(n.AP);services.AddSingleton(n.PO);});
            Host.Context.Permissions=["Accounting.View","Procurement.View","Workflow.Start","Workflow.Approve"];
            Host.Context.Roles=["ConfiguredCompanyRole"];
            Company.TrySelect(Company.Clear(),"actor",new(Invoice.Command.Company,["ConfiguredCompanyRole"],Host.Context.Permissions));
            Client=new(new ApiClient(Host.Client,Mock.Of<IFailureReporter>(),Company),NullLogger<BusinessProcessServiceClient>.Instance);
            var source=(await n.AP.GetSourceReviewAsync(Invoice.Command.Company,"invoice","actor"))!;
            var start=await Client.ReviewCompanyAccountingStartAsync(new("definition","AP_INVOICE","invoice",source.Revision));Id=(await Client.StartCompanyAccountingAsync(start.CreateIntent(Guid.NewGuid().ToString("N")))).InstanceId;
            if(decided)await Invoice.Record();
            var role=actor=="administrator"?"Administrator":"ConfiguredControllerRole";Host.Context.Actor=actor;Host.Context.Roles=[role];
            Host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);Host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role",role);
            Company.TrySelect(Company.Clear(),actor,new(Invoice.Command.Company,[role],Host.Context.Permissions));
        }
        public async ValueTask DisposeAsync(){Company.Dispose();if(Host is not null)await Host.DisposeAsync();Inbox?.Dispose();}
    }
    [Fact] public async Task ConfiguredStepRequiresRecordedDecisionAndCompletesExactNativeTaskOnce()
    {
        await using var f=new Fixture();await f.Prepare();var review=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;
        Assert.True(review.Eligible);Assert.NotNull(review.AccountingEvidenceRevision);
        var command=new CompanyAccountingStepCommand(review,Guid.NewGuid().ToString("N"),"Completed the separately recorded independent variance disposition.");
        var receipt=await f.Client.CompleteCompanyAccountingStepAsync(command);receipt.Require(command);Assert.Single(receipt.CompletedTaskIds);Assert.Empty(receipt.NextTaskIds);
        Assert.Equal("COMPLETED",Assert.Single(f.Process.Native.Rows.OfType<CROSS_PERSONA_TASK>()).TASK_STATUS);
        var writes=f.Process.Native.Writes;var retry=await f.Client.CompleteCompanyAccountingStepAsync(command);retry.Require(command);Assert.Equal(writes,f.Process.Native.Writes);
        Assert.Equal("DRAFT",f.Invoice.Existing.Stored.STATUS);Assert.Equal(2,f.Invoice.Existing.Existing.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("missing")][InlineData("other-controller")][InlineData("administrator")]
    public async Task WorkflowRoleOrForgedEligibleReviewCannotSubstituteForRecordedController(string fault)
    {
        await using var f=new Fixture();await f.Prepare(decided:fault!="missing",actor:fault=="missing"?"reviewer":fault);
        var review=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;Assert.False(review.Eligible);Assert.Equal(fault=="missing"?"RECORDED_DISPOSITION_REQUIRED":"RECORDED_CONTROLLER_REQUIRED",review.Eligibility);
        var forged=new CompanyAccountingStepCommand(review with{Eligible=true,Eligibility="ELIGIBLE"},Guid.NewGuid().ToString("N"),"Attempt to bypass recorded controller evidence.");var writes=f.Process.Native.Writes;
        var error=await Assert.ThrowsAsync<OilGasApiException>(()=>f.Client.CompleteCompanyAccountingStepAsync(forged));Assert.Equal(HttpStatusCode.Conflict,error.StatusCode);Assert.Equal(writes,f.Process.Native.Writes);
        Assert.Equal("PENDING",Assert.Single(f.Process.Native.Rows.OfType<CROSS_PERSONA_TASK>()).TASK_STATUS);
    }
    [Theory][InlineData("hash")][InlineData("header")][InlineData("proof")][InlineData("valid-other-reason")][InlineData("grant")]
    public async Task CurrentDecisionEvidenceAndActionGrantMustStillMatchAtCompletion(string fault)
    {
        await using var f=new Fixture();await f.Prepare();var review=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;var writes=f.Process.Native.Writes;
        if(fault=="hash")review=review with{AccountingEvidenceRevision=new string('C',64)};
        if(fault=="header")f.Invoice.Existing.Stored.TOTAL_AMOUNT=99m;if(fault=="proof")f.Invoice.Existing.Stored.VARIANCE_DECISION_JSON="unverified";
        if(fault=="valid-other-reason")
        {
            var row=f.Invoice.Existing.Stored;var proof=APInvoiceVarianceDecisionProof.Require(f.Invoice.Command.Company,row);
            row.VARIANCE_DECISION_JSON=APInvoiceVarianceDecisionProof.Create(proof.Command with{Reason="Different but structurally valid controller reason."},proof.OriginalInvoice,row);
        }
        if(fault=="grant")f.Host.Context.Permissions=["Accounting.View","Procurement.View","Workflow.Start"];
        var command=new CompanyAccountingStepCommand(review,Guid.NewGuid().ToString("N"),"Complete original current controller evidence.");
        var error=await Assert.ThrowsAsync<OilGasApiException>(()=>f.Client.CompleteCompanyAccountingStepAsync(command));Assert.Equal(fault=="grant"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);Assert.Equal(writes,f.Process.Native.Writes);
    }
    [Theory][InlineData("")][InlineData("UNKNOWN")][InlineData(7)]
    public async Task UnsupportedConfiguredEvidenceCannotPublishEligibility(object configured)
    {
        await using var f=new Fixture();await f.Prepare(configured:configured);
        var error=await Assert.ThrowsAsync<OilGasApiException>(()=>f.Client.ReviewCompanyAccountingStepAsync(f.Id));Assert.Equal(HttpStatusCode.Conflict,error.StatusCode);
    }
    internal sealed class Authentication(Beep.OilandGas.Client.App.CompanyClientContext company):AuthenticationStateProvider
    {public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,company.Selection?.UserId ??""),new(ClaimTypes.Role,"ConfiguredControllerRole")],"test"))));}
    [Theory][InlineData("preparer")][InlineData("other-preparer")][InlineData("no-grant")][InlineData("accepted")][InlineData("missing")][InlineData("unpaired")]
    public async Task CorrectionRequestRoutesExactAccountantTaskAndCannotCompleteUnrecordedCorrection(string mode)
    {
        await using var f=new Fixture();await f.Prepare(decided:mode!="missing",correctionWorkflow:true,acceptCorrection:mode=="accepted",withInbox:true,unpaired:mode=="unpaired");
        var review=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;
        if(mode is "accepted" or "missing" or "unpaired")
        {Assert.Equal(mode=="unpaired",review.Eligible);Assert.Equal(mode=="accepted"?"REQUEST_CORRECTION_REQUIRED":mode=="missing"?"RECORDED_DISPOSITION_REQUIRED":"ELIGIBLE",review.Eligibility);var writes=f.Process.Native.Writes;var command=new CompanyAccountingStepCommand(review with{Eligible=true,Eligibility="ELIGIBLE"},Guid.NewGuid().ToString("N"),"Attempt correction routing without a supported requested correction chain.");var error=await Assert.ThrowsAsync<OilGasApiException>(()=>f.Client.CompleteCompanyAccountingStepAsync(command));Assert.Equal(HttpStatusCode.Conflict,error.StatusCode);Assert.Equal(writes,f.Process.Native.Writes);return;}
        Assert.True(review.Eligible);var intent=new CompanyAccountingStepCommand(review,Guid.NewGuid().ToString("N"),"Hand off the recorded controller correction request.");
        var receipt=await f.Client.CompleteCompanyAccountingStepAsync(intent);receipt.Require(intent);Assert.Single(receipt.CompletedTaskIds);Assert.Single(receipt.NextTaskIds);
        var next=Assert.Single(f.Process.Native.Rows.OfType<CROSS_PERSONA_TASK>(),row=>row.TASK_STATUS=="PENDING");Assert.Equal("ConfiguredAccountantRole",next.ASSIGNED_ROLE);Assert.Equal(receipt.NextStepInstanceId,next.PROCESS_STEP_INSTANCE_ID);Assert.Equal(receipt.NextTaskIds[0],next.CROSS_TASK_ID);
        var writesAfter=f.Process.Native.Writes;(await f.Client.CompleteCompanyAccountingStepAsync(intent)).Require(intent);Assert.Equal(writesAfter,f.Process.Native.Writes);
        var actor=mode=="other-preparer"?"other-preparer":"actor";var db=f.Inbox!.Repository;
        db.Users.Add(new(){Id=actor,IsActive=true});db.Roles.Add(new(){Id="accountant",Name="ConfiguredAccountantRole",NormalizedName="CONFIGUREDACCOUNTANTROLE"});db.UserRoles.Add(new(){UserId=actor,RoleId="accountant"});
        db.Set<AppCompanyMembership>().Add(new(){TenantId="tenant",BusinessAssociateId="company",UserId=actor,GrantedByUserId="reviewer",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
        db.Set<AppUserRoleExtension>().Add(new(){TenantId="tenant",BusinessAssociateId="company",UserId=actor,RoleId="accountant",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
        db.Set<AppUserPersona>().Add(new(){UserId=actor,TenantId="tenant",BusinessAssociateId="company",PersonaCode="ACCOUNTANT"});db.RoleClaims.Add(new(){RoleId="accountant",ClaimType=Beep.OilandGas.Repository.RepositoryPersonaService.WorkspaceClaimType,ClaimValue="ACCOUNTANT"});
        foreach(var permission in new[]{"Accounting.View","Procurement.View","Workflow.Execute"})db.RoleClaims.Add(new(){RoleId="accountant",ClaimType="permission",ClaimValue=permission});await db.SaveChangesAsync();
        f.Host.Context.Actor=actor;f.Host.Context.Roles=["ConfiguredAccountantRole"];f.Host.Context.Permissions=mode=="no-grant"?["Accounting.View","Procurement.View"]:["Accounting.View","Procurement.View","Workflow.Execute"];
        f.Host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");f.Host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);f.Host.Client.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");f.Host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredAccountantRole");
        f.Company.TrySelect(f.Company.Clear(),actor,new(f.Invoice.Command.Company,["ConfiguredAccountantRole"],f.Host.Context.Permissions));
        var correction=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;Assert.False(correction.Eligible);Assert.Equal(mode=="other-preparer"?"ORIGINAL_PREPARER_REQUIRED":mode=="no-grant"?"WORKFLOW_PERMISSION_REQUIRED":"CORRECTION_DATA_REQUIRED",correction.Eligibility);Assert.False(correction.RequiresIndependentActor);Assert.Equal("Workflow.Execute",correction.RequiredPermission);
        var inbox=await new UnifiedTaskInboxService(new ApiClient(f.Host.Client,Mock.Of<IFailureReporter>(),f.Company)).GetInboxAsync("ACCOUNTANT");Assert.Equal(correction,Assert.Single(inbox.CriticalTasks.Concat(inbox.HighPriorityTasks).Concat(inbox.NormalTasks)).AccountingStepReview);
        var personas=new Mock<IPersonaContextService>();personas.Setup(value=>value.EnsureLoadedAsync()).Returns(Task.CompletedTask);personas.SetupGet(value=>value.CurrentProfile).Returns(new AppUserPersona{UserId=actor,PersonaCode="ACCOUNTANT"});personas.SetupGet(value=>value.CurrentPersona).Returns(new AppPersona{Code="ACCOUNTANT"});
        using var state=new PersonaTaskInboxState(new UnifiedTaskInboxService(new ApiClient(f.Host.Client,Mock.Of<IFailureReporter>(),f.Company)),f.Company,personas.Object);state.Start();await state.LoadAsync();state.SelectAccountingEvidence(correction.Eligibility=="WORKFLOW_PERMISSION_REQUIRED"?"ALL":correction.Eligibility);Assert.Single(state.VisibleRows);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager>(new Navigation(f.Id));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>WebUtility.HtmlDecode((await renderer.RenderComponentAsync<Beep.OilandGas.Web.Auth.Tests.Fixtures.PersonaAssignedWorkRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state}}))).ToHtmlString()));
        Assert.Contains(mode=="preparer"?"Accountant correction needed: 1":mode=="other-preparer"?"Original preparer required: 1":"lacks the workflow action permission",html);Assert.NotNull(next.ROUTE);Assert.Contains(next.ROUTE!,html);Assert.DoesNotContain("Complete reviewed workflow step",html);
        var forged=new CompanyAccountingStepCommand(correction with{Eligible=true,Eligibility="ELIGIBLE"},Guid.NewGuid().ToString("N"),"Attempt task completion before recorded correction.");var failure=await Assert.ThrowsAsync<OilGasApiException>(()=>f.Client.CompleteCompanyAccountingStepAsync(forged));Assert.Equal(mode=="no-grant"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,failure.StatusCode);Assert.Equal(writesAfter,f.Process.Native.Writes);Assert.Equal("DRAFT",f.Invoice.Existing.Stored.STATUS);Assert.Equal(2,f.Invoice.Existing.Existing.ReadOwner.Native.Native.Writes);
        if(mode=="preparer")
        {var native=await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Process.Service.CompleteCompanyAccountingStepAsync(forged,()=>Task.CompletedTask));Assert.Contains("Recorded corrected invoice command evidence",native.Message);Assert.Equal(writesAfter,f.Process.Native.Writes);}
    }
    [Theory][InlineData("missing")][InlineData("eligible")][InlineData("other-controller")][InlineData("completed")][InlineData("changed")]
    public async Task ActualPersonaInboxPublishesCurrentConfiguredEvidenceAndClearsCompletedOrChangedWork(string mode)
    {
        await using var f=new Fixture();await f.Prepare(decided:mode!="missing",actor:mode=="other-controller"?mode:"reviewer",withInbox:true);
        if(mode=="completed")
        {var review=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;await f.Client.CompleteCompanyAccountingStepAsync(new(review,Guid.NewGuid().ToString("N"),"Confirmed configured controller work."));}
        if(mode=="changed")f.Inbox!.OnTaskRead=read=>{if(read==2)f.Invoice.Existing.Stored.VARIANCE_DECISION_JSON="changed";};
        var response=await f.Host.Client.GetAsync("/api/workflow/tasks/inbox?personaCode=ACCOUNTANT");
        if(mode=="changed"){Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.DoesNotContain("TotalPending",await response.Content.ReadAsStringAsync());return;}
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);var inbox=(await response.Content.ReadFromJsonAsync<UnifiedInbox>())!;
        var captured=WorkflowInboxReview.Capture(inbox,f.Company.Selection!.Scope,f.Company.Selection.UserId,"ACCOUNTANT",f.Company.Selection.Roles);
        var inboxClient=new UnifiedTaskInboxService(new ApiClient(f.Host.Client,Mock.Of<IFailureReporter>(),f.Company));
        Assert.Equal(captured.Counts.TotalPending,(await inboxClient.GetInboxCountsAsync("ACCOUNTANT")).TotalPending);
        Assert.Equal(captured.Counts.TotalPending,(await inboxClient.GetFilteredTasksAsync(new InboxFilter(),"ACCOUNTANT")).Count);
        if(mode=="completed"){Assert.Equal(0,captured.Counts.TotalPending);return;}
        var task=Assert.Single(captured.CriticalTasks.Concat(captured.HighPriorityTasks).Concat(captured.NormalTasks));
        Assert.Equal(mode=="missing"?"RECORDED_DISPOSITION_REQUIRED":mode=="other-controller"?"RECORDED_CONTROLLER_REQUIRED":"ELIGIBLE",task.AccountingStepReview!.Eligibility);
        Assert.Equal(f.Id,task.AccountingStepReview.InstanceId);Assert.Equal(task.StepName,task.AccountingStepReview.StepInstanceId);
        Assert.Equal(task.AccountingStepReview,await f.Client.ReviewCompanyAccountingStepAsync(f.Id));
        var personas=new Mock<IPersonaContextService>();personas.Setup(value=>value.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        personas.SetupGet(value=>value.CurrentProfile).Returns(new AppUserPersona{UserId=f.Company.Selection.UserId,PersonaCode="ACCOUNTANT"});
        personas.SetupGet(value=>value.CurrentPersona).Returns(new AppPersona{Code="ACCOUNTANT",Name="Configured accountant and controller workspace"});
        using var state=new PersonaTaskInboxState(inboxClient,f.Company,personas.Object);state.Start();await state.LoadAsync();
        var eligibility=task.AccountingStepReview.Eligibility;Assert.Equal(1,state.EvidenceCount(eligibility));state.SelectAccountingEvidence(eligibility);Assert.Single(state.VisibleRows);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager>(new Navigation(f.Id));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>WebUtility.HtmlDecode((await renderer.RenderComponentAsync<Beep.OilandGas.Web.Auth.Tests.Fixtures.PersonaAssignedWorkRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state}}))).ToHtmlString()));
        Assert.Contains("Configured accounting, commercial and delivery work",html);Assert.Contains(mode=="missing"?"Disposition needed: 1":mode=="other-controller"?"Recorded controller required: 1":"Ready for workflow completion: 1",html);
        Assert.Contains(task.Route,html);state.SelectAccountingEvidence(eligibility=="ELIGIBLE"?"RECORDED_DISPOSITION_REQUIRED":"ELIGIBLE");Assert.Empty(state.VisibleRows);
        state.SelectAccountingEvidence("ALL");Assert.Single(state.VisibleRows);
        task.AccountingStepReview=task.AccountingStepReview with{ActorUserId="another-account"};
        Assert.Throws<InvalidOperationException>(()=>WorkflowInboxReview.Capture(captured,f.Company.Selection.Scope,f.Company.Selection.UserId,"ACCOUNTANT",f.Company.Selection.Roles));
        f.Company.Clear();Assert.Null(state.Current);Assert.Equal("ALL",state.AccountingEvidence);
    }
    internal sealed class Navigation:NavigationManager
    {public Navigation(string id)=>Initialize("https://render.invalid/","https://render.invalid/accounting/processes/"+System.Uri.EscapeDataString(id));protected override void NavigateToCore(string uri,bool forceLoad)=>throw new InvalidOperationException("Static rendering has no navigation.");}
    internal sealed class Components(IServiceProvider services):IComponentActivator
    {public IComponent? Detail;public IComponent CreateInstance(Type type){var component=(IComponent)ActivatorUtilities.CreateInstance(services,type);if(type==typeof(Beep.OilandGas.Web.Pages.PPDM39.Accounting.CompanyAccountingProcessDetail))Detail=component;return component;}}
    [Theory][InlineData("missing")][InlineData("other-controller")][InlineData("eligible")]
    public async Task CompleteProcessScreenShowsDispositionRequirementAndOffersCompletionOnlyToRecordedController(string mode)
    {
        await using var f=new Fixture();await f.Prepare(decided:mode!="missing",actor:mode=="other-controller"?mode:"reviewer");
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(f.Company);services.AddScoped<CompanyAccountingStepState>();
        services.AddSingleton<IBusinessProcessServiceClient>(f.Client);services.AddSingleton<IAccountingServiceClient>(new Mock<IAccountingServiceClient>(MockBehavior.Strict).Object);services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<NavigationManager>(new Navigation(f.Id));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",f.Id}}));
            var page=((Components)provider.GetRequiredService<IComponentActivator>()).Detail!;await(Task)page.GetType().GetMethod("ReviewStep",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null)!;
            typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null);return WebUtility.HtmlDecode(root.ToHtmlString());
        });
        if(mode=="eligible"){Assert.Contains("Recorded controller disposition and current role checks",html);Assert.Contains("Complete reviewed workflow step",html);}
        else{Assert.DoesNotContain("Complete reviewed workflow step",html);Assert.Contains(mode=="missing"?"Record an independent controller variance disposition":"Only the controller account that recorded this disposition",html);}
    }
}
#endif
