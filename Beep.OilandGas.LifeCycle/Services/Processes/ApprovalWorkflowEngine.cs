using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Multi-level approval workflow engine for process steps.
/// Supports sequential, parallel, and any-of-N approval patterns with delegation and escalation.
/// </summary>
public class ApprovalWorkflowEngine
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<ApprovalWorkflowEngine>? _logger;

    private PPDMGenericRepository? _approvalRepository;

    public ApprovalWorkflowEngine(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<ApprovalWorkflowEngine>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    public async Task<ApprovalChainResult> CreateApprovalChainAsync(
        string processInstanceId,
        string stepInstanceId,
        ApprovalChainConfig config,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var repo = await GetApprovalRepositoryAsync();
        var result = new ApprovalChainResult { Success = true };

        foreach (var approver in config.Approvers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var approval = new PROCESS_APPROVAL
            {
                PROCESS_APPROVAL_ID = Guid.NewGuid().ToString(),
                PROCESS_STEP_INSTANCE_ID = stepInstanceId,
                APPROVAL_TYPE = config.ApprovalType,
                APPROVAL_STATUS = approver.Sequence == 1 ? "PENDING" : "WAITING",
                REQUESTED_DATE = DateTime.UtcNow,
                REQUESTED_BY = userId,
                APPROVER_SEQUENCE = approver.Sequence,
                APPROVER_USER_ID = approver.ApproverUserId,
                REQUIRED_ACTION = approver.RequiredAction,
                ESCALATION_AFTER_HOURS = config.EscalationAfterHours
            };

            await repo.InsertAsync(approval, userId);
            result.ApprovalsCreated++;
        }

        _logger?.LogInformation(
            "Created approval chain for step {StepInstance} with {Count} approvers",
            stepInstanceId, result.ApprovalsCreated);

        return result;
    }

    public async Task<ApprovalDecisionResult> RecordApprovalAsync(
        string approvalId,
        string decision,
        string? comments,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var repo = await GetApprovalRepositoryAsync();
        var result = new ApprovalDecisionResult { Success = true };

        var existing = await repo.GetByIdAsync(approvalId);
        if (existing == null)
        {
            result.Success = false;
            result.Errors.Add($"Approval {approvalId} not found.");
            return result;
        }

        var approval = (PROCESS_APPROVAL)existing;
        approval.APPROVAL_STATUS = decision.ToUpperInvariant();
        approval.APPROVAL_DATE = DateTime.UtcNow;
        approval.APPROVAL_NOTES = comments;
        approval.APPROVED_BY = userId;

        await repo.UpdateAsync(approval, userId);
        result.ApprovalRecorded = true;

        if (approval.APPROVAL_TYPE == "SEQUENTIAL" && decision == "APPROVED")
        {
            if (!string.IsNullOrEmpty(approval.PROCESS_STEP_INSTANCE_ID))
            {
                await AdvanceNextApprovalAsync(approval.PROCESS_STEP_INSTANCE_ID, approval.APPROVER_SEQUENCE ?? 0, userId);
            }
        }

        var chainComplete = !string.IsNullOrEmpty(approval.PROCESS_STEP_INSTANCE_ID) && await IsApprovalChainCompleteAsync(approval.PROCESS_STEP_INSTANCE_ID!);
        result.ChainComplete = chainComplete;
        result.ChainApproved = chainComplete && !string.IsNullOrEmpty(approval.PROCESS_STEP_INSTANCE_ID) && await IsApprovalChainApprovedAsync(approval.PROCESS_STEP_INSTANCE_ID!);

        _logger?.LogInformation(
            "Approval {ApprovalId} recorded as {Decision} by {UserId}. Chain complete: {ChainComplete}",
            approvalId, decision, userId, chainComplete);

        return result;
    }

    public async Task<bool> DelegateApprovalAsync(
        string approvalId,
        string delegateUserId,
        string reason,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var repo = await GetApprovalRepositoryAsync();

        var existing = await repo.GetByIdAsync(approvalId);
        if (existing == null)
            return false;

        var approval = (PROCESS_APPROVAL)existing;
        approval.DELEGATED_TO = delegateUserId;
        approval.DELEGATION_REASON = reason;
        approval.DELEGATED_BY = userId;
        approval.DELEGATION_DATE = DateTime.UtcNow;
        approval.APPROVAL_STATUS = "DELEGATED";

        await repo.UpdateAsync(approval, userId);

        var newApproval = new PROCESS_APPROVAL
        {
            PROCESS_APPROVAL_ID = Guid.NewGuid().ToString(),
            PROCESS_STEP_INSTANCE_ID = approval.PROCESS_STEP_INSTANCE_ID,
            APPROVAL_TYPE = approval.APPROVAL_TYPE,
            APPROVAL_STATUS = "PENDING",
            REQUESTED_DATE = DateTime.UtcNow,
            REQUESTED_BY = userId,
            APPROVER_SEQUENCE = approval.APPROVER_SEQUENCE,
            APPROVER_USER_ID = delegateUserId,
            REQUIRED_ACTION = approval.REQUIRED_ACTION,
            DELEGATED_FROM = userId
        };

        await repo.InsertAsync(newApproval, userId);

        _logger?.LogInformation("Approval {ApprovalId} delegated from {From} to {To}", approvalId, userId, delegateUserId);
        return true;
    }

    public async Task<List<PROCESS_APPROVAL>> GetPendingApprovalsAsync(string userId)
    {
        var repo = await GetApprovalRepositoryAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "APPROVER_USER_ID", Operator = "=", FilterValue = userId },
            new AppFilter { FieldName = "APPROVAL_STATUS", Operator = "=", FilterValue = "PENDING" }
        };

        var results = await repo.GetAsync(filters);
        return results.OfType<PROCESS_APPROVAL>().ToList();
    }

    public async Task<bool> IsApprovalChainCompleteAsync(string stepInstanceId)
    {
        var repo = await GetApprovalRepositoryAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "PROCESS_STEP_INSTANCE_ID", Operator = "=", FilterValue = stepInstanceId }
        };

        var approvals = (await repo.GetAsync(filters)).OfType<PROCESS_APPROVAL>().ToList();
        if (approvals.Count == 0)
            return true;

        var approvalType = approvals.First().APPROVAL_TYPE;

        if (approvalType == "ANY")
            return approvals.Any(a => a.APPROVAL_STATUS == "APPROVED");

        if (approvalType == "SEQUENTIAL")
        {
            var maxSequence = approvals.Max(a => a.APPROVER_SEQUENCE ?? 0);
            var lastApproval = approvals.FirstOrDefault(a => a.APPROVER_SEQUENCE == maxSequence);
            return lastApproval != null && lastApproval.APPROVAL_STATUS == "APPROVED";
        }

        return approvals.All(a => a.APPROVAL_STATUS == "APPROVED");
    }

    public async Task<bool> IsApprovalChainApprovedAsync(string stepInstanceId)
    {
        var repo = await GetApprovalRepositoryAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "PROCESS_STEP_INSTANCE_ID", Operator = "=", FilterValue = stepInstanceId }
        };

        var approvals = (await repo.GetAsync(filters)).OfType<PROCESS_APPROVAL>().ToList();

        if (approvals.Any(a => a.APPROVAL_STATUS == "REJECTED"))
            return false;

        return await IsApprovalChainCompleteAsync(stepInstanceId);
    }

    private async Task AdvanceNextApprovalAsync(string stepInstanceId, int currentSequence, string userId)
    {
        var repo = await GetApprovalRepositoryAsync();
        var filters = new List<AppFilter>
        {
            new AppFilter { FieldName = "PROCESS_STEP_INSTANCE_ID", Operator = "=", FilterValue = stepInstanceId },
            new AppFilter { FieldName = "APPROVER_SEQUENCE", Operator = "=", FilterValue = (currentSequence + 1).ToString() },
            new AppFilter { FieldName = "APPROVAL_STATUS", Operator = "=", FilterValue = "WAITING" }
        };

        var results = await repo.GetAsync(filters);
        foreach (var item in results)
        {
            var approval = (PROCESS_APPROVAL)item;
            approval.APPROVAL_STATUS = "PENDING";
            await repo.UpdateAsync(approval, userId);
        }
    }

    private async Task<PPDMGenericRepository> GetApprovalRepositoryAsync()
    {
        if (_approvalRepository != null)
            return _approvalRepository;

        _approvalRepository = new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROCESS_APPROVAL), _connectionName, "PROCESS_APPROVAL");

        return _approvalRepository;
    }
}

public class ApprovalChainConfig
{
    public string ApprovalType { get; set; } = "SEQUENTIAL";
    public int? EscalationAfterHours { get; set; }
    public List<ApproverConfig> Approvers { get; set; } = new();
}

public class ApproverConfig
{
    public string ApproverUserId { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public string? RequiredAction { get; set; }
}

public class ApprovalChainResult
{
    public bool Success { get; set; }
    public int ApprovalsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ApprovalDecisionResult
{
    public bool Success { get; set; }
    public bool ApprovalRecorded { get; set; }
    public bool ChainComplete { get; set; }
    public bool ChainApproved { get; set; }
    public List<string> Errors { get; set; } = new();
}
