#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanySalesReceivableWorkflowTests
{
    private static readonly string[] AllGrants=["Accounting.View","Accounting.PrepareSalesReceivable","Accounting.ReviewSalesReceivable","Accounting.RecordSalesReceivableDecision","Workflow.Start","Workflow.Approve","Workflow.Execute"];
    internal sealed class Fixture
    {
        public readonly CompanySalesReceivableDecisionTests.Fixture Data=new();
        public readonly CompanyProcessStartTests.Fixture Workflow=new("ConfiguredCompanyRole");
        public int ProductionReads,LifecycleReads;
        public bool DenyFieldWrite;
        public async Task Prepare(bool rejection=false)
        {
            await Data.Prepare();Workflow.Definition.ENTITY_TYPE="AR_INVOICE";
            var steps=new List<ProcessStepDefinition>{new(){StepId="review",StepName="Independent receivable financial review",SequenceNumber=1,RequiredRoles=["ConfiguredCompanyRole"],RequiresApproval=true,StepType="APPROVAL",StepConfiguration=new(){[CompanyAccountingStepEvidence.ConfigurationKey]=CompanyAccountingSourceTypes.ReceivableApproval}}};
            if(rejection)steps.Add(new(){StepId="resolve",StepName="Original receivable maker rejection review",SequenceNumber=2,RequiredRoles=["ConfiguredCompanyRole"],RequiresApproval=false,StepType="ACTION",StepConfiguration=new(){[CompanyAccountingStepEvidence.ConfigurationKey]=CompanyAccountingSourceTypes.ReceivableRejectionReview}});
            Workflow.Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=steps});CompanyStepCommandTests.ConfigureUpdates(Workflow);
            var production=Data.Data.Invoice.Data.Data.Native;var lifecycle=Workflow.Native;
            production.Source.Setup(value=>value.GetEntity(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).Returns((string table,List<AppFilter> filters)=>{Assert.Null(((ITaskTransactionEvidence)lifecycle.Source.Object).ActiveTransaction);ProductionReads++;return Rows(production.Rows,table,filters);});
            production.Source.Setup(value=>value.GetEntityAsync("REVENUE_TRANSACTION",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{Assert.Null(((ITaskTransactionEvidence)lifecycle.Source.Object).ActiveTransaction);ProductionReads++;return Rows(production.Rows,table,filters.Where(filter=>filter.FieldName is "FIELD_ID" or "PROPERTY_ID").ToList());});
            lifecycle.Source.Setup(value=>value.GetEntity(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).Returns((string table,List<AppFilter> filters)=>{Assert.Null(((ITaskTransactionEvidence)production.Source.Object).ActiveTransaction);LifecycleReads++;return Rows(lifecycle.Rows,table,filters);});
            lifecycle.Source.Setup(value=>value.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{Assert.Null(((ITaskTransactionEvidence)production.Source.Object).ActiveTransaction);LifecycleReads++;return Rows(lifecycle.Rows,table,filters);});
            Data.Data.Invoice.Data.Assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),Data.Command.Review.FieldId,"FIELD","WRITE",null)).ReturnsAsync(()=>new AccessCheckResponse{HasAccess=!DenyFieldWrite});
        }
        private static object[] Rows(List<object> rows,string table,List<AppFilter> filters)=>rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();
        public async Task<AccountingSourceHttpTests.Host> Host(string[]? grants=null)
        {
            var host=await Data.Data.Invoice.Data.Start(permissions:grants ?? AllGrants,configure:(services,context)=>{context.Actor="reviewer";services.AddSingleton<IARService>(Data.Data.Owner);services.AddScoped<CompanySalesReceivableReads>();});host.Context.FixedProcess=Workflow.Service;host.Context.StartBinding=new string('B',64);Actor(host,"reviewer");return host;
        }
        public CompanyClientContext Company(string actor,string[]? grants=null){var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(Data.Command.Company,["ConfiguredCompanyRole"],grants ?? AllGrants));return company;}
        public static void Actor(AccountingSourceHttpTests.Host host,string actor){host.Context.Actor=actor;host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);}
        public static BusinessProcessServiceClient Processes(AccountingSourceHttpTests.Host host,CompanyClientContext company)=>new(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        public async Task<CompanyAccountingProcessStartReceipt> Start(BusinessProcessServiceClient client){var review=await client.ReviewCompanyAccountingStartAsync(new("definition","AR_INVOICE",Data.Command.ReceivableId,Data.Original.Receipt.RECORDED_REVISION));return await client.StartCompanyAccountingAsync(review.CreateIntent(Guid.NewGuid().ToString("N")));}
        public async Task<CompanySalesReceivableOutcome> Decide(AccountingSourceHttpTests.Host host,CompanyClientContext company,string decision="APPROVE")
        {var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);var source=await client.GetCompanySalesReceivableReviewAsync(Data.Command.ReceivableId);return await client.RecordCompanySalesReceivableDecisionAsync(new(Guid.NewGuid().ToString("N"),source,decision,"CONFIRMED","Reviewed financial evidence","CONFIRMED","Reviewed source evidence","Explicit assigned receivable decision"));}
    }
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task ActualNativeReceivableHandoffDecisionAndMakerRejectionReviewUseSeparateOwners(string decision)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare(decision=="REJECT");await using var host=await f.Host();using var maker=f.Company("reviewer");var makerClient=Fixture.Processes(host,maker);var start=await f.Start(makerClient);
        Assert.Equal("AR_INVOICE",start.EntityType);Assert.All(f.Workflow.Native.Rows.OfType<CROSS_PERSONA_TASK>(),task=>Assert.Equal(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",start.InstanceId,"AR_INVOICE"),task.ROUTE));
        Fixture.Actor(host,"controller");using var reviewer=f.Company("controller");var client=Fixture.Processes(host,reviewer);var before=(await client.ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.False(before.Eligible);Assert.Equal("SALES_RECEIVABLE_APPROVAL_REQUIRED",before.Eligibility);Assert.Equal("reviewer",before.SourceMakerUserId);
        var native=await f.Decide(host,reviewer,decision);var reviewed=(await client.ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.Equal(decision=="APPROVE"?"ELIGIBLE":"SALES_RECEIVABLE_REJECTION_RECORDED",reviewed.Eligibility);Assert.Equal(f.Data.Original.Receipt.RECORDED_REVISION,reviewed.SourceRevision);
        var source=await client.GetCompanySalesReceivableSourceAsync(start.InstanceId);source.Require(f.Data.Command.Company,"controller",start.InstanceId);Assert.Equal(native.Receivable.STATUS,source.Receivable.Original.Receivable.STATUS);
        var command=new CompanyAccountingStepCommand(reviewed,Guid.NewGuid().ToString("N"),"Observe original independent receivable decision",decision=="REJECT"?CompanyAccountingSourceTypes.RouteReceivableRejection:null);
        int? uncertainWrites=null;if(decision=="APPROVE"){f.Workflow.Native.FailCommit=true;var failure=await Assert.ThrowsAsync<OilGasApiException>(()=>client.CompleteCompanyAccountingStepAsync(command));Assert.Equal(HttpStatusCode.Conflict,failure.StatusCode);Assert.Equal(ProcessStatus.COMPLETED,(await f.Workflow.Service.GetProcessInstanceAsync(start.InstanceId))!.Status);uncertainWrites=f.Workflow.Native.Writes;f.Workflow.Native.FailCommit=false;}
        var receipt=await client.CompleteCompanyAccountingStepAsync(command);receipt.Require(command);var writes=f.Workflow.Native.Writes;if(uncertainWrites is {} committed)Assert.Equal(committed,writes);
        Assert.Equal(JsonSerializer.Serialize(receipt),JsonSerializer.Serialize(await client.CompleteCompanyAccountingStepAsync(command)));Assert.Equal(writes,f.Workflow.Native.Writes);
        if(decision=="REJECT"){
            var approval=Assert.Single(f.Workflow.Native.Rows.OfType<PROCESS_APPROVAL>());Assert.Equal("SALES_RECEIVABLE_DECISION",approval.APPROVAL_TYPE);Assert.Equal("REJECTED",approval.APPROVAL_STATUS);Fixture.Actor(host,"reviewer");var review=(await makerClient.ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.True(review.Eligible);Assert.False(review.RequiresIndependentActor);await makerClient.CompleteCompanyAccountingStepAsync(new(review,Guid.NewGuid().ToString("N"),"Original maker reviewed receivable rejection"));}
        Assert.Equal(ProcessStatus.COMPLETED,(await f.Workflow.Service.GetProcessInstanceAsync(start.InstanceId))!.Status);Assert.Equal(JsonSerializer.Serialize(f.Data.Original.Receipt),JsonSerializer.Serialize(native.Receipt));Assert.Equal(2,f.Data.Data.Invoice.Data.Data.Native.Writes);Assert.True(f.ProductionReads>0);Assert.True(f.LifecycleReads>0);
    }
    [Theory][InlineData("grant")][InlineData("field")][InlineData("receipt")][InlineData("revision")]
    public async Task HandoffRefusesOtherReadersAndMissingOrChangedNativeEvidence(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();await using var host=await f.Host(fault=="grant"?["Accounting.View","Workflow.Start"]:AllGrants);Fixture.Actor(host,"controller");if(fault=="field")f.Data.Data.Invoice.Data.AssetDenied=true;if(fault=="receipt")f.Data.Data.Invoice.Data.Data.Native.Rows.RemoveAll(row=>row is SALES_RECEIVABLE_OUTCOME);
        var response=await host.Client.PostAsJsonAsync("api/accounting/processes/start-review",new CompanyAccountingProcessStartQuery("definition","AR_INVOICE",f.Data.Command.ReceivableId,fault=="revision"?new string('C',64):f.Data.Original.Receipt.RECORDED_REVISION));Assert.Equal(fault is "grant" or "field"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,response.StatusCode);Assert.Equal(0,f.Workflow.Native.Writes);
    }
    [Fact]public async Task OriginalMakerStartsWithoutReviewGrantButCannotCompleteItsOwnAssignment()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();var grants=new[]{"Accounting.View","Accounting.PrepareSalesReceivable","Workflow.Start","Workflow.Approve"};await using var host=await f.Host(grants);using var company=f.Company("reviewer",grants);var client=Fixture.Processes(host,company);var start=await f.Start(client);
        var observed=(await client.ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.False(observed.Eligible);Assert.Equal("ROLE_OR_INDEPENDENCE_REQUIRED",observed.Eligibility);Assert.Equal(0,f.Data.Data.Invoice.Data.Data.Native.Writes);
        var found=await client.DiscoverNativeDecisionHandoffAsync(new("AR_INVOICE",start.EntityId,f.Data.Original.Receipt.RECORDED_REVISION));Assert.Equal(start.InstanceId,found.Start!.InstanceId());
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);services.AddScoped<CompanySalesReceivableWorkspaceState>();services.AddScoped<CompanyAccountingHandoffState>();services.AddSingleton<IBusinessProcessServiceClient>(client);services.AddSingleton<IAccountingServiceClient>(CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company));services.AddSingleton<AuthenticationStateProvider>(new Authentication("reviewer"));services.AddSingleton<NavigationManager>(new Navigation(start.InstanceId).AtProcess());services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.SalesReceivableWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"InvoiceId",f.Data.Command.Review.InvoiceId}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Handoff!;Assert.NotNull(panel);Assert.Equal("AR_INVOICE",panel.EntityType);Assert.Equal(f.Data.Original.Receipt.RECORDED_REVISION,panel.SourceRevision);await(Task)panel.GetType().GetMethod("Load",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null);await root.QuiescenceTask;Assert.Contains("existing handoff for this exact original preparation",root.ToHtmlString());Assert.Contains(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",start.InstanceId,"AR_INVOICE"),root.ToHtmlString());});Assert.Equal(0,f.Data.Data.Invoice.Data.Data.Native.Writes);
    }
    [Fact]public async Task Utf8ClonePreservesSerializedProofBytesAndDetachedMutableGraph()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();var source=f.Data.Command.Review;var before=JsonSerializer.Serialize(source);var copy=CompanyInventorySourceReview.Copy(source);Assert.Equal(before,JsonSerializer.Serialize(copy));
        copy.Original.Receivable.INVOICE_NUMBER="Different detached number";copy.Original.Receipt.ROW_CREATED_BY="different detached receipt actor";Assert.Equal(before,JsonSerializer.Serialize(source));Assert.Throws<InvalidOperationException>(()=>copy.Require(source.Company,source.ActorUserId,source.ReceivableId));
    }
    [Theory][InlineData("grant")][InlineData("field")][InlineData("source")]
    public async Task AuthorityOrSourceChangeAfterDisplayedEligibleReviewRefusesCompletionWithoutTaskMutation(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();await using var host=await f.Host();using var maker=f.Company("reviewer");var start=await f.Start(Fixture.Processes(host,maker));Fixture.Actor(host,"controller");using var selected=f.Company("controller");await f.Decide(host,selected);var client=Fixture.Processes(host,selected);
        var review=(await client.ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.True(review.Eligible);var command=new CompanyAccountingStepCommand(review,Guid.NewGuid().ToString("N"),"Observe displayed receivable decision");var before=JsonSerializer.Serialize(f.Workflow.Native.Rows);var writes=f.Workflow.Native.Writes;
        if(fault=="grant")host.Context.Permissions=AllGrants.Where(value=>value!="Accounting.RecordSalesReceivableDecision").ToArray();if(fault=="field")f.DenyFieldWrite=true;if(fault=="source")Assert.Single(f.Data.Data.Invoice.Data.Data.Native.Rows.OfType<AR_INVOICE>()).TOTAL_AMOUNT+=1;
        var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.CompleteCompanyAccountingStepAsync(command));Assert.Equal(HttpStatusCode.Conflict,error.StatusCode);Assert.Equal(before,JsonSerializer.Serialize(f.Workflow.Native.Rows));Assert.Equal(writes,f.Workflow.Native.Writes);Assert.Equal(2,f.Data.Data.Invoice.Data.Data.Native.Writes);
    }
    private sealed class Authentication(string actor="controller"):AuthenticationStateProvider
    {public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,actor),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));}
    private sealed class Components:IComponentActivator
    {public Beep.OilandGas.Web.Pages.PPDM39.Accounting.CompanyAccountingProcessDetail? Page;public Beep.OilandGas.Web.Components.Workflow.SalesReceivableReviewWorkbench? Review;public Beep.OilandGas.Web.Components.Workflow.SalesReceivableWorkbench? Workspace;public Beep.OilandGas.Web.Components.BusinessProcess.CompanyAccountingHandoffPanel? Handoff;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is Beep.OilandGas.Web.Pages.PPDM39.Accounting.CompanyAccountingProcessDetail page)Page=page;if(component is Beep.OilandGas.Web.Components.Workflow.SalesReceivableReviewWorkbench review)Review=review;if(component is Beep.OilandGas.Web.Components.Workflow.SalesReceivableWorkbench workspace)Workspace=workspace;if(component is Beep.OilandGas.Web.Components.BusinessProcess.CompanyAccountingHandoffPanel handoff)Handoff=handoff;return component;}}
    private sealed class Navigation(string id):NavigationManager
    {protected override void NavigateToCore(string uri,bool forceLoad)=>Uri=ToAbsoluteUri(uri).ToString();public Navigation AtProcess(){Initialize("http://localhost/","http://localhost/accounting/processes/"+System.Uri.EscapeDataString(id));return this;}}
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task ActualAssignedReceivablePageRecordsDecisionRefreshesSourceAndCompletesOrRoutesTask(string decision)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare(decision=="REJECT");await using var host=await f.Host();using var maker=f.Company("reviewer");var start=await f.Start(Fixture.Processes(host,maker));Fixture.Actor(host,"controller");using var reviewer=f.Company("controller");var processClient=Fixture.Processes(host,reviewer);var accounting=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,reviewer);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(reviewer);services.AddScoped<CompanyAccountingStepState>();services.AddScoped<CompanySalesReceivableWorkspaceState>();services.AddSingleton<IBusinessProcessServiceClient>(processClient);services.AddSingleton<IAccountingServiceClient>(accounting);services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<NavigationManager>(new Navigation(start.InstanceId).AtProcess());services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());var completion=provider.GetRequiredService<CompanyAccountingStepState>();
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",start.InstanceId}}));var components=(Components)provider.GetRequiredService<IComponentActivator>();var page=components.Page!;var workbench=components.Review!;var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            async Task Invoke(object component,string method,object[]? arguments=null){await (Task)component.GetType().GetMethod(method,flags)!.Invoke(component,arguments)!;typeof(ComponentBase).GetMethod("StateHasChanged",flags)!.Invoke(component,null);await root.QuiescenceTask;}
            void Field(object component,string name,string value)=>component.GetType().GetField(name,flags)!.SetValue(component,value);
            Assert.Contains("Assigned receivable financial review",root.ToHtmlString());Assert.NotNull(workbench.AssignedSource);Assert.Equal(start.InstanceId,workbench.AssignedSource!.InstanceId);await Invoke(workbench,"Load");Assert.True(provider.GetRequiredService<CompanySalesReceivableWorkspaceState>().CanRecordReview);
            Field(workbench,"_financial","CONFIRMED");Field(workbench,"_source","CONFIRMED");Field(workbench,"_financialEvidence","Explicit assigned financial evidence");Field(workbench,"_sourceEvidence","Explicit assigned source evidence");Field(workbench,"_reason","Independent assigned receivable decision");typeof(ComponentBase).GetMethod("StateHasChanged",flags)!.Invoke(workbench,null);await root.QuiescenceTask;
            var state=provider.GetRequiredService<CompanySalesReceivableWorkspaceState>();var refreshed=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            void Loaded(){var expected=decision=="APPROVE"?"APPROVED":"REJECTED";if(components.Workspace?.AssignedSource?.Receivable.Original.Receivable.STATUS==expected && !state.Busy && state.Status?.Receivables.Any(row=>row.Id==f.Data.Command.ReceivableId && row.ReviewerId=="controller" && row.Status==expected)==true)refreshed.TrySetResult();}
            state.Changed+=Loaded;try{await Invoke(workbench,"Record",[decision]);Loaded();await refreshed.Task.WaitAsync(TimeSpan.FromMinutes(2));}finally{state.Changed-=Loaded;}
            Assert.Contains("Independent reviewer controller",root.ToHtmlString());Assert.Contains(decision=="APPROVE"?"APPROVED":"REJECTED",root.ToHtmlString());await Invoke(page,"ReviewStep");Assert.Equal(decision=="APPROVE"?"ELIGIBLE":"SALES_RECEIVABLE_REJECTION_RECORDED",completion.Review!.Eligibility);
            Field(page,"_notes","Observe exact recorded assigned receivable decision");await Invoke(page,"CompleteStep");Assert.NotNull(completion.Receipt);Assert.Equal(decision=="APPROVE"?"review":"resolve",(await f.Workflow.Service.GetProcessInstanceAsync(start.InstanceId))!.CurrentStepId);Assert.Equal(2,f.Data.Data.Invoice.Data.Data.Native.Writes);
        });
    }
    [Theory][InlineData("receipt")][InlineData("review")]
    public async Task AssignedReceivableFormRefusesChangedDisplayedReceiptOrReview(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();await using var host=await f.Host();Fixture.Actor(host,"controller");using var selected=f.Company("controller");var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,selected);var source=await client.GetCompanySalesReceivableReviewAsync(f.Data.Command.ReceivableId);using var state=new CompanySalesReceivableWorkspaceState(selected);state.AttachOwner(new object());state.Admit((await new Authentication().GetAuthenticationStateAsync()).User);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadReviewAsync(f.Data.Command.ReceivableId,client,fault=="receipt"?new string('D',64):f.Data.Original.Receipt.RECORDED_REVISION,fault=="review"?new string('E',64):source.Revision));Assert.Null(state.ReviewSource);Assert.Null(state.ReviewPending);Assert.Equal(0,f.Data.Data.Invoice.Data.Data.Native.Writes);Assert.Equal(0,f.Workflow.Native.Writes);
    }
    [Fact]public async Task ActualAppRoleAndPersonaInboxShowsReceivableDecisionThenRemovesCompletedAssignment()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new Fixture();await f.Prepare();using var rbac=new WorkflowInboxHttpTests.Fixture(actor:"controller");foreach(var grant in AllGrants.Where(value=>value!="Accounting.View"))rbac.Repository.RoleClaims.Add(new(){RoleId="role",ClaimType="permission",ClaimValue=grant});rbac.Repository.SaveChanges();
        await using var host=await f.Data.Data.Invoice.Data.Start(permissions:AllGrants,configure:(services,context)=>{rbac.Configure(services,context);services.AddSingleton<IARService>(f.Data.Data.Owner);services.AddScoped<CompanySalesReceivableReads>();services.AddSingleton<IProcessService>(f.Workflow.Service);services.AddSingleton<Beep.OilandGas.LifeCycle.Services.Processes.ICrossPersonaTaskRouter>(f.Workflow.Native.Router);});host.Context.FixedProcess=f.Workflow.Service;host.Context.StartBinding=new string('B',64);Fixture.Actor(host,"reviewer");using var maker=f.Company("reviewer");var start=await f.Start(Fixture.Processes(host,maker));Fixture.Actor(host,"controller");using var reviewer=f.Company("controller");var processes=Fixture.Processes(host,reviewer);
        async Task<UnifiedInbox> Inbox(){var response=await host.Client.GetAsync("api/workflow/tasks/inbox?personaCode=ACCOUNTANT");response.EnsureSuccessStatusCode();return(await response.Content.ReadFromJsonAsync<UnifiedInbox>())!;}
        var before=await Inbox();var row=Assert.Single(before.CriticalTasks.Concat(before.HighPriorityTasks).Concat(before.NormalTasks));Assert.Equal("AR_INVOICE",row.EntityType);Assert.Equal("SALES_RECEIVABLE_APPROVAL_REQUIRED",row.AccountingStepReview!.Eligibility);Assert.Equal(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",start.InstanceId,"AR_INVOICE"),row.Route);Assert.Contains("financial/source decision",PersonaTaskInboxState.SalesReceivableMessage(row.AccountingStepReview));
        await RenderInbox(before,reviewer,"SALES_RECEIVABLE_APPROVAL_REQUIRED",row.Route);
        await f.Decide(host,reviewer);var ready=await Inbox();var task=Assert.Single(ready.CriticalTasks.Concat(ready.HighPriorityTasks).Concat(ready.NormalTasks));Assert.True(task.AccountingStepReview!.Eligible);await RenderInbox(ready,reviewer,"ELIGIBLE",task.Route);await processes.CompleteCompanyAccountingStepAsync(new(task.AccountingStepReview,Guid.NewGuid().ToString("N"),"Observe assigned native receivable decision"));Assert.Equal(0,(await Inbox()).Counts.TotalPending);Assert.Equal(2,f.Data.Data.Invoice.Data.Data.Native.Writes);
    }
    private static async Task RenderInbox(UnifiedInbox response,CompanyClientContext company,string eligibility,string route)
    {
        // The response above comes from actual app RBAC and native process/source owners.
        // Freeze that transport result while verifying shared landing-state filters and rendering.
        var transport=new Mock<IUnifiedTaskInboxService>(MockBehavior.Strict);transport.Setup(value=>value.GetInboxAsync("ACCOUNTANT")).ReturnsAsync(CompanyInventorySourceReview.Copy(response));
        var personas=new Mock<IPersonaContextService>(MockBehavior.Strict);personas.Setup(value=>value.EnsureLoadedAsync()).Returns(Task.CompletedTask);personas.SetupGet(value=>value.CurrentPersona).Returns(new AppPersona{Code="ACCOUNTANT",Name="Accountant",DefaultRoute="/accounting/dashboard"});personas.SetupGet(value=>value.CurrentProfile).Returns(new AppUserPersona{UserId="controller",PersonaCode="ACCOUNTANT"});
        using var state=new PersonaTaskInboxState(transport.Object,company,personas.Object);await state.LoadAsync(["/accounting/dashboard"]);state.SelectSource("AR_INVOICE");state.SelectRole("ConfiguredCompanyRole");state.SelectAccountingEvidence(eligibility);Assert.Single(state.VisibleRows);Assert.Equal(1,state.EvidenceCount(eligibility));Assert.Equal(1,state.SourceCount("AR_INVOICE"));
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager>(new Navigation(response.NormalTasks.Concat(response.HighPriorityTasks).Concat(response.CriticalTasks).First().WorkflowName).AtProcess());await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{var rendered=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.PersonaAssignedWorkPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state},{"WorkspaceName","Accountant"}}));var html=rendered.ToHtmlString();Assert.Contains("Receivable review assignments: 1",html);Assert.Contains(route,html);Assert.Contains(eligibility=="ELIGIBLE"?"recorded receivable approval":"financial/source decision",html);});
        state.SelectSource("INVOICE");Assert.Empty(state.VisibleRows);Assert.Equal(0,state.SelectedCounts.TotalPending);
    }
}
#endif
