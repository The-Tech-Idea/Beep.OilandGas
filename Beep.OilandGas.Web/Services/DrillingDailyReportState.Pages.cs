using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;

namespace Beep.OilandGas.Web.Services;

public sealed partial class DrillingDailyReportState
{
    public bool CanNavigate => !Busy && ready && CanView && !HasCompletionReviewFlow && pending is null && prepared is null && receipt is null && page is not null;
    public Task NextPageAsync() => CanNavigate && page!.HasNext ? NavigatePageAsync(page.AfterNumber + page.Reports.Count) : Task.CompletedTask;
    public Task PreviousPageAsync() => CanNavigate && page!.HasPrevious ? NavigatePageAsync(Math.Max(0, page.AfterNumber - page.PageSize)) : Task.CompletedTask;
    private async Task NavigatePageAsync(int after)
    {
        ClearCompletionRouting();
        if (!CanNavigate) return;
        var checkedPage = Copy(page)!; var selected = company.Snapshot; var epoch = generation; var sequence = ++request;
        bool Current() => sequence == request && Active(selected, epoch);
        reading = true; Error = null; basis = null; correctionTarget = null; completionMode = false; prepared = null; Confirmation = null; Changed?.Invoke();
        try
        {
            var found = await client.GetDailyReportPageAsync(reviewId!, after, checkedPage.PageSize, checkedPage.History.Revision);
            found.Require(); RequireExecution(found.History.Execution);
            if (found.AfterNumber != after || found.PageSize != checkedPage.PageSize || found.History.Revision != checkedPage.History.Revision)
                throw new InvalidOperationException("The checked report page changed. Reload its first page.");
            if (Current()) page = Copy(found);
        }
        catch (Exception error)
        {
            var text = explain(error, "navigating checked report history", "The checked report history or current access is unconfirmed. Reload original report history before navigating or preparing another report");
            if (Current()) { page = null; basis = null; Error = text; }
        }
        finally { if (Current()) { reading = false; Changed?.Invoke(); } }
    }
}
