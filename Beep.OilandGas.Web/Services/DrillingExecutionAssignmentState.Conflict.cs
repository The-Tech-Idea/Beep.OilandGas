using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
namespace Beep.OilandGas.Web.Services;
public sealed partial class DrillingExecutionAssignmentState
{
    private const string ConflictCleanupExplanation="The last validated conflict review confirmed this original assignment graph absent. Clearing the tab input is not confirmed. Keep this page open and use Keep recorded responsibility and clear my unrecorded original again to recheck and finish.";
    private DrillingExecutionAssignmentConflictReview? conflict;
    public DrillingExecutionAssignmentConflictReview? Conflict=>Copy(conflict);
    public bool ConflictAcknowledged{get;private set;}
    public bool CanInspectConflict=>CanRecover && receipt?.Recorded!=true;
    public bool CanAcknowledgeConflict=>CanRecover && conflict is not null;
    public async Task InspectConflictAsync()
    {
        if(!CanInspectConflict)return;
        var original=Copy(pending)!;var selected=company.Snapshot;var epoch=generation;bool Current()=>Active(selected,epoch);
        reading=true;Error=null;conflict=null;ConflictAcknowledged=false;Changed?.Invoke();
        try{var result=await client.GetExecutionAssignmentConflictAsync(original);result.Require(original);if(Current())conflict=Copy(result);}
        catch(Exception error){var text=explain(error,"inspecting competing reporting responsibility","The original absence and different recorded responsibility are unconfirmed. Preserve the original");if(Current())Error=text;}
        finally{if(Current()){reading=false;Changed?.Invoke();}}
    }
    public async Task AcknowledgeConflictAsync()
    {
        if(!CanAcknowledgeConflict)return;
        var original=Copy(pending)!;var reviewed=Copy(conflict)!;var selected=company.Snapshot;var epoch=generation;bool Current()=>Active(selected,epoch);
        writing=true;Error=null;Confirmation=null;var fallback="The reviewed responsibility is unconfirmed. The original input was preserved";Changed?.Invoke();
        try
        {
            var result=await client.ConfirmExecutionAssignmentConflictAsync(new(original,reviewed.Revision));result.Require(original);
            if(result.Revision!=reviewed.Revision)throw new InvalidOperationException("The reviewed responsibility changed. Inspect again before clearing the original.");
            if(!Current())return;
            conflict=Copy(result);ConflictAcknowledged=true;receipt=null;review=null;feedback.ConfirmReceiptAndBeginClear(false);fallback=ConflictCleanupExplanation;Changed?.Invoke();
            if(!Current())return;await journal.ClearExecutionAssignmentAsync(original);if(!Current())return;
            pending=null;feedback.Reset();Confirmation="You kept the exact different recorded responsibility and cleared this confirmed-unrecorded original from this tab. No responsibility, task, execution or report was changed or completed. Reload its recorded source.";
        }
        catch(Exception error){var text=explain(error,"acknowledging competing reporting responsibility",fallback);if(Current()){feedback.Failed();Error=text;}}
        finally{writing=false;if(!disposed)Changed?.Invoke();}
    }
}
