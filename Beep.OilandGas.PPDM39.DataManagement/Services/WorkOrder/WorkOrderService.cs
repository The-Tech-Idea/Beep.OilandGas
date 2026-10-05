using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;

/// <remarks>
/// OILGAS-CATCH-01: a read that fails reaches the caller — it was caught and answered as "no such work order", an empty
/// list or no steps. A transition the work order's state does not allow, and one naming a work order that does not exist,
/// are refusals in this service's words.
/// </remarks>
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
        _logger.LogInformation("Creating work order {Name} type {Type} for field {Field}",
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

            _logger.LogInformation("Work order {InstanceId} created", instanceId);
            return MapToSummary(entity, instanceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create work order {Name}", request.InstanceName);
            throw;
        }
    }

    // ── READ ─────────────────────────────────────────────────────────────────

    public async Task<WorkOrderDetailModel?> GetByIdAsync(string fieldId, string instanceId)
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

    public async Task<List<WorkOrderSummary>> GetByFieldAsync(
        string fieldId, string? state = null, string? woSubType = null)
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
            MapDynamicToSummary(d, s, WorkOrderRowValues.Str(d, "PROJECT_ID"));
            return s;
        }).ToList();
    }

    // ── TRANSITION ───────────────────────────────────────────────────────────

    public async Task<WorkOrderSummary> TransitionStateAsync(
        string fieldId, string instanceId, string toState,
        string userId, string? notes = null)
    {
        _logger.LogInformation("Transitioning WO {InstanceId} to {State}", instanceId, toState);
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
            if (rows.Count == 0) throw RefusalException.NotFound($"Work order {instanceId} was not found in field {fieldId}.");

            dynamic entity       = rows[0];
            string  currentState = WorkOrderRowValues.Str(entity, "PROJECT_STATUS");
            string  woSubType    = WorkOrderRowValues.Str(entity, "WO_SUBTYPE");

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
                string jurisdiction = WorkOrderRowValues.Str(entity, "JURISDICTION");
                if (string.IsNullOrWhiteSpace(jurisdiction)) jurisdiction = "USA";
                await _inspection.SeedChecklistAsync(instanceId, woSubType, jurisdiction, userId);
            }

            var summary = new WorkOrderSummary();
            MapDynamicToSummary(entity, summary, instanceId);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transition WO {InstanceId} to {State}", instanceId, toState);
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
            _logger.LogInformation("Soft-deleted work order {InstanceId}", instanceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete work order {InstanceId}", instanceId);
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
        var meta       = await _metadata.GetTableMetadataAsync("PROJECT_STEP");
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
        var repo       = BuildRepo(entityType, "PROJECT_STEP");

        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = instanceId },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y"        }
        };
        return (await repo.GetAsync(filters)).Select(r => new WorkOrderStep
        {
            InstanceId  = instanceId,
            StepSeq     = WorkOrderRowValues.Int(r, "STEP_SEQ"),
            StepName    = WorkOrderRowValues.Str(r, "STEP_NAME"),
            StepType    = WorkOrderRowValues.Str(r, "STEP_TYPE"),
            Status      = WorkOrderRowValues.Str(r, "STEP_STATUS"),
            PlannedDate = WorkOrderRowValues.Date(r, "PLAN_DATE"),
            ActualDate  = WorkOrderRowValues.Date(r, "ACTUAL_DATE")
        }).ToList();
    }

    private static void MapDynamicToSummary(dynamic d, WorkOrderSummary s, string instanceId)
    {
        s.InstanceId   = instanceId;
        s.InstanceName = WorkOrderRowValues.Str(d, "PROJECT_NAME");
        s.WoSubType    = WorkOrderRowValues.Str(d, "WO_SUBTYPE");
        s.State        = WorkOrderRowValues.Str(d, "PROJECT_STATUS");
        s.FieldId      = WorkOrderRowValues.Str(d, "FIELD_ID");
        s.EquipmentId  = WorkOrderRowValues.Str(d, "EQUIPMENT_ID");
        s.PlannedStart = WorkOrderRowValues.Date(d, "PLAN_START_DATE");
        s.PlannedEnd   = WorkOrderRowValues.Date(d, "PLAN_END_DATE");
        s.ActualStart  = WorkOrderRowValues.Date(d, "ACTUAL_DATE");
        s.ActualEnd    = WorkOrderRowValues.Date(d, "ACTUAL_END_DATE");
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
            throw RefusalException.Conflict(
                $"Work order in state '{from}' cannot be transitioned to '{to}'.");

        // UNDER_REVIEW only valid for Safety-critical WOs
        if (to == WorkOrderState.UnderReview && woSubType != WorkOrderSubType.Safety)
            throw RefusalException.Conflict(
                "UNDER_REVIEW state is only valid for Safety-Critical work orders [SEMS §250.1917].");
    }
}
