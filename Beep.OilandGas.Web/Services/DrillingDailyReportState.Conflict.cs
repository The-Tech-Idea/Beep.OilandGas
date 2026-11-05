using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;

namespace Beep.OilandGas.Web.Services;

public sealed partial class DrillingDailyReportState
{
    private const string ConflictCleanupExplanation = "The last validated report review confirmed this original absent. Clearing the tab input is not confirmed. Keep this page open and use Keep recorded report and clear my unrecorded original again to recheck and finish.";
    private DrillingDailyReportConflictReview? conflict;
    public DrillingDailyReportConflictReview? Conflict => Copy(conflict);
    public bool ConflictAcknowledged { get; private set; }
    public bool CanInspectConflict => CanRecover && receipt?.Recorded is null;
    public bool CanAcknowledgeConflict => CanRecover && conflict is not null;
    public async Task InspectConflictAsync()
    {
        if (!CanInspectConflict) return;
        var original = Copy(pending)!; var selected = company.Snapshot; var epoch = generation;
        bool Current() => Active(selected, epoch);
        reading = true; Error = null; conflict = null; ConflictAcknowledged = false; Changed?.Invoke();
        try
        {
            var result = await client.GetDailyReportConflictAsync(original); result.Require(original);
            if (Current()) conflict = Copy(result);
        }
        catch (Exception error)
        {
            var text = explain(error, "inspecting competing observed report", "The original absence and different recorded observations are unconfirmed. Preserve the original");
            if (Current()) Error = text;
        }
        finally { if (Current()) { reading = false; Changed?.Invoke(); } }
    }
    public async Task AcknowledgeConflictAsync()
    {
        if (!CanAcknowledgeConflict) return;
        var original = Copy(pending)!; var reviewed = Copy(conflict)!; var selected = company.Snapshot; var epoch = generation;
        bool Current() => Active(selected, epoch);
        writing = true; Error = null; Confirmation = null;
        var fallback = "The reviewed report history is unconfirmed. The original input was preserved"; Changed?.Invoke();
        try
        {
            var result = await client.ConfirmDailyReportConflictAsync(new(original, reviewed.Revision)); result.Require(original);
            if (result.Revision != reviewed.Revision) throw new InvalidOperationException("The reviewed report history changed. Inspect again before clearing the original.");
            if (!Current()) return;
            conflict = Copy(result); ConflictAcknowledged = true; receipt = null; basis = null; page = null;
            originalFeedback.ConfirmReceiptAndBeginClear(false); fallback = ConflictCleanupExplanation; Changed?.Invoke();
            if (!Current()) return;
            await journal.ClearDailyReportAsync(original); if (!Current()) return;
            pending = null; originalFeedback.Reset();
            Confirmation = "You kept the exact different recorded report and cleared this confirmed-unrecorded original from this tab. No observation, report, execution or responsibility was changed or completed. Reload the recorded history before preparing another report.";
        }
        catch (Exception error)
        {
            var text = explain(error, "acknowledging competing observed report", fallback);
            if (Current()) { originalFeedback.Failed(); Error = text; }
        }
        finally { writing = false; if (!disposed) Changed?.Invoke(); }
    }
}
