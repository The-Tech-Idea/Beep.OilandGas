using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Analytics;

namespace Beep.OilandGas.Web.Services;

public interface IBusinessProcessServiceClient
{
    Task<List<ProcessDefinition>> GetDefinitionsAsync(CancellationToken cancellationToken = default);
    Task<ProcessDefinition?> GetDefinitionAsync(string processId, CancellationToken cancellationToken = default);
    Task<ProcessInstance?> StartInstanceAsync(ProcessInstanceRequest request, CancellationToken cancellationToken = default);
    Task<ProcessInstance?> SubmitComplianceReportAsync(ComplianceReportRequest request, CancellationToken cancellationToken = default);
    Task<List<ProcessInstanceSummary>> GetHseIncidentsAsync(string? status = null, string? severity = null, CancellationToken cancellationToken = default);
    Task<ProcessInstance?> GetHseIncidentAsync(string incidentId, CancellationToken cancellationToken = default);
    Task SubmitHseIncidentRcaAsync(string incidentId, HSEIncidentUpdateRequest request, CancellationToken cancellationToken = default);
    Task RaiseHseCorrectiveActionsAsync(string incidentId, HSEIncidentUpdateRequest request, CancellationToken cancellationToken = default);
    Task CloseHseCorrectiveActionAsync(string incidentId, string actionId, CorrectiveActionCloseRequest request, CancellationToken cancellationToken = default);
    Task CloseHseIncidentAsync(string incidentId, IncidentCloseRequest request, CancellationToken cancellationToken = default);
    Task<List<ProcessInstanceSummary>> GetInstancesAsync(CancellationToken cancellationToken = default);
    Task<ProcessInstance?> GetInstanceAsync(string instanceId, CancellationToken cancellationToken = default);
    Task<List<ProcessHistoryEntry>> GetHistoryAsync(string instanceId, CancellationToken cancellationToken = default);
    Task<List<ProcessInstanceSummary>> GetGatesAsync(string? status = null, CancellationToken cancellationToken = default);
    Task<List<ProcessInstanceSummary>> GetPendingGatesAsync(CancellationToken cancellationToken = default);
    Task<ProcessInstanceSummary?> GetGateAsync(string gateId, CancellationToken cancellationToken = default);
    Task<GateChecklistResponse?> GetGateChecklistAsync(string gateId, CancellationToken cancellationToken = default);
    Task ApproveGateAsync(string gateId, GateReviewDecisionRequest request, CancellationToken cancellationToken = default);
    Task RejectGateAsync(string gateId, GateReviewDecisionRequest request, CancellationToken cancellationToken = default);
    Task DeferGateAsync(string gateId, GateReviewDecisionRequest request, CancellationToken cancellationToken = default);
    Task<AnalyticsDashboardSummary?> GetAnalyticsDashboardAsync(int days = 30, double exposureHours = 1_000_000, CancellationToken cancellationToken = default);
    Task<List<KPITrendPoint>> GetProductionTrendAsync(int days = 365, CancellationToken cancellationToken = default);
    Task<ReservesMaturationSummary?> GetReservesMaturationAsync(CancellationToken cancellationToken = default);
}