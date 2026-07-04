using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Lightweight event publisher that domain services call when business events occur.
/// Routes events to the BusinessEventTriggerService which evaluates triggers and starts workflows.
/// This completes the circuit: domain action → event → trigger → workflow → tasks.
/// Part of Phase 3 cross-role orchestration (P3-17).
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>Publish an entity was created. Use in domain services after InsertAsync.</summary>
    Task PublishEntityCreatedAsync(string entityType, string entityId, string? fieldId, Dictionary<string, object>? entityFields, string userId);

    /// <summary>Publish an entity's status changed. Use in domain services after UpdateAsync when status field changed.</summary>
    Task PublishStatusChangedAsync(string entityType, string entityId, string? fieldId, string previousStatus, string newStatus, Dictionary<string, object>? entityFields, string userId);

    /// <summary>Publish a field value changed on an entity.</summary>
    Task PublishFieldChangedAsync(string entityType, string entityId, string? fieldId, string fieldName, object? oldValue, object? newValue, string userId);
}

public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IBusinessEventTriggerService _triggerService;
    private readonly ILogger<DomainEventPublisher> _logger;

    public DomainEventPublisher(
        IBusinessEventTriggerService triggerService,
        ILogger<DomainEventPublisher>? logger = null)
    {
        _triggerService = triggerService;
        _logger = logger;
    }

    public async Task PublishEntityCreatedAsync(
        string entityType, string entityId, string? fieldId,
        Dictionary<string, object>? entityFields, string userId)
    {
        var evt = new BusinessEvent
        {
            EventType = "ENTITY_CREATED",
            EntityType = entityType,
            EntityId = entityId,
            FieldId = fieldId,
            ChangedFields = entityFields ?? new(),
            UserId = userId,
            Timestamp = DateTime.UtcNow,
        };

        await FireAsync(evt);
    }

    public async Task PublishStatusChangedAsync(
        string entityType, string entityId, string? fieldId,
        string previousStatus, string newStatus,
        Dictionary<string, object>? entityFields, string userId)
    {
        var evt = new BusinessEvent
        {
            EventType = "STATUS_CHANGED",
            EntityType = entityType,
            EntityId = entityId,
            FieldId = fieldId,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            ChangedFields = entityFields ?? new(),
            UserId = userId,
            Timestamp = DateTime.UtcNow,
        };

        if (entityFields is not null)
        {
            evt.ChangedFields["PreviousStatus"] = previousStatus;
            evt.ChangedFields["NewStatus"] = newStatus;
        }

        await FireAsync(evt);
    }

    public async Task PublishFieldChangedAsync(
        string entityType, string entityId, string? fieldId,
        string fieldName, object? oldValue, object? newValue, string userId)
    {
        var evt = new BusinessEvent
        {
            EventType = "FIELD_CHANGED",
            EntityType = entityType,
            EntityId = entityId,
            FieldId = fieldId,
            ChangedFields = new()
            {
                [fieldName] = newValue ?? string.Empty,
                [$"{fieldName}_old"] = oldValue ?? string.Empty,
            },
            UserId = userId,
            Timestamp = DateTime.UtcNow,
        };

        await FireAsync(evt);
    }

    private async Task FireAsync(BusinessEvent evt)
    {
        try
        {
            var startedInstances = await _triggerService.OnBusinessEventAsync(evt, evt.UserId);

            if (startedInstances.Count > 0)
            {
                _logger?.LogInformation(
                    "Domain event {EventType} on {EntityType}/{EntityId} triggered {Count} workflow(s): {Instances}",
                    evt.EventType, evt.EntityType, evt.EntityId,
                    startedInstances.Count, string.Join(", ", startedInstances));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to fire domain event {EventType} on {EntityType}/{EntityId}",
                evt.EventType, evt.EntityType, evt.EntityId);
        }
    }
}
