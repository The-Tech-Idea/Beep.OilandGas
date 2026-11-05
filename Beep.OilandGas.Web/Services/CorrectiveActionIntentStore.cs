using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.HSE.Models;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Beep.OilandGas.Web.Services;

public sealed record CorrectiveActionOriginal(FieldSelectionReview Context, string IncidentId, AddCARequest Request, CorrectiveActionOptions Options)
{
    public void Require()
    {
        if (Context is null || Request is null || Options is null) throw new InvalidOperationException("The original action, choices and FIELD are required.");
        FieldSelectionReviewValidator.RequireReview(Context, Context.Company, Context.ActorUserId);
        Options.Require();
        if (!LeaseSummaryValidator.Identifier(Context.FieldId) || !LeaseSummaryValidator.Identifier(IncidentId) || Request.ActionId == Guid.Empty ||
            string.IsNullOrWhiteSpace(Request.CADescription) || Request.CADescription != Request.CADescription.Trim() || Request.CADescription.Length > 128 ||
            Request.CADescription.Any(char.IsControl) || !LeaseSummaryValidator.Identifier(Request.CAType) ||
            !LeaseSummaryValidator.Identifier(Request.ResponsibleBaId) || Request.DueDate.Kind != DateTimeKind.Utc || Options.Context != Context || Options.IncidentId != IncidentId ||
            !Options.ActionTypes.Contains(Request.CAType, StringComparer.Ordinal) || !Options.Participants.Contains(Request.ResponsibleBaId, StringComparer.Ordinal))
            throw new InvalidOperationException("The original corrective action is incomplete.");
    }
    public string Revision() { Require(); return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this)); }
}

public sealed record CorrectiveActionCompletionOriginal(CACompletionReview Review, string Notes)
{
    public FieldSelectionReview Context => Review.Context;
    public string IncidentId => Review.IncidentId;
    public int StepSequence => Review.StepSequence;
    public void Require()
    {
        if (Review is null || Context is null) throw new InvalidOperationException("The completion FIELD is required.");
        FieldSelectionReviewValidator.RequireReview(Context, Context.Company, Context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(Context.FieldId) || !LeaseSummaryValidator.Identifier(IncidentId) || !LeaseSummaryValidator.Identifier(Review.PlanId) ||
            !LeaseSummaryValidator.Identifier(Review.StepId) || !LeaseSummaryValidator.Identifier(Review.ResponsiblePerson) || !ProcessDefinitionRevision.IsValid(Review.Revision) ||
            string.IsNullOrWhiteSpace(Review.Description) || Review.DueDate.Kind != DateTimeKind.Utc || StepSequence <= 0 ||
            string.IsNullOrWhiteSpace(Notes) || Notes != Notes.Trim() || Notes.Length > 2000 || Notes.Any(c => char.IsControl(c) && c is not '\r' and not '\n' and not '\t'))
            throw new InvalidOperationException("The original completion evidence is incomplete.");
    }
    public string Revision() { Require(); return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this)); }
}

public interface ICorrectiveActionIntentStore
{
    Task<CompanyHseCorrectionCommand?> ReadCorrectionAsync(FieldSelectionReview context, string incidentId);
    Task RetainCorrectionAsync(CompanyHseCorrectionCommand original);
    Task ClearCorrectionAsync(CompanyHseCorrectionCommand original);
    Task<CorrectiveActionOriginal?> ReadAsync(FieldSelectionReview context, string incidentId);
    Task RetainAsync(CorrectiveActionOriginal original);
    Task ClearAsync(CorrectiveActionOriginal original);
    Task<CorrectiveActionCompletionOriginal?> ReadCompletionAsync(FieldSelectionReview context, string incidentId);
    Task RetainCompletionAsync(CorrectiveActionCompletionOriginal original);
    Task ClearCompletionAsync(CorrectiveActionCompletionOriginal original);
    Task<CAVerificationDecision?> ReadVerificationAsync(FieldSelectionReview context, string incidentId);
    Task RetainVerificationAsync(CAVerificationDecision original);
    Task ClearVerificationAsync(CAVerificationDecision original);
    Task<CompanyHseClosureCommand?> ReadClosureAsync(FieldSelectionReview context, string incidentId);
    Task RetainClosureAsync(CompanyHseClosureCommand original);
    Task ClearClosureAsync(CompanyHseClosureCommand original);
    Task<IncidentClosureResolution?> ReadResolutionAsync(FieldSelectionReview context, string incidentId);
    Task RetainResolutionAsync(IncidentClosureResolution original);
    Task ClearResolutionAsync(IncidentClosureResolution original);
}

public sealed partial class CorrectiveActionIntentStore(ProtectedSessionStorage storage) : ICorrectiveActionIntentStore
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static string Key(FieldSelectionReview context, string incident) => "oilgas.hse.corrective-action." +
        Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { context.Company, context.ActorUserId, context.Database, context.FieldId, incident })));
    public async Task<CAVerificationDecision?> ReadVerificationAsync(FieldSelectionReview context, string incidentId)
    {
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(context.FieldId) || !LeaseSummaryValidator.Identifier(incidentId)) throw new InvalidOperationException("An exact FIELD and incident are required.");
        var value = await storage.GetAsync<CAVerificationDecision>(Key(context, incidentId) + ".verification");
        if (!value.Success) return null;
        var saved = value.Value ?? throw new InvalidOperationException("The original verification is unavailable."); saved.Require(); var prior = saved.Review.Context;
        if (prior.Company != context.Company || prior.ActorUserId != context.ActorUserId || prior.Database != context.Database || prior.FieldId != context.FieldId || saved.Review.IncidentId != incidentId)
            throw new InvalidOperationException("The verification belongs to another account, company, FIELD or incident.");
        return saved;
    }
    public async Task RetainVerificationAsync(CAVerificationDecision original)
    {
        original.Require(); var context = original.Review.Context; var incident = original.Review.IncidentId; await Gate.WaitAsync();
        try
        {
            var prior = await ReadVerificationAsync(context, incident); var json = JsonSerializer.Serialize(original);
            if (prior is not null && JsonSerializer.Serialize(prior) != json) throw new InvalidOperationException("Reconcile the retained verification first.");
            await storage.SetAsync(Key(context, incident) + ".verification", original);
            if (JsonSerializer.Serialize(await ReadVerificationAsync(context, incident)) != json) throw new InvalidOperationException("The verification could not be retained.");
        }
        finally { Gate.Release(); }
    }
    public async Task ClearVerificationAsync(CAVerificationDecision original)
    {
        original.Require(); var context = original.Review.Context; var incident = original.Review.IncidentId; await Gate.WaitAsync();
        try
        {
            var prior = await ReadVerificationAsync(context, incident);
            if (prior is not null && JsonSerializer.Serialize(prior) != JsonSerializer.Serialize(original)) throw new InvalidOperationException("Another retained verification was preserved.");
            await storage.DeleteAsync(Key(context, incident) + ".verification");
        }
        finally { Gate.Release(); }
    }
    public async Task<CorrectiveActionCompletionOriginal?> ReadCompletionAsync(FieldSelectionReview context, string incidentId)
    {
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(context.FieldId) || !LeaseSummaryValidator.Identifier(incidentId)) throw new InvalidOperationException("An exact FIELD and incident are required.");
        var value = await storage.GetAsync<CorrectiveActionCompletionOriginal>(Key(context, incidentId) + ".completion");
        if (!value.Success) return null;
        var saved = value.Value ?? throw new InvalidOperationException("The original completion is unavailable."); saved.Require();
        if (saved.Context.Company != context.Company || saved.Context.ActorUserId != context.ActorUserId || saved.Context.Database != context.Database || saved.Context.FieldId != context.FieldId || saved.IncidentId != incidentId)
            throw new InvalidOperationException("The completion belongs to another account, company, FIELD or incident.");
        return saved;
    }
    public async Task RetainCompletionAsync(CorrectiveActionCompletionOriginal original)
    {
        original.Require(); await Gate.WaitAsync();
        try
        {
            var prior = await ReadCompletionAsync(original.Context, original.IncidentId);
            if (prior is not null && prior.Revision() != original.Revision()) throw new InvalidOperationException("Reconcile the retained completion first.");
            await storage.SetAsync(Key(original.Context, original.IncidentId) + ".completion", original);
            if ((await ReadCompletionAsync(original.Context, original.IncidentId))?.Revision() != original.Revision()) throw new InvalidOperationException("The completion could not be retained.");
        }
        finally { Gate.Release(); }
    }
    public async Task ClearCompletionAsync(CorrectiveActionCompletionOriginal original)
    {
        original.Require(); await Gate.WaitAsync();
        try
        {
            var prior = await ReadCompletionAsync(original.Context, original.IncidentId);
            if (prior is not null && prior.Revision() != original.Revision()) throw new InvalidOperationException("Another retained completion was preserved.");
            await storage.DeleteAsync(Key(original.Context, original.IncidentId) + ".completion");
        }
        finally { Gate.Release(); }
    }
    public async Task<CorrectiveActionOriginal?> ReadAsync(FieldSelectionReview context, string incidentId)
    {
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(context.FieldId) || !LeaseSummaryValidator.Identifier(incidentId)) throw new InvalidOperationException("An exact FIELD and incident are required.");
        var value = await storage.GetAsync<CorrectiveActionOriginal>(Key(context, incidentId));
        if (!value.Success) return null;
        var original = value.Value ?? throw new InvalidOperationException("The original action is unavailable."); original.Require();
        var prior = original.Context;
        if (prior.Company != context.Company || prior.ActorUserId != context.ActorUserId || prior.Database != context.Database || prior.FieldId != context.FieldId || original.IncidentId != incidentId)
            throw new InvalidOperationException("The original action belongs to another account, company, FIELD or incident.");
        return original;
    }
    public async Task RetainAsync(CorrectiveActionOriginal original)
    {
        original.Require(); await Gate.WaitAsync();
        try
        {
            var prior = await ReadAsync(original.Context, original.IncidentId);
            if (prior is not null && prior.Revision() != original.Revision()) throw new InvalidOperationException("Reconcile the retained action first.");
            await storage.SetAsync(Key(original.Context, original.IncidentId), original);
            if ((await ReadAsync(original.Context, original.IncidentId))?.Revision() != original.Revision()) throw new InvalidOperationException("The original action could not be retained.");
        }
        finally { Gate.Release(); }
    }
    public async Task ClearAsync(CorrectiveActionOriginal original)
    {
        original.Require(); await Gate.WaitAsync();
        try
        {
            var prior = await ReadAsync(original.Context, original.IncidentId);
            if (prior is not null && prior.Revision() != original.Revision()) throw new InvalidOperationException("Another retained action was preserved.");
            await storage.DeleteAsync(Key(original.Context, original.IncidentId));
        }
        finally { Gate.Release(); }
    }
}
