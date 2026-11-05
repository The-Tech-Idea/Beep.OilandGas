using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Beep.Report;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesJournalProcessHttpTests(CompanySalesJournalPreparationTests.ApprovedSourceFixture source):IClassFixture<CompanySalesJournalPreparationTests.ApprovedSourceFixture>
{
    [Fact] public async Task ActualConfirmedPostingCanCloseOnlyItsProducerAssignmentAndPreserveExactRetry()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var posting=new CompanySalesJournalPostingTests.Fixture(source.Source);var production=posting.Reviewed.Draft.Native;
        var lifecycle=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");lifecycle.Definition.ENTITY_TYPE="JOURNAL_ENTRY";
        ProcessStepDefinition Step(string id,int sequence,string action,bool approval)=>new(){StepId=id,StepName=id,SequenceNumber=sequence,RequiredRoles=["ConfiguredCompanyRole"],RequiresApproval=approval,StepType=approval?"APPROVAL":"ACTION",StepConfiguration=new(){[CompanyAccountingStepEvidence.ConfigurationKey]=action}};
        lifecycle.Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=new[]{Step("JOURNAL_REVIEW",1,CompanyAccountingSourceTypes.Approval("JOURNAL_ENTRY"),true),Step("JOURNAL_POSTING",2,CompanyAccountingSourceTypes.JournalPosting,false),Step("JOURNAL_REJECTION_REVIEW",3,CompanyAccountingSourceTypes.RejectionReview("JOURNAL_ENTRY"),false)}});CompanyStepCommandTests.ConfigureUpdates(lifecycle);
        static object[] Rows(List<object> rows,string table,List<AppFilter> filters)=>rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();
        var productionReads=0;var lifecycleReads=0;
        production.Source.Setup(value=>value.GetEntity(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).Returns((string table,List<AppFilter> filters)=>{Assert.Null(((ITaskTransactionEvidence)lifecycle.Native.Source.Object).ActiveTransaction);productionReads++;return Rows(production.Rows,table,filters);});
        production.Source.Setup(value=>value.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{Assert.Null(((ITaskTransactionEvidence)lifecycle.Native.Source.Object).ActiveTransaction);productionReads++;return Rows(production.Rows,table,filters);});
        lifecycle.Native.Source.Setup(value=>value.GetEntity(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).Returns((string table,List<AppFilter> filters)=>{Assert.Null(((ITaskTransactionEvidence)production.Source.Object).ActiveTransaction);lifecycleReads++;return Rows(lifecycle.Native.Rows,table,filters);});
        lifecycle.Native.Source.Setup(value=>value.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{Assert.Null(((ITaskTransactionEvidence)production.Source.Object).ActiveTransaction);lifecycleReads++;return Rows(lifecycle.Native.Rows,table,filters);});
        string[] grants=["Accounting.View","Accounting.ReviewSalesJournalSource","Accounting.RecordSalesJournalDecision","Accounting.PostSalesJournal","Workflow.Start","Workflow.Approve","Workflow.Execute"];
        using var rbac=new WorkflowInboxHttpTests.Fixture(actor:"authorized-poster");foreach(var grant in grants.Where(value=>value!="Accounting.View"))rbac.Repository.RoleClaims.Add(new(){RoleId="role",ClaimType="permission",ClaimValue=grant});rbac.Repository.SaveChanges();
        await using var host=await AccountingSourceHttpTests.Start(permissions:grants,configureAccounting:(services,context)=>{rbac.Configure(services,context);context.Actor=source.Source.ActorUserId;context.PolicyBinding=source.Source.BindingRevision;context.StartBinding=source.Source.BindingRevision;context.FixedProcess=lifecycle.Service;services.AddSingleton(posting.Reviewed.Draft.Service);services.AddSingleton<IProcessService>(lifecycle.Service);services.AddSingleton<Beep.OilandGas.LifeCycle.Services.Processes.ICrossPersonaTaskRouter>(lifecycle.Native.Router);});
        host.Context.Resources.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),source.Source.FieldId,"FIELD",It.IsAny<string>(),null)).ReturnsAsync(new AccessCheckResponse{HasAccess=true});
        void Actor(string actor){host.Context.Actor=actor;host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);}
        Actor(source.Source.ActorUserId);using var maker=CompanyGLAccountHttpTests.Selected(actor:source.Source.ActorUserId,grants:grants);var makerClient=CompanySalesReceivableWorkflowTests.Fixture.Processes(host,maker);CompanyAccountingProcessStartReceipt start=null!;
        await posting.Prepare(source,async()=>{var review=await makerClient.ReviewCompanyAccountingStartAsync(new("definition","JOURNAL_ENTRY",posting.Reviewed.Draft.Command.JournalId,CompanyAccountingSourceTypes.JournalPreparationRevision(posting.Reviewed.Original)));start=await makerClient.StartCompanyAccountingAsync(review.CreateIntent(Guid.NewGuid().ToString("N")));});
        Actor(posting.Reviewed.Actor);using var reviewer=CompanyGLAccountHttpTests.Selected(actor:posting.Reviewed.Actor,grants:grants);var reviewerClient=CompanySalesReceivableWorkflowTests.Fixture.Processes(host,reviewer);var approved=(await reviewerClient.ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.True(approved.Eligible);await reviewerClient.CompleteCompanyAccountingStepAsync(new(approved,Guid.NewGuid().ToString("N"),"Observe native independent journal approval"));
        var posted=await posting.Post();var productionWrites=production.Writes;
        var other=(await reviewerClient.ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.False(other.Eligible);Assert.Equal("SALES_JOURNAL_POSTER_REQUIRED",other.Eligibility);
        Actor(posting.Command.Source.ActorUserId);using var poster=CompanyGLAccountHttpTests.Selected(actor:posting.Command.Source.ActorUserId,grants:grants);var posterClient=CompanySalesReceivableWorkflowTests.Fixture.Processes(host,poster);
        var taskWrites=lifecycle.Native.Writes;var correlation=Assert.Single(production.Rows.OfType<SALES_JOURNAL_POSTING_OUTCOME>());production.Rows.Remove(correlation);
        var missing=await Assert.ThrowsAsync<OilGasApiException>(()=>posterClient.ReviewCompanyAccountingStepAsync(start.InstanceId));Assert.Equal(HttpStatusCode.Conflict,missing.StatusCode);Assert.Equal(taskWrites,lifecycle.Native.Writes);Assert.Equal(productionWrites,production.Writes);production.Rows.Add(correlation);
        host.Context.Permissions=grants.Where(value=>value!="Accounting.PostSalesJournal").ToArray();var revoked=(await posterClient.ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.False(revoked.Eligible);Assert.Equal("SALES_JOURNAL_POSTING_PERMISSION_REQUIRED",revoked.Eligibility);Assert.Equal(taskWrites,lifecycle.Native.Writes);host.Context.Permissions=grants;
        async Task<UnifiedInbox> Inbox(){var response=await host.Client.GetAsync("api/workflow/tasks/inbox?personaCode=ACCOUNTANT");response.EnsureSuccessStatusCode();return(await response.Content.ReadFromJsonAsync<UnifiedInbox>())!;}
        var queue=await Inbox();var task=Assert.Single(queue.CriticalTasks.Concat(queue.HighPriorityTasks).Concat(queue.NormalTasks));Assert.Equal("JOURNAL_ENTRY",task.EntityType);Assert.Equal(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",start.InstanceId,"JOURNAL_ENTRY"),task.Route);
        var ready=task.AccountingStepReview!;Assert.True(ready.Eligible);Assert.Equal(CompanyAccountingSourceTypes.JournalPosting,ready.AccountingEvidenceAction);Assert.Contains("original poster",PersonaTaskInboxState.SalesJournalMessage(ready));
        await RenderPostedAssignment(queue,poster,task.Route);
        var command=new CompanyAccountingStepCommand(ready,Guid.NewGuid().ToString("N"),"Observe complete native journal and GL posting correlation");lifecycle.Native.FailCommit=true;var uncertain=await Assert.ThrowsAsync<OilGasApiException>(()=>posterClient.CompleteCompanyAccountingStepAsync(command));Assert.Equal(HttpStatusCode.Conflict,uncertain.StatusCode);var writes=lifecycle.Native.Writes;lifecycle.Native.FailCommit=false;
        var receipt=await posterClient.CompleteCompanyAccountingStepAsync(command);receipt.Require(command);Assert.Equal(writes,lifecycle.Native.Writes);
        Assert.Equal(JsonSerializer.Serialize(receipt),JsonSerializer.Serialize(await posterClient.CompleteCompanyAccountingStepAsync(command)));Assert.Equal(writes,lifecycle.Native.Writes);Assert.Equal(productionWrites,production.Writes);
        var process=(await lifecycle.Service.GetProcessInstanceAsync(start.InstanceId))!;Assert.Equal(ProcessStatus.COMPLETED,process.Status);Assert.Equal("POSTED_CONFIRMED",process.CurrentState);Assert.Empty(receipt.NextTaskIds);Assert.Equal(StepStatus.SKIPPED,process.StepInstances.Single(step=>step.StepId=="JOURNAL_REJECTION_REVIEW").Status);
        var native=await posterClient.GetCompanySalesJournalSourceAsync(start.InstanceId);Assert.Equal(JsonSerializer.Serialize(posted.Journal),JsonSerializer.Serialize(native.Journal.Journal));Assert.Equal(CompanyAccountingSourceTypes.JournalPreparationRevision(posting.Reviewed.Original),native.SourceRevision);Assert.True(productionReads>0);Assert.True(lifecycleReads>0);
        Assert.Equal(0,(await Inbox()).Counts.TotalPending);
    }
    private static async Task RenderPostedAssignment(UnifiedInbox response,Beep.OilandGas.Client.App.CompanyClientContext company,string route)
    {
        var transport=new Mock<IUnifiedTaskInboxService>();transport.Setup(value=>value.GetInboxAsync("ACCOUNTANT")).ReturnsAsync(CompanyInventorySourceReview.Copy(response));
        var personas=new Mock<IPersonaContextService>();personas.Setup(value=>value.EnsureLoadedAsync()).Returns(Task.CompletedTask);personas.SetupGet(value=>value.CurrentPersona).Returns(new AppPersona{Code="ACCOUNTANT",Name="Accountant",DefaultRoute="/accounting/dashboard"});personas.SetupGet(value=>value.CurrentProfile).Returns(new AppUserPersona{UserId=response.ActorUserId,PersonaCode="ACCOUNTANT"});
        using var state=new PersonaTaskInboxState(transport.Object,company,personas.Object);await state.LoadAsync(["/accounting/dashboard"]);state.SelectSource("JOURNAL_ENTRY");state.SelectRole("ConfiguredCompanyRole");state.SelectAccountingEvidence("ELIGIBLE");Assert.Single(state.VisibleRows);Assert.Equal(1,state.SourceCount("JOURNAL_ENTRY"));Assert.Equal(1,state.SelectedCounts.TotalPending);
        var services=new ServiceCollection();services.AddLogging();MudBlazor.Services.ServiceCollectionExtensions.AddMudServices(services);services.AddSingleton<Microsoft.JSInterop.IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<Microsoft.AspNetCore.Components.NavigationManager,APInvoiceFinancialHttpTests.Navigation>();await using var provider=services.BuildServiceProvider();await using var renderer=new Microsoft.AspNetCore.Components.Web.HtmlRenderer(provider,provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.PersonaAssignedWorkPanel>(Microsoft.AspNetCore.Components.ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state},{"WorkspaceName","Accountant"}}));var html=root.ToHtmlString();Assert.Contains("Journal review/posting assignments: 1",html);Assert.Contains("original poster",html);Assert.Contains(route,html);});
        state.SelectSource("AR_INVOICE");Assert.Empty(state.VisibleRows);Assert.Equal(0,state.SelectedCounts.TotalPending);
    }
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task ActualJournalProcessApiKeepsNativeDecisionAndLifecycleTransactionsSeparate(string decision)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var journal=new CompanySalesJournalReviewTests.Fixture(source.Source);await journal.Prepare(decision);
        var lifecycle=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");lifecycle.Definition.ENTITY_TYPE="JOURNAL_ENTRY";
        ProcessStepDefinition Step(string id,int sequence,string action,bool approval)=>new(){StepId=id,StepName=id,SequenceNumber=sequence,RequiredRoles=["ConfiguredCompanyRole"],RequiresApproval=approval,StepType=approval?"APPROVAL":"ACTION",StepConfiguration=new(){[CompanyAccountingStepEvidence.ConfigurationKey]=action}};
        lifecycle.Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=new[]{Step("review",1,CompanyAccountingSourceTypes.Approval("JOURNAL_ENTRY"),true),Step("post",2,CompanyAccountingSourceTypes.JournalPosting,false),Step("reject",3,CompanyAccountingSourceTypes.RejectionReview("JOURNAL_ENTRY"),false)}});CompanyStepCommandTests.ConfigureUpdates(lifecycle);
        var production=journal.Draft.Native;var productionReads=0;var lifecycleReads=0;
        static object[] Rows(List<object> rows,string table,List<AppFilter> filters)=>rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();
        production.Source.Setup(value=>value.GetEntity(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).Returns((string table,List<AppFilter> filters)=>{Assert.Null(((ITaskTransactionEvidence)lifecycle.Native.Source.Object).ActiveTransaction);productionReads++;return Rows(production.Rows,table,filters);});
        production.Source.Setup(value=>value.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{Assert.Null(((ITaskTransactionEvidence)lifecycle.Native.Source.Object).ActiveTransaction);productionReads++;return Rows(production.Rows,table,filters);});
        lifecycle.Native.Source.Setup(value=>value.GetEntity(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).Returns((string table,List<AppFilter> filters)=>{Assert.Null(((ITaskTransactionEvidence)production.Source.Object).ActiveTransaction);lifecycleReads++;return Rows(lifecycle.Native.Rows,table,filters);});
        lifecycle.Native.Source.Setup(value=>value.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{Assert.Null(((ITaskTransactionEvidence)production.Source.Object).ActiveTransaction);lifecycleReads++;return Rows(lifecycle.Native.Rows,table,filters);});
        string[] grants=["Accounting.View","Accounting.ReviewSalesJournalSource","Accounting.PrepareSalesJournal","Accounting.RecordSalesJournalDecision","Accounting.PostSalesJournal","Workflow.Start","Workflow.Approve","Workflow.Execute"];
        await using var host=await AccountingSourceHttpTests.Start(permissions:grants,configureAccounting:(services,context)=>{context.Actor=source.Source.ActorUserId;context.PolicyBinding=source.Source.BindingRevision;context.StartBinding=source.Source.BindingRevision;context.FixedProcess=lifecycle.Service;services.AddSingleton(journal.Draft.Service);});
        var denyField=false;host.Context.Resources.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),source.Source.FieldId,"FIELD",It.IsAny<string>(),null)).ReturnsAsync(()=>new AccessCheckResponse{HasAccess=!denyField});
        void Actor(string actor){host.Context.Actor=actor;host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);}
        Actor(source.Source.ActorUserId);using var maker=CompanyGLAccountHttpTests.Selected(actor:source.Source.ActorUserId,grants:grants);var makerClient=CompanySalesReceivableWorkflowTests.Fixture.Processes(host,maker);
        var query=new CompanyAccountingProcessStartQuery("definition","JOURNAL_ENTRY",journal.Draft.Command.JournalId,CompanyAccountingSourceTypes.JournalPreparationRevision(journal.Original));
        host.Context.Permissions=["Accounting.View"];var startDenied=await Assert.ThrowsAsync<OilGasApiException>(()=>makerClient.ReviewCompanyAccountingStartAsync(query));Assert.Equal(HttpStatusCode.Forbidden,startDenied.StatusCode);Assert.Equal(0,lifecycle.Native.Writes);host.Context.Permissions=grants;
        var review=await makerClient.ReviewCompanyAccountingStartAsync(query);var start=await makerClient.StartCompanyAccountingAsync(review.CreateIntent(Guid.NewGuid().ToString("N")));
        var writes=lifecycle.Native.Writes;var recorded=await makerClient.GetCompanySalesJournalSourceAsync(start.InstanceId);recorded.Require(source.Source.Company,source.Source.ActorUserId,start.InstanceId);Assert.Equal("DRAFT",recorded.Journal.Journal.Header.STATUS);Assert.Equal(writes,lifecycle.Native.Writes);
        denyField=true;var denied=await Assert.ThrowsAsync<OilGasApiException>(()=>makerClient.GetCompanySalesJournalSourceAsync(start.InstanceId));Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);denyField=false;
        host.Client.DefaultRequestHeaders.Remove("X-Test-Role");var roleDenied=await Assert.ThrowsAsync<OilGasApiException>(()=>makerClient.GetCompanySalesJournalSourceAsync(start.InstanceId));Assert.Equal(HttpStatusCode.Forbidden,roleDenied.StatusCode);host.Client.DefaultRequestHeaders.Add("X-Test-Role","yes");Assert.Equal(writes,lifecycle.Native.Writes);
        Actor(journal.Actor);using var reviewer=CompanyGLAccountHttpTests.Selected(actor:journal.Actor,grants:grants);var reviewerClient=CompanySalesReceivableWorkflowTests.Fixture.Processes(host,reviewer);
        host.Context.Permissions=grants.Where(value=>value!="Accounting.ReviewSalesJournalSource").ToArray();var privateSourceDenied=await Assert.ThrowsAsync<OilGasApiException>(()=>reviewerClient.GetCompanySalesJournalSourceAsync(start.InstanceId));Assert.Equal(HttpStatusCode.Forbidden,privateSourceDenied.StatusCode);Assert.Equal(writes,lifecycle.Native.Writes);host.Context.Permissions=grants;
        var pending=(await reviewerClient.ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.Equal("SALES_JOURNAL_APPROVAL_REQUIRED",pending.Eligibility);Assert.Equal(CompanyAccountingSourceTypes.Approval("JOURNAL_ENTRY"),pending.AccountingEvidenceAction);
        await journal.Review();var ready=(await reviewerClient.ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.Equal(decision=="REJECT"?"SALES_JOURNAL_REJECTION_RECORDED":"ELIGIBLE",ready.Eligibility);
        var command=new CompanyAccountingStepCommand(ready,Guid.NewGuid().ToString("N"),"Observe original independent journal decision",decision=="REJECT"?CompanyAccountingSourceTypes.RejectionRoute("JOURNAL_ENTRY"):null);var receipt=await reviewerClient.CompleteCompanyAccountingStepAsync(command);receipt.Require(command);writes=lifecycle.Native.Writes;Assert.Equal(JsonSerializer.Serialize(receipt),JsonSerializer.Serialize(await reviewerClient.CompleteCompanyAccountingStepAsync(command)));Assert.Equal(writes,lifecycle.Native.Writes);
        if(decision=="REJECT")
        {
            Actor(source.Source.ActorUserId);var makerReview=(await makerClient.ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.True(makerReview.Eligible);Assert.Equal(CompanyAccountingSourceTypes.RejectionReview("JOURNAL_ENTRY"),makerReview.AccountingEvidenceAction);await makerClient.CompleteCompanyAccountingStepAsync(new(makerReview,Guid.NewGuid().ToString("N"),"Original maker reviewed journal rejection"));Assert.Equal("REJECTION_REVIEWED",(await lifecycle.Service.GetProcessInstanceAsync(start.InstanceId))!.CurrentState);
        }
        else
        {
            var posting=(await reviewerClient.ReviewCompanyAccountingStepAsync(start.InstanceId))!;Assert.False(posting.Eligible);Assert.Equal("SALES_JOURNAL_POSTING_REQUIRED",posting.Eligibility);Assert.Equal(CompanyAccountingSourceTypes.JournalPosting,posting.AccountingEvidenceAction);Assert.Equal("post",(await lifecycle.Service.GetProcessInstanceAsync(start.InstanceId))!.CurrentStepId);
        }
        Assert.Empty(production.Rows.OfType<GL_ENTRY>());Assert.True(productionReads>0,"The actual production journal reader must execute.");Assert.True(lifecycleReads>0,"The actual lifecycle process reader must execute.");
    }
}
#endif
