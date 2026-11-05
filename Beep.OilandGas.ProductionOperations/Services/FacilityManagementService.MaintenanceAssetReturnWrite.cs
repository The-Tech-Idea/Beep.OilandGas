using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.ProductionOperations.Services;

public sealed partial class FacilityManagementService
{
    // Joins the complete command's existing transaction; never starts/commits one or creates a separate receipt.
    internal async Task ApplyQualifiedAssetReturnRowsAsync(CompanyMaintenanceQualifiedAssets reviewed,WorkOrderDetailModel order,
        IReadOnlyList<CompanyMaintenanceQualifiedReturnObservation> observations,string assignedRole,string returnId,
        DateTime activatedAtUtc,string reason,QualifiedAssetReturnRows original,Func<Task> authority)
    {
        var prepared=CompanyInventorySourceReview.Copy(original);
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original qualified native return source is unavailable.");
        async Task Guard()
        {
            PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);await authority();
            PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original qualified return source changed before a native write.");
        }
        async Task Parents()
        {
            await Guard();var current=await ReadMaintenanceQualifiedAssetsAsync(reviewed.Context,order,Guard,true);
            if(current.IdentityRevision()!=reviewed.IdentityRevision())throw new UnauthorizedAccessException("Current original qualified ownership, installation or affected-target WRITE authority changed.");
            await Guard();
        }
        await Parents();
        var expected=await RevalidateQualifiedAssetReturnRowsAsync(reviewed,order,observations,assignedRole,returnId,activatedAtUtc,reason,prepared,Guard);
        if(JsonSerializer.Serialize(prepared)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("Prepared qualified before/after rows differ from their complete original source, policy, observation and audit proof.");
        var changes=CompanyMaintenanceQualifiedAssetReturn.Resolve(reviewed,order,observations,assignedRole,activatedAtUtc);
        foreach(var change in changes)
        {
            if(change.AssetType=="FACILITY")
            {
                var before=prepared.BeforeFacilities.Single(row=>row.FACILITY_ID==change.AssetId && row.FACILITY_TYPE==change.FacilityType && row.STATUS_ID==change.OriginalStatusId);
                var expired=prepared.AfterFacilities.Single(row=>row.FACILITY_ID==change.AssetId && row.FACILITY_TYPE==change.FacilityType && row.STATUS_ID==change.OriginalStatusId);
                var active=prepared.AfterFacilities.Single(row=>row.FACILITY_ID==change.AssetId && row.FACILITY_TYPE==change.FacilityType && row.STATUS_ID==change.StatusId(returnId));
                await Write(expired,before,"FACILITY_STATUS",["FACILITY_ID","FACILITY_TYPE","STATUS_ID"],PPDMRowMutation.Update);
                await Write(active,null,"FACILITY_STATUS",["FACILITY_ID","FACILITY_TYPE","STATUS_ID"],PPDMRowMutation.Insert);
            }
            else
            {
                var before=prepared.BeforeEquipment.Single(row=>row.EQUIPMENT_ID==change.AssetId && row.EQUIP_STATUS_ID==change.OriginalStatusId);
                var expired=prepared.AfterEquipment.Single(row=>row.EQUIPMENT_ID==change.AssetId && row.EQUIP_STATUS_ID==change.OriginalStatusId);
                var active=prepared.AfterEquipment.Single(row=>row.EQUIPMENT_ID==change.AssetId && row.EQUIP_STATUS_ID==change.StatusId(returnId));
                await Write(expired,before,"EQUIPMENT_STATUS",["EQUIPMENT_ID","EQUIP_STATUS_ID"],PPDMRowMutation.Update);
                await Write(active,null,"EQUIPMENT_STATUS",["EQUIPMENT_ID","EQUIP_STATUS_ID"],PPDMRowMutation.Insert);
            }
        }
        await Parents();
        var facilities=new List<FACILITY_STATUS>();var equipment=new List<EQUIPMENT_STATUS>();
        foreach(var parent in reviewed.Facilities)facilities.AddRange(await MaintenanceAssetRowsCoreAsync<FACILITY_STATUS>(Guard,
            ["FACILITY_ID","FACILITY_TYPE","STATUS_ID"],true,Equal("FACILITY_ID",parent.FacilityId),Equal("FACILITY_TYPE",parent.FacilityType)));
        foreach(var parent in reviewed.Equipment)equipment.AddRange(await MaintenanceAssetRowsCoreAsync<EQUIPMENT_STATUS>(Guard,
            ["EQUIPMENT_ID","EQUIP_STATUS_ID"],true,Equal("EQUIPMENT_ID",parent.EquipmentId)));
        var actualFacilities=facilities.OrderBy(row=>row.FACILITY_ID,StringComparer.Ordinal).ThenBy(row=>row.FACILITY_TYPE,StringComparer.Ordinal).ThenBy(row=>row.STATUS_ID,StringComparer.Ordinal).ToArray();
        var actualEquipment=equipment.OrderBy(row=>row.EQUIPMENT_ID,StringComparer.Ordinal).ThenBy(row=>row.EQUIP_STATUS_ID,StringComparer.Ordinal).ToArray();
        if(JsonSerializer.Serialize(actualFacilities)!=JsonSerializer.Serialize(prepared.AfterFacilities) || JsonSerializer.Serialize(actualEquipment)!=JsonSerializer.Serialize(prepared.AfterEquipment))
            throw new InvalidOperationException("Native qualified return readback differs from its entire declared graph; the owning command must roll back.");
        var zone=await MaintenanceAssetRowsCoreAsync<R_TIMEZONE>(Guard,["TIMEZONE","ACTIVE_IND"],true,Equal("TIMEZONE","UTC"));
        if(zone.Length!=1 || JsonSerializer.Serialize(zone[0])!=JsonSerializer.Serialize(prepared.UtcReference))throw new InvalidOperationException("Native UTC reference changed around qualified return writes.");
        await Parents();

        static AppFilter Equal(string column,string value)=>new(){FieldName=column,Operator="=",FilterValue=value};
        async Task Write<T>(T row,T? before,string table,string[] keys,PPDMRowMutation operation) where T:class
        {
            await Parents();var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(T),_connectionName,table,
                rowAccess:new QualifiedReturnRowPolicy<T>(row,before,table,keys,reviewed.Context.ActorUserId,operation,Guard,Parents));
            if(operation==PPDMRowMutation.Update)await repo.UpdateAsync(row,reviewed.Context.ActorUserId);else await repo.InsertAsync(row,reviewed.Context.ActorUserId);
            await Parents();
        }
    }

    private sealed class QualifiedReturnRowPolicy<T>(T prepared,T? original,string expectedTable,string[] keys,string actor,
        PPDMRowMutation operation,Func<Task> authority,Func<Task> requireTargets):IPPDMRowAccessPolicy where T:class
    {
        private readonly T expected=CompanyInventorySourceReview.Copy(prepared);
        private readonly T? previous=original is null?null:CompanyInventorySourceReview.Copy(original);
        private async Task Require(Type type,string table)
        {
            await authority();if(type!=typeof(T) || table!=expectedTable || table is not ("FACILITY_STATUS" or "EQUIPMENT_STATUS"))
                throw new InvalidOperationException("Qualified return policy grants no other native table/type operation.");
        }
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table){await Require(type,table);return [];}
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)
        {
            await Require(type,table);if(previous is null || operation!=PPDMRowMutation.Update)throw new InvalidOperationException("A qualified return insert grants no general read.");
            return [keys.Select(key=>new AppFilter{FieldName=key,Operator="=",FilterValue=typeof(T).GetProperty(key)!.GetValue(previous)!.ToString()}).ToArray()];
        }
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)
        {
            await Require(type,table);if(previous is null || rows.Count!=1 || JsonSerializer.Serialize(rows[0])!=JsonSerializer.Serialize(previous))
                throw new InvalidOperationException("Only the exact complete native qualified preimage may be read for its update.");
        }
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool includeInherited=true)=>throw new InvalidOperationException("Qualified return mutation grants no general effective access.");
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? persisted,PPDMRowMutation requested,string? user)
        {
            // Exact preimage reads stay private; target ownership/WRITE is refreshed at mutation validation and both write bookends.
            await Require(type,table);await requireTargets();if(!ReferenceEquals(entity,prepared) || requested!=operation || user!=actor || (previous is null)!=(persisted is null) || previous is not null && JsonSerializer.Serialize(persisted)!=JsonSerializer.Serialize(previous))
                throw new InvalidOperationException("Exact original qualified row, native identity, actor and operation are required.");
            foreach(var name in new[]{"PPDM_GUID","ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","ROW_EFFECTIVE_DATE","ROW_EXPIRY_DATE","ROW_QUALITY"})
            {var property=typeof(T).GetProperty(name)??throw new InvalidOperationException("Canonical qualified native audit fields are required.");property.SetValue(prepared,property.GetValue(expected));}
            if(JsonSerializer.Serialize(prepared)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("Only the frozen declared qualified business/audit effect may be persisted.");
        }
    }
}
