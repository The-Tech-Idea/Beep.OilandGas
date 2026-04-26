using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services.HSE;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.LifeCycle.Services.HSE;

public class PPDMHSEService : IFieldHSEService
{
    private readonly string _fieldId;
    private readonly IHSEService _incidentService;
    private readonly IRCAService _rcaService;
    private readonly IBarrierManagementService _barrierService;
    private readonly ICorrectiveActionService _correctiveActionService;
    private readonly IHAZOPService _hazopService;
    private readonly IHSEKPIService _kpiService;

    public PPDMHSEService(
        string fieldId,
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName)
    {
        _fieldId = fieldId ?? throw new ArgumentNullException(nameof(fieldId));
        _incidentService = new HSEIncidentService(editor, commonColumnHandler, defaults, metadata, connectionName, NullLogger<HSEIncidentService>.Instance);
        _rcaService = new RCAService(editor, commonColumnHandler, defaults, metadata, connectionName, NullLogger<RCAService>.Instance);
        _barrierService = new BarrierManagementService(editor, commonColumnHandler, defaults, metadata, connectionName, NullLogger<BarrierManagementService>.Instance);
        _correctiveActionService = new CorrectiveActionService(editor, commonColumnHandler, defaults, metadata, connectionName, NullLogger<CorrectiveActionService>.Instance);
        _hazopService = new HAZOPService(editor, commonColumnHandler, defaults, metadata, connectionName, NullLogger<HAZOPService>.Instance);
        _kpiService = new HSEKPIService(editor, commonColumnHandler, defaults, metadata, connectionName, NullLogger<HSEKPIService>.Instance);
    }

    public Task<List<HSEIncidentRecord>> GetIncidentsAsync(DateRangeFilter? range)
        => _incidentService.GetFieldIncidentsAsync(_fieldId, range);

    public async Task<HSEIncidentRecord?> GetIncidentAsync(string incidentId)
    {
        var incident = await _incidentService.GetByIdAsync(incidentId);
        return incident is not null && IsForCurrentField(incident) ? incident : null;
    }

    public Task<HSEIncidentRecord> ReportIncidentAsync(ReportIncidentRequest request, string userId)
        => _incidentService.ReportIncidentAsync(request with { FieldId = _fieldId }, userId);

    public async Task AssignInvestigatorAsync(string incidentId, string baId, string userId)
    {
        await EnsureIncidentInFieldAsync(incidentId);
        await _incidentService.AssignInvestigatorAsync(incidentId, baId, userId);
    }

    public async Task UpdateTierAsync(string incidentId, int tier, string userId)
    {
        await EnsureIncidentInFieldAsync(incidentId);
        await _incidentService.UpdateTierAsync(incidentId, tier, userId);
    }

    public async Task<bool> TransitionAsync(string incidentId, string trigger, string? reason, string userId)
    {
        await EnsureIncidentInFieldAsync(incidentId);
        return await _incidentService.TransitionAsync(incidentId, trigger, reason, userId);
    }

    public async Task<List<string>> GetAvailableTriggersAsync(string incidentId)
    {
        await EnsureIncidentInFieldAsync(incidentId);
        return await _incidentService.GetAvailableTriggersAsync(incidentId);
    }

    public async Task<List<CauseFinding>> GetCausesAsync(string incidentId)
    {
        await EnsureIncidentInFieldAsync(incidentId);
        return await _rcaService.GetCauseChainAsync(incidentId);
    }

    public async Task AddCauseAsync(string incidentId, AddCauseRequest request, string userId)
    {
        await EnsureIncidentInFieldAsync(incidentId);
        await _rcaService.AddCauseAsync(incidentId, request, userId);
    }

    public async Task<bool> IsRcaCompleteAsync(string incidentId)
    {
        await EnsureIncidentInFieldAsync(incidentId);
        return await _rcaService.IsRCACompleteAsync(incidentId);
    }

    public async Task<List<BarrierRecord>> GetBarriersAsync(string incidentId)
    {
        await EnsureIncidentInFieldAsync(incidentId);
        return await _barrierService.GetBarriersForIncidentAsync(incidentId);
    }

    public async Task AddBarrierAsync(string incidentId, AddBarrierRequest request, string userId)
    {
        await EnsureIncidentInFieldAsync(incidentId);
        await _barrierService.AddBarrierAsync(incidentId, request, userId);
    }

    public async Task SetBarrierStatusAsync(string incidentId, string equipId, string status, string userId)
    {
        await EnsureIncidentInFieldAsync(incidentId);
        await _barrierService.SetBarrierStatusAsync(incidentId, equipId, status, userId);
    }

    public async Task<BarrierSummary> GetBarrierSummaryAsync(string incidentId)
    {
        await EnsureIncidentInFieldAsync(incidentId);
        return await _barrierService.GetBarrierSummaryAsync(incidentId);
    }

    public async Task<List<CAStatus>> GetCorrectiveActionsAsync(string incidentId)
    {
        await EnsureIncidentInFieldAsync(incidentId);
        return await _correctiveActionService.GetCAStatusAsync(incidentId);
    }

    public async Task<string> CreateCaPlanAsync(string incidentId, string userId)
    {
        await EnsureIncidentInFieldAsync(incidentId);
        return await _correctiveActionService.CreateCAPlanAsync(incidentId, userId);
    }

    public async Task<string> AddCorrectiveActionAsync(string incidentId, AddCARequest request, string userId)
    {
        await EnsureIncidentInFieldAsync(incidentId);
        return await _correctiveActionService.AddCorrectiveActionAsync(incidentId, request, userId);
    }

    public async Task RecordCompletionAsync(string incidentId, int stepSeq, string notes, string userId)
    {
        await EnsureIncidentInFieldAsync(incidentId);
        await _correctiveActionService.RecordCompletionAsync(incidentId, stepSeq, notes, userId);
    }

    public Task<List<HAZOPSummary>> GetStudiesAsync()
        => _hazopService.GetStudiesAsync(_fieldId);

    public Task<string> CreateStudyAsync(CreateHAZOPStudyRequest request, string userId)
    {
        var normalizedRequest = new CreateHAZOPStudyRequest
        {
            FieldId = _fieldId,
            Scope = request.Scope,
            StudyName = request.StudyName,
        };

        return _hazopService.CreateStudyAsync(normalizedRequest, userId);
    }

    public Task<HAZOPSummary> GetStudySummaryAsync(string studyId)
        => _hazopService.GetSummaryAsync(studyId);

    public Task<List<HAZOPNode>> GetNodesAsync(string studyId)
        => _hazopService.GetNodesAsync(studyId);

    public Task<string> AddNodeAsync(string studyId, AddNodeRequest request, string userId)
        => _hazopService.AddNodeAsync(studyId, request, userId);

    public Task<string> AddDeviationAsync(string studyId, int nodeSeq, AddDeviationRequest request, string userId)
        => _hazopService.AddDeviationAsync(studyId, nodeSeq, request, userId);

    public Task UpdateDeviationStatusAsync(string studyId, int nodeSeq, int condSeq, string status, string userId)
        => _hazopService.UpdateDeviationStatusAsync(studyId, nodeSeq, condSeq, status, userId);

    public Task<HSEKPISet> GetKpisAsync(DateRangeFilter range)
        => _kpiService.GetKPIsAsync(_fieldId, range);

    public Task<List<TierRateTrend>> GetTrendAsync(int months)
        => _kpiService.GetTierRateTrendAsync(_fieldId, months);

    private async Task EnsureIncidentInFieldAsync(string incidentId)
    {
        var incident = await GetIncidentAsync(incidentId);
        if (incident is null)
        {
            throw new InvalidOperationException($"Incident {incidentId} was not found in field {_fieldId}.");
        }
    }

    private bool IsForCurrentField(HSEIncidentRecord incident)
        => string.Equals(incident.FieldId, _fieldId, StringComparison.OrdinalIgnoreCase);
}