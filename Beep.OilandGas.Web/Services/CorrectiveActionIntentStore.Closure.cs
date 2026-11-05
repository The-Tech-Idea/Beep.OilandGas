using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

public sealed partial class CorrectiveActionIntentStore
{
    public async Task<IncidentClosureResolution?> ReadResolutionAsync(FieldSelectionReview context, string incidentId)
    {
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw new InvalidOperationException("An exact incident is required.");
        var stored = await storage.GetAsync<IncidentClosureResolution>(Key(context, incidentId) + ".native-resolution"); if (!stored.Success) return null;
        var original = stored.Value ?? throw new InvalidOperationException("The native resolution original is unavailable."); original.Require(); var prior = original.Review.Context;
        if (prior.Company != context.Company || prior.ActorUserId != context.ActorUserId || prior.Database != context.Database || prior.FieldId != context.FieldId || original.Review.IncidentId != incidentId) throw new InvalidOperationException("The retained native resolution belongs to another scope."); return original;
    }
    public async Task RetainResolutionAsync(IncidentClosureResolution original)
    {
        original.Require(); await Gate.WaitAsync();
        try
        {
            var r = original.Review; var prior = await ReadResolutionAsync(r.Context, r.IncidentId);
            if (prior is not null && JsonSerializer.Serialize(prior) != JsonSerializer.Serialize(original)) throw new InvalidOperationException("Reconcile the retained native resolution before replacing it.");
            await storage.SetAsync(Key(r.Context, r.IncidentId) + ".native-resolution", original);
            if (JsonSerializer.Serialize(await ReadResolutionAsync(r.Context, r.IncidentId)) != JsonSerializer.Serialize(original)) throw new InvalidOperationException("The native resolution could not be retained.");
        }
        finally { Gate.Release(); }
    }
    public async Task ClearResolutionAsync(IncidentClosureResolution original)
    {
        original.Require(); await Gate.WaitAsync();
        try
        {
            var r = original.Review;
            var prior = await ReadResolutionAsync(r.Context, r.IncidentId);
            if (prior is not null && JsonSerializer.Serialize(prior) != JsonSerializer.Serialize(original)) throw new InvalidOperationException("The native resolution confirmation differs from its retained original.");
            if (prior is not null) await storage.DeleteAsync(Key(r.Context, r.IncidentId) + ".native-resolution");
            if (await ReadResolutionAsync(r.Context, r.IncidentId) is not null) throw new InvalidOperationException("The confirmed native resolution could not be cleared.");
        }
        finally { Gate.Release(); }
    }
    public async Task<CompanyHseClosureCommand?> ReadClosureAsync(FieldSelectionReview context, string incidentId)
    {
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(incidentId) || !LeaseSummaryValidator.Identifier(context.FieldId)) throw new InvalidOperationException("An exact closure FIELD and incident are required.");
        var value = await storage.GetAsync<CompanyHseClosureCommand>(Key(context, incidentId) + ".closure"); if (!value.Success) return null;
        var original = value.Value ?? throw new InvalidOperationException("The original closure is unavailable."); original.Require(); var prior = original.Review.Context;
        if (prior.Company != context.Company || prior.ActorUserId != context.ActorUserId || prior.Database != context.Database || prior.FieldId != context.FieldId || original.Review.Evidence.Actions.IncidentId != incidentId) throw new InvalidOperationException("The closure original belongs to another scope.");
        return original;
    }
    public async Task RetainClosureAsync(CompanyHseClosureCommand original)
    {
        original.Require(); await Gate.WaitAsync();
        try
        {
            var context = original.Review.Context; var incident = original.Review.Evidence.Actions.IncidentId; var prior = await ReadClosureAsync(context, incident);
            if (await ReadCorrectionAsync(context, incident) is not null) throw new InvalidOperationException("Reconcile the retained correction return before approving a stage.");
            if (prior is not null && JsonSerializer.Serialize(prior) != JsonSerializer.Serialize(original)) throw new InvalidOperationException("Reconcile the retained closure before starting another stage.");
            await storage.SetAsync(Key(context, incident) + ".closure", original);
            if (JsonSerializer.Serialize(await ReadClosureAsync(context, incident)) != JsonSerializer.Serialize(original)) throw new InvalidOperationException("The closure original could not be retained.");
        }
        finally { Gate.Release(); }
    }
    public async Task ClearClosureAsync(CompanyHseClosureCommand original)
    {
        original.Require(); await Gate.WaitAsync();
        try
        {
            var context = original.Review.Context; var incident = original.Review.Evidence.Actions.IncidentId;
            var prior = await ReadClosureAsync(context, incident);
            if (prior is not null && JsonSerializer.Serialize(prior) != JsonSerializer.Serialize(original)) throw new InvalidOperationException("The retained closure differs from its confirmation.");
            if (prior is not null) await storage.DeleteAsync(Key(context, incident) + ".closure");
            if (await ReadClosureAsync(context, incident) is not null) throw new InvalidOperationException("The confirmed closure original could not be cleared.");
        }
        finally { Gate.Release(); }
    }
    public async Task<CompanyHseCorrectionCommand?> ReadCorrectionAsync(FieldSelectionReview context, string incidentId)
    {
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(incidentId) || !LeaseSummaryValidator.Identifier(context.FieldId)) throw new InvalidOperationException("An exact correction return FIELD and incident are required.");
        var value = await storage.GetAsync<CompanyHseCorrectionCommand>(Key(context, incidentId) + ".correction"); if (!value.Success) return null;
        var original = value.Value ?? throw new InvalidOperationException("The original correction return is unavailable."); original.Require(); var prior = original.Review.Context;
        if (prior.Company != context.Company || prior.ActorUserId != context.ActorUserId || prior.Database != context.Database || prior.FieldId != context.FieldId || original.Review.Evidence.Actions.IncidentId != incidentId) throw new InvalidOperationException("The correction return original belongs to another scope.");
        return original;
    }
    public async Task RetainCorrectionAsync(CompanyHseCorrectionCommand original)
    {
        original.Require(); await Gate.WaitAsync();
        try
        {
            var context = original.Review.Context; var incident = original.Review.Evidence.Actions.IncidentId; var prior = await ReadCorrectionAsync(context, incident);
            if (await ReadClosureAsync(context, incident) is not null) throw new InvalidOperationException("Reconcile the retained closure before returning rejected work.");
            if (prior is not null && JsonSerializer.Serialize(prior) != JsonSerializer.Serialize(original)) throw new InvalidOperationException("Reconcile the retained correction return before starting another stage.");
            await storage.SetAsync(Key(context, incident) + ".correction", original);
            if (JsonSerializer.Serialize(await ReadCorrectionAsync(context, incident)) != JsonSerializer.Serialize(original)) throw new InvalidOperationException("The correction return original could not be retained.");
        }
        finally { Gate.Release(); }
    }
    public async Task ClearCorrectionAsync(CompanyHseCorrectionCommand original)
    {
        original.Require(); await Gate.WaitAsync();
        try
        {
            var context = original.Review.Context; var incident = original.Review.Evidence.Actions.IncidentId;
            var prior = await ReadCorrectionAsync(context, incident);
            if (prior is not null && JsonSerializer.Serialize(prior) != JsonSerializer.Serialize(original)) throw new InvalidOperationException("The retained correction return differs from its confirmation.");
            if (prior is not null) await storage.DeleteAsync(Key(context, incident) + ".correction");
            if (await ReadCorrectionAsync(context, incident) is not null) throw new InvalidOperationException("The confirmed correction return original could not be cleared.");
        }
        finally { Gate.Release(); }
    }
}
