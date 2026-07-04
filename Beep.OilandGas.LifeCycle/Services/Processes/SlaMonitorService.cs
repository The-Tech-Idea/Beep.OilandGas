using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Background service that monitors SLA breaches on active process steps.
/// When a step exceeds its SLA_HOURS, triggers escalation via IEscalationActionService.
/// Runs on a configurable interval (default: every 60 seconds).
/// Part of Phase 2 workflow engine enhancement.
/// </summary>
public class SlaMonitorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SlaMonitorService> _logger;
    private readonly TimeSpan _checkInterval;

    // Track which breaches have already been escalated (prevents duplicate escalations)
    private readonly HashSet<string> _alreadyEscalated = new();

    public SlaMonitorService(
        IServiceScopeFactory scopeFactory,
        ILogger<SlaMonitorService>? logger = null,
        TimeSpan? checkInterval = null)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _checkInterval = checkInterval ?? TimeSpan.FromMinutes(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger?.LogInformation("SLA Monitor started. Check interval: {Interval}s", _checkInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckSlaBreachesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unhandled error in SLA monitor loop");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger?.LogInformation("SLA Monitor stopped");
    }

    private async Task CheckSlaBreachesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var editor = scope.ServiceProvider.GetRequiredService<IDMEEditor>();
        var commonCol = scope.ServiceProvider.GetRequiredService<ICommonColumnHandler>();
        var defaults = scope.ServiceProvider.GetRequiredService<IPPDM39DefaultsRepository>();
        var metadata = scope.ServiceProvider.GetRequiredService<IPPDMMetadataRepository>();
        var escalationService = scope.ServiceProvider.GetRequiredService<IEscalationActionService>();
        var connectionName = "PPDM39";

        var repo = new PPDMGenericRepository(
            editor, commonCol, defaults, metadata,
            typeof(PROCESS_STEP_INSTANCE), connectionName, "PROCESS_STEP_INSTANCE", null);

        var filters = new List<AppFilter>
        {
            new() { FieldName = "STATUS", FilterValue = "IN_PROGRESS" },
        };

        var allActive = (await repo.GetAsync(filters))
            .OfType<PROCESS_STEP_INSTANCE>()
            .Where(s => s.SLA_HOURS.HasValue && s.SLA_HOURS > 0 && s.STARTED_DATE.HasValue)
            .ToList();

        foreach (var step in allActive)
        {
            ct.ThrowIfCancellationRequested();

            var elapsed = DateTime.UtcNow - step.STARTED_DATE!.Value;
            var slaWindow = TimeSpan.FromHours(step.SLA_HOURS!.Value);

            // Not yet breached — skip
            if (elapsed < slaWindow)
                continue;

            // Already escalated — skip
            var breachKey = $"{step.PROCESS_STEP_INSTANCE_ID}|SLA";
            if (_alreadyEscalated.Contains(breachKey))
                continue;

            _logger?.LogWarning(
                "SLA breach detected: Step={StepId} ({StepName}), SLA={SlaHours}h, Elapsed={Elapsed:F1}h, Process={ProcessId}",
                step.STEP_NAME, step.PROCESS_STEP_INSTANCE_ID, step.SLA_HOURS, elapsed.TotalHours, step.PROCESS_INSTANCE_ID);

            try
            {
                // Determine escalation action from step data or use default
                var action = ResolveEscalationAction(step, escalationService);

                await escalationService.ExecuteEscalationAsync(
                    step.PROCESS_INSTANCE_ID,
                    step.PROCESS_STEP_INSTANCE_ID,
                    action,
                    step.ASSIGNED_TO,
                    "SYSTEM");

                _alreadyEscalated.Add(breachKey);

                // Log to PROCESS_HISTORY
                var historyRepo = new PPDMGenericRepository(
                    editor, commonCol, defaults, metadata,
                    typeof(PROCESS_HISTORY), connectionName, "PROCESS_HISTORY", null);

                var history = new PROCESS_HISTORY
                {
                    PROCESS_HISTORY_ID = Guid.NewGuid().ToString(),
                    PROCESS_INSTANCE_ID = step.PROCESS_INSTANCE_ID,
                    PROCESS_STEP_INSTANCE_ID = step.PROCESS_STEP_INSTANCE_ID,
                    EVENT_TYPE = "SLA_BREACH",
                    EVENT_DATE = DateTime.UtcNow,
                    USER_ID = "SYSTEM",
                    DETAILS = $"SLA breached after {elapsed.TotalHours:F1}h (limit: {step.SLA_HOURS}h). Action: {action}",
                };

                await historyRepo.InsertAsync(history, "SYSTEM");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to execute escalation for step {StepId}", step.PROCESS_STEP_INSTANCE_ID);
            }
        }
    }

    private static string ResolveEscalationAction(
        PROCESS_STEP_INSTANCE step, IEscalationActionService escalationService)
    {
        // Check step data for custom escalation action
        if (!string.IsNullOrWhiteSpace(step.STEP_DATA_JSON))
        {
            try
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(step.STEP_DATA_JSON);
                if (data?.TryGetValue("escalationAction", out var action) == true &&
                    action?.ToString() is { Length: > 0 } actionStr)
                {
                    return actionStr;
                }
            }
            catch { /* Use default */ }
        }

        // Determine step type from step name or configuration
        var stepType = step.STEP_NAME?.Contains("APPROVAL", StringComparison.OrdinalIgnoreCase) == true
            ? "APPROVAL"
            : step.STEP_NAME?.Contains("REVIEW", StringComparison.OrdinalIgnoreCase) == true
                ? "REVIEW"
                : "DATA_ENTRY";

        return escalationService.GetDefaultAction(stepType);
    }

    public override void Dispose()
    {
        _alreadyEscalated.Clear();
        base.Dispose();
    }
}
