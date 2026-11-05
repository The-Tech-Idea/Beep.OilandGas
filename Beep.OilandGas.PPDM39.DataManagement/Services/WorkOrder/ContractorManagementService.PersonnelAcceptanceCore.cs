using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
public partial class ContractorManagementService
{
    /// <summary>Explicit read proof under the inspection owner's same PPDM_CORE serializable transaction; no lifecycle transaction borrowing.</summary>
    public Task RequirePersonnelAcceptanceCoreAsync(CompanyPersonnelPolicyEvidenceReview original,Func<Task> requireAuthority)
        =>RequirePersonnelCoreAsync(original.Assignment.CurrentPersonnel.Personnel.Context,original,requireAuthority,false);

    /// <summary>Compare retained personnel facts under the actual executor's owned core boundary; never rebase historical proof.</summary>
    public Task RequireExecutionPersonnelCoreAsync(FieldSelectionReview executor,CompanyPersonnelPolicyEvidenceReview original,Func<Task> requireAuthority)
        =>RequirePersonnelCoreAsync(executor,original,requireAuthority,true);

    private async Task RequirePersonnelCoreAsync(FieldSelectionReview context,CompanyPersonnelPolicyEvidenceReview original,Func<Task> requireAuthority,bool execution)
    {
        var retained=original.Assignment.CurrentPersonnel.Personnel.Context;original.Require(retained.Company,retained.ActorUserId,original.Assignment.Query);
        FieldSelectionReviewValidator.RequireReview(context,retained.Company,context.ActorUserId);
        if(context.Database!=retained.Database || context.FieldId!=original.Assignment.Query.Permit.Task.FieldId)
            throw new UnauthorizedAccessException("Execution personnel proof requires the original company/FIELD/native database.");
        if(execution && original.EligibilityEvidenceVersion!=1)throw new InvalidOperationException("Current execution requires explicit company employment eligibility policy.");
        await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);
        var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original personnel acceptance core source is unavailable.");
        async Task Guard()
        {
            await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);if(!ReferenceEquals(source,_editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original personnel acceptance native core target changed.");PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);RequirePersonnelAssignmentSchema(source);RequirePersonnelReleaseSchema(source);
        }
        await Guard();var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_PERSONNEL_ASSIGNMENT),context.Database.ConnectionName,"WORK_ORDER_PERSONNEL_ASSIGNMENT");var task=original.Assignment.Query.Permit.Task;
        var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=context.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=context.Company.BusinessAssociateId},new(){FieldName="FIELD_ID",Operator="=",FilterValue=task.FieldId},new(){FieldName="WORK_ORDER_ID",Operator="=",FilterValue=task.WorkOrderId},new(){FieldName="PROCESS_INSTANCE_ID",Operator="=",FilterValue=task.InstanceId},new(){FieldName="PERMIT_INTENT_ID",Operator="=",FilterValue=original.Assignment.Query.Permit.IntentId.ToString("D")},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}])).ToArray();await Guard();
        if(raw.Length>4096 || raw.Any(value=>value is not WORK_ORDER_PERSONNEL_ASSIGNMENT row || row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.FIELD_ID!=task.FieldId || row.WORK_ORDER_ID!=task.WorkOrderId || row.PROCESS_INSTANCE_ID!=task.InstanceId || row.PERMIT_INTENT_ID!=original.Assignment.Query.Permit.IntentId.ToString("D") || row.ACTIVE_IND!="Y"))throw new UnauthorizedAccessException("Complete original core personnel assignment headers differ.");
        var rows=raw.Cast<WORK_ORDER_PERSONNEL_ASSIGNMENT>().OrderBy(row=>row.ASSIGNMENT_ID,StringComparer.Ordinal).ToArray();var facts=rows.Select(CompanyWorkOrderPersonnelAssignmentFacts.From).ToArray();
        if(JsonSerializer.Serialize(facts)!=JsonSerializer.Serialize(original.Assignment.Roster.Assignments))throw new InvalidOperationException("Complete original local assignment roster changed during acceptance.");
        var releaseRows=await ReadPersonnelReleasesCoreAsync(context,rows,Guard,true);var releases=releaseRows.Select(row=>CompanyWorkOrderPersonnelReleaseFacts.From(row,rows.Single(assignment=>assignment.ASSIGNMENT_ID==row.ASSIGNMENT_ID))).OrderBy(row=>row.AssignmentId,StringComparer.Ordinal).ToArray();
        if(JsonSerializer.Serialize(releases)!=JsonSerializer.Serialize(original.Assignment.Roster.Releases))throw new InvalidOperationException("Complete original local cancellation ledger changed during personnel acceptance.");
        var policyContext=execution?new CompanyPersonnelPolicyContext(context.Company,context.Database,context.ActorUserId,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(context))):original.Policies.Context;
        var policyRows=await ReadPersonnelPoliciesCoreAsync(policyContext,Guard,true);var policies=policyRows.Select(CompanyPersonnelPolicyVersion.From).ToArray();
        if(JsonSerializer.Serialize(policies)!=JsonSerializer.Serialize(original.Policies.Versions))throw new InvalidOperationException("Complete original local personnel policy catalog changed during acceptance.");
        var credentials=await ReadPersonnelCredentialsCoreAsync(context,original.Credentials.Personnel.Query,Guard,true);
        if(credentials.Personnel.Employment!=original.Credentials.Personnel.Employment || credentials.Personnel.License!=original.Credentials.Personnel.License || JsonSerializer.Serialize(credentials.Statuses)!=JsonSerializer.Serialize(original.Credentials.Statuses) || JsonSerializer.Serialize(credentials.Conditions)!=JsonSerializer.Serialize(original.Credentials.Conditions))throw new InvalidOperationException("Complete original local membership/licence/status/condition evidence changed during acceptance.");
        if(execution && CompanyPersonnelPolicyEvidenceReview.GetBlockers(original.Assignment,original.Policies,credentials,original.EligibilityEvidenceVersion).Any(value=>value!=CompanyPersonnelPolicyEvidenceReview.PendingAcceptanceBlocker))
            throw new InvalidOperationException("Current execution duty, employment or credentials do not satisfy the complete recorded company policy/window.");
        await Guard();
    }
}
