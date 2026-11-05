using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.HSE.Models;

public sealed record CompanyHseAssignedAction(string IncidentId, int StepSequence, string Description, DateTime DueAtUtc);

/// <summary>Current recorded responsibility, not a FIELD-wide action count or a workflow assignment.</summary>
public sealed record CompanyHseAssignedActions(FieldSelectionReview Context, DateTime CheckedAtUtc,
    int ReturnedIncidentCount, IReadOnlyList<CompanyHseAssignedAction> Actions)
{
    public void Require()
    {
        FieldSelectionReviewValidator.RequireReview(Context, Context.Company, Context.ActorUserId);
        if (CheckedAtUtc.Kind != DateTimeKind.Utc || CheckedAtUtc == default || ReturnedIncidentCount is < 0 or > 64 ||
            Actions is null || Actions.Count > 4096 || Actions.Any(row => row is null ||
                !LeaseSummaryValidator.Identifier(row.IncidentId) || row.StepSequence <= 0 ||
                string.IsNullOrWhiteSpace(row.Description) || row.Description.Length > 2048 ||
                row.DueAtUtc.Kind != DateTimeKind.Utc || row.DueAtUtc == default || row.DueAtUtc >= CheckedAtUtc) ||
            Actions.Select(row => (row.IncidentId, row.StepSequence)).Distinct().Count() != Actions.Count ||
            Actions.Select(row => row.IncidentId).Distinct(StringComparer.Ordinal).Count() > ReturnedIncidentCount)
            throw new InvalidOperationException("The current assigned overdue-action population cannot be confirmed.");
    }
}
