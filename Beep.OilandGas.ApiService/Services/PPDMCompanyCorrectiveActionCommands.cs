using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using static Beep.OilandGas.ApiService.Services.CompanyPPDMEvidenceReader;
using CA = Beep.OilandGas.Models.Constants.HseReferenceCodes.CorrectiveAction;

namespace Beep.OilandGas.ApiService.Services;

// Typed commands over the existing policy/evidence reader/repository and owned Beep transaction lane.
public sealed partial class PPDMCompanyCorrectiveActionCommands(CompanyModuleConnection company, ICompanyFieldSelectionService selections,
    IUserAssetAccessStore assets, IDMEEditor editor, ICommonColumnHandler columns, IPPDM39DefaultsRepository defaults,
    IPPDMMetadataRepository metadata, IPPDMRowAccessPolicy policy) : ICompanyCorrectiveActionCommands
{
    private readonly IPPDMRowAccessPolicy rowPolicy = policy;
    private IPPDMMetadataRepository Metadata => metadata;
    private const string Source = "Beep.OilandGas.HSE";
    private sealed record State(PROJECT_STEP Step, PROJECT_STATUS Status, PROJECT_STEP_BA? Responsible);

    public async Task<CorrectiveActionOptions> ReviewActionOptionsAsync(FieldSelectionReview review, string incidentId, string projectId, string actor)
    {
        var authority = await RequireAuthorityAsync(review, incidentId, projectId, actor);
        await RequireOpenIncidentMutationAsync(review, incidentId);
        if (rowPolicy is not CompanyPPDMRowAccessPolicy canonical)
            throw RefusalException.Conflict("The canonical corrective-action reference policy is unavailable.");
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, actor);
        await reader.OpenAsync();
        await DefinitionAsync(metadata, typeof(R_PROJ_STEP_TYPE), ["STEP_TYPE"]);
        await DefinitionAsync(metadata, typeof(PROJECT_BA_ROLE), ["PROJECT_ID", "BUSINESS_ASSOCIATE_ID", "ROLE", "ROLE_SEQ_NO"], ("PROJECT_ID", "PROJECT_BA", "PROJECT_ID"), ("ROLE", "R_PROJECT_BA_ROLE", "ROLE"));
        async Task<(string Evidence, string[] Types, string[] Participants)> Capture()
        {
            if (await RequireAuthorityAsync(review, incidentId, projectId, actor) != authority) throw RefusalException.Conflict("The corrective plan authority changed during review.");
            var types = (await reader.ReadAsync(typeof(R_PROJ_STEP_TYPE), [Equal("ACTIVE_IND", "Y")])).Cast<R_PROJ_STEP_TYPE>().ToArray();
            var roles = (await reader.ReadAsync(typeof(PROJECT_BA_ROLE), [Equal("PROJECT_ID", projectId), Equal("ROLE", CA.ResponsibleRole), Equal("ACTIVE_IND", "Y")])).Cast<PROJECT_BA_ROLE>().ToArray();
            if (types.Any(row => !Identifier(row.STEP_TYPE)) || types.Select(row => row.STEP_TYPE).Distinct(StringComparer.Ordinal).Count() != types.Length ||
                roles.Any(row => !Identifier(row.BUSINESS_ASSOCIATE_ID) || !Observation(row.ROLE_SEQ_NO)) || roles.Select(row => row.BUSINESS_ASSOCIATE_ID).Distinct(StringComparer.Ordinal).Count() != roles.Length)
                throw RefusalException.Conflict("Recorded action types or project roles are ambiguous.");
            var roleReference = await canonical.CorrectiveActionRoleReferenceAsync(reader);
            var currentTypes = types.Where(CompanyPPDMRowAccessPolicy.Current).ToArray();
            var currentRoles = roles.Where(CompanyPPDMRowAccessPolicy.Current).ToArray();
            var evidence = new List<string> { Fingerprint(types), Fingerprint(roles), Snapshot(roleReference, true) };
            foreach (var role in currentRoles.OrderBy(row => row.BUSINESS_ASSOCIATE_ID, StringComparer.Ordinal))
            {
                var participant = await RequireParticipantAsync(review, projectId, "review", role.BUSINESS_ASSOCIATE_ID, validateAssignment: false);
                if (participant.Sequence != role.ROLE_SEQ_NO) throw RefusalException.Conflict("The project participant role changed.");
                evidence.Add(participant.Evidence);
            }
            await reader.RequireCurrentAsync();
            return (FingerprintRecords(evidence), currentTypes.Select(row => row.STEP_TYPE).Order(StringComparer.Ordinal).ToArray(), currentRoles.Select(row => row.BUSINESS_ASSOCIATE_ID).Order(StringComparer.Ordinal).ToArray());
        }
        var first = await Capture(); var second = await Capture();
        if (first.Evidence != second.Evidence || !first.Types.SequenceEqual(second.Types) || !first.Participants.SequenceEqual(second.Participants) ||
            await RequireAuthorityAsync(review, incidentId, projectId, actor) != authority)
            throw RefusalException.Conflict("The corrective-action choices changed during review.");
        var result = new CorrectiveActionOptions(review, incidentId, projectId, second.Evidence, second.Types, second.Participants); result.Require(); return result;
    }

    // Read-only reconciliation of the original three-row effect. Current vocabulary can change
    // after commit, so this checks recorded intent and current authority without creating anything.
    public async Task<string?> FindRecordedActionAsync(FieldSelectionReview review, string incidentId, string projectId, AddCARequest request, string actor)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.ActionId == Guid.Empty || !Identifier(incidentId) || !Identifier(projectId) || actor != review.ActorUserId ||
            string.IsNullOrWhiteSpace(request.CADescription) || request.CADescription.Length > 128 || request.CADescription.Any(char.IsControl) ||
            !Identifier(request.CAType) || !Identifier(request.ResponsibleBaId) || request.DueDate.Kind != DateTimeKind.Utc)
            throw RefusalException.Invalid("A complete original corrective-action intent is required.");
        var authority = await RequireAuthorityAsync(review, incidentId, projectId, actor);
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, actor);
        await reader.OpenAsync();
        await DefinitionAsync(metadata, typeof(PROJECT_STEP), ["PROJECT_ID", "STEP_ID"], ("PROJECT_ID", "PROJECT", "PROJECT_ID"), ("STEP_TYPE", "R_PROJ_STEP_TYPE", "STEP_TYPE"));
        await DefinitionAsync(metadata, typeof(PROJECT_STATUS), ["PROJECT_ID", "STATUS_ID"]);
        await DefinitionAsync(metadata, typeof(PROJECT_STEP_BA), ["PROJECT_ID", "BUSINESS_ASSOCIATE_ID", "ROLE", "ROLE_SEQ_NO", "STEP_ID", "STEP_BA_OBS_NO"]);
        var id = request.ActionId.ToString("N");
        async Task<(string Evidence, bool Found)> Capture()
        {
            var steps = (await reader.ReadAsync(typeof(PROJECT_STEP), [Equal("PROJECT_ID", projectId), Equal("STEP_ID", id)])).Cast<PROJECT_STEP>().ToArray();
            var statuses = (await reader.ReadAsync(typeof(PROJECT_STATUS), [Equal("PROJECT_ID", projectId), Equal("STATUS_ID", id)])).Cast<PROJECT_STATUS>().ToArray();
            var assignments = (await reader.ReadAsync(typeof(PROJECT_STEP_BA), [Equal("PROJECT_ID", projectId), Equal("STEP_ID", id)])).Cast<PROJECT_STEP_BA>().ToArray();
            var children = (await reader.ReadAsync(typeof(PROJECT_STATUS), [Equal("PROJECT_ID", projectId), Equal("STEP_ID", id)])).Cast<PROJECT_STATUS>().ToArray();
            if (steps.Length == 0 && statuses.Length == 0 && assignments.Length == 0 && children.Length == 0)
                return (FingerprintRecords([]), false);
            if (steps.Length != 1 || statuses.Length != 1 || assignments.Length is < 1 or > 256 || children.Length is < 1 or > 2 ||
                children.Count(row => Snapshot(statuses[0], true) == Snapshot(row, true)) != 1 ||
                children.Select(row => row.STATUS_ID).Distinct(StringComparer.Ordinal).Count() != children.Length)
                throw RefusalException.Conflict("The original action has incomplete or ambiguous recorded evidence.");
            var orderedAssignments = assignments.OrderBy(row => row.STEP_BA_OBS_NO).ToArray();
            if (orderedAssignments.Select(row => row.PPDM_GUID).Distinct(StringComparer.OrdinalIgnoreCase).Count() != orderedAssignments.Length)
                throw RefusalException.Conflict("The responsibility history contains duplicate assignment identities.");
            var step = steps[0]; var status = statuses[0]; var responsible = orderedAssignments[0];
            var completion = children.SingleOrDefault(row => row.STATUS_ID != id);
            bool Created(object row, bool unchanged)
            {
                var type = row.GetType();
                return type.GetProperty("SOURCE")!.GetValue(row) as string == Source && type.GetProperty("PPDM_GUID")!.GetValue(row) as string == request.ActionId.ToString().ToUpperInvariant() &&
                    type.GetProperty("ROW_CREATED_BY")!.GetValue(row) as string == actor &&
                    (unchanged ? type.GetProperty("ROW_CHANGED_BY")!.GetValue(row) as string == actor : Identifier(type.GetProperty("ROW_CHANGED_BY")!.GetValue(row) as string)) &&
                    type.GetProperty("ROW_CREATED_DATE")!.GetValue(row) is DateTime { Kind: DateTimeKind.Utc } created && created <= DateTime.UtcNow &&
                    type.GetProperty("ROW_CHANGED_DATE")!.GetValue(row) is DateTime { Kind: DateTimeKind.Utc } changed && changed >= created && changed <= DateTime.UtcNow;
            }
            if (step.ACTIVE_IND != "Y" || step.STEP_SEQ_NO <= 0 || decimal.Truncate(step.STEP_SEQ_NO) != step.STEP_SEQ_NO || step.STEP_SEQ_NO > int.MaxValue ||
                step.STEP_NAME != request.CADescription || step.DESCRIPTION != request.CADescription || step.STEP_TYPE != request.CAType ||
                step.DUE_DATE != request.DueDate || status.ACTIVE_IND != "Y" || status.STEP_ID != id ||
                status.STATUS_TYPE != CA.StatusType || status.STATUS != CA.Open || status.REMARK != request.CADescription || status.DEFINED_BY_BA_ID is not null ||
                responsible.ACTIVE_IND != (orderedAssignments.Length == 1 ? "Y" : "N") || responsible.ACTUAL_IND != "Y" || responsible.ROLE != CA.ResponsibleRole ||
                responsible.BUSINESS_ASSOCIATE_ID != request.ResponsibleBaId || !Observation(responsible.ROLE_SEQ_NO) || responsible.STEP_BA_OBS_NO != 1 ||
                step.EFFECTIVE_DATE is not DateTime { Kind: DateTimeKind.Utc } || step.EFFECTIVE_DATE != status.EFFECTIVE_DATE || step.EFFECTIVE_DATE != responsible.EFFECTIVE_DATE ||
                !Created(step, completion is null) || !Created(status, true) || !Created(responsible, orderedAssignments.Length == 1))
                throw RefusalException.Conflict("The original action does not match the complete recorded creation intent.");
            for (var i = 1; i < orderedAssignments.Length; i++)
            {
                var previous = orderedAssignments[i - 1]; var current = orderedAssignments[i];
                if (current.STEP_BA_OBS_NO != i + 1 || current.ROLE != CA.ResponsibleRole || current.ACTUAL_IND != "Y" ||
                    current.ACTIVE_IND != (i == orderedAssignments.Length - 1 ? "Y" : "N") || !Identifier(current.BUSINESS_ASSOCIATE_ID) ||
                    current.BUSINESS_ASSOCIATE_ID == previous.BUSINESS_ASSOCIATE_ID || !Observation(current.ROLE_SEQ_NO) || current.SOURCE != Source ||
                    !Guid.TryParse(current.PPDM_GUID, out var assignmentGuid) || assignmentGuid == Guid.Empty ||
                    !Identifier(current.ROW_CREATED_BY) || previous.ROW_CHANGED_BY != current.ROW_CREATED_BY || !Identifier(current.ROW_CHANGED_BY) ||
                    (i == orderedAssignments.Length - 1 && current.ROW_CHANGED_BY != current.ROW_CREATED_BY) ||
                    current.EFFECTIVE_DATE is not DateTime { Kind: DateTimeKind.Utc } effective || effective <= previous.EFFECTIVE_DATE || effective > DateTime.UtcNow ||
                    current.ROW_CREATED_DATE is not DateTime { Kind: DateTimeKind.Utc } created || current.ROW_CHANGED_DATE is not DateTime { Kind: DateTimeKind.Utc } changed ||
                    changed < created || changed > DateTime.UtcNow)
                    throw RefusalException.Conflict("The recorded responsibility history cannot be reconciled with its original assignment.");
            }
            if (completion is null)
            {
                if (step.REMARK != request.CADescription || step.ACTUAL_END_DATE is not null)
                    throw RefusalException.Conflict("The original open action has changed.");
            }
            else if (!Identifier(completion.STATUS_ID) || completion.ACTIVE_IND != "Y" || completion.STATUS_TYPE != CA.StatusType || completion.STATUS != CA.Completed ||
                completion.SOURCE != Source || !Guid.TryParse(completion.PPDM_GUID, out var completionGuid) || completionGuid == Guid.Empty ||
                !Identifier(completion.ROW_CREATED_BY) || completion.ROW_CHANGED_BY != completion.ROW_CREATED_BY || step.ROW_CHANGED_BY != completion.ROW_CREATED_BY ||
                completion.ROW_CREATED_DATE is not DateTime { Kind: DateTimeKind.Utc } completionCreated || completion.ROW_CHANGED_DATE is not DateTime { Kind: DateTimeKind.Utc } completionChanged ||
                completionChanged < completionCreated || completionChanged > DateTime.UtcNow || completion.EFFECTIVE_DATE is not DateTime { Kind: DateTimeKind.Utc } completionTime ||
                completionTime <= status.EFFECTIVE_DATE || completionTime > DateTime.UtcNow || step.ACTUAL_END_DATE != completionTime ||
                orderedAssignments.Any(row => row.EFFECTIVE_DATE > completionTime) ||
                string.IsNullOrWhiteSpace(completion.REMARK) || completion.REMARK != step.REMARK)
                throw RefusalException.Conflict("The recorded action completion cannot be reconciled with its original creation.");
            return (FingerprintRecords([Snapshot(step, true), Snapshot(status, true), InventorySnapshot(assignments), InventorySnapshot(children)]), true);
        }
        var first = await Capture();
        await reader.RequireCurrentAsync();
        var second = await Capture();
        await reader.RequireCurrentAsync();
        if (first != second || await RequireAuthorityAsync(review, incidentId, projectId, actor) != authority)
            throw RefusalException.Conflict("The corrective-action recovery evidence changed during review.");
        return second.Found ? id : null;
    }

    public async Task<string> AddCorrectiveActionAsync(FieldSelectionReview review, string incidentId, string projectId, AddCARequest request, string actor)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.ActionId == Guid.Empty || !Identifier(incidentId) || !Identifier(projectId) || actor != review.ActorUserId ||
            string.IsNullOrWhiteSpace(request.CADescription) || request.CADescription.Length > 128 || request.CADescription.Any(char.IsControl) ||
            !Identifier(request.CAType) || !Identifier(request.ResponsibleBaId) || request.DueDate.Kind != DateTimeKind.Utc)
            throw RefusalException.Invalid("A stable action ID, bounded description, recorded type/participant and explicit UTC due date are required.");
        var authority = await RequireAuthorityAsync(review, incidentId, projectId, actor);
        var stepId = request.ActionId.ToString("N");
        var participant = await RequireParticipantAsync(review, projectId, stepId, request.ResponsibleBaId, validateAssignment: false);
        var inventory = await ReadStepInventoryAsync(review, projectId);
        var inventoryEvidence = InventorySnapshot(inventory);
        var source = editor.GetDataSource(review.Database.ConnectionName) ?? throw new InvalidOperationException("The corrective-action command datasource is unavailable.");
        PROJECT_STEP? expectedStep = null; PROJECT_STATUS? expectedStatus = null; PROJECT_STEP_BA? expectedResponsible = null;
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            PPDMDataSourceTransaction.RequireOwned(source, System.Data.IsolationLevel.Serializable);
            await RequireContextAsync();
            var current = await ReadStepInventoryAsync(review, projectId);
            if (InventorySnapshot(current) != inventoryEvidence) throw RefusalException.Conflict("The complete action inventory changed before allocation.");
            var existing = current.SingleOrDefault(row => row.STEP_ID == stepId);
            if (existing is not null)
            {
                var saved = await ReadStateAsync(review, projectId, checked((int)existing.STEP_SEQ_NO));
                ValidateInterpretation(saved);
                if (saved.Step.ACTIVE_IND != "Y" || saved.Step.STEP_ID != stepId || saved.Step.STEP_NAME != request.CADescription || saved.Step.DESCRIPTION != request.CADescription ||
                    saved.Step.REMARK != request.CADescription || saved.Step.STEP_TYPE != request.CAType || saved.Step.DUE_DATE != request.DueDate || saved.Step.ACTUAL_END_DATE is not null ||
                    saved.Status.STATUS_ID != stepId || saved.Status.STATUS != CA.Open || saved.Status.REMARK != request.CADescription || saved.Status.DEFINED_BY_BA_ID is not null ||
                    saved.Responsible!.BUSINESS_ASSOCIATE_ID != request.ResponsibleBaId || saved.Responsible.ROLE_SEQ_NO != participant.Sequence || saved.Responsible.STEP_BA_OBS_NO != 1 ||
                    new object[] { saved.Step, saved.Status, saved.Responsible }.Any(row => !CreatedByIntent(row)))
                    throw RefusalException.Conflict("This action ID does not establish the same recorded creation intent; reconcile its evidence.");
                if (saved.Step.EFFECTIVE_DATE != saved.Status.EFFECTIVE_DATE || saved.Step.EFFECTIVE_DATE != saved.Responsible.EFFECTIVE_DATE)
                    throw RefusalException.Conflict("The recorded creation effect has inconsistent effective dates.");
                expectedStep = saved.Step; expectedStatus = saved.Status; expectedResponsible = saved.Responsible;
            }
            else
            {
                var keyReader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, actor);
                await keyReader.OpenAsync();
                if ((await ReadActionChildrenAsync(review, projectId, stepId)).Length != 0 ||
                    (await keyReader.ReadAsync(typeof(PROJECT_STATUS), [Equal("PROJECT_ID", projectId), Equal("STATUS_ID", stepId)])).Length != 0)
                    throw RefusalException.Conflict("The new action identity already has recorded child history.");
                var maximum = current.Length == 0 ? 0 : current.Max(row => row.STEP_SEQ_NO);
                if (maximum >= int.MaxValue) throw RefusalException.Conflict("The recorded action sequence exceeds the supported allocation range.");
                var now = DateTime.UtcNow; var guid = request.ActionId.ToString().ToUpperInvariant();
                expectedStep = new PROJECT_STEP { PROJECT_ID = projectId, STEP_ID = stepId, STEP_SEQ_NO = maximum + 1,
                    STEP_NAME = request.CADescription, DESCRIPTION = request.CADescription, REMARK = request.CADescription, STEP_TYPE = request.CAType, DUE_DATE = request.DueDate,
                    ACTIVE_IND = "Y", EFFECTIVE_DATE = now, ROW_EFFECTIVE_DATE = now, PPDM_GUID = guid, SOURCE = Source };
                expectedStatus = new PROJECT_STATUS { PROJECT_ID = projectId, STATUS_ID = stepId, STEP_ID = stepId, STATUS_TYPE = CA.StatusType, STATUS = CA.Open,
                    REMARK = request.CADescription, ACTIVE_IND = "Y", EFFECTIVE_DATE = now, ROW_EFFECTIVE_DATE = now, PPDM_GUID = guid, SOURCE = Source };
                expectedResponsible = new PROJECT_STEP_BA { PROJECT_ID = projectId, STEP_ID = stepId, BUSINESS_ASSOCIATE_ID = request.ResponsibleBaId,
                    ROLE = CA.ResponsibleRole, ROLE_SEQ_NO = participant.Sequence, STEP_BA_OBS_NO = 1, ACTIVE_IND = "Y", ACTUAL_IND = "Y",
                    EFFECTIVE_DATE = now, ROW_EFFECTIVE_DATE = now, PPDM_GUID = guid, SOURCE = Source };
                await RequirePreparedAsync();
                await Repository(review, typeof(PROJECT_STEP), new Mutation(this, review, incidentId, projectId, actor, source, authority, expectedStep, null, PPDMRowMutation.Insert, RequirePreparedAsync, preparedNode: true)).InsertAsync(expectedStep, actor);
                await Repository(review, typeof(PROJECT_STATUS), new Mutation(this, review, incidentId, projectId, actor, source, authority, expectedStatus, null, PPDMRowMutation.Insert, RequirePreparedAsync)).InsertAsync(expectedStatus, actor);
                await Repository(review, typeof(PROJECT_STEP_BA), new Mutation(this, review, incidentId, projectId, actor, source, authority, expectedResponsible, null, PPDMRowMutation.Insert, RequirePreparedAsync)).InsertAsync(expectedResponsible, actor);
            }
            await ConfirmAsync();
            PPDMDataSourceTransaction.RequireOwned(source, System.Data.IsolationLevel.Serializable);
        });
        await ConfirmAsync();
        return stepId;

        bool CreatedByIntent(object row)
        {
            var type = row.GetType();
            return type.GetProperty("SOURCE")!.GetValue(row) as string == Source && type.GetProperty("PPDM_GUID")!.GetValue(row) as string == request.ActionId.ToString().ToUpperInvariant() &&
                type.GetProperty("ROW_CREATED_BY")!.GetValue(row) as string == actor && type.GetProperty("ROW_CHANGED_BY")!.GetValue(row) as string == actor &&
                type.GetProperty("ROW_CREATED_DATE")!.GetValue(row) is DateTime { Kind: DateTimeKind.Utc } created && created <= DateTime.UtcNow &&
                type.GetProperty("ROW_CHANGED_DATE")!.GetValue(row) is DateTime { Kind: DateTimeKind.Utc } changed && changed >= created && changed <= DateTime.UtcNow;
        }
        async Task RequireContextAsync()
        {
            if (await RequireAuthorityAsync(review, incidentId, projectId, actor) != authority ||
                await RequireParticipantAsync(review, projectId, stepId, request.ResponsibleBaId, validateAssignment: false) != participant)
                throw RefusalException.Conflict("The action creation authority or participant evidence changed.");
        }
        async Task RequirePreparedAsync()
        {
            await RequireContextAsync();
            if (rowPolicy is not CompanyPPDMRowAccessPolicy canonical || expectedStep is null || expectedStatus is null || expectedResponsible is null)
                throw RefusalException.Conflict("The canonical prepared action reference policy is unavailable.");
            await canonical.ValidatePreparedCorrectiveActionAsync(expectedStep, expectedStatus, expectedResponsible);
            await RequireContextAsync();
        }
        async Task ConfirmAsync()
        {
            await RequireContextAsync();
            if (expectedStep is null || expectedStatus is null || expectedResponsible is null) throw RefusalException.Conflict("The action creation outcome is unconfirmed.");
            var saved = await ReadStateAsync(review, projectId, checked((int)expectedStep.STEP_SEQ_NO));
            ValidateInterpretation(saved);
            var expectedInventory = inventory.Where(row => row.STEP_ID != stepId).Append(expectedStep).ToArray();
            if (StateSnapshot(saved) != Snapshot(expectedStep, true) + Snapshot(expectedStatus, true) + Snapshot(expectedResponsible, true) ||
                InventorySnapshot(await ReadStepInventoryAsync(review, projectId)) != InventorySnapshot(expectedInventory) ||
                InventorySnapshot(await ReadActionChildrenAsync(review, projectId, stepId)) != InventorySnapshot(new object[] { expectedStatus, expectedResponsible }))
                throw RefusalException.Conflict("The complete recorded action creation effect is unconfirmed; reconcile before retrying.");
            await RequireContextAsync();
        }
    }
    private async Task<PROJECT_STEP[]> ReadStepInventoryAsync(FieldSelectionReview review, string project)
    {
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, review.ActorUserId);
        await reader.OpenAsync();
        await DefinitionAsync(metadata, typeof(PROJECT_STEP), ["PROJECT_ID", "STEP_ID"], ("PROJECT_ID", "PROJECT", "PROJECT_ID"), ("STEP_TYPE", "R_PROJ_STEP_TYPE", "STEP_TYPE"));
        var rows = (await reader.ReadAsync(typeof(PROJECT_STEP), [Equal("PROJECT_ID", project)])).Cast<PROJECT_STEP>().ToArray();
        if (rows.Any(row => !Identifier(row.STEP_ID) || row.STEP_SEQ_NO <= 0 || row.STEP_SEQ_NO > int.MaxValue || decimal.Truncate(row.STEP_SEQ_NO) != row.STEP_SEQ_NO || row.ACTIVE_IND is not ("Y" or "N")) ||
            rows.Select(row => row.STEP_ID).Distinct(StringComparer.Ordinal).Count() != rows.Length || rows.Select(row => row.STEP_SEQ_NO).Distinct().Count() != rows.Length)
            throw RefusalException.Conflict("The complete action inventory has invalid or ambiguous recorded keys/sequences.");
        await rowPolicy.ValidateRowsAsync(typeof(PROJECT_STEP), "PROJECT_STEP", rows.Where(row => row.ACTIVE_IND == "Y").Cast<object>().ToArray());
        return rows;
    }
    private async Task<object[]> ReadActionChildrenAsync(FieldSelectionReview review, string project, string step)
    {
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, review.ActorUserId);
        await reader.OpenAsync();
        return [.. await reader.ReadAsync(typeof(PROJECT_STATUS), [Equal("PROJECT_ID", project), Equal("STEP_ID", step)]),
            .. await reader.ReadAsync(typeof(PROJECT_STEP_BA), [Equal("PROJECT_ID", project), Equal("STEP_ID", step)])];
    }

    public async Task SetDueDateAsync(FieldSelectionReview review, string incidentId, string projectId, int stepSequence, DateTime dueDate, string actor)
    {
        if (dueDate.Kind != DateTimeKind.Utc) throw RefusalException.Invalid("An explicit UTC corrective-action due date is required.");
        await ExecuteAsync(review, incidentId, projectId, stepSequence, actor, dueDate, null);
    }
    public async Task RecordCompletionAsync(FieldSelectionReview review, string incidentId, string projectId, int stepSequence, string notes, string actor)
    {
        if (string.IsNullOrWhiteSpace(notes) || notes.Length > 2000 || notes.Any(character => char.IsControl(character) && character is not '\r' and not '\n' and not '\t'))
            throw RefusalException.Invalid("Explicit bounded corrective-action completion notes are required.");
        await ExecuteAsync(review, incidentId, projectId, stepSequence, actor, null, notes);
    }
    public async Task<CACompletionReview> ReviewCompletionAsync(FieldSelectionReview review, string incidentId, string projectId, int stepSequence, string actor)
    {
        if (!Identifier(incidentId) || !Identifier(projectId) || stepSequence <= 0 || actor != review.ActorUserId) throw RefusalException.Invalid("An exact action and actor are required.");
        var authority = await RequireAuthorityAsync(review, incidentId, projectId, actor);
        var first = await ReadStateAsync(review, projectId, stepSequence); ValidateInterpretation(first);
        var second = await ReadStateAsync(review, projectId, stepSequence); ValidateInterpretation(second);
        if (first.Status.STATUS != CA.Open || StateSnapshot(first) != StateSnapshot(second) || await RequireAuthorityAsync(review, incidentId, projectId, actor) != authority)
            throw RefusalException.Conflict("The open corrective action changed during review.");
        return new(review, incidentId, projectId, stepSequence, first.Step.STEP_ID, first.Step.DESCRIPTION ?? first.Step.STEP_NAME,
            first.Responsible!.BUSINESS_ASSOCIATE_ID, first.Step.DUE_DATE!.Value, FingerprintRecords([StateSnapshot(first)]));
    }
    public async Task<CAVerificationReview> ReviewVerificationAsync(FieldSelectionReview review, string incidentId, string projectId, string actor)
    {
        if (!Identifier(incidentId) || !Identifier(projectId) || actor != review.ActorUserId) throw RefusalException.Invalid("An exact incident, plan and actor are required.");
        var authority = await RequireAuthorityAsync(review, incidentId, projectId, actor);
        async Task<(string Revision, CAVerificationAction[] Actions)> Capture()
        {
            var inventory = await ReadStepInventoryAsync(review, projectId);
            var active = inventory.Where(row => row.ACTIVE_IND == "Y").OrderBy(row => row.STEP_SEQ_NO).ToArray();
            if (active.Length > 256) throw RefusalException.Conflict("The corrective-action population exceeds the supported review size.");
            var states = new List<State>();
            foreach (var step in active)
            {
                var state = await ReadStateAsync(review, projectId, checked((int)step.STEP_SEQ_NO)); ValidateInterpretation(state);
                if (state.Step.STEP_ID != step.STEP_ID || Snapshot(state.Step, true) != Snapshot(step, true) || state.Status.STATUS is not (CA.Open or CA.Completed))
                    throw RefusalException.Conflict("The corrective-action population changed during verification review.");
                if (state.Status.STATUS == CA.Completed && (state.Status.SOURCE != Source || !Identifier(state.Status.ROW_CREATED_BY) ||
                    state.Status.ROW_CHANGED_BY != state.Status.ROW_CREATED_BY || state.Step.ROW_CHANGED_BY != state.Status.ROW_CREATED_BY ||
                    state.Status.ROW_CREATED_DATE is not DateTime { Kind: DateTimeKind.Utc } created || state.Status.ROW_CHANGED_DATE is not DateTime { Kind: DateTimeKind.Utc } changed ||
                    changed < created || changed > DateTime.UtcNow || state.Step.ACTUAL_END_DATE != state.Status.EFFECTIVE_DATE || string.IsNullOrWhiteSpace(state.Status.REMARK) || state.Step.REMARK != state.Status.REMARK))
                    throw RefusalException.Conflict("The recorded completion evidence or author is unconfirmed.");
                states.Add(state);
            }
            var actions = states.Select(state => new CAVerificationAction(checked((int)state.Step.STEP_SEQ_NO), state.Step.STEP_ID,
                state.Step.DESCRIPTION ?? state.Step.STEP_NAME, state.Status.STATUS, state.Responsible!.BUSINESS_ASSOCIATE_ID, state.Step.DUE_DATE!.Value,
                state.Status.STATUS == CA.Completed ? state.Step.ACTUAL_END_DATE : null,
                state.Status.STATUS == CA.Completed ? state.Status.ROW_CREATED_BY : null,
                state.Status.STATUS == CA.Completed ? state.Status.REMARK : null)).ToArray();
            return (FingerprintRecords([InventorySnapshot(inventory), .. states.Select(StateSnapshot)]), actions);
        }
        var first = await Capture(); var second = await Capture();
        if (first.Revision != second.Revision || !first.Actions.SequenceEqual(second.Actions) || await RequireAuthorityAsync(review, incidentId, projectId, actor) != authority)
            throw RefusalException.Conflict("The corrective-action verification evidence changed during review.");
        var result = new CAVerificationReview(review, incidentId, projectId, second.Revision, second.Actions); result.Require(); return result;
    }
    public async Task RecordReviewedCompletionAsync(CACompletionReview review, string notes, string actor)
    {
        if (review is null || review.Context is null || !Identifier(review.StepId) || !ProcessDefinitionRevision.IsValid(review.Revision) ||
            string.IsNullOrWhiteSpace(notes) || notes.Length > 2000 || notes.Any(character => char.IsControl(character) && character is not '\r' and not '\n' and not '\t'))
            throw RefusalException.Invalid("The reviewed action and completion evidence are required.");
        await ExecuteAsync(review.Context, review.IncidentId, review.PlanId, review.StepSequence, actor, null, notes, review);
    }
    public async Task AssignResponsiblePersonAsync(FieldSelectionReview review, string incidentId, string projectId, int stepSequence, string businessAssociateId, string actor)
    {
        if (!Identifier(incidentId) || !Identifier(projectId) || !Identifier(businessAssociateId) || stepSequence <= 0 || actor != review.ActorUserId)
            throw RefusalException.Invalid("An exact incident/project/participant, positive action sequence and authenticated actor are required.");
        var authority = await RequireAuthorityAsync(review, incidentId, projectId, actor);
        var initial = await ReadStateAsync(review, projectId, stepSequence, requireResponsible: false);
        ValidateInterpretation(initial, requireDueDate: false);
        if (initial.Status.STATUS != CA.Open) throw RefusalException.Conflict("Only a recorded open corrective action can assign a responsible participant.");
        var initialState = StateSnapshot(initial);
        var inventory = await ReadAssignmentInventoryAsync(review, projectId, initial.Step.STEP_ID);
        var inventorySnapshot = InventorySnapshot(inventory);
        var target = await RequireParticipantAsync(review, projectId, initial.Step.STEP_ID, businessAssociateId);
        var source = editor.GetDataSource(review.Database.ConnectionName) ?? throw new InvalidOperationException("The corrective-action command datasource is unavailable.");
        PROJECT_STEP_BA? retired = null; PROJECT_STEP_BA? assigned = null;
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            PPDMDataSourceTransaction.RequireOwned(source, System.Data.IsolationLevel.Serializable);
            await RequireContextAsync();
            var current = await ReadStateAsync(review, projectId, stepSequence, requireResponsible: false);
            if (StateSnapshot(current) != initialState || InventorySnapshot(await ReadAssignmentInventoryAsync(review, projectId, current.Step.STEP_ID)) != inventorySnapshot)
                throw RefusalException.Conflict("The complete corrective-action assignment evidence changed before the command.");
            if (current.Responsible?.BUSINESS_ASSOCIATE_ID == businessAssociateId && current.Responsible.ROLE_SEQ_NO == target.Sequence)
                assigned = current.Responsible;
            else
            {
                var maximum = inventory.Select(row => row.STEP_BA_OBS_NO).DefaultIfEmpty(0).Max();
                if (maximum >= int.MaxValue) throw RefusalException.Conflict("The recorded assignment observation exceeds the supported allocation range.");
                var now = DateTime.UtcNow;
                assigned = new PROJECT_STEP_BA { PROJECT_ID = projectId, STEP_ID = current.Step.STEP_ID, BUSINESS_ASSOCIATE_ID = businessAssociateId,
                    ROLE = CA.ResponsibleRole, ROLE_SEQ_NO = target.Sequence, STEP_BA_OBS_NO = maximum + 1, ACTIVE_IND = "Y", ACTUAL_IND = "Y",
                    EFFECTIVE_DATE = now, ROW_EFFECTIVE_DATE = now, PPDM_GUID = Guid.NewGuid().ToString().ToUpperInvariant(), SOURCE = Source };
                await rowPolicy.ValidateRowsAsync(typeof(PROJECT_STEP_BA), "PROJECT_STEP_BA", [assigned]);
                if (current.Responsible is not null)
                {
                    retired = Copy(current.Responsible); retired.ACTIVE_IND = "N";
                    await Repository(review, typeof(PROJECT_STEP_BA), new Mutation(this, review, incidentId, projectId, actor, source, authority, retired, current.Responsible, PPDMRowMutation.Update, RequireContextAsync)).UpdateAsync(retired, actor);
                }
                await Repository(review, typeof(PROJECT_STEP_BA), new Mutation(this, review, incidentId, projectId, actor, source, authority, assigned, null, PPDMRowMutation.Insert, RequireContextAsync)).InsertAsync(assigned, actor);
            }
            await ConfirmAssignmentAsync();
            PPDMDataSourceTransaction.RequireOwned(source, System.Data.IsolationLevel.Serializable);
        });
        await ConfirmAssignmentAsync();

        async Task RequireContextAsync()
        {
            if (await RequireAuthorityAsync(review, incidentId, projectId, actor) != authority || await RequireParticipantAsync(review, projectId, initial.Step.STEP_ID, businessAssociateId) != target)
                throw RefusalException.Conflict("The incident/project or requested participant-role evidence changed during assignment.");
            var action = await ReadStateAsync(review, projectId, stepSequence, requireResponsible: false);
            if (Snapshot(action.Step, true) != Snapshot(initial.Step, true) || Snapshot(action.Status, true) != Snapshot(initial.Status, true))
                throw RefusalException.Conflict("The action/status evidence changed during assignment.");
        }
        async Task ConfirmAssignmentAsync()
        {
            await RequireContextAsync();
            var current = await ReadStateAsync(review, projectId, stepSequence);
            var expected = inventory.Where(row => retired is null || AssignmentKey(row) != AssignmentKey(retired)).ToList();
            if (retired is not null) expected.Add(retired);
            if (assigned is null) throw RefusalException.Conflict("The assignment command outcome is unconfirmed.");
            if (!expected.Any(row => AssignmentKey(row) == AssignmentKey(assigned))) expected.Add(assigned);
            if (Snapshot(current.Responsible, true) != Snapshot(assigned, true) || InventorySnapshot(await ReadAssignmentInventoryAsync(review, projectId, initial.Step.STEP_ID)) != InventorySnapshot(expected))
                throw RefusalException.Conflict("The complete recorded assignment outcome is unconfirmed; reconcile before retrying.");
            await RequireContextAsync();
        }
    }
    private async Task<(decimal Sequence, string Evidence)> RequireParticipantAsync(FieldSelectionReview review, string project, string step, string associate, bool validateAssignment = true)
    {
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, review.ActorUserId);
        await reader.OpenAsync();
        await DefinitionAsync(metadata, typeof(PROJECT_BA_ROLE), ["PROJECT_ID", "BUSINESS_ASSOCIATE_ID", "ROLE", "ROLE_SEQ_NO"], ("PROJECT_ID", "PROJECT_BA", "PROJECT_ID"), ("ROLE", "R_PROJECT_BA_ROLE", "ROLE"));
        await DefinitionAsync(metadata, typeof(PROJECT_BA), ["PROJECT_ID", "BUSINESS_ASSOCIATE_ID"], ("PROJECT_ID", "PROJECT", "PROJECT_ID"), ("BUSINESS_ASSOCIATE_ID", "BUSINESS_ASSOCIATE", "BUSINESS_ASSOCIATE_ID"));
        await DefinitionAsync(metadata, typeof(BUSINESS_ASSOCIATE), ["BUSINESS_ASSOCIATE_ID"]);
        var roles = (await reader.ReadAsync(typeof(PROJECT_BA_ROLE), [Equal("PROJECT_ID", project), Equal("BUSINESS_ASSOCIATE_ID", associate), Equal("ROLE", CA.ResponsibleRole), Equal("ACTIVE_IND", "Y")])).Cast<PROJECT_BA_ROLE>().ToArray();
        var members = await reader.ReadAsync(typeof(PROJECT_BA), [Equal("PROJECT_ID", project), Equal("BUSINESS_ASSOCIATE_ID", associate), Equal("ACTIVE_IND", "Y")]);
        var associates = await reader.ReadAsync(typeof(BUSINESS_ASSOCIATE), [Equal("BUSINESS_ASSOCIATE_ID", associate), Equal("ACTIVE_IND", "Y")]);
        if (roles.Length != 1 || !Observation(roles[0].ROLE_SEQ_NO) || members.Length != 1 || associates.Length != 1 ||
            !CompanyPPDMRowAccessPolicy.Current(roles[0]) || !CompanyPPDMRowAccessPolicy.Current(members[0]) || !CompanyPPDMRowAccessPolicy.Current(associates[0]))
            throw RefusalException.Conflict("One recorded current project participant and responsible role sequence are required.");
        var target = (roles[0].ROLE_SEQ_NO, Snapshot(roles[0], true) + Snapshot(members[0], true) + Snapshot(associates[0], true));
        // Existing row policy proves the approved role, actual BA/member/window and exact action relationship.
        if (validateAssignment) await rowPolicy.ValidateRowsAsync(typeof(PROJECT_STEP_BA), "PROJECT_STEP_BA", [new PROJECT_STEP_BA { PROJECT_ID = project, STEP_ID = step, BUSINESS_ASSOCIATE_ID = associate, ROLE = CA.ResponsibleRole, ROLE_SEQ_NO = target.Item1, STEP_BA_OBS_NO = 1, ACTIVE_IND = "Y" }]);
        var freshRoles = await reader.ReadAsync(typeof(PROJECT_BA_ROLE), [Equal("PROJECT_ID", project), Equal("BUSINESS_ASSOCIATE_ID", associate), Equal("ROLE", CA.ResponsibleRole), Equal("ACTIVE_IND", "Y")]);
        var freshMembers = await reader.ReadAsync(typeof(PROJECT_BA), [Equal("PROJECT_ID", project), Equal("BUSINESS_ASSOCIATE_ID", associate), Equal("ACTIVE_IND", "Y")]);
        var freshAssociates = await reader.ReadAsync(typeof(BUSINESS_ASSOCIATE), [Equal("BUSINESS_ASSOCIATE_ID", associate), Equal("ACTIVE_IND", "Y")]);
        if (freshRoles.Length != 1 || freshMembers.Length != 1 || freshAssociates.Length != 1 || Snapshot(freshRoles[0], true) + Snapshot(freshMembers[0], true) + Snapshot(freshAssociates[0], true) != target.Item2)
            throw RefusalException.Conflict("The requested participant-role evidence changed during validation.");
        await reader.RequireCurrentAsync();
        return target;
    }
    private async Task<PROJECT_STEP_BA[]> ReadAssignmentInventoryAsync(FieldSelectionReview review, string project, string step)
    {
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, review.ActorUserId);
        await reader.OpenAsync();
        await DefinitionAsync(metadata, typeof(PROJECT_STEP_BA), ["PROJECT_ID", "BUSINESS_ASSOCIATE_ID", "ROLE", "ROLE_SEQ_NO", "STEP_ID", "STEP_BA_OBS_NO"], ("PROJECT_ID", "PROJECT_STEP", "PROJECT_ID"), ("PROJECT_ID", "PROJECT_BA_ROLE", "PROJECT_ID"));
        var rows = (await reader.ReadAsync(typeof(PROJECT_STEP_BA), [Equal("PROJECT_ID", project), Equal("STEP_ID", step), Equal("ROLE", CA.ResponsibleRole)])).Cast<PROJECT_STEP_BA>().ToArray();
        if (rows.Any(row => !Identifier(row.BUSINESS_ASSOCIATE_ID) || !Observation(row.ROLE_SEQ_NO) || !Observation(row.STEP_BA_OBS_NO) || row.ACTIVE_IND is not ("Y" or "N")) || rows.Select(AssignmentKey).Distinct(StringComparer.Ordinal).Count() != rows.Length)
            throw RefusalException.Conflict("Recorded assignment history requires complete unique identities and explicit activity.");
        await rowPolicy.ValidateRowsAsync(typeof(PROJECT_STEP_BA), "PROJECT_STEP_BA", rows.Where(row => row.ACTIVE_IND == "Y").ToArray());
        return rows;
    }
    private static string AssignmentKey(PROJECT_STEP_BA row) => JsonSerializer.Serialize(new { row.PROJECT_ID, row.BUSINESS_ASSOCIATE_ID, row.ROLE, row.ROLE_SEQ_NO, row.STEP_ID, row.STEP_BA_OBS_NO });
    private static string InventorySnapshot(IEnumerable<object> rows) => JsonSerializer.Serialize(rows.Select(row => Snapshot(row, true)).Order(StringComparer.Ordinal));
    private async Task ExecuteAsync(FieldSelectionReview review, string incidentId, string projectId, int sequence, string actor, DateTime? dueDate, string? notes, CACompletionReview? expectedReview = null)
    {
        if (!Identifier(incidentId) || !Identifier(projectId) || sequence <= 0 || actor != review.ActorUserId)
            throw RefusalException.Invalid("An exact incident/project, positive action sequence and authenticated actor are required.");
        var initialAuthority = await RequireAuthorityAsync(review, incidentId, projectId, actor);
        var initial = await ReadStateAsync(review, projectId, sequence);
        var initialState = StateSnapshot(initial);
        if (expectedReview is not null && (initial.Step.STEP_ID != expectedReview.StepId ||
            (initial.Step.DESCRIPTION ?? initial.Step.STEP_NAME) != expectedReview.Description || initial.Step.DUE_DATE != expectedReview.DueDate ||
            initial.Responsible?.BUSINESS_ASSOCIATE_ID != expectedReview.ResponsiblePerson ||
            initial.Status.STATUS != CA.Completed && FingerprintRecords([initialState]) != expectedReview.Revision))
            throw RefusalException.Conflict("The reviewed corrective action changed before completion.");
        var source = editor.GetDataSource(review.Database.ConnectionName) ?? throw new InvalidOperationException("The corrective-action command datasource is unavailable.");
        PROJECT_STEP? expectedStep = null; PROJECT_STATUS? expectedStatus = null;
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            PPDMDataSourceTransaction.RequireOwned(source, System.Data.IsolationLevel.Serializable);
            if (await RequireAuthorityAsync(review, incidentId, projectId, actor) != initialAuthority) throw RefusalException.Conflict("The incident/project relationship changed before the command.");
            var current = await ReadStateAsync(review, projectId, sequence);
            if (StateSnapshot(current) != initialState) throw RefusalException.Conflict("Corrective-action evidence changed before the owned command.");
            ValidateInterpretation(current, requireDueDate: dueDate is null);
            if (dueDate is not null)
            {
                if (current.Status.STATUS != CA.Open) throw RefusalException.Conflict("Only an explicitly open corrective action can change its due date.");
                expectedStep = Copy(current.Step);
                expectedStep.DUE_DATE = dueDate;
                expectedStatus = current.Status;
                if (current.Step.DUE_DATE != dueDate)
                    await Repository(review, typeof(PROJECT_STEP), new Mutation(this, review, incidentId, projectId, actor, source, initialAuthority, expectedStep, current.Step, PPDMRowMutation.Update)).UpdateAsync(expectedStep, actor);
            }
            else if (current.Status.STATUS == CA.Completed)
            {
                if (current.Step.REMARK != notes || current.Status.REMARK != notes || current.Step.ROW_CHANGED_BY != actor || current.Status.ROW_CREATED_BY != actor || current.Status.SOURCE != Source)
                    throw RefusalException.Conflict("This completed action does not establish the same command; reconcile its recorded evidence.");
                expectedStep = current.Step; expectedStatus = current.Status;
            }
            else
            {
                if (current.Status.STATUS != CA.Open) throw RefusalException.Conflict("The recorded action status does not permit completion.");
                var completionNotes = notes ?? throw RefusalException.Invalid("Completion notes are required.");
                var now = DateTime.UtcNow;
                if (current.Status.EFFECTIVE_DATE >= now) throw RefusalException.Conflict("A completion must follow its recorded open state.");
                expectedStep = Copy(current.Step); expectedStep.ACTUAL_END_DATE = now; expectedStep.REMARK = completionNotes;
                expectedStatus = new PROJECT_STATUS { PROJECT_ID = projectId, STATUS_ID = Guid.NewGuid().ToString("N"), STEP_ID = current.Step.STEP_ID,
                    STATUS_TYPE = CA.StatusType, STATUS = CA.Completed, PPDM_GUID = Guid.NewGuid().ToString().ToUpperInvariant(), ROW_EFFECTIVE_DATE = now, EFFECTIVE_DATE = now, ACTIVE_IND = "Y", REMARK = completionNotes, SOURCE = Source };
                // Validate the actual reference code before the first write; the existing row policy proves its type/step relationships.
                await rowPolicy.ValidateRowsAsync(typeof(PROJECT_STATUS), "PROJECT_STATUS", [expectedStatus]);
                await Repository(review, typeof(PROJECT_STEP), new Mutation(this, review, incidentId, projectId, actor, source, initialAuthority, expectedStep, current.Step, PPDMRowMutation.Update)).UpdateAsync(expectedStep, actor);
                await Repository(review, typeof(PROJECT_STATUS), new Mutation(this, review, incidentId, projectId, actor, source, initialAuthority, expectedStatus, null, PPDMRowMutation.Insert)).InsertAsync(expectedStatus, actor);
            }
            await ConfirmAsync();
            await RequireAuthorityAsync(review, incidentId, projectId, actor);
            PPDMDataSourceTransaction.RequireOwned(source, System.Data.IsolationLevel.Serializable);
        });
        // A provider commit acknowledgment does not establish durable command success.
        await ConfirmAsync();
        await RequireAuthorityAsync(review, incidentId, projectId, actor);

        async Task ConfirmAsync()
        {
            if (await RequireAuthorityAsync(review, incidentId, projectId, actor) != initialAuthority) throw RefusalException.Conflict("The incident/project relationship changed during command confirmation.");
            var saved = await ReadStateAsync(review, projectId, sequence);
            ValidateInterpretation(saved);
            if (expectedStep is null || expectedStatus is null || Snapshot(saved.Step, true) != Snapshot(expectedStep, true) || Snapshot(saved.Status, true) != Snapshot(expectedStatus, true) || Snapshot(saved.Responsible, true) != Snapshot(initial.Responsible, true))
                throw RefusalException.Conflict("The recorded corrective-action command outcome is unconfirmed; reconcile before retrying.");
            if (saved.Step.ROW_CHANGED_BY != actor && dueDate != initial.Step.DUE_DATE || notes is not null && saved.Status.ROW_CREATED_BY != actor)
                throw RefusalException.Conflict("The corrective-action command audit actor is unconfirmed.");
        }
    }
    private PPDMGenericRepository Repository(FieldSelectionReview review, Type type, IPPDMRowAccessPolicy? mutation = null)
        => new(editor, columns, defaults, metadata, type, review.Database.ConnectionName, type.Name, rowAccess: mutation ?? rowPolicy);

    private async Task<string> RequireAuthorityAsync(FieldSelectionReview review, string incidentId, string projectId, string actor)
    {
        FieldSelectionReviewValidator.RequireReview(review, review.Company, actor);
        if (selections is not ICompanyFieldSelectionAuthority preferences) throw RefusalException.Conflict("The fixed stored FIELD preference owner is required.");
        await preferences.RequireStoredSelectionAsync(review);
        if (string.IsNullOrEmpty(review.FieldId) || await company.ResolveAsync() != review.Database || company.Company.Scope != review.Company || company.Company.UserId != actor)
            throw RefusalException.Conflict("The corrective-action account/company/database/FIELD review changed.");
        var authority = await company.ResolveSubjectAsync(actor);
        if (authority?.Permissions.Contains(PermissionConstants.HSE.ManageIncidents, StringComparer.Ordinal) != true)
            throw RefusalException.Forbidden("Current company HSE.ManageIncidents permission is required for corrective-action changes.");
        var grants = await assets.ReadAsync(actor, review.Database.Fingerprint);
        var grantSnapshots = grants.Where(grant => grant.IsActive && grant.Inherit && grant.AssetType == "FIELD" && grant.AssetId == review.FieldId && PPDMCompanyFieldEligibility.Rank(grant.AccessLevel) >= PPDMCompanyFieldEligibility.Rank("WRITE")).Select(GrantSnapshot).ToHashSet(StringComparer.Ordinal);
        if (grantSnapshots.Count == 0)
            throw RefusalException.Forbidden("An inherited current FIELD write grant is required for corrective-action changes.");
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, actor);
        await reader.OpenAsync();
        await DefinitionAsync(metadata, typeof(FIELD), ["FIELD_ID"]);
        var fields = await reader.ReadAsync(typeof(FIELD), [Equal("FIELD_ID", review.FieldId), Equal("ACTIVE_IND", "Y")]);
        var fieldEvidence = Fingerprint(fields);
        if (fields.Length != 1 || PPDMCompanyFieldEligibility.Rank(await rowPolicy.GetEffectiveAccessLevelAsync(typeof(FIELD), "FIELD", fields[0])) < PPDMCompanyFieldEligibility.Rank("WRITE"))
            throw RefusalException.Forbidden("Current company FIELD entitlement must permit corrective-action writes.");
        await DefinitionAsync(metadata, typeof(HSE_INCIDENT_RESPONSE), ["INCIDENT_ID", "RESPONSE_OBS_NO"], ("INCIDENT_ID", "HSE_INCIDENT", "INCIDENT_ID"), ("PROJECT_ID", "PROJECT", "PROJECT_ID"), ("RESPONSE_TYPE", "R_HSE_RESPONSE_TYPE", "ACTION_TYPE"));
        var links = (await reader.ReadAsync(typeof(HSE_INCIDENT_RESPONSE), [Equal("PROJECT_ID", projectId), Equal("ACTIVE_IND", "Y")])).Cast<HSE_INCIDENT_RESPONSE>().ToArray();
        if (links.Length != 1 || links[0].INCIDENT_ID != incidentId) throw RefusalException.Conflict("The command requires the same unique recorded incident/project relationship.");
        await rowPolicy.ValidateRowsAsync(typeof(HSE_INCIDENT_RESPONSE), "HSE_INCIDENT_RESPONSE", links);
        var components = (await reader.ReadAsync(typeof(HSE_INCIDENT_COMPONENT), [Equal("INCIDENT_ID", incidentId), Equal("COMPONENT_ROLE", HseReferenceCodes.FieldContext.PrimaryRole), Equal("ACTIVE_IND", "Y")])).Cast<HSE_INCIDENT_COMPONENT>().ToArray();
        await rowPolicy.ValidateRowsAsync(typeof(HSE_INCIDENT_COMPONENT), "HSE_INCIDENT_COMPONENT", components);
        if (components.Length != 1 || components[0].FIELD_ID != review.FieldId) throw RefusalException.Forbidden("The incident is outside the command's exact reviewed FIELD.");
        var projects = (await reader.ReadAsync(typeof(PROJECT), [Equal("PROJECT_ID", projectId), Equal("PROJECT_TYPE", CA.ProjectType), Equal("ACTIVE_IND", "Y")])).ToArray();
        await rowPolicy.ValidateRowsAsync(typeof(PROJECT), "PROJECT", projects);
        if (projects.Length != 1) throw RefusalException.Conflict("The canonical linked corrective-action project is unavailable.");
        // Re-read the authoritative app grant/permission after relationship evidence.
        var fresh = await assets.ReadAsync(actor, review.Database.Fingerprint);
        if (!fresh.Any(after => grantSnapshots.Contains(GrantSnapshot(after))))
            throw RefusalException.Forbidden("FIELD command authority changed during evidence review.");
        var closingFields = await reader.ReadAsync(typeof(FIELD), [Equal("FIELD_ID", review.FieldId!), Equal("ACTIVE_IND", "Y")]);
        if (closingFields.Length != 1 || Fingerprint(closingFields) != fieldEvidence || PPDMCompanyFieldEligibility.Rank(await rowPolicy.GetEffectiveAccessLevelAsync(typeof(FIELD), "FIELD", closingFields[0])) < PPDMCompanyFieldEligibility.Rank("WRITE"))
            throw RefusalException.Forbidden("FIELD data or entitlement changed during command evidence review.");
        if ((await company.ResolveSubjectAsync(actor))?.Permissions.Contains(PermissionConstants.HSE.ManageIncidents, StringComparer.Ordinal) != true)
            throw RefusalException.Forbidden("HSE command permission was revoked during evidence review.");
        await reader.RequireCurrentAsync();
        await preferences.RequireStoredSelectionAsync(review);
        return fieldEvidence + Snapshot(links[0], true) + Snapshot(components[0], true) + Snapshot(projects[0], true);
    }
    private async Task<State> ReadStateAsync(FieldSelectionReview review, string projectId, int sequence, bool requireResponsible = true)
    {
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, review.ActorUserId);
        await reader.OpenAsync();
        var steps = (await reader.ReadAsync(typeof(PROJECT_STEP), [Equal("PROJECT_ID", projectId), Equal("STEP_SEQ_NO", sequence.ToString(System.Globalization.CultureInfo.InvariantCulture)), Equal("ACTIVE_IND", "Y")])).Cast<PROJECT_STEP>().ToArray();
        await rowPolicy.ValidateRowsAsync(typeof(PROJECT_STEP), "PROJECT_STEP", steps);
        if (steps.Length == 0) throw RefusalException.NotFound("The recorded corrective action is unavailable.");
        if (steps.Length != 1 || !Identifier(steps[0].STEP_ID) || string.IsNullOrWhiteSpace(steps[0].STEP_NAME)) throw RefusalException.Conflict("The corrective-action sequence is incomplete or ambiguous.");
        var statuses = (await reader.ReadAsync(typeof(PROJECT_STATUS), [Equal("PROJECT_ID", projectId), Equal("STEP_ID", steps[0].STEP_ID), Equal("STATUS_TYPE", CA.StatusType), Equal("ACTIVE_IND", "Y")])).Cast<PROJECT_STATUS>().ToArray();
        await rowPolicy.ValidateRowsAsync(typeof(PROJECT_STATUS), "PROJECT_STATUS", statuses);
        if (statuses.Length == 0 || statuses.Any(row => row.EFFECTIVE_DATE is not { Kind: DateTimeKind.Utc })) throw RefusalException.Conflict("Recorded UTC corrective-action status evidence is required.");
        var latest = statuses.Where(row => row.EFFECTIVE_DATE == statuses.Max(value => value.EFFECTIVE_DATE)).ToArray();
        if (latest.Length != 1) throw RefusalException.Conflict("The latest corrective-action status is ambiguous.");
        var responsible = (await reader.ReadAsync(typeof(PROJECT_STEP_BA), [Equal("PROJECT_ID", projectId), Equal("STEP_ID", steps[0].STEP_ID), Equal("ROLE", CA.ResponsibleRole), Equal("ACTIVE_IND", "Y")])).Cast<PROJECT_STEP_BA>().ToArray();
        await rowPolicy.ValidateRowsAsync(typeof(PROJECT_STEP_BA), "PROJECT_STEP_BA", responsible);
        if (responsible.Length > 1 || requireResponsible && responsible.Length != 1) throw RefusalException.Conflict("One recorded responsible project participant is required.");
        return new(steps[0], latest[0], responsible.SingleOrDefault());
    }
    private static void ValidateInterpretation(State state, bool requireDueDate = true)
    {
        if (requireDueDate && state.Step.DUE_DATE is not { Kind: DateTimeKind.Utc } || state.Status.EFFECTIVE_DATE is not { Kind: DateTimeKind.Utc } ||
            state.Status.STATUS == CA.Completed && (state.Step.ACTUAL_END_DATE is not { Kind: DateTimeKind.Utc } || state.Step.ACTUAL_END_DATE > state.Status.EFFECTIVE_DATE || state.Step.ACTUAL_END_DATE > DateTime.UtcNow) ||
            state.Status.STATUS != CA.Completed && state.Step.ACTUAL_END_DATE is not null)
            throw RefusalException.Conflict("The recorded corrective-action due/status/completion evidence is inconsistent.");
    }
    private static string GrantSnapshot(AppUserAssetAccess grant) => JsonSerializer.Serialize(new { grant.Id, grant.UserId, grant.DatabaseScope, grant.AssetType, grant.AssetId, grant.FacilityType, grant.OrganizationId, grant.AccessLevel, grant.Inherit, grant.IsActive, grant.ConcurrencyStamp });
    private static PropertyInfo[] Properties(Type type) => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(property => property.CanRead && property.CanWrite).OrderBy(property => property.Name, StringComparer.Ordinal).ToArray();
    private static T Copy<T>(T row) where T : new()
    { var result = new T(); foreach (var property in Properties(typeof(T))) property.SetValue(result, property.GetValue(row)); return result; }
    private static string Snapshot(object? row, bool audit = false) => row is null ? "null" : JsonSerializer.Serialize(Properties(row.GetType()).Where(property => audit || property.Name is not ("ROW_CHANGED_BY" or "ROW_CHANGED_DATE" or "ROW_CREATED_BY" or "ROW_CREATED_DATE")).Select(property => new object?[] { property.Name, property.GetValue(row) }));
    private static string StateSnapshot(State state) => Snapshot(state.Step, true) + Snapshot(state.Status, true) + Snapshot(state.Responsible, true);

    // One-use exact mutation; generic HSE writes retain their existing fence.
    private sealed class Mutation(PPDMCompanyCorrectiveActionCommands access, FieldSelectionReview review, string incident, string project, string actor, IDataSource source, string authoritySnapshot, object expected, object? original, PPDMRowMutation operation, Func<Task>? requireEvidence = null, bool preparedNode = false, bool preparedPlan = false, Func<Task<string>>? authorityReader = null) : IPPDMRowAccessPolicy
    {
        private readonly string expectedSnapshot = Snapshot(expected);
        private readonly string? originalSnapshot = original is null ? null : Snapshot(original, true);
        private int used;
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type, string table)
        {
            PPDMDataSourceTransaction.RequireOwned(source, System.Data.IsolationLevel.Serializable);
            if (type != expected.GetType() || table != type.Name) throw RefusalException.Forbidden("A command reads only its prepared canonical row.");
            var keys = await access.Metadata.GetPrimaryKeyColumnsAsync(table);
            var predicates = keys.Select(key => Equal(key, Convert.ToString(type.GetProperty(key)!.GetValue(expected), System.Globalization.CultureInfo.InvariantCulture)!)).ToArray();
            if (predicates.Length == 0) throw RefusalException.Conflict("The prepared command has no canonical primary key.");
            return [predicates];
        }
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type, string table) => access.rowPolicy.GetReadConstraintsAsync(type, table);
        public Task ValidateRowsAsync(Type type, string table, IReadOnlyList<object> rows) => access.rowPolicy.ValidateRowsAsync(type, table, rows);
        public Task<string?> GetEffectiveAccessLevelAsync(Type type, string table, object row, bool includeInherited = true) => access.rowPolicy.GetEffectiveAccessLevelAsync(type, table, row, includeInherited);
        public async Task ValidateMutationAsync(Type type, string table, object entity, object? previous, PPDMRowMutation requested, string? assertedActor)
        {
            PPDMDataSourceTransaction.RequireOwned(source, System.Data.IsolationLevel.Serializable);
            if (type != expected.GetType() || table != type.Name || requested != operation || assertedActor != actor || Snapshot(entity) != expectedSnapshot ||
                (previous is null ? originalSnapshot is not null : Snapshot(previous, true) != originalSnapshot) || Interlocked.Exchange(ref used, 1) != 0)
                throw RefusalException.Forbidden("The mutation differs from its exact reviewed corrective-action command.");
            if (await (authorityReader is null ? access.RequireAuthorityAsync(review, incident, project, actor) : authorityReader()) != authoritySnapshot) throw RefusalException.Conflict("The command relationship changed before mutation.");
            await access.RequireOpenIncidentMutationAsync(review, incident);
            if (requireEvidence is not null) await requireEvidence();
            // Retiring the exact original assignment preserves its six-column key and historical evidence.
            // The original active row remains subject to the canonical policy; the prepared inactive copy is private to this command.
            if (preparedNode && (entity is not PROJECT_STEP || requested != PPDMRowMutation.Insert || previous is not null || requireEvidence is null))
                throw RefusalException.Forbidden("Only the exact prepared new action node may use reference preparation.");
            if (preparedPlan && (entity is not (PROJECT or PROJECT_BA or PROJECT_BA_ROLE or HSE_INCIDENT_RESPONSE) || requested != PPDMRowMutation.Insert || previous is not null || requireEvidence is null || authorityReader is null || preparedNode))
                throw RefusalException.Forbidden("Only the exact prepared compound plan may use canonical incident reference preparation.");
            if (!preparedNode && !preparedPlan) await access.rowPolicy.ValidateRowsAsync(type, table, [entity is PROJECT_STEP_BA { ACTIVE_IND: "N" } && previous is PROJECT_STEP_BA ? previous : entity]);
            var changedBy = type.GetProperty("ROW_CHANGED_BY")!.GetValue(entity) as string;
            if (changedBy != actor || type.GetProperty("ROW_CHANGED_DATE")!.GetValue(entity) is not DateTime { Kind: DateTimeKind.Utc } changed || changed > DateTime.UtcNow ||
                requested == PPDMRowMutation.Insert && (type.GetProperty("ROW_CREATED_BY")!.GetValue(entity) as string != actor || type.GetProperty("ROW_CREATED_DATE")!.GetValue(entity) is not DateTime { Kind: DateTimeKind.Utc }))
                throw RefusalException.Conflict("The canonical corrective-action audit columns are unavailable.");
            PPDMDataSourceTransaction.RequireOwned(source, System.Data.IsolationLevel.Serializable);
        }
    }
}
