using System.Data;
using TheTechIdea.Beep;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.DrillingAndConstruction.Services;
public partial class DrillingOperationService
{
    private static void RequireExecutionSchema(IDataSource source)
    {
        var schema=source.GetEntityStructure(new EntityStructure{EntityName="DRILLING_OPERATION",DatasourceEntityName="DRILLING_OPERATION"},true)??throw new InvalidOperationException("Review DRILLING_EXECUTION through Beep Migration before recording execution.");var keys=schema.Fields?.Where(field=>field.IsKey).ToArray();if(keys is not {Length:1} || keys[0].FieldName!="DRILLING_OPERATION_ID")throw new InvalidOperationException("An exact DRILLING_OPERATION_ID key is required.");
        void Field(string name,Type type,int width=0,bool required=false){var fields=schema.Fields!.Where(field=>field.FieldName==name).ToArray();if(fields.Length!=1 || fields[0].Fieldtype!=type.FullName || required && fields[0].AllowDBNull || width>0 && Math.Max(fields[0].Size,fields[0].Size1)<width)throw new InvalidOperationException("Review execution column "+name+" through Beep Migration before recording start.");}
        foreach(var name in new[]{"DRILLING_OPERATION_ID","OWNER_TENANT_ID","OWNER_COMPANY_BA_ID","FIELD_ID","WELL_UWI","PROGRAM_ID","PROGRAM_REVIEW_INSTANCE_ID","RECORDER_ROLE"})Field(name,typeof(string),128,true);
        foreach(var name in new[]{"PROGRAM_DRAFT_REVISION","PROGRAM_REVIEW_NATIVE_REVISION","EXECUTION_START_REVISION","RECORDER_PERSONA_CODE"})Field(name,typeof(string),64,true);Field("EXECUTION_START_JSON",typeof(string),DrillingExecutionStart.MaximumBodyLength,true);Field("STATUS",typeof(string),32,true);Field("SPUD_DATE",typeof(DateTime),required:true);
        foreach(var name in new[]{"SOURCE","ACTIVE_IND","PPDM_GUID","ROW_CREATED_BY","ROW_CHANGED_BY"})Field(name,typeof(string));foreach(var name in new[]{"ROW_CREATED_DATE","ROW_CHANGED_DATE"})Field(name,typeof(DateTime));
    }
    private async Task<DRILLING_OPERATION?> ReadExecutionRowAsync(DrillingExecutionBasis basis,string? operation=null,string? intent=null)
    {
        var scope=basis.Scope();var target=basis.Source.Basis.ExecutionDatabase;var repository=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(DRILLING_OPERATION),target.ConnectionName,"DRILLING_OPERATION");AppFilter Eq(string name,string value)=>new(){FieldName=name,FilterValue=value,Operator="="};
        async Task<object[]> Rows(List<AppFilter> filters)=>(await repository.GetAsync(filters))?.ToArray()??throw new InvalidOperationException("The execution rowset is unavailable; absence was not confirmed.");
        var rows=await Rows([Eq("OWNER_TENANT_ID",scope.Company.TenantId),Eq("OWNER_COMPANY_BA_ID",scope.Company.BusinessAssociateId),Eq("PROGRAM_ID",basis.Source.Program.PROGRAM_ID)]);
        if(rows.Length>1 || rows.Any(row=>row is not DRILLING_OPERATION))throw new InvalidOperationException("The original program execution population is ambiguous.");
        if(operation is not null)
        {
            var byId=await Rows([Eq("DRILLING_OPERATION_ID",operation)]);var byIntent=await Rows([Eq("OWNER_TENANT_ID",scope.Company.TenantId),Eq("OWNER_COMPANY_BA_ID",scope.Company.BusinessAssociateId),Eq("PPDM_GUID",intent!)]);
            if(byId.Length>1 || byIntent.Length>1 || byId.Any(row=>row is not DRILLING_OPERATION) || byIntent.Any(row=>row is not DRILLING_OPERATION) || byId.Length!=byIntent.Length || rows.Length!=byId.Length || JsonSerializer.Serialize(rows)!=JsonSerializer.Serialize(byId) || JsonSerializer.Serialize(byId)!=JsonSerializer.Serialize(byIntent))throw new InvalidOperationException("The original execution key/intent/program is missing, conflicting or ambiguous. Recover its original evidence.");
        }
        if(rows.Length==0)return null;var found=(DRILLING_OPERATION)rows[0];if(found.OWNER_TENANT_ID!=scope.Company.TenantId || found.OWNER_COMPANY_BA_ID!=scope.Company.BusinessAssociateId)throw new UnauthorizedAccessException("The execution belongs to another original company.");var original=DrillingExecutionStart.FromRecord(found);original.Basis.RequireSource(basis,false,false);return CompanyInventorySourceReview.Copy(found);
    }
    public async Task<DrillingExecutionStartReceipt> RecordExecutionStartAsync(DrillingExecutionStart input,Func<Task> authority,Func<Task<DrillingExecutionBasis>> readBasis,bool recover=false)
    {
        input.Require();var command=CompanyInventorySourceReview.Copy(input);var basis=command.Basis;var target=basis.Source.Basis.ExecutionDatabase;var source=_editor.GetDataSource(target.ConnectionName)??throw new InvalidOperationException("The original execution source is unavailable.");
        async Task Guard(bool owned){if(!ReferenceEquals(source,_editor.GetDataSource(target.ConnectionName)))throw new InvalidOperationException("The original execution source changed.");MigrationConnectionTarget.Validate(_editor,source,target.ConnectionName);if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority();if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);}
        async Task Basis(){await Guard(false);basis.RequireSource(await readBasis(),!recover);await Guard(false);}
        async Task<DRILLING_OPERATION?> Read(){var found=await ReadExecutionRowAsync(basis,command.OperationId(),command.IntentId.ToString("D"));if(found is not null && DrillingExecutionStart.FromRecord(found).Revision()!=command.Revision())throw new InvalidOperationException("The original execution start intent or recorded source changed.");return found;}
        await Basis();DRILLING_OPERATION? recorded=null;await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);RequireExecutionSchema(source);recorded=await Read();await Guard(true);
            if(recorded is null && !recover)
            {
                var row=command.ToRecord(DateTime.UtcNow);_commonColumnHandler.PrepareForInsert(row,basis.Scope().ActorUserId);row.EXECUTION_START_REVISION=DrillingExecutionStart.Digest(row);DrillingExecutionStart.FromRecord(row);await Guard(true);
                await PPDMDataSourceTransaction.WithSourceAsync(source,()=>{BeepOperationResult.EnsureSuccess(source.InsertEntity("DRILLING_OPERATION",row),"Insert","DRILLING_OPERATION");return Task.FromResult(true);});await Guard(true);recorded=await Read();if(recorded is null)throw new InvalidOperationException("The original execution was not confirmed recorded.");
            }
            await Guard(true);
        });
        await Guard(false);RequireExecutionSchema(source);var closing=await Read();if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original execution commit is unconfirmed. Recover the retained original.");await Basis();var result=new DrillingExecutionStartReceipt(command.Revision(),command.OperationId(),closing,DateTimeOffset.UtcNow);result.Require(command);return result;
    }
    public async Task<DrillingExecutionStatus> ReadExecutionStatusAsync(DrillingExecutionBasis basis,Func<Task> authority)
    {
        basis.Require();var target=basis.Source.Basis.ExecutionDatabase;var source=_editor.GetDataSource(target.ConnectionName)??throw new InvalidOperationException("The original execution source is unavailable.");
        async Task Guard(bool owned){if(!ReferenceEquals(source,_editor.GetDataSource(target.ConnectionName)))throw new InvalidOperationException("The original execution source changed.");MigrationConnectionTarget.Validate(_editor,source,target.ConnectionName);if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority();if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);}
        DRILLING_OPERATION? found=null;await Guard(false);await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{await Guard(true);RequireExecutionSchema(source);found=await ReadExecutionRowAsync(basis);await Guard(true);if(JsonSerializer.Serialize(found)!=JsonSerializer.Serialize(await ReadExecutionRowAsync(basis)))throw new InvalidOperationException("The original execution changed during readback.");});await Guard(false);if(JsonSerializer.Serialize(found)!=JsonSerializer.Serialize(await ReadExecutionRowAsync(basis)))throw new InvalidOperationException("The original execution or its confirmed absence changed.");await Guard(false);var result=new DrillingExecutionStatus(basis,found,"",DateTimeOffset.UtcNow);result=result with{Revision=result.ContentRevision()};result.Require();return result;
    }
}
