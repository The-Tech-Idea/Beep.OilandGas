using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InvoiceVarianceWorkspaceTests
{
    internal static CompanyClientContext Selected(bool grant=true)
    {var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],grant?["Accounting.View","Procurement.View","Accounting.PrepareInvoiceVariance","Accounting.RecordInvoiceVariance"]:["Accounting.View","Procurement.View","Accounting.PrepareInvoiceVariance"]));return company;}
    internal static async Task Prepare(APInvoiceWorkspaceState state,InvoiceVarianceCommandTests.Fixture f)
    {
        await state.LoadDetailAsync("invoice",id=>f.Existing.ReadOwner.Native.AP.GetSourceReviewAsync(new("tenant","company"),id,"actor"));
        await state.LoadFinancialReviewAsync((_,_)=>InvoiceVariancePreparationTests.Financial(f.Existing));var proposal=f.Review.Proposal;
        await state.ReviewVarianceProposalAsync(proposal.Disposition,proposal.Explanations,proposal.Reason,(_,value)=>Task.FromResult(APInvoiceVarianceProposalReview.Create(state.FinancialReview!,value,new string('B',64))));
    }
    internal static async Task<APInvoiceVarianceResult> Write(InvoiceVarianceCommandTests.Fixture f,APInvoiceVarianceSubmission input)=>new(input.Command,
        await f.Existing.ReadOwner.Native.AP.RecordVarianceProposalAsync(input.Command,input.Review,f.Existing.ReadOwner.Native.Authority,()=>InvoiceVariancePreparationTests.Financial(f.Existing)));
    internal static Task<APInvoiceSourceReview?> Read(InvoiceVarianceCommandTests.Fixture f,string id)=>f.Existing.ReadOwner.Native.AP.GetSourceReviewAsync(new("tenant","company"),id,"actor");
    [Fact] public async Task WorkspaceConfirmsFullResultAndCurrentSourceThenReloadsRecordedProposal()
    {
        var f=new InvoiceVarianceCommandTests.Fixture();await f.Prepare();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await Prepare(state,f);
        await state.RecordVarianceAsync(input=>Write(f,input),id=>Read(f,id));Assert.NotNull(state.VarianceResult);Assert.NotNull(state.RecordedVariance);Assert.False(state.HasUnconfirmedVarianceIntent);Assert.Null(state.FinancialReview);
        state.VarianceResult!.Invoice.TOTAL_AMOUNT=99m;Assert.Equal(2.01m,state.VarianceResult!.Invoice.TOTAL_AMOUNT);var intent=state.RecordedVariance!.Command.IntentId;
        await state.LoadDetailAsync("invoice",id=>Read(f,id));Assert.Null(state.VarianceResult);Assert.Null(state.VarianceIntent);Assert.Equal(intent,state.RecordedVariance!.Command.IntentId);Assert.Equal(1,f.Existing.ReadOwner.Native.Native.Writes);
    }
    [Fact] public async Task UnknownCommitKeepsFrozenOriginalIntentAndRetryAddsNoWrite()
    {
        var f=new InvoiceVarianceCommandTests.Fixture();await f.Prepare();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await Prepare(state,f);f.Existing.ReadOwner.Native.Native.FailCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordVarianceAsync(input=>Write(f,input),id=>Read(f,id)));Assert.True(state.HasUnconfirmedVarianceIntent);var original=state.VarianceIntent!;
        state.VarianceIntent!.Review.Financial.Invoice.Invoice.TOTAL_AMOUNT=99m;Assert.Equal(2.01m,state.VarianceIntent!.Review.Financial.Invoice.Invoice.TOTAL_AMOUNT);
        state.Clear();Assert.True(state.HasUnconfirmedVarianceIntent);Assert.Equal("/accounting/payables/invoices/invoice",state.VarianceRecoveryRoute);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewVarianceProposalAsync("REQUEST_CORRECTION",[],"new",(_,_)=>throw new Exception()));
        f.Existing.ReadOwner.Native.Native.FailCommit=false;await state.RecordVarianceAsync(input=>{Assert.True(input.Command.Same(original.Command));return Write(f,input);},id=>Read(f,id));Assert.False(state.HasUnconfirmedVarianceIntent);Assert.Equal(1,f.Existing.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("header")][InlineData("line")][InlineData("read")]
    public async Task CurrentSourceMustConfirmExactResultAndOriginalLines(string fault)
    {
        var f=new InvoiceVarianceCommandTests.Fixture();await f.Prepare();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await Prepare(state,f);
        async Task<APInvoiceSourceReview?> Changed(string id)
        {
            if(fault=="read")throw new InvalidOperationException("unavailable");var source=(await Read(f,id))!;var header=PurchaseReceiptDecisionProof.Copy(source.Invoice);var lines=source.Lines.Select(PurchaseReceiptDecisionProof.Copy).ToArray();
            if(fault=="header")header.TOTAL_AMOUNT=99m;if(fault=="line")lines[0].ITEM_DESCRIPTION="changed";return APInvoiceSourceReview.Create(source.Company,source.ActorUserId,header,lines);
        }
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordVarianceAsync(input=>Write(f,input),Changed));Assert.Null(state.VarianceResult);Assert.True(state.HasUnconfirmedVarianceIntent);Assert.Equal(1,f.Existing.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("company")][InlineData("source")][InlineData("dispose")]
    public async Task LateResultCannotPublishAfterContextOrSourceInvalidation(string fault)
    {
        var f=new InvoiceVarianceCommandTests.Fixture();await f.Prepare();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await Prepare(state,f);
        var pending=new TaskCompletionSource<APInvoiceVarianceResult>();APInvoiceVarianceSubmission? original=null;var recording=state.RecordVarianceAsync(input=>{original=input;return pending.Task;},id=>Read(f,id));Assert.True(state.VarianceCommandBusy);
        if(fault=="company")company.Clear();if(fault=="source")state.Clear();if(fault=="dispose")state.Dispose();pending.SetResult(await Write(f,original!));await recording;
        Assert.Null(state.VarianceResult);Assert.True(state.HasUnconfirmedVarianceIntent);Assert.False(state.VarianceCommandBusy);
    }
    [Fact] public async Task MissingRecordingGrantDoesNotInventAPendingIntent()
    {
        var f=new InvoiceVarianceCommandTests.Fixture();await f.Prepare();using var company=Selected(false);using var state=new APInvoiceWorkspaceState(company);await Prepare(state,f);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordVarianceAsync(input=>Write(f,input),id=>Read(f,id)));Assert.Null(state.VarianceIntent);Assert.Equal(0,f.Existing.ReadOwner.Native.Native.Writes);
    }
    [Fact] public async Task MalformedRecordedProofIsUnavailableOnReload()
    {
        var f=new InvoiceVarianceCommandTests.Fixture();await f.Prepare();f.Stored.VARIANCE_PROPOSAL_JSON="unverified";using var company=Selected();using var state=new APInvoiceWorkspaceState(company);
        await state.LoadDetailAsync("invoice",id=>Read(f,id));Assert.True(state.HasRecordedVarianceSource);Assert.Null(state.RecordedVariance);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task OriginalIntentDetailsAndRecoveryAreUnavailableToAnotherCompanyOrAccount(bool account)
    {
        var f=new InvoiceVarianceCommandTests.Fixture();await f.Prepare();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await Prepare(state,f);f.Existing.ReadOwner.Native.Native.FailCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordVarianceAsync(input=>Write(f,input),id=>Read(f,id)));var intent=state.VarianceIntent!.Command.IntentId;
        company.TrySelect(company.Clear(),account?"other":"actor",new(new("tenant",account?"company":"other"),["ConfiguredCompanyRole"],["Accounting.View","Accounting.PrepareInvoiceVariance","Accounting.RecordInvoiceVariance"]));
        Assert.True(state.HasUnconfirmedVarianceIntent);Assert.False(state.CanViewVarianceIntent);Assert.False(state.CanReconcileVariance);Assert.Null(state.VarianceRecoveryRoute);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordVarianceAsync(input=>Write(f,input),id=>Read(f,id)));Assert.Equal(intent,state.VarianceIntent!.Command.IntentId);Assert.Equal(1,f.Existing.ReadOwner.Native.Native.Writes);
    }
}
#endif
