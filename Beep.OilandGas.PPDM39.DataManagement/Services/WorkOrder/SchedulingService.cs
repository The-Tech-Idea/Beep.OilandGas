using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;

/// <remarks>
/// OILGAS-CATCH-01: a failure reaches the caller. Saving a plan was caught and the schedule still answered as made; the
/// conflict check answered "no conflicts" on a failure, so equipment could be double-booked; rescheduling answered the
/// exception's text; and the calendar answered empty.
/// </remarks>
public class SchedulingService : ISchedulingService
{
    private readonly IDMEEditor                _editor;
    private readonly ICommonColumnHandler      _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository   _metadata;
    private readonly string                    _connectionName;
    private readonly ILogger<SchedulingService> _logger;

    public SchedulingService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<SchedulingService> logger)
    {
        _editor              = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults            = defaults;
        _metadata            = metadata;
        _connectionName      = connectionName;
        _logger              = logger;
    }

    public async Task<ScheduleResult> ScheduleWorkOrderAsync(
        string instanceId, string equipmentId,
        DateTime proposedStart, TimeSpan duration, string userId)
    {
        _logger.LogInformation("Scheduling WO {InstanceId} equipment {Equip} start {Start}",
            instanceId, equipmentId, proposedStart);

        var proposedEnd = proposedStart + duration;
        var conflicts   = await GetConflictsAsync(equipmentId, proposedStart, proposedEnd, instanceId);

        if (conflicts.Count > 0)
        {
            var suggestion = conflicts.Max(c => c.OverlapEnd).AddHours(1);
            var reason     = $"Conflict with {conflicts[0].ConflictingInstanceId} until {conflicts[0].OverlapEnd:g}";
            _logger.LogWarning("WO {InstanceId} conflict — suggested restart {Next}", instanceId, suggestion);
            return new ScheduleResult(false, proposedStart, proposedEnd, reason);
        }

        // Persist plan to PROJECT_PLAN
        var meta       = await _metadata.GetTableMetadataAsync("PROJECT_PLAN");
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
        var repo       = BuildRepo(entityType, "PROJECT_PLAN");

        // Remove any existing plan for this WO
        var existing = (await repo.GetAsync(new List<AppFilter>
        {
            new() { FieldName = "PROJECT_ID",  Operator = "=", FilterValue = instanceId  },
            new() { FieldName = "ACTIVE_IND",  Operator = "=", FilterValue = "Y"         }
        })).ToList();

        foreach (var old in existing)
        {
            dynamic d = old;
            string  planId = WorkOrderRowValues.Str(d, "PLAN_ID");
            await repo.SoftDeleteAsync(planId, userId);
        }

        dynamic plan = Activator.CreateInstance(entityType!)!;
        plan.PLAN_ID         = Guid.NewGuid().ToString("N").ToUpperInvariant()[..20];
        plan.PROJECT_ID      = instanceId;
        plan.EQUIPMENT_ID    = equipmentId;
        plan.PLAN_START_DATE = proposedStart;
        plan.PLAN_END_DATE   = proposedEnd;
        plan.ACTIVE_IND      = "Y";
        await repo.InsertAsync(plan, userId);

        _logger.LogInformation("WO {InstanceId} scheduled {Start}–{End}", instanceId, proposedStart, proposedEnd);

        return new ScheduleResult(true, proposedStart, proposedEnd, null);
    }

    public async Task<List<ScheduleConflict>> GetConflictsAsync(
        string equipmentId, DateTime from, DateTime to,
        string? excludeInstanceId = null)
    {
        var meta       = await _metadata.GetTableMetadataAsync("PROJECT_PLAN");
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
        var repo       = BuildRepo(entityType, "PROJECT_PLAN");

        var filters = new List<AppFilter>
        {
            new() { FieldName = "EQUIPMENT_ID", Operator = "=", FilterValue = equipmentId },
            new() { FieldName = "ACTIVE_IND",   Operator = "=", FilterValue = "Y"         }
        };

        var plans = (await repo.GetAsync(filters)).ToList();

        return plans
            .Select(p => (dynamic)p)
            .Where(p =>
            {
                var projId = WorkOrderRowValues.Str(p, "PROJECT_ID");
                if (!string.IsNullOrEmpty(excludeInstanceId) && projId == excludeInstanceId) return false;
                var start  = WorkOrderRowValues.Date(p, "PLAN_START_DATE");
                var end    = WorkOrderRowValues.Date(p, "PLAN_END_DATE");
                return start.HasValue && end.HasValue && start.Value < to && end.Value > from;
            })
            .Select(p => new ScheduleConflict(
                WorkOrderRowValues.Str(p, "PROJECT_ID"),
                WorkOrderRowValues.Date(p, "PLAN_START_DATE") ?? from,
                WorkOrderRowValues.Date(p, "PLAN_END_DATE")   ?? to))
            .ToList();
    }

    public async Task<ScheduleResult> RescheduleAsync(
        string instanceId, DateTime newStart, string userId)
    {
        var meta       = await _metadata.GetTableMetadataAsync("PROJECT_PLAN");
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
        var repo       = BuildRepo(entityType, "PROJECT_PLAN");

        var existing = (await repo.GetAsync(new List<AppFilter>
        {
            new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = instanceId },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y"        }
        })).ToList();

        if (existing.Count == 0)
            return new ScheduleResult(false, newStart, newStart, "No existing plan found");

        dynamic plan      = existing[0];
        var     oldStart  = WorkOrderRowValues.Date(plan, "PLAN_START_DATE") ?? DateTime.UtcNow;
        var     oldEnd    = WorkOrderRowValues.Date(plan, "PLAN_END_DATE")   ?? DateTime.UtcNow;
        var     duration  = oldEnd - oldStart;
        var     equipId   = WorkOrderRowValues.Str(plan, "EQUIPMENT_ID");

        var conflicts = await GetConflictsAsync(equipId, newStart, newStart + duration, instanceId);
        if (conflicts.Count > 0)
            return new ScheduleResult(false, newStart, newStart + duration,
                $"Conflict with {conflicts[0].ConflictingInstanceId}");

        plan.PLAN_START_DATE = newStart;
        plan.PLAN_END_DATE   = newStart + duration;
        await repo.UpdateAsync(plan, userId);

        return new ScheduleResult(true, newStart, newStart + duration, null);
    }

    public async Task<List<CalendarSlot>> GetFieldCalendarAsync(
        string fieldId, DateTime from, DateTime to)
    {
        // Join PROJECT_PLAN + PROJECT in C# to get state and name
        var planMeta       = await _metadata.GetTableMetadataAsync("PROJECT_PLAN");
        var planEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{planMeta.EntityTypeName}");
        var planRepo       = BuildRepo(planEntityType, "PROJECT_PLAN");

        var projMeta       = await _metadata.GetTableMetadataAsync("PROJECT");
        var projEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{projMeta.EntityTypeName}");
        var projRepo       = BuildRepo(projEntityType, "PROJECT");

        // Get all plans in range for the field
        var projFilters = new List<AppFilter>
        {
            new() { FieldName = "FIELD_ID",     Operator = "=", FilterValue = fieldId      },
            new() { FieldName = "PROJECT_TYPE", Operator = "=", FilterValue = "WORK_ORDER" },
            new() { FieldName = "ACTIVE_IND",   Operator = "=", FilterValue = "Y"          }
        };
        var projects = (await projRepo.GetAsync(projFilters)).ToList();

        var result  = new List<CalendarSlot>();
        foreach (var proj in projects)
        {
            dynamic p = proj;
            var instanceId = WorkOrderRowValues.Str(p, "PROJECT_ID");
            var planFilters = new List<AppFilter>
            {
                new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = instanceId },
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y"        }
            };
            var plans = (await planRepo.GetAsync(planFilters)).ToList();
            foreach (var plan in plans)
            {
                dynamic pl   = plan;
                var     start = WorkOrderRowValues.Date(pl, "PLAN_START_DATE");
                var     end   = WorkOrderRowValues.Date(pl, "PLAN_END_DATE");
                if (!start.HasValue || !end.HasValue) continue;
                // Include only if overlapping with the requested range
                if (start.Value > to || end.Value < from) continue;

                result.Add(new CalendarSlot(
                    instanceId,
                    WorkOrderRowValues.Str(p, "PROJECT_NAME"),
                    WorkOrderRowValues.Str(pl, "EQUIPMENT_ID"),
                    start.Value,
                    end.Value,
                    WorkOrderRowValues.Str(p, "PROJECT_STATUS")));
            }
        }
        return result;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private PPDMGenericRepository BuildRepo(Type? entityType, string tableName) =>
        new(_editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, tableName);
}
