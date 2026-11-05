using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.Accounting.Services;

public partial class AccountingPolicyService
{
    private static void RequireBillingSchema(IDataSource source)
    {
        var schema=source.GetEntityStructure(new EntityStructure{EntityName="ACCOUNTING_METHOD",DatasourceEntityName="ACCOUNTING_METHOD"},true);
        var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!="ACCOUNTING_METHOD_ID" || keys[0].Fieldtype!=typeof(string).FullName || keys[0].Size<128 || keys[0].AllowDBNull)
            throw new InvalidOperationException("Review the bounded canonical policy identity in the PRODUCTION Beep schema.");
        foreach(var (name,size) in new[]{("OWNER_TENANT_ID",128),("OWNER_COMPANY_BA_ID",128),("MATCHING_POLICY_JSON",4000),("MATCHING_POLICY_STATUS",32),("MATCHING_POLICY_COMMAND_JSON",CompanyBillingPolicyProof.MaximumLength)})
        {
            var fields=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();
            if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || fields[0].Size<size || !fields[0].AllowDBNull)
                throw new InvalidOperationException("Review the existing nullable bounded policy columns through the PRODUCTION Beep migration.");
        }
    }
    public async Task<CompanyBillingPolicyReview> GetCompanyBillingPoliciesAsync(CompanyScope company,string actor,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);PurchaseOrderOwnership.RequireScope(company);
        _=CompanyBillingPolicyReview.Create(company,actor,[]);await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION policy resolver is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The selected production binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The company policy datasource is unavailable.");
        async Task Guard()
        {await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original company billing-policy binding changed.");}
        await Guard();RequireBillingSchema(source);var repo=await GetRepoAsync<ACCOUNTING_METHOD>("ACCOUNTING_METHOD",connection);
        var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},
            new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="METHOD_TYPE",Operator="=",FilterValue=CompanyBillingPolicyProof.MethodType}]))?.ToArray();
        await Guard();if(raw is null || raw.Length>CompanyBillingPolicyReview.MaximumRecords || raw.Any(row=>row is not ACCOUNTING_METHOD))throw new InvalidOperationException("The complete bounded billing policy history is unavailable.");
        var review=CompanyBillingPolicyReview.Create(company,actor,raw.Cast<ACCOUNTING_METHOD>().ToArray());await Guard();return review;
    }
    public Task<ACCOUNTING_METHOD> PrepareCompanyBillingPolicyAsync(CompanyBillingPolicyDraft command,CompanyBillingPolicyReview displayed,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(displayed);command=PurchaseReceiptDecisionProof.Copy(command);displayed=PurchaseReceiptDecisionProof.Copy(displayed);command.Require(displayed);
        return RecordCompanyBillingPolicyAsync(command,null,displayed,requireAuthority);
    }
    public Task<ACCOUNTING_METHOD> ApproveCompanyBillingPolicyAsync(CompanyBillingPolicyApproval command,CompanyBillingPolicyReview displayed,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(displayed);command=PurchaseReceiptDecisionProof.Copy(command);displayed=PurchaseReceiptDecisionProof.Copy(displayed);command.Require(displayed);
        return RecordCompanyBillingPolicyAsync(null,command,displayed,requireAuthority);
    }
    private async Task<ACCOUNTING_METHOD> RecordCompanyBillingPolicyAsync(CompanyBillingPolicyDraft? draft,CompanyBillingPolicyApproval? approval,CompanyBillingPolicyReview displayed,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);var company=displayed.Company;var actor=displayed.ActorUserId;var id=draft?.PolicyId ??approval!.PolicyId;
        await requireAuthority();if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION policy resolver is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The selected production binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The policy datasource is unavailable.");
        async Task Guard(bool owned)
        {await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original billing-policy binding changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);}
        ACCOUNTING_METHOD? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);RequireBillingSchema(source);var repo=await GetRepoAsync<ACCOUNTING_METHOD>("ACCOUNTING_METHOD",connection);
            async Task<ACCOUNTING_METHOD?> Read()
            {
                await Guard(true);var raw=(await repo.GetAsync([new(){FieldName="ACCOUNTING_METHOD_ID",Operator="=",FilterValue=id}]))?.ToArray();await Guard(true);
                if(raw is null || raw.Length>1 || raw.Any(row=>row is not ACCOUNTING_METHOD))throw new InvalidOperationException("The original policy identity is unavailable or ambiguous.");
                if(raw.SingleOrDefault() is not ACCOUNTING_METHOD row)return null;
                if(row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.METHOD_TYPE!=CompanyBillingPolicyProof.MethodType)
                    throw new InvalidOperationException("The original policy identity belongs to another company or policy kind.");
                return PurchaseReceiptDecisionProof.Copy(row);
            }
            async Task Review(ACCOUNTING_METHOD? row,bool reconcileRecorded=false)
            {
                await Guard(true);var current=await GetCompanyBillingPoliciesAsync(company,actor,()=>Guard(true));ACCOUNTING_METHOD? original=row;
                if(row is not null)
                {
                    var proof=CompanyBillingPolicyProof.Require(company,row);
                    if(draft is not null)
                    {if(proof.Draft!=draft)throw new InvalidOperationException("Another original billing preparation intent is recorded.");original=null;}
                    else if(proof.Approval is not null)
                    {if(proof.Approval!=approval)throw new InvalidOperationException("Another original billing approval intent is recorded.");original=proof.Original;}
                    if(JsonSerializer.Serialize(current.Records.SingleOrDefault(value=>value.ACCOUNTING_METHOD_ID==id))!=JsonSerializer.Serialize(row))throw new InvalidOperationException("The complete history does not contain the exact original policy outcome.");
                    // A proven recorded command is an outcome, not a proposal against today's history.
                    // New writes and their transaction closure still require the exact original history below.
                    if(reconcileRecorded && (draft is not null || proof.Approval is not null)){await Guard(true);return;}
                }
                var recreated=CompanyBillingPolicyReview.Create(company,actor,current.Records.Where(value=>value.ACCOUNTING_METHOD_ID!=id).Concat(original is null?[]:new[]{original}).ToArray());
                if(draft is not null)draft.Require(recreated);else approval!.Require(recreated);await Guard(true);
            }
            var row=await Read();await Review(row,true);var newlyWritten=false;
            if(row is null || approval is not null && row.MATCHING_POLICY_STATUS=="DRAFT")
            {
                var original=row is null?null:PurchaseReceiptDecisionProof.Copy(row);CompanyBillingPolicyRecord record;CompanyBillingPolicyDraft prepared;
                if(draft is not null)
                {
                    prepared=draft;row=new ACCOUNTING_METHOD{ACCOUNTING_METHOD_ID=id,OWNER_TENANT_ID=company.TenantId,OWNER_COMPANY_BA_ID=company.BusinessAssociateId,
                        METHOD_TYPE=CompanyBillingPolicyProof.MethodType,MATCHING_POLICY_JSON=JsonSerializer.Serialize(draft.Rules),MATCHING_POLICY_STATUS="DRAFT",ACTIVE_IND="Y",
                        EFFECTIVE_DATE=draft.Rules.EffectiveFrom,EXPIRY_DATE=draft.Rules.EffectiveThrough,DESCRIPTION=draft.Rules.Version,REMARK=draft.Reason,SOURCE=CompanyBillingPolicyProof.MethodType,PPDM_GUID=draft.IntentId};
                    record=new(draft.IntentId,company,id,actor,DateTime.UtcNow,draft.Rules,draft.Reason,null,null,null,null);
                }
                else
                {
                    var proof=CompanyBillingPolicyProof.Require(company,row!);prepared=proof.Draft;
                    record=proof.Record with{ApprovalIntentId=approval!.IntentId,ApprovedBy=actor,ApprovedAtUtc=DateTime.UtcNow,ApprovalReason=approval.Reason};row!.MATCHING_POLICY_STATUS="APPROVED";
                    // Validate the proposed complete history before the provider mutation.
                    var candidate=PurchaseReceiptDecisionProof.Copy(row);candidate.ROW_CHANGED_BY=actor;candidate.ROW_CHANGED_DATE=record.ApprovedAtUtc;
                    candidate.MATCHING_POLICY_COMMAND_JSON=CompanyBillingPolicyProof.Create(record,prepared,approval,original,candidate);
                    _=CompanyBillingPolicyReview.Create(company,actor,displayed.Records.Where(value=>value.ACCOUNTING_METHOD_ID!=id).Append(candidate).ToArray());
                }
                var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(ACCOUNTING_METHOD),connection,"ACCOUNTING_METHOD",
                    rowAccess:new BillingPolicyMutation(record,prepared,approval,original,()=>Guard(true)));
                if(draft is not null)await writer.InsertAsync(row!,actor);else await writer.UpdateAsync(row!,actor);
                newlyWritten=true;
                var stored=await Read();if(JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("The stored billing policy differs from the reviewed write.");row=stored;
            }
            _=CompanyBillingPolicyProof.Require(company,row!);await Review(row,!newlyWritten);confirmed=row;await Guard(true);
        });
        await Guard(false);var finalRepo=await GetRepoAsync<ACCOUNTING_METHOD>("ACCOUNTING_METHOD",connection);
        var durable=(await finalRepo.GetAsync([new(){FieldName="ACCOUNTING_METHOD_ID",Operator="=",FilterValue=id}]))?.ToArray();
        if(durable is not {Length:1} || durable[0] is not ACCOUNTING_METHOD final || JsonSerializer.Serialize(final)!=JsonSerializer.Serialize(confirmed))throw new InvalidOperationException("Committed billing-policy outcome is unconfirmed. Reconcile the original intent.");
        var finalProof=CompanyBillingPolicyProof.Require(company,final);if(draft is not null?finalProof.Draft!=draft:finalProof.Approval!=approval)throw new InvalidOperationException("The committed original billing-policy intent is unconfirmed.");
        await Guard(false);return PurchaseReceiptDecisionProof.Copy(final);
    }
    private sealed class BillingPolicyMutation(CompanyBillingPolicyRecord record,CompanyBillingPolicyDraft draft,CompanyBillingPolicyApproval? approval,ACCOUNTING_METHOD? original,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private string Actor=>approval?.ActorUserId ??draft.ActorUserId;
        private async Task Table(Type type,string table){await guard();if(type!=typeof(ACCOUNTING_METHOD) || table!="ACCOUNTING_METHOD")throw new InvalidOperationException("Billing policy mutation is restricted to ACCOUNTING_METHOD.");}
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await Table(type,table);if(actor!=Actor || entity is not ACCOUNTING_METHOD row || row.ACCOUNTING_METHOD_ID!=draft.PolicyId || operation!=(approval is null?PPDMRowMutation.Insert:PPDMRowMutation.Update) || JsonSerializer.Serialize(previous)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Only the exact reviewed billing policy mutation is authorized.");
            // The repository has just populated audit values; retain those exact provider-bound values.
            var audited=approval is null?record with{PreparedAtUtc=row.ROW_CREATED_DATE ??throw new InvalidOperationException("Preparation audit is unavailable.")}:record with{ApprovedAtUtc=row.ROW_CHANGED_DATE};
            row.MATCHING_POLICY_COMMAND_JSON=CompanyBillingPolicyProof.Create(audited,draft,approval,original,row);_=CompanyBillingPolicyProof.Require(draft.Company,row);
        }
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)
        {await Table(type,table);if(original is null)throw new InvalidOperationException("Draft insertion cannot grant reads.");return new List<IReadOnlyList<AppFilter>>{new List<AppFilter>{new(){FieldName="ACCOUNTING_METHOD_ID",Operator="=",FilterValue=draft.PolicyId}}};}
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)
        {await Table(type,table);if(original is null)throw new InvalidOperationException("Draft insertion cannot grant reads.");return new List<AppFilter>{new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=draft.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=draft.Company.BusinessAssociateId}};}
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)
        {await Table(type,table);if(original is null || rows.Count!=1 || JsonSerializer.Serialize(rows[0])!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The exact prepared billing policy changed before approval.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Mutation policy cannot grant reads.");
    }
}
