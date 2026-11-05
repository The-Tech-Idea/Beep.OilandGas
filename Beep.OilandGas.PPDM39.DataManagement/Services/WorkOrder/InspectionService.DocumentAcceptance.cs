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
public sealed partial class InspectionService : ICompanyWorkOrderDocumentAcceptanceOwner
{
    private static readonly System.Diagnostics.ActivitySource DocumentAcceptanceActivities=new("Beep.OilandGas.WorkOrderPermitDecision");
    public async Task<CompanyWorkOrderDocumentAcceptanceRecovery> RecoverDocumentAcceptanceReferenceAsync(FieldSelectionReview context,CompanyWorkOrderDocumentAcceptanceReference reference,Func<Task> requireAuthority)
    {
        reference.Require();FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);if(reference.Permit.Task.FieldId!=context.FieldId)throw new UnauthorizedAccessException("Original document recovery FIELD differs.");var task=reference.Permit.Task;
        async Task Guard(){await requireAuthority();await database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);await requireOrder(context.ActorUserId,task.FieldId,task.WorkOrderId,"WRITE");if(requirePermitPrerequisite is null)throw new UnauthorizedAccessException("Current independent prerequisite authority is required.");await requirePermitPrerequisite(context.ActorUserId);}
        await Guard();var raw=(await ReadRowsAsync(task.WorkOrderId,task.FieldId,context.ActorUserId)).SingleOrDefault(row=>row.INSPECTION_ID==reference.InspectionId)??throw new InvalidOperationException("Original accepted document inspection is unavailable.");
        if(raw.FIELD_ID!=context.FieldId || raw.WORK_ORDER_ID!=task.WorkOrderId || raw.INSPECTOR!=context.ActorUserId)throw new UnauthorizedAccessException("Original acceptance is private to its recorded reviewer/FIELD/order.");await Guard();var command=CompanyWorkOrderDocumentAcceptanceCommand.Read(raw);var envelope=new CompanyWorkOrderDocumentAcceptanceRecovery(command,raw);envelope.Require(context.Company,context.ActorUserId,reference);
        var recorded=await RecordDocumentAcceptanceAsync(command,Guard,_=>throw new InvalidOperationException("Private reference recovery cannot record fresh acceptance"),_=>throw new InvalidOperationException("Private document recovery cannot record fresh acceptance"),true);await Guard();if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(raw))throw new InvalidOperationException("Complete original accepted document record changed during reference recovery.");var result=new CompanyWorkOrderDocumentAcceptanceRecovery(command,recorded);result.Require(context.Company,context.ActorUserId,reference);return result;
    }
    public async Task<WORK_ORDER_INSPECTION> RecordDocumentAcceptanceAsync(CompanyWorkOrderDocumentAcceptanceCommand original,Func<Task> requireAuthority,Func<WORK_ORDER_INSPECTION?,Task> requireCurrentProof,Func<WORK_ORDER_INSPECTION?,Task> requireCoreProof,bool readOnly=false)
    {
        using var acceptanceActivity=DocumentAcceptanceActivities.StartActivity("permit.document.acceptance");
        var command=CompanyInventorySourceReview.Copy(original);command.Require();ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(requireCurrentProof);ArgumentNullException.ThrowIfNull(requireCoreProof);var context=command.Review.PermitReview;var actor=context.Context.ActorUserId;var task=command.Review.Source.Query.Permit.Task;var target=context.CurrentSource.InspectionDatabase;
        await database.RequireContextAsync(context.Context.Company,actor,target);var source=editor.GetDataSource(target.ConnectionName)??throw new InvalidOperationException("Original document inspection source is unavailable.");
        async Task Guard(bool owned=false)
        {
            await requireAuthority();await database.RequireContextAsync(context.Context.Company,actor,target);await requireOrder(actor,task.FieldId,task.WorkOrderId,"WRITE");if(requirePermitPrerequisite is null)throw new UnauthorizedAccessException("Current document prerequisite reviewer authority is required.");await requirePermitPrerequisite(actor);
            if(!ReferenceEquals(source,editor.GetDataSource(target.ConnectionName)) || !source.CheckEntityExist("WORK_ORDER_INSPECTION"))throw new InvalidOperationException("Original document inspection native target changed.");
            var shape=source.GetEntityStructure(new EntityStructure{EntityName="WORK_ORDER_INSPECTION",DatasourceEntityName="WORK_ORDER_INSPECTION"},true);var keys=shape?.Fields?.Where(field=>field.IsKey).ToArray();if(keys is not {Length:1} || keys[0].FieldName!="INSPECTION_ID" || keys[0].AllowDBNull)throw new InvalidOperationException("Original document inspection key metadata is unavailable.");
            foreach(var (name,size) in new[]{("DOCUMENT_ACCEPTANCE_DATA_JSON",CompanyWorkOrderDocumentAcceptanceCommand.MaximumBodyLength),("DOCUMENT_ACCEPTANCE_REVISION",64)}){var fields=shape!.Fields.Where(field=>field.FieldName==name).ToArray();if(fields.Length!=1 || fields[0].Fieldtype!=typeof(string).FullName || !fields[0].AllowDBNull || fields[0].Size1<size)throw new InvalidOperationException("Review the existing PPDM_CORE Beep manifest for nullable bounded document acceptance proof columns.");}
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
        }
        async Task<WORK_ORDER_INSPECTION> Read(bool owned=false)
        {
            var rows=await ReadCompleteGuardedRowsAsync(task.WorkOrderId,task.FieldId,()=>Guard(owned));var row=rows.SingleOrDefault(row=>row.INSPECTION_ID==command.InspectionId)??throw new InvalidOperationException("Original document inspection is unavailable.");return CompanyInventorySourceReview.Copy(row);
        }
        WORK_ORDER_INSPECTION? Existing(WORK_ORDER_INSPECTION row)
        {
            if(row.DOCUMENT_ACCEPTANCE_DATA_JSON is null){if(row.DOCUMENT_ACCEPTANCE_REVISION is not null || row.RESULT_STATUS!="")throw new InvalidOperationException("Another result cannot be adopted as this retained document acceptance.");return null;}
            // Check the public recorded reviewer before deserializing the private retained command.
            if(row.INSPECTOR!=actor)throw new UnauthorizedAccessException("Only the original document reviewer may recover its private command evidence.");command.RequireRecorded(row);return row;
        }
        await Guard();var first=await Read();var recorded=Existing(first);if(readOnly || recorded is not null)
        {
            if(recorded is null)throw new InvalidOperationException("Original document acceptance is not recorded; retain its intent before retrying.");var closing=Existing(await Read());if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Complete original recorded document acceptance changed during recovery.");return recorded;
        }
        command.RequireCurrentAcceptance();
        async Task CorePermit()
        {
            using var permitActivity=DocumentAcceptanceActivities.StartActivity("permit.document.acceptance.owned-permit");
            await Guard(true);if(!source.CheckEntityExist("WORK_ORDER_EXECUTION_PERMIT"))throw new InvalidOperationException("Original local pending permit schema is unavailable.");var repo=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_EXECUTION_PERMIT),target.ConnectionName,"WORK_ORDER_EXECUTION_PERMIT");var rows=(await repo.GetAsync([new AppFilter{FieldName="PERMIT_ID",Operator="=",FilterValue=context.Proposal.PermitId()}])).ToArray();
            if(rows.Length!=1 || rows[0] is not WORK_ORDER_EXECUTION_PERMIT row || ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(row))!=context.RecordedRevision)throw new InvalidOperationException("Complete original local pending permit changed inside document acceptance.");context.Proposal.RequireRecorded(row);await Guard(true);
        }
        async Task CoreDocument()
        {
            using var documentActivity=DocumentAcceptanceActivities.StartActivity("permit.document.acceptance.owned-document");
            await Guard(true);var current=await ReadDocumentContributorsCoreAsync(context.Context,command.Review.Source.Query,command.Review.Source.ExpectedReference,()=>Guard(true),true,true);var originalContributors=command.ContributorReview!;
            if(CompanyWorkOrderDocumentContributors.EvidenceProof(current)!=CompanyWorkOrderDocumentContributors.EvidenceProof(originalContributors.Contributors) || CompanyWorkOrderPermitDocumentFacts.CurrentProof(current.Source)!=CompanyWorkOrderPermitDocumentFacts.CurrentProof(command.Review.Source) || current.ComparedAtUtc<context.Proposal.ValidFromUtc || current.ComparedAtUtc>=context.Proposal.ValidToUtc)throw new InvalidOperationException("Complete original local document/contributor/native identity proof changed inside acceptance.");await Guard(true);
        }
        await requireCurrentProof(null);await Guard();
        WORK_ORDER_INSPECTION? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);var pending=await Read(true);confirmed=Existing(pending);if(confirmed is not null)return;
            await CorePermit();await CoreDocument();await requireCoreProof(null);await Guard(true);var row=command.Prepare(pending,DateTime.UtcNow);
            var writer=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_INSPECTION),target.ConnectionName,"WORK_ORDER_INSPECTION",rowAccess:new DocumentAcceptanceUpdatePolicy(pending,row,()=>Guard(true)));
            await writer.UpdateAsync(row,actor);confirmed=Existing(await Read(true));if(confirmed is null)throw new InvalidOperationException("Original document acceptance update is unconfirmed.");await CorePermit();await CoreDocument();await requireCoreProof(confirmed);await Guard(true);
        });
        var durable=Existing(await Read());if(confirmed is null || JsonSerializer.Serialize(confirmed)!=JsonSerializer.Serialize(durable))throw new InvalidOperationException("Committed original document acceptance is unconfirmed. Recover the retained original intent.");await requireCurrentProof(durable);await Guard();return durable!;
    }
    private sealed class DocumentAcceptanceUpdatePolicy(WORK_ORDER_INSPECTION pending,WORK_ORDER_INSPECTION original,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private readonly WORK_ORDER_INSPECTION expected=CompanyInventorySourceReview.Copy(original);
        private readonly string before=JsonSerializer.Serialize(pending);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(WORK_ORDER_INSPECTION) || table!="WORK_ORDER_INSPECTION" || operation!=PPDMRowMutation.Update || actor!=expected.INSPECTOR || !ReferenceEquals(entity,original) || JsonSerializer.Serialize(previous)!=before)throw new InvalidOperationException("Only one exact original pending document inspection update is authorized.");
            original.PPDM_GUID=expected.PPDM_GUID;original.ACTIVE_IND=expected.ACTIVE_IND;original.ROW_CREATED_BY=expected.ROW_CREATED_BY;original.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;original.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;original.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;if(JsonSerializer.Serialize(original)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("Original document acceptance body/result/audit changed before update.");
        }
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table){await guard();if(type!=typeof(WORK_ORDER_INSPECTION) || table!="WORK_ORDER_INSPECTION")throw new InvalidOperationException("Document update grants no other row reads.");return new[]{new AppFilter{FieldName="INSPECTION_ID",Operator="=",FilterValue=pending.INSPECTION_ID},new AppFilter{FieldName="FIELD_ID",Operator="=",FilterValue=pending.FIELD_ID},new AppFilter{FieldName="WORK_ORDER_ID",Operator="=",FilterValue=pending.WORK_ORDER_ID}};}
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>new[]{await GetReadConstraintsAsync(type,table)};
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows){await guard();if(type!=typeof(WORK_ORDER_INSPECTION) || table!="WORK_ORDER_INSPECTION" || rows.Count!=1 || JsonSerializer.Serialize(rows[0])!=before)throw new InvalidOperationException("Exact original pending inspection mutation source changed.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Document update grants no general row access.");
    }
}
