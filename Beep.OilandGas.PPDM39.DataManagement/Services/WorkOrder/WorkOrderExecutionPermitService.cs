using System.Data;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;

/// <summary>Native pending work-permit issue owner; never independent approval or execution authorization.</summary>
public sealed partial class WorkOrderExecutionPermitService(IDMEEditor editor,ICommonColumnHandler columns,IPPDM39DefaultsRepository defaults,IPPDMMetadataRepository metadata)
{
    private const string Table="WORK_ORDER_EXECUTION_PERMIT";
    public async Task<CompanyWorkOrderExecutionPermitApprovalReview> ReviewApprovalAsync(FieldSelectionReview current,
        CompanyWorkOrderExecutionPermitReference query,Func<Task> authority,Func<Task<CompanyMaintenanceInspectionReview>> readSource,Func<Task<CompanyMaintenanceInspectionReview>>? readRequirementSource=null,
        CompanyWorkOrderExecutionPermitDecision? recordedDecision=null)
    {
        ArgumentNullException.ThrowIfNull(current);ArgumentNullException.ThrowIfNull(query);ArgumentNullException.ThrowIfNull(authority);ArgumentNullException.ThrowIfNull(readSource);
        query.Require();FieldSelectionReviewValidator.RequireReview(current,current.Company,current.ActorUserId);if(current.FieldId!=query.Task.FieldId)throw new UnauthorizedAccessException("The original permit FIELD is required.");
        var source=editor.GetDataSource(current.Database.ConnectionName)??throw new InvalidOperationException("The original permit core source is unavailable.");
        async Task Guard(){PPDMDataSourceTransaction.RequireIdle(source);await authority();if(!ReferenceEquals(source,editor.GetDataSource(current.Database.ConnectionName)))throw new InvalidOperationException("The permit review database changed.");PPDMDataSourceTransaction.RequireIdle(source);}
        await Guard();RequireSchema(source);
        var repository=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_EXECUTION_PERMIT),current.Database.ConnectionName,Table);
        var id=CompanyWorkOrderExecutionPermitIntent.PermitIdFor(current.Company,query.IntentId);
        async Task<WORK_ORDER_EXECUTION_PERMIT> Read()
        {
            var rows=(await repository.GetAsync([new AppFilter{FieldName="PERMIT_ID",Operator="=",FilterValue=id}])).ToArray();
            if(rows.Length!=1 || rows[0] is not WORK_ORDER_EXECUTION_PERMIT row)throw new InvalidOperationException("The original pending permit is unavailable or ambiguous.");
            if(row.TENANT_ID!=current.Company.TenantId || row.BUSINESS_ASSOCIATE_ID!=current.Company.BusinessAssociateId || row.FIELD_ID!=query.Task.FieldId || row.WORK_ORDER_ID!=query.Task.WorkOrderId || row.PROCESS_INSTANCE_ID!=query.Task.InstanceId || row.ISSUED_BY==current.ActorUserId)throw new UnauthorizedAccessException("An independently authorized reviewer of the original company/task is required.");return row;
        }
        var physical=await Read();var frozen=JsonSerializer.Serialize(physical);
        if(recordedDecision is not null && (recordedDecision.Review.Context!=current || recordedDecision.ProposalReference()!=query))throw new UnauthorizedAccessException("The original private decision reviewer/company/task is required for current proof closure.");
        if(recordedDecision is not null && physical.DECISION_DATA_JSON is null)recordedDecision.RequirePending(physical);
        var pending=recordedDecision is null || physical.DECISION_DATA_JSON is null?physical:recordedDecision.ReadOriginalPending(physical);CompanyWorkOrderExecutionPermitIntent proposal;
        try{proposal=JsonSerializer.Deserialize<CompanyWorkOrderExecutionPermitIntent>(pending.ISSUE_DATA_JSON)??throw new InvalidOperationException("The original pending proposal is unavailable.");}
        catch(JsonException error){throw new InvalidOperationException("The original pending proposal is malformed.",error);}
        proposal.RequireRecorded(pending);var original=proposal.RequireRecordedSource(pending);
        if(proposal.Context.Company!=current.Company || proposal.Context.Database!=current.Database || proposal.Task!=query.Task || proposal.IntentId!=query.IntentId)throw new UnauthorizedAccessException("The original pending proposal source/company/task changed.");
        var currentRead=proposal.RequirementEvidenceVersion is 1 or 2?readRequirementSource??throw new InvalidOperationException("Versioned permit requires its original immutable native requirement source."):readSource;
        await Guard();var observed=CompanyInventorySourceReview.Copy(await currentRead());observed.Require(current.Company,current.ActorUserId,query.Task);await Guard();
        var closing=await currentRead();closing.Require(current.Company,current.ActorUserId,query.Task);
        if(proposal.RequirementEvidenceVersion is 1 or 2 && observed.Inspections.Any(row=>!ProcessDefinitionRevision.IsValid(row.RequirementRevision)))throw new InvalidOperationException("Current original immutable requirement evidence is incomplete.");
        if(observed.SnapshotRevision!=closing.SnapshotRevision)throw new InvalidOperationException("The complete current order/inspection/assignment changed during independent review.");
        await Guard();RequireSchema(source);if(frozen!=JsonSerializer.Serialize(await Read()))throw new InvalidOperationException("The original pending proposal changed during independent review.");await Guard();
        var utc=DateTime.UtcNow;var policy=proposal.RequirementEvidenceVersion==2?7:1;var result=new CompanyWorkOrderExecutionPermitApprovalReview(current,proposal,original,observed,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(pending)),utc,CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(proposal,original,observed,utc,policy)){PrerequisiteEvidenceVersion=policy};result.Require(current.Company,current.ActorUserId,query);return CompanyInventorySourceReview.Copy(result);
    }
    private void RequireSchema(IDataSource source)
    {
        if(!source.CheckEntityExist(Table))throw new InvalidOperationException("The work-permit schema is unavailable. Review its PPDM_CORE Beep migration before issuing permits.");
        var structure=source.GetEntityStructure(new EntityStructure{EntityName=Table,DatasourceEntityName=Table},true);
        if(structure?.Fields is null || structure.Fields.Count==0 || structure.Fields.GroupBy(field=>field.FieldName,StringComparer.OrdinalIgnoreCase).Any(group=>group.Count()!=1))throw new InvalidOperationException("The complete physical work-permit schema is unconfirmed.");
        var expected=editor.classCreator.ConvertToEntityStructure(typeof(WORK_ORDER_EXECUTION_PERMIT))??throw new InvalidOperationException("The canonical work-permit migration shape is unavailable.");
        foreach(var column in expected.Fields)
        {
            var fields=structure.Fields.Where(field=>field.FieldName==column.FieldName).ToArray();
            if(fields.Length!=1 || column.IsKey && (!fields[0].IsKey || fields[0].AllowDBNull))throw new InvalidOperationException("The work-permit column or identity metadata is unconfirmed.");
        }
        if(structure.Fields.Where(field=>field.IsKey).Select(field=>field.FieldName).SingleOrDefault()!="PERMIT_ID")throw new InvalidOperationException("The physical work-permit identity must be explicit and unique.");
    }
    public async Task<CompanyWorkOrderExecutionPermitIntent> ReadOriginalAsync(FieldSelectionReview current,CompanyWorkOrderExecutionPermitReference query,Func<Task> authority)
    {
        ArgumentNullException.ThrowIfNull(current);ArgumentNullException.ThrowIfNull(query);ArgumentNullException.ThrowIfNull(authority);query.Require();FieldSelectionReviewValidator.RequireReview(current,current.Company,current.ActorUserId);
        if(current.FieldId!=query.Task.FieldId)throw new UnauthorizedAccessException("The original pending permit FIELD/account is required.");
        var source=editor.GetDataSource(current.Database.ConnectionName)??throw new InvalidOperationException("The original pending permit datasource is unavailable.");
        async Task Guard(){PPDMDataSourceTransaction.RequireIdle(source);await authority();if(!ReferenceEquals(source,editor.GetDataSource(current.Database.ConnectionName)))throw new InvalidOperationException("The original pending permit database changed.");PPDMDataSourceTransaction.RequireIdle(source);}
        await Guard();if(!source.CheckEntityExist(Table))throw new InvalidOperationException("The pending permit schema is unavailable.");
        var repository=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_EXECUTION_PERMIT),current.Database.ConnectionName,Table);var id=CompanyWorkOrderExecutionPermitIntent.PermitIdFor(current.Company,query.IntentId);
        var rows=(await repository.GetAsync([new AppFilter{FieldName="PERMIT_ID",Operator="=",FilterValue=id}])).ToArray();
        if(rows.Length!=1 || rows[0] is not WORK_ORDER_EXECUTION_PERMIT row)throw new InvalidOperationException("The original pending permit reference is unavailable or ambiguous.");
        if(row.TENANT_ID!=current.Company.TenantId || row.BUSINESS_ASSOCIATE_ID!=current.Company.BusinessAssociateId || row.ISSUED_BY!=current.ActorUserId || row.FIELD_ID!=query.Task.FieldId || row.WORK_ORDER_ID!=query.Task.WorkOrderId || row.PROCESS_INSTANCE_ID!=query.Task.InstanceId)throw new UnauthorizedAccessException("The pending permit intent is private to its original issuer/company/task.");
        CompanyWorkOrderExecutionPermitIntent intent;
        try{intent=JsonSerializer.Deserialize<CompanyWorkOrderExecutionPermitIntent>(row.ISSUE_DATA_JSON)??throw new InvalidOperationException("The original pending permit intent is unavailable.");intent.Require();}
        catch(JsonException invalid){throw new InvalidOperationException("The original pending permit intent is malformed.",invalid);}
        if(intent.Context!=current || intent.Task!=query.Task || intent.IntentId!=query.IntentId)throw new UnauthorizedAccessException("The original pending permit authority/reference changed.");intent.RequireRecorded(row);
        var snapshot=JsonSerializer.Serialize(row);await Guard();var closing=(await repository.GetAsync([new AppFilter{FieldName="PERMIT_ID",Operator="=",FilterValue=id}])).ToArray();
        if(closing.Length!=1 || snapshot!=JsonSerializer.Serialize(closing[0]))throw new InvalidOperationException("The original pending permit changed during private reference review.");await Guard();return CompanyInventorySourceReview.Copy(intent);
    }
    public Task<CompanyWorkOrderExecutionPermitReceipt> IssueAsync(CompanyWorkOrderExecutionPermitIntent intent,Func<Task> authority,Func<Task<CompanyMaintenanceInspectionReview>> readSource)
        =>ReadOrIssueAsync(intent,authority,readSource,false);
    public Task<CompanyWorkOrderExecutionPermitReceipt> RecoverAsync(CompanyWorkOrderExecutionPermitIntent intent,Func<Task> authority,Func<Task<CompanyMaintenanceInspectionReview>> readSource)
        =>ReadOrIssueAsync(intent,authority,readSource,true);
    private async Task<CompanyWorkOrderExecutionPermitReceipt> ReadOrIssueAsync(CompanyWorkOrderExecutionPermitIntent original,Func<Task> authority,Func<Task<CompanyMaintenanceInspectionReview>> readSource,bool readOnly)
    {
        ArgumentNullException.ThrowIfNull(original);ArgumentNullException.ThrowIfNull(authority);ArgumentNullException.ThrowIfNull(readSource);
        var intent=CompanyInventorySourceReview.Copy(original);intent.Require();
        var source=editor.GetDataSource(intent.Context.Database.ConnectionName)??throw new InvalidOperationException("The original work-permit core datasource is unavailable.");
        var repository=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_EXECUTION_PERMIT),intent.Context.Database.ConnectionName,Table);
        async Task Guard(bool owned)
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();
            if(!ReferenceEquals(source,editor.GetDataSource(intent.Context.Database.ConnectionName)))throw new InvalidOperationException("The original work-permit core database binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        void Schema()=>RequireSchema(source);
        async Task<WORK_ORDER_EXECUTION_PERMIT?> Read()
        {
            var rows=(await repository.GetAsync([new AppFilter{FieldName="PERMIT_ID",Operator="=",FilterValue=intent.PermitId()}])).ToArray();
            if(rows.Length>1 || rows.Any(row=>row is not WORK_ORDER_EXECUTION_PERMIT))throw new InvalidOperationException("The original work-permit identity is ambiguous.");
            if(rows.Length==0)return null;var row=(WORK_ORDER_EXECUTION_PERMIT)rows[0];intent.RequireRecorded(row);return row;
        }
        await Guard(false);Schema();var originalSource=CompanyInventorySourceReview.Copy(await readSource());intent.RequireSource(originalSource);await Guard(false);
        WORK_ORDER_EXECUTION_PERMIT? recorded=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);Schema();recorded=await Read();
            if(recorded is null)
            {
                if(readOnly)throw new InvalidOperationException("The original work-permit issue is not recorded. Read-only recovery cannot create it.");
                var intended=intent.Prepare(DateTime.UtcNow,originalSource);await repository.InsertAsync(intended,intent.Context.ActorUserId);recorded=await Read()??throw new InvalidOperationException("The pending work-permit insert is unconfirmed.");
                if(recorded.PPDM_GUID!=intended.PPDM_GUID || recorded.ROW_CREATED_DATE!=intended.ROW_CREATED_DATE)throw new InvalidOperationException("The original work-permit insert audit changed.");
            }
            await Guard(true);
        });
        if(recorded is null)throw new InvalidOperationException("The original pending work-permit issue is unconfirmed.");
        var frozen=JsonSerializer.Serialize(recorded);await Guard(false);intent.RequireSource(await readSource());await Guard(false);Schema();
        var committed=await Read()??throw new InvalidOperationException("The original work-permit commit is unconfirmed. Retain its original intent.");
        if(frozen!=JsonSerializer.Serialize(committed))throw new InvalidOperationException("The pending work-permit issue changed after source/authority confirmation.");
        await Guard(false);
        var receipt=new CompanyWorkOrderExecutionPermitReceipt(intent.Context.Company,intent.Context.ActorUserId,intent.IntentId,intent.PermitId(),ProcessDefinitionRevision.FromJson(frozen));receipt.Require(intent);return receipt;
    }
}
