#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Web.Services;
using TheTechIdea.Data.OilGas;
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
public sealed class CorrectionReturnWorkflowTests
{
    [Theory][InlineData("base")][InlineData("valid")][InlineData("empty")][InlineData("duplicate")][InlineData("number")][InlineData("upper")]
    public void TaskRoundRequiresOneCanonicalDeclaredCycle(string mode)
    {
        var id="abcdef0123456789abcdef0123456789";
        var json=mode switch{"base"=>"{}","empty"=>"{\"correctionCycle\":\"\"}","duplicate"=>"{\"correctionCycle\":\""+id+"\",\"correctionCycle\":\""+id+"\"}","number"=>"{\"correctionCycle\":7}","upper"=>"{\"correctionCycle\":\""+id.ToUpperInvariant()+"\"}",_=>"{\"correctionCycle\":\""+id+"\"}"};
        if(mode=="base")Assert.Null(CompanyAccountingCorrectionCycle.TaskRound(json));else if(mode=="valid")Assert.Equal(id,CompanyAccountingCorrectionCycle.TaskRound(json));else Assert.Throws<InvalidOperationException>(()=>CompanyAccountingCorrectionCycle.TaskRound(json));
    }
    private static async Task<CompanyAccountingStepCommand> ReturnThroughScreen(InvoiceVarianceStepEvidenceTests.Fixture f,string notes)
    {
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(f.Company);services.AddScoped<CompanyAccountingStepState>();
        services.AddSingleton<IBusinessProcessServiceClient>(f.Client);services.AddSingleton<IAccountingServiceClient>(new Mock<IAccountingServiceClient>(MockBehavior.Strict).Object);services.AddSingleton<AuthenticationStateProvider,InvoiceVarianceStepEvidenceTests.Authentication>();services.AddSingleton<NavigationManager>(new InvoiceVarianceStepEvidenceTests.Navigation(f.Id));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,InvoiceVarianceStepEvidenceTests.Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",f.Id}}));
            var page=((InvoiceVarianceStepEvidenceTests.Components)provider.GetRequiredService<IComponentActivator>()).Detail!;
            await(Task)page.GetType().GetMethod("ReviewStep",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null)!;
            typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null);
            Assert.Contains("Return newer correction to original preparer",WebUtility.HtmlDecode(root.ToHtmlString()));
            page.GetType().GetField("_notes",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,notes);
            await(Task)page.GetType().GetMethod("CompleteStep",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null)!;
            typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null);
            Assert.Contains("Recorded correction return and process reload confirmed",WebUtility.HtmlDecode(root.ToHtmlString()));
        });
        var completion=provider.GetRequiredService<CompanyAccountingStepState>();Assert.NotNull(completion.Receipt);Assert.False(completion.HasUnconfirmedIntent);Assert.Equal("RETURN_CORRECTION_WORK",completion.Intent!.Action);return completion.Intent;
    }
    private static void Select(InvoiceVarianceStepEvidenceTests.Fixture f,string actor,string role)
    {
        f.Host.Context.Actor=actor;f.Host.Context.Roles=[role];f.Host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.CorrectInvoice","Accounting.ReviewInvoiceVariance","Workflow.Approve","Workflow.Execute"];
        f.Host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");f.Host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);
        f.Host.Client.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");f.Host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role",role);
        f.Company.TrySelect(f.Company.Clear(),actor,new(f.Invoice.Command.Company,[role],f.Host.Context.Permissions));
    }
    [Theory][InlineData("valid")][InlineData("retry")][InlineData("unknown")][InlineData("late-financial")][InlineData("grant")][InlineData("binding")][InlineData("changed-source")][InlineData("wrong-action")][InlineData("second-cycle")][InlineData("route-failure")]
    public async Task ReturnPreservesPriorWorkAndCreatesOneNewCycleForOriginalPreparer(string mode)
    {
        await using var f=new InvoiceVarianceStepEvidenceTests.Fixture();await f.Prepare(correctionWorkflow:true,acknowledgement:true);f.Host.Context.PolicyBinding=f.Invoice.Command.BindingRevision;
        var request=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;await f.Client.CompleteCompanyAccountingStepAsync(new(request,Guid.NewGuid().ToString("N"),"Complete recorded correction request."));
        var correction=new InvoiceCorrectionCommandTests.Fixture(f.Invoice);await correction.Prepare(alreadyPrepared:true);await correction.Record();
        Select(f,"actor","ConfiguredAccountantRole");var work=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;
        await f.Client.CompleteCompanyAccountingStepAsync(new(work,Guid.NewGuid().ToString("N"),"Complete original version-one correction work."));
        var oldWork=f.Process.Native.Rows.OfType<CROSS_PERSONA_TASK>().Where(row=>row.PROCESS_STEP_INSTANCE_ID==work.StepInstanceId).Select(PurchaseReceiptDecisionProof.Copy).ToArray();
        await correction.PrepareNext(4m);await correction.Record();Select(f,"reviewer","ConfiguredControllerRole");
        var review=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;Assert.False(review.Eligible);Assert.Equal("CORRECTION_WORK_EVIDENCE_REQUIRED",review.Eligibility);Assert.Equal(CompanyAccountingStepEvidence.InvoiceCorrectionReturn,review.CorrectionEvidence!.Action);Assert.Equal(2,review.CorrectionEvidence.CorrectionVersion);
        var command=new CompanyAccountingStepCommand(review,Guid.NewGuid().ToString("N"),"Return latest correction to original preparer; prior work remains audited.","RETURN_CORRECTION_WORK");var writes=f.Process.Native.Writes;
        var native=f.Invoice.Existing.Existing.ReadOwner.Native;
        if(mode=="wrong-action"){Assert.Throws<ArgumentException>(()=>(command with{Action=null}).Require());Assert.Equal(writes,f.Process.Native.Writes);return;}
        if(mode=="grant")f.Host.Context.Permissions=["Accounting.View","Procurement.View","Workflow.Approve"];
        if(mode=="binding")f.Host.Context.PolicyBinding=new string('C',64);
        if(mode=="changed-source")native.Native.Rows.OfType<AP_INVOICE_LINE>().Single(row=>row.AP_INVOICE_ID=="invoice").ITEM_DESCRIPTION="Unverified source change";
        if(mode is "grant" or "binding" or "changed-source")
        {var failure=await Assert.ThrowsAsync<OilGasApiException>(()=>f.Client.CompleteCompanyAccountingStepAsync(command));Assert.Equal(mode=="grant"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,failure.StatusCode);Assert.Equal(writes,f.Process.Native.Writes);return;}
                if(mode=="route-failure")
        {
            f.Process.Native.Personas.Setup(reader=>reader.GetActivePersonasAsync("ConfiguredAccountantRole")).ReturnsAsync(new List<string>());
            var failure=await Assert.ThrowsAsync<OilGasApiException>(()=>f.Client.CompleteCompanyAccountingStepAsync(command));Assert.Equal(HttpStatusCode.Conflict,failure.StatusCode);
            var unchanged=(await f.Process.Service.GetProcessInstanceAsync(f.Id))!;Assert.Equal(StepStatus.PENDING,unchanged.StepInstances.Single(row=>row.StepInstanceId==review.StepInstanceId).Status);
            Assert.False(unchanged.ProcessData.Data.ContainsKey(CompanyAccountingCorrectionCycle.PayloadKey));Assert.DoesNotContain(unchanged.History,row=>row.Action=="CORRECTION_RETURNED");Assert.Contains("rollback",f.Process.Native.Transactions);return;
        }
        if(mode=="unknown")f.Process.Native.FailCommit=true;
        if(mode=="late-financial")f.Process.Native.AfterCommit=()=>native.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(mode is "unknown" or "late-financial")
        {
            await Assert.ThrowsAsync<OilGasApiException>(()=>f.Client.CompleteCompanyAccountingStepAsync(command));Assert.True(f.Process.Native.Writes>writes);writes=f.Process.Native.Writes;
            f.Process.Native.FailCommit=false;f.Process.Native.AfterCommit=null;native.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=3m;
        }
        if(mode=="valid")command=await ReturnThroughScreen(f,command.Notes);
        var receipt=await f.Client.CompleteCompanyAccountingStepAsync(command);receipt.Require(command);Assert.Equal(work.StepInstanceId,receipt.NextStepInstanceId);
        if(mode is "unknown" or "late-financial")Assert.Equal(writes,f.Process.Native.Writes);
        var returned=(await f.Process.Service.GetProcessInstanceAsync(f.Id))!;Assert.Equal(StepStatus.BLOCKED,returned.StepInstances.Single(row=>row.StepInstanceId==review.StepInstanceId).Status);
        Assert.Equal("CORRECTION_RETURNED",returned.History.Single(row=>row.HistoryId==receipt.HistoryId).Action);
        foreach(var old in oldWork)Assert.Equal(JsonSerializer.Serialize(old),JsonSerializer.Serialize(f.Process.Native.Rows.OfType<CROSS_PERSONA_TASK>().Single(row=>row.CROSS_TASK_ID==old.CROSS_TASK_ID)));
        var newWork=Assert.Single(f.Process.Native.Rows.OfType<CROSS_PERSONA_TASK>().Where(row=>row.PROCESS_STEP_INSTANCE_ID==work.StepInstanceId && row.TASK_STATUS=="PENDING"));Assert.DoesNotContain(oldWork,row=>row.CROSS_TASK_ID==newWork.CROSS_TASK_ID);Assert.Equal(command.IntentId,CompanyAccountingCorrectionCycle.TaskRound(newWork.TASK_CONTEXT_JSON));
        Assert.Single(f.Process.Native.Rows.OfType<PROCESS_APPROVAL>());
        if(mode=="retry"){writes=f.Process.Native.Writes;var retry=await f.Client.CompleteCompanyAccountingStepAsync(command);Assert.Equal(receipt.RecordedRevision,retry.RecordedRevision);Assert.Equal(writes,f.Process.Native.Writes);}
        Select(f,"actor","ConfiguredAccountantRole");var reopened=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;Assert.True(reopened.Eligible);Assert.Equal(2,reopened.CorrectionEvidence!.CorrectionVersion);
        await f.Client.CompleteCompanyAccountingStepAsync(new(reopened,Guid.NewGuid().ToString("N"),"Complete confirmed version-two correction work."));
        Select(f,"reviewer","ConfiguredControllerRole");var acknowledgement=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;
        if(mode=="second-cycle")
        {
            await correction.PrepareNext(5m);await correction.Record();var nextReturn=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;
            var nextCommand=new CompanyAccountingStepCommand(nextReturn,Guid.NewGuid().ToString("N"),"Return latest version-three correction.","RETURN_CORRECTION_WORK");await f.Client.CompleteCompanyAccountingStepAsync(nextCommand);
            Select(f,"actor","ConfiguredAccountantRole");var latest=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;Assert.Equal(3,latest.CorrectionEvidence!.CorrectionVersion);await f.Client.CompleteCompanyAccountingStepAsync(new(latest,Guid.NewGuid().ToString("N"),"Confirm latest version-three work."));
            Select(f,"reviewer","ConfiguredControllerRole");acknowledgement=(await f.Client.ReviewCompanyAccountingStepAsync(f.Id))!;
        }
        Assert.True(acknowledgement.Eligible);await f.Client.CompleteCompanyAccountingStepAsync(new(acknowledgement,Guid.NewGuid().ToString("N"),"Independently acknowledge exact latest correction work."));
        Assert.Equal(ProcessStatus.COMPLETED,(await f.Process.Service.GetProcessInstanceAsync(f.Id))!.Status);Assert.Single(f.Process.Native.Rows.OfType<PROCESS_APPROVAL>().Where(row=>row.APPROVAL_TYPE=="CORRECTION_ACKNOWLEDGEMENT"));Assert.Equal("DRAFT",f.Invoice.Existing.Stored.STATUS);Assert.Equal(mode=="second-cycle"?8:6,native.Native.Writes);
    }
}
#endif
