#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using TheTechIdea.Data.OilGas;
using Beep.OilandGas.Web.Services;
using Xunit;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Moq;
using TheTechIdeaWeb.Diagnostics;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CorrectionAcknowledgementWorkflowTests
{
    private static async Task<CompanyAccountingStepCommand> VerifyAcknowledgementScreen(InvoiceVarianceStepEvidenceTests.Fixture f,string notes)
    {
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(f.Company);services.AddScoped<CompanyAccountingStepState>();
        services.AddSingleton<IBusinessProcessServiceClient>(f.Client);services.AddSingleton<IAccountingServiceClient>(new Mock<IAccountingServiceClient>(MockBehavior.Strict).Object);services.AddSingleton<AuthenticationStateProvider,InvoiceVarianceStepEvidenceTests.Authentication>();services.AddSingleton<NavigationManager>(new InvoiceVarianceStepEvidenceTests.Navigation(f.Id));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,InvoiceVarianceStepEvidenceTests.Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());var writes=f.Process.Native.Writes;
        var html=await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",f.Id}}));
            var page=((InvoiceVarianceStepEvidenceTests.Components)provider.GetRequiredService<IComponentActivator>()).Detail!;
            await(Task)page.GetType().GetMethod("ReviewStep",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null)!;
            typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null);return WebUtility.HtmlDecode(root.ToHtmlString());
        });
        Assert.Contains("Correction version: 1",html);Assert.Contains("This step records independent correction acknowledgement; financial approval remains separate.",html);Assert.Contains("Complete reviewed workflow step",html);Assert.Equal(writes,f.Process.Native.Writes);
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var page=((InvoiceVarianceStepEvidenceTests.Components)provider.GetRequiredService<IComponentActivator>()).Detail!;
            page.GetType().GetField("_notes",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,notes);
            await(Task)page.GetType().GetMethod("CompleteStep",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null)!;
        });
        var completion=provider.GetRequiredService<CompanyAccountingStepState>();Assert.NotNull(completion.Receipt);Assert.False(completion.HasUnconfirmedIntent);Assert.NotNull(completion.Intent);
        completion.Receipt!.Require(completion.Intent!);return completion.Intent!;
    }
    [Theory][InlineData("valid")][InlineData("version")][InlineData("intent")][InlineData("actor")][InlineData("binding")][InlineData("action")][InlineData("revision")]
    public async Task CanonicalWorkEvidenceBindsLatestVersionActorAndFinancialSnapshot(string mode)
    {
        var f=new InvoiceCorrectionCommandTests.Fixture();await f.Prepare();await f.Record();
        var current=await InvoiceVarianceDecisionReviewTests.Financial(f.Existing.Existing,"actor");
        var evidence=APInvoiceCorrectionStepEvidence.Create(current,f.Command.BindingRevision,CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork);
        evidence=mode switch{"version"=>evidence with{CorrectionVersion=2},"intent"=>evidence with{CorrectionIntentId=Guid.NewGuid().ToString("N")},
            "actor"=>evidence with{ActorUserId="reviewer"},"binding"=>evidence with{ProductionBindingRevision=new string('C',64)},
            "action"=>evidence with{Action=CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement},"revision"=>evidence with{Revision=new string('C',64)},_=>evidence};
        if(mode=="valid"){evidence.Require(f.Command.Company,"actor","invoice");Assert.Equal(f.Command.IntentId,evidence.CorrectionIntentId);Assert.Equal(current.Revision,evidence.FinancialRevision);Assert.Null(evidence.CorrectionWorkIntentId);}
        else Assert.Throws<InvalidOperationException>(()=>evidence.Require(f.Command.Company,"actor","invoice"));
        Assert.Equal(4,f.Existing.Existing.Existing.ReadOwner.Native.Native.Writes);
    }
    private static void Select(InvoiceVarianceStepEvidenceTests.Fixture f,string actor,string role)
    {
        f.Host.Context.Actor=actor;f.Host.Context.Roles=[role];f.Host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.CorrectInvoice","Accounting.ReviewInvoiceVariance","Workflow.Approve","Workflow.Execute"];
        f.Host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");f.Host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);
        f.Host.Client.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");f.Host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role",role);
        f.Company.TrySelect(f.Company.Clear(),actor,new(f.Invoice.Command.Company,[role],f.Host.Context.Permissions));
    }
    [Theory][InlineData("valid")][InlineData("retry")][InlineData("changed-financial")][InlineData("late-financial")][InlineData("grant")][InlineData("binding")][InlineData("maker")][InlineData("preparer")][InlineData("version")][InlineData("missing-next")]
    public async Task NativeConfiguredCorrectionWorkOpensIndependentVersionBoundAcknowledgement(string mode)
    {
        await using var f=new InvoiceVarianceStepEvidenceTests.Fixture();await f.Prepare(correctionWorkflow:true,acknowledgement:mode!="missing-next");f.Host.Context.PolicyBinding=f.Invoice.Command.BindingRevision;
        var controller=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;await f.Client.CompleteCompanyAccountingStepAsync(new(controller,Guid.NewGuid().ToString("N"),"Complete recorded controller correction request."));
        var correction=new InvoiceCorrectionCommandTests.Fixture(f.Invoice);await correction.Prepare(alreadyPrepared:true);await correction.Record();
        Select(f,"actor","ConfiguredAccountantRole");var work=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;
        Assert.Equal(mode!="missing-next",work.Eligible);Assert.Equal(1,work.CorrectionEvidence!.CorrectionVersion);Assert.Equal(correction.Command.IntentId,work.CorrectionEvidence.CorrectionIntentId);
        var workCommand=new CompanyAccountingStepCommand(mode=="missing-next"?work with{Eligible=true,Eligibility="ELIGIBLE"}:work,Guid.NewGuid().ToString("N"),"Confirm recorded source correction work; independent acknowledgement remains required.");
        if(mode=="missing-next")
        {var writes=f.Process.Native.Writes;var error=await Assert.ThrowsAsync<OilGasApiException>(()=>f.Client.CompleteCompanyAccountingStepAsync(workCommand));Assert.Equal(HttpStatusCode.Conflict,error.StatusCode);Assert.Equal(writes,f.Process.Native.Writes);return;}
        var opened=await f.Client.CompleteCompanyAccountingStepAsync(workCommand);Assert.NotEmpty(opened.NextTaskIds);
        Select(f,mode=="maker"?"source-maker":mode=="preparer"?"actor":"reviewer","ConfiguredControllerRole");
        if(mode is "maker" or "preparer")
        {await Assert.ThrowsAsync<OilGasApiException>(()=>f.Client.ReviewCompanyAccountingStepAsync(f.Id));Assert.Single(f.Process.Native.Rows.OfType<PROCESS_APPROVAL>());return;}
        var acknowledgement=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;Assert.True(acknowledgement.Eligible);Assert.True(acknowledgement.RequiresIndependentActor);
        Assert.Equal(CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement,acknowledgement.CorrectionEvidence!.Action);Assert.Equal(1,acknowledgement.CorrectionEvidence.CorrectionVersion);Assert.Equal(workCommand.IntentId,acknowledgement.CorrectionEvidence.CorrectionWorkIntentId);
        var command=new CompanyAccountingStepCommand(acknowledgement,Guid.NewGuid().ToString("N"),"Acknowledge the latest source correction against current diagnostics; financial approval remains separate.");
        if(mode=="valid")command=await VerifyAcknowledgementScreen(f,command.Notes);
        var before=f.Process.Native.Writes;var native=f.Invoice.Existing.Existing.ReadOwner.Native;
        void Drift()=>native.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(mode=="changed-financial")Drift();
        if(mode=="grant")f.Host.Context.Permissions=["Accounting.View","Procurement.View","Workflow.Approve"];
        if(mode=="binding")f.Host.Context.PolicyBinding=new string('C',64);
        if(mode=="version"){await correction.PrepareNext(4m);await correction.Record();var latest=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;Assert.False(latest.Eligible);Assert.Equal("CORRECTION_WORK_EVIDENCE_REQUIRED",latest.Eligibility);}
        if(mode=="late-financial")f.Process.Native.AfterCommit=Drift;
        if(mode is "changed-financial" or "late-financial" or "grant" or "binding" or "version")
        {
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>f.Client.CompleteCompanyAccountingStepAsync(command));Assert.Equal(mode=="grant"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);
            if(mode!="late-financial")Assert.Equal(before,f.Process.Native.Writes);
                        else
            {
                Assert.True(f.Process.Native.Writes>before);var committed=f.Process.Native.Writes;f.Process.Native.AfterCommit=null;
                native.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=3m;
                var recovered=await f.Client.CompleteCompanyAccountingStepAsync(command);recovered.Require(command);Assert.Equal(committed,f.Process.Native.Writes);
            }
        }
        else
        {
            var receipt=await f.Client.CompleteCompanyAccountingStepAsync(command);receipt.Require(command);Assert.Null(receipt.NextStepInstanceId);
            var ack=Assert.Single(f.Process.Native.Rows.OfType<PROCESS_APPROVAL>().Where(row=>row.APPROVAL_TYPE=="CORRECTION_ACKNOWLEDGEMENT"));Assert.Equal("ACKNOWLEDGED",ack.APPROVAL_STATUS);Assert.Equal("ACKNOWLEDGE_CORRECTION",ack.REQUIRED_ACTION);Assert.Equal("reviewer",ack.APPROVED_BY);
            var retained=await f.Process.Service.GetRecordedCompanyAccountingStepCommandAsync(f.Id);Assert.Equal(command,retained);Assert.Equal(workCommand.IntentId,retained!.Review.CorrectionEvidence!.CorrectionWorkIntentId);
            Assert.Equal(ProcessStatus.COMPLETED,(await f.Process.Service.GetProcessInstanceAsync(f.Id))!.Status);
            if(mode=="retry"){var writes=f.Process.Native.Writes;var retried=await f.Client.CompleteCompanyAccountingStepAsync(command);Assert.Equal(receipt.RecordedRevision,retried.RecordedRevision);Assert.Equal(writes,f.Process.Native.Writes);}
        }
        Assert.Equal("DRAFT",f.Invoice.Existing.Stored.STATUS);Assert.Equal(mode=="version"?6:4,native.Native.Writes);
    }
}
#endif
