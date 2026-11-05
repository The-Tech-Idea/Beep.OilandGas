using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class MatchingPolicyWorkspaceTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    private static CompanyClientContext Selected(string actor="actor")
    {var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(Company,["ConfiguredCompanyRole"],["Accounting.View","Accounting.PrepareMatchingPolicy","Accounting.ApproveMatchingPolicy"]));return company;}
    private static async Task<CompanyMatchingPolicyCommandReview> Review(CompanyMatchingPolicyCommandTests.Fixture native,string actor,string action)=>new(await native.Owner.GetCompanyMatchingPoliciesAsync(Company,actor),action,new string('B',64));
    private static Task<CompanyMatchingPolicyResult> Record(CompanyMatchingPolicyCommandTests.Fixture native,CompanyMatchingPolicySubmission submission)    {
        return Execute();
        async Task<CompanyMatchingPolicyResult> Execute(){var row=await native.Owner.RecordCompanyMatchingPolicyAsync(submission.Command,submission.Review.History,native.ReadOwner.Native.Authority);return new(submission.Command,row);}
    }
    private static Task Submit(APInvoiceWorkspaceState state,CompanyMatchingPolicyCommandTests.Fixture native,string actor="actor",Func<CompanyMatchingPolicySubmission,Task<CompanyMatchingPolicyResult>>? writer=null,Func<Task<CompanyMatchingPolicyReview>>? reader=null,string reason="Explicit reviewed policy reason")=>
        state.RecordMatchingPolicyAsync("new-policy",CompanyMatchingPolicyReview.Rules(native.ReadOwner.Row),native.ReadOwner.Row.EFFECTIVE_DATE,null,reason,
            writer ?? (submission=>Record(native,submission)),reader ?? (()=>native.Owner.GetCompanyMatchingPoliciesAsync(Company,actor)));
    [Fact] public async Task DraftAndIndependentApprovalUseFrozenReviewAndCompleteHistoryBeforeSuccess()
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);
        await state.LoadPolicyCommandReviewAsync("DRAFT",action=>Review(native,"actor",action));await Submit(state,native);
        Assert.False(state.HasUnconfirmedPolicyIntent);Assert.Equal("DRAFT",state.PolicyResult!.Policy.MATCHING_POLICY_STATUS);Assert.Equal(1,native.ReadOwner.Native.Native.Writes);
        state.PolicyResult!.Policy.DESCRIPTION="changed";Assert.NotEqual("changed",state.PolicyResult!.Policy.DESCRIPTION);
        using var reviewerCompany=Selected("controller");using var reviewer=new APInvoiceWorkspaceState(reviewerCompany);
        await reviewer.LoadPolicyCommandReviewAsync("APPROVE",action=>Review(native,"controller",action));Assert.Equal(new[]{"new-policy"},reviewer.ReviewablePolicyDraftIds);
        reviewer.PolicyCommandReview!.History.Records[0].MATCHING_POLICY_STATUS="RETIRED";Assert.Equal("DRAFT",reviewer.PolicyCommandReview!.History.Records[0].MATCHING_POLICY_STATUS);reviewer.SelectPolicyDraft("new-policy");
        await Submit(reviewer,native,"controller");Assert.Equal("APPROVED",reviewer.PolicyResult!.Policy.MATCHING_POLICY_STATUS);Assert.Equal("controller",reviewer.PolicyResult.Policy.ROW_CHANGED_BY);Assert.Equal(2,native.ReadOwner.Native.Native.Writes);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Submit(reviewer,native,"controller"));Assert.Equal(2,native.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("lost-reply")][InlineData("history")][InlineData("missing")][InlineData("changed")]
    public async Task UnknownOutcomeRetainsOriginalIntentAndExactRetryRefreshesHistory(string fault)
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);
        await state.LoadPolicyCommandReviewAsync("DRAFT",action=>Review(native,"actor",action));
        async Task<CompanyMatchingPolicyResult> First(CompanyMatchingPolicySubmission submission){var result=await Record(native,submission);if(fault=="lost-reply")throw new InvalidOperationException("Unknown reply");return result;}
        async Task<CompanyMatchingPolicyReview> Read()
        {
            if(fault=="history")throw new InvalidOperationException("Unavailable history");var history=await native.Owner.GetCompanyMatchingPoliciesAsync(Company,"actor");
            if(fault=="missing")return CompanyMatchingPolicyReview.Create(Company,"actor",[]);if(fault=="changed")history.Records[0].DESCRIPTION="changed";return history;
        }
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Submit(state,native,writer:First,reader:Read));var original=JsonSerializer.Serialize(state.PolicyIntent);
        Assert.True(state.HasUnconfirmedPolicyIntent);Assert.Null(state.PolicyResult);Assert.Equal(1,native.ReadOwner.Native.Native.Writes);
        state.Clear();state.PolicyIntent!.Command.Rules.Require();await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadPolicyCommandReviewAsync("DRAFT",action=>Review(native,"actor",action)));
        await Submit(state,native,reason:"Changed caller reason must not replace retained intent");Assert.Equal(original,JsonSerializer.Serialize(state.PolicyIntent));Assert.False(state.HasUnconfirmedPolicyIntent);Assert.Equal(1,native.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("company")][InlineData("clear")][InlineData("dispose")]
    public async Task LateCompletedWriteCannotPublishSuccessAfterContextChange(string fault)
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);
        await state.LoadPolicyCommandReviewAsync("DRAFT",action=>Review(native,"actor",action));var pending=new TaskCompletionSource<CompanyMatchingPolicyResult>();CompanyMatchingPolicySubmission? submitted=null;
        var write=Submit(state,native,writer:submission=>{submitted=submission;return pending.Task;});Assert.True(state.PolicyCommandBusy);
        if(fault=="company")company.Clear();if(fault=="clear")state.Clear();if(fault=="dispose")state.Dispose();
        pending.SetResult(await Record(native,submitted!));await write;Assert.True(state.HasUnconfirmedPolicyIntent);Assert.Null(state.PolicyResult);Assert.False(state.PolicyCommandBusy);
        if(fault=="company"){Assert.False(state.CanReconcilePolicy);company.TrySelect(company.Clear(),"actor",new(Company,["ConfiguredCompanyRole"],["Accounting.View"]));}
        if(fault!="dispose"){await Submit(state,native);Assert.False(state.HasUnconfirmedPolicyIntent);Assert.Equal(1,native.ReadOwner.Native.Native.Writes);}
    }
    [Fact] public async Task BusyCommandCannotBeReplacedAndRetainedIntentIsNotMutableThroughItsProjection()
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();using var company=Selected();using var state=new APInvoiceWorkspaceState(company);
        await state.LoadPolicyCommandReviewAsync("DRAFT",action=>Review(native,"actor",action));var pending=new TaskCompletionSource<CompanyMatchingPolicyResult>();CompanyMatchingPolicySubmission? original=null;
        var write=Submit(state,native,writer:submission=>{original=submission;return pending.Task;});
        Assert.NotSame(original!.Review.History,state.PolicyIntent!.Review.History);Assert.Equal(JsonSerializer.Serialize(original),JsonSerializer.Serialize(state.PolicyIntent));await Assert.ThrowsAsync<InvalidOperationException>(()=>Submit(state,native));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadPolicyCommandReviewAsync("APPROVE",action=>Review(native,"actor",action)));
        pending.SetResult(await Record(native,original!));await write;Assert.False(state.HasUnconfirmedPolicyIntent);Assert.Equal(1,native.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("self")][InlineData("legacy")]
    public async Task UnverifiedOrSelfPreparedDraftsAreUnavailableForApprovalSelection(string mode)
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();if(mode=="self"){await native.PrepareDraft("actor");await native.Record();}else native.ReadOwner.Native.Native.Rows.Add(native.ReadOwner.Row);
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await state.LoadPolicyCommandReviewAsync("APPROVE",action=>Review(native,"actor",action));
        Assert.Empty(state.ReviewablePolicyDraftIds);Assert.Throws<InvalidOperationException>(()=>state.SelectPolicyDraft(mode=="self"?"new-policy":"policy"));
    }
}
#endif
