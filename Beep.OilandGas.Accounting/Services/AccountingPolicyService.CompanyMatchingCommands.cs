using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.Accounting.Services;
public partial class AccountingPolicyService
{
    public async Task<ACCOUNTING_METHOD> RecordCompanyMatchingPolicyAsync(CompanyMatchingPolicyCommand command,CompanyMatchingPolicyReview displayed,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(displayed);ArgumentNullException.ThrowIfNull(requireAuthority);
        command=PurchaseReceiptDecisionProof.Copy(command);displayed=PurchaseReceiptDecisionProof.Copy(displayed);command.Require(displayed);await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION resolver is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The selected production binding is unavailable.");
        var source=_editor.GetDataSource(connection) ?? throw new InvalidOperationException("The policy datasource is unavailable.");
        async Task Guard(bool owned)
        {
            await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original matching-policy binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
        }
        ACCOUNTING_METHOD? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);
            var schema=source.GetEntityStructure(new EntityStructure{EntityName="ACCOUNTING_METHOD",DatasourceEntityName="ACCOUNTING_METHOD"},true);
            var identities=schema?.Fields?.Where(field=>field.FieldName=="ACCOUNTING_METHOD_ID").ToArray();
            if(identities is not {Length:1} || identities[0].Fieldtype!=typeof(string).FullName || identities[0].Size<128 || identities[0].AllowDBNull || !identities[0].IsKey)
                throw new InvalidOperationException("The bounded canonical policy identity is unconfirmed. Review the PRODUCTION Beep schema.");
            var fields=schema?.Fields?.Where(field=>field.FieldName=="MATCHING_POLICY_COMMAND_JSON").ToArray();
            if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || !fields[0].AllowDBNull || fields[0].Size<CompanyMatchingPolicyProof.MaximumLength)
                throw new InvalidOperationException("Review the nullable bounded policy command column in the PRODUCTION Beep migration.");
            var repo=await GetRepoAsync<ACCOUNTING_METHOD>("ACCOUNTING_METHOD",connection);
            async Task<ACCOUNTING_METHOD?> Read()
            {
                await Guard(true);var raw=(await repo.GetAsync([new(){FieldName="ACCOUNTING_METHOD_ID",Operator="=",FilterValue=command.PolicyId}]))?.ToArray();
                if(raw is null || raw.Length>1 || raw.Any(item=>item is not ACCOUNTING_METHOD))throw new InvalidOperationException("The original policy identity is unavailable or ambiguous.");
                var row=raw.SingleOrDefault() as ACCOUNTING_METHOD;
                if(row is not null && (row.OWNER_TENANT_ID!=command.Company.TenantId || row.OWNER_COMPANY_BA_ID!=command.Company.BusinessAssociateId))throw new InvalidOperationException("The original policy identity belongs to another company.");
                return row is null?null:PurchaseReceiptDecisionProof.Copy(row);
            }
            async Task Review(ACCOUNTING_METHOD? row)
            {
                await Guard(true);var current=await GetCompanyMatchingPoliciesAsync(command.Company,command.ActorUserId);ACCOUNTING_METHOD? original=row;
                if(row is not null && (command.Action=="DRAFT" || row.MATCHING_POLICY_STATUS=="APPROVED"))
                {
                    var proof=CompanyMatchingPolicyProof.Require(command.Company,row);
                    if(proof.Command!=command)throw new InvalidOperationException("Another original policy intent is recorded. Do not overwrite it.");
                    original=proof.Original;
                }
                if(row is not null && JsonSerializer.Serialize(current.Records.SingleOrDefault(item=>item.ACCOUNTING_METHOD_ID==command.PolicyId))!=JsonSerializer.Serialize(row))
                    throw new InvalidOperationException("The complete company history does not contain the original policy outcome.");
                var recreated=CompanyMatchingPolicyReview.Create(command.Company,command.ActorUserId,
                    current.Records.Where(item=>item.ACCOUNTING_METHOD_ID!=command.PolicyId).Concat(original is null?[]:new[]{original}).ToArray());
                command.Require(recreated);await Guard(true);
            }
            var row=await Read();await Review(row);
            var alreadyRecorded=row?.MATCHING_POLICY_STATUS==(command.Action=="DRAFT"?"DRAFT":"APPROVED");
            if(!alreadyRecorded)
            {
                var original=row is null?null:PurchaseReceiptDecisionProof.Copy(row);
                if(command.Action=="DRAFT")row=new ACCOUNTING_METHOD{ACCOUNTING_METHOD_ID=command.PolicyId,OWNER_TENANT_ID=command.Company.TenantId,OWNER_COMPANY_BA_ID=command.Company.BusinessAssociateId,
                    METHOD_TYPE=CompanyMatchingPolicyReview.MethodType,MATCHING_POLICY_JSON=JsonSerializer.Serialize(command.Rules),MATCHING_POLICY_STATUS="DRAFT",ACTIVE_IND="Y",
                    EFFECTIVE_DATE=command.EffectiveAtUtc,EXPIRY_DATE=command.ExpiresAtUtc,DESCRIPTION=command.Rules.PolicyVersion,REMARK=command.Reason,SOURCE=CompanyMatchingPolicyReview.MethodType,PPDM_GUID=Guid.ParseExact(command.IntentId,"N").ToString()};
                else row!.MATCHING_POLICY_STATUS="APPROVED";
                var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(ACCOUNTING_METHOD),connection,"ACCOUNTING_METHOD",
                    rowAccess:new MatchingPolicyMutation(command,original,()=>Guard(true)));
                if(command.Action=="DRAFT")await writer.InsertAsync(row!,command.ActorUserId);else await writer.UpdateAsync(row!,command.ActorUserId);
                var stored=await Read();if(JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("The stored policy differs from the reviewed write.");row=stored;
            }
            _=CompanyMatchingPolicyProof.Require(command.Company,row!);await Review(row);confirmed=row;await Guard(true);
        });
        await Guard(false);var finalRepo=await GetRepoAsync<ACCOUNTING_METHOD>("ACCOUNTING_METHOD",connection);
        var durable=(await finalRepo.GetAsync([new(){FieldName="ACCOUNTING_METHOD_ID",Operator="=",FilterValue=command.PolicyId}]))?.ToArray();
        if(durable is not {Length:1} || durable[0] is not ACCOUNTING_METHOD final || JsonSerializer.Serialize(final)!=JsonSerializer.Serialize(confirmed) || CompanyMatchingPolicyProof.Require(command.Company,final).Command!=command)
            throw new InvalidOperationException("Committed policy outcome is unconfirmed. Reconcile the original intent.");
        await Guard(false);return PurchaseReceiptDecisionProof.Copy(final);
    }
    private sealed class MatchingPolicyMutation(CompanyMatchingPolicyCommand command,ACCOUNTING_METHOD? original,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private async Task Table(Type type,string table){await guard();if(type!=typeof(ACCOUNTING_METHOD) || table!="ACCOUNTING_METHOD")throw new InvalidOperationException("The mutation is restricted to ACCOUNTING_METHOD.");}
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await Table(type,table);
            if(actor!=command.ActorUserId || entity is not ACCOUNTING_METHOD row || row.ACCOUNTING_METHOD_ID!=command.PolicyId ||
                operation!=(command.Action=="DRAFT"?PPDMRowMutation.Insert:PPDMRowMutation.Update) || JsonSerializer.Serialize(previous)!=JsonSerializer.Serialize(original))
                throw new InvalidOperationException("Only the original reviewed policy mutation is authorized.");
            row.MATCHING_POLICY_COMMAND_JSON=CompanyMatchingPolicyProof.Create(command,original,row);_=CompanyMatchingPolicyProof.Require(command.Company,row);
        }
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)
        {await Table(type,table);if(original is null)throw new InvalidOperationException("Draft insertion cannot grant reads.");return new List<IReadOnlyList<AppFilter>>{new List<AppFilter>{new(){FieldName="ACCOUNTING_METHOD_ID",Operator="=",FilterValue=command.PolicyId}}};}
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)
        {await Table(type,table);if(original is null)throw new InvalidOperationException("Draft insertion cannot grant reads.");return new List<AppFilter>{new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=command.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=command.Company.BusinessAssociateId}};}
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)
        {await Table(type,table);if(original is null || rows.Count!=1 || JsonSerializer.Serialize(rows[0])!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The prepared policy changed before approval.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Mutation policy cannot grant reads.");
    }
}
