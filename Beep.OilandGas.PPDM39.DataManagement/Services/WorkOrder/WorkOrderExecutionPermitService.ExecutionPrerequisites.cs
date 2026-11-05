using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;

public sealed partial class WorkOrderExecutionPermitService
{
    public Task<CompanyWorkOrderExecutionPrerequisites> ReadExecutionPrerequisitesAsync(FieldSelectionReview executor,
        CompanyWorkOrderExecutionPermitReference query,Func<Task> authority,
        Func<bool,Task<(WorkOrderDetailModel Order,IReadOnlyList<InspectionCondition> Requirements)>> readCurrent,
        ICompanyPersonnelAcceptanceCoreProof personnel,ICompanyWorkOrderDocumentContributorCore documents,ICompanyRecordedReviewerIdentityCore identities,
        Func<CompanyWorkOrderExecutionPermitApprovalReview,Task> currentStock,
        Func<CompanyWorkOrderExecutionPermitApprovalReview,Task<bool>> ownedStock)
        =>WithExecutionPrerequisitesAsync(executor,query,authority,readCurrent,personnel,documents,identities,currentStock,ownedStock,null,null);

    private async Task<CompanyWorkOrderExecutionPrerequisites> WithExecutionPrerequisitesAsync(FieldSelectionReview executor,
        CompanyWorkOrderExecutionPermitReference query,Func<Task> authority,
        Func<bool,Task<(WorkOrderDetailModel Order,IReadOnlyList<InspectionCondition> Requirements)>> readCurrent,
        ICompanyPersonnelAcceptanceCoreProof personnel,ICompanyWorkOrderDocumentContributorCore documents,ICompanyRecordedReviewerIdentityCore identities,
        Func<CompanyWorkOrderExecutionPermitApprovalReview,Task> currentStock,
        Func<CompanyWorkOrderExecutionPermitApprovalReview,Task<bool>> ownedStock,
        Func<CompanyWorkOrderExecutionPrerequisites,Task>? extraOwnedProof,Func<Task>? mutation)
    {
        ArgumentNullException.ThrowIfNull(personnel);ArgumentNullException.ThrowIfNull(documents);ArgumentNullException.ThrowIfNull(identities);
        ArgumentNullException.ThrowIfNull(currentStock);ArgumentNullException.ThrowIfNull(ownedStock);
        var comparison=await ReadExecutionRequirementsAsync(executor,query,authority,()=>readCurrent(false));
        if(comparison.Blockers.Count!=0)throw new InvalidOperationException("Current original order/requirements/permit validity blocks execution prerequisite review.");
        var source=editor.GetDataSource(executor.Database.ConnectionName)??throw new InvalidOperationException("Original execution core source is unavailable.");
        async Task Guard(bool owned)
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(!ReferenceEquals(source,editor.GetDataSource(executor.Database.ConnectionName)))throw new InvalidOperationException("Original execution core source changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequireSchema(source);
        }
        var repo=new PPDMGenericRepository(editor,columns,defaults,metadata,typeof(WORK_ORDER_EXECUTION_PERMIT),executor.Database.ConnectionName,Table);
        async Task<CompanyWorkOrderExecutionPermitDecision> Original(bool owned)
        {
            await Guard(owned);var raw=(await repo.GetAsync([new(){FieldName="PERMIT_ID",Operator="=",FilterValue=comparison.Approval.PermitId}])).ToArray();
            if(raw.Length!=1 || raw[0] is not WORK_ORDER_EXECUTION_PERMIT row)throw new InvalidOperationException("The exact approved original permit is unavailable.");
            if(row.TENANT_ID!=executor.Company.TenantId || row.BUSINESS_ASSOCIATE_ID!=executor.Company.BusinessAssociateId || row.FIELD_ID!=query.Task.FieldId || row.WORK_ORDER_ID!=query.Task.WorkOrderId || row.PROCESS_INSTANCE_ID!=query.Task.InstanceId)throw new UnauthorizedAccessException("Original company/FIELD/order/task closes before private approval proof.");
            var captured=CompanyInventorySourceReview.Copy(row);await Guard(owned);
            if(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(captured))!=comparison.Approval.RecordedRevision)throw new InvalidOperationException("Original approved permit changed during execution proof.");
            var decision=CompanyWorkOrderExecutionPermitDecision.FromRecorded(captured);
            if(decision.Outcome!="Approve" || decision.ProposalReference()!=query || decision.Review.Context.Database!=executor.Database)throw new InvalidOperationException("The exact original approved decision/binding is unavailable.");
            decision.Review.RequireCurrentPrerequisiteAcceptance(DateTime.UtcNow);return decision;
        }
        var decision=await Original(false);await currentStock(decision.Review);await Guard(false);
        static string Facts(CompanyWorkOrderExecutionRequirements value)=>JsonSerializer.Serialize(new{Order=CompanyWorkOrderExecutionRequirements.OrderRevision(value.Order),Requirements=value.Requirements.OrderBy(row=>row.CondSeq).ToArray()});
        var baseline=Facts(comparison);bool? sharesCore=null;
        async Task Current(bool owned)
        {
            await Guard(owned);var value=await readCurrent(owned);var order=CompanyInventorySourceReview.Copy(value.Order);var rows=CompanyInventorySourceReview.Copy(value.Requirements.ToArray());await Guard(owned);
            var utc=DateTime.UtcNow;var current=new CompanyWorkOrderExecutionRequirements(executor,comparison.Approval,order,rows,comparison.OriginalRequirementPopulation,comparison.OriginalOrderRevision,utc,CompanyWorkOrderExecutionRequirements.GetBlockers(comparison.Approval,comparison.OriginalRequirementPopulation,rows,comparison.OriginalOrderRevision,order,utc));
            current.Require(executor.Company,executor.ActorUserId,query);if(current.Blockers.Count!=0 || Facts(current)!=baseline)throw new InvalidOperationException("Complete original order/requirements/results changed during execution proof.");comparison=current;
        }
        async Task Core()
        {
            var original=await Original(true);await Current(true);
            await personnel.RequireExecutionPersonnelCoreAsync(executor,original.Review.CurrentPersonnelAcceptance!.Evidence,()=>Guard(true));
            foreach(var document in original.Review.CurrentDocumentAcceptance!)
            {
                await documents.RequireExecutionDocumentCoreAsync(executor,document,()=>Guard(true));
                await identities.RequireExecutionReviewerIdentityCoreAsync(executor,document.CurrentIdentities!,document.RecordedReviewerPerson!,document.Acceptance.Reviewer,original.Review.Proposal.ValidFromUtc,original.Review.Proposal.ValidToUtc,()=>Guard(true));
            }
            var shared=await ownedStock(original.Review);if(sharesCore is {} previous && previous!=shared)throw new InvalidOperationException("Stock/core ownership changed during execution proof.");sharesCore=shared;await Guard(true);
        }
        CompanyWorkOrderExecutionPrerequisites Result()
        {
            var result=new CompanyWorkOrderExecutionPrerequisites(comparison,decision.Review.CurrentMaterialAcceptance!.Reservation,decision.Review.CurrentPersonnelAcceptance!.Evidence.Assignment.Assignment,
                decision.Review.CurrentDocumentAcceptance!.OrderBy(row=>row.Acceptance.Kind,StringComparer.Ordinal).Select(row=>row.Acceptance).ToArray(),decision.Review.PrerequisiteProofRevision(),sharesCore!.Value,DateTime.UtcNow,"");
            result=result with{Revision=CompanyWorkOrderExecutionPrerequisites.Digest(result)};result.Require(executor.Company,executor.ActorUserId,query);return result;
        }
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Core();if(extraOwnedProof is not null)await extraOwnedProof(Result());
            if(mutation is not null)await mutation();
            await Core();if(extraOwnedProof is not null)await extraOwnedProof(Result());
        });
        await Guard(false);await currentStock(decision.Review);await Current(false);await Original(false);
        var result=Result();await Guard(false);return result;
    }
}
