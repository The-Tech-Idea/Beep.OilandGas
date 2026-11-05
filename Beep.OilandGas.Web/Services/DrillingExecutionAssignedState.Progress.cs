using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;

namespace Beep.OilandGas.Web.Services;

public sealed partial class DrillingExecutionAssignedState
{
    private DrillingDailyReportHistory? progress;
    private long progressRequest;
    public DrillingDailyReportHistory? Progress => progress is null ? null : CompanyInventorySourceReview.Copy(progress);
    public bool ProgressBusy { get; private set; }
    public string? ProgressError { get; private set; }
    private void ClearProgress() { ++progressRequest; progress = null; ProgressBusy = false; ProgressError = null; }
    public async Task LoadProgressAsync()
    {
        if (disposed || current is null || Busy || ProgressBusy) return;
        var assigned = Current!; var selected = company.Snapshot; var epoch = generation; var sequence = ++progressRequest;
        bool Active() => !disposed && selected == company.Snapshot && epoch == generation && sequence == progressRequest && current?.Revision == assigned.Revision;
        progress = null; ProgressError = null; ProgressBusy = true; Changed?.Invoke();
        try
        {
            var page = await client.GetDailyReportPageAsync(assigned.Source.Basis.ReviewInstanceId);
            if (!Active()) return;
            page.Require(); var history = page.History; var scope = history.Execution.Basis.Scope();
            if (scope.Company != selected.Selection!.Scope || scope.ActorUserId != selected.Selection.UserId)
                throw new InvalidOperationException("The observed progress belongs to another current company/account.");
            assigned.Responsibility.Root.RequireSource(history.Execution, false);
            assigned.Source.Basis.RequireSource(history.Execution.Basis, false);
            progress = CompanyInventorySourceReview.Copy(history);
        }
        catch (Exception error)
        {
            var text = explain(error, "loading assigned execution progress and reported completion", "The original observed history and reported completion could not be confirmed");
            if (Active()) { progress = null; ProgressError = text; }
        }
        finally { if (Active()) { ProgressBusy = false; Changed?.Invoke(); } }
    }
}
