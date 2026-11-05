using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class DrillingCompletionAssignedState
{
    private readonly Dictionary<int,DrillingCompletionAcceptedTest> acceptedTests=[];
    public IReadOnlyList<DrillingCompletionAcceptedTest> AcceptedTests=>current is null?[]:acceptedTests.Values.OrderBy(test=>test.ReportedTestIndex).Select(CompanyInventorySourceReview.Copy).ToArray();
    public bool AcceptanceEligible=>current is {} review && review.IsCurrentClaim && review.StageNumber==review.Original.Workflow.Stages.Count &&
        review.Original.Source.ReportedCompletion!.Report.OutstandingItems.Count==0 && review.Original.Source.ReportedCompletion.Report.Tests.All(test=>test.ReportedResult=="REPORTED_PASS");
    public bool CanSignTest=>CanPrepareDecision && AcceptanceEligible && testInspection is not null;
    public bool CanPrepareAcceptance=>CanPrepareDecision && AcceptanceEligible && acceptedTests.Count==current!.Original.Source.ReportedCompletion!.Report.Tests.Count;
    private void ResetAcceptanceDraft()=>acceptedTests.Clear();
    public void SignTest(string criteria,string utcCertificate,string dateResolution,string reason,bool passing,bool reportedUtc)
    {
        if(!CanSignTest)return;Error=null;
        try
        {
            var evidence=testInspection!;var test=new DrillingCompletionAcceptedTest(evidence.Selection.ReportedTestIndex,CompanyInventorySourceReview.Copy(evidence.Native),evidence.Revision,evidence.CheckedAtUtc,
                criteria.Trim(),utcCertificate.Trim(),dateResolution.Trim(),reason.Trim(),passing,reportedUtc);test.Require(current!);acceptedTests[test.ReportedTestIndex]=test;
        }
        catch(Exception error){Error=explain(error,"checking independent physical test acceptance","Verify the exact certificate, criteria, reported UTC and any date discrepancy before including this test");}
        Changed?.Invoke();
    }
    public void PrepareAcceptance(string actualUtc,string? reason,bool confirmed)
    {
        if(!CanPrepareAcceptance)return;Error=null;
        try
        {
            if(!ExplicitUtcInput.TryParse(actualUtc,out var utc))throw new InvalidOperationException("Enter explicit actual acceptance UTC ending in Z or +00:00.");
            var review=CompanyInventorySourceReview.Copy(current!);var acceptance=new DrillingCompletionAcceptance(review.Revision,new DateTimeOffset(utc),AcceptedTests,confirmed);
            var original=new DrillingCompletionReviewDecision(review,Guid.NewGuid(),"APPROVE",reason?.Trim()??""){Acceptance=acceptance};original.Require();preparedDecision=original;ResetTestInspection();ResetAcceptanceDraft();
        }
        catch(Exception error){Error=explain(error,"preparing independent physical completion acceptance","Confirm every checked test, clear outstanding work and enter actual UTC with a completion rationale");}
        Changed?.Invoke();
    }
}
