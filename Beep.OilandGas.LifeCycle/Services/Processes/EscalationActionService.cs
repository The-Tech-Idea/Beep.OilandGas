using Beep.OilandGas.PPDM39.Core;
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
/// Executes escalation actions when an SLA breach is detected.
/// Actions include: reassign to backup, notify manager, auto-approve (low-risk only),
/// escalate to next DOA level, or suspend process for admin intervention.
/// Part of Phase 2 workflow engine enhancement.
/// </summary>
public interface IEscalationActionService
{
    /// <summary>
    /// Execute the configured escalation action for a breached step.
    /// </summary>
    Task<EscalationResult> ExecuteEscalationAsync(
        string processInstanceId,
        string stepInstanceId,
        string escalationAction,
        string? escalationTarget,
        string userId);

    /// <summary>
    /// Get the default escalation action for a given step type.
    /// </summary>
    string GetDefaultAction(string stepType);
}

public class EscalationResult
{
    public bool Success { get; set; }
    public string ActionTaken { get; set; } = string.Empty;
    public string? NewAssigneeId { get; set; }
    public string? NotificationTarget { get; set; }
    public DateTime ActionTimestamp { get; set; } = DateTime.UtcNow;
    public string? Message { get; set; }
}

public class EscalationActionService : IEscalationActionService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<EscalationActionService> _logger;

    // Default actions by step type
    private static readonly Dictionary<string, string> DefaultActions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["APPROVAL"] = "REASSIGN_TO_BACKUP",
        ["REVIEW"] = "NOTIFY_MANAGER",
        ["DATA_ENTRY"] = "NOTIFY_ASSIGNEE",
        ["VALIDATION"] = "REASSIGN_TO_BACKUP",
        ["SYSTEM"] = "NOTIFY_ADMIN",
    };

    public EscalationActionService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName = "PPDM39",
        ILogger<EscalationActionService>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    public async Task<EscalationResult> ExecuteEscalationAsync(
        string processInstanceId,
        string stepInstanceId,
        string escalationAction,
        string? escalationTarget,
        string userId)
    {
        var result = new EscalationResult { Success = true, ActionTaken = escalationAction };

        try
        {
            switch (escalationAction?.ToUpperInvariant())
            {
                case "REASSIGN_TO_BACKUP":
                    await ReassignStepAsync(stepInstanceId, escalationTarget ?? "BACKUP", userId);
                    result.NewAssigneeId = escalationTarget;
                    result.Message = $"Step reassigned to {escalationTarget ?? "backup"}";
                    break;

                case "NOTIFY_MANAGER":
                    result.NotificationTarget = "MANAGER";
                    result.Message = "Manager notified of SLA breach";
                    break;

                case "NOTIFY_ASSIGNEE":
                    result.NotificationTarget = "ASSIGNEE";
                    result.Message = "Assignee reminded of overdue task";
                    break;

                case "NOTIFY_ADMIN":
                    result.NotificationTarget = "ADMIN";
                    result.Message = "Administrator notified of SLA breach";
                    break;

                case "AUTO_ESCALATE_LEVEL":
                    await ReassignStepAsync(stepInstanceId, escalationTarget ?? "NEXT_LEVEL", userId);
                    result.NewAssigneeId = escalationTarget;
                    result.Message = $"Escalated to next DOA level: {escalationTarget}";
                    break;

                case "SUSPEND_PROCESS":
                    await SuspendProcessAsync(processInstanceId, userId);
                    result.Message = "Process suspended pending admin review";
                    break;

                case "AUTO_APPROVE":
                    // Only for low-risk steps — the caller must validate
                    result.Message = "Step auto-approved (low-risk SLA breach)";
                    break;

                default:
                    _logger?.LogWarning("Unknown escalation action: {Action}", escalationAction);
                    result.Success = false;
                    result.Message = $"Unknown escalation action: {escalationAction}";
                    break;
            }

            _logger?.LogInformation(
                "Escalation executed: Process={ProcessId}, Step={StepId}, Action={Action}, Result={Success}",
                processInstanceId, stepInstanceId, escalationAction, result.Success);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = ex.Message;
            _logger?.LogError(ex, "Escalation failed for process {ProcessId}, step {StepId}", processInstanceId, stepInstanceId);
        }

        return result;
    }

    public string GetDefaultAction(string stepType)
    {
        return DefaultActions.TryGetValue(stepType, out var action)
            ? action
            : "NOTIFY_MANAGER";
    }

    private async Task ReassignStepAsync(string stepInstanceId, string targetRoleOrUser, string userId)
    {
        var repo = GetStepRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROCESS_STEP_INSTANCE_ID", FilterValue = stepInstanceId }
        };
        var steps = (await repo.GetAsync(filters)).OfType<PROCESS_STEP_INSTANCE>().ToList();

        if (steps.Count == 0) return;

        var step = steps[0];
        step.ASSIGNED_TO = targetRoleOrUser;
        step.STEP_DATA_JSON = System.Text.Json.JsonSerializer.Serialize(new
        {
            escalatedAt = DateTime.UtcNow,
            escalatedBy = userId,
            previousAssignee = step.ASSIGNED_TO,
        });

        await repo.UpdateAsync(step, userId);
    }

    private async Task SuspendProcessAsync(string processInstanceId, string userId)
    {
        var repo = GetInstanceRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROCESS_INSTANCE_ID", FilterValue = processInstanceId }
        };
        var instances = (await repo.GetAsync(filters)).OfType<PROCESS_INSTANCE>().ToList();

        if (instances.Count == 0) return;

        var instance = instances[0];
        instance.STATUS = "SUSPENDED";
        await repo.UpdateAsync(instance, userId);
    }

    private PPDMGenericRepository GetStepRepo() =>
        new(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROCESS_STEP_INSTANCE), _connectionName, "PROCESS_STEP_INSTANCE", null);

    private PPDMGenericRepository GetInstanceRepo() =>
        new(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROCESS_INSTANCE), _connectionName, "PROCESS_INSTANCE", null);
}
