using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InvoiceVarianceCommandHttpTests
{
    [Theory][InlineData("valid")][InlineData("grant")][InlineData("prepare-grant")][InlineData("role")][InlineData("binding")][InlineData("source")][InlineData("schema")]
    public async Task ActualApiAndTypedClientRecordOnlyUnderOriginalCurrentAuthorityAndEvidence(string fault)
    {
        var f=new InvoiceVarianceCommandTests.Fixture();await f.Prepare();var n=f.Existing.ReadOwner.Native;
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Existing.Owner);services.AddSingleton(n.AP);services.AddSingleton(n.PO);});
        host.Context.PolicyBinding=f.Command.BindingRevision;host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.PrepareInvoiceVariance","Accounting.RecordInvoiceVariance"];
        using var company=InvoiceVariancePreparationTests.Selected();var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var input=new APInvoiceVarianceSubmission(f.Command,f.Review);
        if(fault=="grant")host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.PrepareInvoiceVariance"];if(fault=="prepare-grant")host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.RecordInvoiceVariance"];
        if(fault=="role")host.Client.DefaultRequestHeaders.Remove("X-Test-Role");if(fault=="binding")host.Context.PolicyBinding=new string('C',64);
        if(fault=="source")n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(fault=="schema")n.Native.Source.Setup(source=>source.GetEntityStructure(It.Is<TheTechIdea.Beep.DataBase.EntityStructure>(row=>row.EntityName=="AP_INVOICE"),true)).Returns(new TheTechIdea.Beep.DataBase.EntityStructure());
        if(fault=="valid")
        {
            var result=await client.RecordAPInvoiceVarianceAsync(input);result.Require(input);Assert.Equal("DRAFT",result.Invoice.STATUS);Assert.Equal("actor",result.Invoice.ROW_CHANGED_BY);
            var retry=await client.RecordAPInvoiceVarianceAsync(input);Assert.Equal(result.Invoice.VARIANCE_PROPOSAL_JSON,retry.Invoice.VARIANCE_PROPOSAL_JSON);Assert.Equal(1,n.Native.Writes);
        }
        else{var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordAPInvoiceVarianceAsync(input));Assert.Equal(fault is "source" or "schema"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,error.StatusCode);Assert.Equal(0,n.Native.Writes);}
    }
    [Theory][InlineData("valid")][InlineData("grant")][InlineData("source")]
    public async Task ActualUnknownCommitRetainsExactOriginalIntentForRetryOrRefusal(string retryFault)
    {
        var f=new InvoiceVarianceCommandTests.Fixture();await f.Prepare();var n=f.Existing.ReadOwner.Native;n.Native.FailCommit=true;
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Existing.Owner);services.AddSingleton(n.AP);services.AddSingleton(n.PO);});
        host.Context.PolicyBinding=f.Command.BindingRevision;host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.PrepareInvoiceVariance","Accounting.RecordInvoiceVariance"];
        using var company=InvoiceVariancePreparationTests.Selected();var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);var input=new APInvoiceVarianceSubmission(f.Command,f.Review);
        var unknown=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordAPInvoiceVarianceAsync(input));Assert.Equal(HttpStatusCode.Conflict,unknown.StatusCode);Assert.NotNull(f.Stored.VARIANCE_PROPOSAL_JSON);n.Native.FailCommit=false;
        if(retryFault=="grant")host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.PrepareInvoiceVariance"];
        if(retryFault=="source")n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(retryFault=="valid"){var confirmed=await client.RecordAPInvoiceVarianceAsync(input);confirmed.Require(input);}
        else{var refused=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordAPInvoiceVarianceAsync(input));Assert.Equal(retryFault=="grant"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,refused.StatusCode);}
        Assert.Equal(1,n.Native.Writes);Assert.Equal("DRAFT",f.Stored.STATUS);
    }
}
#endif
