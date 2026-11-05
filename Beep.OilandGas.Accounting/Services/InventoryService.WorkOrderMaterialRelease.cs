using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
using System.Data;
using System.Text.Json;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.PPDM39.Core;
namespace Beep.OilandGas.Accounting.Services;
public partial class InventoryService
{
    public async Task<CompanyWorkOrderMaterialReleaseRecovery> RecoverWorkOrderMaterialReleaseReferenceAsync(CompanyScope company,string actor,CompanyWorkOrderMaterialReleaseReferenceQuery query,Func<Task> requireAuthority)
    {
        query.Require();var allocation=await RecoverWorkOrderMaterialAllocationReferenceAsync(company,actor,query.Allocation,requireAuthority);var connection=await RequiredMaterialConnection(allocation.Command.Review.Source);var source=_editor.GetDataSource(connection)??throw new InvalidOperationException("Original release target is unavailable.");var id="wo-material-release-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{allocation.Recorded.ALLOCATION_ID}));
        async Task Guard(){await requireAuthority();await RequiredMaterialConnection(allocation.Command.Review.Source);if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("Original release target changed during recovery.");}
        async Task<WORK_ORDER_MATERIAL_RELEASE> Read()
        {
            await Guard();var raw=(await MaterialReleaseRepository(connection).GetAsync([new(){FieldName="RELEASE_ID",Operator="=",FilterValue=id}]))?.ToArray();await Guard();if(raw is not {Length:1} || raw[0] is not WORK_ORDER_MATERIAL_RELEASE row)throw new InvalidOperationException("Original release is not uniquely recorded.");
            if(row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.ROW_CREATED_BY!=actor || row.ALLOCATION_ID!=allocation.Recorded.ALLOCATION_ID || row.FIELD_ID!=query.Allocation.Task.FieldId || row.WORK_ORDER_ID!=query.Allocation.Task.WorkOrderId || row.PROCESS_INSTANCE_ID!=query.Allocation.Task.InstanceId)throw new UnauthorizedAccessException("Original release belongs to another account/company/task.");return CompanyInventorySourceReview.Copy(row);
        }
        var first=await Read();var command=CompanyWorkOrderMaterialReleaseCommand.Read(first);var result=new CompanyWorkOrderMaterialReleaseRecovery(command,first);result.Require(company,actor,query);if(System.Text.Json.JsonSerializer.Serialize(command.Original)!=System.Text.Json.JsonSerializer.Serialize(allocation))throw new InvalidOperationException("Original release reservation evidence changed.");var closing=await Read();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original release evidence changed during recovery.");return result;
    }
    private PPDMGenericRepository MaterialReleaseRepository(string connection)
    {
        const string table="WORK_ORDER_MATERIAL_RELEASE";var source=_editor.GetDataSource(connection)??throw new InvalidOperationException("Release datasource is unavailable.");var shape=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var keys=shape?.Fields?.Where(f=>f.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!="RELEASE_ID" || keys[0].AllowDBNull)throw new InvalidOperationException("Original release key metadata is unavailable; review the PRODUCTION Beep migration.");
        foreach(var (name,size) in new[]{("RELEASE_ID",128),("ALLOCATION_ID",128),("OWNER_TENANT_ID",64),("OWNER_COMPANY_BA_ID",128),("INVENTORY_ITEM_ID",128),("FIELD_ID",128),("WORK_ORDER_ID",128),("PROCESS_INSTANCE_ID",128),("COMMAND_JSON",4194304),("RECORDED_REVISION",64)})
        {var fields=shape!.Fields.Where(f=>f.FieldName==name).ToArray();if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || fields[0].AllowDBNull || fields[0].Size1<size)throw new InvalidOperationException("Complete original release metadata is unavailable.");}
        foreach(var (name,type) in new[]{("RELEASED_QUANTITY",typeof(decimal)),("RELEASED_AT_UTC",typeof(DateTime))}){var fields=shape!.Fields.Where(f=>f.FieldName==name).ToArray();if(fields is not {Length:1} || fields[0].Fieldtype!=type.FullName || fields[0].AllowDBNull)throw new InvalidOperationException("Native release quantity/UTC metadata is unavailable.");}
        return new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_MATERIAL_RELEASE),connection,table);
    }
    private async Task<WORK_ORDER_MATERIAL_RELEASE[]> MaterialReleases(string connection,CompanyScope company,string item)
    {
        var raw=(await MaterialReleaseRepository(connection).GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="INVENTORY_ITEM_ID",Operator="=",FilterValue=item}]))?.ToArray();
        if(raw is null || raw.Length>CompanyWorkOrderMaterialAllocationReview.MaximumAllocations || raw.Any(r=>r is not WORK_ORDER_MATERIAL_RELEASE))throw new InvalidOperationException("Complete bounded release population is unavailable; no records are truncated.");var rows=raw.Cast<WORK_ORDER_MATERIAL_RELEASE>().OrderBy(r=>r.ALLOCATION_ID,StringComparer.Ordinal).ToArray();
        if(rows.Any(r=>r.OWNER_TENANT_ID!=company.TenantId || r.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || r.INVENTORY_ITEM_ID!=item) || rows.Select(r=>r.ALLOCATION_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Length)throw new InvalidOperationException("Original release ownership/membership is ambiguous.");foreach(var row in rows)CompanyWorkOrderMaterialReleaseCommand.Read(row).RequireRecorded(row);return rows;
    }
    public async Task<WORK_ORDER_MATERIAL_RELEASE> ReleaseWorkOrderMaterialAsync(CompanyWorkOrderMaterialReleaseCommand original,Func<Task> requireAuthority,Func<Task> requireCurrentTask,bool readOnly=false)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var sourceReview=command.Original.Command.Review.Source;await requireAuthority();await requireCurrentTask();var connection=await RequiredMaterialConnection(sourceReview);var source=_editor.GetDataSource(connection)??throw new InvalidOperationException("Original release target is unavailable.");
        async Task Guard(bool owned){await requireAuthority();await RequiredMaterialConnection(sourceReview);if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("Original release target changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);}
        async Task OriginalReservation(bool owned)
        {
            await Guard(owned);var rows=(await MaterialAllocationRepository(connection).GetAsync([new(){FieldName="ALLOCATION_ID",Operator="=",FilterValue=command.Original.Recorded.ALLOCATION_ID}]))?.ToArray();await Guard(owned);
            if(rows is not {Length:1} || rows[0] is not WORK_ORDER_MATERIAL_ALLOCATION row || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(command.Original.Recorded))throw new InvalidOperationException("Complete original reservation changed before release.");command.Original.Command.RequireRecorded(row);
        }
        async Task<WORK_ORDER_MATERIAL_RELEASE?> Existing(bool owned)
        {
            await Guard(owned);var rows=(await MaterialReleaseRepository(connection).GetAsync([new(){FieldName="RELEASE_ID",Operator="=",FilterValue=command.ReleaseId()}]))?.ToArray();await Guard(owned);
            if(rows is null || rows.Length>1 || rows.Any(r=>r is not WORK_ORDER_MATERIAL_RELEASE))throw new InvalidOperationException("Original release identity is ambiguous.");if(rows.Length==0)return null;var row=(WORK_ORDER_MATERIAL_RELEASE)rows[0];command.RequireRecorded(row);return CompanyInventorySourceReview.Copy(row);
        }
        if(readOnly){await OriginalReservation(false);var first=await Existing(false)??throw new InvalidOperationException("Original release is not recorded.");await requireCurrentTask();await OriginalReservation(false);var closing=await Existing(false);if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original release changed during recovery.");return first;}
        WORK_ORDER_MATERIAL_RELEASE? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await OriginalReservation(true);confirmed=await Existing(true);if(confirmed is not null)return;var reservations=await MaterialAllocations(connection,sourceReview.Context.Company,sourceReview.Query.ItemId);var releases=await MaterialReleases(connection,sourceReview.Context.Company,sourceReview.Query.ItemId);
            foreach(var release in releases)CompanyWorkOrderMaterialReleaseReference.From(release,reservations.SingleOrDefault(r=>r.ALLOCATION_ID==release.ALLOCATION_ID)??throw new InvalidOperationException("Original reservation for a recorded release is missing."));
            if(releases.Any(r=>r.ALLOCATION_ID==command.Original.Recorded.ALLOCATION_ID))throw new InvalidOperationException("This reservation already has another original release.");var row=command.Prepare(DateTime.UtcNow);
            var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_MATERIAL_RELEASE),connection,"WORK_ORDER_MATERIAL_RELEASE",rowAccess:new MaterialReleaseInsertPolicy(row,()=>Guard(true)));await writer.InsertAsync(row,sourceReview.Context.ActorUserId);await OriginalReservation(true);confirmed=await Existing(true);if(confirmed is null)throw new InvalidOperationException("Original release write is unconfirmed.");await Guard(true);
        });
        await requireCurrentTask();await OriginalReservation(false);var durable=await Existing(false);if(confirmed is null || JsonSerializer.Serialize(confirmed)!=JsonSerializer.Serialize(durable))throw new InvalidOperationException("Committed release is unconfirmed; recover the retained original intent.");return durable!;
    }
    private sealed class MaterialReleaseInsertPolicy(WORK_ORDER_MATERIAL_RELEASE original,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private readonly WORK_ORDER_MATERIAL_RELEASE expected=CompanyInventorySourceReview.Copy(original);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(WORK_ORDER_MATERIAL_RELEASE) || table!="WORK_ORDER_MATERIAL_RELEASE" || operation!=PPDMRowMutation.Insert || previous is not null || actor!=expected.ROW_CREATED_BY || !ReferenceEquals(original,entity))throw new InvalidOperationException("Only original append-only release insertion is authorized.");
            original.PPDM_GUID=expected.PPDM_GUID;original.ACTIVE_IND=expected.ACTIVE_IND;original.ROW_CREATED_BY=expected.ROW_CREATED_BY;original.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;original.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;original.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;if(JsonSerializer.Serialize(original)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("Original release body/audit changed before insertion.");
        }
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Release insertion grants no reads.");
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Release insertion grants no reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Release insertion grants no reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Release insertion grants no reads.");
    }
}