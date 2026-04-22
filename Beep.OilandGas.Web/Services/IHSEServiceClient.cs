using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.HSE;

namespace Beep.OilandGas.Web.Services;

public interface IHSEServiceClient
{
    Task<List<HSEIncidentRecord>> GetIncidentsAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<HSEIncidentRecord?> GetIncidentAsync(string incidentId, CancellationToken cancellationToken = default);
    Task<List<CauseFinding>> GetIncidentCausesAsync(string incidentId, CancellationToken cancellationToken = default);
    Task<List<CAStatus>> GetIncidentCorrectiveActionsAsync(string incidentId, CancellationToken cancellationToken = default);
    Task<BarrierSummary?> GetIncidentBarrierSummaryAsync(string incidentId, CancellationToken cancellationToken = default);
    Task<bool> IsIncidentRcaCompleteAsync(string incidentId, CancellationToken cancellationToken = default);
    Task<bool> TransitionIncidentAsync(string incidentId, TransitionIncidentRequest request, CancellationToken cancellationToken = default);
    Task<bool> AddIncidentCauseAsync(string incidentId, AddCauseRequest request, CancellationToken cancellationToken = default);
    Task<bool> AddIncidentCorrectiveActionAsync(string incidentId, AddCARequest request, CancellationToken cancellationToken = default);
    Task<bool> CompleteIncidentCorrectiveActionAsync(string incidentId, int stepSeq, string notes = "", CancellationToken cancellationToken = default);
    Task<HAZOPSummary?> GetHazopStudyAsync(string studyId, CancellationToken cancellationToken = default);
    Task<List<HAZOPNode>> GetHazopNodesAsync(string studyId, CancellationToken cancellationToken = default);
    Task<string?> AddHazopNodeAsync(string studyId, AddNodeRequest request, CancellationToken cancellationToken = default);
    Task<string?> AddHazopDeviationAsync(string studyId, int nodeSeq, AddDeviationRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateHazopDeviationStatusAsync(string studyId, int nodeSeq, int condSeq, string status, CancellationToken cancellationToken = default);
    Task<HSEKPISet?> GetKpisAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<HSEIncidentRecord?> ReportIncidentAsync(ReportIncidentRequest request, CancellationToken cancellationToken = default);
}