using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Report;
using static Beep.OilandGas.ApiService.Services.CompanyPPDMEvidenceReader;
using CA = Beep.OilandGas.Models.Constants.HseReferenceCodes.CorrectiveAction;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class PPDMCompanyCorrectiveActionCommands
{
    private static readonly (Type Type, string[] Keys)[] PlanGraph = [
        (typeof(PROJECT), ["PROJECT_ID"]), (typeof(PROJECT_BA), ["PROJECT_ID", "BUSINESS_ASSOCIATE_ID"]),
        (typeof(PROJECT_BA_ROLE), ["PROJECT_ID", "BUSINESS_ASSOCIATE_ID", "ROLE", "ROLE_SEQ_NO"]),
        (typeof(PROJECT_STEP), ["PROJECT_ID", "STEP_ID"]), (typeof(PROJECT_STATUS), ["PROJECT_ID", "STATUS_ID"]),
        (typeof(PROJECT_STEP_BA), ["PROJECT_ID", "BUSINESS_ASSOCIATE_ID", "ROLE", "ROLE_SEQ_NO", "STEP_ID", "STEP_BA_OBS_NO"]),
        (typeof(PROJECT_CONDITION), ["PROJECT_ID", "CONDITION_OBS_NO"]) ];
    private sealed record PlanInventory(object[] Graph, HSE_INCIDENT_RESPONSE[] History, HSE_INCIDENT_RESPONSE[] Links);

    public async Task<CAPlanResolution?> ResolveIncidentPlanAsync(FieldSelectionReview review, string incidentId, string actor)
    {
        FieldSelectionReviewValidator.RequireReview(review, review.Company, actor);
        if (actor != review.ActorUserId || !Identifier(incidentId) || !Identifier(review.FieldId) || selections is not ICompanyFieldSelectionAuthority preferences || rowPolicy is not CompanyPPDMRowAccessPolicy canonical)
            throw RefusalException.Conflict("The fixed FIELD preference and native incident plan owners are required.");
        await preferences.RequireStoredSelectionAsync(review);
        var authority = await canonical.CaptureHseIncidentAuthorityAsync(review, incidentId);
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, actor); await reader.OpenAsync();
        await DefinitionAsync(metadata, typeof(HSE_INCIDENT_RESPONSE), ["INCIDENT_ID", "RESPONSE_OBS_NO"], ("PROJECT_ID", "PROJECT", "PROJECT_ID"));
        await DefinitionAsync(metadata, typeof(PROJECT), ["PROJECT_ID"], ("PROJECT_TYPE", "R_PROJECT_TYPE", "PROJECT_TYPE"));
        async Task<(string? Plan, string Revision)> Capture()
        {
            await preferences.RequireStoredSelectionAsync(review);
            var history = (await reader.ReadAsync(typeof(HSE_INCIDENT_RESPONSE), [Equal("INCIDENT_ID", incidentId)])).Cast<HSE_INCIDENT_RESPONSE>().ToArray();
            if (history.Length > 256 || history.Select(r => r.RESPONSE_OBS_NO).Distinct().Count() != history.Length) throw RefusalException.Conflict("The incident response population is ambiguous or exceeds the plan review limit.");
            var responses = history.Where(r => r.ACTIVE_IND == "Y").ToArray();
            await canonical.ValidateRowsForFiltersAsync(typeof(HSE_INCIDENT_RESPONSE), nameof(HSE_INCIDENT_RESPONSE), [Equal("INCIDENT_ID", incidentId)], responses);
            var links = responses.Where(r => !string.IsNullOrEmpty(r.PROJECT_ID)).ToArray();
            if (links.Length == 0)
            {
                if (history.Any(r => !string.IsNullOrEmpty(r.PROJECT_ID)))
                    throw RefusalException.Conflict("Recorded project history exists without a current active link. Review its status before interpreting actions.");
                await reader.RequireCurrentAsync();
                return (null, FingerprintRecords([InventorySnapshot(history)]));
            }
            if (links.Any(r => !Identifier(r.PROJECT_ID) || r.PROJECT_ID != r.PROJECT_ID.Trim())) throw RefusalException.Conflict("The recorded incident project identity is invalid.");
            var projects = new List<PROJECT>();
            foreach (var id in links.Select(r => r.PROJECT_ID!).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
            {
                var rows = (await reader.ReadAsync(typeof(PROJECT), [Equal("PROJECT_ID", id), Equal("PROJECT_TYPE", CA.ProjectType), Equal("ACTIVE_IND", "Y")])).Cast<PROJECT>().ToArray();
                if (rows.Length > 1) throw RefusalException.Conflict("The recorded corrective-action project is ambiguous.");
                await canonical.ValidateRowsForFiltersAsync(typeof(PROJECT), nameof(PROJECT), [Equal("PROJECT_ID", id)], rows); projects.AddRange(rows);
            }
            if (projects.Count != 1 || links.Count(r => r.PROJECT_ID == projects[0].PROJECT_ID) != 1) throw RefusalException.Conflict("One unique recorded corrective-action project and incident response link are required.");
            var owningLinks = (await reader.ReadAsync(typeof(HSE_INCIDENT_RESPONSE), [Equal("PROJECT_ID", projects[0].PROJECT_ID), Equal("ACTIVE_IND", "Y")])).Cast<HSE_INCIDENT_RESPONSE>().ToArray();
            if (owningLinks.Length != 1 || owningLinks[0].INCIDENT_ID != incidentId || Snapshot(owningLinks[0], true) != Snapshot(links.Single(r => r.PROJECT_ID == projects[0].PROJECT_ID), true)) throw RefusalException.Conflict("The corrective-action project belongs to an ambiguous or changed incident relationship.");
            await reader.RequireCurrentAsync();
            return (projects[0].PROJECT_ID, FingerprintRecords([InventorySnapshot(history), InventorySnapshot(projects), InventorySnapshot(owningLinks)]));
        }
        var first = await Capture(); var second = await Capture();
        if (first != second || await canonical.CaptureHseIncidentAuthorityAsync(review, incidentId) != authority) throw RefusalException.Conflict("The current incident, FIELD or corrective-action plan changed during resolution.");
        await preferences.RequireStoredSelectionAsync(review); return second.Plan is null ? null : new(review, incidentId, second.Plan);
    }

    public Task<CAPlanCreationReview> ReviewPlanCreationAsync(FieldSelectionReview review, string incidentId) =>
        (rowPolicy as CompanyPPDMRowAccessPolicy ?? throw RefusalException.Conflict("The canonical plan reference policy is unavailable."))
            .ReviewCorrectiveActionPlanCreationAsync(selections, review, incidentId);

    public async Task<string> CreatePlanAsync(FieldSelectionReview review, string incidentId, CreateCAPlanRequest request, string actor)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.ExpectedRevision is not { Length: 64 } || request.ExpectedRevision.Any(value => !char.IsAsciiHexDigit(value) || char.IsAsciiLetterLower(value)) || request.PlanId == Guid.Empty || !Identifier(incidentId) || actor != review.ActorUserId || !Identifier(request.PlanName) || !Identifier(request.PlanNumber) ||
            !Identifier(request.ResponseType) || request.ResponseType == "STATUS" || !Identifier(request.ResponsibleBaId) ||
            !Observation(request.PartyObservation) || !Observation(request.RoleObservation) || request.PartyObservation > int.MaxValue || request.RoleObservation > int.MaxValue)
            throw RefusalException.Invalid("A stable plan ID, bounded explicit name/number, recorded response type and complete incident participant key are required.");
        var canonical = rowPolicy as CompanyPPDMRowAccessPolicy ?? throw RefusalException.Conflict("The canonical plan reference policy is unavailable.");
        var project = request.PlanId.ToString("N");
        string? catalogWriteAuthority = null;
        async Task<string> ContextAsync()
        {
            var currentWriteAuthority = await canonical.CaptureHseIncidentWriteAuthorityAsync(selections, review, incidentId, actor);
            catalogWriteAuthority ??= currentWriteAuthority;
            return currentWriteAuthority + await canonical.CaptureCorrectiveActionPlanReferencesAsync(review, incidentId, request);
        }
        var authority = await ContextAsync();
        var initial = await ReadPlanInventoryAsync(review, incidentId, project);
        var inventory = PlanInventorySnapshot(initial);
        await RequireCatalogAsync();
        var source = editor.GetDataSource(review.Database.ConnectionName) ?? throw new InvalidOperationException("The plan command datasource is unavailable.");
        object[]? expected = null; string[]? payloads = null; string? confirmed = null;
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            PPDMDataSourceTransaction.RequireOwned(source, System.Data.IsolationLevel.Serializable);
            await RequireContextAsync();
            await RequireCatalogAsync();
            var current = await ReadPlanInventoryAsync(review, incidentId, project);
            if (PlanInventorySnapshot(current) != inventory) throw RefusalException.Conflict("The complete plan/link inventory changed before creation.");
            var parents = current.Graph.OfType<PROJECT>().ToArray();
            if (parents.Length == 1)
            {
                var saved = parents[0];
                var links = current.Links.Where(row => row.INCIDENT_ID == incidentId && row.PROJECT_ID == project).ToArray();
                if (saved.EFFECTIVE_DATE is not { Kind: DateTimeKind.Utc } start || links.Length != 1)
                    throw RefusalException.Conflict("The plan ID has incomplete recorded creation evidence.");
                var intended = PreparePlanRows(project, incidentId, request, start, links[0].RESPONSE_OBS_NO);
                var found = current.Graph.Concat(current.Links).ToArray();
                if (found.Length != 4)
                    throw RefusalException.Conflict("The plan ID has changed child history; reconcile the recorded plan.");
                if (!found.Select(row => Snapshot(row)).Order(StringComparer.Ordinal).SequenceEqual(intended.Select(row => Snapshot(row)).Order(StringComparer.Ordinal)) ||
                    found.Any(row => !PlanCreatedByIntent(row, request.PlanId, actor)) || current.History.Any(row => !string.IsNullOrEmpty(row.PROJECT_ID) && row.PROJECT_ID != project))
                    throw RefusalException.Conflict("This plan ID does not establish the same unchanged creation intent.");
                expected = found.Select(ClonePlanRow).ToArray();
                payloads = expected.Select(row => Snapshot(row)).ToArray();
            }
            else
            {
                if (current.Graph.Length != 0 || current.Links.Length != 0 || current.History.Any(row => !string.IsNullOrEmpty(row.PROJECT_ID)))
                    throw RefusalException.Conflict("A project identity or incident project link already has recorded history; reconcile it before creation.");
                var maximum = current.History.Length == 0 ? 0 : current.History.Max(row => row.RESPONSE_OBS_NO);
                if (maximum >= int.MaxValue) throw RefusalException.Conflict("The incident response sequence exceeds the supported allocation range.");
                expected = PreparePlanRows(project, incidentId, request, DateTime.UtcNow, maximum + 1);
                payloads = expected.Select(row => Snapshot(row)).ToArray();
                foreach (var row in expected)
                {
                    await Repository(review, row.GetType(), new Mutation(this, review, incidentId, project, actor, source, authority, row, null, PPDMRowMutation.Insert, RequireClosingWriteAuthorityAsync, preparedPlan: true, authorityReader: ContextAsync)).InsertAsync(row, actor);
                    // A provider may mutate its input; confirmation must still compare the frozen reviewed payload.
                    if (Snapshot(row) != payloads[Array.IndexOf(expected, row)]) throw RefusalException.Conflict("The provider changed a prepared plan row.");
                }
            }
            await ConfirmAsync();
            PPDMDataSourceTransaction.RequireOwned(source, System.Data.IsolationLevel.Serializable);
        });
        await ConfirmAsync();
        return project;

        async Task RequireContextAsync()
        {
            if (await ContextAsync() != authority) throw RefusalException.Conflict("The current incident/participant/reference/write authority changed during plan creation.");
        }
        // ContextAsync already captures and closes the full selected participant/reference evidence.
        // Close authority after those reads without repeating the entire reference capture for the same prepared row.
        async Task RequireClosingWriteAuthorityAsync()
        {
            if (await canonical.CaptureHseIncidentWriteAuthorityAsync(selections, review, incidentId, actor) != catalogWriteAuthority)
                throw RefusalException.Conflict("Incident write authority changed during prepared plan reference review.");
        }
        async Task RequireCatalogAsync()
        {
            var catalog = await canonical.ReviewCorrectiveActionPlanCreationForCommandAsync(selections, review, incidentId, project, catalogWriteAuthority!);
            if (catalog.Revision != request.ExpectedRevision || !catalog.ResponseTypes.Contains(request.ResponseType, StringComparer.Ordinal) ||
                catalog.Participants.Count(row => row.PartyObservation == request.PartyObservation && row.RoleObservation == request.RoleObservation && row.BusinessAssociateId == request.ResponsibleBaId) != 1)
                throw RefusalException.Conflict("The recorded plan creation catalog changed or the intent did not select its exact recorded choices.");
        }
        async Task ConfirmAsync()
        {
            await RequireContextAsync();
            await RequireCatalogAsync();
            var current = await ReadPlanInventoryAsync(review, incidentId, project);
            var effect = current.Graph.Concat(current.Links).ToArray();
            var link = expected!.OfType<HSE_INCIDENT_RESPONSE>().Single();
            var history = initial.History.Any(row => row.PROJECT_ID == project) ? initial.History : initial.History.Append(link).ToArray();
            if (effect.Length != 4 || !effect.Select(row => Snapshot(row)).Order(StringComparer.Ordinal).SequenceEqual(payloads!.Order(StringComparer.Ordinal)) ||
                effect.Any(row => !PlanCreatedByIntent(row, request.PlanId, actor)) || InventorySnapshot(current.History) != InventorySnapshot(history))
                throw RefusalException.Conflict("The canonical compound plan effect or surrounding response history could not be confirmed.");
            var fingerprint = PlanInventorySnapshot(current);
            if (confirmed is not null && fingerprint != confirmed) throw RefusalException.Conflict("The compound plan effect changed during confirmation.");
            confirmed = fingerprint;
            await RequireContextAsync();
            if (PlanInventorySnapshot(await ReadPlanInventoryAsync(review, incidentId, project)) != fingerprint)
                throw RefusalException.Conflict("The stored compound plan effect changed during the closing authority check.");
        }
    }

    private static object[] PreparePlanRows(string project, string incident, CreateCAPlanRequest request, DateTime now, decimal observation)
    {
        var guid = request.PlanId.ToString().ToUpperInvariant();
        var rows = new object[] {
            new PROJECT { PROJECT_ID = project, PROJECT_TYPE = CA.ProjectType, PROJECT_NAME = request.PlanName, PROJECT_NUM = request.PlanNumber },
            new PROJECT_BA { PROJECT_ID = project, BUSINESS_ASSOCIATE_ID = request.ResponsibleBaId },
            new PROJECT_BA_ROLE { PROJECT_ID = project, BUSINESS_ASSOCIATE_ID = request.ResponsibleBaId, ROLE = CA.ResponsibleRole, ROLE_SEQ_NO = 1 },
            new HSE_INCIDENT_RESPONSE { INCIDENT_ID = incident, RESPONSE_OBS_NO = observation, PROJECT_ID = project, RESPONSE_TYPE = request.ResponseType,
                REMARK = JsonSerializer.Serialize(new { request.PartyObservation, request.RoleObservation, request.ExpectedRevision }) }
        };
        foreach (var row in rows)
        {
            var type = row.GetType();
            type.GetProperty("ACTIVE_IND")!.SetValue(row, "Y"); type.GetProperty("EFFECTIVE_DATE")!.SetValue(row, now);
            type.GetProperty("ROW_EFFECTIVE_DATE")!.SetValue(row, now); type.GetProperty("SOURCE")!.SetValue(row, Source); type.GetProperty("PPDM_GUID")!.SetValue(row, guid);
        }
        return rows;
    }
    private static object ClonePlanRow(object row)
    { var copy = Activator.CreateInstance(row.GetType())!; foreach (var property in Properties(row.GetType())) property.SetValue(copy, property.GetValue(row)); return copy; }
    private static bool PlanCreatedByIntent(object row, Guid intent, string actor)
    {
        var type = row.GetType();
        return type.GetProperty("SOURCE")!.GetValue(row) as string == Source && type.GetProperty("PPDM_GUID")!.GetValue(row) as string == intent.ToString().ToUpperInvariant() &&
            type.GetProperty("ROW_CREATED_BY")!.GetValue(row) as string == actor && type.GetProperty("ROW_CHANGED_BY")!.GetValue(row) as string == actor &&
            type.GetProperty("ROW_CREATED_DATE")!.GetValue(row) is DateTime { Kind: DateTimeKind.Utc } created && created <= DateTime.UtcNow &&
            type.GetProperty("ROW_CHANGED_DATE")!.GetValue(row) is DateTime { Kind: DateTimeKind.Utc } changed && changed >= created && changed <= DateTime.UtcNow;
    }
    private async Task<PlanInventory> ReadPlanInventoryAsync(FieldSelectionReview review, string incident, string project)
    {
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, review.ActorUserId); await reader.OpenAsync();
        var graph = new List<object>();
        foreach (var (type, keys) in PlanGraph)
        {
            await DefinitionAsync(metadata, type, keys);
            var rows = await reader.ReadAsync(type, [Equal("PROJECT_ID", project)]);
            string Key(object row) => JsonSerializer.Serialize(keys.Select(key => type.GetProperty(key)!.GetValue(row)));
            if (rows.Select(Key).Distinct(StringComparer.Ordinal).Count() != rows.Length || rows.Any(row => keys.Any(key => type.GetProperty(key)?.GetValue(row) is null)))
                throw RefusalException.Conflict("The plan graph contains incomplete or duplicate identity evidence.");
            graph.AddRange(rows.Select(ClonePlanRow));
        }
        var history = (await reader.ReadAsync(typeof(HSE_INCIDENT_RESPONSE), [Equal("INCIDENT_ID", incident)])).Cast<HSE_INCIDENT_RESPONSE>().Select(Copy).ToArray();
        var links = (await reader.ReadAsync(typeof(HSE_INCIDENT_RESPONSE), [Equal("PROJECT_ID", project)])).Cast<HSE_INCIDENT_RESPONSE>().Select(Copy).ToArray();
        foreach (var rows in new[] { history, links })
            if (rows.Any(row => !Identifier(row.INCIDENT_ID) || !Observation(row.RESPONSE_OBS_NO) || row.RESPONSE_OBS_NO > int.MaxValue || row.ACTIVE_IND is not ("Y" or "N") ||
                row.EFFECTIVE_DATE is { Kind: not DateTimeKind.Utc } || row.EXPIRY_DATE is { Kind: not DateTimeKind.Utc }) || rows.Select(row => (row.INCIDENT_ID, row.RESPONSE_OBS_NO)).Distinct().Count() != rows.Length)
                throw RefusalException.Conflict("The complete incident response history has invalid or duplicate key/date evidence.");
        return new(graph.ToArray(), history, links);
    }
    private static string PlanInventorySnapshot(PlanInventory rows) => InventorySnapshot(rows.Graph) + InventorySnapshot(rows.History) + InventorySnapshot(rows.Links);

}
