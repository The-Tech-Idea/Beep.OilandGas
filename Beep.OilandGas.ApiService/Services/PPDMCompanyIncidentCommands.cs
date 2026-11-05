using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using static Beep.OilandGas.ApiService.Services.CompanyPPDMEvidenceReader;

namespace Beep.OilandGas.ApiService.Services;

// Exact investigator assignment over the existing case/reference policy and owned Beep transaction lane.
public sealed partial class PPDMCompanyIncidentCommands(CompanyModuleConnection company, ICompanyFieldSelectionService selections,
    IDMEEditor editor, ICommonColumnHandler columns, IPPDM39DefaultsRepository defaults,
    IPPDMMetadataRepository metadata, IPPDMRowAccessPolicy policy) : ICompanyIncidentCommands
{
    private IPPDMMetadataRepository Metadata => metadata;
    private IPPDMRowAccessPolicy Policy => policy;
    private const string Source = "Beep.OilandGas.HSE";
    private CompanyPPDMRowAccessPolicy Canonical => policy as CompanyPPDMRowAccessPolicy ?? throw RefusalException.Conflict("Canonical HSE incident policy is required.");
    public async Task<IncidentReportOptions> ReviewReportOptionsAsync(FieldSelectionReview review)
    {
        ArgumentNullException.ThrowIfNull(review);
        var authority = await Canonical.CaptureHseReportAuthorityAsync(selections, review, review.ActorUserId);
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, review.ActorUserId);
        await reader.OpenAsync();
        await DefinitionAsync(metadata, typeof(HSE_INCIDENT_SET), ["INCIDENT_SET_ID"]);
        await DefinitionAsync(metadata, typeof(HSE_INCIDENT_TYPE), ["INCIDENT_SET_ID", "INCIDENT_TYPE_ID"], ("INCIDENT_SET_ID", "HSE_INCIDENT_SET", "INCIDENT_SET_ID"));
        await DefinitionAsync(metadata, typeof(HSE_INCIDENT_SEVERITY), ["INCIDENT_SET_ID", "INCIDENT_TYPE_ID", "INCIDENT_SEVERITY_ID"], ("INCIDENT_SET_ID", "HSE_INCIDENT_TYPE", "INCIDENT_SET_ID"));
        async Task<(IncidentReportOption[] Options, string Evidence)> ReadAsync()
        {
            var sets = (await reader.ReadAsync(typeof(HSE_INCIDENT_SET), [Equal("ACTIVE_IND", "Y")], ["INCIDENT_SET_ID", "ACTIVE_IND"], ["EFFECTIVE_DATE", "EXPIRY_DATE"])).Cast<HSE_INCIDENT_SET>().ToArray();
            var setEvidence = Fingerprint(sets);
            var types = (await reader.ReadAsync(typeof(HSE_INCIDENT_TYPE), [Equal("ACTIVE_IND", "Y")], ["INCIDENT_SET_ID", "INCIDENT_TYPE_ID", "ACTIVE_IND"], ["EFFECTIVE_DATE", "EXPIRY_DATE"])).Cast<HSE_INCIDENT_TYPE>().ToArray();
            var typeEvidence = Fingerprint(types);
            var severities = (await reader.ReadAsync(typeof(HSE_INCIDENT_SEVERITY), [Equal("ACTIVE_IND", "Y")], ["INCIDENT_SET_ID", "INCIDENT_TYPE_ID", "INCIDENT_SEVERITY_ID", "ACTIVE_IND"], ["EFFECTIVE_DATE", "EXPIRY_DATE"])).Cast<HSE_INCIDENT_SEVERITY>().ToArray();
            if (Fingerprint(sets) != setEvidence || Fingerprint(types) != typeEvidence || sets.Any(row => !Identifier(row.INCIDENT_SET_ID)) ||
                sets.Select(row => row.INCIDENT_SET_ID).Distinct(StringComparer.Ordinal).Count() != sets.Length ||
                sets.Any(row => row.EFFECTIVE_DATE is { Kind: not DateTimeKind.Utc } || row.EXPIRY_DATE is { Kind: not DateTimeKind.Utc }) ||
                types.Any(row => !Identifier(row.INCIDENT_SET_ID) || !Identifier(row.INCIDENT_TYPE_ID)) ||
                severities.Any(row => !Identifier(row.INCIDENT_SET_ID) || !Identifier(row.INCIDENT_TYPE_ID) || !Identifier(row.INCIDENT_SEVERITY_ID)) ||
                types.Any(row => row.EFFECTIVE_DATE is { Kind: not DateTimeKind.Utc } || row.EXPIRY_DATE is { Kind: not DateTimeKind.Utc }) ||
                severities.Any(row => row.EFFECTIVE_DATE is { Kind: not DateTimeKind.Utc } || row.EXPIRY_DATE is { Kind: not DateTimeKind.Utc }) ||
                types.Select(row => (row.INCIDENT_SET_ID, row.INCIDENT_TYPE_ID)).Distinct().Count() != types.Length ||
                severities.Select(row => (row.INCIDENT_SET_ID, row.INCIDENT_TYPE_ID, row.INCIDENT_SEVERITY_ID)).Distinct().Count() != severities.Length)
                throw RefusalException.Conflict("Incident type or severity references are invalid or ambiguous.");
            var validSets = sets.Where(row => Window(row, TimeZoneInfo.Utc, TimeProvider.System, false)).Select(row => row.INCIDENT_SET_ID).ToHashSet(StringComparer.Ordinal);
            var validTypes = types.Where(row => validSets.Contains(row.INCIDENT_SET_ID) && Window(row, TimeZoneInfo.Utc, TimeProvider.System, false)).Select(row => (row.INCIDENT_SET_ID, row.INCIDENT_TYPE_ID)).ToHashSet();
            var options = severities.Where(row => validTypes.Contains((row.INCIDENT_SET_ID, row.INCIDENT_TYPE_ID)) && Window(row, TimeZoneInfo.Utc, TimeProvider.System, false) &&
                    row.INCIDENT_SEVERITY_ID is "TIER_1" or "TIER_2" or "TIER_3" or "TIER_4")
                .Select(row => new IncidentReportOption(row.INCIDENT_SET_ID, row.INCIDENT_TYPE_ID, row.INCIDENT_SEVERITY_ID))
                .OrderBy(row => row.IncidentSetId, StringComparer.Ordinal).ThenBy(row => row.IncidentTypeId, StringComparer.Ordinal).ThenBy(row => row.SeverityId, StringComparer.Ordinal).ToArray();
            return (options, Fingerprint(sets.Cast<object>().Concat(types).Concat(severities)));
        }
        var first = await ReadAsync();
        if (await Canonical.CaptureHseReportAuthorityAsync(selections, review, review.ActorUserId) != authority || (await ReadAsync()).Evidence != first.Evidence ||
            await Canonical.CaptureHseReportAuthorityAsync(selections, review, review.ActorUserId) != authority)
            throw RefusalException.Conflict("Incident reporting authority or recorded options changed during review.");
        return new(review, FingerprintRecords([authority, first.Evidence]), first.Options);
    }
    public async Task<InvestigatorAssignmentReview> ReviewInvestigatorAssignmentAsync(FieldSelectionReview review, string incidentId)
    {
        if (!Identifier(incidentId)) throw RefusalException.Invalid("An exact incident is required.");
        var authority = await RequireAuthorityAsync(review, incidentId, review.ActorUserId);
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, review.ActorUserId);
        await reader.OpenAsync();
        var inventory = await ReadInventoryAsync(review, incidentId, reader);
        var snapshot = Fingerprint(inventory);
        var participantEvidence = await Canonical.CaptureHseParticipantRosterAsync(review, incidentId);
        if (await RequireAuthorityAsync(review, incidentId, review.ActorUserId) != authority || Fingerprint(await ReadInventoryAsync(review, incidentId, reader)) != snapshot)
            throw RefusalException.Conflict("Incident authority or participant history changed during review.");
        if (await RequireAuthorityAsync(review, incidentId, review.ActorUserId) != authority ||
            await Canonical.CaptureHseParticipantRosterAsync(review, incidentId) != participantEvidence ||
            Fingerprint(await ReadInventoryAsync(review, incidentId, reader)) != snapshot)
            throw RefusalException.Conflict("Incident authority or participant evidence changed before review completed.");
        var current = CurrentInvestigators(inventory).SingleOrDefault();
        if (current is not null && current.INVOLVED_BA_STATUS != HseReferenceCodes.Investigator.Assigned)
            throw RefusalException.Conflict("The current investigator has an inconsistent assignment status.");
        var choices = inventory.Where(row => row.ACTIVE_IND == "Y" && Window(row, TimeZoneInfo.Utc, TimeProvider.System, false))
            .OrderBy(row => row.PARTY_OBS_NO).ThenBy(row => row.PARTY_ROLE_OBS_NO).Select(Choice).ToArray();
        return new(review, incidentId, ReviewRevision(review, incidentId, authority + participantEvidence, snapshot), choices, current is null ? null : Choice(current));
    }

    private static RecordedIncidentParticipant Choice(HSE_INCIDENT_BA row) => new(row.PARTY_OBS_NO, row.PARTY_ROLE_OBS_NO, row.INVOLVED_PARTY_BA_ID, row.INVOLVED_BA_ROLE);
    private static string ReviewRevision(FieldSelectionReview review, string incident, string authority, string inventory) =>
        FingerprintRecords([JsonSerializer.Serialize(new { Version = 1, Context = review, Incident = incident, Authority = authority, Inventory = inventory })]);

    public async Task AssignInvestigatorAsync(FieldSelectionReview review, string incidentId, AssignIncidentInvestigatorRequest request, string actor)
    {
        if (request is null || !Identifier(incidentId) || !Identifier(request.BusinessAssociateId) || actor != review.ActorUserId ||
            !Observation(request.PartyObservation) || !Observation(request.RoleObservation) || request.ExpectedRevision is not { Length: 64 } ||
            request.ExpectedRevision.Any(value => !char.IsAsciiHexDigit(value) || char.IsAsciiLetterLower(value)))
            throw RefusalException.Invalid("An exact full-key recorded participant, expected review revision and authenticated actor are required.");
        var businessAssociateId = request.BusinessAssociateId;
        var authority = await RequireAuthorityAsync(review, incidentId, actor);
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, actor);
        await reader.OpenAsync();
        var inventory = await InventoryAsync(); var inventorySnapshot = Fingerprint(inventory);
        var participantEvidence = await Canonical.CaptureHseParticipantRosterAsync(review, incidentId);
        if (request.ExpectedRevision != ReviewRevision(review, incidentId, authority + participantEvidence, inventorySnapshot))
            throw RefusalException.Conflict("The investigator review is stale; review the recorded outcome before another submission.");
        var current = CurrentInvestigators(inventory);
        var targets = inventory.Where(row => row.PARTY_OBS_NO == request.PartyObservation && row.PARTY_ROLE_OBS_NO == request.RoleObservation &&
            row.ACTIVE_IND == "Y" && Window(row, TimeZoneInfo.Utc, TimeProvider.System, false) && row.INVOLVED_PARTY_BA_ID == businessAssociateId).ToArray();
        if (targets.Length != 1) throw RefusalException.Conflict("The exact current recorded participant role is required; arbitrary BA identities cannot create a party.");
        var target = Copy(targets[0]);
        var alreadyAssigned = current.SingleOrDefault()?.INVOLVED_PARTY_BA_ID == businessAssociateId && current[0].PARTY_OBS_NO == target.PARTY_OBS_NO;
        if (alreadyAssigned && current[0].INVOLVED_BA_STATUS != HseReferenceCodes.Investigator.Assigned)
            throw RefusalException.Conflict("The existing investigator has an inconsistent assignment status.");
        HSE_INCIDENT_BA? retired = null; HSE_INCIDENT_BA? assigned = null;
        var source = editor.GetDataSource(review.Database.ConnectionName) ?? throw new InvalidOperationException("The incident command datasource is unavailable.");
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            PPDMDataSourceTransaction.RequireOwned(source, System.Data.IsolationLevel.Serializable);
            await RequireContextAsync();
            if (Fingerprint(await InventoryAsync()) != inventorySnapshot || await Canonical.CaptureHseParticipantRosterAsync(review, incidentId) != participantEvidence)
                throw RefusalException.Conflict("The complete participant/reference history changed before assignment.");
            if (alreadyAssigned) assigned = Copy(current[0]);
            else
            {
                var maximum = inventory.Where(row => row.PARTY_OBS_NO == target.PARTY_OBS_NO).Max(row => row.PARTY_ROLE_OBS_NO);
                if (maximum >= int.MaxValue) throw RefusalException.Conflict("Participant role observation allocation exceeds its supported range.");
                var now = DateTime.UtcNow;
                assigned = Copy(target); assigned.PARTY_ROLE_OBS_NO = maximum + 1;
                assigned.REMARK = null!; // Role remarks remain on their original recorded observation.
                assigned.INVOLVED_BA_ROLE = HseReferenceCodes.Investigator.Role; assigned.INVOLVED_BA_STATUS = HseReferenceCodes.Investigator.Assigned;
                assigned.ACTIVE_IND = "Y"; assigned.EFFECTIVE_DATE = now; assigned.EXPIRY_DATE = null;
                assigned.ROW_EFFECTIVE_DATE = now; assigned.ROW_EXPIRY_DATE = null;
                assigned.ROW_CREATED_BY = null!; assigned.ROW_CREATED_DATE = null; assigned.ROW_CHANGED_BY = null!; assigned.ROW_CHANGED_DATE = null;
                assigned.PPDM_GUID = Guid.NewGuid().ToString().ToUpperInvariant(); assigned.SOURCE = Source;
                var reference = await Canonical.ValidatePreparedHseInvestigatorAsync(assigned, review.FieldId!);
                async Task PreparedAsync()
                {
                    await RequireContextAsync();
                    if (await Canonical.ValidatePreparedHseInvestigatorAsync(assigned, review.FieldId!) != reference)
                        throw RefusalException.Conflict("The prepared assignment reference evidence changed.");
                }
                if (current.Length == 1)
                {
                    retired = Copy(current[0]); retired.ACTIVE_IND = "N"; retired.EXPIRY_DATE = now; retired.ROW_EXPIRY_DATE = now;
                    await Repository(new Mutation(this, review, incidentId, actor, source, authority, retired, current[0], PPDMRowMutation.Update, PreparedAsync)).UpdateAsync(retired, actor);
                }
                await Repository(new Mutation(this, review, incidentId, actor, source, authority, assigned, null, PPDMRowMutation.Insert, PreparedAsync)).InsertAsync(assigned, actor);
            }
            await ConfirmAsync();
            PPDMDataSourceTransaction.RequireOwned(source, System.Data.IsolationLevel.Serializable);
        });
        await ConfirmAsync();

        PPDMGenericRepository Repository(IPPDMRowAccessPolicy commandPolicy) => new(editor, columns, defaults, metadata, typeof(HSE_INCIDENT_BA), review.Database.ConnectionName, "HSE_INCIDENT_BA", rowAccess: commandPolicy);
        Task<HSE_INCIDENT_BA[]> InventoryAsync() => ReadInventoryAsync(review, incidentId, reader);
        async Task RequireContextAsync()
        {
            if (await RequireAuthorityAsync(review, incidentId, actor) != authority) throw RefusalException.Conflict("Incident state, case interpretation or command authority changed.");
        }
        async Task ConfirmAsync()
        {
            await RequireContextAsync();
            if (assigned is null) throw RefusalException.Conflict("The investigator outcome is unconfirmed.");
            var expected = inventory.Where(row => retired is null || row.PARTY_OBS_NO != retired.PARTY_OBS_NO || row.PARTY_ROLE_OBS_NO != retired.PARTY_ROLE_OBS_NO).ToList();
            if (retired is not null) expected.Add(retired);
            if (!expected.Any(row => row.PARTY_OBS_NO == assigned.PARTY_OBS_NO && row.PARTY_ROLE_OBS_NO == assigned.PARTY_ROLE_OBS_NO)) expected.Add(assigned);
            var saved = await InventoryAsync();
            if (Fingerprint(saved) != Fingerprint(expected) || CurrentInvestigators(saved).SingleOrDefault()?.INVOLVED_PARTY_BA_ID != businessAssociateId)
                throw RefusalException.Conflict("The complete committed investigator roster is unconfirmed; reconcile before retrying.");
            await RequireContextAsync();
        }
    }
    private async Task<HSE_INCIDENT_BA[]> ReadInventoryAsync(FieldSelectionReview review, string incidentId, CompanyPPDMEvidenceReader reader)
    {
        await DefinitionAsync(metadata, typeof(HSE_INCIDENT_BA), ["INCIDENT_ID", "PARTY_OBS_NO", "PARTY_ROLE_OBS_NO"]);
        var rows = (await reader.ReadAsync(typeof(HSE_INCIDENT_BA), [Equal("INCIDENT_ID", incidentId)])).Cast<HSE_INCIDENT_BA>().Select(Copy).ToArray();
        if (rows.Any(row => !Observation(row.PARTY_OBS_NO) || !Observation(row.PARTY_ROLE_OBS_NO) || row.ACTIVE_IND is not ("Y" or "N") ||
            row.EFFECTIVE_DATE is { Kind: not DateTimeKind.Utc } || row.EXPIRY_DATE is { Kind: not DateTimeKind.Utc }) ||
            rows.Select(row => (row.PARTY_OBS_NO, row.PARTY_ROLE_OBS_NO)).Distinct().Count() != rows.Length)
            throw RefusalException.Conflict("The full participant history has invalid or ambiguous keys/dates.");
        await policy.ValidateRowsAsync(typeof(HSE_INCIDENT_BA), "HSE_INCIDENT_BA", rows.Where(row => row.ACTIVE_IND == "Y" && Window(row, TimeZoneInfo.Utc, TimeProvider.System, false)).Cast<object>().ToArray());
        return rows;
    }

    private Task<string> RequireAuthorityAsync(FieldSelectionReview review, string incident, string actor)
        => Canonical.CaptureHseIncidentWriteAuthorityAsync(selections, review, incident, actor);
    private static HSE_INCIDENT_BA[] CurrentInvestigators(IEnumerable<HSE_INCIDENT_BA> rows)
    {
        var found = rows.Where(row => row.ACTIVE_IND == "Y" && Window(row, TimeZoneInfo.Utc, TimeProvider.System, false) && row.INVOLVED_BA_ROLE == HseReferenceCodes.Investigator.Role).ToArray();
        if (found.Length > 1) throw RefusalException.Conflict("The current investigator is ambiguous."); return found;
    }
    private static PropertyInfo[] Properties => typeof(HSE_INCIDENT_BA).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(property => property.CanRead && property.CanWrite).ToArray();
    private static HSE_INCIDENT_BA Copy(HSE_INCIDENT_BA row) { var copy = new HSE_INCIDENT_BA(); foreach (var property in Properties) property.SetValue(copy, property.GetValue(row)); return copy; }
    private static string Payload(HSE_INCIDENT_BA row) => JsonSerializer.Serialize(Properties.Where(property => property.Name is not ("ROW_CHANGED_BY" or "ROW_CHANGED_DATE" or "ROW_CREATED_BY" or "ROW_CREATED_DATE")).OrderBy(property => property.Name).Select(property => new object?[] { property.Name, property.GetValue(row) }));
    private sealed class Mutation(PPDMCompanyIncidentCommands owner, FieldSelectionReview review, string incident, string actor, IDataSource source, string authority,
        HSE_INCIDENT_BA expected, HSE_INCIDENT_BA? original, PPDMRowMutation operation, Func<Task> prepared) : IPPDMRowAccessPolicy
    {
        private readonly string payload = Payload(expected); private readonly string? originalSnapshot = original is null ? null : CanonicalRecord(original); private int used;
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type, string table)
        {
            PPDMDataSourceTransaction.RequireOwned(source, System.Data.IsolationLevel.Serializable);
            if (type != typeof(HSE_INCIDENT_BA) || table != type.Name) throw RefusalException.Forbidden("The command reads only its exact canonical participant.");
            await DefinitionAsync(owner.Metadata, type, ["INCIDENT_ID", "PARTY_OBS_NO", "PARTY_ROLE_OBS_NO"]);
            return [new[] { Equal("INCIDENT_ID", expected.INCIDENT_ID), Equal("PARTY_OBS_NO", expected.PARTY_OBS_NO), Equal("PARTY_ROLE_OBS_NO", expected.PARTY_ROLE_OBS_NO) }];
        }
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type, string table) => owner.Policy.GetReadConstraintsAsync(type, table);
        public Task ValidateRowsAsync(Type type, string table, IReadOnlyList<object> rows) => owner.Policy.ValidateRowsAsync(type, table, rows);
        public Task<string?> GetEffectiveAccessLevelAsync(Type type, string table, object row, bool includeInherited = true) => owner.Policy.GetEffectiveAccessLevelAsync(type, table, row, includeInherited);
        public async Task ValidateMutationAsync(Type type, string table, object entity, object? previous, PPDMRowMutation requested, string? assertedActor)
        {
            PPDMDataSourceTransaction.RequireOwned(source, System.Data.IsolationLevel.Serializable);
            if (type != typeof(HSE_INCIDENT_BA) || table != type.Name || entity is not HSE_INCIDENT_BA row || requested != operation || assertedActor != actor || Payload(row) != payload ||
                (previous is null ? originalSnapshot is not null : CanonicalRecord(previous) != originalSnapshot) || Interlocked.Exchange(ref used, 1) != 0)
                throw RefusalException.Forbidden("The mutation differs from its exact reviewed investigator command.");
            if (await owner.RequireAuthorityAsync(review, incident, actor) != authority) throw RefusalException.Conflict("The incident command authority changed before mutation.");
            await prepared();
            if (original is not null) await owner.Policy.ValidateRowsAsync(type, table, [original]);
            if (row.ROW_CHANGED_BY != actor || row.ROW_CHANGED_DATE is not { Kind: DateTimeKind.Utc } changed || changed > DateTime.UtcNow ||
                requested == PPDMRowMutation.Insert && (row.ROW_CREATED_BY != actor || row.ROW_CREATED_DATE is not { Kind: DateTimeKind.Utc }))
                throw RefusalException.Conflict("Canonical investigator audit columns are unavailable.");
            PPDMDataSourceTransaction.RequireOwned(source, System.Data.IsolationLevel.Serializable);
        }
    }
}
