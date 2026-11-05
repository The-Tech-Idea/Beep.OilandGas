using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.DataManagement.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services.HSE;
using Microsoft.Extensions.Logging.Abstractions;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.LifeCycle.Services.HSE;
public class PPDMHSEService : IFieldHSEService
{
    private readonly string _fieldId;
    private readonly ICompanyFieldSelectionService _selections;
    private readonly IPPDMRowAccessPolicy _rowAccess;
    private FieldSelectionReview? _review;
    private readonly IHSEService _incidentService;
    private readonly IRCAService _rcaService;
    private readonly IBarrierManagementService _barrierService;
    private readonly ICorrectiveActionService _correctiveActionService;
    private readonly IHAZOPService _hazopService;
    private readonly IHSEKPIService _kpiService;
    public PPDMHSEService(string fieldId, IDMEEditor editor, ICommonColumnHandler commonColumnHandler, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata, ICompanyFieldSelectionService selections, IPPDMRowAccessPolicy rowAccess, ICompanyCorrectiveActionCommands commands, ICompanyIncidentCommands? incidentCommands = null)
    {
        _fieldId = fieldId ?? throw new ArgumentNullException(nameof(fieldId));
        _selections = selections ?? throw new ArgumentNullException(nameof(selections));
        _rowAccess = rowAccess ?? throw new ArgumentNullException(nameof(rowAccess));
        _incidentService = new HSEIncidentService(editor, commonColumnHandler, defaults, metadata, selections, rowAccess, NullLogger<HSEIncidentService>.Instance, incidentCommands);
        _rcaService = new RCAService(editor, commonColumnHandler, defaults, metadata, selections, rowAccess, _incidentService, NullLogger<RCAService>.Instance);
        _barrierService = new BarrierManagementService(editor, commonColumnHandler, defaults, metadata, selections, rowAccess, _incidentService, NullLogger<BarrierManagementService>.Instance);
        _correctiveActionService = new CorrectiveActionService(editor, commonColumnHandler, defaults, metadata, selections, rowAccess, _incidentService, commands, NullLogger<CorrectiveActionService>.Instance);
        _hazopService = new HAZOPService(editor, commonColumnHandler, defaults, metadata, selections, rowAccess, NullLogger<HAZOPService>.Instance);
        _kpiService = new HSEKPIService(editor, commonColumnHandler, defaults, metadata, selections, rowAccess, NullLogger<HSEKPIService>.Instance);
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

    public async Task<HseRestrictionReview> ReviewIncidentRestrictionsAsync(string incidentId)
    {
        await RequireCurrentBindingAsync();
        return await ConfirmAsync(await _rowAccess.ReviewHseRestrictionsAsync(_review!, incidentId));
    }

    public async Task<List<HSEIncidentRecord>> GetIncidentsAsync(DateRangeFilter? range)
    {
        await RequireCurrentBindingAsync(_fieldId, null, true);
        return await ConfirmAsync(await _incidentService.GetFieldIncidentsAsync(_fieldId, range));
    }

    public async Task<HseIncidentPage> GetVisibleIncidentPageAsync(int pageNumber, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await RequireCurrentBindingAsync(_fieldId, null, true);
        var result = await _incidentService.GetVisibleFieldIncidentPageAsync(_fieldId, pageNumber, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        result.Require();
        if (result.Context != _review || result.PageNumber != pageNumber)
            throw RefusalException.Conflict("The incident page differs from the current FIELD request.");
        return await ConfirmAsync(result);
    }

    public async Task<HSEIncidentRecord?> GetIncidentAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(_fieldId, null, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        var incident = await _incidentService.GetByIdAsync(incidentId);
        return await ConfirmAsync(incident is not null && IsForCurrentField(incident) ? incident : null);
    }

    public async Task<HSEIncidentRecord> ReportIncidentAsync(ReportIncidentRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(_fieldId, userId, true);
        ArgumentNullException.ThrowIfNull(request);
        if (request.FieldId != _review!.FieldId)
            throw RefusalException.Conflict("The submitted HSE FIELD differs from the reviewed selection.");
        return await ConfirmAsync(await _incidentService.ReportIncidentAsync(request with { FieldId = _fieldId }, userId));
    }

    public async Task<InvestigatorAssignmentReview> ReviewInvestigatorAssignmentAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(_fieldId, null, true);
        if (string.IsNullOrWhiteSpace(incidentId)) throw RefusalException.Invalid("incidentId is required.");
        return await ConfirmAsync(await _incidentService.ReviewInvestigatorAssignmentAsync(incidentId));
    }

    public async Task AssignInvestigatorAsync(string incidentId, AssignIncidentInvestigatorRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(_fieldId, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await EnsureIncidentInFieldAsync(incidentId);
        await _incidentService.AssignInvestigatorAsync(incidentId, request, userId);
        await ConfirmAsync();
    }

    public async Task UpdateTierAsync(string incidentId, int tier, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(_fieldId, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await EnsureIncidentInFieldAsync(incidentId);
        await _incidentService.UpdateTierAsync(incidentId, tier, userId);
        await ConfirmAsync();
    }

    public async Task<bool> TransitionAsync(string incidentId, string trigger, string? reason, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(_fieldId, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await EnsureIncidentInFieldAsync(incidentId);
        return await ConfirmAsync(await _incidentService.TransitionAsync(incidentId, trigger, reason, userId));
    }

    public async Task<List<string>> GetAvailableTriggersAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(_fieldId, null, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await EnsureIncidentInFieldAsync(incidentId);
        return await ConfirmAsync(await _incidentService.GetAvailableTriggersAsync(incidentId));
    }

    public async Task<List<CauseFinding>> GetCausesAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(_fieldId, null, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await EnsureIncidentInFieldAsync(incidentId);
        return await ConfirmAsync(await _rcaService.GetCauseChainAsync(incidentId));
    }

    public async Task AddCauseAsync(string incidentId, AddCauseRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(_fieldId, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        ArgumentNullException.ThrowIfNull(request);
        await EnsureIncidentInFieldAsync(incidentId);
        await _rcaService.AddCauseAsync(incidentId, request, userId);
        await ConfirmAsync();
    }

    public async Task<bool> IsRcaCompleteAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(_fieldId, null, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await EnsureIncidentInFieldAsync(incidentId);
        return await ConfirmAsync(await _rcaService.IsRCACompleteAsync(incidentId));
    }

    public async Task<List<BarrierRecord>> GetBarriersAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(_fieldId, null, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await EnsureIncidentInFieldAsync(incidentId);
        return await ConfirmAsync(await _barrierService.GetBarriersForIncidentAsync(incidentId));
    }

    public async Task AddBarrierAsync(string incidentId, AddBarrierRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(_fieldId, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        ArgumentNullException.ThrowIfNull(request);
        await EnsureIncidentInFieldAsync(incidentId);
        await _barrierService.AddBarrierAsync(incidentId, request, userId);
        await ConfirmAsync();
    }

    public async Task SetBarrierStatusAsync(string incidentId, string equipId, string status, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(_fieldId, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await EnsureIncidentInFieldAsync(incidentId);
        await _barrierService.SetBarrierStatusAsync(incidentId, equipId, status, userId);
        await ConfirmAsync();
    }

    public async Task<BarrierSummary> GetBarrierSummaryAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(_fieldId, null, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await EnsureIncidentInFieldAsync(incidentId);
        return await ConfirmAsync(await _barrierService.GetBarrierSummaryAsync(incidentId));
    }

    public async Task<List<CAStatus>> GetCorrectiveActionsAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(_fieldId, null, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await EnsureIncidentInFieldAsync(incidentId);
        return await ConfirmAsync(await _correctiveActionService.GetCAStatusAsync(incidentId));
    }

    public async Task<CAPlanResolution> CreateCaPlanAsync(string incidentId, CreateCAPlanSubmission submission, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(_fieldId, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await EnsureIncidentInFieldAsync(incidentId);
        if (submission is null || submission.Context is null || submission.Intent is null)
            throw RefusalException.Invalid("A reviewed FIELD context and explicit plan creation intent are required.");
        if (submission.Context != _review) throw RefusalException.Conflict("The plan submission differs from the current FIELD review.");
        return await ConfirmAsync(await _correctiveActionService.CreateCAPlanAsync(incidentId, submission, userId));
    }

    public async Task<CAPlanCreationReview> ReviewCaPlanCreationAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(_fieldId, null, true);
        if (string.IsNullOrWhiteSpace(incidentId)) throw RefusalException.Invalid("incidentId is required.");
        await EnsureIncidentInFieldAsync(incidentId);
        var result = await _correctiveActionService.ReviewCAPlanCreationAsync(incidentId);
        if (result is null || result.Context != _review || result.IncidentId != incidentId)
            throw RefusalException.Conflict("The plan creation review differs from the current incident/FIELD.");
        await RequireCurrentBindingAsync(_fieldId, null, true);
        return result;
    }

    public async Task<CAPlanResolution> GetCaPlanAsync(string incidentId)
    {
        await RequireCurrentBindingAsync(_fieldId, null, true);
        if (string.IsNullOrWhiteSpace(incidentId)) throw RefusalException.Invalid("incidentId is required.");
        await EnsureIncidentInFieldAsync(incidentId);
        return await ConfirmAsync(await _correctiveActionService.GetCAPlanAsync(incidentId));
    }

    public async Task<string> AddCorrectiveActionAsync(string incidentId, AddCARequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(_fieldId, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        ArgumentNullException.ThrowIfNull(request);
        await EnsureIncidentInFieldAsync(incidentId);
        return await ConfirmAsync(await _correctiveActionService.AddCorrectiveActionAsync(incidentId, request, userId));
    }

    public async Task AssignCorrectiveActionResponsibleAsync(string incidentId, int stepSeq, string businessAssociateId, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(_fieldId, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await EnsureIncidentInFieldAsync(incidentId);
        await _correctiveActionService.AssignResponsiblePersonAsync(incidentId, stepSeq, businessAssociateId, userId);
        await ConfirmAsync();
    }

    public async Task RecordCompletionAsync(string incidentId, int stepSeq, string notes, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(_fieldId, userId, true);
        if (string.IsNullOrWhiteSpace(incidentId))
            throw RefusalException.Invalid("incidentId is required.");
        await EnsureIncidentInFieldAsync(incidentId);
        await _correctiveActionService.RecordCompletionAsync(incidentId, stepSeq, notes, userId);
        await ConfirmAsync();
    }

    public async Task<List<HAZOPSummary>> GetStudiesAsync()
    {
        await RequireCurrentBindingAsync(_fieldId, null, true);
        return await ConfirmAsync(await _hazopService.GetStudiesAsync(_fieldId));
    }

    public async Task<string> CreateStudyAsync(CreateHAZOPStudyRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(_fieldId, userId, true);
        ArgumentNullException.ThrowIfNull(request);
        if (request.FieldId != _review!.FieldId)
            throw RefusalException.Conflict("The submitted HSE FIELD differs from the reviewed selection.");
        var normalizedRequest = new CreateHAZOPStudyRequest
        {
            FieldId = _fieldId,
            Scope = request.Scope,
            StudyName = request.StudyName,
        };
        return await ConfirmAsync(await _hazopService.CreateStudyAsync(normalizedRequest, userId));
    }

    public async Task<HAZOPSummary> GetStudySummaryAsync(string studyId)
    {
        await RequireCurrentBindingAsync(_fieldId, null, true);
        if (string.IsNullOrWhiteSpace(studyId))
            throw RefusalException.Invalid("studyId is required.");
        return await ConfirmAsync(await _hazopService.GetSummaryAsync(studyId));
    }

    public async Task<List<HAZOPNode>> GetNodesAsync(string studyId)
    {
        await RequireCurrentBindingAsync(_fieldId, null, true);
        if (string.IsNullOrWhiteSpace(studyId))
            throw RefusalException.Invalid("studyId is required.");
        return await ConfirmAsync(await _hazopService.GetNodesAsync(studyId));
    }

    public async Task<string> AddNodeAsync(string studyId, AddNodeRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(_fieldId, userId, true);
        if (string.IsNullOrWhiteSpace(studyId))
            throw RefusalException.Invalid("studyId is required.");
        ArgumentNullException.ThrowIfNull(request);
        return await ConfirmAsync(await _hazopService.AddNodeAsync(studyId, request, userId));
    }

    public async Task<string> AddDeviationAsync(string studyId, int nodeSeq, AddDeviationRequest request, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(_fieldId, userId, true);
        if (string.IsNullOrWhiteSpace(studyId))
            throw RefusalException.Invalid("studyId is required.");
        ArgumentNullException.ThrowIfNull(request);
        return await ConfirmAsync(await _hazopService.AddDeviationAsync(studyId, nodeSeq, request, userId));
    }

    public async Task UpdateDeviationStatusAsync(string studyId, int nodeSeq, int condSeq, string status, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw RefusalException.Invalid("userId is required.");
        await RequireCurrentBindingAsync(_fieldId, userId, true);
        if (string.IsNullOrWhiteSpace(studyId))
            throw RefusalException.Invalid("studyId is required.");
        await _hazopService.UpdateDeviationStatusAsync(studyId, nodeSeq, condSeq, status, userId);
        await ConfirmAsync();
    }

    public async Task<HSEKPISet> GetKpisAsync(DateRangeFilter range)
    {
        await RequireCurrentBindingAsync(_fieldId, null, true);
        return await ConfirmAsync(await _kpiService.GetKPIsAsync(_fieldId, range));
    }

    public async Task<List<TierRateTrend>> GetTrendAsync(int months)
    {
        await RequireCurrentBindingAsync(_fieldId, null, true);
        return await ConfirmAsync(await _kpiService.GetTierRateTrendAsync(_fieldId, months));
    }

    private async Task EnsureIncidentInFieldAsync(string incidentId)
    {
        var incident = await GetIncidentAsync(incidentId);
        if (incident is null)
        {
            throw RefusalException.NotFound("The incident is unavailable in the reviewed FIELD.");
        }
    }

    private bool IsForCurrentField(HSEIncidentRecord incident) => string.Equals(incident.FieldId, _fieldId, StringComparison.Ordinal);
}
