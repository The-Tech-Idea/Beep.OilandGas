#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InvoiceVarianceWorkflowSourceTests
{
    [Fact] public async Task VerifiedControllerDecisionPreservesProposalHandoffRevisionAcrossReaders()
    {
        var f=new InvoiceVarianceDecisionCommandTests.Fixture();await f.Prepare();var n=f.Existing.Existing.ReadOwner.Native;
        var before=(await n.AP.GetSourceReviewAsync(f.Command.Company,"invoice","actor"))!;var revision=before.ForCompanyWorkflow().Revision;Assert.Equal(before.Revision,revision);
        await f.Record();var after=(await n.AP.GetSourceReviewAsync(f.Command.Company,"invoice","reviewer"))!;Assert.NotEqual(before.Revision,after.Revision);
        Assert.Equal(revision,after.ForCompanyWorkflow().Revision);Assert.Equal("reviewer",after.ForCompanyWorkflow().ActorUserId);Assert.Equal("actor",after.ForCompanyWorkflow().Invoice.ROW_CHANGED_BY);
        after.ForCompanyWorkflow().Invoice.TOTAL_AMOUNT=99m;Assert.Equal(2.01m,after.ForCompanyWorkflow().Invoice.TOTAL_AMOUNT);
    }
    [Theory][InlineData("line")][InlineData("maker")][InlineData("header")][InlineData("proposal")][InlineData("decision")]
    public async Task ProjectionCannotNormalizeUnprovenFinancialSourceOrHistoryChanges(string fault)
    {
        var f=new InvoiceVarianceDecisionCommandTests.Fixture();await f.Prepare();await f.Record();var n=f.Existing.Existing.ReadOwner.Native;
        if(fault=="line")n.Line.ITEM_DESCRIPTION="changed";if(fault=="maker")n.Line.ROW_CREATED_BY="other";if(fault=="header")f.Existing.Stored.TOTAL_AMOUNT=99m;
        if(fault=="proposal")f.Existing.Stored.VARIANCE_PROPOSAL_JSON="unverified";if(fault=="decision")f.Existing.Stored.VARIANCE_DECISION_JSON="unverified";
        var source=(await n.AP.GetSourceReviewAsync(f.Command.Company,"invoice","reviewer"))!;Assert.Throws<InvalidOperationException>(()=>source.ForCompanyWorkflow());
    }
    [Theory][InlineData("valid")][InlineData("line")][InlineData("proof")][InlineData("grant")][InlineData("role")]
    public async Task ExistingApiCanStartRecordedProposalHandoffAndReadItAfterVerifiedControllerDecision(string scenario)
    {
        var f=new InvoiceVarianceDecisionCommandTests.Fixture();await f.Prepare();var n=f.Existing.Existing.ReadOwner.Native;
        var process=new CompanyProcessStartTests.Fixture("ConfiguredControllerRole");process.Native.Destinations=["ACCOUNTANT"];
        await using var host=await AccountingSourceHttpTests.Start(configure:(_,context)=>{context.FixedProcess=process.Service;context.StartBinding=new string('B',64);},configureAccounting:(services,_)=>{services.AddSingleton(f.Existing.Existing.Owner);services.AddSingleton(n.AP);services.AddSingleton(n.PO);});
        host.Context.Permissions=["Accounting.View","Procurement.View","Workflow.Start","Workflow.Approve"];
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(f.Command.Company,["ConfiguredCompanyRole"],host.Context.Permissions));
        var client=new BusinessProcessServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var source=(await n.AP.GetSourceReviewAsync(f.Command.Company,"invoice","actor"))!;
        var review=await client.ReviewCompanyAccountingStartAsync(new("definition","AP_INVOICE","invoice",source.Revision));Assert.Equal("source-maker",review.SourcePreparerUserId);Assert.Equal("ConfiguredControllerRole",review.InitialRole);
        var receipt=await client.StartCompanyAccountingAsync(review.CreateIntent(Guid.NewGuid().ToString("N")));
        var task=Assert.Single(process.Native.Rows.OfType<CROSS_PERSONA_TASK>());Assert.Single(receipt.TaskIds);
        Assert.Equal(receipt.InstanceId,task.PROCESS_INSTANCE_ID);Assert.Equal("ConfiguredControllerRole",task.ASSIGNED_ROLE);Assert.Equal("ACCOUNTANT",task.TARGET_PERSONA_CODE);
        Assert.Equal(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",receipt.InstanceId,"AP_INVOICE"),task.ROUTE);
        await f.Record();
        host.Context.Actor="reviewer";host.Context.Roles=["ConfiguredControllerRole"];host.Client.DefaultRequestHeaders.Add("X-Test-Actor","reviewer");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredControllerRole");
        company.TrySelect(company.Clear(),"reviewer",new(f.Command.Company,["ConfiguredControllerRole"],host.Context.Permissions));
        if(scenario=="line")n.Line.ITEM_DESCRIPTION="changed";if(scenario=="proof")f.Existing.Stored.VARIANCE_DECISION_JSON="unverified";
        if(scenario=="grant")host.Context.Permissions=[];if(scenario=="role")host.Context.Roles=["OtherRole"];
        if(scenario=="valid")
        {
            var owned=await client.GetCompanyAccountingInstanceAsync(receipt.InstanceId);Assert.NotNull(owned);
            var step=await client.ReviewCompanyAccountingStepAsync(receipt.InstanceId);Assert.NotNull(step);Assert.True(step.Eligible);Assert.Equal(source.Revision,step.SourceRevision);Assert.Equal("source-maker",step.SourceMakerUserId);
        }
        else{var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetCompanyAccountingInstanceAsync(receipt.InstanceId));Assert.Equal(scenario is "grant" or "role"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);}
        Assert.Equal("DRAFT",f.Existing.Stored.STATUS);Assert.Equal(2,n.Native.Writes);Assert.Equal(5,process.Native.Writes);
    }
    [Theory][InlineData("line")][InlineData("proof")][InlineData("audit")]
    public async Task InvalidProposalSourceCannotOfferAStartReviewOrCreateTasks(string fault)
    {
        var f=new InvoiceVarianceDecisionCommandTests.Fixture();await f.Prepare();var n=f.Existing.Existing.ReadOwner.Native;var process=new CompanyProcessStartTests.Fixture("ConfiguredControllerRole");
        await using var host=await AccountingSourceHttpTests.Start(configure:(_,context)=>{context.FixedProcess=process.Service;context.StartBinding=new string('B',64);},configureAccounting:(services,_)=>{services.AddSingleton(f.Existing.Existing.Owner);services.AddSingleton(n.AP);services.AddSingleton(n.PO);});
        host.Context.Permissions=["Accounting.View","Workflow.Start"];
        if(fault=="line")n.Line.ITEM_DESCRIPTION="changed";if(fault=="proof")f.Existing.Stored.VARIANCE_PROPOSAL_JSON="unverified";if(fault=="audit")f.Existing.Stored.ROW_CHANGED_BY="unproven-editor";
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(f.Command.Company,["ConfiguredCompanyRole"],host.Context.Permissions));
        var client=new BusinessProcessServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.ReviewCompanyAccountingStartAsync(new("definition","AP_INVOICE","invoice")));Assert.Equal(HttpStatusCode.Conflict,error.StatusCode);Assert.Equal(0,process.Native.Writes);Assert.Empty(process.Native.Rows.OfType<CROSS_PERSONA_TASK>());
    }
}
#endif
