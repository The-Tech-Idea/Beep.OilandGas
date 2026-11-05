using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
public sealed partial class InspectionService : IInspectionService, ICompanyWorkOrderMaterialAcceptanceOwner, ICompanyWorkOrderPersonnelAcceptanceOwner, ICompanyWorkOrderRequirementSource
{
    private readonly IDMEEditor editor;
    private readonly ICommonColumnHandler columns;
    private readonly IPPDM39DefaultsRepository defaults;
    private readonly IPPDMMetadataRepository metadata;
    private readonly WorkOrderDatabaseBinding database;
    private readonly Func<string,string,string,string,Task> requireOrder;
    private readonly Func<string,Task>? requirePermitPrerequisite;
    public InspectionService(IDMEEditor editor,ICommonColumnHandler commonColumnHandler,IPPDM39DefaultsRepository defaults,IPPDMMetadataRepository metadata,
        Func<Task<(CompanyScope Company,string Actor,AssetDatabaseScope Database)>> resolveDatabase,ILogger<InspectionService> logger,Func<string,string,string,string,Task> requireOrder,Func<string,Task>? requirePermitPrerequisite=null)
    {
        this.editor=editor;columns=commonColumnHandler;this.defaults=defaults;this.metadata=metadata;database=new(resolveDatabase);this.requireOrder=requireOrder ?? throw new ArgumentNullException(nameof(requireOrder));this.requirePermitPrerequisite=requirePermitPrerequisite;
    }
    private async Task RequirePrerequisiteAsync(WORK_ORDER_INSPECTION row,string actor)
    {
        if(row.PERMIT_PREREQUISITE_KIND is null)return;
        if(requirePermitPrerequisite is null)throw RefusalException.Forbidden("Current app authority for permit-prerequisite inspection is unavailable.");await requirePermitPrerequisite(actor);
        var source=editor.GetDataSource(await database.RequireAsync(actor));var structure=source.GetEntityStructure(new TheTechIdea.Beep.DataBase.EntityStructure{EntityName="WORK_ORDER_INSPECTION",DatasourceEntityName="WORK_ORDER_INSPECTION"},true);
        if(structure?.Fields is null || structure.Fields.Count(field=>field.FieldName=="PERMIT_PREREQUISITE_KIND")!=1)throw RefusalException.Conflict("The explicit prerequisite inspection column is unavailable; review its existing Beep migration.");await requirePermitPrerequisite(actor);
    }
    private async Task<PPDMGenericRepository> RepoAsync()
    {
        var connection=await database.RequireAsync();
        var source=editor.GetDataSource(connection) ?? throw RefusalException.Conflict("The inspection datasource is unavailable.");
        if(!source.CheckEntityExist("WORK_ORDER_INSPECTION"))throw RefusalException.Conflict("The recorded inspection schema is unavailable. Review its PPDM_CORE Beep migration before using inspection commands.");
        return new(editor,columns,defaults,metadata,typeof(WORK_ORDER_INSPECTION),connection,"WORK_ORDER_INSPECTION");
    }
    private static void RequireApplicabilitySchema(TheTechIdea.Beep.IDataSource source,WORK_ORDER_INSPECTION row)
    {
        if(row.APPLICABLE_STAGE is null)return;
        var shape=source.GetEntityStructure(new TheTechIdea.Beep.DataBase.EntityStructure{EntityName="WORK_ORDER_INSPECTION",DatasourceEntityName="WORK_ORDER_INSPECTION"},true);
        var fields=shape?.Fields?.Where(value=>value.FieldName=="APPLICABLE_STAGE").ToArray();
        if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || !fields[0].AllowDBNull || fields[0].Size1<32)throw RefusalException.Conflict("Review the existing PPDM_CORE Beep Migration for the nullable explicit inspection applicability column before using it.");
    }
    private async Task<List<WORK_ORDER_INSPECTION>> ReadRowsAsync(string order,string field,string actor)
    {
        await database.RequireAsync(actor);await requireOrder(actor,field,order,"READ");
        var repo=await RepoAsync();
        var raw=(await database.ReadAsync(()=>repo.GetAsync([new(){FieldName="WORK_ORDER_ID",Operator="=",FilterValue=order},new(){FieldName="FIELD_ID",Operator="=",FilterValue=field},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}]))).ToList();
        if(raw.Any(r=>r is not WORK_ORDER_INSPECTION))throw RefusalException.Conflict("The inspection provider returned an incompatible record.");
        var rows=raw.Cast<WORK_ORDER_INSPECTION>().ToList();WorkOrderInspectionRules.Require(field,order,rows);
        await database.RequireAsync(actor);await requireOrder(actor,field,order,"READ");return rows;
    }
    // Only an explicit command or versioned reader's complete fresh guard may close this private native read.
    // Public checklist reads retain their independent database/order authorization path.
    private async Task<IReadOnlyList<WORK_ORDER_INSPECTION>> ReadCompleteGuardedRowsAsync(string order,string field,Func<Task> completeGuard)
    {
        await completeGuard();var repository=await RepoAsync();
        var raw=(await repository.GetAsync([new(){FieldName="WORK_ORDER_ID",Operator="=",FilterValue=order},new(){FieldName="FIELD_ID",Operator="=",FilterValue=field},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}])).ToArray();
        if(raw.Length>256 || raw.Any(row=>row is not WORK_ORDER_INSPECTION))throw new InvalidOperationException("Complete original inspection population is unavailable or exceeds its review bound.");
        var rows=raw.Cast<WORK_ORDER_INSPECTION>().Select(CompanyInventorySourceReview.Copy).ToArray();
        await completeGuard();WorkOrderInspectionRules.Require(field,order,rows);
        if(rows.Any(row=>row.APPLICABLE_STAGE is not null))RequireApplicabilitySchema(editor.GetDataSource(await database.RequireAsync())??throw new InvalidOperationException("Original applicability source is unavailable."),rows.First(row=>row.APPLICABLE_STAGE is not null));
        return rows;
    }
    public Task<IReadOnlyList<InspectionCondition>> ReadRequirementProofAsync(FieldSelectionReview context,string workOrderId,Func<Task> requireAuthority)=>ReadRequirementProofCore(context,workOrderId,requireAuthority,false);
    public Task<IReadOnlyList<InspectionCondition>> ReadRequirementProofCoreAsync(FieldSelectionReview context,string workOrderId,Func<Task> requireAuthority)=>ReadRequirementProofCore(context,workOrderId,requireAuthority,true);
    private async Task<IReadOnlyList<InspectionCondition>> ReadRequirementProofCore(FieldSelectionReview context,string workOrderId,Func<Task> requireAuthority,bool owned)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);if(!LeaseSummaryValidator.Identifier(workOrderId) || !LeaseSummaryValidator.Identifier(context.FieldId))throw new ArgumentException("Exact original requirement FIELD/order is required.");
        var source=editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original requirement native source is unavailable.");
        async Task Guard(){if(owned)PPDMDataSourceTransaction.RequireOwned(source,System.Data.IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await requireAuthority();await database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);if(!owned)await requireOrder(context.ActorUserId,context.FieldId!,workOrderId,"WRITE");if(!ReferenceEquals(source,editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original requirement source changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,System.Data.IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);}
        async Task Parent(){await Guard();await requireOrder(context.ActorUserId,context.FieldId!,workOrderId,"WRITE");await Guard();}
        async Task RequireRequirementSchema()
        {
            await Guard();if(!source.CheckEntityExist("WORK_ORDER_INSPECTION"))throw new InvalidOperationException("Explicit versioned inspection schema is unavailable.");var shape=source.GetEntityStructure(new TheTechIdea.Beep.DataBase.EntityStructure{EntityName="WORK_ORDER_INSPECTION",DatasourceEntityName="WORK_ORDER_INSPECTION"},true);var keys=shape?.Fields?.Where(value=>value.IsKey).ToArray();if(keys is not {Length:1} || keys[0].FieldName!="INSPECTION_ID" || keys[0].AllowDBNull)throw new InvalidOperationException("Complete immutable requirement key metadata is unavailable.");
            var definition=await metadata.GetTableMetadataAsync("WORK_ORDER_INSPECTION");var primary=(await metadata.GetPrimaryKeyColumnsAsync("WORK_ORDER_INSPECTION")).ToArray();if(definition is null || definition.EntityTypeName!=typeof(WORK_ORDER_INSPECTION).FullName && definition.EntityTypeName!=nameof(WORK_ORDER_INSPECTION) || definition.PrimaryKeyColumn!="INSPECTION_ID" || primary.Length!=1 || primary[0]!="INSPECTION_ID")throw new InvalidOperationException("Canonical native requirement key/type metadata is unavailable.");await Guard();
        }
        async Task<IReadOnlyList<InspectionCondition>> Read(){if(owned)await Parent();await RequireRequirementSchema();var rows=await ReadCompleteGuardedRowsAsync(workOrderId,context.FieldId!,Guard);if(rows.Count>256)throw new InvalidOperationException("Complete native requirement population exceeds its bounded versioned review capacity.");var result=rows.OrderBy(row=>row.COND_SEQ).Select(WorkOrderInspectionRules.ProjectWithRequirementProof).ToArray();if(owned)await Parent();else await Guard();return result;}
        var first=await Read();var closing=await Read();if(System.Text.Json.JsonSerializer.Serialize(first)!=System.Text.Json.JsonSerializer.Serialize(closing))throw new InvalidOperationException("Complete native requirement/result/creation audit population changed.");return closing;
    }
    public async Task<List<InspectionCondition>> GetChecklistAsync(string instanceId,string fieldId)
    {
        var actor=await database.ActorAsync();return (await ReadRowsAsync(instanceId,fieldId,actor)).OrderBy(r=>r.COND_SEQ).Select(WorkOrderInspectionRules.Project).ToList();
    }
    public async Task<bool> AllConditionsPassedAsync(string instanceId,string fieldId,IEnumerable<string> condTypes)=>WorkOrderWorkflow.RecordedConditionsPassed(await GetChecklistAsync(instanceId,fieldId),condTypes);
    public async Task<InspectionCondition> AddRequirementAsync(string instanceId,AddWorkOrderInspectionRequirementRequest request,string userId)
    {
        var prepared=WorkOrderInspectionRules.PrepareRequirement(request,instanceId,userId,Guid.NewGuid().ToString("N"),DateTime.UtcNow);
        await database.RequireAsync(userId);await requireOrder(userId,request.FieldId,instanceId,"WRITE");await RequirePrerequisiteAsync(prepared,userId);var repo=await RepoAsync();var source=editor.GetDataSource(await database.RequireAsync(userId));RequireApplicabilitySchema(source,prepared);
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            PPDMDataSourceTransaction.RequireOwned(source,System.Data.IsolationLevel.Serializable);
            RequireApplicabilitySchema(source,prepared);
            var rows=await ReadRowsAsync(instanceId,request.FieldId,userId);if(rows.Any(r=>r.COND_SEQ==request.CondSeq))throw RefusalException.Conflict("That inspection sequence already exists on the recorded order.");
            await requireOrder(userId,request.FieldId,instanceId,"WRITE");await database.RequireAsync(userId);await RequirePrerequisiteAsync(prepared,userId);RequireApplicabilitySchema(source,prepared);await repo.InsertAsync(prepared,userId);
            var saved=(await ReadRowsAsync(instanceId,request.FieldId,userId)).SingleOrDefault(r=>r.INSPECTION_ID==prepared.INSPECTION_ID);
            WorkOrderInspectionRules.RequireReadback(prepared,saved);await requireOrder(userId,request.FieldId,instanceId,"WRITE");await database.RequireAsync(userId);await RequirePrerequisiteAsync(prepared,userId);
        });
        var committed=(await ReadRowsAsync(instanceId,request.FieldId,userId)).SingleOrDefault(r=>r.INSPECTION_ID==prepared.INSPECTION_ID);WorkOrderInspectionRules.RequireReadback(prepared,committed);await requireOrder(userId,request.FieldId,instanceId,"WRITE");await database.RequireAsync(userId);await RequirePrerequisiteAsync(prepared,userId);return WorkOrderInspectionRules.Project(committed!);
    }
    public async Task<InspectionCondition> RecordResultAsync(string instanceId,int condSeq,RecordInspectionResultRequest request,string userId)
    {
        if(request is null || condSeq<=0)throw RefusalException.Invalid("Original inspection intent is required.");
        await database.RequireAsync(userId);await requireOrder(userId,request.FieldId,instanceId,"WRITE");var repo=await RepoAsync();var source=editor.GetDataSource(await database.RequireAsync(userId));
        WORK_ORDER_INSPECTION? intended=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            PPDMDataSourceTransaction.RequireOwned(source,System.Data.IsolationLevel.Serializable);
            var row=(await ReadRowsAsync(instanceId,request.FieldId,userId)).SingleOrDefault(r=>r.COND_SEQ==condSeq && r.INSPECTION_ID==request.InspectionId) ?? throw RefusalException.NotFound("The original inspection requirement was not found.");
            if(row.PERMIT_PREREQUISITE_KIND=="PERSONNEL" && request.Result=="PASS")throw RefusalException.Conflict("PERSONNEL PASS requires the reviewed current assignment/policy/credential acceptance command; a generic result cannot establish acceptance.");
            await RequirePrerequisiteAsync(row,userId);intended=WorkOrderInspectionRules.PrepareResult(row,request,userId,DateTime.UtcNow);
            await requireOrder(userId,request.FieldId,instanceId,"WRITE");await database.RequireAsync(userId);await repo.UpdateAsync(intended,userId);
            WorkOrderInspectionRules.RequireReadback(intended,(await ReadRowsAsync(instanceId,request.FieldId,userId)).SingleOrDefault(r=>r.INSPECTION_ID==intended.INSPECTION_ID));
            await requireOrder(userId,request.FieldId,instanceId,"WRITE");await database.RequireAsync(userId);await RequirePrerequisiteAsync(intended,userId);
        });
        var committed=(await ReadRowsAsync(instanceId,request.FieldId,userId)).SingleOrDefault(r=>r.INSPECTION_ID==request.InspectionId);WorkOrderInspectionRules.RequireReadback(intended!,committed);await requireOrder(userId,request.FieldId,instanceId,"WRITE");await database.RequireAsync(userId);await RequirePrerequisiteAsync(intended!,userId);return WorkOrderInspectionRules.Project(committed!);
    }
}
