using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class DrillingProgramDraftState
{
    private DrillingProgramReviewStart? reviewPrepared,reviewPending;private DrillingProgramReviewStartReceipt? reviewReceipt;
    public DrillingProgramReviewStart? ReviewPrepared=>reviewPrepared is null?null:CompanyInventorySourceReview.Copy(reviewPrepared);
    public DrillingProgramReviewStart? ReviewPending=>reviewPending is null?null:CompanyInventorySourceReview.Copy(reviewPending);
    public DrillingProgramReviewStartReceipt? ReviewReceipt=>reviewReceipt is null?null:CompanyInventorySourceReview.Copy(reviewReceipt);
    public bool CanPrepareReview=>CanReviewRouting && CanCreate && company.Selection!.Permissions.Contains("Workflow.Start",StringComparer.Ordinal) && routing is not null && routing.ExistingReviewInstanceId is null;
    public bool CanSubmitReview=>!disposed && !Busy && ready && CanCreate && company.Selection!.Permissions.Contains("Workflow.Start",StringComparer.Ordinal) && reviewPrepared is not null && reviewPending is null && pending is null;
    public bool CanRecoverReview=>!disposed && !Busy && ready && CanView && reviewPending is not null;
    public void PrepareReview(string processId)
    {
        if(!CanPrepareReview)return;Error=null;
        try{var option=routing!.Options.SingleOrDefault(option=>option.ProcessId==processId)??throw new InvalidOperationException("Choose an actual configured program workflow.");var value=new DrillingProgramReviewStart(CompanyInventorySourceReview.Copy(routing.Source),CompanyInventorySourceReview.Copy(option),Guid.NewGuid());value.Require();reviewPrepared=value;}
        catch(Exception error){Error=explain(error,"preparing original program review","The exact saved source and current independent recipients must be reviewed before submission");}Changed?.Invoke();
    }
    public void CancelReview(){if(Busy || reviewPending is not null)return;reviewPrepared=null;Changed?.Invoke();}
    public Task SubmitReviewAsync()=>RecordReview(false);public Task RecoverReviewAsync()=>RecordReview(true);
    private async Task RecordReview(bool recover)
    {
        if(disposed || (recover?!CanRecoverReview:!CanSubmitReview))return;var value=CompanyInventorySourceReview.Copy(recover?reviewPending!:reviewPrepared!);var selected=company.Snapshot;var epoch=generation;bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
        Busy=true;Error=null;Confirmation=null;reviewReceipt=null;if(!recover){reviewPending=value;reviewPrepared=null;}Changed?.Invoke();
        try
        {
            if(!recover){await journal.RetainProgramReviewAsync(value);if(!Active())return;}
            var result=recover?await client.RecoverProgramReviewAsync(value):await client.SubmitProgramReviewAsync(value);result.Require(value);if(!recover && !result.Recorded)throw new InvalidOperationException("The original program review was not confirmed recorded.");if(!Active())return;
            await journal.ClearProgramReviewAsync(value);if(!Active())return;reviewPending=null;reviewReceipt=CompanyInventorySourceReview.Copy(result);basis=null;drafts=null;routing=null;
            Confirmation=result.Recorded?"The original program review and first independent receiving assignment were confirmed recorded. The program remains DRAFT; no program approval or execution is recorded by submission.":"This original program review was confirmed absent. Reload the current program and routing before preparing another submission.";
        }
        catch(Exception error){var text=explain(error,recover?"recovering original program review":"submitting original program review","The program assignment outcome is unconfirmed. Its exact original recovery input was preserved");if(Active())Error=text;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
}
