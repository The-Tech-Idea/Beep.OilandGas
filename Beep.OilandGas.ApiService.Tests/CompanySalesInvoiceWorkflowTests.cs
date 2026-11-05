#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Claims;
using System.Reflection;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanySalesInvoiceWorkflowTests
{
    private static readonly string[] AllGrants=["Accounting.View","Accounting.PrepareSalesInvoice","Accounting.ReviewSalesInvoice","Accounting.RecordSalesInvoiceDecision","Workflow.Start","Workflow.Approve","Workflow.Execute"];
    internal sealed class Fixture
    {
        public readonly CompanySalesInvoiceHttpTests.Fixture Invoice=new();
        public readonly CompanyProcessStartTests.Fixture Workflow=new("ConfiguredCompanyRole");
        public CompanySalesInvoiceDraftOutcome Original=null!;
        public int ProductionReads,LifecycleReads;
        public bool DenyFieldWrite,FailInvoice;
        public async Task Prepare(bool rejection=false)
        {
            await Invoice.Prepare("partial");
            async Task<CompanySalesInvoiceDraftReview> Maker()=>CompanySalesInvoiceDraftReview.Create(Invoice.Data.Current.Delivery,Invoice.Data.Current.Policies,await Invoice.Data.Owner.GetCompanySalesInvoicesForDeliveryAsync(Invoice.Data.Current.Company,Invoice.Data.Current.FieldId,Invoice.Ticket,Invoice.Data.Authority),Invoice.Data.Current.BindingRevision);
            Original=await Invoice.Data.Owner.ExecuteCompanyInvoiceDraftAsync(Invoice.Data.Command,Invoice.Data.Authority,Maker);
            Workflow.Definition.ENTITY_TYPE="INVOICE";
            var steps=new List<ProcessStepDefinition>{new(){StepId="review",StepName="Independent sales invoice review",SequenceNumber=1,RequiredRoles=["ConfiguredCompanyRole"],RequiresApproval=true,StepType="APPROVAL",StepConfiguration=new(){[CompanyAccountingStepEvidence.ConfigurationKey]=CompanyAccountingSourceTypes.InvoiceApproval}}};
            if(rejection)steps.Add(new(){StepId="resolve",StepName="Original invoice maker rejection review",SequenceNumber=2,RequiredRoles=["ConfiguredCompanyRole"],RequiresApproval=false,StepType="ACTION",StepConfiguration=new(){[CompanyAccountingStepEvidence.ConfigurationKey]=CompanyAccountingSourceTypes.InvoiceRejectionReview}});
            Workflow.Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=steps});CompanyStepCommandTests.ConfigureUpdates(Workflow);
            var production=Invoice.Data.Native;var lifecycle=Workflow.Native;
            production.Source.Setup(value=>value.GetEntity(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).Returns((string table,List<AppFilter> filters)=>{
                Assert.Null(((ITaskTransactionEvidence)lifecycle.Source.Object).ActiveTransaction);ProductionReads++;return Rows(production.Rows,table,filters);});
            production.Source.Setup(value=>value.GetEntityAsync("REVENUE_TRANSACTION",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
                Assert.Null(((ITaskTransactionEvidence)lifecycle.Source.Object).ActiveTransaction);ProductionReads++;return Rows(production.Rows,table,filters.Where(filter=>filter.FieldName is "FIELD_ID" or "PROPERTY_ID").ToList());});
            lifecycle.Source.Setup(value=>value.GetEntity(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).Returns((string table,List<AppFilter> filters)=>{
                Assert.Null(((ITaskTransactionEvidence)production.Source.Object).ActiveTransaction);LifecycleReads++;return Rows(lifecycle.Rows,table,filters);});
            lifecycle.Source.Setup(value=>value.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
                Assert.Null(((ITaskTransactionEvidence)production.Source.Object).ActiveTransaction);LifecycleReads++;return Rows(lifecycle.Rows,table,filters);});
            production.Source.Setup(value=>value.InsertEntity("SALES_INVOICE_REVIEW_INTENT",It.IsAny<object>())).Returns((string _,object value)=>{production.Writes++;production.Rows.Add(CompanyInventorySourceReview.Copy((SALES_INVOICE_REVIEW_INTENT)value));return new ErrorsInfo{Flag=Errors.Ok};});
            production.Source.Setup(value=>value.UpdateEntity("INVOICE",It.IsAny<object>())).Returns((string _,object value)=>{production.Writes++;if(FailInvoice)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((INVOICE)value);production.Rows.RemoveAll(item=>item is INVOICE invoice && invoice.INVOICE_ID==row.INVOICE_ID);production.Rows.Add(row);return new ErrorsInfo{Flag=Errors.Ok};});
            Invoice.Assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),Invoice.Data.Current.FieldId,"FIELD","WRITE",null)).ReturnsAsync(()=>new AccessCheckResponse{HasAccess=!DenyFieldWrite});
        }
        private static object[] Rows(List<object> rows,string table,List<AppFilter> filters)=>rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();
        public async Task<AccountingSourceHttpTests.Host> Host(string[]? grants=null)
        {var host=await Invoice.Start(permissions:grants ?? AllGrants);host.Context.FixedProcess=Workflow.Service;host.Context.StartBinding=new string('B',64);return host;}
        public CompanyClientContext Company(string actor,string[]? grants=null)
        {var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(Invoice.Data.Current.Company,["ConfiguredCompanyRole"],grants ?? AllGrants));return company;}
        public static void Actor(AccountingSourceHttpTests.Host host,string actor){host.Context.Actor=actor;host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);}
        public static BusinessProcessServiceClient Processes(AccountingSourceHttpTests.Host host,CompanyClientContext company)=>new(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        public async Task<CompanyAccountingProcessStartReceipt> Start(BusinessProcessServiceClient client)
        {var review=await client.ReviewCompanyAccountingStartAsync(new("definition","INVOICE",Original.Invoice.INVOICE_ID,Original.Receipt.RECORDED_REVISION));return await client.StartCompanyAccountingAsync(review.CreateIntent(Guid.NewGuid().ToString("N")));}
        public async Task<CompanySalesInvoiceDraftOutcome> Decide(AccountingSourceHttpTests.Host host,CompanyClientContext company,string decision="APPROVE")
        {var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,company);var source=await client.GetCompanySalesInvoiceReviewAsync(Original.Invoice.INVOICE_ID);return await client.RecordCompanySalesInvoiceDecisionAsync(new(Guid.NewGuid().ToString("N"),source,decision,"CONFIRMED","Reviewed tax evidence","CONFIRMED","Reviewed charges evidence","CONFIRMED","Reviewed entitlement evidence","Explicit assigned invoice decision"));}
    }
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task ActualNativeInvoiceHandoffDecisionObservationAndMakerRejectionReviewUseSeparateOwners(string decision)
    {
        var f=new Fixture();await f.Prepare(decision=="REJECT");await using var host=await f.Host();using var maker=f.Company(f.Invoice.Actor);var makerClient=Fixture.Processes(host,maker);var start=await f.Start(makerClient);
        Assert.Equal("INVOICE",start.EntityType);Assert.All(f.Workflow.Native.Rows.OfType<CROSS_PERSONA_TASK>(),task=>Assert.Equal(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",start.InstanceId,"INVOICE"),task.ROUTE));
        Fixture.Actor(host,"reviewer");using var selected=f.Company("reviewer");var client=Fixture.Processes(host,selected);var before=(await client.ReviewCompanyAccountingStepAsync(start.InstanceId))!;
        Assert.False(before.Eligible);Assert.Equal("SALES_INVOICE_APPROVAL_REQUIRED",before.Eligibility);Assert.Equal(f.Invoice.Actor,before.SourceMakerUserId);
        var native=await f.Decide(host,selected,decision);var reviewed=(await client.ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.Equal(decision=="APPROVE"?"ELIGIBLE":"SALES_INVOICE_REJECTION_RECORDED",reviewed.Eligibility);
        Assert.Equal(start.EntityId,native.Invoice.INVOICE_ID);Assert.Equal(f.Original.Receipt.RECORDED_REVISION,reviewed.SourceRevision);
        var source=await client.GetCompanySalesInvoiceSourceAsync(start.InstanceId);source.Require(f.Invoice.Data.Current.Company,"reviewer",start.InstanceId);Assert.Equal(native.Invoice.STATUS,source.Invoice.Original.Invoice.STATUS);
        var command=new CompanyAccountingStepCommand(reviewed,Guid.NewGuid().ToString("N"),"Observe recorded independent invoice decision",decision=="REJECT"?CompanyAccountingSourceTypes.RouteInvoiceRejection:null);
        var receipt=await client.CompleteCompanyAccountingStepAsync(command);receipt.Require(command);var writes=f.Workflow.Native.Writes;var replay=await client.CompleteCompanyAccountingStepAsync(command);Assert.Equal(JsonSerializer.Serialize(receipt),JsonSerializer.Serialize(replay));Assert.Equal(writes,f.Workflow.Native.Writes);
        if(decision=="REJECT")
        {
            var approval=Assert.Single(f.Workflow.Native.Rows.OfType<PROCESS_APPROVAL>());Assert.Equal("SALES_INVOICE_DECISION",approval.APPROVAL_TYPE);Assert.Equal("REJECTED",approval.APPROVAL_STATUS);
            Fixture.Actor(host,f.Invoice.Actor);var review=(await makerClient.ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.True(review.Eligible);Assert.False(review.RequiresIndependentActor);Assert.Equal("Workflow.Execute",review.RequiredPermission);
            await makerClient.CompleteCompanyAccountingStepAsync(new(review,Guid.NewGuid().ToString("N"),"Original maker reviewed explicit invoice rejection"));
        }
        var process=await f.Workflow.Service.GetProcessInstanceAsync(start.InstanceId);Assert.Equal(ProcessStatus.COMPLETED,process!.Status);if(decision=="REJECT")Assert.Equal("REJECTION_REVIEWED",process.CurrentState);
        Assert.Equal(decision=="APPROVE"?"APPROVED":"REJECTED",Assert.Single(f.Invoice.Data.Native.Rows.OfType<INVOICE>()).STATUS);Assert.Equal(5,f.Invoice.Data.Native.Writes);Assert.True(f.ProductionReads>0);Assert.True(f.LifecycleReads>0);
    }
    [Theory][InlineData("grant")][InlineData("field")][InlineData("receipt")][InlineData("revision")]
    public async Task StartSourceRefusesOtherReadersWithoutReviewGrantOrMissingChangedNativeEvidence(string fault)
    {
        var f=new Fixture();await f.Prepare();await using var host=await f.Host(fault=="grant"?["Accounting.View","Workflow.Start"]:AllGrants);Fixture.Actor(host,"reviewer");if(fault=="field")f.Invoice.AssetDenied=true;if(fault=="receipt")f.Invoice.Data.Native.Rows.RemoveAll(row=>row is SALES_INVOICE_OUTCOME);
        var response=await host.Client.PostAsJsonAsync("api/accounting/processes/start-review",new CompanyAccountingProcessStartQuery("definition","INVOICE",f.Original.Invoice.INVOICE_ID,fault=="revision"?new string('C',64):f.Original.Receipt.RECORDED_REVISION));
        Assert.Equal(fault is "grant" or "field"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,response.StatusCode);Assert.Equal(0,f.Workflow.Native.Writes);
    }
    [Fact]public async Task OriginalMakerCanHandOffWithoutPrivateReviewGrantAndDiscoveryRecoversExactExistingProcess()
    {
        var grants=new[]{"Accounting.View","Accounting.PrepareSalesInvoice","Workflow.Start"};var f=new Fixture();await f.Prepare();await using var host=await f.Host(grants);using var company=f.Company(f.Invoice.Actor,grants);var client=Fixture.Processes(host,company);
        var query=new CompanyAccountingProcessStartCatalogQuery("INVOICE",f.Original.Invoice.INVOICE_ID,f.Original.Receipt.RECORDED_REVISION);var absent=await client.DiscoverNativeDecisionHandoffAsync(query);Assert.Null(absent.Start);
        var catalog=await client.GetCompanyAccountingStartCatalogAsync(query);Assert.Single(catalog.Choices);var start=await f.Start(client);var existing=await client.DiscoverNativeDecisionHandoffAsync(query);Assert.Equal(start.InstanceId,existing.Start!.InstanceId());Assert.Equal(5,f.Workflow.Native.Writes);
    }
    [Theory][InlineData("maker")][InlineData("other-reviewer")][InlineData("grant")][InlineData("field")]
    public async Task ObservationRequiresCurrentIndependentAssignedReviewerAndDecisionAuthority(string fault)
    {
        var f=new Fixture();await f.Prepare();await using var host=await f.Host();using var maker=f.Company(f.Invoice.Actor);var start=await f.Start(Fixture.Processes(host,maker));Fixture.Actor(host,"reviewer");using var reviewer=f.Company("reviewer");await f.Decide(host,reviewer);
        var actor=fault=="maker"?f.Invoice.Actor:fault=="other-reviewer"?"other-reviewer":"reviewer";Fixture.Actor(host,actor);if(fault=="grant")host.Context.Permissions=AllGrants.Where(value=>value!="Accounting.RecordSalesInvoiceDecision").ToArray();if(fault=="field")f.DenyFieldWrite=true;
        using var selected=f.Company(actor);var review=(await Fixture.Processes(host,selected).ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.False(review.Eligible);Assert.Equal(fault switch{"maker"=>"ROLE_OR_INDEPENDENCE_REQUIRED","other-reviewer"=>"SALES_INVOICE_REVIEWER_REQUIRED","grant"=>"SALES_INVOICE_DECISION_PERMISSION_REQUIRED",_=>"SALES_INVOICE_FIELD_WRITE_REQUIRED"},review.Eligibility);Assert.Equal(5,f.Workflow.Native.Writes);
    }
    [Fact] public async Task UncertainObservedCompletionRetainsExactCommandAndReconcilesWithoutAnotherNativeInvoiceOrTaskWrite()
    {
        var f=new Fixture();await f.Prepare();await using var host=await f.Host();using var maker=f.Company(f.Invoice.Actor);var start=await f.Start(Fixture.Processes(host,maker));Fixture.Actor(host,"reviewer");using var reviewer=f.Company("reviewer");await f.Decide(host,reviewer);var client=Fixture.Processes(host,reviewer);
        var review=(await client.ReviewCompanyAccountingStepAsync(start.InstanceId))!;var command=new CompanyAccountingStepCommand(review,Guid.NewGuid().ToString("N"),"Exact original invoice observation");
        f.Workflow.Native.FailCommit=true;var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.CompleteCompanyAccountingStepAsync(command));Assert.Equal(HttpStatusCode.Conflict,error.StatusCode);
        Assert.Equal("APPROVED",Assert.Single(f.Invoice.Data.Native.Rows.OfType<INVOICE>()).STATUS);Assert.Equal(ProcessStatus.COMPLETED,(await f.Workflow.Service.GetProcessInstanceAsync(start.InstanceId))!.Status);var writes=f.Workflow.Native.Writes;
        f.Workflow.Native.FailCommit=false;var receipt=await client.CompleteCompanyAccountingStepAsync(command);receipt.Require(command);Assert.Equal(writes,f.Workflow.Native.Writes);Assert.Equal(5,f.Invoice.Data.Native.Writes);
        await Assert.ThrowsAsync<OilGasApiException>(()=>client.CompleteCompanyAccountingStepAsync(command with{Notes="Changed observation cannot adopt the original outcome"}));Assert.Equal(writes,f.Workflow.Native.Writes);
    }
    [Theory][InlineData("grant")][InlineData("field")][InlineData("source")]
    public async Task AuthorityOrSourceChangeAfterDisplayedEligibleReviewRefusesCompletionWithoutTaskMutation(string fault)
    {
        var f=new Fixture();await f.Prepare();await using var host=await f.Host();using var maker=f.Company(f.Invoice.Actor);var start=await f.Start(Fixture.Processes(host,maker));Fixture.Actor(host,"reviewer");using var reviewer=f.Company("reviewer");await f.Decide(host,reviewer);var client=Fixture.Processes(host,reviewer);
        var review=(await client.ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.True(review.Eligible);var command=new CompanyAccountingStepCommand(review,Guid.NewGuid().ToString("N"),"Observe displayed invoice decision");var before=JsonSerializer.Serialize(f.Workflow.Native.Rows);
        if(fault=="grant")host.Context.Permissions=AllGrants.Where(value=>value!="Accounting.RecordSalesInvoiceDecision").ToArray();if(fault=="field")f.DenyFieldWrite=true;if(fault=="source")Assert.Single(f.Invoice.Data.Native.Rows.OfType<INVOICE>()).TOTAL_AMOUNT+=1;
        var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.CompleteCompanyAccountingStepAsync(command));Assert.Equal(HttpStatusCode.Conflict,error.StatusCode);Assert.Equal(before,JsonSerializer.Serialize(f.Workflow.Native.Rows));Assert.Equal(5,f.Workflow.Native.Writes);
    }
    [Fact] public async Task RecordedRejectionRequiresExplicitReturnConfigurationAndCannotStartANewApprovalProcess()
    {
        var f=new Fixture();await f.Prepare();await using var host=await f.Host();using var maker=f.Company(f.Invoice.Actor);var start=await f.Start(Fixture.Processes(host,maker));Fixture.Actor(host,"reviewer");using var reviewer=f.Company("reviewer");await f.Decide(host,reviewer,"REJECT");var client=Fixture.Processes(host,reviewer);
        var review=(await client.ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.Equal("SALES_INVOICE_RETURN_CONFIGURATION_REQUIRED",review.Eligibility);Assert.False(review.Eligible);
        await Assert.ThrowsAsync<OilGasApiException>(()=>client.ReviewCompanyAccountingStartAsync(new("definition","INVOICE",f.Original.Invoice.INVOICE_ID,f.Original.Receipt.RECORDED_REVISION)));Assert.Equal(5,f.Workflow.Native.Writes);Assert.Equal(5,f.Invoice.Data.Native.Writes);
    }
    [Fact] public async Task ActualAppRoleAndPersonaInboxShowsInvoiceDecisionWorkThenRemovesCompletedAssignment()
    {
        var f=new Fixture();await f.Prepare();using var rbac=new WorkflowInboxHttpTests.Fixture(actor:"reviewer");
        foreach(var grant in AllGrants.Where(value=>value!="Accounting.View"))rbac.Repository.RoleClaims.Add(new(){RoleId="role",ClaimType="permission",ClaimValue=grant});rbac.Repository.SaveChanges();
        await using var host=await f.Invoice.Start(permissions:AllGrants,configure:(services,context)=>{rbac.Configure(services,context);services.AddSingleton<Beep.OilandGas.Models.Core.Interfaces.IProcessService>(f.Workflow.Service);services.AddSingleton<Beep.OilandGas.LifeCycle.Services.Processes.ICrossPersonaTaskRouter>(f.Workflow.Native.Router);});
        host.Context.FixedProcess=f.Workflow.Service;host.Context.StartBinding=new string('B',64);using var maker=f.Company(f.Invoice.Actor);var start=await f.Start(Fixture.Processes(host,maker));Fixture.Actor(host,"reviewer");using var reviewer=f.Company("reviewer");var processes=Fixture.Processes(host,reviewer);
        async Task<UnifiedInbox> Inbox(){var response=await host.Client.GetAsync("api/workflow/tasks/inbox?personaCode=ACCOUNTANT");response.EnsureSuccessStatusCode();return(await response.Content.ReadFromJsonAsync<UnifiedInbox>())!;}
        var before=await Inbox();var row=Assert.Single(before.CriticalTasks.Concat(before.HighPriorityTasks).Concat(before.NormalTasks));Assert.Equal("INVOICE",row.EntityType);Assert.Equal("SALES_INVOICE_APPROVAL_REQUIRED",row.AccountingStepReview!.Eligibility);Assert.Equal(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",start.InstanceId,"INVOICE"),row.Route);
        await f.Decide(host,reviewer);var ready=await Inbox();var task=Assert.Single(ready.CriticalTasks.Concat(ready.HighPriorityTasks).Concat(ready.NormalTasks));Assert.True(task.AccountingStepReview!.Eligible);
        await processes.CompleteCompanyAccountingStepAsync(new(task.AccountingStepReview,Guid.NewGuid().ToString("N"),"Observe assigned independent native invoice decision"));Assert.Equal(0,(await Inbox()).Counts.TotalPending);Assert.Equal(5,f.Invoice.Data.Native.Writes);
    }
    private sealed class Authentication:AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"reviewer"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));
    }
    private sealed class Components:IComponentActivator
    {
        public Beep.OilandGas.Web.Pages.PPDM39.Accounting.CompanyAccountingProcessDetail? Page;
        public Beep.OilandGas.Web.Components.Workflow.SalesInvoiceReviewWorkbench? Invoice;
        public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is Beep.OilandGas.Web.Pages.PPDM39.Accounting.CompanyAccountingProcessDetail page)Page=page;if(component is Beep.OilandGas.Web.Components.Workflow.SalesInvoiceReviewWorkbench invoice)Invoice=invoice;return component;}
    }
    private sealed class Navigation(string id):NavigationManager
    {
        protected override void NavigateToCore(string uri,bool forceLoad)=>Uri=ToAbsoluteUri(uri).ToString();
        public NavigationManager AtProcess(){Initialize("http://localhost/","http://localhost/accounting/processes/"+System.Uri.EscapeDataString(id));return this;}
    }
    [Theory][InlineData("APPROVE")][InlineData("REJECT")][InlineData("uncertain")][InlineData("external")]
    public async Task ActualAssignedPageRecordsInvoiceDecisionRefreshesDisplayedEvidenceAndObservesOrRoutesTasks(string mode)
    {
        var f=new Fixture();await f.Prepare(mode=="REJECT");await using var host=await f.Host();using var maker=f.Company(f.Invoice.Actor);var start=await f.Start(Fixture.Processes(host,maker));Fixture.Actor(host,"reviewer");using var reviewer=f.Company("reviewer");var processClient=Fixture.Processes(host,reviewer);var accounting=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,reviewer);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(reviewer);services.AddScoped<CompanyAccountingStepState>();services.AddScoped<CompanySalesInvoiceReviewWorkspaceState>();services.AddSingleton<IBusinessProcessServiceClient>(processClient);services.AddSingleton<IAccountingServiceClient>(accounting);services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<NavigationManager>(new Navigation(start.InstanceId).AtProcess());services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());var completion=provider.GetRequiredService<CompanyAccountingStepState>();
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",start.InstanceId}}));var components=(Components)provider.GetRequiredService<IComponentActivator>();var page=components.Page!;var workbench=components.Invoice!;
            async Task Invoke(object component,string method){await (Task)component.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(component,null)!;typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(component,null);await root.QuiescenceTask;}
            void Field(object component,string name,string value)=>component.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(component,value);
            Assert.Contains("Assigned sales invoice review",root.ToHtmlString());Assert.Contains(f.Original.Receipt.RECORDED_REVISION,root.ToHtmlString());Assert.NotNull(workbench.AssignedSource);Assert.Equal(start.InstanceId,workbench.AssignedSource!.InstanceId);
            if(mode=="external")
            {
                var before=workbench.AssignedSource.ProcessRevision;await f.Decide(host,reviewer);var reviewState=provider.GetRequiredService<CompanySalesInvoiceReviewWorkspaceState>();var loaded=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                void Loaded(){if(!reviewState.Busy && reviewState.Source?.Original.Invoice.STATUS=="APPROVED")loaded.TrySetResult();}
                reviewState.Changed+=Loaded;try{await Invoke(page,"Reload");Loaded();await loaded.Task.WaitAsync(TimeSpan.FromMinutes(2));}finally{reviewState.Changed-=Loaded;}
                Assert.Equal(before,components.Invoice!.AssignedSource!.ProcessRevision);Assert.False(reviewState.CanDecide);Assert.Equal("APPROVED",reviewState.Source!.Original.Invoice.STATUS);
            }
            else
            {
                Assert.True(provider.GetRequiredService<CompanySalesInvoiceReviewWorkspaceState>().CanDecide);
                Field(workbench,"_decision",mode=="REJECT"?"REJECT":"APPROVE");foreach(var name in new[]{"_taxDisposition","_chargesDisposition","_entitlementDisposition"})Field(workbench,name,"CONFIRMED");foreach(var name in new[]{"_taxEvidence","_chargesEvidence","_entitlementEvidence"})Field(workbench,name,"Explicit assigned invoice evidence");Field(workbench,"_reason","Independent assigned invoice decision");typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(workbench,null);await root.QuiescenceTask;await Invoke(workbench,"Record");
            }
            var html=root.ToHtmlString();Assert.Contains("Recorded reviewer: reviewer",html);Assert.Contains(mode=="external"?"Explicit assigned invoice decision":"Independent assigned invoice decision",html);Assert.Equal(mode=="REJECT"?"REJECTED":"APPROVED",Assert.Single(f.Invoice.Data.Native.Rows.OfType<INVOICE>()).STATUS);
            await Invoke(page,"ReviewStep");Assert.Equal(mode=="REJECT"?"SALES_INVOICE_REJECTION_RECORDED":"ELIGIBLE",completion.Review!.Eligibility);Field(page,"_notes","Observe exact recorded assigned invoice decision");if(mode=="uncertain")f.Workflow.Native.FailCommit=true;await Invoke(page,"CompleteStep");
            if(mode=="uncertain"){Assert.True(completion.HasUnconfirmedIntent);Assert.Null(completion.Receipt);var intent=completion.Intent;var writes=f.Workflow.Native.Writes;f.Workflow.Native.FailCommit=false;await Invoke(page,"CompleteStep");Assert.Equal(intent,completion.Intent);Assert.Equal(writes,f.Workflow.Native.Writes);}
            Assert.NotNull(completion.Receipt);Assert.Equal(mode=="REJECT"?"resolve":"review",(await f.Workflow.Service.GetProcessInstanceAsync(start.InstanceId))!.CurrentStepId);Assert.Equal(5,f.Invoice.Data.Native.Writes);Assert.Empty(f.Invoice.Data.Native.Rows.OfType<JOURNAL_ENTRY>());
        });
    }
    [Theory][InlineData("receipt")][InlineData("review")]
    public async Task AssignedWorkbenchRefusesChangedDisplayedReceiptOrReviewBeforePublishingPrivateEvidence(string changed)
    {
        var f=new Fixture();await f.Prepare();await using var host=await f.Host();Fixture.Actor(host,"reviewer");using var selected=f.Company("reviewer");var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,selected);var source=await client.GetCompanySalesInvoiceReviewAsync(f.Original.Invoice.INVOICE_ID);using var state=new CompanySalesInvoiceReviewWorkspaceState(selected);state.AttachOwner(new object());state.Admit((await new Authentication().GetAuthenticationStateAsync()).User);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadAsync(f.Original.Invoice.INVOICE_ID,client,changed=="receipt"?new string('D',64):f.Original.Receipt.RECORDED_REVISION,changed=="review"?new string('E',64):source.Revision));Assert.Null(state.Source);Assert.Null(state.Pending);Assert.Equal(3,f.Invoice.Data.Native.Writes);Assert.Equal(0,f.Workflow.Native.Writes);
    }
}
#endif
