using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
public partial class ContractorManagementService
{
    private static void RequirePersonnelPolicySchema(IDataSource source)
    {
        const string table="WORK_ORDER_PERSONNEL_POLICY";if(!source.CheckEntityExist(table))throw new InvalidOperationException("Review the existing PPDM_CORE Beep personnel policy manifest; missing schema is not an unconfigured policy.");
        var shape=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var keys=shape?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!="POLICY_RECORD_ID" || keys[0].AllowDBNull)throw new InvalidOperationException("Exact native personnel policy key is unavailable.");
        foreach(var (name,size) in new[]{("POLICY_JSON",1024*1024),("COMMAND_JSON",2*1024*1024),("RECORDED_REVISION",64)})
        {
            var fields=shape!.Fields.Where(field=>field.FieldName==name).ToArray();if(fields.Length!=1 || fields[0].Fieldtype!=typeof(string).FullName || fields[0].AllowDBNull || fields[0].Size1<size)throw new InvalidOperationException("Bounded personnel policy proof metadata is unavailable.");
        }
    }
    private async Task<WORK_ORDER_PERSONNEL_POLICY[]> ReadPersonnelPoliciesCoreAsync(CompanyPersonnelPolicyContext context,Func<Task> requireAuthority,bool owned=false)
    {
        context.Require();await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);
        var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original personnel policy native source is unavailable.");
        if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequirePersonnelPolicySchema(source);
        var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_PERSONNEL_POLICY),context.Database.ConnectionName,"WORK_ORDER_PERSONNEL_POLICY");
        var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=context.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=context.Company.BusinessAssociateId}])).ToArray();
        await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);
        if(!ReferenceEquals(source,_editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original personnel policy native target changed.");
        if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        if(raw.Length>4096 || raw.Any(row=>row is not WORK_ORDER_PERSONNEL_POLICY))throw new InvalidOperationException("Complete bounded native personnel policy history is unavailable.");
        var rows=raw.Cast<WORK_ORDER_PERSONNEL_POLICY>().ToArray();
        if(rows.Any(row=>row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.ACTIVE_IND!="Y"))throw new UnauthorizedAccessException("Policy company/active headers differ before private proof parsing.");
        if(rows.Select(row=>row.POLICY_RECORD_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Length)throw new InvalidOperationException("Policy record identities are ambiguous.");
        return rows.Select(CompanyInventorySourceReview.Copy).OrderBy(row=>row.POLICY_RECORD_ID,StringComparer.Ordinal).ToArray();
    }
    public async Task<CompanyPersonnelPolicyCatalog> ReadPersonnelPoliciesAsync(CompanyPersonnelPolicyContext context,Func<Task> requireAuthority)
    {
        var first=await ReadPersonnelPoliciesCoreAsync(context,requireAuthority);var closing=await ReadPersonnelPoliciesCoreAsync(context,requireAuthority);
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Complete original policy history changed during review.");
        var result=CompanyPersonnelPolicyCatalog.Create(context,closing.Select(CompanyPersonnelPolicyVersion.From).ToArray(),DateTime.UtcNow);await requireAuthority();return result;
    }
    public async Task<WORK_ORDER_PERSONNEL_POLICY> RecordPersonnelPolicyAsync(CompanyPersonnelPolicyCommand original,Func<Task> requireAuthority,bool readOnly=false)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);var command=CompanyInventorySourceReview.Copy(original);command.Require();var context=command.Context;
        await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original policy native source is unavailable.");
        async Task Guard(bool owned=false)
        {
            await requireAuthority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);if(!ReferenceEquals(source,_editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original policy native target changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequirePersonnelPolicySchema(source);
        }
        WORK_ORDER_PERSONNEL_POLICY? Existing(IReadOnlyList<WORK_ORDER_PERSONNEL_POLICY> rows)
        {
            var row=rows.SingleOrDefault(row=>row.POLICY_RECORD_ID==command.RecordId());if(row is null)return null;
            if(row.ROW_CREATED_BY!=context.ActorUserId)throw new UnauthorizedAccessException("Only the original policy maker may recover its private command.");
            command.RequireRecorded(row);return row;
        }
        async Task<WORK_ORDER_PERSONNEL_POLICY[]> Read(bool owned=false)=>await ReadPersonnelPoliciesCoreAsync(context,()=>Guard(owned),owned);
        void Baseline(IReadOnlyList<WORK_ORDER_PERSONNEL_POLICY> rows)=>command.RequireCatalog(CompanyPersonnelPolicyCatalog.Create(context,rows.Select(CompanyPersonnelPolicyVersion.From).ToArray(),DateTime.UtcNow));
        var first=await Read();var saved=Existing(first);
        if(readOnly || saved is not null)
        {
            if(saved is null)throw new InvalidOperationException("Original policy is not recorded; keep its retained intent.");var closing=Existing(await Read());
            if(JsonSerializer.Serialize(saved)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original policy changed during recovery.");return saved;
        }
        command.Policy.RequireCurrentEligibility();Baseline(first);WORK_ORDER_PERSONNEL_POLICY? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);var before=await Read(true);confirmed=Existing(before);if(confirmed is not null)return;Baseline(before);
            var row=command.Prepare(DateTime.UtcNow);var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_PERSONNEL_POLICY),context.Database.ConnectionName,"WORK_ORDER_PERSONNEL_POLICY",rowAccess:new PersonnelPolicyInsertPolicy(row,command,()=>Guard(true)));
            await writer.InsertAsync(row,context.ActorUserId);var after=await Read(true);confirmed=Existing(after);if(confirmed is null)throw new InvalidOperationException("Original policy append is unconfirmed.");
            Baseline(after.Where(value=>value.POLICY_RECORD_ID!=row.POLICY_RECORD_ID).ToArray());CompanyPersonnelPolicyCatalog.Create(context,after.Select(CompanyPersonnelPolicyVersion.From).ToArray(),DateTime.UtcNow).Require();await Guard(true);
        });
        var durable=Existing(await Read());if(confirmed is null || JsonSerializer.Serialize(confirmed)!=JsonSerializer.Serialize(durable))throw new InvalidOperationException("Committed original personnel policy is unconfirmed. Recover its retained intent.");await Guard();return durable!;
    }
    public async Task<CompanyPersonnelPolicyRecovery> RecoverPersonnelPolicyReferenceAsync(CompanyPersonnelPolicyContext context,CompanyPersonnelPolicyReference reference,Func<Task> requireAuthority)
    {
        reference.Require();var rows=await ReadPersonnelPoliciesCoreAsync(context,requireAuthority);var row=rows.SingleOrDefault(row=>row.POLICY_RECORD_ID==reference.RecordId)??throw new InvalidOperationException("Original personnel policy is unavailable.");
        if(row.ROW_CREATED_BY!=context.ActorUserId)throw new UnauthorizedAccessException("Recorded original policy maker closes before private proof parsing.");
        var command=CompanyPersonnelPolicyCommand.Read(row);var result=new CompanyPersonnelPolicyRecovery(command,row);result.Require(context.Company,context.ActorUserId,reference);
        var saved=await RecordPersonnelPolicyAsync(command,requireAuthority,true);if(JsonSerializer.Serialize(saved)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("Complete original policy changed during reference recovery.");await requireAuthority();return new(command,saved);
    }
    private sealed class PersonnelPolicyInsertPolicy(WORK_ORDER_PERSONNEL_POLICY original,CompanyPersonnelPolicyCommand command,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private readonly WORK_ORDER_PERSONNEL_POLICY expected=CompanyInventorySourceReview.Copy(original);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(WORK_ORDER_PERSONNEL_POLICY) || table!="WORK_ORDER_PERSONNEL_POLICY" || operation!=PPDMRowMutation.Insert || previous is not null || !ReferenceEquals(entity,original) || actor!=expected.ROW_CREATED_BY)throw new InvalidOperationException("Only one exact original personnel policy append is authorized.");
            original.PPDM_GUID=expected.PPDM_GUID;original.ACTIVE_IND=expected.ACTIVE_IND;original.ROW_CREATED_BY=expected.ROW_CREATED_BY;original.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;original.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;original.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;command.RequireRecorded(original);
        }
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Policy append grants no general reads.");
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Policy append grants no general reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Policy append grants no general reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Policy append grants no general access.");
    }
}
