#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class APReceiptAllocationCommandHttpTests
{
    private static readonly string[] Permissions=["Accounting.View","Procurement.View","Accounting.PrepareReceiptAllocation","Accounting.RecordReceiptAllocation"];
    internal static Task<AccountingSourceHttpTests.Host> Host(APReceiptAllocationCommandTests.Fixture fixture)=>AccountingSourceHttpTests.Start(permissions:Permissions,
        configureAccounting:(services,context)=>{services.AddSingleton(fixture.AP);services.AddSingleton(fixture.PO);context.Source=fixture.Native.Source;});
    private static AccountingServiceClient Client(HttpClient http,CompanyClientContext company)=>new(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
    private static void Select(CompanyClientContext company,string actor="actor",string owner="company")=>company.TrySelect(company.Clear(),actor,new(new("tenant",owner),["ConfiguredCompanyRole"],Permissions));
    private static string Route(APReceiptAllocationCommand command)=>"api/accounting/traditional/ap/"+Uri.EscapeDataString(command.InvoiceId)+"/lines/"+Uri.EscapeDataString(command.LineId)+"/receipt-allocations";
    [Fact] public async Task ActualProposalApiBindsOriginalProductionReviewAndTypedRecordingReplaysWithoutDuplicates()
    {
        var fixture=new APReceiptAllocationCommandTests.Fixture();fixture.Invoice.AP_INVOICE_ID="invoice/α%2F";fixture.Line.AP_INVOICE_ID=fixture.Invoice.AP_INVOICE_ID;
        fixture.Line.AP_INVOICE_LINE_ID="line/حساب%2F";
        // Prepare uses fixed native IDs, so this encoded route case obtains all evidence through the actual API.
        await using var host=await Host(fixture);using var company=new CompanyClientContext();Select(company);var client=Client(host.Client,company);
        var invoice=(await client.GetAPInvoiceAsync(fixture.Invoice.AP_INVOICE_ID))!;
        var purchase=await client.GetAPLinePurchaseSourceAsync(fixture.Invoice.AP_INVOICE_ID,fixture.Line.AP_INVOICE_LINE_ID,invoice.Revision);
        var allocations=await client.GetAPReceiptAllocationsAsync(fixture.Invoice.AP_INVOICE_ID,fixture.Line.AP_INVOICE_LINE_ID,invoice.Revision,purchase.Order.Revision);
        var proposal=new APReceiptAllocationProposal("receipt",2m,"bbl",invoice.Revision,purchase.Order.Revision,allocations.Revision);
        var review=await client.ReviewAPReceiptAllocationAsync(fixture.Invoice.AP_INVOICE_ID,fixture.Line.AP_INVOICE_LINE_ID,proposal);
        Assert.Equal("binding",review.BindingRevision);
        var command=new APReceiptAllocationCommand(new("tenant","company"),"actor",fixture.Invoice.AP_INVOICE_ID,fixture.Line.AP_INVOICE_LINE_ID,
            proposal,review.ReceiptConsumption!.Revision,review.Revision,review.BindingRevision!,Guid.NewGuid().ToString("N"),"Reviewed receipt.");
        var submission=new APReceiptAllocationSubmission(command,review);
        var first=await client.RecordAPReceiptAllocationAsync(submission);first.Require(command,review);
        var retry=await client.RecordAPReceiptAllocationAsync(submission);Assert.Equal(first.Allocation.AP_RECEIPT_ALLOCATION_ID,retry.Allocation.AP_RECEIPT_ALLOCATION_ID);
        Assert.Equal(1,fixture.Native.Writes);Assert.Equal("DRAFT",fixture.Invoice.STATUS);
        Assert.Equal(new[]{"begin","commit","begin","commit"},fixture.Native.Transactions);
    }
    [Theory][InlineData("auth",401)][InlineData("role",403)][InlineData("permission",403)][InlineData("preparation-only",403)]
    [InlineData("procurement",403)][InlineData("company",403)][InlineData("actor",403)][InlineData("binding",403)]
    [InlineData("source",409)][InlineData("contributors",409)][InlineData("proposal",409)][InlineData("route",400)]
    public async Task OriginalSourceAndIndependentRecordingAuthorityGateNativeMutation(string fault,int status)
    {
        var fixture=new APReceiptAllocationCommandTests.Fixture();await fixture.Prepare();await using var host=await Host(fixture);
        var command=fixture.Command;var review=fixture.Displayed;var route=Route(command);
        switch(fault)
        {
            case "auth":host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");break;
            case "role":host.Client.DefaultRequestHeaders.Remove("X-Test-Role");break;
            case "permission":host.Context.Permissions=["Accounting.View","Procurement.View"];break;
            case "preparation-only":host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.PrepareReceiptAllocation"];break;
            case "procurement":host.Context.Permissions=["Accounting.View","Accounting.RecordReceiptAllocation"];break;
            case "company":command=command with{Company=new("tenant","other")};break;
            case "actor":command=command with{ActorUserId="other"};break;
            case "binding":command=command with{BindingRevision="changed"};break;
            case "source":fixture.Invoice.TOTAL_AMOUNT=11m;break;
            case "contributors":fixture.AddCompeting(1m);break;
            case "proposal":command=command with{ExpectedProposalRevision=new string('a',64)};break;
            case "route":route=route.Replace("/lines/line/","/lines/other/");break;
        }
        var response=await host.Client.PostAsJsonAsync(route,new APReceiptAllocationSubmission(command,review));
        Assert.Equal(status,(int)response.StatusCode);Assert.Equal(0,fixture.Native.Writes);
        Assert.DoesNotContain(fixture.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>(),row=>row.AP_RECEIPT_ALLOCATION_ID==fixture.Command.AllocationId);
        Assert.DoesNotContain("private provider",await response.Content.ReadAsStringAsync());
    }
    [Theory][InlineData("roles")][InlineData("permissions")][InlineData("binding")][InlineData("repository")]
    public async Task AuthoritySetDriftDuringTransactionRollsBackInsertedAllocation(string fault)
    {
        var fixture=new APReceiptAllocationCommandTests.Fixture();await fixture.Prepare();await using var host=await Host(fixture);
        fixture.OnInsert=()=>{
            if(fault=="roles")host.Context.Roles=["ConfiguredCompanyRole","AnotherRole"];
            if(fault=="permissions")host.Context.Permissions=[..Permissions,"Another.Permission"];
            if(fault=="binding")host.Context.ChangeBinding=true;
            if(fault=="repository")host.Context.OnCapture=_=>throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Forbidden("Current repository grant revoked.");};
        var response=await host.Client.PostAsJsonAsync(Route(fixture.Command),new APReceiptAllocationSubmission(fixture.Command,fixture.Displayed));
        Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);Assert.Empty(fixture.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>());
        Assert.Equal(new[]{"begin","rollback"},fixture.Native.Transactions);
    }
    [Fact] public async Task HttpUncertainCommitAndOriginalTypedRetryConfirmOneRecordedAllocation()
    {
        var fixture=new APReceiptAllocationCommandTests.Fixture();await fixture.Prepare();await using var host=await Host(fixture);
        using var company=new CompanyClientContext();Select(company);var client=Client(host.Client,company);
        var submission=new APReceiptAllocationSubmission(fixture.Command,fixture.Displayed);fixture.Native.FailCommit=true;
        var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordAPReceiptAllocationAsync(submission));Assert.Equal(HttpStatusCode.Conflict,error.StatusCode);
        Assert.Single(fixture.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>());fixture.Native.FailCommit=false;
        var receipt=await client.RecordAPReceiptAllocationAsync(submission);receipt.Require(fixture.Command,fixture.Displayed);Assert.Equal(1,fixture.Native.Writes);
    }
    [Theory][InlineData("company")][InlineData("account")]
    public async Task LateClientContextChangeCannotAdoptACommittedReceipt(string change)
    {
        var fixture=new APReceiptAllocationCommandTests.Fixture();await fixture.Prepare();await using var host=await Host(fixture);
        using var company=new CompanyClientContext();Select(company);
        var handler=new AfterResponseHandler(host.Client,()=>Select(company,change=="account"?"other":"actor",change=="company"?"other":"company"));
        using var http=new HttpClient(handler){BaseAddress=host.Client.BaseAddress};var client=Client(http,company);
        await Assert.ThrowsAsync<OperationCanceledException>(()=>client.RecordAPReceiptAllocationAsync(new(fixture.Command,fixture.Displayed)));
        Assert.Equal(1,fixture.Native.Writes);Assert.Single(fixture.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>());
    }
    private sealed class AfterResponseHandler(HttpClient server,Action change):HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            using var forwarded=new HttpRequestMessage(request.Method,request.RequestUri);
            foreach(var header in request.Headers)forwarded.Headers.TryAddWithoutValidation(header.Key,header.Value);
            if(request.Content is not null)
            {
                forwarded.Content=new ByteArrayContent(await request.Content.ReadAsByteArrayAsync(token));
                foreach(var header in request.Content.Headers)forwarded.Content.Headers.TryAddWithoutValidation(header.Key,header.Value);
            }
            var response=await server.SendAsync(forwarded,token);change();return response;
        }
    }
    [Theory][InlineData("company")][InlineData("account")]
    public async Task WrongOriginalClientSelectionCannotSendAnotherCommand(string change)
    {
        var fixture=new APReceiptAllocationCommandTests.Fixture();await fixture.Prepare();await using var host=await Host(fixture);
        using var company=new CompanyClientContext();Select(company,change=="account"?"other":"actor",change=="company"?"other":"company");
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Client(host.Client,company).RecordAPReceiptAllocationAsync(new(fixture.Command,fixture.Displayed)));
        Assert.Equal(0,fixture.Native.Writes);Assert.Equal(0,host.Context.Calls);
    }
    [Fact] public async Task RecordingGrantDoesNotRequireTheSeparateProposalReadGrantForOriginalRetry()
    {
        var fixture=new APReceiptAllocationCommandTests.Fixture();await fixture.Prepare();await using var host=await Host(fixture);
        host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.RecordReceiptAllocation"];
        var response=await host.Client.PostAsJsonAsync(Route(fixture.Command),new APReceiptAllocationSubmission(fixture.Command,fixture.Displayed));
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);Assert.Equal(1,fixture.Native.Writes);
    }
    [Theory][InlineData("intent")][InlineData("order")][InlineData("quantity")][InlineData("proof")][InlineData("time")][InlineData("row")]
    public async Task CompleteTypedClientRejectsWrongOrIncompleteReturnedOutcomeAfterCommit(string fault)
    {
        var fixture=new APReceiptAllocationCommandTests.Fixture();await fixture.Prepare();await using var host=await Host(fixture);
        using var company=new CompanyClientContext();Select(company);
        using var http=new HttpClient(new AlterResponseHandler(host.Client,async response=>{
            response.EnsureSuccessStatusCode();var receipt=(await response.Content.ReadFromJsonAsync<APReceiptAllocationReceipt>())!;
            if(fault=="intent")receipt=receipt with{Command=receipt.Command with{IntentId=Guid.NewGuid().ToString("N")}};
            if(fault=="order")receipt.Allocation.PURCHASE_ORDER_ID="other-order";
            if(fault=="quantity")receipt.Allocation.ALLOCATED_QUANTITY=3m;
            if(fault is "order" or "quantity")receipt.Allocation.REVIEW_COMMAND_JSON=APReceiptAllocationRecordedProof.Create(receipt.Command,receipt.Allocation);
            if(fault=="proof")receipt.Allocation.REVIEW_COMMAND_JSON="{}";
            if(fault=="time")receipt=receipt with{ConfirmedAtUtc=default};if(fault=="row")receipt=receipt with{Allocation=null!};
            response.Content=JsonContent.Create(receipt);
        })){BaseAddress=host.Client.BaseAddress};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Client(http,company).RecordAPReceiptAllocationAsync(new(fixture.Command,fixture.Displayed)));
        Assert.Equal(1,fixture.Native.Writes);Assert.Single(fixture.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>());
    }
    private sealed class AlterResponseHandler(HttpClient server,Func<HttpResponseMessage,Task> alter):HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            using var forwarded=new HttpRequestMessage(request.Method,request.RequestUri);
            foreach(var header in request.Headers)forwarded.Headers.TryAddWithoutValidation(header.Key,header.Value);
            if(request.Content is not null)
            {
                forwarded.Content=new ByteArrayContent(await request.Content.ReadAsByteArrayAsync(token));
                foreach(var header in request.Content.Headers)forwarded.Content.Headers.TryAddWithoutValidation(header.Key,header.Value);
            }
            var response=await server.SendAsync(forwarded,token);await alter(response);return response;
        }
    }
}
#endif
