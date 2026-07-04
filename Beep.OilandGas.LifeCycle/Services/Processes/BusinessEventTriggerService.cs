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
/// Listens for business events and automatically triggers workflows when conditions match.
/// Example: PDEN_VOL_SUMMARY posted → start Production→Revenue recognition workflow.
/// Part of Phase 3 cross-role orchestration.
/// </summary>
public interface IBusinessEventTriggerService
{
    /// <summary>
    /// Register a new trigger.
    /// </summary>
    Task<BUSINESS_EVENT_TRIGGER> RegisterTriggerAsync(BUSINESS_EVENT_TRIGGER trigger, string userId);

    /// <summary>
    /// Called by domain services when a business event occurs.
    /// Evaluates all matching triggers and starts workflows.
    /// Returns the process instance IDs that were started.
    /// </summary>
    Task<List<string>> OnBusinessEventAsync(BusinessEvent eventData, string userId);

    /// <summary>
    /// Get all active triggers for an entity type.
    /// </summary>
    Task<List<BUSINESS_EVENT_TRIGGER>> GetTriggersForEntityAsync(string entityType);
}

public class BusinessEvent
{
    public string EventType { get; set; } = "STATUS_CHANGED";
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? FieldId { get; set; }
    public Dictionary<string, object> ChangedFields { get; set; } = new();
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
    public string UserId { get; set; } = "SYSTEM";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class BusinessEventTriggerService : IBusinessEventTriggerService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<BusinessEventTriggerService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public BusinessEventTriggerService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<BusinessEventTriggerService>? logger,
        IServiceProvider serviceProvider)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<BUSINESS_EVENT_TRIGGER> RegisterTriggerAsync(
        BUSINESS_EVENT_TRIGGER trigger, string userId)
    {
        var repo = GetRepo();
        await repo.InsertAsync(trigger, userId);
        _logger?.LogInformation("Registered business event trigger: {TriggerName} for {EntityType}",
            trigger.TRIGGER_NAME, trigger.ENTITY_TYPE);
        return trigger;
    }

    public async Task<List<string>> OnBusinessEventAsync(BusinessEvent eventData, string userId)
    {
        var startedInstances = new List<string>();
        var triggers = await GetTriggersForEntityAsync(eventData.EntityType);

        foreach (var trigger in triggers.Where(t =>
            string.Equals(t.IS_ACTIVE, "Y", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(t.EVENT_TYPE, eventData.EventType, StringComparison.OrdinalIgnoreCase)))
        {
            if (!MatchesCondition(trigger, eventData))
                continue;

            try
            {
                var processService = _serviceProvider.GetService(typeof(IProcessService)) as IProcessService;
                if (processService is null)
                {
                    _logger?.LogWarning("IProcessService not available for event trigger {TriggerId}", trigger.TRIGGER_ID);
                    continue;
                }

                var instance = await processService.StartProcessAsync(
                    trigger.TARGET_PROCESS_DEF_ID,
                    eventData.EntityId,
                    eventData.EntityType,
                    eventData.FieldId ?? string.Empty,
                    userId);

                startedInstances.Add(instance.InstanceId);

                _logger?.LogInformation(
                    "Business event trigger fired: {TriggerName} → Process {ProcessId}, Instance {InstanceId}",
                    trigger.TRIGGER_NAME, trigger.TARGET_PROCESS_DEF_ID, instance.InstanceId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to fire trigger {TriggerId} for event on {EntityType}/{EntityId}",
                    trigger.TRIGGER_ID, eventData.EntityType, eventData.EntityId);
            }
        }

        return startedInstances;
    }

    public async Task<List<BUSINESS_EVENT_TRIGGER>> GetTriggersForEntityAsync(string entityType)
    {
        var repo = GetRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "ENTITY_TYPE", FilterValue = entityType },
            new() { FieldName = "IS_ACTIVE", FilterValue = "Y" },
        };
        var results = await repo.GetAsync(filters);
        return results.OfType<BUSINESS_EVENT_TRIGGER>()
            .OrderBy(t => t.PRIORITY)
            .ToList();
    }

    private static bool MatchesCondition(BUSINESS_EVENT_TRIGGER trigger, BusinessEvent eventData)
    {
        if (string.IsNullOrWhiteSpace(trigger.CONDITION_EXPRESSION))
            return true;

        var expr = trigger.CONDITION_EXPRESSION.Trim();

        // Handle STATUS_CHANGED conditions
        if (expr.StartsWith("NewStatus", StringComparison.OrdinalIgnoreCase))
        {
            var parts = expr.Split("==", 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                var expectedStatus = parts[1].Trim('\'', '"');
                return string.Equals(eventData.NewStatus, expectedStatus, StringComparison.OrdinalIgnoreCase);
            }
        }

        if (expr.StartsWith("PreviousStatus", StringComparison.OrdinalIgnoreCase))
        {
            var parts = expr.Split("==", 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                var expectedStatus = parts[1].Trim('\'', '"');
                return string.Equals(eventData.PreviousStatus, expectedStatus, StringComparison.OrdinalIgnoreCase);
            }
        }

        // Default: trigger fires
        return true;
    }

    private PPDMGenericRepository GetRepo() =>
        new(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(BUSINESS_EVENT_TRIGGER), _connectionName, "BUSINESS_EVENT_TRIGGER", null);
}
