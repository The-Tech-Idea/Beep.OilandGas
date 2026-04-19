# Phase 6 — Scheduling Engine
## ISchedulingService, Calendar-Based WO Scheduling, and Conflict Detection

---

## Interface

```csharp
public interface ISchedulingService
{
    /// <summary>
    /// Schedule a work order to start at or after the proposed start date.
    /// Returns the confirmed start/end dates after conflict detection.
    /// </summary>
    Task<ScheduleResult> ScheduleWorkOrderAsync(
        string instanceId, string equipmentId, DateTime proposedStart, TimeSpan duration, string userId);

    /// <summary>
    /// Check whether an equipment item has any overlapping WOs in the date window.
    /// </summary>
    Task<List<ScheduleConflict>> GetConflictsAsync(
        string equipmentId, DateTime from, DateTime to, string? excludeInstanceId = null);

    /// <summary>
    /// Reschedule an in-PLANNED work order to a new start date.
    /// Fails if date causes conflict.
    /// </summary>
    Task<ScheduleResult> RescheduleAsync(
        string instanceId, DateTime newStart, string userId);

    /// <summary>
    /// Return all planned WOs for a field in a date range (calendar view data).
    /// </summary>
    Task<List<CalendarSlot>> GetFieldCalendarAsync(
        string fieldId, DateTime from, DateTime to);
}

public record ScheduleResult(bool Success, DateTime ConfirmedStart, DateTime ConfirmedEnd, string? ConflictReason);
public record ScheduleConflict(string ConflictingInstanceId, DateTime OverlapStart, DateTime OverlapEnd);
public record CalendarSlot(string InstanceId, string InstanceName, string EquipmentId, DateTime Start, DateTime End, string State);
```

---

## SchedulingService Implementation

```csharp
public class SchedulingService : ISchedulingService
{
    // Constructor omitted — same pattern as other services (IDMEEditor, etc.)

    public async Task<ScheduleResult> ScheduleWorkOrderAsync(
        string instanceId, string equipmentId, DateTime proposedStart, TimeSpan duration, string userId)
    {
        Log.Information("Scheduling WO {InstanceId} for equipment {EquipmentId} starting {Start}",
            instanceId, equipmentId, proposedStart);

        var proposedEnd = proposedStart + duration;
        var conflicts   = await GetConflictsAsync(equipmentId, proposedStart, proposedEnd, instanceId);

        if (conflicts.Count > 0)
        {
            var next = conflicts.Max(c => c.OverlapEnd).AddHours(1);
            Log.Warning("WO {InstanceId} has conflicts; suggested restart {Suggestion}", instanceId, next);
            return new ScheduleResult(false, proposedStart, proposedEnd,
                $"Conflict with {conflicts[0].ConflictingInstanceId} until {conflicts[0].OverlapEnd:g}");
        }

        // Persist to PROJECT_PLAN
        var planRepo  = BuildRepo("PROJECT_PLAN");
        var planEntry = /* create/update PROJECT_PLAN row */;
        await planRepo.InsertAsync(planEntry, userId);

        return new ScheduleResult(true, proposedStart, proposedEnd, null);
    }

    public async Task<List<ScheduleConflict>> GetConflictsAsync(
        string equipmentId, DateTime from, DateTime to, string? excludeInstanceId = null)
    {
        var planRepo = BuildRepo("PROJECT_PLAN");
        var filters  = new List<AppFilter>
        {
            new() { FieldName = "EQUIPMENT_ID", Operator = "=",  FilterValue = equipmentId },
            new() { FieldName = "ACTIVE_IND",   Operator = "=",  FilterValue = "Y"         },
            // Overlap condition: start < to AND end > from
            // Post-filter in C# since AppFilter doesn't support OR or column math
        };

        var plans = await planRepo.GetAsync(filters);

        return plans
            .Select(p => (dynamic)p)
            .Where(p => (DateTime?)p.PROJECT_ID != excludeInstanceId &&
                        ((DateTime)p.PLAN_START_DATE < to) &&
                        ((DateTime)p.PLAN_END_DATE   > from))
            .Select(p => new ScheduleConflict(
                (string)p.PROJECT_ID,
                (DateTime)p.PLAN_START_DATE,
                (DateTime)p.PLAN_END_DATE))
            .ToList();
    }
}
```

---

## Calendar View (Blazor)

The `WorkOrderCalendar.razor` page uses a simple week-grid layout (no external calendar library required for Phase 6; upgrade to a full scheduler library in Phase 10 if needed).

```razor
@page "/ppdm39/workorder/calendar"
@attribute [Authorize]
@inject ApiClient ApiClient

<MudGrid>
    @foreach (var day in _weekDays)
    {
        <MudItem xs="12" md="2">
            <MudText Typo="Typo.h6">@day.ToString("ddd d")</MudText>
            @foreach (var slot in _slots.Where(s => s.Start.Date == day.Date))
            {
                <MudPaper Class="pa-1 mb-1" Elevation="1">
                    <MudText Typo="Typo.body2">@slot.InstanceName</MudText>
                    <MudText Typo="Typo.caption">@slot.EquipmentId</MudText>
                    <ProcessStateChip State="@slot.State" Size="Size.Small" />
                </MudPaper>
            }
        </MudItem>
    }
</MudGrid>
```

---

## Conflict Detection Rules

| Rule | Applies To | Guard Location |
|---|---|---|
| Same equipment, overlapping dates | All WO types | `SchedulingService.GetConflictsAsync` |
| Turnaround locks all equipment in facility during shutdown | `WO-TURNAROUND` | Additional guard in `WorkOrderService.ScheduleWorkOrderAsync` |
| Regulatory inspection date is government-mandated (cannot shift) | `WO-REGULATORY` | Service returns `Success=false` if proposed date differs from mandated date |
