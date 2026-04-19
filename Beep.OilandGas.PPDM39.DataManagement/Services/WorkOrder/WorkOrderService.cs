using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using Serilog;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;

public class WorkOrderService : IWorkOrderService
{
    private readonly IDMEEditor              _editor;
    private readonly ICommonColumnHandler    _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly IInspectionService      _inspection;
    private readonly string                  _connectionName;
    private readonly ILogger<WorkOrderService> _logger;

    public WorkOrderService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        IInspectionService inspection,
        string connectionName,
        ILogger<WorkOrderService> logger)
    {
        _editor              = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults            = defaults;
        _metadata            = metadata;
        _inspection          = inspection;
        _connectionName      = connectionName;
        _logger              = logger;
    }

    // ── CREATE ───────────────────────────────────────────────────────────────

    public async Task<WorkOrderSummary> CreateAsync(CreateWorkOrderRequest request, string userId)
    {
        Log.Information("Creating work order {Name} type {Type} for field {Field}",
            request.InstanceName, request.WoSubType, request.FieldId);
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("PROJECT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = BuildRepo(entityType, "PROJECT");

            var instanceId = Guid.NewGuid().ToString("N").ToUpperInvariant()[..20];
            dynamic entity = Activator.CreateInstance(entityType!)!;
            entity.PROJECT_ID       = instanceId;
            entity.FIELD_ID         = request.FieldId;
            entity.PROJECT_TYPE     = "WORK_ORDER";
            entity.WO_SUBTYPE       = request.WoSubType;
            entity.PROJECT_NAME     = request.InstanceName;
            entity.PROJECT_DESC     = request.Description;
            entity.EQUIPMENT_ID     = request.EquipmentId;
            entity.JURISDICTION     = request.Jurisdiction;
            entity.PROJECT_STATUS   = WorkOrderState.Draft;
            entity.ACTIVE_IND       = "Y";

            await repo.InsertAsync(entity, userId);

            // Seed inspection checklist when WO is created in PLANNED state
            if (request.ProposedStart.HasValue)
                await _inspection.SeedChecklistAsync(
                    instanceId, request.WoSubType, request.Jurisdiction, userId);

            Log.Information("Work order {InstanceId} created", instanceId);
            return MapToSummary(entity, instanceId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create work order {Name}", request.InstanceName);
            throw;
        }
    }

    // ── READ ─────────────────────────────────────────────────────────────────

    public async Task<WorkOrderDetailModel?> GetByIdAsync(string fieldId, string instanceId)
    {
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("PROJECT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = BuildRepo(entityType, "PROJECT");

            var filters = new List<AppFilter>
            {
                new() { FieldName = "PROJECT_ID",   Operator = "=", FilterValue = instanceId },
                new() { FieldName = "FIELD_ID",     Operator = "=", FilterValue = fieldId    },
                new() { FieldName = "PROJECT_TYPE", Operator = "=", FilterValue = "WORK_ORDER"},
                new() { FieldName = "ACTIVE_IND",   Operator = "=", FilterValue = "Y"        }
            };
            var rows = (await repo.GetAsync(filters)).ToList();
            if (rows.Count == 0) return null;

            dynamic row     = rows[0];
            var detail      = new WorkOrderDetailModel();
            MapDynamicToSummary(row, detail, instanceId);

            // Load steps, checklist
            detail.Steps     = await GetStepsAsync(instanceId);
            detail.Checklist = await _inspection.GetChecklistAsync(instanceId);

            return detail;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get work order {InstanceId}", instanceId);
            return null;
        }
    }

    public async Task<List<WorkOrderSummary>> GetByFieldAsync(
        string fieldId, string? state = null, string? woSubType = null)
    {
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("PROJECT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = BuildRepo(entityType, "PROJECT");

            var filters = new List<AppFilter>
            {
                new() { FieldName = "FIELD_ID",     Operator = "=", FilterValue = fieldId      },
                new() { FieldName = "PROJECT_TYPE", Operator = "=", FilterValue = "WORK_ORDER" },
                new() { FieldName = "ACTIVE_IND",   Operator = "=", FilterValue = "Y"          }
            };
            if (!string.IsNullOrWhiteSpace(state))
                filters.Add(new AppFilter { FieldName = "PROJECT_STATUS", Operator = "=", FilterValue = state });
            if (!string.IsNullOrWhiteSpace(woSubType))
                filters.Add(new AppFilter { FieldName = "WO_SUBTYPE", Operator = "=", FilterValue = woSubType });

            var rows = (await repo.GetAsync(filters)).ToList();
            return rows.Select(r =>
            {
                dynamic d = r;
                var s = new WorkOrderSummary();
                MapDynamicToSummary(d, s, GetStr(d, "PROJECT_ID"));
                return s;
            }).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to list work orders for field {FieldId}", fieldId);
            return new();
        }
    }

    // ── TRANSITION ───────────────────────────────────────────────────────────

    public async Task<WorkOrderSummary> TransitionStateAsync(
        string fieldId, string instanceId, string toState,
        string userId, string? notes = null)
    {
        Log.Information("Transitioning WO {InstanceId} to {State}", instanceId, toState);
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("PROJECT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = BuildRepo(entityType, "PROJECT");

            var filters = new List<AppFilter>
            {
                new() { FieldName = "PROJECT_ID",   Operator = "=", FilterValue = instanceId  },
                new() { FieldName = "FIELD_ID",     Operator = "=", FilterValue = fieldId     },
                new() { FieldName = "PROJECT_TYPE", Operator = "=", FilterValue = "WORK_ORDER"},
                new() { FieldName = "ACTIVE_IND",   Operator = "=", FilterValue = "Y"         }
            };
            var rows = (await repo.GetAsync(filters)).ToList();
            if (rows.Count == 0) throw new InvalidOperationException($"Work order {instanceId} not found");

            dynamic entity       = rows[0];
            string  currentState = GetStr(entity, "PROJECT_STATUS");
            string  woSubType    = GetStr(entity, "WO_SUBTYPE");

            ValidateTransition(currentState, toState, woSubType);

            entity.PROJECT_STATUS = toState;
            if (toState == WorkOrderState.InProgress)
                entity.ACTUAL_DATE = DateTime.UtcNow;
            else if (toState == WorkOrderState.Completed)
                entity.ACTUAL_END_DATE = DateTime.UtcNow;

            await repo.UpdateAsync(entity, userId);

            // Seed checklist on PLANNED transition
            if (toState == WorkOrderState.Planned)
            {
                string jurisdiction = GetStr(entity, "JURISDICTION");
                if (string.IsNullOrWhiteSpace(jurisdiction)) jurisdiction = "USA";
                await _inspection.SeedChecklistAsync(instanceId, woSubType, jurisdiction, userId);
            }

            var summary = new WorkOrderSummary();
            MapDynamicToSummary(entity, summary, instanceId);
            return summary;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to transition WO {InstanceId} to {State}", instanceId, toState);
            throw;
        }
    }

    // ── DELETE ───────────────────────────────────────────────────────────────

    public async Task DeleteAsync(string fieldId, string instanceId, string userId)
    {
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("PROJECT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = BuildRepo(entityType, "PROJECT");
            await repo.SoftDeleteAsync(instanceId, userId);
            Log.Information("Soft-deleted work order {InstanceId}", instanceId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete work order {InstanceId}", instanceId);
            throw;
        }
    }

    // ── TRANSITIONS ──────────────────────────────────────────────────────────

    public List<string> GetAvailableTransitions(string currentState, string woSubType)
    {
        // Safety-critical WOs add UNDER_REVIEW before COMPLETED
        bool isSafety = woSubType == WorkOrderSubType.Safety;

        return currentState switch
        {
            WorkOrderState.Draft       => new() { WorkOrderState.Scoped, WorkOrderState.Planned },
            WorkOrderState.Scoped      => new() { WorkOrderState.Planned, WorkOrderState.Cancelled },
            WorkOrderState.Planned     => new() { WorkOrderState.InProgress, WorkOrderState.Cancelled },
            WorkOrderState.InProgress  => isSafety
                ? new() { WorkOrderState.UnderReview, WorkOrderState.Cancelled }
                : new() { WorkOrderState.Completed, WorkOrderState.Cancelled },
            WorkOrderState.UnderReview => new() { WorkOrderState.Completed, WorkOrderState.InProgress },
            _                          => new()
        };
    }

    // ── PRIVATE HELPERS ───────────────────────────────────────────────────────

    private PPDMGenericRepository BuildRepo(Type? entityType, string tableName) =>
        new(_editor, _commonColumnHandler, _defaults, _metadata,
            entityType, _connectionName, tableName);

    private async Task<List<WorkOrderStep>> GetStepsAsync(string instanceId)
    {
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("PROJECT_STEP");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = BuildRepo(entityType, "PROJECT_STEP");

            var filters = new List<AppFilter>
            {
                new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = instanceId },
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y"        }
            };
            return (await repo.GetAsync(filters)).Select(r =>
            {
                dynamic d = r;
                return new WorkOrderStep
                {
                    InstanceId  = instanceId,
                    StepSeq     = GetInt(d, "STEP_SEQ"),
                    StepName    = GetStr(d, "STEP_NAME"),
                    StepType    = GetStr(d, "STEP_TYPE"),
                    Status      = GetStr(d, "STEP_STATUS"),
                    PlannedDate = GetDate(d, "PLAN_DATE"),
                    ActualDate  = GetDate(d, "ACTUAL_DATE")
                };
            }).ToList();
        }
        catch { return new(); }
    }

    private static void MapDynamicToSummary(dynamic d, WorkOrderSummary s, string instanceId)
    {
        s.InstanceId   = instanceId;
        s.InstanceName = GetStr(d, "PROJECT_NAME");
        s.WoSubType    = GetStr(d, "WO_SUBTYPE");
        s.State        = GetStr(d, "PROJECT_STATUS");
        s.FieldId      = GetStr(d, "FIELD_ID");
        s.EquipmentId  = GetStr(d, "EQUIPMENT_ID");
        s.PlannedStart = GetDate(d, "PLAN_START_DATE");
        s.PlannedEnd   = GetDate(d, "PLAN_END_DATE");
        s.ActualStart  = GetDate(d, "ACTUAL_DATE");
        s.ActualEnd    = GetDate(d, "ACTUAL_END_DATE");
    }

    private static WorkOrderSummary MapToSummary(dynamic entity, string instanceId)
    {
        var s = new WorkOrderSummary();
        MapDynamicToSummary(entity, s, instanceId);
        return s;
    }

    private static void ValidateTransition(string from, string to, string woSubType)
    {
        // Completed and Cancelled WOs cannot be transitioned further
        if (from == WorkOrderState.Completed || from == WorkOrderState.Cancelled)
            throw new InvalidOperationException(
                $"Work order in state '{from}' cannot be transitioned to '{to}'.");

        // UNDER_REVIEW only valid for Safety-critical WOs
        if (to == WorkOrderState.UnderReview && woSubType != WorkOrderSubType.Safety)
            throw new InvalidOperationException(
                "UNDER_REVIEW state is only valid for Safety-Critical work orders [SEMS §250.1917].");
    }

    private static string GetStr(dynamic d, string prop)
    {
        try { return (string?)d.GetType().GetProperty(prop)?.GetValue(d) ?? string.Empty; }
        catch { return string.Empty; }
    }

    private static int GetInt(dynamic d, string prop)
    {
        try
        {
            var v = d.GetType().GetProperty(prop)?.GetValue(d);
            return v is int i ? i : v is long l ? (int)l : 0;
        }
        catch { return 0; }
    }

    private static DateTime? GetDate(dynamic d, string prop)
    {
        try
        {
            var v = d.GetType().GetProperty(prop)?.GetValue(d);
            if (v is DateTime dt) return dt;
            if (v is string s && DateTime.TryParse(s, out var p)) return p;
            return null;
        }
        catch { return null; }
    }
}
