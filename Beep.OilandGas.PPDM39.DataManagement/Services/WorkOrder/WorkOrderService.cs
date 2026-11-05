using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core;
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
    private readonly Func<string,string,string?,string?,Task> _requireCreateAccess;
    private readonly WorkOrderDatabaseBinding _database;
    private readonly ILogger<WorkOrderService> _logger;

    public WorkOrderService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        IInspectionService inspection,
        Func<Task<(CompanyScope Company,string Actor,AssetDatabaseScope Database)>> resolveDatabase,
        ILogger<WorkOrderService> logger,
        Func<string,string,string?,string?,Task> requireCreateAccess)
    {
        _editor              = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults            = defaults;
        _metadata            = metadata;
        _inspection          = inspection;
        _database = new(resolveDatabase ?? throw new ArgumentNullException(nameof(resolveDatabase)));
        _logger              = logger;
        _requireCreateAccess=requireCreateAccess ?? throw new ArgumentNullException(nameof(requireCreateAccess));
    }

    // ── CREATE ───────────────────────────────────────────────────────────────

    public async Task<WorkOrderSummary> CreateAsync(CreateWorkOrderRequest request, string userId)
    {
        var id=Guid.NewGuid().ToString("N");
        var prepared=RecordedWorkOrderCreation.Prepare(request,userId,id,DateTime.UtcNow);
        var connection=await _database.RequireAsync(userId);
        await _requireCreateAccess(userId,request.FieldId,request.EquipmentId,request.WellId);
        var headerRepo=await BuildRepoAsync(typeof(Beep.OilandGas.PPDM39.Models.WORK_ORDER),"WORK_ORDER");
        var componentRepo=await BuildRepoAsync(typeof(Beep.OilandGas.PPDM39.Models.WORK_ORDER_COMPONENT),"WORK_ORDER_COMPONENT");
        var fieldRepo=await BuildRepoAsync(typeof(Beep.OilandGas.PPDM39.Models.FIELD),"FIELD");
        var fieldFilters=new List<AppFilter>{new(){FieldName="FIELD_ID",Operator="=",FilterValue=request.FieldId},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}};
        var fields=(await _database.ReadAsync(()=>fieldRepo.GetAsync(fieldFilters))).ToList();
        if(fields.Count!=1 || fields[0] is not Beep.OilandGas.PPDM39.Models.FIELD field || field.FIELD_ID!=request.FieldId)
            throw RefusalException.Conflict("The recorded FIELD reference is unavailable or ambiguous.");
        var typeRepo=await BuildRepoAsync(typeof(Beep.OilandGas.PPDM39.Models.R_WO_TYPE),"R_WO_TYPE");
        var typeFilters=new List<AppFilter>{new(){FieldName="WORK_ORDER_TYPE",Operator="=",FilterValue=request.WoSubType},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}};
        var types=(await _database.ReadAsync(()=>typeRepo.GetAsync(typeFilters))).ToList();
        if(types.Count!=1 || types[0] is not Beep.OilandGas.PPDM39.Models.R_WO_TYPE type || type.WORK_ORDER_TYPE!=request.WoSubType)
            throw RefusalException.Conflict("Choose an existing active recorded work-order type. No type is seeded automatically.");
        PPDMGenericRepository? wellRepo=null;
        List<AppFilter>? wellFilters=null;
        if(!string.IsNullOrWhiteSpace(request.WellId))
        {
            wellRepo=await BuildRepoAsync(typeof(Beep.OilandGas.PPDM39.Models.WELL),"WELL");
            wellFilters=[new(){FieldName="UWI",Operator="=",FilterValue=request.WellId},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}];
        }
        var source=_editor.GetDataSource(connection) ?? throw RefusalException.Conflict("The work-order provider is unavailable.");
        WorkOrderSummary? readback=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            PPDMDataSourceTransaction.RequireOwned(source,System.Data.IsolationLevel.Serializable);
            await _database.RequireAsync(userId);
            await _requireCreateAccess(userId,request.FieldId,request.EquipmentId,request.WellId);
            var currentFields=(await _database.ReadAsync(()=>fieldRepo.GetAsync(fieldFilters))).ToList();
            if(currentFields.Count!=1 || currentFields[0] is not Beep.OilandGas.PPDM39.Models.FIELD currentField || currentField.FIELD_ID!=request.FieldId)
                throw RefusalException.Conflict("The recorded FIELD changed before creation.");
            var currentTypes=(await _database.ReadAsync(()=>typeRepo.GetAsync(typeFilters))).ToList();
            if(currentTypes.Count!=1 || currentTypes[0] is not Beep.OilandGas.PPDM39.Models.R_WO_TYPE currentType || currentType.WORK_ORDER_TYPE!=request.WoSubType)
                throw RefusalException.Conflict("The recorded work-order type changed before creation.");
            if(wellRepo is not null)
            {
                var wells=(await _database.ReadAsync(()=>wellRepo.GetAsync(wellFilters!))).ToList();
                RecordedWorkOrderCreation.RequireWellReference(request.WellId,wells);
            }
            var recorded=await ReadRecordedAsync(request.FieldId,null);
            if(recorded.Any(row=>row.InstanceName==request.WorkOrderNumber))
                throw RefusalException.Conflict("The recorded order number is already in use in this FIELD. Review before creating another header.");
            await _database.RequireAsync(userId);
            await headerRepo.InsertAsync(prepared.Header,userId);
            await _database.RequireAsync(userId);
            await _requireCreateAccess(userId,request.FieldId,request.EquipmentId,request.WellId);
            await componentRepo.InsertAsync(prepared.Component,userId);
            readback=await GetByIdAsync(request.FieldId,id);
            RecordedWorkOrderCreation.RequireReadback(request,id,readback);
            await _database.RequireAsync(userId);
            await _requireCreateAccess(userId,request.FieldId,request.EquipmentId,request.WellId);
        });
        await _database.RequireAsync(userId);
        await _requireCreateAccess(userId,request.FieldId,request.EquipmentId,request.WellId);
        var committed=await GetByIdAsync(request.FieldId,id);
        RecordedWorkOrderCreation.RequireReadback(request,id,committed);
        return committed!;
    }

    // ── READ ─────────────────────────────────────────────────────────────────

    public async Task<WorkOrderDetailModel?> GetByIdAsync(string fieldId, string instanceId)
    {
        if(string.IsNullOrWhiteSpace(instanceId))throw RefusalException.Invalid("A recorded work-order identity is required.");
        var rows=await ReadRecordedAsync(fieldId,instanceId);
        if(rows.Count==0)return null;
        var row=rows.Single();
        return new WorkOrderDetailModel{InstanceId=row.InstanceId,InstanceName=row.InstanceName,FieldId=row.FieldId,
            WoSubType=row.WoSubType,State=row.State,Source=row.Source,EvidenceUnavailableReason=row.EvidenceUnavailableReason,
            Components=row.Components,RequestDate=row.RequestDate,DueDate=row.DueDate,ActualEnd=row.ActualEnd,Instructions=row.Instructions};
    }

    public async Task<List<WorkOrderSummary>> GetByFieldAsync(string fieldId,string? state=null,string? woSubType=null)
    {
        var rows=await ReadRecordedAsync(fieldId,null);
        if(!string.IsNullOrWhiteSpace(woSubType))rows=rows.Where(x=>x.WoSubType==woSubType).ToList();
        if(!string.IsNullOrWhiteSpace(state))throw RefusalException.Conflict("Recorded work-order headers do not establish workflow state. State filtering is unavailable until the workflow relationship is confirmed.");
        return rows;
    }

    public async Task<List<WorkOrderSummary>> GetByFieldSampleAsync(string fieldId, int limit, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fieldId) || limit is < 1 or > 20)
            throw RefusalException.Invalid("Choose a FIELD and a sample limit between one and twenty.");
        cancellationToken.ThrowIfCancellationRequested();
        var connection = await _database.RequireAsync();
        var source = _editor.GetDataSource(connection) ?? throw RefusalException.Conflict("The work-order provider is unavailable.");
        var filters = new List<AppFilter> { new() { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } };
        var page = await _database.ReadAsync(() => PPDMDataSourceTransaction.WithSourceAsync(source,
            () => Task.FromResult(source.GetEntity("WORK_ORDER_COMPONENT", filters, 1, limit + 1))));
        cancellationToken.ThrowIfCancellationRequested();
        if (page is null || page.Data is null || page.PageNumber != 1 || page.PageSize != limit + 1 ||
            source.ErrorObject?.Flag == TheTechIdea.Beep.ConfigUtil.Errors.Failed)
            throw RefusalException.Conflict("The provider did not confirm a bounded work-order sample.");
        var repo = new WorkOrderSampleRepository(_editor, _commonColumnHandler, _defaults, _metadata, connection);
        var components = repo.Convert(page.Data).ToList();
        if (components.Count > limit + 1) throw RefusalException.Conflict("The provider exceeded the work-order sample bound.");
        RecordedWorkOrderRows.ValidateComponents(fieldId, components);
        var result = new List<WorkOrderSummary>();
        foreach (var id in components.Take(limit).Cast<Beep.OilandGas.PPDM39.Models.WORK_ORDER_COMPONENT>()
                     .Select(row => row.WORK_ORDER_ID).Distinct(StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var rows = await ReadRecordedAsync(fieldId, id);
            if (source.ErrorObject?.Flag == TheTechIdea.Beep.ConfigUtil.Errors.Failed || rows.Count != 1 || rows[0].InstanceId != id)
                throw RefusalException.Conflict("A sampled order is unavailable or changed.");
            result.Add(rows[0]);
        }
        await _database.RequireAsync();
        return result;
    }

    private sealed class WorkOrderSampleRepository(IDMEEditor editor, ICommonColumnHandler columns, IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata, string connection) : PPDMGenericRepository(editor, columns, defaults, metadata,
            typeof(Beep.OilandGas.PPDM39.Models.WORK_ORDER_COMPONENT), connection, "WORK_ORDER_COMPONENT",
            requiredReadColumns: ["WORK_ORDER_ID", "COMPONENT_ID", "FIELD_ID", "ACTIVE_IND"])
    {
        public IEnumerable<object> Convert(object data) => ConvertToTypedList(data, typeof(Beep.OilandGas.PPDM39.Models.WORK_ORDER_COMPONENT));
    }

    private async Task<List<WorkOrderSummary>> ReadRecordedAsync(string field,string? id)
    {
        if(string.IsNullOrWhiteSpace(field))throw RefusalException.Invalid("A confirmed FIELD is required.");
        var componentsRepo=await BuildRepoAsync(typeof(Beep.OilandGas.PPDM39.Models.WORK_ORDER_COMPONENT),"WORK_ORDER_COMPONENT", ["WORK_ORDER_ID", "COMPONENT_ID", "FIELD_ID", "ACTIVE_IND"]);
        var filters=new List<AppFilter>{new(){FieldName="FIELD_ID",Operator="=",FilterValue=field},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}};
        if(id is not null)filters.Add(new(){FieldName="WORK_ORDER_ID",Operator="=",FilterValue=id});
        var components=(await _database.ReadAsync(()=>componentsRepo.GetAsync(filters))).ToList();
        RecordedWorkOrderRows.ValidateComponents(field,components);
        var headersRepo=await BuildRepoAsync(typeof(Beep.OilandGas.PPDM39.Models.WORK_ORDER),"WORK_ORDER", ["WORK_ORDER_ID", "ACTIVE_IND"]);
        var headers=new List<object>();
        foreach(var key in components.Cast<Beep.OilandGas.PPDM39.Models.WORK_ORDER_COMPONENT>().Select(x=>x.WORK_ORDER_ID).Distinct(StringComparer.Ordinal))
        {
            var headerFilters=new List<AppFilter>{new(){FieldName="WORK_ORDER_ID",Operator="=",FilterValue=key},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}};
            var found=(await _database.ReadAsync(()=>headersRepo.GetAsync(headerFilters))).ToList();
            if(found.Count!=1)throw RefusalException.Conflict("A recorded FIELD component has no unique active work-order header.");
            headers.AddRange(found);
        }
        return RecordedWorkOrderRows.Project(field,components,headers);
    }

    // ── TRANSITION ───────────────────────────────────────────────────────────

    public async Task<WorkOrderSummary> TransitionStateAsync(
        string fieldId,string instanceId,string toState,string userId,string? notes=null)
    {
        await _database.RequireAsync(userId);
        if(string.IsNullOrWhiteSpace(fieldId) || string.IsNullOrWhiteSpace(instanceId) || string.IsNullOrWhiteSpace(toState))throw RefusalException.Invalid("The original FIELD/order and explicit transition are required.");
        throw RefusalException.Conflict("A native work-order header does not establish operational state. Use the controlled execution, verification and return workflow with its original approved evidence.");
    }

    // ── DELETE ───────────────────────────────────────────────────────────────

    public async Task DeleteAsync(string fieldId, string instanceId, string userId)
    {
        try
        {
            var meta       = await _metadata.GetTableMetadataAsync("PROJECT");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            var repo       = await BuildRepoAsync(entityType, "PROJECT");
            await _database.RequireAsync(userId);
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

    public List<string> GetAvailableTransitions(string currentState, string woSubType) =>
        WorkOrderWorkflow.Available(currentState, woSubType);

    // ── PRIVATE HELPERS ───────────────────────────────────────────────────────

    private async Task<PPDMGenericRepository> BuildRepoAsync(Type? entityType, string tableName, IReadOnlyList<string>? requiredReadColumns = null)
    {
        var connection = await _database.RequireAsync();
        return new(_editor, _commonColumnHandler, _defaults, _metadata, entityType, connection, tableName, requiredReadColumns: requiredReadColumns);
    }

    private async Task<List<WorkOrderStep>> GetStepsAsync(string instanceId)
    {
        var meta       = await _metadata.GetTableMetadataAsync("PROJECT_STEP");
        var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
        var repo       = await BuildRepoAsync(entityType, "PROJECT_STEP");

        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROJECT_ID", Operator = "=", FilterValue = instanceId },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y"        }
        };
        return (await _database.ReadAsync(() => repo.GetAsync(filters))).Select(r => new WorkOrderStep
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
        if (!WorkOrderWorkflow.Available(from, woSubType).Contains(to, StringComparer.Ordinal))
            throw RefusalException.Conflict("The requested transition is not available from the recorded work-order state and subtype.");
    }
}
