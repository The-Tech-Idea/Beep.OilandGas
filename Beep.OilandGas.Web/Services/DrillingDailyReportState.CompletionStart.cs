using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;

namespace Beep.OilandGas.Web.Services;

public sealed partial class DrillingDailyReportState
{
    private readonly RetainedOriginalFeedback completionFeedback=new();
    private DrillingCompletionReviewStart? completionPrepared,completionPending;
    private DrillingCompletionReviewStartReceipt? completionReceipt;
    private bool HasCompletionReviewFlow=>completionPrepared is not null || completionPending is not null || completionReceipt is not null;
    public DrillingCompletionReviewStart? CompletionPrepared=>Copy(completionPrepared);
    public DrillingCompletionReviewStart? CompletionPending=>completionPending?.Source.Execution.Basis.ReviewInstanceId==reviewId?Copy(completionPending):null;
    public string? RetainedCompletionReviewId=>completionPending?.Source.Execution.Basis.ReviewInstanceId;
    public DrillingCompletionReviewStartReceipt? CompletionReceipt=>Copy(completionReceipt);
    public string CompletionPendingTitle=>completionFeedback.Title("completion review");
    public string CompletionPendingExplanation=>completionFeedback.Explanation;
    public bool CompletionTabCopyConfirmed=>completionPending is not null && completionFeedback.TabCopyConfirmed;
    public bool CanPrepareCompletionReview=>CanInspectCompletionRouting && completionRouting is not null && completionRouting.ExistingReviewInstanceId is null && completionRouting.SenderRoles.Count>0;
    private bool CompletionProfileMatches(DrillingCompletionReviewStart original)=>HasWrite && company.Selection!.Permissions.Contains("Workflow.Start",StringComparer.Ordinal) &&
        company.Selection.Roles.Contains(original.SenderRole,StringComparer.Ordinal) && personas.CurrentProfile?.UserId==company.Selection.UserId &&
        personas.CurrentProfile.PersonaCode==original.SenderPersonaCode && personas.CurrentPersona?.Code==original.SenderPersonaCode;
    public bool CanSubmitCompletionReview=>!Busy && ready && completionPrepared is not null && completionPending is null && CompletionProfileMatches(completionPrepared);
    public bool CanRecoverCompletionReview=>!Busy && ready && CanView && CompletionPending is not null && completionPending!.Scope().Company==company.Selection!.Scope && completionPending.Scope().ActorUserId==company.Selection.UserId;
    private void ClearCompletionFlow(){completionPrepared=null;completionPending=null;completionReceipt=null;completionFeedback.Reset();}
    public void PrepareCompletionReview(string workflow,string role,string? reason)
    {
        if(!CanPrepareCompletionReview)return;Error=null;
        try
        {
            var routing=completionRouting!;routing.Require(company.Selection!.Scope,company.Selection.UserId,reviewId!);
            if(!routing.SenderRoles.Contains(role,StringComparer.Ordinal))throw new InvalidOperationException("Choose a current configured sender role.");
            var option=routing.Options.SingleOrDefault(value=>value.ProcessId==workflow)??throw new InvalidOperationException("Choose an exact configured completion-review workflow.");
            var original=new DrillingCompletionReviewStart(Copy(routing.Source)!,Copy(option)!,role,routing.SenderPersonaCode!,routing.SenderAuthorityRevision!,Guid.NewGuid(),reason?.Trim()??"");
            original.Require();if(!CompletionProfileMatches(original))throw new InvalidOperationException("The current completion sender workspace changed.");
            completionPrepared=Copy(original);
        }
        catch(Exception error){Error=explain(error,"preparing completion review","Confirm the current claim, configured independent recipients, sender role/workspace and explicit review reason");}
        Changed?.Invoke();
    }
    public void CancelCompletionReview(){if(Busy || completionPending is not null)return;completionPrepared=null;Changed?.Invoke();}
    public Task SubmitCompletionReviewAsync()=>RecordCompletionReview(false);
    public Task RecoverCompletionReviewAsync()=>RecordCompletionReview(true);
    private async Task RecordCompletionReview(bool recover)
    {
        if(disposed || (recover?!CanRecoverCompletionReview:!CanSubmitCompletionReview))return;
        var original=Copy(recover?completionPending:completionPrepared)!;var selected=company.Snapshot;var epoch=generation;
        bool Current()=>Active(selected,epoch);
        writing=true;Error=null;Confirmation=null;
        if(!recover){completionPending=original;completionPrepared=null;completionReceipt=null;page=null;basis=null;ClearCompletionRouting();completionFeedback.BeginSaving();}
        var fallback=completionFeedback.FailureExplanation;Changed?.Invoke();
        try
        {
            if(!recover)
            {
                await journal.RetainCompletionReviewAsync(original);if(!Current())return;
                completionFeedback.ConfirmSaved();completionFeedback.AttemptSubmission();fallback=completionFeedback.FailureExplanation;Changed?.Invoke();if(!Current())return;
            }
            var result=recover?await client.RecoverCompletionReviewAsync(original):await client.SubmitCompletionReviewAsync(original);result.Require(original);
            if(!recover && !result.Recorded)throw new InvalidOperationException("The original completion review was not confirmed recorded.");if(!Current())return;
            completionReceipt=Copy(result);completionFeedback.ConfirmReceiptAndBeginClear(result.Recorded);fallback=completionFeedback.FailureExplanation;Changed?.Invoke();if(!Current())return;
            await journal.ClearCompletionReviewAsync(original);if(!Current())return;
            completionPending=null;completionFeedback.Reset();page=null;basis=null;ClearCompletionRouting();
            Confirmation=result.Recorded?"The exact completion review and first independent approval tasks were confirmed recorded. Completion/tests remain unaccepted and reporting responsibility remains ongoing.":
                "This exact completion-review intent was confirmed absent. Reload the current claim and routing before preparing another.";
        }
        catch(Exception error){var text=explain(error,recover?"recovering completion review":"submitting completion review",fallback);if(Current()){completionFeedback.Failed();Error=text;}}
        finally{writing=false;if(!disposed)Changed?.Invoke();}
    }
}
