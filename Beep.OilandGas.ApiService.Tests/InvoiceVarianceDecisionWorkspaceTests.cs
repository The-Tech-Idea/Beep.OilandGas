#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InvoiceVarianceDecisionWorkspaceTests
{
    internal static CompanyClientContext Selected(bool grant=true)
    {var company=new CompanyClientContext();company.TrySelect(company.Clear(),"reviewer",new(new("tenant","company"),["ConfiguredControllerRole"],grant?["Accounting.View","Procurement.View","Accounting.ReviewInvoiceVariance","Accounting.RecordInvoiceVarianceDecision"]:["Accounting.View","Procurement.View","Accounting.ReviewInvoiceVariance"]));return company;}
    internal static Task<APInvoiceSourceReview?> Read(InvoiceVarianceDecisionCommandTests.Fixture f,string id)=>f.Existing.Existing.ReadOwner.Native.AP.GetSourceReviewAsync(f.Command.Company,id,"reviewer");
    internal static async Task Prepare(APInvoiceWorkspaceState state,InvoiceVarianceDecisionCommandTests.Fixture f)
    {await state.LoadDetailAsync("invoice",id=>Read(f,id));await state.LoadVarianceDecisionReviewAsync((_,_)=>Task.FromResult(f.Review));}
    internal static async Task<APInvoiceVarianceDecisionResult> Write(InvoiceVarianceDecisionCommandTests.Fixture f,APInvoiceVarianceDecisionSubmission input)=>new(input.Command,
        await f.Existing.Existing.ReadOwner.Native.AP.RecordVarianceDecisionAsync(input.Command,input.Review,f.Existing.Existing.ReadOwner.Native.Authority,()=>InvoiceVarianceDecisionReviewTests.Financial(f.Existing)));
    [Fact] public async Task ConfirmedResultRequiresFreshSourceAndReloadShowsProtectedHistory()
    {
        var f=new InvoiceVarianceDecisionCommandTests.Fixture();await f.Prepare();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await Prepare(state,f);
        await state.RecordVarianceDecisionAsync(f.Command.Disposition,f.Command.Reason,input=>Write(f,input),id=>Read(f,id));Assert.False(state.HasUnconfirmedVarianceDecisionIntent);Assert.NotNull(state.VarianceDecisionResult);Assert.NotNull(state.RecordedVarianceDecision);Assert.Null(state.VarianceDecisionReview);
        state.VarianceDecisionResult!.Invoice.TOTAL_AMOUNT=99m;Assert.Equal(2.01m,state.VarianceDecisionResult!.Invoice.TOTAL_AMOUNT);var intent=state.RecordedVarianceDecision!.Command.IntentId;
        await state.LoadDetailAsync("invoice",id=>Read(f,id));Assert.Null(state.VarianceDecisionResult);Assert.Null(state.VarianceDecisionIntent);Assert.Equal(intent,state.RecordedVarianceDecision!.Command.IntentId);Assert.Equal(2,f.Existing.Existing.ReadOwner.Native.Native.Writes);
    }
    [Fact] public async Task UnknownOutcomeKeepsOriginalInputAcrossClearAndRetriesWithoutAdditionalWrite()
    {
        var f=new InvoiceVarianceDecisionCommandTests.Fixture();await f.Prepare();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await Prepare(state,f);var n=f.Existing.Existing.ReadOwner.Native.Native;n.FailCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordVarianceDecisionAsync(f.Command.Disposition,f.Command.Reason,input=>Write(f,input),id=>Read(f,id)));Assert.True(state.HasUnconfirmedVarianceDecisionIntent);var original=state.VarianceDecisionIntent!;
        state.VarianceDecisionIntent!.Review.Current.Invoice.Invoice.TOTAL_AMOUNT=99m;Assert.Equal(2.01m,state.VarianceDecisionIntent!.Review.Current.Invoice.Invoice.TOTAL_AMOUNT);
        state.Clear();Assert.True(state.HasUnconfirmedVarianceDecisionIntent);Assert.Equal("/accounting/payables/invoices/invoice",state.VarianceDecisionRecoveryRoute);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadVarianceDecisionReviewAsync((_,_)=>throw new Exception()));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadPolicyCommandReviewAsync("DRAFT",_=>throw new Exception()));
        n.FailCommit=false;await state.RecordVarianceDecisionAsync("REQUEST_CORRECTION","replacement must be ignored",input=>{Assert.True(input.Command.Same(original.Command));return Write(f,input);},id=>Read(f,id));Assert.False(state.HasUnconfirmedVarianceDecisionIntent);Assert.Equal(2,n.Writes);
    }
    [Theory][InlineData("header")][InlineData("line")][InlineData("read")]
    public async Task CurrentSourceMustConfirmExactRowAndReviewedLines(string fault)
    {
        var f=new InvoiceVarianceDecisionCommandTests.Fixture();await f.Prepare();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await Prepare(state,f);
        async Task<APInvoiceSourceReview?> Changed(string id)
        {if(fault=="read")throw new InvalidOperationException("unavailable");var source=(await Read(f,id))!;var header=PurchaseReceiptDecisionProof.Copy(source.Invoice);var lines=source.Lines.Select(PurchaseReceiptDecisionProof.Copy).ToArray();if(fault=="header")header.TOTAL_AMOUNT=99m;if(fault=="line")lines[0].ITEM_DESCRIPTION="changed";return APInvoiceSourceReview.Create(source.Company,source.ActorUserId,header,lines);}
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordVarianceDecisionAsync(f.Command.Disposition,f.Command.Reason,input=>Write(f,input),Changed));Assert.True(state.HasUnconfirmedVarianceDecisionIntent);Assert.Null(state.VarianceDecisionResult);
    }
    [Theory][InlineData("company")][InlineData("source")][InlineData("dispose")]
    public async Task LateCompletionCannotPublishAfterInvalidation(string fault)
    {
        var f=new InvoiceVarianceDecisionCommandTests.Fixture();await f.Prepare();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await Prepare(state,f);
        var pending=new TaskCompletionSource<APInvoiceVarianceDecisionResult>();APInvoiceVarianceDecisionSubmission? original=null;var recording=state.RecordVarianceDecisionAsync(f.Command.Disposition,f.Command.Reason,input=>{original=input;return pending.Task;},id=>Read(f,id));Assert.True(state.VarianceDecisionCommandBusy);
        if(fault=="company")company.Clear();if(fault=="source")state.Clear();if(fault=="dispose")state.Dispose();pending.SetResult(await Write(f,original!));await recording;Assert.Null(state.VarianceDecisionResult);Assert.True(state.HasUnconfirmedVarianceDecisionIntent);Assert.False(state.VarianceDecisionCommandBusy);
    }
    [Fact] public async Task MissingRecordingGrantCannotInventPendingIntent()
    {var f=new InvoiceVarianceDecisionCommandTests.Fixture();await f.Prepare();using var company=Selected(false);using var state=new APInvoiceWorkspaceState(company);await Prepare(state,f);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordVarianceDecisionAsync(f.Command.Disposition,f.Command.Reason,input=>Write(f,input),id=>Read(f,id)));Assert.Null(state.VarianceDecisionIntent);}
    [Theory][InlineData("disposition")][InlineData("reason")][InlineData("bound")]
    public async Task InvalidScreenChoiceOrReasonCannotCreateAnIntent(string fault)
    {
        var f=new InvoiceVarianceDecisionCommandTests.Fixture();await f.Prepare();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await Prepare(state,f);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordVarianceDecisionAsync(fault=="disposition"?"APPROVED":f.Command.Disposition,fault=="reason"?" ":fault=="bound"?new string('x',1001):f.Command.Reason,_=>throw new Exception("must not write"),id=>Read(f,id)));Assert.Null(state.VarianceDecisionIntent);Assert.Equal(1,f.Existing.Existing.ReadOwner.Native.Native.Writes);
    }
    [Fact] public async Task BusySubmissionCannotBeReplacedOrInvalidateItsReviewGeneration()
    {
        var f=new InvoiceVarianceDecisionCommandTests.Fixture();await f.Prepare();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await Prepare(state,f);
        var pending=new TaskCompletionSource<APInvoiceVarianceDecisionResult>();APInvoiceVarianceDecisionSubmission? original=null;
        var recording=state.RecordVarianceDecisionAsync(f.Command.Disposition,f.Command.Reason,input=>{original=input;return pending.Task;},id=>Read(f,id));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordVarianceDecisionAsync("REQUEST_CORRECTION","replacement",_=>throw new Exception(),id=>Read(f,id)));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadVarianceDecisionReviewAsync((_,_)=>throw new Exception()));
        pending.SetResult(await Write(f,original!));await recording;Assert.NotNull(state.VarianceDecisionResult);Assert.False(state.HasUnconfirmedVarianceDecisionIntent);Assert.Equal(original!.Command.IntentId,state.RecordedVarianceDecision!.Command.IntentId);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task PendingIntentDetailsAndRecoveryArePrivateToOriginalCompanyAndAccount(bool account)
    {
        var f=new InvoiceVarianceDecisionCommandTests.Fixture();await f.Prepare();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await Prepare(state,f);f.Existing.Existing.ReadOwner.Native.Native.FailCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordVarianceDecisionAsync(f.Command.Disposition,f.Command.Reason,input=>Write(f,input),id=>Read(f,id)));var intent=state.VarianceDecisionIntent!.Command.IntentId;
        company.TrySelect(company.Clear(),account?"other":"reviewer",new(new("tenant",account?"company":"other"),["ConfiguredControllerRole"],["Accounting.View","Accounting.ReviewInvoiceVariance","Accounting.RecordInvoiceVarianceDecision"]));
        Assert.True(state.HasUnconfirmedVarianceDecisionIntent);Assert.False(state.CanViewVarianceDecisionIntent);Assert.False(state.CanReconcileVarianceDecision);Assert.Null(state.VarianceDecisionRecoveryRoute);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordVarianceDecisionAsync(f.Command.Disposition,f.Command.Reason,input=>Write(f,input),id=>Read(f,id)));Assert.Equal(intent,state.VarianceDecisionIntent!.Command.IntentId);
    }
}
#endif
