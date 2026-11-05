using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyStepCommandHttpTests
{
    private static readonly string[] Permissions=["Accounting.View","Procurement.View","Workflow.Start","Workflow.Approve"];
    private const string Route="/api/accounting/processes/step-complete";
    [Theory][InlineData("AP_INVOICE")][InlineData("PURCHASE_ORDER")]
    public async Task TypedCompanyStepCommandConfirmsRecordedOutcomeAndExactRetry(string type)
    {
        var (native,command)=await CompanyStepCommandTests.Prepare(type:type);await using var host=await CompanyProcessStartHttpTests.Host(native,permissions:Permissions);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],Permissions));
        var client=new BusinessProcessServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var receipt=await client.CompleteCompanyAccountingStepAsync(command);receipt.Require(command);
        var process=await client.GetCompanyAccountingInstanceAsync(command.Review.InstanceId);Assert.Equal("post",process!.CurrentStepId);Assert.Contains(process.History,row=>row.HistoryId==receipt.HistoryId);
        var retry=await client.CompleteCompanyAccountingStepAsync(command);Assert.Equal(JsonSerializer.Serialize(receipt),JsonSerializer.Serialize(retry));Assert.Equal(16,native.Native.Writes);
    }
    [Theory][InlineData("auth",401)][InlineData("role",403)][InlineData("permission",403)][InlineData("company",403)][InlineData("actor",403)][InlineData("binding",403)]
    [InlineData("source",409)][InlineData("definition",409)][InlineData("revision",409)][InlineData("process",409)][InlineData("maker",409)][InlineData("step",409)]
    public async Task DeniedOrChangedAssertionsCannotMutateOriginalHandoff(string fault,int status)
    {
        var (native,command)=await CompanyStepCommandTests.Prepare();await using var host=await CompanyProcessStartHttpTests.Host(native,permissions:Permissions);
        switch(fault)
        {
            case "auth":host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");break;
            case "role":host.Client.DefaultRequestHeaders.Remove("X-Test-Role");break;
            case "permission":host.Context.Permissions=["Accounting.View"];break;
            case "company":command=command with{Review=command.Review with{Company=new("tenant","second")}};break;
            case "actor":command=command with{Review=command.Review with{ActorUserId="other"}};break;
            case "binding":command=command with{Review=command.Review with{BindingRevision=new string('C',64)}};break;
            case "source":command=command with{Review=command.Review with{SourceRevision=new string('C',64)}};break;
            case "definition":command=command with{Review=command.Review with{DefinitionRevision=new string('C',64)}};break;
            case "revision":command=command with{Review=command.Review with{Revision=new string('C',64)}};break;
            case "process":command=command with{Review=command.Review with{ProcessRevision=new string('C',64)}};break;
            case "maker":command=command with{Review=command.Review with{SourceMakerUserId="other"}};break;
            case "step":command=command with{Review=command.Review with{StepInstanceId="other"}};break;
        }
        var response=await host.Client.PostAsJsonAsync(Route,command);Assert.Equal(status,(int)response.StatusCode);Assert.Equal(6,native.Native.Writes);Assert.DoesNotContain("RecordedRevision",await response.Content.ReadAsStringAsync());
    }
    [Fact] public async Task AuthorityLossAfterTaskClosureRollsBackEveryCommandRecord()
    {
        var (native,command)=await CompanyStepCommandTests.Prepare();await using var host=await CompanyProcessStartHttpTests.Host(native,permissions:Permissions);
        var before=JsonSerializer.Serialize(native.Native.Rows);host.Context.OnCapture=_=>{if(native.Native.Writes>6)host.Context.Permissions=["Accounting.View"];};
        var response=await host.Client.PostAsJsonAsync(Route,command);Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);Assert.Equal(before,JsonSerializer.Serialize(native.Native.Rows));Assert.Equal("rollback",native.Native.Transactions.Last());
    }
    [Fact] public async Task PostCommitSourceDriftIsUnconfirmedAndOriginalIntentReconcilesAfterSourceRestoration()
    {
        var (native,command)=await CompanyStepCommandTests.Prepare();await using var host=await CompanyProcessStartHttpTests.Host(native,permissions:Permissions);
        host.Context.OnCapture=_=>{if(native.Native.Writes>6)host.Context.SourceEdited=true;};
        var response=await host.Client.PostAsJsonAsync(Route,command);Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.Equal(16,native.Native.Writes);Assert.Equal("commit",native.Native.Transactions.Last());
        host.Context.OnCapture=null;host.Context.SourceEdited=false;
        var retry=await host.Client.PostAsJsonAsync(Route,command);Assert.Equal(HttpStatusCode.OK,retry.StatusCode);(await retry.Content.ReadFromJsonAsync<CompanyAccountingStepReceipt>())!.Require(command);Assert.Equal(16,native.Native.Writes);
        var changed=await host.Client.PostAsJsonAsync(Route,command with{Notes="different"});Assert.Equal(HttpStatusCode.Conflict,changed.StatusCode);Assert.Equal(16,native.Native.Writes);
    }
    [Fact] public async Task MissingRecordedTaskSetCannotBeRepairedThroughCommandRetry()
    {
        var (native,command)=await CompanyStepCommandTests.Prepare();await using var host=await CompanyProcessStartHttpTests.Host(native,permissions:Permissions);
        var response=await host.Client.PostAsJsonAsync(Route,command);response.EnsureSuccessStatusCode();var receipt=(await response.Content.ReadFromJsonAsync<CompanyAccountingStepReceipt>())!;
        native.Native.Rows.RemoveAll(row=>row is CROSS_PERSONA_TASK task && receipt.CompletedTaskIds.Contains(task.CROSS_TASK_ID));
        var retry=await host.Client.PostAsJsonAsync(Route,command);Assert.Equal(HttpStatusCode.Conflict,retry.StatusCode);Assert.Equal(16,native.Native.Writes);
    }
    [Theory][InlineData("company")][InlineData("process")][InlineData("account")]
    public async Task CircuitRetainsOriginalIntentAndDiscardsLateRecordedReceipt(string change)
    {
        var (native,command)=await CompanyStepCommandTests.Prepare();await using var host=await CompanyProcessStartHttpTests.Host(native,permissions:Permissions);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],Permissions));
        var client=new BusinessProcessServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        using var state=new CompanyAccountingStepState(company);state.AttachProcess(command.Review.InstanceId,command.Review.ProcessRevision);state.AcceptReview(command.Review);
        var ready=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var boundary=new Mock<IBusinessProcessServiceClient>();
        boundary.Setup(value=>value.CompleteCompanyAccountingStepAsync(It.IsAny<CompanyAccountingStepCommand>(),It.IsAny<CancellationToken>())).Returns(async(CompanyAccountingStepCommand intent,CancellationToken token)=>{
            var result=await client.CompleteCompanyAccountingStepAsync(intent,token);ready.SetResult();await release.Task;return result;});
        var submission=state.SubmitAsync(boundary.Object,command.Notes);await ready.Task;
        Assert.True(state.Busy);var original=state.Intent;await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitAsync(client,"duplicate"));
        if(change=="process")state.AttachProcess("another-process",command.Review.ProcessRevision);
        else company.TrySelect(company.Clear(),change=="account"?"other":"actor",new(new("tenant",change=="company"?"second":"company"),["ConfiguredCompanyRole"],Permissions));
        release.SetResult();await submission;
        Assert.Null(state.Receipt);Assert.Null(state.Review);Assert.True(state.HasUnconfirmedIntent);Assert.False(state.CanReconcile);Assert.Equal(original,state.Intent);Assert.False(state.Busy);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitAsync(client,"replacement"));Assert.Equal(16,native.Native.Writes);
        company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],Permissions));
        var process=await client.GetCompanyAccountingInstanceAsync(command.Review.InstanceId);
        state.AttachProcess(command.Review.InstanceId,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process)));
        await state.SubmitAsync(client,"replacement");Assert.NotNull(state.Receipt);Assert.Equal(original,state.Intent);Assert.Equal(command.Notes,state.Intent!.Notes);Assert.Equal(16,native.Native.Writes);
        var receipt=state.Receipt!;((IList<string>)receipt.CompletedTaskIds)[0]="consumer mutation";Assert.DoesNotContain("consumer mutation",state.Receipt!.CompletedTaskIds);
    }
    [Theory][InlineData("missing")][InlineData("history")][InlineData("source")]
    public async Task CircuitRequiresAuthoritativeProcessReadbackBeforeShowingCompletion(string fault)
    {
        var (native,command)=await CompanyStepCommandTests.Prepare();await using var host=await CompanyProcessStartHttpTests.Host(native,permissions:Permissions);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],Permissions));
        var client=new BusinessProcessServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        using var state=new CompanyAccountingStepState(company);state.AttachProcess(command.Review.InstanceId,command.Review.ProcessRevision);state.AcceptReview(command.Review);
        var boundary=new Mock<IBusinessProcessServiceClient>();
        boundary.Setup(value=>value.CompleteCompanyAccountingStepAsync(It.IsAny<CompanyAccountingStepCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyAccountingStepCommand intent,CancellationToken token)=>client.CompleteCompanyAccountingStepAsync(intent,token));
        boundary.Setup(value=>value.GetCompanyAccountingInstanceAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns(async(string id,CancellationToken token)=>{
            var process=await client.GetCompanyAccountingInstanceAsync(id,token);if(fault=="missing")return null;
            if(fault=="history")process!.History.Single(row=>row.Action=="STEP_COMPLETED").Notes="different";
            else process!.ProcessId="different";return process;});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitAsync(boundary.Object,command.Notes));
        var retained=state.Intent;Assert.Null(state.Receipt);Assert.True(state.HasUnconfirmedIntent);Assert.Null(state.Review);Assert.Equal(16,native.Native.Writes);
        await state.SubmitAsync(client,"different reason");Assert.NotNull(state.Receipt);Assert.Equal(retained,state.Intent);Assert.Equal(16,native.Native.Writes);
    }
    private sealed class ChangeAfterResponse(HttpClient target,Action change) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {
            using var forwarded=new HttpRequestMessage(request.Method,request.RequestUri);
            foreach(var header in request.Headers)forwarded.Headers.TryAddWithoutValidation(header.Key,header.Value);
            if(request.Content is not null)
            {
                forwarded.Content=new ByteArrayContent(await request.Content.ReadAsByteArrayAsync(cancellationToken));
                foreach(var header in request.Content.Headers)forwarded.Content.Headers.TryAddWithoutValidation(header.Key,header.Value);
            }
            var response=await target.SendAsync(forwarded,cancellationToken);if(request.RequestUri!.AbsolutePath==Route)change();return response;
        }
    }
    [Fact] public async Task TypedClientDiscardsPostCommitReceiptWhenCompanySelectionChanges()
    {
        var (native,command)=await CompanyStepCommandTests.Prepare();await using var host=await CompanyProcessStartHttpTests.Host(native,permissions:Permissions);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],Permissions));
        using var transport=new HttpClient(new ChangeAfterResponse(host.Client,()=>company.Clear())){BaseAddress=host.Client.BaseAddress};
        var client=new BusinessProcessServiceClient(new ApiClient(transport,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var failure=await Assert.ThrowsAnyAsync<Exception>(()=>client.CompleteCompanyAccountingStepAsync(command));Assert.True(failure is OperationCanceledException or InvalidOperationException);Assert.Equal(16,native.Native.Writes);Assert.Null(company.Selection);
        Assert.Equal(command,await native.Service.GetRecordedCompanyAccountingStepCommandAsync(command.Review.InstanceId));
    }
}
#endif
