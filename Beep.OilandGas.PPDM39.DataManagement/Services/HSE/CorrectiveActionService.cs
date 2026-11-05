using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Constants;
using System.Globalization;
using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdea.Data.OilGas;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.HSE;
public class CorrectiveActionService : ICorrectiveActionService
{
    private const string CorrectiveActionProjectType = HseReferenceCodes.CorrectiveAction.ProjectType;
    private const string CorrectiveActionStatusType = HseReferenceCodes.CorrectiveAction.StatusType;
    private const string OpenStatus = HseReferenceCodes.CorrectiveAction.Open;
    private const string CompletedStatus = HseReferenceCodes.CorrectiveAction.Completed;
    private const string ResponsibleRole = HseReferenceCodes.CorrectiveAction.ResponsibleRole;
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly ICompanyFieldSelectionService _selections;
    private readonly IPPDMRowAccessPolicy _rowAccess;
    private FieldSelectionReview? _review;
    private readonly IHSEService _incidents;
    private readonly ICompanyCorrectiveActionCommands _commands;
    private readonly ILogger<CorrectiveActionService> _logger;
    public CorrectiveActionService(IDMEEditor editor, ICommonColumnHandler commonColumnHandler, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata, ICompanyFieldSelectionService selections, IPPDMRowAccessPolicy rowAccess, IHSEService incidents, ICompanyCorrectiveActionCommands commands, ILogger<CorrectiveActionService> logger)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _selections = selections ?? throw new ArgumentNullException(nameof(selections));
        _rowAccess = rowAccess ?? throw new ArgumentNullException(nameof(rowAccess));
        _incidents = incidents ?? throw new ArgumentNullException(nameof(incidents));
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        _logger = logger;
    }

    // The existing profile owns authority; this is only a fence for one phase request.
    private async Task<string> RequireCurrentBindingAsync(string? fieldId = null, string? actor = null, bool requireField = true)
    {
        var current = await _selections.GetCurrentFieldSelectionAsync();
        if (current.SelectionContext is not { } review)
            throw RefusalException.Conflict("The company FIELD review is unavailable.");
        FieldSelectionReviewValidator.RequireCurrent(current, review.Company, review.ActorUserId);
        if (requireField && !current.SelectionAvailable || fieldId is not null && fieldId != current.FieldId || actor is not null && actor != review.ActorUserId || _review is not null && _review != review)
            throw RefusalException.Conflict("The HSE account, company, database or FIELD differs from its reviewed request.");
        _review = review;
        return review.Database.ConnectionName;
    }

    private async Task<T> ConfirmAsync<T>(T result)
    {
        await RequireCurrentBindingAsync();
        return result;
    }

    private async Task ConfirmAsync()
    {
        await RequireCurrentBindingAsync();
    }

    private async Task RequireIncidentAsync(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        var incident = await _incidents.GetByIdAsync(incidentId);
        if (incident is null || incident.IncidentId != incidentId || incident.FieldId != _review!.FieldId)
            throw RefusalException.NotFound("The incident is not recorded in the reviewed HSE FIELD.");
        await RequireCurrentBindingAsync();
    }

    public async Task<CAPlanResolution> CreateCAPlanAsync(string incidentId, CreateCAPlanSubmission submission, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId)) throw RefusalException.Invalid("incidentId is required.");
        if (submission is null || submission.Context is null || submission.Intent is null)
            throw RefusalException.Invalid("A reviewed FIELD context and explicit plan creation intent are required.");
        if (submission.Context != _review) throw RefusalException.Conflict("The plan submission differs from the current FIELD review.");
        await RequireIncidentAsync(incidentId);
        var id = await _commands.CreatePlanAsync(_review!, incidentId, submission.Intent, userId);
        if (id != submission.Intent.PlanId.ToString("N") || await ResolvePlanAsync(incidentId) != id)
            throw RefusalException.Conflict("The created plan could not be resolved through its current canonical incident link.");
        return await ConfirmAsync(new CAPlanResolution(_review!, incidentId, id));
    }

    public async Task<CAPlanCreationReview> ReviewCAPlanCreationAsync(string incidentId)
    {
        await RequireCurrentBindingAsync();
        await RequireIncidentAsync(incidentId);
        if (_commands is null) throw RefusalException.Conflict("The canonical plan command owner is unavailable.");
        var result = await _commands.ReviewPlanCreationAsync(_review!, incidentId);
        if (result is null || result.Context != _review || result.IncidentId != incidentId)
            throw RefusalException.Conflict("The plan creation review differs from the current incident/FIELD.");
        await RequireCurrentBindingAsync();
        return result;
    }

    public async Task<CAPlanResolution> GetCAPlanAsync(string incidentId)
    {
        await RequireCurrentBindingAsync();
        return await ConfirmAsync(new CAPlanResolution(_review!, incidentId, await ResolvePlanAsync(incidentId)));
    }

    public async Task<string> AddCorrectiveActionAsync(string incidentId, AddCARequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        ArgumentNullException.ThrowIfNull(request);
        var planId = await ResolvePlanAsync(incidentId);
        var stepId = await _commands.AddCorrectiveActionAsync(_review!, incidentId, planId, request, userId);
        return await ConfirmAsync(stepId);
    }

    public async Task AssignResponsiblePersonAsync(string incidentId, int stepSeq, string baId, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        var planId = await ResolvePlanAsync(incidentId);
        await _commands.AssignResponsiblePersonAsync(_review!, incidentId, planId, stepSeq, baId, userId);
        await ConfirmAsync();
    }

    public async Task SetDueDateAsync(string incidentId, int stepSeq, DateTime dueDate, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        var planId = await ResolvePlanAsync(incidentId);
        await _commands.SetDueDateAsync(_review!, incidentId, planId, stepSeq, dueDate, userId);
        await ConfirmAsync();
    }

    public async Task RecordCompletionAsync(string incidentId, int stepSeq, string completionNotes, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(null, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        var planId = await ResolvePlanAsync(incidentId);
        await _commands.RecordCompletionAsync(_review!, incidentId, planId, stepSeq, completionNotes, userId);
        await ConfirmAsync();
    }

    public async Task<bool> AllCAsMeetDeadlineAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(null, null, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        var statuses = await GetCAStatusAsync(incidentId);
        if (statuses.Count == 0) throw RefusalException.Conflict("No recorded corrective actions establish completion or deadline evidence.");
        return await ConfirmAsync(statuses.All(ca => !ca.IsOverdue && ca.Status == "COMPLETED"));
    }

    public async Task<List<CAStatus>> GetCAStatusAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(null, null, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        var planId = await FindPlanAsync(incidentId);
        if (planId is null)
        {
            var incident = await _incidents.GetByIdAsync(incidentId);
            var state = incident?.CurrentState;
            if (incident?.IncidentId != incidentId || incident.FieldId != _review!.FieldId ||
                state is not (IncidentState.Reported or IncidentState.UnderInvestigation or IncidentState.PendingRCA))
                throw RefusalException.Conflict("Only an early-stage incident with confirmed absence of a recorded plan can have no corrective actions.");
            if (await FindPlanAsync(incidentId) is not null)
                throw RefusalException.Conflict("A corrective-action plan was recorded during review. Review again.");
            var closing = await _incidents.GetByIdAsync(incidentId);
            if (closing?.IncidentId != incidentId || closing.FieldId != _review.FieldId || closing.CurrentState != state)
                throw RefusalException.Conflict("The incident state changed while confirming the absence of corrective work.");
            return await ConfirmAsync(new List<CAStatus>());
        }
        var steps = await GetStepsAsync(planId);
        var statuses = await GetLatestStepStatusesAsync(planId);
        var responsiblePeople = await GetResponsiblePeopleAsync(planId);
        var now = DateTime.UtcNow;
        if (steps.Any(step => step.STEP_SEQ_NO <= 0 || step.STEP_SEQ_NO > int.MaxValue || decimal.Truncate(step.STEP_SEQ_NO) != step.STEP_SEQ_NO ||
            string.IsNullOrWhiteSpace(step.STEP_NAME) || string.IsNullOrWhiteSpace(step.STEP_TYPE) || string.IsNullOrWhiteSpace(step.STEP_ID)) ||
            steps.Select(step => step.STEP_SEQ_NO).Distinct().Count() != steps.Count || steps.Select(step => step.STEP_ID).Distinct(StringComparer.Ordinal).Count() != steps.Count)
            throw RefusalException.Conflict("The recorded corrective-action identity, sequence or description is incomplete or ambiguous.");
        var stepIds = steps.Select(step => step.STEP_ID).ToHashSet(StringComparer.Ordinal);
        if (statuses.Keys.Any(key => !stepIds.Contains(key)) || responsiblePeople.Keys.Any(key => !stepIds.Contains(key)))
            throw RefusalException.Conflict("The corrective-action evidence contains an orphan status or responsible assignment.");
        var result = new List<CAStatus>();
        foreach (var step in steps.OrderBy(step => step.STEP_SEQ_NO))
        {
            if (!statuses.TryGetValue(step.STEP_ID, out var status) || !responsiblePeople.TryGetValue(step.STEP_ID, out var responsible))
                throw RefusalException.Conflict("A corrective action requires explicit recorded status and responsible participant evidence.");
            if (step.DUE_DATE is not { Kind: DateTimeKind.Utc } dueDate)
                throw RefusalException.Conflict("A corrective action requires its recorded UTC due date.");
            var completed = status.STATUS == CompletedStatus;
            if (completed && (step.ACTUAL_END_DATE is not { Kind: DateTimeKind.Utc } || step.ACTUAL_END_DATE > now || step.ACTUAL_END_DATE > status.EFFECTIVE_DATE) ||
                !completed && step.ACTUAL_END_DATE is not null)
                throw RefusalException.Conflict("The corrective-action completion date disagrees with its recorded status.");
            result.Add(new CAStatus((int)step.STEP_SEQ_NO, step.STEP_NAME, step.STEP_TYPE, status.STATUS, dueDate,
                completed ? step.ACTUAL_END_DATE!.Value > dueDate : dueDate < now, responsible, step.ACTUAL_END_DATE));
        }
        if (await ResolvePlanAsync(incidentId) != planId)
            throw RefusalException.Conflict("The corrective-action incident/project relationship changed during review.");
        return await ConfirmAsync(result);
    }

    private async Task<string> ResolvePlanAsync(string incidentId) =>
        await FindPlanAsync(incidentId) ?? throw RefusalException.Conflict("A recorded corrective-action plan is required for this operation.");

    private async Task<string?> FindPlanAsync(string incidentId)
    {
        await RequireCurrentBindingAsync();
        var context = _review!;
        var resolution = await _commands.ResolveIncidentPlanAsync(context, incidentId, context.ActorUserId);
        if (resolution is not null && (resolution.Context != context || resolution.IncidentId != incidentId || string.IsNullOrWhiteSpace(resolution.PlanId)))
            throw RefusalException.Conflict("The native incident plan resolution differs from the current FIELD request.");
        await RequireCurrentBindingAsync(); return resolution?.PlanId;
    }
    private async Task<PPDMGenericRepository> BuildStepRepoAsync() => new(_editor, _commonColumnHandler, _defaults, _metadata, typeof(PROJECT_STEP), await RequireCurrentBindingAsync(), "PROJECT_STEP", rowAccess: _rowAccess);
    private async Task<PPDMGenericRepository> BuildStepBaRepoAsync() => new(_editor, _commonColumnHandler, _defaults, _metadata, typeof(PROJECT_STEP_BA), await RequireCurrentBindingAsync(), "PROJECT_STEP_BA", rowAccess: _rowAccess);
    private async Task<PPDMGenericRepository> BuildStatusRepoAsync() => new(_editor, _commonColumnHandler, _defaults, _metadata, typeof(PROJECT_STATUS), await RequireCurrentBindingAsync(), "PROJECT_STATUS", rowAccess: _rowAccess);
    private async Task<List<PROJECT_STEP>> GetStepsAsync(string planId)
    {
        var repo = await BuildStepRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "PROJECT_ID",
                Operator = "=",
                FilterValue = planId
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        return (await repo.GetAsync(filters)).OfType<PROJECT_STEP>().ToList();
    }

    private async Task<Dictionary<string, PROJECT_STATUS>> GetLatestStepStatusesAsync(string planId)
    {
        var repo = await BuildStatusRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "PROJECT_ID",
                Operator = "=",
                FilterValue = planId
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
            new AppFilter
            {
                FieldName = "STATUS_TYPE",
                Operator = "=",
                FilterValue = CorrectiveActionStatusType
            },
        };
        var rows = (await repo.GetAsync(filters)).Cast<PROJECT_STATUS>().ToArray();
        if (rows.Any(row => row.EFFECTIVE_DATE is not { Kind: DateTimeKind.Utc } || string.IsNullOrWhiteSpace(row.STEP_ID)))
            throw RefusalException.Conflict("A corrective-action status requires recorded UTC effective-date and step evidence.");
        var result = new Dictionary<string, PROJECT_STATUS>(StringComparer.Ordinal);
        foreach (var group in rows.GroupBy(row => row.STEP_ID, StringComparer.Ordinal))
        {
            var latest = group.Where(row => row.EFFECTIVE_DATE == group.Max(value => value.EFFECTIVE_DATE)).ToArray();
            if (latest.Length != 1) throw RefusalException.Conflict("The latest recorded corrective-action status is ambiguous.");
            result.Add(group.Key, latest[0]);
        }
        return result;
    }

    private async Task<Dictionary<string, string>> GetResponsiblePeopleAsync(string planId)
    {
        var repo = await BuildStepBaRepoAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter
            {
                FieldName = "PROJECT_ID",
                Operator = "=",
                FilterValue = planId
            },
            new AppFilter
            {
                FieldName = "ROLE",
                Operator = "=",
                FilterValue = ResponsibleRole
            },
            new AppFilter
            {
                FieldName = "ACTIVE_IND",
                Operator = "=",
                FilterValue = "Y"
            },
        };
        var rows = (await repo.GetAsync(filters)).Cast<PROJECT_STEP_BA>().ToArray();
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var row in rows)
            if (string.IsNullOrWhiteSpace(row.STEP_ID) || string.IsNullOrWhiteSpace(row.BUSINESS_ASSOCIATE_ID) || !result.TryAdd(row.STEP_ID, row.BUSINESS_ASSOCIATE_ID))
                throw RefusalException.Conflict("The recorded responsible corrective-action assignment is missing or ambiguous.");
        return result;
    }


}
