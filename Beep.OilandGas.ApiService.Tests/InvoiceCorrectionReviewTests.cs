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
public sealed class InvoiceCorrectionReviewTests
{
    private static APInvoiceCorrectionDraft Draft(APInvoiceFinancialReview current,APInvoiceVarianceDecisionCommand command)=>
        new(current.Invoice.Revision,current.Revision,command.IntentId,APInvoiceCorrectionReview.DecisionRevision(current.Invoice.Invoice.VARIANCE_DECISION_JSON!),3m,
            "Correct billed quantity and price as requested by the independent controller.",new APInvoiceCorrectionLine[]{new("line",3m,1m,3m,"Corrected billed quantity and price")});
    [Theory][InlineData("valid")][InlineData("no-op")][InlineData("total")][InlineData("line-total")][InlineData("duplicate")][InlineData("missing")][InlineData("foreign-line")][InlineData("decision-hash")][InlineData("financial")][InlineData("decision")][InlineData("source")][InlineData("binding")][InlineData("control")][InlineData("reason")][InlineData("other-preparer")][InlineData("accepted")]
    public async Task CorrectionReviewRequiresExactCompleteValuesAndOriginalRequestedCorrection(string fault)
    {
        var f=new InvoiceVarianceDecisionCommandTests.Fixture();await f.Prepare(within:fault=="accepted");await f.Record();
        var current=await InvoiceVarianceDecisionReviewTests.Financial(f.Existing,fault=="other-preparer"?fault:"actor");var draft=Draft(current,f.Command);var binding=f.Command.BindingRevision;
        if(fault=="no-op"){var line=current.Invoice.Lines.Single();draft=draft with{TotalAmount=current.Invoice.Invoice.TOTAL_AMOUNT!.Value,Lines=[new(line.AP_INVOICE_LINE_ID,line.QUANTITY!.Value,line.UNIT_PRICE!.Value,line.LINE_TOTAL!.Value,line.ITEM_DESCRIPTION)]};}
        if(fault=="total")draft=draft with{TotalAmount=4m};if(fault=="line-total")draft=draft with{Lines=[draft.Lines[0] with{LineTotal=2m}]};
        if(fault=="duplicate")draft=draft with{TotalAmount=6m,Lines=[draft.Lines[0],draft.Lines[0]]};if(fault=="missing")draft=draft with{Lines=[]};if(fault=="foreign-line")draft=draft with{Lines=[draft.Lines[0] with{LineId="another-line"}]};
        if(fault=="decision-hash")draft=draft with{ExpectedDecisionRevision=new string('C',64)};if(fault=="financial")draft=draft with{ExpectedFinancialRevision=new string('C',64)};if(fault=="decision")draft=draft with{DecisionIntentId=Guid.NewGuid().ToString("N")};if(fault=="source")draft=draft with{ExpectedInvoiceRevision=new string('C',64)};if(fault=="binding")binding=new string('C',64);if(fault=="control")draft=draft with{Lines=[draft.Lines[0] with{Description="changed\nsource"}]};if(fault=="reason")draft=draft with{Reason=""};
        if(fault=="other-preparer")Assert.Throws<UnauthorizedAccessException>(()=>APInvoiceCorrectionReview.Create(current,draft,binding));
        else if(fault!="valid")Assert.Throws<InvalidOperationException>(()=>APInvoiceCorrectionReview.Create(current,draft,binding));
        else
        {
            var review=APInvoiceCorrectionReview.Create(current,draft,binding);review.Require(f.Command.Company,"actor","invoice",draft);Assert.Equal("REQUEST_CORRECTION",review.RecordedRequest.Command.Disposition);Assert.Equal(3m,review.Draft.TotalAmount);Assert.Equal(2.01m,review.Current.Invoice.Invoice.TOTAL_AMOUNT);
            var original=PurchaseReceiptDecisionProof.Copy(draft);current.Invoice.Invoice.TOTAL_AMOUNT=999m;((APInvoiceCorrectionLine[])draft.Lines)[0]=draft.Lines[0] with{Quantity=999m};review.Require(f.Command.Company,"actor","invoice",original);
            Assert.Throws<InvalidOperationException>(()=>(review with{Revision=new string('C',64)}).Require(f.Command.Company,"actor","invoice",original));
        }
        Assert.Equal(2,f.Existing.Existing.ReadOwner.Native.Native.Writes);Assert.Equal("DRAFT",f.Existing.Stored.STATUS);Assert.Equal(2.01m,f.Existing.Stored.TOTAL_AMOUNT);
    }
    [Theory][InlineData("valid")][InlineData("grant")][InlineData("role")][InlineData("binding")][InlineData("invoice")][InlineData("decision")][InlineData("contributor")][InlineData("late-grant")][InlineData("late-decision")][InlineData("other-preparer")]
    public async Task ActualApiAndTypedClientReviewCorrectionWithoutWritesUnderCurrentAuthority(string fault)
    {
        var f=new InvoiceVarianceDecisionCommandTests.Fixture();await f.Prepare(within:false);await f.Record();var n=f.Existing.Existing.ReadOwner.Native;
        var financial=await InvoiceVarianceDecisionReviewTests.Financial(f.Existing,"actor");var draft=Draft(financial,f.Command);
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Existing.Existing.Owner);services.AddSingleton(n.AP);services.AddSingleton(n.PO);});
        host.Context.PolicyBinding=f.Command.BindingRevision;host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.CorrectInvoice"];
        using var company=new CompanyClientContext();var actor=fault=="other-preparer"?fault:"actor";company.TrySelect(company.Clear(),actor,new(f.Command.Company,["ConfiguredCompanyRole"],host.Context.Permissions));
        if(fault=="other-preparer"){host.Context.Actor=actor;host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);}
        if(fault=="grant")host.Context.Permissions=["Accounting.View","Procurement.View"];if(fault=="role")host.Context.Roles=[];if(fault=="binding")host.Context.ChangeBinding=true;
        if(fault=="invoice")f.Existing.Stored.TOTAL_AMOUNT=99m;if(fault=="decision"){var row=f.Existing.Stored;var proof=APInvoiceVarianceDecisionProof.Require(f.Command.Company,row);row.VARIANCE_DECISION_JSON=APInvoiceVarianceDecisionProof.Create(proof.Command with{Reason="A different recorded controller correction reason."},proof.OriginalInvoice,row);}
        if(fault=="contributor")n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(fault=="late-grant")host.Context.OnCapture=read=>{if(read==3)host.Context.Permissions=["Accounting.View","Procurement.View"];};
        if(fault=="late-decision")host.Context.OnCapture=read=>{if(read==3){var row=f.Existing.Stored;var proof=APInvoiceVarianceDecisionProof.Require(f.Command.Company,row);row.VARIANCE_DECISION_JSON=APInvoiceVarianceDecisionProof.Create(proof.Command with{Reason="Controller reason changed during correction review."},proof.OriginalInvoice,row);}};
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(fault=="valid"){var review=await client.ReviewAPInvoiceCorrectionAsync("invoice",draft);review.Require(f.Command.Company,"actor","invoice",draft);Assert.Equal(3m,review.Draft.TotalAmount);Assert.Equal(2.01m,review.Current.Invoice.Invoice.TOTAL_AMOUNT);}
        else{var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.ReviewAPInvoiceCorrectionAsync("invoice",draft));Assert.Equal(fault is "invoice" or "decision" or "late-decision" or "contributor"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,error.StatusCode);}
        Assert.Equal(2,n.Native.Writes);Assert.Equal(2m,Assert.Single(financial.Invoice.Lines).QUANTITY);
    }
}
#endif
