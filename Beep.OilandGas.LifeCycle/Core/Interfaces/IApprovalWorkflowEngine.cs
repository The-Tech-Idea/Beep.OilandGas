using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;

namespace Beep.OilandGas.LifeCycle.Core.Interfaces;

/// <summary>
/// Multi-level approval workflow engine for process steps.
/// Supports sequential, parallel, quorum, and DoA-driven approval patterns.
/// </summary>
public interface IApprovalWorkflowEngine
{
    Task<ApprovalChainResult> CreateApprovalChainAsync(string processInstanceId, string stepInstanceId, ApprovalChainConfig config, string userId, CancellationToken cancellationToken = default);
    Task<ApprovalChainResult> CreateApprovalChainWithDoAAsync(string processInstanceId, string stepInstanceId, string entityType, Dictionary<string, object> entityFields, IDoAEvaluationService doaService, string userId, CancellationToken cancellationToken = default);
    Task<ApprovalDecisionResult> RecordApprovalAsync(string approvalId, string decision, string? comments, string userId, CancellationToken cancellationToken = default);
    Task<bool> DelegateApprovalAsync(string approvalId, string delegateUserId, string reason, string userId, CancellationToken cancellationToken = default);
    Task<List<PROCESS_APPROVAL>> GetPendingApprovalsAsync(string userId);
    Task<bool> IsApprovalChainCompleteAsync(string stepInstanceId);
    Task<bool> IsApprovalChainApprovedAsync(string stepInstanceId);
}
