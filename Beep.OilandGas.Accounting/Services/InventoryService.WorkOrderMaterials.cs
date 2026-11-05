using System.Data;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.PPDM39.Core;
namespace Beep.OilandGas.Accounting.Services;
public partial class InventoryService
{
    public async Task<CompanyMaterialReservationQueue> ReadMaterialReservationQueueAsync(FieldSelectionReview context,Func<string,bool> canReadWorkOrder,Func<Task> requireAuthority)
    {
        FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);ArgumentNullException.ThrowIfNull(canReadWorkOrder);ArgumentNullException.ThrowIfNull(requireAuthority);await requireAuthority();if(_resolveConnection is null || await _resolveConnection()!=context.Database.ConnectionName || string.IsNullOrWhiteSpace(context.FieldId))throw new InvalidOperationException("Original current FIELD/PRODUCTION queue binding is required.");var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original reservation queue target is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=context.Database.ConnectionName || !ReferenceEquals(source,_editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original queue target changed.");}
        async Task<CompanyMaterialReservationQueueEntry[]> Read()
        {
            await Guard();var filters=new List<AppFilter>{new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=context.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=context.Company.BusinessAssociateId},new(){FieldName="FIELD_ID",Operator="=",FilterValue=context.FieldId}};
            var raw=(await MaterialAllocationRepository(context.Database.ConnectionName).GetAsync(filters))?.ToArray();var releaseRaw=(await MaterialReleaseRepository(context.Database.ConnectionName).GetAsync(filters))?.ToArray();await Guard();
            if(raw is null || releaseRaw is null || raw.Length>CompanyWorkOrderMaterialAllocationReview.MaximumAllocations || releaseRaw.Length>CompanyWorkOrderMaterialAllocationReview.MaximumAllocations || raw.Any(row=>row is not WORK_ORDER_MATERIAL_ALLOCATION) || releaseRaw.Any(row=>row is not WORK_ORDER_MATERIAL_RELEASE))throw new InvalidOperationException("Complete bounded FIELD reservation/release population is unavailable; no rows are truncated.");
            var all=raw.Cast<WORK_ORDER_MATERIAL_ALLOCATION>().ToArray();var allReleases=releaseRaw.Cast<WORK_ORDER_MATERIAL_RELEASE>().ToArray();
            if(all.Any(row=>row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.FIELD_ID!=context.FieldId) || allReleases.Any(row=>row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.FIELD_ID!=context.FieldId))throw new InvalidOperationException("FIELD reservation query returned foreign ownership.");
            // Apply direct current order rights before parsing any private original command evidence.
            var rows=all.Where(row=>canReadWorkOrder(row.WORK_ORDER_ID)).ToArray();var releases=allReleases.Where(row=>canReadWorkOrder(row.WORK_ORDER_ID)).ToArray();
            if(rows.Select(row=>row.ALLOCATION_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Length || releases.Select(row=>row.ALLOCATION_ID).Distinct(StringComparer.Ordinal).Count()!=releases.Length)throw new InvalidOperationException("Authorized reservation/release identities are ambiguous.");
            foreach(var release in releases)CompanyWorkOrderMaterialReleaseReference.From(release,rows.SingleOrDefault(row=>row.ALLOCATION_ID==release.ALLOCATION_ID)??throw new InvalidOperationException("Authorized release lacks its original reservation."));
            var usageByItem=new Dictionary<string,CompanyWorkOrderMaterialUsageFacts[]>(StringComparer.Ordinal);
            var usageSchema=source.CheckEntityExist("WORK_ORDER_MATERIAL_USAGE");
            if(usageSchema)
            {
                foreach(var itemId in rows.Select(row=>row.INVENTORY_ITEM_ID).Distinct(StringComparer.Ordinal))
                {
                    await Guard();var inventory=await GetCompanyInventorySourceAsync(context.Company,itemId,context.ActorUserId)
                        ??throw new InvalidOperationException("Original authorized reservation item evidence is unavailable.");
                    await RequireMaterialUsagePopulation(context.Database.ConnectionName,inventory,Guard,true);
                    usageByItem[itemId]=inventory.Transactions.Where(row=>row.TRANSACTION_TYPE=="WORK_ORDER_MATERIAL_USE" || row.WORK_ORDER_MATERIAL_USAGE_JSON is not null)
                        .Select(CompanyWorkOrderMaterialUsageFacts.From).OrderBy(row=>row.UsageId,StringComparer.Ordinal).ToArray();await Guard();
                }
            }
            var result=rows.OrderBy(row=>row.ALLOCATION_ID,StringComparer.Ordinal).Select(row=>new CompanyMaterialReservationQueueEntry(CompanyWorkOrderMaterialReservationFacts.From(row),releases.SingleOrDefault(r=>r.ALLOCATION_ID==row.ALLOCATION_ID) is {} release?CompanyWorkOrderMaterialReleaseReference.From(release,row):null)
                {Usages=usageSchema?usageByItem[row.INVENTORY_ITEM_ID].Where(usage=>usage.AllocationId==row.ALLOCATION_ID).ToArray():null}).ToArray();
            await Guard();if(source.CheckEntityExist("WORK_ORDER_MATERIAL_USAGE")!=usageSchema)throw new InvalidOperationException("Material usage schema availability changed around the authorized queue.");return result;
        }
        var first=await Read();var closing=await Read();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Complete authorized FIELD reservation/release queue changed.");await Guard();return CompanyMaterialReservationQueue.Create(context,closing,DateTime.UtcNow);
    }
    public async Task<CompanyWorkOrderMaterialReservationFacts> ReadWorkOrderMaterialReservationFactsAsync(CompanyScope company,CompanyMaintenanceReceivingQuery task,Guid intentId,Func<Task> requireAuthority)
    {
        task.Require();PurchaseOrderOwnership.RequireScope(company);if(intentId==Guid.Empty || _resolveConnection is null)throw new ArgumentException("Original material reservation intent and target are required.");await requireAuthority();var connection=await _resolveConnection();var source=_editor.GetDataSource(connection)??throw new InvalidOperationException("Original material target is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("Original material target changed.");}
        async Task<WORK_ORDER_MATERIAL_ALLOCATION> Read()
        {
            await Guard();var rows=(await MaterialAllocationRepository(connection).GetAsync([new(){FieldName="ALLOCATION_ID",Operator="=",FilterValue=CompanyWorkOrderMaterialReservationFacts.IdFor(company,intentId)}]))?.ToArray();await Guard();
            if(rows is not {Length:1} || rows[0] is not WORK_ORDER_MATERIAL_ALLOCATION row)throw new InvalidOperationException("Unique original reservation is unavailable.");if(row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.FIELD_ID!=task.FieldId || row.WORK_ORDER_ID!=task.WorkOrderId || row.PROCESS_INSTANCE_ID!=task.InstanceId)throw new UnauthorizedAccessException("Original reservation belongs to another company/task.");return CompanyInventorySourceReview.Copy(row);
        }
        var first=await Read();var result=CompanyWorkOrderMaterialReservationFacts.From(first);result.Require(company,task,intentId);var closing=await Read();if(System.Text.Json.JsonSerializer.Serialize(first)!=System.Text.Json.JsonSerializer.Serialize(closing))throw new InvalidOperationException("Complete original material reservation changed.");await Guard();return result;
    }
    public async Task<CompanyWorkOrderMaterialAllocationRecovery> RecoverWorkOrderMaterialAllocationReferenceAsync(CompanyScope company,string actor,CompanyWorkOrderMaterialAllocationReferenceQuery query,Func<Task> requireAuthority)
    {
        query.Require();PurchaseOrderOwnership.RequireScope(company);if(string.IsNullOrWhiteSpace(actor) || _resolveConnection is null)throw new UnauthorizedAccessException("Original reservation account/target is required.");await requireAuthority();var connection=await _resolveConnection();var source=_editor.GetDataSource(connection)??throw new InvalidOperationException("Original reservation target is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("Original reservation target changed during recovery.");}
        var id="wo-material-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,IntentId=query.IntentId}));
        async Task<WORK_ORDER_MATERIAL_ALLOCATION> Read()
        {
            await Guard();var raw=(await MaterialAllocationRepository(connection).GetAsync([new(){FieldName="ALLOCATION_ID",Operator="=",FilterValue=id}]))?.ToArray();await Guard();
            if(raw is not {Length:1} || raw[0] is not WORK_ORDER_MATERIAL_ALLOCATION row)throw new InvalidOperationException("The unique original reservation is unavailable.");
            // Check public ownership/task columns before deserializing private retained command evidence.
            if(row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.ROW_CREATED_BY!=actor || row.FIELD_ID!=query.Task.FieldId || row.WORK_ORDER_ID!=query.Task.WorkOrderId || row.PROCESS_INSTANCE_ID!=query.Task.InstanceId)throw new UnauthorizedAccessException("The original reservation belongs to another company/account/task.");return CompanyInventorySourceReview.Copy(row);
        }
        var first=await Read();var original=CompanyWorkOrderMaterialAllocationCommand.Read(first);var result=new CompanyWorkOrderMaterialAllocationRecovery(original,first);result.Require(company,actor,query);
        var closing=await Read();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original reservation changed during read-only recovery.");await Guard();return result;
    }
    private PPDMGenericRepository MaterialAllocationRepository(string connection)
    {
        const string table="WORK_ORDER_MATERIAL_ALLOCATION";var source=_editor.GetDataSource(connection)??throw new InvalidOperationException("Original allocation datasource is unavailable.");
        var shape=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var keys=shape?.Fields?.Where(f=>f.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!="ALLOCATION_ID" || keys[0].AllowDBNull)throw new InvalidOperationException("Original nonnullable allocation key metadata is required; review the PRODUCTION Beep migration.");
        foreach(var (name,size) in new[]{("ALLOCATION_ID",128),("OWNER_TENANT_ID",64),("OWNER_COMPANY_BA_ID",128),("INVENTORY_ITEM_ID",128),("FIELD_ID",128),("WORK_ORDER_ID",128),("PROCESS_INSTANCE_ID",128),("QUANTITY_UNIT",64),("WAREHOUSE_ID",128),("LOCATION_ID",128),("COMMAND_JSON",2097152),("RECORDED_REVISION",64)})
        {var fields=shape!.Fields.Where(f=>f.FieldName==name).ToArray();if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || fields[0].AllowDBNull || fields[0].Size1<size)throw new InvalidOperationException("Complete original allocation metadata is unavailable.");}
        foreach(var (name,type) in new[]{("QUANTITY",typeof(decimal)),("VALID_FROM_UTC",typeof(DateTime)),("VALID_TO_UTC",typeof(DateTime))})
        {var fields=shape!.Fields.Where(f=>f.FieldName==name).ToArray();if(fields is not {Length:1} || fields[0].Fieldtype!=type.FullName || fields[0].AllowDBNull)throw new InvalidOperationException("Native allocation quantity/effectivity metadata is unavailable.");}
        return new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_MATERIAL_ALLOCATION),connection,table);
    }
    private async Task<WORK_ORDER_MATERIAL_ALLOCATION[]> MaterialAllocations(string connection,CompanyScope company,string item)
    {
        var rows=(await MaterialAllocationRepository(connection).GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="INVENTORY_ITEM_ID",Operator="=",FilterValue=item}]))?.ToArray();
        if(rows is null || rows.Length>CompanyWorkOrderMaterialAllocationReview.MaximumAllocations || rows.Any(r=>r is not WORK_ORDER_MATERIAL_ALLOCATION))throw new InvalidOperationException("Complete bounded competing allocations are unconfirmed; no rows are truncated.");
        var typed=rows.Cast<WORK_ORDER_MATERIAL_ALLOCATION>().OrderBy(r=>r.ALLOCATION_ID,StringComparer.Ordinal).ToArray();
        if(typed.Any(r=>r.OWNER_TENANT_ID!=company.TenantId || r.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || r.INVENTORY_ITEM_ID!=item) || typed.Select(r=>r.ALLOCATION_ID).Distinct(StringComparer.Ordinal).Count()!=typed.Length)throw new InvalidOperationException("Competing allocation ownership or identities are ambiguous.");
        foreach(var row in typed)CompanyWorkOrderMaterialAllocationCommand.Read(row).RequireRecorded(row);return typed;
    }
    public Task<CompanyWorkOrderMaterialAllocationReview> ReviewWorkOrderMaterialAllocationsAsync(CompanyWorkOrderMaterialSourceReview original,Func<Task> requireAuthority)
        =>ReviewMaterialAllocationsCoreAsync(original,requireAuthority,original.Context.ActorUserId);
    private async Task<CompanyWorkOrderMaterialAllocationReview> ReviewMaterialAllocationsCoreAsync(CompanyWorkOrderMaterialSourceReview original,Func<Task> requireAuthority,string viewer)
    {
        original.Require(original.Context.Company,original.Context.ActorUserId,original.Query);await requireAuthority();var connection=await RequiredMaterialConnection(original);
        var first=await MaterialAllocations(connection,original.Context.Company,original.Query.ItemId);var initialReleases=await MaterialReleases(connection,original.Context.Company,original.Query.ItemId);await requireAuthority();
        var current=await GetCompanyInventorySourceAsync(original.Context.Company,original.Query.ItemId,viewer)??throw new InvalidOperationException("Original owned inventory is unavailable.");
        await RequireMaterialUsagePopulation(connection,current,requireAuthority);
        var closing=await MaterialAllocations(connection,original.Context.Company,original.Query.ItemId);var closingReleases=await MaterialReleases(connection,original.Context.Company,original.Query.ItemId);await requireAuthority();await RequiredMaterialConnection(original);
        if(current.Revision!=original.Inventory.Revision || JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing) || JsonSerializer.Serialize(initialReleases)!=JsonSerializer.Serialize(closingReleases))throw new InvalidOperationException("Inventory or competing reservations changed during allocation review.");
        return CompanyWorkOrderMaterialAllocationReview.Create(original,closing,closingReleases,DateTime.UtcNow);
    }
    /// <summary>Close exact accepted stock proof using explicit idle or caller-owned core isolation.</summary>
    public Task RequireAcceptedMaterialCurrentProofAsync(CompanyWorkOrderExecutionPermitIntent proposal,CompanyWorkOrderAcceptedMaterialReview original,Func<Task> requireAuthority,bool ownedCore=false,string? coreConnectionName=null)
        =>RequireMaterialProofCoreAsync(proposal,original,requireAuthority,ownedCore,coreConnectionName,original.Material.Source.Context.ActorUserId);

    public Task RequireExecutionMaterialCurrentProofAsync(FieldSelectionReview executor,CompanyWorkOrderExecutionPermitIntent proposal,CompanyWorkOrderAcceptedMaterialReview original,Func<Task> requireAuthority)
    {
        RequireExecutionMaterialContext(executor,proposal,original);
        return RequireMaterialProofCoreAsync(proposal,original,requireAuthority,false,null,executor.ActorUserId);
    }
    public Task<bool> RequireExecutionMaterialOwnedCoreProofAsync(FieldSelectionReview executor,CompanyWorkOrderExecutionPermitIntent proposal,CompanyWorkOrderAcceptedMaterialReview original,Func<Task> requireAuthority,string coreConnectionName)
    {
        RequireExecutionMaterialContext(executor,proposal,original);
        return RequireMaterialOwnedCoreAsync(proposal,original,requireAuthority,coreConnectionName,executor.ActorUserId);
    }
    private static void RequireExecutionMaterialContext(FieldSelectionReview executor,CompanyWorkOrderExecutionPermitIntent proposal,CompanyWorkOrderAcceptedMaterialReview original)
    {
        original.Require(proposal,original.Material.Source.WorkOrder);
        if(executor.Company!=proposal.Context.Company || executor.Database!=original.Material.Source.Context.Database || executor.FieldId!=proposal.Task.FieldId)
            throw new UnauthorizedAccessException("Execution stock proof requires the original company/FIELD/PRODUCTION binding.");
        FieldSelectionReviewValidator.RequireReview(executor,proposal.Context.Company,executor.ActorUserId);
        var utc=DateTime.UtcNow;if(original.Blockers.Count!=0 || utc<proposal.ValidFromUtc || utc>=proposal.ValidToUtc)throw new InvalidOperationException("Execution stock proof requires accepted material within the recorded permit window.");
    }
    private async Task RequireMaterialProofCoreAsync(CompanyWorkOrderExecutionPermitIntent proposal,CompanyWorkOrderAcceptedMaterialReview original,Func<Task> requireAuthority,bool ownedCore,string? coreConnectionName,string viewer)
    {
        ArgumentNullException.ThrowIfNull(original);ArgumentNullException.ThrowIfNull(requireAuthority);original.Require(proposal,original.Material.Source.WorkOrder);
        var connection=await RequiredMaterialConnection(original.Material.Source);var source=_editor.GetDataSource(connection)??throw new InvalidOperationException("Original accepted stock source is unavailable.");
        var core=ownedCore && coreConnectionName is not null?_editor.GetDataSource(coreConnectionName)??throw new InvalidOperationException("Original owned approval core source is unavailable."):null;
        var sharedCore=ownedCore && (coreConnectionName is null || ReferenceEquals(source,core));
        if(ownedCore && !sharedCore)throw new InvalidOperationException("Separate stock proof must close outside the owned core transaction.");
        async Task Guard()
        {
            if(core is not null)PPDMDataSourceTransaction.RequireOwned(core,IsolationLevel.Serializable);
            if(sharedCore)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await requireAuthority();await RequiredMaterialConnection(original.Material.Source);
            if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("Original accepted stock source changed during approval proof.");
            if(coreConnectionName is not null && !ReferenceEquals(core,_editor.GetDataSource(coreConnectionName)))throw new InvalidOperationException("Original owned approval core source changed during stock proof.");
            if(core is not null)PPDMDataSourceTransaction.RequireOwned(core,IsolationLevel.Serializable);
            if(sharedCore)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task RequireOriginalBinding()
        {
            await Guard();var rows=(await MaterialAllocationRepository(connection).GetAsync([new(){FieldName="ALLOCATION_ID",Operator="=",FilterValue=original.Reservation.AllocationId}])).ToArray();
            if(rows.Length!=1 || rows[0] is not WORK_ORDER_MATERIAL_ALLOCATION row)throw new InvalidOperationException("The original recorded reservation is unavailable or ambiguous.");
            if(row.OWNER_TENANT_ID!=proposal.Context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=proposal.Context.Company.BusinessAssociateId || row.FIELD_ID!=proposal.Task.FieldId || row.WORK_ORDER_ID!=proposal.Task.WorkOrderId || row.PROCESS_INSTANCE_ID!=proposal.Task.InstanceId)throw new UnauthorizedAccessException("The original reservation public company/task headers differ before private binding proof.");
            if(CompanyWorkOrderMaterialAllocationCommand.Read(row).Review.Source.Context.Database!=original.Material.Source.Context.Database)throw new InvalidOperationException("The original recorded reservation native binding differs from current stock proof.");
            await Guard();
        }
        await RequireOriginalBinding();var task=proposal.Task;var first=await ReadWorkOrderMaterialReservationFactsAsync(proposal.Context.Company,task,original.Acceptance.AllocationIntentId,Guard);
        var current=await ReviewMaterialAllocationsCoreAsync(original.Material.Source,Guard,viewer);var closing=await ReadWorkOrderMaterialReservationFactsAsync(proposal.Context.Company,task,original.Acceptance.AllocationIntentId,Guard);
        if(first!=original.Reservation || first!=closing || JsonSerializer.Serialize(current.Allocations)!=JsonSerializer.Serialize(original.Material.Allocations) || JsonSerializer.Serialize(current.Releases)!=JsonSerializer.Serialize(original.Material.Releases) || CompanyWorkOrderAcceptedMaterialReview.GetBlockers(proposal,original.Acceptance,closing,current,DateTime.UtcNow).Length!=0)throw new InvalidOperationException("Complete original current stock/reservation/release proof changed during approval.");
        await RequireOriginalBinding();
    }
    /// <summary>Read stock only when it shares the actual owned core source; separate stock closes before/after the core command.</summary>
    public Task<bool> RequireAcceptedMaterialOwnedCoreProofAsync(CompanyWorkOrderExecutionPermitIntent proposal,CompanyWorkOrderAcceptedMaterialReview original,Func<Task> requireAuthority,string coreConnectionName)
        =>RequireMaterialOwnedCoreAsync(proposal,original,requireAuthority,coreConnectionName,original.Material.Source.Context.ActorUserId);
    private async Task<bool> RequireMaterialOwnedCoreAsync(CompanyWorkOrderExecutionPermitIntent proposal,CompanyWorkOrderAcceptedMaterialReview original,Func<Task> requireAuthority,string coreConnectionName,string viewer)
    {
        original.Require(proposal,original.Material.Source.WorkOrder);var connection=await RequiredMaterialConnection(original.Material.Source);
        var core=_editor.GetDataSource(coreConnectionName)??throw new InvalidOperationException("Original owned approval core source is unavailable.");var stock=_editor.GetDataSource(connection)??throw new InvalidOperationException("Original current stock source is unavailable.");
        PPDMDataSourceTransaction.RequireOwned(core,IsolationLevel.Serializable);await requireAuthority();await RequiredMaterialConnection(original.Material.Source);
        if(!ReferenceEquals(core,_editor.GetDataSource(coreConnectionName)) || !ReferenceEquals(stock,_editor.GetDataSource(connection)))throw new InvalidOperationException("Original core/stock boundary changed during owned proof selection.");
        PPDMDataSourceTransaction.RequireOwned(core,IsolationLevel.Serializable);
        if(!ReferenceEquals(core,stock))return false;
        await RequireMaterialProofCoreAsync(proposal,original,requireAuthority,true,coreConnectionName,viewer);return true;
    }
    private async Task<string> RequiredMaterialConnection(CompanyWorkOrderMaterialSourceReview original)
    {
        if(_resolveConnection is null || await _resolveConnection()!=original.Context.Database.ConnectionName)throw new InvalidOperationException("Original request-bound PRODUCTION allocation target changed.");return original.Context.Database.ConnectionName;
    }
    public async Task<WORK_ORDER_MATERIAL_ALLOCATION> RecordWorkOrderMaterialAllocationAsync(CompanyWorkOrderMaterialAllocationCommand original,Func<Task> requireAuthority,Func<Task> requireWorkOrderSource,bool readOnly=false)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var sourceReview=command.Review.Source;await requireAuthority();await requireWorkOrderSource();var connection=await RequiredMaterialConnection(sourceReview);var source=_editor.GetDataSource(connection)??throw new InvalidOperationException("Original allocation datasource is unavailable.");
        async Task Guard(bool owned){await requireAuthority();await RequiredMaterialConnection(sourceReview);if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("Original allocation datasource changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);}
        async Task<WORK_ORDER_MATERIAL_ALLOCATION?> Existing(bool owned)
        {
            await Guard(owned);var raw=(await MaterialAllocationRepository(connection).GetAsync([new(){FieldName="ALLOCATION_ID",Operator="=",FilterValue=command.AllocationId()}]))?.ToArray();await Guard(owned);
            if(raw is null || raw.Length>1 || raw.Any(r=>r is not WORK_ORDER_MATERIAL_ALLOCATION))throw new InvalidOperationException("Original allocation identity is unavailable or ambiguous.");if(raw.Length==0)return null;var row=(WORK_ORDER_MATERIAL_ALLOCATION)raw[0];command.RequireRecorded(row);return CompanyInventorySourceReview.Copy(row);
        }
        if(readOnly){var first=await Existing(false)??throw new InvalidOperationException("The original allocation was not recorded.");await requireWorkOrderSource();var closing=await Existing(false);if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original recovery evidence changed.");return first;}
        WORK_ORDER_MATERIAL_ALLOCATION? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            confirmed=await Existing(true);if(confirmed is not null)return;
            var rows=await MaterialAllocations(connection,sourceReview.Context.Company,sourceReview.Query.ItemId);var inventory=await GetCompanyInventorySourceAsync(sourceReview.Context.Company,sourceReview.Query.ItemId,sourceReview.Context.ActorUserId)??throw new InvalidOperationException("Original inventory source is unavailable.");await Guard(true);
            var releases=await MaterialReleases(connection,sourceReview.Context.Company,sourceReview.Query.ItemId);var fresh=CompanyWorkOrderMaterialAllocationReview.Create(sourceReview,rows,releases,command.Review.ComparedAtUtc);
            if(inventory.Revision!=sourceReview.Inventory.Revision || fresh.Revision!=command.Review.Revision)throw new InvalidOperationException("Original inventory or complete competing reservations changed before allocation.");
            var row=command.Prepare(DateTime.UtcNow);var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(WORK_ORDER_MATERIAL_ALLOCATION),connection,"WORK_ORDER_MATERIAL_ALLOCATION",rowAccess:new MaterialAllocationInsertPolicy(row,()=>Guard(true)));
            await writer.InsertAsync(row,sourceReview.Context.ActorUserId);confirmed=await Existing(true);if(confirmed is null)throw new InvalidOperationException("Original allocation write is unconfirmed.");await Guard(true);
        });
        await requireWorkOrderSource();var closingInventory=await GetCompanyInventorySourceAsync(sourceReview.Context.Company,sourceReview.Query.ItemId,sourceReview.Context.ActorUserId);if(closingInventory?.Revision!=sourceReview.Inventory.Revision)throw new InvalidOperationException("Committed allocation source changed; retain the original intent for read-only recovery.");var durable=await Existing(false);if(confirmed is null || JsonSerializer.Serialize(confirmed)!=JsonSerializer.Serialize(durable))throw new InvalidOperationException("Committed allocation is unconfirmed; recover the original intent without another write.");return durable!;
    }
    private sealed class MaterialAllocationInsertPolicy(WORK_ORDER_MATERIAL_ALLOCATION original,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private readonly WORK_ORDER_MATERIAL_ALLOCATION expected=CompanyInventorySourceReview.Copy(original);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(WORK_ORDER_MATERIAL_ALLOCATION) || table!="WORK_ORDER_MATERIAL_ALLOCATION" || operation!=PPDMRowMutation.Insert || previous is not null || actor!=expected.ROW_CREATED_BY || !ReferenceEquals(original,entity))throw new InvalidOperationException("Only original append-only allocation insertion is authorized.");
            original.PPDM_GUID=expected.PPDM_GUID;original.ACTIVE_IND=expected.ACTIVE_IND;original.ROW_CREATED_BY=expected.ROW_CREATED_BY;original.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;original.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;original.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;
            if(JsonSerializer.Serialize(original)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("Original allocation evidence changed before insertion.");
        }
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Allocation insertion grants no reads.");
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Allocation insertion grants no reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Allocation insertion grants no reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Allocation insertion grants no reads.");
    }
}
