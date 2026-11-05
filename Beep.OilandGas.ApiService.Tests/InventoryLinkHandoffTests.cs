#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InventoryLinkHandoffTests
{
    [Theory][InlineData("prepared")][InlineData("resolved")][InlineData("reopened")][InlineData("grant")][InlineData("history")][InlineData("audit")][InlineData("proof")]
    public async Task ActualCycleSourceCanHandOffToConfiguredIndependentWorkOnlyWithVerifiedMakerAndCurrentInventoryAuthority(string mode)
    {
        var f=await InventoryLinkCycleTests.Fixture.Create(pending:mode=="prepared",withMaker:true);
        if(mode is "reopened" or "history" or "audit")await f.Record();var baseline=f.Native.Writes;
        if(mode=="history")Assert.Single(f.Native.Rows.OfType<PO_INVENTORY_LINK_HISTORY>()).REASON="tampered";
        if(mode=="audit")f.Stored.ROW_CHANGED_BY="unreviewed-editor";if(mode=="proof")f.Stored.INVENTORY_LINK_DECISION_JSON="{}";
        var workflow=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");workflow.Definition.ENTITY_TYPE="PURCHASE_ORDER";workflow.Native.Destinations=["ACCOUNTANT"];CompanyStepCommandTests.ConfigureUpdates(workflow);
        workflow.Native.Personas.Setup(value=>value.GetActivePersonasAsync("Controller")).ReturnsAsync(new List<string>{"CONTROLLER"});
        using var rbac=new WorkflowInboxHttpTests.Fixture("ACCOUNTANT");string[] grants=mode=="grant"?["Procurement.View","Workflow.Start"]:["Procurement.View","Inventory.View","Workflow.Start"];
        await using var host=await AccountingSourceHttpTests.Start(permissions:grants,configure:(services,context)=>{rbac.Configure(services,context);context.FixedProcess=workflow.Service;context.StartBinding=new string('B',64);services.AddSingleton<IProcessService>(workflow.Service);services.AddSingleton<ICrossPersonaTaskRouter>(workflow.Native.Router);},configureAccounting:(services,context)=>{services.AddSingleton(f.Service);services.AddSingleton(f.Existing.Existing.Existing.AP);context.Actor="starter";context.Permissions=grants;});
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","starter");using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"starter",new(f.Command.Company,["ConfiguredCompanyRole"],grants));
        var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var processes=new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance);
        var source=(await f.Service.GetSourceReviewAsync(f.Command.Company,"order","starter"))!;
        var query=new CompanyAccountingProcessStartCatalogQuery("PURCHASE_ORDER","order",source.Revision);
        if(mode is "grant" or "history" or "audit" or "proof")
        {
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>processes.GetCompanyAccountingStartCatalogAsync(query));Assert.Equal(mode=="grant"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);Assert.Equal(0,workflow.Native.Writes);Assert.Equal(baseline,f.Native.Writes);return;
        }
        using var handoff=new CompanyAccountingHandoffState(company);handoff.AttachSource("PURCHASE_ORDER","order",source.Revision);await handoff.LoadCatalogAsync(processes);handoff.Select("definition");await handoff.SubmitAsync(processes);
        Assert.Equal("po-preparer",handoff.Intent!.SourcePreparerUserId);Assert.Equal(source.Revision,handoff.Intent.SourceRevision);var started=handoff.Receipt!;
        host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Context.Actor="actor";host.Context.Permissions=["Procurement.View","Inventory.View","Workflow.Approve"];company.TrySelect(company.Clear(),"actor",new(f.Command.Company,["ConfiguredCompanyRole"],host.Context.Permissions));
        var inbox=await new UnifiedTaskInboxService(api).GetInboxAsync("ACCOUNTANT");var task=Assert.Single(inbox.CriticalTasks.Concat(inbox.HighPriorityTasks).Concat(inbox.NormalTasks));Assert.Equal(started.TaskIds[0],task.TaskId);
        var step=(await processes.ReviewCompanyAccountingStepAsync(started.InstanceId))!;Assert.True(step.Eligible);Assert.Equal("po-preparer",step.SourceMakerUserId);Assert.Equal(source.Revision,step.SourceRevision);
        host.Context.Permissions=["Procurement.View","Workflow.Approve"];
        var denied=await Assert.ThrowsAsync<OilGasApiException>(()=>processes.ReviewCompanyAccountingStepAsync(started.InstanceId));Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);
        Assert.Equal(baseline,f.Native.Writes);Assert.Equal("DRAFT",f.Existing.Existing.Existing.Invoice.STATUS);
    }
}
#endif
