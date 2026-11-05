using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InvoiceCorrectionHttpTests
{
    private static CompanyClientContext Selected()
    {var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Procurement.View","Accounting.CorrectInvoice"]));return company;}
    private static async Task<AccountingSourceHttpTests.Host> Host(InvoiceCorrectionCommandTests.Fixture f)
    {
        var n=f.Existing.Existing.Existing.ReadOwner.Native;
        var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Existing.Existing.Existing.Owner);services.AddSingleton(n.AP);services.AddSingleton(n.PO);});
        host.Context.PolicyBinding=f.Command.BindingRevision;host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.CorrectInvoice"];return host;
    }
    [Theory][InlineData("valid")][InlineData("grant")][InlineData("role")][InlineData("binding")][InlineData("invoice")][InlineData("line")][InlineData("contributor")][InlineData("late-grant")][InlineData("late-role")][InlineData("late-contributor")][InlineData("other-account")][InlineData("route")][InlineData("company")]
    public async Task ActualApiAndTypedClientRecordUnderIndependentCurrentAuthorityAndFreshOwnedEvidence(string fault)
    {
        var f=new InvoiceCorrectionCommandTests.Fixture();await f.Prepare();var n=f.Existing.Existing.Existing.ReadOwner.Native;
        await using var host=await Host(f);using var company=Selected();var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);var input=new APInvoiceCorrectionSubmission(f.Command,f.Review);
        if(fault=="grant")host.Context.Permissions=["Accounting.View","Procurement.View"];if(fault=="role")host.Context.Roles=[];if(fault=="binding")host.Context.PolicyBinding=new string('C',64);
        if(fault=="invoice")f.Existing.Existing.Stored.TOTAL_AMOUNT=99m;if(fault=="line")n.Line.ITEM_DESCRIPTION="changed";if(fault=="contributor")n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(fault=="late-grant")f.OnLine=()=>host.Context.Permissions=["Accounting.View","Procurement.View"];if(fault=="late-role")f.OnLine=()=>host.Context.Roles=[];if(fault=="late-contributor")f.OnLine=()=>n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(fault=="other-account"){host.Context.Actor="another";host.Client.DefaultRequestHeaders.Add("X-Test-Actor","another");}
        if(fault is "route" or "company")
        {
            if(fault=="company")host.Client.DefaultRequestHeaders.Add("X-Test-Company","another");
            using var response=await host.Client.PostAsJsonAsync("api/accounting/traditional/ap/"+(fault=="route"?"another":"invoice")+"/correction-commands",input);Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);Assert.Equal(2,n.Native.Writes);return;
        }
        if(fault=="valid")
        {
            var result=await client.RecordAPInvoiceCorrectionAsync(input);result.Require(input);Assert.Equal(3m,result.Invoice.TOTAL_AMOUNT);Assert.Equal("DRAFT",result.Invoice.STATUS);
            var retry=await client.RecordAPInvoiceCorrectionAsync(input);retry.Require(input);Assert.Equal(result.Invoice.CORRECTION_HISTORY_JSON,retry.Invoice.CORRECTION_HISTORY_JSON);Assert.Equal(4,n.Native.Writes);
            var source=await client.GetAPInvoiceAsync("invoice");Assert.Equal(result.Invoice.CORRECTION_HISTORY_JSON,source!.Invoice.CORRECTION_HISTORY_JSON);Assert.Equal(3m,Assert.Single(source.Lines).LINE_TOTAL);
        }
        else
        {
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordAPInvoiceCorrectionAsync(input));
            Assert.Equal(fault is "grant" or "role" or "binding" or "late-grant" or "late-role" or "other-account"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);
            Assert.Null(f.Existing.Existing.Stored.CORRECTION_HISTORY_JSON);if(fault.StartsWith("late-",StringComparison.Ordinal))Assert.Equal(new[]{"begin","rollback"},n.Native.Transactions);else Assert.Equal(2,n.Native.Writes);
        }
    }
    [Theory][InlineData("valid")][InlineData("changed-intent")][InlineData("changed-contributor")]
    public async Task ActualUnknownCommitRetainsOriginalCorrectionForExactReconciliationOnly(string fault)
    {
        var f=new InvoiceCorrectionCommandTests.Fixture();await f.Prepare();var n=f.Existing.Existing.Existing.ReadOwner.Native.Native;await using var host=await Host(f);using var company=Selected();
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);var input=new APInvoiceCorrectionSubmission(f.Command,f.Review);n.FailCommit=true;
        var failure=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordAPInvoiceCorrectionAsync(input));Assert.Equal(HttpStatusCode.Conflict,failure.StatusCode);Assert.NotNull(f.Existing.Existing.Stored.CORRECTION_HISTORY_JSON);Assert.Equal(4,n.Writes);n.FailCommit=false;
        if(fault=="changed-intent")input=input with{Command=input.Command with{IntentId=Guid.NewGuid().ToString("N")}};if(fault=="changed-contributor")n.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(fault=="valid")(await client.RecordAPInvoiceCorrectionAsync(input)).Require(input);else{var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordAPInvoiceCorrectionAsync(input));Assert.Equal(HttpStatusCode.Conflict,error.StatusCode);}Assert.Equal(4,n.Writes);
    }
    [Theory][InlineData("company")][InlineData("account")]
    public async Task TypedClientRefusesSubmissionOutsideItsOriginalSelectedContext(string fault)
    {
        var f=new InvoiceCorrectionCommandTests.Fixture();await f.Prepare();await using var host=await Host(f);using var company=Selected();
        company.TrySelect(company.Clear(),fault=="account"?"another":"actor",new(new("tenant",fault=="company"?"another":"company"),["ConfiguredCompanyRole"],["Accounting.View","Procurement.View","Accounting.CorrectInvoice"]));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>client.RecordAPInvoiceCorrectionAsync(new(f.Command,f.Review)));Assert.Equal(0,host.Context.Calls);Assert.Equal(2,f.Existing.Existing.Existing.ReadOwner.Native.Native.Writes);
    }
    private sealed class AfterResponse(Func<HttpResponseMessage,Task> change):DelegatingHandler(new HttpClientHandler())
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {var response=await base.SendAsync(request,cancellationToken);if(response.IsSuccessStatusCode)await change(response);return response;}
    }
    [Theory][InlineData("company")][InlineData("account")][InlineData("lines")][InlineData("intent")]
    public async Task TypedClientCannotPublishCommittedResponseUnderChangedContextOrAlteredResult(string fault)
    {
        var f=new InvoiceCorrectionCommandTests.Fixture();await f.Prepare();await using var host=await Host(f);using var company=Selected();
        using var transport=new HttpClient(new AfterResponse(async response=>
        {
            if(fault is "company" or "account")company.TrySelect(company.Clear(),fault=="account"?"another":"actor",new(new("tenant",fault=="company"?"another":"company"),["ConfiguredCompanyRole"],["Accounting.View","Procurement.View","Accounting.CorrectInvoice"]));
            else
            {
                var result=(await response.Content.ReadFromJsonAsync<APInvoiceCorrectionResult>())!;
                result=fault=="lines"?result with{Lines=null!}:result with{Command=result.Command with{IntentId=Guid.NewGuid().ToString("N")}};
                response.Content.Dispose();response.Content=JsonContent.Create(result);
            }
        })){BaseAddress=host.Client.BaseAddress};transport.DefaultRequestHeaders.Add("X-Test-Auth","yes");transport.DefaultRequestHeaders.Add("X-Test-Role","yes");
        var client=new AccountingServiceClient(new ApiClient(transport,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        await Assert.ThrowsAnyAsync<Exception>(()=>client.RecordAPInvoiceCorrectionAsync(new(f.Command,f.Review)));Assert.Equal(4,f.Existing.Existing.Existing.ReadOwner.Native.Native.Writes);
        Assert.Equal(3m,f.Existing.Existing.Stored.TOTAL_AMOUNT);Assert.NotNull(f.Existing.Existing.Stored.CORRECTION_HISTORY_JSON);
    }
}
#endif
