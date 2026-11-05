using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;

namespace Beep.OilandGas.Web.Services;

public sealed partial class DrillingDailyReportState
{
    private DrillingCompletionReviewReadiness? completionRouting;
    public DrillingCompletionReviewReadiness? CompletionRouting => Copy(completionRouting);
    public string? CompletionRoutingError { get; private set; }
    public bool CanInspectCompletionRouting => CanNavigate && HasWrite && page!.History.ReportedCompletion is not null &&
        company.Selection!.Permissions.Contains("Workflow.Start", StringComparer.Ordinal);
    private void ClearCompletionRouting() { completionRouting = null; CompletionRoutingError = null; }

    public async Task InspectCompletionRoutingAsync()
    {
        if (!CanInspectCompletionRouting) return;
        var selected = company.Snapshot; var epoch = generation; var sequence = ++request;
        var sourceRevision = page!.History.Revision; ClearCompletionRouting(); reading = true; Changed?.Invoke();
        bool Current() => Active(selected, epoch) && sequence == request;
        try
        {
            var found = await client.GetCompletionRoutingAsync(reviewId!); if (!Current()) return;
            found.Require(selected.Selection!.Scope, selected.Selection.UserId, reviewId!);
            if (found.Source.Revision != sourceRevision || page?.History.Revision != sourceRevision)
                throw new InvalidOperationException("The current completion or complete report history changed. Reload before inspecting its independent routing.");
            if (found.SenderRoles.Count > 0 && (personas.CurrentProfile?.UserId != selected.Selection.UserId ||
                personas.CurrentProfile.PersonaCode != found.SenderPersonaCode || personas.CurrentPersona?.Code != found.SenderPersonaCode ||
                !found.SenderRoles.All(role => selected.Selection.Roles.Contains(role, StringComparer.Ordinal))))
                throw new InvalidOperationException("Confirm the current configured sender role and workspace before inspecting private completion routing.");
            completionRouting = Copy(found);
        }
        catch (Exception error)
        {
            var text = explain(error, "inspecting independent completion routing", "The current completion source, configured approval stages and independent recipients could not be confirmed. Reload before trying again");
            if (Current()) { completionRouting = null; CompletionRoutingError = text; }
        }
        finally { if (Current()) { reading = false; Changed?.Invoke(); } }
    }
}
