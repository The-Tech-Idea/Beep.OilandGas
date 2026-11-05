using System.Text.Json;
using System.Data;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;

public sealed partial class WorkOrderExecutionPermitService
{
    public Task<CompanyWorkOrderExecutionRequirements> ReadExecutionRequirementsAsync(FieldSelectionReview current,
        CompanyWorkOrderExecutionPermitReference query,Func<Task> authority,Func<Task<(WorkOrderDetailModel Order,IReadOnlyList<InspectionCondition> Requirements)>> readCurrent)
        =>ReadExecutionRequirementsAsync(current,query,authority,readCurrent,false);
    public Task<CompanyWorkOrderExecutionRequirements> ReadExecutionRequirementsCoreAsync(FieldSelectionReview current,
        CompanyWorkOrderExecutionPermitReference query,Func<Task> authority,Func<Task<(WorkOrderDetailModel Order,IReadOnlyList<InspectionCondition> Requirements)>> readCurrent)
        =>ReadExecutionRequirementsAsync(current,query,authority,readCurrent,true);
    private async Task<CompanyWorkOrderExecutionRequirements> ReadExecutionRequirementsAsync(FieldSelectionReview current,
        CompanyWorkOrderExecutionPermitReference query,Func<Task> authority,Func<Task<(WorkOrderDetailModel Order,IReadOnlyList<InspectionCondition> Requirements)>> readCurrent,bool owned)
    {
        var approval=await ReadRecordedApprovalAsync(current,query,authority,owned);
        var source=editor.GetDataSource(current.Database.ConnectionName)??throw new InvalidOperationException("Original approval source is unavailable.");
        void Ownership(){if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);}
        async Task Guard(){Ownership();await authority();if(!ReferenceEquals(source,editor.GetDataSource(current.Database.ConnectionName)))throw new InvalidOperationException("Original execution requirements target changed.");Ownership();RequireSchema(source);}
        await Guard();var repository=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_EXECUTION_PERMIT),current.Database.ConnectionName,Table);
        var raw=(await repository.GetAsync([new(){FieldName="PERMIT_ID",Operator="=",FilterValue=approval.PermitId}])).ToArray();await Guard();
        if(raw.Length!=1 || raw[0] is not WORK_ORDER_EXECUTION_PERMIT row || ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(row))!=approval.RecordedRevision)throw new InvalidOperationException("Original recorded approval changed before requirement comparison.");
        var decision=CompanyWorkOrderExecutionPermitDecision.FromRecorded(row);var original=CompanyWorkOrderExecutionRequirements.Population(query.Task.FieldId,query.Task.WorkOrderId,decision.Review.OriginalSource.Inspections);
        async Task<(WorkOrderDetailModel Order,IReadOnlyList<InspectionCondition> Requirements)> CaptureCurrent()
        {
            var value=await readCurrent();
            // Providers may return tracked mutable objects. Detach before any awaited authority check.
            return(CompanyInventorySourceReview.Copy(value.Order),CompanyInventorySourceReview.Copy(value.Requirements.ToArray()));
        }
        var observed=await CaptureCurrent();var frozen=JsonSerializer.Serialize(new{Order=CompanyWorkOrderExecutionRequirements.OrderRevision(observed.Order),Requirements=observed.Requirements.OrderBy(row=>row.CondSeq).ToArray()});await Guard();var closing=await CaptureCurrent();await Guard();
        if(frozen!=JsonSerializer.Serialize(new{Order=CompanyWorkOrderExecutionRequirements.OrderRevision(closing.Order),Requirements=closing.Requirements.OrderBy(row=>row.CondSeq).ToArray()}))throw new InvalidOperationException("Complete native order/requirement/result population changed during execution review.");
        var closingApproval=await ReadRecordedApprovalAsync(current,query,authority,owned);if(approval!=closingApproval)throw new InvalidOperationException("Original recorded approval changed around current execution requirements.");
        var utc=DateTime.UtcNow;var result=new CompanyWorkOrderExecutionRequirements(current,approval,closing.Order,closing.Requirements,original,CompanyWorkOrderExecutionRequirements.OrderRevision(decision.Review.OriginalSource.Order),utc,CompanyWorkOrderExecutionRequirements.GetBlockers(approval,original,closing.Requirements,CompanyWorkOrderExecutionRequirements.OrderRevision(decision.Review.OriginalSource.Order),closing.Order,utc));result.Require(current.Company,current.ActorUserId,query);await Guard();return result;
    }
    public Task<CompanyWorkOrderRecordedApproval> ReadRecordedApprovalAsync(FieldSelectionReview current,
        CompanyWorkOrderExecutionPermitReference query,Func<Task> authority)
        =>ReadRecordedApprovalAsync(current,query,authority,false);
    private async Task<CompanyWorkOrderRecordedApproval> ReadRecordedApprovalAsync(FieldSelectionReview current,
        CompanyWorkOrderExecutionPermitReference query,Func<Task> authority,bool owned)
    {
        query.Require();FieldSelectionReviewValidator.RequireReview(current,current.Company,current.ActorUserId);
        if(current.FieldId!=query.Task.FieldId)throw new UnauthorizedAccessException("The recorded permit's original FIELD is required.");
        var source=editor.GetDataSource(current.Database.ConnectionName)??throw new InvalidOperationException("Original recorded permit datasource is unavailable.");
        void Ownership(){if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);}
        async Task Guard(){Ownership();await authority();if(!ReferenceEquals(source,editor.GetDataSource(current.Database.ConnectionName)))throw new InvalidOperationException("Original recorded permit datasource changed.");Ownership();RequireSchema(source);}
        var repository=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_EXECUTION_PERMIT),current.Database.ConnectionName,Table);
        async Task<WORK_ORDER_EXECUTION_PERMIT> Read()
        {
            await Guard();var raw=(await repository.GetAsync([new(){FieldName="PERMIT_ID",Operator="=",FilterValue=CompanyWorkOrderExecutionPermitIntent.PermitIdFor(current.Company,query.IntentId)}])).ToArray();
            if(raw.Length!=1 || raw[0] is not WORK_ORDER_EXECUTION_PERMIT row)throw new InvalidOperationException("One complete original recorded approval is required.");
            // Close public ownership before opening the retained private decision body internally.
            if(row.TENANT_ID!=current.Company.TenantId || row.BUSINESS_ASSOCIATE_ID!=current.Company.BusinessAssociateId || row.FIELD_ID!=query.Task.FieldId || row.WORK_ORDER_ID!=query.Task.WorkOrderId || row.PROCESS_INSTANCE_ID!=query.Task.InstanceId)throw new UnauthorizedAccessException("Recorded approval belongs to another company/FIELD/order/task.");
            var captured=CompanyInventorySourceReview.Copy(row);await Guard();return captured;
        }
        var original=await Read();var decision=CompanyWorkOrderExecutionPermitDecision.FromRecorded(original);
        if(decision.Outcome!="Approve" || decision.ProposalReference()!=query || decision.Review.Context.Company!=current.Company || decision.Review.Context.Database!=current.Database)throw new InvalidOperationException("The exact original permit has no confirmed approval in this database.");
        var closing=await Read();if(JsonSerializer.Serialize(original)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Complete recorded approval changed during public review.");await Guard();
        var proposal=decision.Review.Proposal;
        var result=new CompanyWorkOrderRecordedApproval(current.Company,query,proposal.PermitId(),decision.IntentId,proposal.Context.ActorUserId,decision.Review.Context.ActorUserId,original.DECIDED_AT_UTC!.Value,
            proposal.ValidFromUtc,proposal.ValidToUtc,proposal.PermitType,proposal.Jurisdiction,proposal.ScopeDescription,proposal.RequirementEvidenceVersion,decision.Review.PrerequisiteEvidenceVersion,
            decision.Review.OriginalSource.RequirementFactsRevision(),ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(original)));
        result.Require(current.Company,query);return result;
    }
}
