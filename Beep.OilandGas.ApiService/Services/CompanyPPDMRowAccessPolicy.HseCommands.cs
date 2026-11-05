using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Models;
using static Beep.OilandGas.ApiService.Services.CompanyPPDMEvidenceReader;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyPPDMRowAccessPolicy
{
    // A new incident has no case row yet. Capture the reporting authority from the
    // selected FIELD and its actual company entitlement, not an invented case.
    public Task<string> CaptureHseReportAuthorityAsync(ICompanyFieldSelectionService selections, FieldSelectionReview review, string actor) =>
        CaptureHseFieldWriteAuthorityAsync(selections, review, actor, PermissionConstants.HSE.ReportIncident);

    private async Task<string> CaptureHseFieldWriteAuthorityAsync(ICompanyFieldSelectionService selections, FieldSelectionReview review, string actor, string permission)
    {
        ArgumentNullException.ThrowIfNull(selections); ArgumentNullException.ThrowIfNull(review);
        if (string.IsNullOrWhiteSpace(actor) || actor != review.ActorUserId || !Identifier(review.FieldId))
            throw RefusalException.Invalid("An exact reporting account and FIELD are required.");
        FieldSelectionReviewValidator.RequireReview(review, review.Company, actor);
        if (selections is not ICompanyFieldSelectionAuthority preferences) throw RefusalException.Conflict("The fixed stored FIELD preference owner is required.");
        async Task<string> GrantsAsync()
        {
            await preferences.RequireStoredSelectionAsync(review);
            if (review.Database != await company.ResolveAsync() || review.Company != company.Company.Scope || actor != company.Company.UserId)
                throw RefusalException.Conflict("Incident reporting account/company/database/FIELD review changed.");
            if ((await company.ResolveSubjectAsync(actor))?.Permissions.Contains(permission, StringComparer.Ordinal) != true)
                throw RefusalException.Forbidden("Current company HSE command permission is required.");
            var grants = (await assets.ReadAsync(actor, review.Database.Fingerprint)).Where(grant => grant.IsActive && grant.Inherit &&
                grant.AssetType == "FIELD" && grant.AssetId == review.FieldId &&
                PPDMCompanyFieldEligibility.Rank(grant.AccessLevel) >= PPDMCompanyFieldEligibility.Rank("WRITE")).ToArray();
            if (grants.Length == 0) throw RefusalException.Forbidden("An inherited current FIELD write grant is required.");
            return FingerprintRecords(grants.Select(grant => JsonSerializer.Serialize(new { grant.Id, grant.UserId, grant.DatabaseScope,
                grant.AssetType, grant.AssetId, grant.FacilityType, grant.OrganizationId, grant.AccessLevel, grant.Inherit, grant.IsActive, grant.ConcurrencyStamp })));
        }
        var grantEvidence = await GrantsAsync();
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, actor);
        await reader.OpenAsync();
        await DefinitionAsync(metadata, typeof(FIELD), ["FIELD_ID"]);
        async Task<string> FieldAsync()
        {
            var fields = await reader.ReadAsync(typeof(FIELD), [Equal("FIELD_ID", review.FieldId!), Equal("ACTIVE_IND", "Y")]);
            if (fields.Length != 1) throw RefusalException.Forbidden("A unique active FIELD is required for incident reporting.");
            var snapshot = Fingerprint(fields);
            if (PPDMCompanyFieldEligibility.Rank(await GetEffectiveAccessLevelAsync(typeof(FIELD), "FIELD", fields[0])) < PPDMCompanyFieldEligibility.Rank("WRITE"))
                throw RefusalException.Forbidden("Current FIELD data entitlement must permit incident reporting.");
            if (Fingerprint(fields) != snapshot) throw RefusalException.Conflict("The FIELD changed during incident reporting review.");
            return snapshot;
        }
        var fieldEvidence = await FieldAsync();
        if (await GrantsAsync() != grantEvidence || await FieldAsync() != fieldEvidence || await GrantsAsync() != grantEvidence)
            throw RefusalException.Conflict("Incident reporting authority or FIELD evidence changed during review.");
        return FingerprintRecords([grantEvidence, fieldEvidence]);
    }

    public async Task<string> CaptureHseRecordedEvidenceAuthorityAsync(ICompanyFieldSelectionService selections, FieldSelectionReview review, string incident, string actor)
    {
        // Immutable evidence remains readable after closure; mutation routes retain their open-state authority.
        var field = await CaptureHseFieldWriteAuthorityAsync(selections, review, actor, PermissionConstants.HSE.ManageIncidents);
        var recorded = await CaptureHseIncidentAuthorityAsync(review, incident);
        if (await CaptureHseFieldWriteAuthorityAsync(selections, review, actor, PermissionConstants.HSE.ManageIncidents) != field ||
            await CaptureHseIncidentAuthorityAsync(review, incident) != recorded)
            throw RefusalException.Conflict("Current authority or incident ownership changed during recorded cause review.");
        return FingerprintRecords([field, recorded]);
    }

    public async Task<string> CaptureHseIncidentWriteAuthorityAsync(ICompanyFieldSelectionService selections, FieldSelectionReview review, string incident, string actor)
    {
        var fieldAuthority = await CaptureHseFieldWriteAuthorityAsync(selections, review, actor, PermissionConstants.HSE.ManageIncidents);
        var caseEvidence = await CaptureHseIncidentAuthorityAsync(review, incident);
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, actor); await reader.OpenAsync();
        await DefinitionAsync(metadata, typeof(HSE_INCIDENT_RESPONSE), ["INCIDENT_ID", "RESPONSE_OBS_NO"], ("RESPONSE_TYPE", "R_HSE_RESPONSE_TYPE", "ACTION_TYPE"));
        var statuses = (await reader.ReadAsync(typeof(HSE_INCIDENT_RESPONSE), [Equal("INCIDENT_ID", incident), Equal("RESPONSE_TYPE", "STATUS"), Equal("ACTIVE_IND", "Y")])).Cast<HSE_INCIDENT_RESPONSE>().ToArray();
        var statusEvidence = Fingerprint(statuses);
        await ValidateRowsAsync(typeof(HSE_INCIDENT_RESPONSE), "HSE_INCIDENT_RESPONSE", statuses);
        await DefinitionAsync(metadata, typeof(R_HSE_RESPONSE_TYPE), ["ACTION_TYPE"]);
        var vocabulary = await reader.ReadAsync(typeof(R_HSE_RESPONSE_TYPE), [Equal("ACTION_TYPE", "STATUS"), Equal("ACTIVE_IND", "Y")]);
        if (vocabulary.Length != 1 || vocabulary[0].GetType().GetProperty("EFFECTIVE_DATE")!.GetValue(vocabulary[0]) is DateTime { Kind: not DateTimeKind.Utc } || vocabulary[0].GetType().GetProperty("EXPIRY_DATE")!.GetValue(vocabulary[0]) is DateTime { Kind: not DateTimeKind.Utc } || !Window(vocabulary[0], TimeZoneInfo.Utc, TimeProvider.System, false) || statuses.Length == 0 || statuses.Any(row => !Observation(row.RESPONSE_OBS_NO) || row.EFFECTIVE_DATE is not { Kind: DateTimeKind.Utc } || row.EFFECTIVE_DATE > DateTime.UtcNow || row.EXPIRY_DATE is { Kind: not DateTimeKind.Utc }) || statuses.Select(row => row.RESPONSE_OBS_NO).Distinct().Count() != statuses.Length)
            throw RefusalException.Conflict("Current recorded UTC incident state/reference evidence is required.");
        var latest = statuses.Where(row => row.EFFECTIVE_DATE == statuses.Max(value => value.EFFECTIVE_DATE)).ToArray();
        if (latest.Length != 1 || !Window(latest[0], TimeZoneInfo.Utc, TimeProvider.System, false) || latest[0].RESPONSE_RESULT is not (IncidentState.Reported or IncidentState.UnderInvestigation or IncidentState.PendingRCA or IncidentState.PendingCorrectiveAction or IncidentState.PendingCloseOut))
            throw RefusalException.Conflict("A unique recorded incident state permitting incident commands is required.");
        var vocabularyEvidence = Fingerprint(vocabulary);
        if (await CaptureHseIncidentAuthorityAsync(review, incident) != caseEvidence) throw RefusalException.Conflict("The incident read interpretation changed during command review.");
        if (Fingerprint(await reader.ReadAsync(typeof(HSE_INCIDENT_RESPONSE), [Equal("INCIDENT_ID", incident), Equal("RESPONSE_TYPE", "STATUS"), Equal("ACTIVE_IND", "Y")])) != statusEvidence ||
            Fingerprint(await reader.ReadAsync(typeof(R_HSE_RESPONSE_TYPE), [Equal("ACTION_TYPE", "STATUS"), Equal("ACTIVE_IND", "Y")])) != vocabularyEvidence)
            throw RefusalException.Conflict("The recorded incident state/reference changed during command review.");
        if (await CaptureHseFieldWriteAuthorityAsync(selections, review, actor, PermissionConstants.HSE.ManageIncidents) != fieldAuthority)
            throw RefusalException.Conflict("The FIELD grant or entitlement changed before command review completed.");
        return caseEvidence + statusEvidence + vocabularyEvidence + fieldAuthority;
    }
    // Prepared compound plan creation checks existing canonical references without widening generic mutations.
    public async Task<string> CaptureCorrectiveActionPlanReferencesAsync(FieldSelectionReview review, string incident, CreateCAPlanRequest request)
    {
        await DefinitionAsync(metadata, typeof(PROJECT), ["PROJECT_ID"], ("PROJECT_TYPE", "R_PROJECT_TYPE", "PROJECT_TYPE"));
        await DefinitionAsync(metadata, typeof(PROJECT_BA), ["PROJECT_ID", "BUSINESS_ASSOCIATE_ID"], ("PROJECT_ID", "PROJECT", "PROJECT_ID"), ("BUSINESS_ASSOCIATE_ID", "BUSINESS_ASSOCIATE", "BUSINESS_ASSOCIATE_ID"));
        await DefinitionAsync(metadata, typeof(PROJECT_BA_ROLE), ["PROJECT_ID", "BUSINESS_ASSOCIATE_ID", "ROLE", "ROLE_SEQ_NO"], ("PROJECT_ID", "PROJECT_BA", "PROJECT_ID"), ("ROLE", "R_PROJECT_BA_ROLE", "ROLE"));
        await DefinitionAsync(metadata, typeof(HSE_INCIDENT_RESPONSE), ["INCIDENT_ID", "RESPONSE_OBS_NO"], ("INCIDENT_ID", "HSE_INCIDENT", "INCIDENT_ID"), ("PROJECT_ID", "PROJECT", "PROJECT_ID"), ("RESPONSE_TYPE", "R_HSE_RESPONSE_TYPE", "ACTION_TYPE"));
        var roster = await CaptureHseParticipantRosterAsync(review, incident);
        var reader = await EvidenceReaderAsync();
        async Task<string> CaptureAsync()
        {
            var participant = await reader.ReadAsync(typeof(HSE_INCIDENT_BA), [Equal("INCIDENT_ID", incident), Equal("PARTY_OBS_NO", request.PartyObservation), Equal("PARTY_ROLE_OBS_NO", request.RoleObservation), Equal("INVOLVED_PARTY_BA_ID", request.ResponsibleBaId), Equal("ACTIVE_IND", "Y")]);
            if (participant.Length != 1) throw RefusalException.Conflict("The exact recorded incident participant is unavailable or ambiguous.");
            await ValidateRowsAsync(typeof(HSE_INCIDENT_BA), "HSE_INCIDENT_BA", participant);
            await ReferenceAsync(reader, typeof(R_PROJECT_TYPE), ["PROJECT_TYPE"], [Equal("PROJECT_TYPE", HseReferenceCodes.CorrectiveAction.ProjectType)]);
            await ReferenceAsync(reader, typeof(R_PROJECT_BA_ROLE), ["ROLE"], [Equal("ROLE", HseReferenceCodes.CorrectiveAction.ResponsibleRole)]);
            await ReferenceAsync(reader, typeof(R_HSE_RESPONSE_TYPE), ["ACTION_TYPE"], [Equal("ACTION_TYPE", request.ResponseType)]);
            var projectType = await reader.ReadAsync(typeof(R_PROJECT_TYPE), [Equal("PROJECT_TYPE", HseReferenceCodes.CorrectiveAction.ProjectType), Equal("ACTIVE_IND", "Y")]);
            var role = await reader.ReadAsync(typeof(R_PROJECT_BA_ROLE), [Equal("ROLE", HseReferenceCodes.CorrectiveAction.ResponsibleRole), Equal("ACTIVE_IND", "Y")]);
            var responseType = await reader.ReadAsync(typeof(R_HSE_RESPONSE_TYPE), [Equal("ACTION_TYPE", request.ResponseType), Equal("ACTIVE_IND", "Y")]);
            return Fingerprint(participant.Concat(projectType).Concat(role).Concat(responseType));
        }
        var evidence = await CaptureAsync();
        if (await CaptureHseParticipantRosterAsync(review, incident) != roster || await CaptureAsync() != evidence)
            throw RefusalException.Conflict("The prepared plan participant or reference interpretation changed.");
        return roster + evidence;
    }
    public Task<CAPlanCreationReview> ReviewCorrectiveActionPlanCreationAsync(ICompanyFieldSelectionService selections, FieldSelectionReview review, string incident) =>
        ReviewCorrectiveActionPlanCreationCoreAsync(selections, review, incident, null);

    // Only the typed command may exclude its own separately confirmed compound creation effect.
    internal Task<CAPlanCreationReview> ReviewCorrectiveActionPlanCreationForCommandAsync(ICompanyFieldSelectionService selections, FieldSelectionReview review, string incident, string preparedProject, string capturedWriteAuthority)
    {
        if (string.IsNullOrWhiteSpace(capturedWriteAuthority)) throw RefusalException.Conflict("The command's captured write authority is required.");
        return ReviewCorrectiveActionPlanCreationCoreAsync(selections, review, incident, preparedProject, capturedWriteAuthority);
    }

    private async Task<CAPlanCreationReview> ReviewCorrectiveActionPlanCreationCoreAsync(ICompanyFieldSelectionService selections, FieldSelectionReview review, string incident, string? preparedProject, string? capturedWriteAuthority = null)
    {
        if (!Identifier(incident)) throw RefusalException.Invalid("An exact incident is required.");
        // The typed command already owns repeated write-authority checks around preparation, each mutation and closing confirmation.
        // The public review remains independently responsible for capturing and closing its own write authority.
        var authority = capturedWriteAuthority ?? await CaptureHseIncidentWriteAuthorityAsync(selections, review, incident, review.ActorUserId);
        var reader = await EvidenceReaderAsync();
        async Task<object[]> ReadVocabularyAsync()
        {
            var active = await reader.ReadAsync(typeof(R_HSE_RESPONSE_TYPE), [Equal("ACTIVE_IND", "Y")]);
            var frozenActive = Fingerprint(active);
            var inactive = await reader.ReadAsync(typeof(R_HSE_RESPONSE_TYPE), [Equal("ACTIVE_IND", "N")]);
            if (Fingerprint(active) != frozenActive) throw RefusalException.Conflict("Response vocabulary changed while materializing its history.");
            if (active.Length + inactive.Length > 256) throw RefusalException.Conflict("Response vocabulary exceeds the supported reviewed lookup limit.");
            return active.Concat(inactive).ToArray();
        }
        async Task<(RecordedIncidentParticipant[] Participants, string[] Responses, string Evidence)> CaptureAsync()
        {
            var roster = await CaptureHseParticipantRosterAsync(review, incident);
            var participants = (await reader.ReadAsync(typeof(HSE_INCIDENT_BA), [Equal("INCIDENT_ID", incident), Equal("ACTIVE_IND", "Y")])).Cast<HSE_INCIDENT_BA>().ToArray();
            var participantSnapshot = Fingerprint(participants);
            var choices = participants.Where(Current).OrderBy(row => row.PARTY_OBS_NO).ThenBy(row => row.PARTY_ROLE_OBS_NO)
                .Select(row => new RecordedIncidentParticipant(row.PARTY_OBS_NO, row.PARTY_ROLE_OBS_NO, row.INVOLVED_PARTY_BA_ID, row.INVOLVED_BA_ROLE)).ToArray();
            if (choices.Length == 0) throw RefusalException.Conflict("A current recorded incident participant is required for plan creation.");
            await ReferenceAsync(reader, typeof(R_PROJECT_TYPE), ["PROJECT_TYPE"], [Equal("PROJECT_TYPE", HseReferenceCodes.CorrectiveAction.ProjectType)]);
            await ReferenceAsync(reader, typeof(R_PROJECT_BA_ROLE), ["ROLE"], [Equal("ROLE", HseReferenceCodes.CorrectiveAction.ResponsibleRole)]);
            await DefinitionAsync(metadata, typeof(R_HSE_RESPONSE_TYPE), ["ACTION_TYPE"]);
            var vocabulary = (await ReadVocabularyAsync()).Cast<R_HSE_RESPONSE_TYPE>().ToArray();
            var referenceSnapshot = Fingerprint(vocabulary);
            if (vocabulary.Any(row => !Identifier(row.ACTION_TYPE) || row.ACTIVE_IND is not ("Y" or "N") || row.EFFECTIVE_DATE is { Kind: not DateTimeKind.Utc } || row.EXPIRY_DATE is { Kind: not DateTimeKind.Utc }) ||
                vocabulary.Select(row => row.ACTION_TYPE).Distinct(StringComparer.Ordinal).Count() != vocabulary.Length)
                throw RefusalException.Conflict("Response vocabulary has invalid or ambiguous recorded identities/dates.");
            var responses = vocabulary.Where(row => row.ACTIVE_IND == "Y" && row.ACTION_TYPE != "STATUS" && Current(row)).Select(row => row.ACTION_TYPE).Order(StringComparer.Ordinal).ToArray();
            if (responses.Length == 0) throw RefusalException.Conflict("A recorded non-status response type is required for plan creation.");
            foreach (var response in responses) await ReferenceAsync(reader, typeof(R_HSE_RESPONSE_TYPE), ["ACTION_TYPE"], [Equal("ACTION_TYPE", response)]);
            var projectTypes = await reader.ReadAsync(typeof(R_PROJECT_TYPE), [Equal("PROJECT_TYPE", HseReferenceCodes.CorrectiveAction.ProjectType), Equal("ACTIVE_IND", "Y")]);
            var roles = await reader.ReadAsync(typeof(R_PROJECT_BA_ROLE), [Equal("ROLE", HseReferenceCodes.CorrectiveAction.ResponsibleRole), Equal("ACTIVE_IND", "Y")]);
            await DefinitionAsync(metadata, typeof(HSE_INCIDENT_RESPONSE), ["INCIDENT_ID", "RESPONSE_OBS_NO"], ("RESPONSE_TYPE", "R_HSE_RESPONSE_TYPE", "ACTION_TYPE"));
            var history = (await reader.ReadAsync(typeof(HSE_INCIDENT_RESPONSE), [Equal("INCIDENT_ID", incident)])).Cast<HSE_INCIDENT_RESPONSE>().ToArray();
            var completeHistorySnapshot = Fingerprint(history);
            var historySnapshot = Fingerprint(history.Where(row => row.PROJECT_ID != preparedProject || preparedProject is null));
            if (history.Any(row => !Observation(row.RESPONSE_OBS_NO) || row.RESPONSE_OBS_NO > int.MaxValue || row.RESPONSE_OBS_NO == int.MaxValue && (preparedProject is null || row.PROJECT_ID != preparedProject) || row.ACTIVE_IND is not ("Y" or "N") || !string.IsNullOrEmpty(row.PROJECT_ID) && row.PROJECT_ID != preparedProject ||
                row.EFFECTIVE_DATE is { Kind: not DateTimeKind.Utc } || row.EXPIRY_DATE is { Kind: not DateTimeKind.Utc }) || history.Select(row => row.RESPONSE_OBS_NO).Distinct().Count() != history.Length)
                throw RefusalException.Conflict("Incident project/history must be reconciled before reviewing a new plan.");
            if (await CaptureHseParticipantRosterAsync(review, incident) != roster || Fingerprint(await reader.ReadAsync(typeof(HSE_INCIDENT_BA), [Equal("INCIDENT_ID", incident), Equal("ACTIVE_IND", "Y")])) != participantSnapshot ||
                Fingerprint(await ReadVocabularyAsync()) != referenceSnapshot || Fingerprint(await reader.ReadAsync(typeof(HSE_INCIDENT_RESPONSE), [Equal("INCIDENT_ID", incident)])) != completeHistorySnapshot)
                throw RefusalException.Conflict("Plan creation choices changed during review.");
            return (choices, responses, FingerprintRecords([roster, participantSnapshot, referenceSnapshot, historySnapshot, Fingerprint(projectTypes), Fingerprint(roles)]));
        }
        var initial = await CaptureAsync();
        if (capturedWriteAuthority is null && await CaptureHseIncidentWriteAuthorityAsync(selections, review, incident, review.ActorUserId) != authority ||
            (await CaptureAsync()).Evidence != initial.Evidence ||
            capturedWriteAuthority is null && await CaptureHseIncidentWriteAuthorityAsync(selections, review, incident, review.ActorUserId) != authority)
            throw RefusalException.Conflict("Plan creation authority or recorded choices changed before delivery.");
        var revision = FingerprintRecords([JsonSerializer.Serialize(new { Version = 1, Context = review, Incident = incident, Authority = authority, initial.Evidence })]);
        return new(review, incident, revision, Array.AsReadOnly(initial.Participants), Array.AsReadOnly(initial.Responses));
    }

}
