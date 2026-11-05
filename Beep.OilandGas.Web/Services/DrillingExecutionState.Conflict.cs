using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

public sealed partial class DrillingExecutionState
{
    private const string ConflictCleanupExplanation="The last validated conflict review confirmed this original was absent. Clearing the tab input is not confirmed. Keep this page open and use Keep recorded execution and clear my unrecorded original again to recheck the conflict and finish.";
    private DrillingExecutionConflictReview? conflict;
    public DrillingExecutionConflictReview? Conflict => Copy(conflict);
    public bool ConflictAcknowledged { get; private set; }
    public bool CanInspectConflict => CanRecover && receipt?.Recorded is null;
    public bool CanAcknowledgeConflict => CanRecover && conflict is not null;

    public async Task InspectConflictAsync()
    {
        if(!CanInspectConflict)return;
        var original=Copy(pending)!;var selected=company.Snapshot;var epoch=generation;bool Current()=>Active(selected,epoch);
        reading=true;Error=null;conflict=null;ConflictAcknowledged=false;Changed?.Invoke();
        try
        {
            var result=await client.GetExecutionConflictAsync(original);result.Require(original);
            if(Current())conflict=Copy(result);
        }
        catch(Exception error)
        {
            var text=explain(error,"inspecting competing execution","The absent original and exact different execution could not be confirmed. Preserve and recover the original");
            if(Current())Error=text;
        }
        finally{if(Current()){reading=false;Changed?.Invoke();}}
    }

    public async Task AcknowledgeConflictAsync()
    {
        if(!CanAcknowledgeConflict)return;
        var original=Copy(pending)!;var reviewed=Copy(conflict)!;var selected=company.Snapshot;var epoch=generation;bool Current()=>Active(selected,epoch);
        writing=true;Error=null;Confirmation=null;var fallback="The reviewed competing execution is unconfirmed. The original input was preserved";Changed?.Invoke();
        try
        {
            var result=await client.ConfirmExecutionConflictAsync(new(original,reviewed));result.Require(original);
            if(result.Revision!=reviewed.Revision)throw new InvalidOperationException("The reviewed competing execution changed. Inspect it again.");
            if(!Current())return;
            conflict=Copy(result);ConflictAcknowledged=true;receipt=null;basis=null;status=null;
            originalFeedback.ConfirmReceiptAndBeginClear(false);fallback=ConflictCleanupExplanation;Changed?.Invoke();
            if(!Current())return;await journal.ClearExecutionAsync(original);if(!Current())return;
            pending=null;originalFeedback.Reset();
            Confirmation="You kept the exact different recorded execution and cleared this confirmed-unrecorded original from this tab. No execution, WELL, report or financial record was changed. Reload its recorded outcome.";
        }
        catch(Exception error)
        {
            var text=explain(error,"acknowledging competing execution",fallback);
            if(Current()){originalFeedback.Failed();Error=text;}
        }
        finally{writing=false;if(!disposed)Changed?.Invoke();}
    }
}
