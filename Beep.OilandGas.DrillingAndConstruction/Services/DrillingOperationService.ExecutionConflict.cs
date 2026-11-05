using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.DrillingAndConstruction.Services;

public partial class DrillingOperationService
{
    public async Task<DrillingExecutionConflictReview> ReadExecutionConflictAsync(DrillingExecutionStart input,
        Func<Task> authority, Func<Task<DrillingExecutionBasis>> readBasis)
    {
        input.Require(); ArgumentNullException.ThrowIfNull(authority); ArgumentNullException.ThrowIfNull(readBasis);
        var original=CompanyInventorySourceReview.Copy(input);var basis=original.Basis;var scope=basis.Scope();
        var target=basis.Source.Basis.ExecutionDatabase;
        var source=_editor.GetDataSource(target.ConnectionName)??throw new InvalidOperationException("The original execution source is unavailable.");
        async Task Guard(bool owned)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(target.ConnectionName)))throw new InvalidOperationException("The original execution source changed.");
            MigrationConnectionTarget.Validate(_editor,source,target.ConnectionName);
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task Basis()
        {
            await Guard(false);basis.RequireSource(await readBasis(),false);await Guard(false);
        }
        var repository=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(DRILLING_OPERATION),target.ConnectionName,"DRILLING_OPERATION");
        AppFilter Eq(string name,string value)=>new(){FieldName=name,FilterValue=value,Operator="="};
        async Task<DRILLING_OPERATION> Read()
        {
            RequireExecutionSchema(source);
            var byId=(await repository.GetAsync([Eq("DRILLING_OPERATION_ID",original.OperationId())]))?.ToArray()
                ??throw new InvalidOperationException("The original execution key lookup is unavailable.");
            var byIntent=(await repository.GetAsync([Eq("OWNER_TENANT_ID",scope.Company.TenantId),Eq("OWNER_COMPANY_BA_ID",scope.Company.BusinessAssociateId),Eq("PPDM_GUID",original.IntentId.ToString("D"))]))?.ToArray()
                ??throw new InvalidOperationException("The original execution intent lookup is unavailable.");
            if(byId.Length!=0 || byIntent.Length!=0)
                throw new InvalidOperationException("The original key or intent has native evidence. Use exact original recovery; it cannot be discarded as a different execution.");
            var competing=await ReadExecutionRowAsync(basis)
                ??throw new InvalidOperationException("No competing recorded execution is confirmed. Recover the exact original outcome instead.");
            var review=new DrillingExecutionConflictReview(original.Revision(),original.OperationId(),true,competing,"",DateTimeOffset.UtcNow);
            review=review with{Revision=review.ContentRevision()};review.Require(original);return competing;
        }
        await Basis();DRILLING_OPERATION? recorded=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);recorded=await Read();await Guard(true);
            if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(await Read()))
                throw new InvalidOperationException("The exact competing execution changed during reconciliation.");
            await Guard(true);
        });
        await Guard(false);
        if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(await Read()))
            throw new InvalidOperationException("The original absence or competing execution changed after reconciliation.");
        await Basis();
        if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(await Read()))
            throw new InvalidOperationException("The competing execution changed around closing source confirmation.");
        await Guard(false);
        var result=new DrillingExecutionConflictReview(original.Revision(),original.OperationId(),true,recorded!,"",DateTimeOffset.UtcNow);
        result=result with{Revision=result.ContentRevision()};result.Require(original);return result;
    }
}
