#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InvoiceCorrectionDecisionReviewTests
{
    [Theory][InlineData("valid")][InlineData("preparer")][InlineData("maker")][InlineData("binding")][InlineData("history")][InlineData("policy")][InlineData("consumption")][InlineData("source")][InlineData("actor")][InlineData("revision")]
    public async Task CanonicalIndependentReviewRequiresLatestVerifiedCorrectionAndCompleteCurrentFacts(string fault)
    {
        var f=new InvoiceCorrectionCommandTests.Fixture();await f.Prepare();await f.Record();var actor=fault=="preparer"?"actor":fault=="maker"?"source-maker":"reviewer";
        var current=await InvoiceVarianceDecisionReviewTests.Financial(f.Existing.Existing,actor);var binding=f.Command.BindingRevision;
        if(fault=="binding")binding=new string('C',64);
        if(fault=="policy")current=APInvoiceFinancialReview.Create(current.Invoice,current.PurchaseLines,null,current.ReceiptConsumption);
        if(fault=="consumption")current=APInvoiceFinancialReview.Create(current.Invoice,current.PurchaseLines,current.MatchingPolicies,null);
        if(fault is "preparer" or "maker")Assert.Throws<UnauthorizedAccessException>(()=>APInvoiceCorrectionDecisionReview.Create(current,binding));
        else if(fault is "binding" or "policy" or "consumption")Assert.Throws<InvalidOperationException>(()=>APInvoiceCorrectionDecisionReview.Create(current,binding));
        else
        {
            var review=APInvoiceCorrectionDecisionReview.Create(current,binding);var source=current.Invoice.Revision;
            if(fault=="history")review=review with{RecordedHistory=review.RecordedHistory with{Versions=[]}};
            if(fault=="revision")review=review with{Revision=new string('C',64)};
            if(fault!="valid")Assert.Throws<InvalidOperationException>(()=>review.Require(f.Command.Company,fault=="actor"?"another":actor,"invoice",fault=="source"?new string('C',64):source));
            else
            {
                review.Require(f.Command.Company,actor,"invoice",source);Assert.Equal(1,review.CorrectionVersion);Assert.Equal(f.Command.IntentId,review.CorrectionIntentId);Assert.Equal(3m,review.Current.Invoice.Invoice.TOTAL_AMOUNT);Assert.Equal(2.01m,review.RecordedHistory.OriginalInvoice.TOTAL_AMOUNT);
                Assert.Equal(ProcessDefinitionRevision.FromJson(current.Invoice.Invoice.CORRECTION_HISTORY_JSON!),review.HistoryRevision);
                current.Invoice.Invoice.TOTAL_AMOUNT=99m;review.Require(f.Command.Company,actor,"invoice",source);Assert.Equal("REQUEST_CORRECTION",APInvoiceVarianceDecisionProof.Require(f.Command.Company,review.Current.Invoice.Invoice).Command.Disposition);
            }
        }
        Assert.Equal(4,f.Existing.Existing.Existing.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("valid")][InlineData("grant")][InlineData("role")][InlineData("binding")][InlineData("source")][InlineData("line")][InlineData("contributor")][InlineData("late-grant")][InlineData("preparer")][InlineData("maker")][InlineData("missing-history")][InlineData("second-version")]
    public async Task ActualApiAndTypedClientReviewCurrentRecordedCorrectionIndependentlyWithoutWrites(string fault)
    {
        var f=new InvoiceCorrectionCommandTests.Fixture();await f.Prepare();var saved=await f.Record();var n=f.Existing.Existing.Existing.ReadOwner.Native;
        if(fault=="second-version"){await f.PrepareNext(4m);saved=await f.Record();}
        var actor=fault=="preparer"?"actor":fault=="maker"?"source-maker":"reviewer";var current=await InvoiceVarianceDecisionReviewTests.Financial(f.Existing.Existing,actor);var writes=n.Native.Writes;
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Existing.Existing.Existing.Owner);services.AddSingleton(n.AP);services.AddSingleton(n.PO);});
        host.Context.Actor=actor;host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);host.Context.PolicyBinding=f.Command.BindingRevision;host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.ReviewInvoiceVariance"];
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(f.Command.Company,["ConfiguredCompanyRole"],host.Context.Permissions));var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(fault=="grant")host.Context.Permissions=["Accounting.View","Procurement.View"];if(fault=="role")host.Context.Roles=[];if(fault=="binding")host.Context.ChangeBinding=true;
        if(fault=="source")f.Existing.Existing.Stored.TOTAL_AMOUNT=99m;if(fault=="line")n.Native.Rows.OfType<AP_INVOICE_LINE>().Single(row=>row.AP_INVOICE_ID=="invoice").ITEM_DESCRIPTION="Changed after correction";
        if(fault=="contributor")n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(fault=="late-grant")host.Context.OnCapture=read=>{if(read==3)host.Context.Permissions=["Accounting.View","Procurement.View"];};
        if(fault=="missing-history")f.Existing.Existing.Stored.CORRECTION_HISTORY_JSON=null;
        if(fault is "valid" or "contributor" or "second-version")
        {
            var review=await client.GetAPInvoiceCorrectionDecisionReviewAsync("invoice",current.Invoice.Revision);review.Require(f.Command.Company,actor,"invoice",current.Invoice.Revision);
            Assert.Equal(fault=="second-version"?2:1,review.CorrectionVersion);Assert.Equal(saved.Command.IntentId,review.CorrectionIntentId);
            if(fault=="contributor")Assert.NotEqual(current.Revision,review.Current.Revision);else Assert.Equal(current.Revision,review.Current.Revision);
        }
        else
        {
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetAPInvoiceCorrectionDecisionReviewAsync("invoice",current.Invoice.Revision));
            Assert.Equal(fault is "grant" or "role" or "binding" or "late-grant" or "preparer" or "maker"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);
        }
        Assert.Equal(writes,n.Native.Writes);Assert.Equal("DRAFT",f.Existing.Existing.Stored.STATUS);
    }
}
#endif
