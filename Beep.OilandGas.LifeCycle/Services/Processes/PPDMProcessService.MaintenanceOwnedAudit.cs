using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.LifeCycle.Services.Processes;
public partial class PPDMProcessService
{
    private static string MaintenanceAuditGuid(string table,string id)=>Guid.ParseExact(ProcessDefinitionRevision.FromJson(table+"|"+id)[..32],"N").ToString("D").ToUpperInvariant();
    private PPDMGenericRepository MaintenanceDecisionWriter<T>(T row,T? previous,string table,string key,string actor,PPDMRowMutation operation,Func<Task> authority) where T:ModelEntityBase
        =>new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(T),_connectionName,table,rowAccess:new MaintenanceDecisionRowPolicy<T>(row,previous,table,key,actor,operation,authority));
    private sealed class MaintenanceDecisionRowPolicy<T>(T prepared,T? original,string expectedTable,string key,string actor,PPDMRowMutation operation,Func<Task> authority):IPPDMRowAccessPolicy where T:ModelEntityBase
    {
        private readonly T expected=CompanyInventorySourceReview.Copy(prepared);
        private readonly T? previous=original is null?null:CompanyInventorySourceReview.Copy(original);
        private async Task Require(Type type,string table){await authority();if(type!=typeof(T) || table!=expectedTable)throw new InvalidOperationException("This owned decision policy grants no other entity/table operation.");}
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table){await Require(type,table);return [];}
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)
        {
            await Require(type,table);if(operation!=PPDMRowMutation.Update || previous is null)throw new InvalidOperationException("This decision insert grants no general read.");return [new AppFilter[]{new(){FieldName=key,Operator="=",FilterValue=typeof(T).GetProperty(key)!.GetValue(previous)!.ToString()}}];
        }
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)
        {await Require(type,table);if(previous is null || rows.Count!=1 || JsonSerializer.Serialize(rows[0])!=JsonSerializer.Serialize(previous))throw new InvalidOperationException("Only the exact original owned decision process row may be read for update.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool includeInherited=true)=>throw new InvalidOperationException("This owned decision mutation grants no general effective access.");
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? persisted,PPDMRowMutation requested,string? user)
        {
            await Require(type,table);if(!ReferenceEquals(entity,prepared) || requested!=operation || user!=actor || (previous is null)!=(persisted is null) || previous is not null && JsonSerializer.Serialize(persisted)!=JsonSerializer.Serialize(previous))throw new InvalidOperationException("Exact original decision row, mutation, actor and preimage are required.");
            foreach(var name in new[]{"PPDM_GUID","ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","ROW_EFFECTIVE_DATE","ROW_EXPIRY_DATE","ROW_QUALITY"}){var property=typeof(T).GetProperty(name)!;property.SetValue(prepared,property.GetValue(expected));}
            if(JsonSerializer.Serialize(prepared)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("Only the frozen original decision business/audit row may be persisted.");
        }
    }
}
