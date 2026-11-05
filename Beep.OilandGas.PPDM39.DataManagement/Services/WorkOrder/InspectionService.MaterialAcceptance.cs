using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
public sealed partial class InspectionService
{
    public async Task<CompanyWorkOrderMaterialAcceptanceRecovery> RecoverMaterialAcceptanceReferenceAsync(FieldSelectionReview context,CompanyWorkOrderMaterialAcceptanceReference reference,Func<Task> requireAuthority)
    {
        reference.Require();FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);if(reference.Permit.Task.FieldId!=context.FieldId)throw new UnauthorizedAccessException("Original material recovery FIELD differs.");var task=reference.Permit.Task;
        async Task Guard(){await requireAuthority();await database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);await requireOrder(context.ActorUserId,task.FieldId,task.WorkOrderId,"WRITE");if(requirePermitPrerequisite is null)throw new UnauthorizedAccessException("Current independent prerequisite authority is required.");await requirePermitPrerequisite(context.ActorUserId);}
        await Guard();var raw=(await ReadRowsAsync(task.WorkOrderId,task.FieldId,context.ActorUserId)).SingleOrDefault(row=>row.INSPECTION_ID==reference.InspectionId)??throw new InvalidOperationException("Original accepted material inspection is unavailable.");
        if(raw.FIELD_ID!=context.FieldId || raw.WORK_ORDER_ID!=task.WorkOrderId || raw.INSPECTOR!=context.ActorUserId)throw new UnauthorizedAccessException("Original acceptance is private to its recorded reviewer/FIELD/order.");await Guard();var command=CompanyWorkOrderMaterialAcceptanceCommand.Read(raw);var envelope=new CompanyWorkOrderMaterialAcceptanceRecovery(command,raw);envelope.Require(context.Company,context.ActorUserId,reference);
        var recorded=await RecordMaterialAcceptanceAsync(command,Guard,_=>throw new InvalidOperationException("Private reference recovery cannot record fresh acceptance"),true);await Guard();if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(raw))throw new InvalidOperationException("Complete original accepted material record changed during reference recovery.");var result=new CompanyWorkOrderMaterialAcceptanceRecovery(command,recorded);result.Require(context.Company,context.ActorUserId,reference);return result;
    }
    public async Task<WORK_ORDER_INSPECTION> RecordMaterialAcceptanceAsync(CompanyWorkOrderMaterialAcceptanceCommand original,Func<Task> requireAuthority,Func<WORK_ORDER_INSPECTION?,Task> requireCurrentProof,bool readOnly=false,Func<WORK_ORDER_INSPECTION?,Task>? requireCoreProof=null)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(requireCurrentProof);var context=command.Review.PermitReview;var actor=context.Context.ActorUserId;var task=command.Review.Query.Permit.Task;var target=context.CurrentSource.InspectionDatabase;
        await database.RequireContextAsync(context.Context.Company,actor,target);var source=editor.GetDataSource(target.ConnectionName)??throw new InvalidOperationException("Original material inspection source is unavailable.");
        async Task Guard(bool owned=false)
        {
            await requireAuthority();await database.RequireContextAsync(context.Context.Company,actor,target);await requireOrder(actor,task.FieldId,task.WorkOrderId,"WRITE");if(requirePermitPrerequisite is null)throw new UnauthorizedAccessException("Current material prerequisite reviewer authority is required.");await requirePermitPrerequisite(actor);
            if(!ReferenceEquals(source,editor.GetDataSource(target.ConnectionName)) || !source.CheckEntityExist("WORK_ORDER_INSPECTION"))throw new InvalidOperationException("Original material inspection native target changed.");
            var shape=source.GetEntityStructure(new EntityStructure{EntityName="WORK_ORDER_INSPECTION",DatasourceEntityName="WORK_ORDER_INSPECTION"},true);var keys=shape?.Fields?.Where(field=>field.IsKey).ToArray();if(keys is not {Length:1} || keys[0].FieldName!="INSPECTION_ID" || keys[0].AllowDBNull)throw new InvalidOperationException("Original material inspection key metadata is unavailable.");
            foreach(var (name,size) in new[]{("MATERIAL_ACCEPTANCE_DATA_JSON",CompanyWorkOrderMaterialAcceptanceCommand.MaximumBodyLength),("MATERIAL_ACCEPTANCE_REVISION",64)}){var fields=shape!.Fields.Where(field=>field.FieldName==name).ToArray();if(fields.Length!=1 || fields[0].Fieldtype!=typeof(string).FullName || !fields[0].AllowDBNull || fields[0].Size1<size)throw new InvalidOperationException("Review the existing PPDM_CORE Beep manifest for nullable bounded material acceptance proof columns.");}
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
        }
        async Task<WORK_ORDER_INSPECTION> Read(bool owned=false)
        {
            await Guard(owned);var rows=await ReadRowsAsync(task.WorkOrderId,task.FieldId,actor);var row=rows.SingleOrDefault(row=>row.INSPECTION_ID==command.InspectionId)??throw new InvalidOperationException("Original material inspection is unavailable.");await Guard(owned);return CompanyInventorySourceReview.Copy(row);
        }
        WORK_ORDER_INSPECTION? Existing(WORK_ORDER_INSPECTION row)
        {
            if(row.MATERIAL_ACCEPTANCE_DATA_JSON is null){if(row.MATERIAL_ACCEPTANCE_REVISION is not null || row.RESULT_STATUS!="")throw new InvalidOperationException("Another result cannot be adopted as this retained material acceptance.");return null;}
            // Check the public recorded reviewer before deserializing the private retained command.
            if(row.INSPECTOR!=actor)throw new UnauthorizedAccessException("Only the original material reviewer may recover its private command evidence.");command.RequireRecorded(row);return row;
        }
        await Guard();var first=await Read();var recorded=Existing(first);if(readOnly || recorded is not null)
        {
            if(recorded is null)throw new InvalidOperationException("Original material acceptance is not recorded; retain its intent before retrying.");var closing=Existing(await Read());if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Complete original recorded material acceptance changed during recovery.");return recorded;
        }
        async Task CorePermit()
        {
            await Guard(true);if(!source.CheckEntityExist("WORK_ORDER_EXECUTION_PERMIT"))throw new InvalidOperationException("Original local pending permit schema is unavailable.");var repo=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_EXECUTION_PERMIT),target.ConnectionName,"WORK_ORDER_EXECUTION_PERMIT");var rows=(await repo.GetAsync([new AppFilter{FieldName="PERMIT_ID",Operator="=",FilterValue=context.Proposal.PermitId()}])).ToArray();
            if(rows.Length!=1 || rows[0] is not WORK_ORDER_EXECUTION_PERMIT row || ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(row))!=context.RecordedRevision)throw new InvalidOperationException("Complete original local pending permit changed inside material acceptance.");context.Proposal.RequireRecorded(row);await Guard(true);
        }
        await requireCurrentProof(null);await Guard();
        WORK_ORDER_INSPECTION? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);var pending=await Read(true);confirmed=Existing(pending);if(confirmed is not null)return;
            await CorePermit();await (requireCoreProof??requireCurrentProof)(null);await Guard(true);var row=command.Prepare(pending,DateTime.UtcNow);
            var writer=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_INSPECTION),target.ConnectionName,"WORK_ORDER_INSPECTION",rowAccess:new MaterialAcceptanceUpdatePolicy(pending,row,()=>Guard(true)));
            await writer.UpdateAsync(row,actor);confirmed=Existing(await Read(true));if(confirmed is null)throw new InvalidOperationException("Original material acceptance update is unconfirmed.");await CorePermit();await (requireCoreProof??requireCurrentProof)(confirmed);await Guard(true);
        });
        var durable=Existing(await Read());if(confirmed is null || JsonSerializer.Serialize(confirmed)!=JsonSerializer.Serialize(durable))throw new InvalidOperationException("Committed original material acceptance is unconfirmed. Recover the retained original intent.");await requireCurrentProof(durable);await Guard();return durable!;
    }
    private sealed class MaterialAcceptanceUpdatePolicy(WORK_ORDER_INSPECTION pending,WORK_ORDER_INSPECTION original,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private readonly WORK_ORDER_INSPECTION expected=CompanyInventorySourceReview.Copy(original);
        private readonly string before=JsonSerializer.Serialize(pending);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(WORK_ORDER_INSPECTION) || table!="WORK_ORDER_INSPECTION" || operation!=PPDMRowMutation.Update || actor!=expected.INSPECTOR || !ReferenceEquals(entity,original) || JsonSerializer.Serialize(previous)!=before)throw new InvalidOperationException("Only one exact original pending material inspection update is authorized.");
            original.PPDM_GUID=expected.PPDM_GUID;original.ACTIVE_IND=expected.ACTIVE_IND;original.ROW_CREATED_BY=expected.ROW_CREATED_BY;original.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;original.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;original.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;if(JsonSerializer.Serialize(original)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("Original material acceptance body/result/audit changed before update.");
        }
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table){await guard();if(type!=typeof(WORK_ORDER_INSPECTION) || table!="WORK_ORDER_INSPECTION")throw new InvalidOperationException("Material update grants no other row reads.");return new[]{new AppFilter{FieldName="INSPECTION_ID",Operator="=",FilterValue=pending.INSPECTION_ID},new AppFilter{FieldName="FIELD_ID",Operator="=",FilterValue=pending.FIELD_ID},new AppFilter{FieldName="WORK_ORDER_ID",Operator="=",FilterValue=pending.WORK_ORDER_ID}};}
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>new[]{await GetReadConstraintsAsync(type,table)};
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows){await guard();if(type!=typeof(WORK_ORDER_INSPECTION) || table!="WORK_ORDER_INSPECTION" || rows.Count!=1 || JsonSerializer.Serialize(rows[0])!=before)throw new InvalidOperationException("Exact original pending inspection mutation source changed.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Material update grants no general row access.");
    }
}
