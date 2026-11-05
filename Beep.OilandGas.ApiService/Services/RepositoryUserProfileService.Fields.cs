using System.Text.Json;
using System.Text.Json.Nodes;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

// Selection is a self-owned app preference on the existing profile, revalidated through the Beep business gateway.
public sealed partial class RepositoryUserProfileService
{
    private const string SelectionKey = "OilGasFieldSelections";
    private static readonly JsonSerializerOptions SelectionJson = new(JsonSerializerDefaults.Web);

    public async Task RequireStoredSelectionAsync(FieldSelectionReview review)
    {
        ArgumentNullException.ThrowIfNull(review);RequireReview(review);await RequireSelectionBoundary(review);
        async Task Check()
        {
            var current=await (from user in repository.Users.AsNoTracking()
                join extension in repository.Set<AppUserExtension>().AsNoTracking() on user.Id equals extension.UserId into profiles
                from profile in profiles.DefaultIfEmpty()
                where user.Id==review.ActorUserId && user.IsActive
                select new{user.ConcurrencyStamp,PreferencesJson=profile==null?null:profile.PreferencesJson}).SingleOrDefaultAsync()
                ??throw RefusalException.Forbidden("The original selected account is unavailable.");
            var selected=Selections(PreferenceRoot(current.PreferencesJson)).SingleOrDefault(row=>row.Company==review.Company && row.Database==review.Database);
            if(selected is null || selected.ActorUserId!=review.ActorUserId || selected.FieldId!=review.FieldId || RequireRevision(current.ConcurrencyStamp)!=review.Revision)
                throw RefusalException.Conflict("The original stored company/account/FIELD preference changed.");
        }
        await Check();await RequireSelectionBoundary(review);await Check();
    }

    public async Task<List<FieldListItem>> GetAvailableFieldsAsync(string? connectionName = null)
    {
        var captured = await SelectionBoundary(connectionName);
        var result = await ReadFields(captured, null);
        await RequireSelectionBoundary(captured);
        return result.Select(row => new FieldListItem { FieldId = Text(row, "FIELD_ID", required: true)!, FieldName = Text(row, "FIELD_NAME") ?? "",
            Description = Text(row, "REMARK"), LastModifiedDate = Date(row, "ROW_CHANGED_DATE") }).ToList();
    }

    public async Task<FieldResponse> GetCurrentFieldSelectionAsync()
    {
        var captured = await SelectionBoundary();
        var user = await repository.Users.AsNoTracking().SingleAsync(x => x.Id == captured.ActorUserId && x.IsActive);
        var preferences = await repository.Set<AppUserExtension>().AsNoTracking().Where(x => x.UserId == user.Id).Select(x => x.PreferencesJson).SingleOrDefaultAsync();
        var selected = Selections(PreferenceRoot(preferences)).SingleOrDefault(row => row.Company == captured.Company && row.Database == captured.Database);
        var review = captured with { Revision = RequireRevision(user.ConcurrencyStamp), FieldId = selected?.FieldId };
        Dictionary<string, object>? field = null;
        if (selected is not null) {
            if (selected.ActorUserId != user.Id) throw RefusalException.Conflict("The stored FIELD preference does not belong to the resolved account.");
            field = (await ReadFields(review, selected.FieldId)).SingleOrDefault();
        }
        await RequireSelectionBoundary(review);
        if (await repository.Users.AsNoTracking().Where(x => x.Id == user.Id).Select(x => x.ConcurrencyStamp).SingleAsync() != review.Revision)
            throw RefusalException.Conflict("The account preferences changed during selection review. Refresh before continuing.");
        return new FieldResponse { SelectionContext = review, SelectionAvailable = field is not null, FieldId = field is null ? "" : review.FieldId!, FieldName = field is null ? "" : Text(field, "FIELD_NAME") ?? "",
            Field = field?.ToDictionary(pair => pair.Key, pair => pair.Value is DBNull ? null : pair.Value)! };
    }

    public async Task<SetActiveFieldResponse> SetCurrentFieldSelectionAsync(SetActiveFieldRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var id = request.FieldId; var assertedConnection = request.ConnectionName; var reviewed = request.Context;
        if (!LeaseSummaryValidator.Identifier(id) || reviewed is null) throw RefusalException.Invalid("Review an exact company-bound FIELD selection before saving it.");
        RequireReview(reviewed);
        var captured = await SelectionBoundary(assertedConnection);
        if (reviewed.Company != captured.Company || reviewed.Database != captured.Database || reviewed.ActorUserId != captured.ActorUserId)
            throw RefusalException.Conflict("The FIELD selection's account, company or database changed. Refresh the selector.");
        var user = await repository.Users.SingleAsync(x => x.Id == captured.ActorUserId && x.IsActive);
        if (RequireRevision(user.ConcurrencyStamp) != reviewed.Revision) throw RefusalException.Conflict("The account preferences changed. Refresh before saving a FIELD selection.");
        var field = (await ReadFields(captured, id)).SingleOrDefault() ?? throw RefusalException.Forbidden("Select a FIELD currently available to this account in the selected company.");
        await RequireSelectionBoundary(captured);
        var metadata = await repository.Set<AppUserExtension>().SingleOrDefaultAsync(x => x.UserId == user.Id);
        var root = PreferenceRoot(metadata?.PreferencesJson); var selections = Selections(root);
        var current = selections.SingleOrDefault(row => row.Company == captured.Company && row.Database == captured.Database);
        if (current?.FieldId != reviewed.FieldId) throw RefusalException.Conflict("The saved FIELD preference changed. Refresh the selector.");
        var revision = Guid.NewGuid().ToString("N"); var confirmed = captured with { Revision = revision, FieldId = id };
        selections.RemoveAll(row => row.Company == captured.Company && row.Database == captured.Database);
        selections.Add(confirmed); root[SelectionKey] = JsonSerializer.SerializeToNode(selections, SelectionJson);
        var json = root.ToJsonString(SelectionJson);
        if (json.Length > 4000) throw RefusalException.Conflict("The existing account preference capacity is exceeded. Review retained preferences before selecting another FIELD.");
        if (metadata is null) { metadata = new() { UserId = user.Id, CreatedUtc = time.GetUtcNow().UtcDateTime }; repository.Add(metadata); }
        metadata.PreferencesJson = json; metadata.ChangedBy = user.Id; metadata.ChangedUtc = time.GetUtcNow().UtcDateTime;
        user.ConcurrencyStamp = revision; // Existing Identity optimistic token; SaveChanges commits profile and token together.
        await RequireSelectionBoundary(captured);
        try { await repository.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException error) { throw new RefusalException(RefusalKind.Conflict, "The account preferences changed during selection. Refresh before another save.", error); }
        var readback = await GetCurrentFieldSelectionAsync();
        if (readback.SelectionContext != confirmed || readback.FieldId != id || Text(field, "FIELD_ID", required: true) != id)
            throw RefusalException.Conflict("The saved FIELD could not be confirmed. Reload selection before another change.");
        return new SetActiveFieldResponse { Success = true, FieldId = id, Context = confirmed };
    }

    private async Task<FieldSelectionReview> SelectionBoundary(string? connectionName = null)
    {
        var actor = await account.RequireAsync(); company.RequireActor(actor);
        var database = await company.ResolveAsync(connectionName);
        var user = await repository.Users.AsNoTracking().SingleAsync(x => x.Id == actor && x.IsActive);
        return new(company.Company.Scope, database, actor, RequireRevision(user.ConcurrencyStamp), null);
    }
    private async Task RequireSelectionBoundary(FieldSelectionReview captured)
    {
        if (await account.RequireAsync() != captured.ActorUserId || await company.ResolveAsync() != captured.Database || company.Company.Scope != captured.Company)
            throw RefusalException.Conflict("Company/account/database authority changed during FIELD selection.");
        company.RequireActor(captured.ActorUserId);
    }
    private async Task<List<Dictionary<string, object>>> ReadFields(FieldSelectionReview captured, string? id)
    {
        await RequireSelectionBoundary(captured);
        var filters = new List<AppFilter> { new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } };
        if (id is not null) filters.Add(new() { FieldName = "FIELD_ID", Operator = "=", FilterValue = id });
        var result = await fields.GetEntitiesAsync("FIELD", filters, captured.Database.ConnectionName);
        if (result is null || !result.Success || result.Entities is null || result.Count != result.Entities.Count || result.Entities.Count > 1000)
            throw RefusalException.Conflict("The provider did not return a complete supported FIELD catalog.");
        foreach (var row in result.Entities)
            if (row is null || !LeaseSummaryValidator.Identifier(Text(row, "FIELD_ID", required: true)) || Text(row, "ACTIVE_IND", required: true) != "Y" || id is not null && Text(row, "FIELD_ID", required: true) != id)
                throw RefusalException.Conflict("The FIELD catalog contains incomplete or mismatching identities.");
        if (result.Entities.Select(row => Text(row, "FIELD_ID", required: true)).Distinct(StringComparer.Ordinal).Count() != result.Entities.Count)
            throw RefusalException.Conflict("The FIELD catalog contains duplicate canonical identities.");
        await RequireSelectionBoundary(captured);
        return result.Entities;
    }
    private static string? Text(Dictionary<string, object> row, string key, bool required = false)
    {
        if (!row.TryGetValue(key, out var value) || value is null or DBNull) { if (required) throw RefusalException.Conflict("Required FIELD evidence is absent."); return null; }
        return value as string ?? throw RefusalException.Conflict("FIELD text evidence has an unexpected provider type.");
    }
    private static DateTime? Date(Dictionary<string, object> row, string key)
    {
        if (!row.TryGetValue(key, out var value) || value is null or DBNull) return null;
        return value is DateTime date ? date : throw RefusalException.Conflict("FIELD date evidence has an unexpected provider type.");
    }
    private static string RequireRevision(string? value) => LeaseSummaryValidator.Identifier(value) ? value! : throw RefusalException.Conflict("The existing account preference revision is unavailable.");
    private static void RequireReview(FieldSelectionReview review)
    {
        if (review.Company is null || review.Database is null || !LeaseSummaryValidator.Identifier(review.Company.TenantId) || !LeaseSummaryValidator.Identifier(review.Company.BusinessAssociateId) ||
            !LeaseSummaryValidator.Identifier(review.Database.ConnectionName) || !LeaseSummaryValidator.Identifier(review.Database.Fingerprint) || !LeaseSummaryValidator.Identifier(review.ActorUserId) ||
            !LeaseSummaryValidator.Identifier(review.Revision) || review.FieldId is not null && !LeaseSummaryValidator.Identifier(review.FieldId))
            throw RefusalException.Invalid("The FIELD preference review is incomplete.");
    }
    private static JsonObject PreferenceRoot(string? json)
    {
        if (json is null) return new();
        try {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object || document.RootElement.EnumerateObject().Select(p => p.Name).Distinct(StringComparer.Ordinal).Count() != document.RootElement.EnumerateObject().Count())
                throw RefusalException.Conflict("Existing preferences need an unambiguous object before FIELD selection can be saved.");
            return JsonNode.Parse(json)!.AsObject();
        } catch (JsonException error) { throw new RefusalException(RefusalKind.Conflict, "Stored preferences are invalid; review their existing owner before selecting a FIELD.", error); }
    }
    private static List<FieldSelectionReview> Selections(JsonObject root)
    {
        if (!root.TryGetPropertyValue(SelectionKey, out var node)) return [];
        List<FieldSelectionReview> result;
        try { result = node?.Deserialize<List<FieldSelectionReview>>(SelectionJson) ?? throw RefusalException.Conflict("Stored FIELD preference evidence is missing."); }
        catch (JsonException error) { throw new RefusalException(RefusalKind.Conflict, "Stored FIELD preference evidence is invalid.", error); }
        foreach (var review in result) { if (review is null || review.FieldId is null) throw RefusalException.Conflict("Stored FIELD selection is incomplete."); RequireReview(review); }
        if (result.Select(row => (row.Company, row.Database)).Distinct().Count() != result.Count) throw RefusalException.Conflict("Stored FIELD preferences have duplicate company/database identities.");
        return result;
    }
    private static string PreserveFieldSelections(string? stored, string incoming)
    {
        var next = JsonNode.Parse(incoming);
        if (next is JsonObject supplied && supplied.ContainsKey(SelectionKey)) throw RefusalException.Invalid("FIELD selections belong to the reviewed selector endpoint, not general profile updates.");
        if (stored is null) return incoming;
        var before = JsonNode.Parse(stored);
        if (before is not JsonObject root || !root.TryGetPropertyValue(SelectionKey, out var selections)) return incoming;
        Selections(root);
        if (next is not JsonObject changed) throw RefusalException.Invalid("General preferences must remain an object while saved FIELD preferences exist.");
        changed[SelectionKey] = selections!.DeepClone();
        var json = changed.ToJsonString(SelectionJson);
        if (json.Length > 4000) throw RefusalException.Invalid("Preferences and retained FIELD selections exceed the existing account capacity.");
        return json;
    }
}
