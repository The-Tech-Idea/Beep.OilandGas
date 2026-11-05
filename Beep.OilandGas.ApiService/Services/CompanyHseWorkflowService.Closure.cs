using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Data.Security;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyHseWorkflowService
{
    public async Task<IncidentClosureResolutionReview> ReviewClosureResolutionAsync(ClaimsPrincipal user, string incidentId)
    {
        var (current, lifecycle, owner) = await ClosureAuthorityAsync(user);
        var recorded = await owner.RecoverClosureStageAsync(current, lifecycle, incidentId, role => RequireClosureRoleAsync(user, current, lifecycle, role), original => RequireClosureEvidenceAsync(user, current, lifecycle, original))
            ?? throw new InvalidOperationException("The original terminal approval is unavailable."); recorded.Require();
        if (recorded.Original.Review.Stage != "INC_CLOSURE") throw new InvalidOperationException("Only a recorded terminal approval can resolve changed native history.");
        await RequireClosureEvidenceAsync(user, current, lifecycle, recorded.Original);
        var review = await corrective.ReviewClosureResolutionAsync(current, recorded.Original, recorded.Receipt); review.RequireAgainst(recorded.Original, recorded.Receipt);
        if (review.Context != current || review.IncidentId != incidentId) throw new InvalidOperationException("The native resolution scope changed.");
        await RequireClosureEvidenceAsync(user, current, lifecycle, recorded.Original); return review;
    }
    public async Task<IncidentClosedReceipt> ResolveClosureAsync(ClaimsPrincipal user, IncidentClosureResolution resolution)
    {
        resolution.Require(); resolution = JsonSerializer.Deserialize<IncidentClosureResolution>(JsonSerializer.Serialize(resolution))!;
        var (current, lifecycle, owner) = await ClosureAuthorityAsync(user);
        async Task<CompanyHseClosureRecovery> Confirm()
        {
            var recorded = await owner.RecoverClosureStageAsync(current, lifecycle, resolution.Review.IncidentId, role => RequireClosureRoleAsync(user, current, lifecycle, role), original => RequireClosureEvidenceAsync(user, current, lifecycle, original))
                ?? throw new InvalidOperationException("The original terminal approval is unavailable."); recorded.Require(); resolution.Review.RequireAgainst(recorded.Original, recorded.Receipt);
            await RequireClosureEvidenceAsync(user, current, lifecycle, recorded.Original); return recorded;
        }
        var approval = await Confirm();
        async Task SameApproval()
        {
            if (JsonSerializer.Serialize(await Confirm()) != JsonSerializer.Serialize(approval)) throw new InvalidOperationException("The terminal approval changed during native resolution.");
        }
        var receipt = await corrective.ResolveIncidentClosureAsync(approval.Original, approval.Receipt, resolution, SameApproval) ?? throw new InvalidOperationException("The native resolution is unconfirmed."); receipt.Require();
        if (JsonSerializer.Serialize(receipt.Original) != JsonSerializer.Serialize(approval.Original) || JsonSerializer.Serialize(receipt.Lifecycle) != JsonSerializer.Serialize(approval.Receipt) || JsonSerializer.Serialize(receipt.Resolution) != JsonSerializer.Serialize(resolution)) throw new InvalidOperationException("The resolved native receipt differs from the reviewed original.");
        await SameApproval(); return receipt;
    }
    private async Task<(FieldSelectionReview Context, AssetDatabaseScope Lifecycle, ICompanyHseWorkflowCommands Owner)> ClosureAuthorityAsync(ClaimsPrincipal user)
    {
        var access = await company.RefreshCompanyAsync(); var actor = user.ActingUserId(); var selected = await selections.GetCurrentFieldSelectionAsync();
        FieldSelectionReviewValidator.RequireCurrent(selected, access.Scope, actor);
        if (!selected.SelectionAvailable || selected.FieldId != field.CurrentFieldId || !access.Permissions.Contains(PermissionConstants.Workflow.Approve, StringComparer.Ordinal)) throw new UnauthorizedAccessException("Current company workflow approval authority and FIELD are required.");
        var context = selected.SelectionContext!; var lifecycle = await RequireAuthorityAsync(user, context);
        if (processes is not BoundProcessService bound || await bound.CreateBoundServiceAsync(lifecycle.ConnectionName) is not ICompanyHseWorkflowCommands owner)
            throw new InvalidOperationException("The fixed incident and lifecycle command owners are required.");
        return (context, lifecycle, owner);
    }
    private async Task RequireClosureRoleAsync(ClaimsPrincipal user, FieldSelectionReview context, AssetDatabaseScope lifecycle, string role)
    {
        if (await RequireAuthorityAsync(user, context) != lifecycle) throw new UnauthorizedAccessException("The closure lifecycle binding changed.");
        var access = await company.RefreshCompanyAsync();
        if (!access.Roles.Contains(role, StringComparer.Ordinal) || !user.IsInRole(role) || !access.Permissions.Contains(PermissionConstants.Workflow.Approve, StringComparer.Ordinal)) throw new UnauthorizedAccessException("The current assigned incident approval role is required.");
    }
    private async Task RequireClosureEvidenceAsync(ClaimsPrincipal user, FieldSelectionReview current, AssetDatabaseScope lifecycle, CompanyHseClosureCommand original)
    {
        original.Require(); await RequireClosureReviewEvidenceAsync(user, current, lifecycle, original.Review);
    }
    private async Task RequireClosureReviewEvidenceAsync(ClaimsPrincipal user, FieldSelectionReview current, AssetDatabaseScope lifecycle, CompanyHseClosureReview review, bool archivedRejection = false)
    {
        var prior = review.Context;
        if (prior.Company != current.Company || prior.ActorUserId != current.ActorUserId || prior.Database != current.Database || prior.FieldId != current.FieldId || review.LifecycleDatabase != lifecycle) throw new UnauthorizedAccessException("The retained closure belongs to another account, company, FIELD or binding.");
        await RequireClosureRoleAsync(user, current, lifecycle, review.AssignedRole);
        var plan = await corrective.ResolveIncidentPlanAsync(current, review.Evidence.Actions.IncidentId, current.ActorUserId);
        if (plan is null || plan.Context != current || plan.IncidentId != review.Evidence.Actions.IncidentId || plan.PlanId != review.Evidence.Actions.PlanId) throw new InvalidOperationException("The incident corrective plan changed.");
        var evidence = await corrective!.ReviewClosureAsync(current, plan.IncidentId, plan.PlanId, current.ActorUserId);
        evidence.Require(); evidence = evidence with { Actions = evidence.Actions with { Context = prior } };
        if (archivedRejection)
        {
            review.RequireCorrectionReview();
            var expected = review.Evidence.Decisions.Single(d => d.Review.Revision == review.Evidence.Actions.Revision && d.Decision == "REJECTED");
            var recorded = evidence.Decisions.Where(d => d.IntentId == expected.IntentId).ToArray();
            if (recorded.Length != 1 || JsonSerializer.Serialize(recorded[0]) != JsonSerializer.Serialize(expected)) throw new InvalidOperationException("The original recorded rejection changed.");
        }
        else if (JsonSerializer.Serialize(evidence) != JsonSerializer.Serialize(review.Evidence)) throw new InvalidOperationException("The recorded action or verification evidence changed.");
        if (review.RecordedCause is { } cause)
        {
            var matches = (await incidents.ReadSharedRcaAsync(current, plan.IncidentId)).Where(item => item.IntentId == cause.IntentId).ToArray();
            if (matches.Length != 1 || JsonSerializer.Serialize(matches[0]) != JsonSerializer.Serialize(cause)) throw new InvalidOperationException("The recorded root cause changed before incident closure.");
        }
        await RequireClosureRoleAsync(user, current, lifecycle, review.AssignedRole);
    }
    public async Task<CompanyHseClosureReview> ReviewCorrectionReturnAsync(ClaimsPrincipal user, string incidentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw new InvalidOperationException("An exact incident is required.");
        var (context, lifecycle, owner) = await ClosureAuthorityAsync(user);
        var plan = await corrective.ResolveIncidentPlanAsync(context, incidentId, context.ActorUserId);
        if (plan is null || plan.Context != context || plan.IncidentId != incidentId || !LeaseSummaryValidator.Identifier(plan.PlanId)) throw new InvalidOperationException("The incident corrective plan is unavailable.");
        var evidence = await corrective.ReviewClosureAsync(context, incidentId, plan.PlanId, context.ActorUserId); evidence.Require();
        var native = await corrective.ReviewNativeClosureRevisionAsync(context, incidentId, plan.PlanId);
        var review = await owner.ReviewCorrectionReturnAsync(evidence, lifecycle, native, role => RequireClosureRoleAsync(user, context, lifecycle, role)); review.RequireCorrectionReview();
        if (review.Context != context || review.LifecycleDatabase != lifecycle || JsonSerializer.Serialize(review.Evidence) != JsonSerializer.Serialize(evidence) || review.NativeRevision != native) throw new InvalidOperationException("The rejected verification review differs from its recorded source.");
        await RequireClosureReviewEvidenceAsync(user, context, lifecycle, review);
        if (native != await corrective.ReviewNativeClosureRevisionAsync(context, incidentId, plan.PlanId)) throw new InvalidOperationException("The native incident history changed during correction review.");
        return review;
    }
    public async Task<CompanyHseCorrectionReceipt> ReturnForCorrectionAsync(ClaimsPrincipal user, CompanyHseCorrectionCommand original)
    {
        original.Require(); original = JsonSerializer.Deserialize<CompanyHseCorrectionCommand>(JsonSerializer.Serialize(original))!;
        var (current, lifecycle, owner) = await ClosureAuthorityAsync(user);
        async Task Source()
        {
            await RequireClosureReviewEvidenceAsync(user, current, lifecycle, original.Review);
            if (original.Review.NativeRevision != await corrective.ReviewNativeClosureRevisionAsync(current, original.Review.Evidence.Actions.IncidentId, original.Review.Evidence.Actions.PlanId)) throw new InvalidOperationException("The native incident history changed before correction return.");
        }
        Task Recorded() => RequireClosureReviewEvidenceAsync(user, current, lifecycle, original.Review, true);
        await Recorded();
        var receipt = await owner.ReturnForCorrectionAsync(original, role => RequireClosureRoleAsync(user, current, lifecycle, role), Source, Recorded) ?? throw new InvalidOperationException("The incident correction return is unconfirmed."); receipt.Require();
        if (JsonSerializer.Serialize(receipt.Original) != JsonSerializer.Serialize(original)) throw new InvalidOperationException("The correction receipt differs from its retained original.");
        await Recorded(); return receipt;
    }
    public async Task<CompanyHseClosureReview> ReviewClosureStageAsync(ClaimsPrincipal user, string incidentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw new InvalidOperationException("An exact incident is required.");
        var (context, lifecycle, owner) = await ClosureAuthorityAsync(user);
        var plan = await corrective.ResolveIncidentPlanAsync(context, incidentId, context.ActorUserId);
        if (plan is null || plan.Context != context || plan.IncidentId != incidentId || !LeaseSummaryValidator.Identifier(plan.PlanId)) throw new InvalidOperationException("The incident corrective plan is unavailable.");
        var evidence = await corrective!.ReviewClosureAsync(context, incidentId, plan.PlanId, context.ActorUserId); evidence.Require();
        var native = await corrective.ReviewNativeClosureRevisionAsync(context, incidentId, plan.PlanId);
        var review = await owner.ReviewClosureStageAsync(evidence, lifecycle, native, role => RequireClosureRoleAsync(user, context, lifecycle, role)); review.Require();
        if (review.Context != context || review.LifecycleDatabase != lifecycle || JsonSerializer.Serialize(review.Evidence) != JsonSerializer.Serialize(evidence) || review.NativeRevision != native) throw new InvalidOperationException("The closure review differs from its recorded source.");
        await RequireClosureEvidenceAsync(user, context, lifecycle, new(Guid.NewGuid(), review, "Review current incident closure evidence."));
        if (native != await corrective.ReviewNativeClosureRevisionAsync(context, incidentId, plan.PlanId)) throw new InvalidOperationException("The native incident responses changed during review.");
        return review;
    }
    public async Task<CompanyHseClosureReceipt> CompleteClosureStageAsync(ClaimsPrincipal user, CompanyHseClosureCommand original)
    {
        original.Require(); original = JsonSerializer.Deserialize<CompanyHseClosureCommand>(JsonSerializer.Serialize(original))!;
        var (current, lifecycle, owner) = await ClosureAuthorityAsync(user);
        Task Role(string role) { if (role != original.Review.AssignedRole) throw new UnauthorizedAccessException("The assigned closure role changed."); return RequireClosureRoleAsync(user, current, lifecycle, role); }
        Task Source() => RequireClosureEvidenceAsync(user, current, lifecycle, original);
        await Source();
        var receipt = await owner.CompleteClosureStageAsync(original, Role, Source) ?? throw new InvalidOperationException("The recorded lifecycle approval is unavailable.");
        receipt.Require(original); await Source();
        return receipt;
    }
    public async Task<CompanyHseClosureOutcome> RecordApprovedIncidentClosedAsync(ClaimsPrincipal user, CompanyHseClosureRecovery approved)
    {
        approved.Require(); approved = JsonSerializer.Deserialize<CompanyHseClosureRecovery>(JsonSerializer.Serialize(approved))!;
        var original = approved.Original; var receipt = approved.Receipt;
        if (original.Review.Stage != "INC_CLOSURE") throw new InvalidOperationException("Only the final lifecycle approval can close an incident.");
        var (current, lifecycle, owner) = await ClosureAuthorityAsync(user);
        Task Role(string role) { if (role != original.Review.AssignedRole) throw new UnauthorizedAccessException("The assigned closure role changed."); return RequireClosureRoleAsync(user, current, lifecycle, role); }
        Task Source() => RequireClosureEvidenceAsync(user, current, lifecycle, original);
        async Task Confirm()
        {
            var recorded = await owner.RecoverClosureStageAsync(current, lifecycle, original.Review.Evidence.Actions.IncidentId, Role, _ => Source()) ?? throw new InvalidOperationException("The terminal lifecycle approval is unavailable."); recorded.Require();
            if (JsonSerializer.Serialize(recorded.Original) != JsonSerializer.Serialize(original) || JsonSerializer.Serialize(recorded.Receipt) != JsonSerializer.Serialize(receipt)) throw new InvalidOperationException("The terminal lifecycle approval changed.");
        }
        var native = await corrective!.RecordIncidentClosedAsync(original, receipt, Confirm) ?? throw new InvalidOperationException("The native incident CLOSED status is unconfirmed."); native.Require();
        await Source();
        var result = new CompanyHseClosureOutcome(original, receipt, native); result.Require(); return result;
    }
    public async Task<CompanyHseClosureRecovery?> RecoverClosureStageAsync(ClaimsPrincipal user, string incidentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw new InvalidOperationException("An exact incident is required.");
        var (current, lifecycle, owner) = await ClosureAuthorityAsync(user);
        var recovered = await owner.RecoverClosureStageAsync(current, lifecycle, incidentId, role => RequireClosureRoleAsync(user, current, lifecycle, role), original => RequireClosureEvidenceAsync(user, current, lifecycle, original));
        if (recovered is not null) { recovered.Require(); await RequireClosureEvidenceAsync(user, current, lifecycle, recovered.Original); }
        return recovered;
    }
}
