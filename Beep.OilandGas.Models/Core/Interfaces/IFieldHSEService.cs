using Beep.OilandGas.Models.Data.HSE;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IFieldHSEService
{
    Task<List<HSEIncidentRecord>> GetIncidentsAsync(DateRangeFilter? range);
    Task<HSEIncidentRecord?> GetIncidentAsync(string incidentId);
    Task<HSEIncidentRecord> ReportIncidentAsync(ReportIncidentRequest request, string userId);
    Task AssignInvestigatorAsync(string incidentId, string baId, string userId);
    Task UpdateTierAsync(string incidentId, int tier, string userId);
    Task<bool> TransitionAsync(string incidentId, string trigger, string? reason, string userId);
    Task<List<string>> GetAvailableTriggersAsync(string incidentId);

    Task<List<CauseFinding>> GetCausesAsync(string incidentId);
    Task AddCauseAsync(string incidentId, AddCauseRequest request, string userId);
    Task<bool> IsRcaCompleteAsync(string incidentId);

    Task<List<BarrierRecord>> GetBarriersAsync(string incidentId);
    Task AddBarrierAsync(string incidentId, AddBarrierRequest request, string userId);
    Task SetBarrierStatusAsync(string incidentId, string equipId, string status, string userId);
    Task<BarrierSummary> GetBarrierSummaryAsync(string incidentId);

    Task<List<CAStatus>> GetCorrectiveActionsAsync(string incidentId);
    Task<string> CreateCaPlanAsync(string incidentId, string userId);
    Task<string> AddCorrectiveActionAsync(string incidentId, AddCARequest request, string userId);
    Task RecordCompletionAsync(string incidentId, int stepSeq, string notes, string userId);

    Task<List<HAZOPSummary>> GetStudiesAsync();
    Task<string> CreateStudyAsync(CreateHAZOPStudyRequest request, string userId);
    Task<HAZOPSummary> GetStudySummaryAsync(string studyId);
    Task<List<HAZOPNode>> GetNodesAsync(string studyId);
    Task<string> AddNodeAsync(string studyId, AddNodeRequest request, string userId);
    Task<string> AddDeviationAsync(string studyId, int nodeSeq, AddDeviationRequest request, string userId);
    Task UpdateDeviationStatusAsync(string studyId, int nodeSeq, int condSeq, string status, string userId);

    Task<HSEKPISet> GetKpisAsync(DateRangeFilter range);
    Task<List<TierRateTrend>> GetTrendAsync(int months);
}