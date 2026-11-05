using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ProductionOperations.Services;
public sealed partial class FacilityManagementService:ICompanyMaintenanceReturnPolicyOwner
{
    private async Task RequireMaintenanceReturnPolicyContextAsync(CompanyMaintenanceReturnPolicyContext context)
    {
        context.Require();if(context.Database.ConnectionName!=_connectionName || _resolveModuleConnection is not null && await _resolveModuleConnection("PPDM_CORE")!=_connectionName)throw new InvalidOperationException("Original maintenance policy CORE source binding changed.");
    }
    private async Task RequireMaintenanceReturnPolicySchemaAsync(IDataSource source)
    {
        const string table="WORK_ORDER_MAINTENANCE_RETURN_POLICY";
        var metadata=await _metadata.GetTableMetadataAsync(table);
        if(metadata?.TableName!=table || metadata.EntityTypeName!=typeof(WORK_ORDER_MAINTENANCE_RETURN_POLICY).FullName || metadata.PrimaryKeyColumn!="POLICY_RECORD_ID" || metadata.Module!="PPDM_CORE")throw new InvalidOperationException("Canonical type-owned maintenance policy CORE metadata is unavailable.");
        if(!source.CheckEntityExist(table))throw new InvalidOperationException("Review and apply the explicit PPDM_CORE Beep maintenance return policy manifest before using this workflow; schema is never created by a policy command.");
        var shape=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var expected=_editor.classCreator.ConvertToEntityStructure(typeof(WORK_ORDER_MAINTENANCE_RETURN_POLICY));var keys=shape?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!="POLICY_RECORD_ID" || keys[0].AllowDBNull || shape!.Fields.GroupBy(field=>field.FieldName,StringComparer.OrdinalIgnoreCase).Any(group=>group.Count()!=1) || expected?.Fields is null || expected.Fields.Any(field=>shape.Fields.Count(actual=>actual.FieldName==field.FieldName && actual.Fieldtype==field.Fieldtype)!=1) || source.ErrorObject?.Flag==Errors.Failed)throw new InvalidOperationException("Complete physical maintenance policy fields and the exact non-null native primary key are required.");
        foreach(var (name,size) in new[]{("POLICY_JSON",1024*1024),("COMMAND_JSON",2*1024*1024),("RECORDED_REVISION",64)})
        {
            var fields=shape.Fields.Where(field=>field.FieldName==name).ToArray();if(fields.Length!=1 || fields[0].Fieldtype!=typeof(string).FullName || fields[0].AllowDBNull || fields[0].Size1<size)throw new InvalidOperationException("Bounded maintenance policy proof capacity is unavailable.");
        }
    }
    private async Task<WORK_ORDER_MAINTENANCE_RETURN_POLICY[]> ReadMaintenanceReturnPoliciesCoreAsync(CompanyMaintenanceReturnPolicyContext context,Func<Task> requireAuthority,bool owned=false)
    {
        context.Require();await requireAuthority();await RequireMaintenanceReturnPolicyContextAsync(context);
        var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original maintenance return policy native source is unavailable.");
        if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await RequireMaintenanceReturnPolicySchemaAsync(source);
        var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_MAINTENANCE_RETURN_POLICY),context.Database.ConnectionName,"WORK_ORDER_MAINTENANCE_RETURN_POLICY",requiredReadColumns:["POLICY_RECORD_ID","OWNER_TENANT_ID","OWNER_COMPANY_BA_ID","POLICY_KEY","VERSION_NO","EFFECTIVE_FROM_UTC","EFFECTIVE_TO_UTC","POLICY_JSON","COMMAND_JSON","RECORDED_REVISION","ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","PPDM_GUID","SOURCE"]);
        var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=context.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=context.Company.BusinessAssociateId}])).ToArray();
        await requireAuthority();await RequireMaintenanceReturnPolicyContextAsync(context);
        if(!ReferenceEquals(source,_editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original maintenance return policy native target changed.");
        if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await RequireMaintenanceReturnPolicySchemaAsync(source);await requireAuthority();await RequireMaintenanceReturnPolicyContextAsync(context);
        if(raw.Length>4096 || raw.Any(row=>row is not WORK_ORDER_MAINTENANCE_RETURN_POLICY))throw new InvalidOperationException("Complete bounded native maintenance return policy history is unavailable.");
        var rows=raw.Cast<WORK_ORDER_MAINTENANCE_RETURN_POLICY>().ToArray();
        if(rows.Any(row=>row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.ACTIVE_IND!="Y"))throw new UnauthorizedAccessException("Policy company/active headers differ before private proof parsing.");
        if(rows.Select(row=>row.POLICY_RECORD_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Length)throw new InvalidOperationException("Policy record identities are ambiguous.");
        return rows.Select(row=>
        {
            // This app-owned table writes only UTC values, with the original UTC command retained.
            // Providers may omit DateTime.Kind; preserve the declared storage clock without a zone conversion.
            static DateTime UtcColumn(DateTime value)=>value.Kind==DateTimeKind.Local?throw new InvalidOperationException("Declared UTC policy storage cannot use machine-local timestamps."):DateTime.SpecifyKind(value,DateTimeKind.Utc);
            var copy=CompanyInventorySourceReview.Copy(row);copy.EFFECTIVE_FROM_UTC=UtcColumn(copy.EFFECTIVE_FROM_UTC);copy.EFFECTIVE_TO_UTC=UtcColumn(copy.EFFECTIVE_TO_UTC);copy.ROW_CREATED_DATE=UtcColumn(copy.ROW_CREATED_DATE!.Value);copy.ROW_CHANGED_DATE=UtcColumn(copy.ROW_CHANGED_DATE!.Value);return copy;
        }).OrderBy(row=>row.POLICY_RECORD_ID,StringComparer.Ordinal).ToArray();
    }
    public Task<CompanyMaintenanceReturnPolicyCatalog> ReadMaintenanceReturnPoliciesAsync(CompanyMaintenanceReturnPolicyContext context,Func<Task> requireAuthority) =>
        ReadMaintenanceReturnPoliciesAsync(context,requireAuthority,false);

    internal async Task<CompanyMaintenanceReturnPolicyCatalog> ReadMaintenanceReturnPoliciesAsync(CompanyMaintenanceReturnPolicyContext context,Func<Task> requireAuthority,bool owned)
    {
        context.Require();
        var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original maintenance return policy source is unavailable.");
        async Task Guard()
        {
            RequireMaintenanceEvidenceLane(source,owned);await requireAuthority();await RequireMaintenanceReturnPolicyContextAsync(context);
            RequireMaintenanceEvidenceLane(source,owned);
            if(!ReferenceEquals(source,_editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original maintenance policy source changed around the complete catalog.");
        }
        await Guard();
        var first=await ReadMaintenanceReturnPoliciesCoreAsync(context,requireAuthority,owned);var closing=await ReadMaintenanceReturnPoliciesCoreAsync(context,requireAuthority,owned);
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Complete original policy history changed during review.");
        var result=CompanyMaintenanceReturnPolicyCatalog.Create(context,closing.Select(CompanyMaintenanceReturnPolicyVersion.From).ToArray(),DateTime.UtcNow);await Guard();return result;
    }
    public async Task<WORK_ORDER_MAINTENANCE_RETURN_POLICY> RecordMaintenanceReturnPolicyAsync(CompanyMaintenanceReturnPolicyCommand original,Func<Task> requireAuthority,bool readOnly=false)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);var command=CompanyInventorySourceReview.Copy(original);command.Require();var context=command.Context;
        await requireAuthority();await RequireMaintenanceReturnPolicyContextAsync(context);var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original policy native source is unavailable.");
        async Task Guard(bool owned=false)
        {
            await requireAuthority();await RequireMaintenanceReturnPolicyContextAsync(context);if(!ReferenceEquals(source,_editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original policy native target changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await RequireMaintenanceReturnPolicySchemaAsync(source);
        }
        WORK_ORDER_MAINTENANCE_RETURN_POLICY? Existing(IReadOnlyList<WORK_ORDER_MAINTENANCE_RETURN_POLICY> rows)
        {
            var row=rows.SingleOrDefault(row=>row.POLICY_RECORD_ID==command.RecordId());if(row is null)return null;
            if(row.ROW_CREATED_BY!=context.ActorUserId)throw new UnauthorizedAccessException("Only the original policy maker may recover its private command.");
            command.RequireRecorded(row);return row;
        }
        async Task<WORK_ORDER_MAINTENANCE_RETURN_POLICY[]> Read(bool owned=false)=>await ReadMaintenanceReturnPoliciesCoreAsync(context,()=>Guard(owned),owned);
        void Baseline(IReadOnlyList<WORK_ORDER_MAINTENANCE_RETURN_POLICY> rows)=>command.RequireCatalog(CompanyMaintenanceReturnPolicyCatalog.Create(context,rows.Select(CompanyMaintenanceReturnPolicyVersion.From).ToArray(),DateTime.UtcNow));
        var first=await Read();var saved=Existing(first);
        if(readOnly || saved is not null)
        {
            if(saved is null)throw new InvalidOperationException("Original policy is not recorded; keep its retained intent.");var closing=Existing(await Read());
            if(JsonSerializer.Serialize(saved)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original policy changed during recovery.");return saved;
        }
        command.Policy.Require();Baseline(first);WORK_ORDER_MAINTENANCE_RETURN_POLICY? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);var before=await Read(true);confirmed=Existing(before);if(confirmed is not null)return;Baseline(before);
            var row=command.Prepare(DateTime.UtcNow);var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_MAINTENANCE_RETURN_POLICY),context.Database.ConnectionName,"WORK_ORDER_MAINTENANCE_RETURN_POLICY",rowAccess:new MaintenanceReturnPolicyInsertPolicy(row,command,()=>Guard(true)));
            await writer.InsertAsync(row,context.ActorUserId);var after=await Read(true);confirmed=Existing(after);if(confirmed is null)throw new InvalidOperationException("Original policy append is unconfirmed.");
            Baseline(after.Where(value=>value.POLICY_RECORD_ID!=row.POLICY_RECORD_ID).ToArray());CompanyMaintenanceReturnPolicyCatalog.Create(context,after.Select(CompanyMaintenanceReturnPolicyVersion.From).ToArray(),DateTime.UtcNow).Require();await Guard(true);
        });
        var durable=Existing(await Read());if(confirmed is null || JsonSerializer.Serialize(confirmed)!=JsonSerializer.Serialize(durable))throw new InvalidOperationException("Committed original maintenance return policy is unconfirmed. Recover its retained intent.");await Guard();return durable!;
    }
    public async Task<CompanyMaintenanceReturnPolicyRecovery> RecoverMaintenanceReturnPolicyReferenceAsync(CompanyMaintenanceReturnPolicyContext context,CompanyMaintenanceReturnPolicyReference reference,Func<Task> requireAuthority)
    {
        reference.Require();var rows=await ReadMaintenanceReturnPoliciesCoreAsync(context,requireAuthority);var row=rows.SingleOrDefault(row=>row.POLICY_RECORD_ID==reference.RecordId)??throw new InvalidOperationException("Original maintenance return policy is unavailable.");
        if(row.ROW_CREATED_BY!=context.ActorUserId)throw new UnauthorizedAccessException("Recorded original policy maker closes before private proof parsing.");
        var command=CompanyMaintenanceReturnPolicyCommand.Read(row);var result=new CompanyMaintenanceReturnPolicyRecovery(command,row);result.Require(context.Company,context.ActorUserId,reference);
        var saved=await RecordMaintenanceReturnPolicyAsync(command,requireAuthority,true);if(JsonSerializer.Serialize(saved)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("Complete original policy changed during reference recovery.");await requireAuthority();return new(command,saved);
    }
    private sealed class MaintenanceReturnPolicyInsertPolicy(WORK_ORDER_MAINTENANCE_RETURN_POLICY original,CompanyMaintenanceReturnPolicyCommand command,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private readonly WORK_ORDER_MAINTENANCE_RETURN_POLICY expected=CompanyInventorySourceReview.Copy(original);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(WORK_ORDER_MAINTENANCE_RETURN_POLICY) || table!="WORK_ORDER_MAINTENANCE_RETURN_POLICY" || operation!=PPDMRowMutation.Insert || previous is not null || !ReferenceEquals(entity,original) || actor!=expected.ROW_CREATED_BY)throw new InvalidOperationException("Only one exact original maintenance return policy append is authorized.");
            original.PPDM_GUID=expected.PPDM_GUID;original.ACTIVE_IND=expected.ACTIVE_IND;original.ROW_CREATED_BY=expected.ROW_CREATED_BY;original.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;original.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;original.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;command.RequireRecorded(original);
        }
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Policy append grants no general reads.");
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Policy append grants no general reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Policy append grants no general reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Policy append grants no general access.");
    }
}
