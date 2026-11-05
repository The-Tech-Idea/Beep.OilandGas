using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.HSE;
using System.Text.Json;
namespace Beep.OilandGas.Models.Data.HSE;

public sealed record IncidentResponseEvidence(long Observation, string Type, string? Result, string? ProjectId, string? Reason,
    string Active, DateTime? EffectiveAt, string? Source, string? ChangedBy, DateTime? ChangedAt);
public sealed record IncidentClosureResolutionReview(FieldSelectionReview Context, AssetDatabaseScope LifecycleDatabase,
    string IncidentId, string PlanId, Guid ApprovedIntentId, string ApprovedGraphRevision, string ApprovedNativeRevision,
    string Revision, IReadOnlyList<IncidentResponseEvidence> Responses)
{
    public void Require()
    {
        if (Context is null) throw new InvalidOperationException("A current native history review context is required.");
        FieldSelectionReviewValidator.RequireReview(Context, Context.Company, Context.ActorUserId);
        if (LifecycleDatabase is null || !LeaseSummaryValidator.Identifier(LifecycleDatabase.ConnectionName) || !LeaseSummaryValidator.Identifier(LifecycleDatabase.Fingerprint) ||
            !LeaseSummaryValidator.Identifier(IncidentId) || !LeaseSummaryValidator.Identifier(PlanId) || ApprovedIntentId == Guid.Empty ||
            !ProcessDefinitionRevision.IsValid(ApprovedGraphRevision) || !ProcessDefinitionRevision.IsValid(ApprovedNativeRevision) || !ProcessDefinitionRevision.IsValid(Revision) || Revision == ApprovedNativeRevision ||
            Responses is not { Count: > 0 and <= 256 } || Responses.Any(r => r is null || r.Observation <= 0 || string.IsNullOrWhiteSpace(r.Type) || r.Active is not ("Y" or "N")) ||
            Responses.Select(r => r.Observation).Distinct().Count() != Responses.Count || JsonSerializer.Serialize(this).Length > 16384)
            throw new InvalidOperationException("A bounded, changed native history review bound to its original approval is required.");
    }
    public void RequireAgainst(CompanyHseClosureCommand original, CompanyHseClosureReceipt receipt)
    {
        Require(); receipt.Require(original); var r = original.Review; var prior = r.Context;
        if (r.Stage != "INC_CLOSURE" || Context.Company != prior.Company || Context.ActorUserId != prior.ActorUserId || Context.Database != prior.Database || Context.FieldId != prior.FieldId ||
            LifecycleDatabase != r.LifecycleDatabase || IncidentId != r.Evidence.Actions.IncidentId || PlanId != r.Evidence.Actions.PlanId || ApprovedIntentId != original.IntentId || ApprovedGraphRevision != receipt.GraphRevision || ApprovedNativeRevision != r.NativeRevision)
            throw new InvalidOperationException("The native resolution differs from its original terminal approval.");
    }
}
public sealed record IncidentClosureResolution(Guid IntentId, IncidentClosureResolutionReview Review, string Reason)
{
    public void Require()
    {
        if (Review is null) throw new InvalidOperationException("An original native resolution review is required."); Review.Require();
        if (IntentId == Guid.Empty || IntentId == Review.ApprovedIntentId || string.IsNullOrWhiteSpace(Reason) || Reason != Reason.Trim() || Reason.Length is < 20 or > 2000 || Reason.Any(c => char.IsControl(c) && c is not ('\r' or '\n')))
            throw new InvalidOperationException("An explicit resolution identity and reason are required.");
    }
}
