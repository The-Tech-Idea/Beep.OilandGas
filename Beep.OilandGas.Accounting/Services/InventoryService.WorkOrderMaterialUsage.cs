using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.Accounting.Services;

public partial class InventoryService
{
    public async Task<CompanyWorkOrderMaterialCostSource> ReadWorkOrderMaterialCostSourceAsync(FieldSelectionReview context,AssetDatabaseScope production,string workOrderId,string usageId,Func<Task> authority,bool owned=false)
    {
        FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);
        if(!LeaseSummaryValidator.Identifier(workOrderId) || !LeaseSummaryValidator.Identifier(usageId))throw new ArgumentException("Original work-order/material usage identity is required.");
        var source=_editor.GetDataSource(production.ConnectionName)??throw new InvalidOperationException("Original material consumption PRODUCTION source is unavailable.");
        async Task Guard()
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(_resolveConnection is null || await _resolveConnection()!=production.ConnectionName || !ReferenceEquals(source,_editor.GetDataSource(production.ConnectionName)))
                throw new InvalidOperationException("Original material consumption PRODUCTION binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task<CompanyWorkOrderMaterialCostSource> Read()
        {
            await Guard();var rows=(await MaterialUsageRepository(production.ConnectionName).GetAsync([new(){FieldName="USAGE_ID",Operator="=",FilterValue=usageId}])).ToArray();
            if(rows.Length!=1 || rows[0] is not WORK_ORDER_MATERIAL_USAGE value)throw new InvalidOperationException("One original recorded material consumption is required.");
            var row=CompanyInventorySourceReview.Copy(value);
            if(row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.FIELD_ID!=context.FieldId || row.WORK_ORDER_ID!=workOrderId)
                throw new UnauthorizedAccessException("Original material consumption company/FIELD/order headers differ.");
            await Guard();var movements=(await (await GetRepoAsync<INVENTORY_TRANSACTION>("INVENTORY_TRANSACTION")).GetAsync([new(){FieldName="INVENTORY_TRANSACTION_ID",Operator="=",FilterValue=usageId}])).ToArray();
            if(movements.Length!=1 || movements[0] is not INVENTORY_TRANSACTION movement)throw new InvalidOperationException("One original matching physical consumption movement is required.");
            var result=CompanyWorkOrderMaterialCostSource.From(row,CompanyInventorySourceReview.Copy(movement));
            if(result.Database!=production || result.Company!=context.Company || result.WorkOrderId!=workOrderId || result.FieldId!=context.FieldId)throw new InvalidOperationException("Original material consumption proof differs from current native bindings.");
            await Guard();return result;
        }
        var first=await Read();var closing=await Read();if(first!=closing)throw new InvalidOperationException("Original physical consumption changed during financial source observation.");await Guard();return closing;
    }
    private PPDMGenericRepository MaterialUsageRepository(string connection)
    {
        const string table = "WORK_ORDER_MATERIAL_USAGE";
        var source = _editor.GetDataSource(connection) ?? throw new InvalidOperationException("Original material use datasource is unavailable.");
        var shape = source.GetEntityStructure(new EntityStructure { EntityName = table, DatasourceEntityName = table }, true);
        var keys = shape?.Fields?.Where(field => field.IsKey).ToArray();
        if (keys is not { Length: 1 } || keys[0].FieldName != "USAGE_ID" || keys[0].AllowDBNull)
            throw new InvalidOperationException("Review the explicit PRODUCTION Beep migration: material use key is unavailable.");
        foreach (var property in typeof(WORK_ORDER_MATERIAL_USAGE).GetProperties().Where(property => property.DeclaringType == typeof(WORK_ORDER_MATERIAL_USAGE)))
        {
            var fields = shape!.Fields.Where(field => field.FieldName == property.Name).ToArray();
            var length = property.GetCustomAttributes(typeof(MaxLengthAttribute), true).Cast<MaxLengthAttribute>().SingleOrDefault()?.Length;
            if (fields is not { Length: 1 } || fields[0].AllowDBNull || fields[0].Fieldtype != property.PropertyType.FullName ||
                length is {} required && fields[0].Size1 < required)
                throw new InvalidOperationException("Original physical usage schema type/nullability/capacity is unconfirmed.");
        }
        RequireCompanySourceSchema(connection, "INVENTORY_ITEM");
        RequireCompanySourceSchema(connection, "INVENTORY_TRANSACTION");
        foreach(var (nativeTable, column, type) in new[]{("INVENTORY_ITEM","QUANTITY_ON_HAND",typeof(decimal)),
            ("INVENTORY_TRANSACTION","QUANTITY",typeof(decimal)),("INVENTORY_TRANSACTION","TRANSACTION_DATE",typeof(DateTime))})
        {
            var nativeShape=source.GetEntityStructure(new EntityStructure{EntityName=nativeTable,DatasourceEntityName=nativeTable},true);
            var fields=nativeShape?.Fields?.Where(field=>field.FieldName==column).ToArray();
            if(fields is not {Length:1} || fields[0].Fieldtype!=type.FullName)
                throw new InvalidOperationException("Physical stock quantity/movement UTC metadata differs from the native Beep contract.");
        }
        var movement = source.GetEntityStructure(new EntityStructure { EntityName = "INVENTORY_TRANSACTION", DatasourceEntityName = "INVENTORY_TRANSACTION" }, true);
        var provenance = movement?.Fields?.Where(field => field.FieldName == "WORK_ORDER_MATERIAL_USAGE_JSON").ToArray();
        if (provenance is not { Length: 1 } || provenance[0].Fieldtype != typeof(string).FullName ||
            !provenance[0].AllowDBNull || provenance[0].Size1 < 16384)
            throw new InvalidOperationException("Review the additive Beep migration for nullable stock usage provenance.");
        return new(_editor, _commonColumnHandler, _defaults, _metadata, typeof(WORK_ORDER_MATERIAL_USAGE), connection, table);
    }

    private async Task<CompanyWorkOrderMaterialUsageRecovery?> ReadMaterialUsage(string connection, FieldSelectionReview context,
        CompanyWorkOrderMaterialUsageReference reference, Func<Task> guard)
    {
        reference.Require(); FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (context.FieldId != reference.Start.Permit.Task.FieldId) throw new UnauthorizedAccessException("Original usage FIELD differs from current authority.");
        await guard(); var id = CompanyWorkOrderMaterialUsageFacts.IdFor(context.Company, reference.IntentId);
        var rows = (await MaterialUsageRepository(connection).GetAsync([new() { FieldName = "USAGE_ID", Operator = "=", FilterValue = id }]))?.ToArray();
        await guard();
        if (rows is null || rows.Length > 1 || rows.Any(row => row is not WORK_ORDER_MATERIAL_USAGE))
            throw new InvalidOperationException("Original usage membership is unavailable or ambiguous.");
        var movements = (await (await GetRepoAsync<INVENTORY_TRANSACTION>("INVENTORY_TRANSACTION")).GetAsync([
            new() { FieldName = "INVENTORY_TRANSACTION_ID", Operator = "=", FilterValue = id }]))?.ToArray();
        await guard();
        if (rows.Length == 0)
        {
            if (movements is not { Length: 0 }) throw new InvalidOperationException("A stock movement exists without its original private usage intent.");
            return null;
        }
        var row = (WORK_ORDER_MATERIAL_USAGE)rows[0]; var task = reference.Start.Permit.Task;
        // Ownership and direct task headers are checked before deserializing any private command body.
        if (row.OWNER_TENANT_ID != context.Company.TenantId || row.OWNER_COMPANY_BA_ID != context.Company.BusinessAssociateId ||
            row.ROW_CREATED_BY != context.ActorUserId || row.FIELD_ID != task.FieldId || row.WORK_ORDER_ID != task.WorkOrderId ||
            row.PROCESS_INSTANCE_ID != task.InstanceId || row.START_ID != reference.Start.StartId(context.Company))
            throw new UnauthorizedAccessException("Original private usage belongs to another company/account/task/start.");
        if (movements is not { Length: 1 } || movements[0] is not INVENTORY_TRANSACTION movement)
            throw new InvalidOperationException("The unique original physical stock movement is unavailable.");
        var command = CompanyWorkOrderMaterialUsageCommand.Read(row);
        if (command.Review.Material.Source.Context.Database != context.Database)
            throw new InvalidOperationException("Original usage PRODUCTION binding changed.");
        var result = new CompanyWorkOrderMaterialUsageRecovery(command, CompanyInventorySourceReview.Copy(row), CompanyInventorySourceReview.Copy(movement));
        result.Require(context.Company, context.ActorUserId, reference); await guard(); return result;
    }

    private async Task RequireMaterialUsagePopulation(string connection, CompanyInventorySourceReview inventory, Func<Task> guard, bool requireSchema = false)
    {
        var movements = inventory.Transactions.Where(row => row.TRANSACTION_TYPE == "WORK_ORDER_MATERIAL_USE" || row.WORK_ORDER_MATERIAL_USAGE_JSON is not null).ToArray();
        // Historical reservation reads without usage retain their existing schema requirements.
        if (movements.Length == 0 && !requireSchema && _editor.GetDataSource(connection)?.CheckEntityExist("WORK_ORDER_MATERIAL_USAGE") != true) return;
        await guard(); var rows = (await MaterialUsageRepository(connection).GetAsync([
            new() { FieldName = "OWNER_TENANT_ID", Operator = "=", FilterValue = inventory.Company.TenantId },
            new() { FieldName = "OWNER_COMPANY_BA_ID", Operator = "=", FilterValue = inventory.Company.BusinessAssociateId },
            new() { FieldName = "INVENTORY_ITEM_ID", Operator = "=", FilterValue = inventory.Item.INVENTORY_ITEM_ID }]))?.ToArray();
        await guard();
        if (rows is null || rows.Length > CompanyWorkOrderMaterialAllocationReview.MaximumAllocations || rows.Length != movements.Length ||
            rows.Any(row => row is not WORK_ORDER_MATERIAL_USAGE)) throw new InvalidOperationException("Complete bounded private usage/movement membership is unavailable.");
        var typed = rows.Cast<WORK_ORDER_MATERIAL_USAGE>().ToArray();
        if (typed.Select(row => row.USAGE_ID).Distinct(StringComparer.Ordinal).Count() != typed.Length)
            throw new InvalidOperationException("Private material usage identities are ambiguous.");
        foreach (var row in typed)
        {
            if (row.OWNER_TENANT_ID != inventory.Company.TenantId || row.OWNER_COMPANY_BA_ID != inventory.Company.BusinessAssociateId ||
                row.INVENTORY_ITEM_ID != inventory.Item.INVENTORY_ITEM_ID)
                throw new UnauthorizedAccessException("Private usage population returned foreign company/item headers.");
            var command = CompanyWorkOrderMaterialUsageCommand.Read(row); command.RequireRecorded(row);
            if (command.Review.Material.Source.Context.Database.ConnectionName != connection ||
                movements.SingleOrDefault(movement => movement.INVENTORY_TRANSACTION_ID == row.USAGE_ID) is not {} movement ||
                JsonSerializer.Serialize(movement) != JsonSerializer.Serialize(command.PrepareMovement(row.ROW_CREATED_DATE!.Value)))
                throw new InvalidOperationException("A physical usage movement lacks its exact original private command/audit.");
        }
        await guard();
    }

    public async Task<CompanyWorkOrderMaterialUsageRecovery> RecoverWorkOrderMaterialUsageAsync(FieldSelectionReview context,
        CompanyWorkOrderMaterialUsageReference reference, Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority); reference.Require();
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        await requireAuthority(); if (_resolveConnection is null || await _resolveConnection() != context.Database.ConnectionName)
            throw new InvalidOperationException("Original request-bound usage PRODUCTION target is required.");
        var connection = context.Database.ConnectionName;
        var source = _editor.GetDataSource(connection) ?? throw new InvalidOperationException("Original usage source is unavailable.");
        async Task Guard()
        {
            PPDMDataSourceTransaction.RequireIdle(source); await requireAuthority();
            if (await _resolveConnection() != connection || !ReferenceEquals(source, _editor.GetDataSource(connection)))
                throw new InvalidOperationException("Original usage target changed during recovery.");
            PPDMDataSourceTransaction.RequireIdle(source);
        }
        var first = await ReadMaterialUsage(connection, context, reference, Guard) ?? throw new InvalidOperationException("Original material usage is not recorded.");
        var closing = await ReadMaterialUsage(connection, context, reference, Guard);
        if (JsonSerializer.Serialize(first) != JsonSerializer.Serialize(closing)) throw new InvalidOperationException("Original material usage changed during recovery.");
        return first;
    }

    /// <summary>
    /// The execution owner must close the exact recorded start, approved reservation, current order and permit window.
    /// The owned flag identifies this PRODUCTION transaction; a separate CORE provider is never claimed atomic.
    /// </summary>
    public async Task<CompanyWorkOrderMaterialUsageRecovery> RecordWorkOrderMaterialUsageAsync(CompanyWorkOrderMaterialUsageCommand original,
        Func<Task> requireAuthority, Func<bool, Task> requireCurrentExecution)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority); ArgumentNullException.ThrowIfNull(requireCurrentExecution);
        var command = CompanyInventorySourceReview.Copy(original); command.Require();
        var review = command.Review; var sourceReview = review.Material.Source; var context = sourceReview.Context;
        var reference = new CompanyWorkOrderMaterialUsageReference(new(review.Start.Permit, review.Start.IntentId), command.IntentId);
        await requireAuthority(); var connection = await RequiredMaterialConnection(sourceReview);
        var source = _editor.GetDataSource(connection) ?? throw new InvalidOperationException("Original physical use source is unavailable.");
        async Task Guard(bool owned)
        {
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
            await requireAuthority(); await RequiredMaterialConnection(sourceReview);
            if (!ReferenceEquals(source, _editor.GetDataSource(connection))) throw new InvalidOperationException("Original material use target changed.");
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task<CompanyWorkOrderMaterialUsageRecovery?> Existing(bool owned)
        {
            var found = await ReadMaterialUsage(connection, context, reference, () => Guard(owned));
            if (found is not null && JsonSerializer.Serialize(found.Command) != JsonSerializer.Serialize(command))
                throw new InvalidOperationException("Original material usage identity is already used by another body.");
            return found;
        }
        await Guard(false);
        // Exact original retry is historical recovery: no new execution proof and no additional stock mutation.
        if (await Existing(false) is not null) return await RecoverWorkOrderMaterialUsageAsync(context, reference, requireAuthority);
        await requireCurrentExecution(false); await Guard(false);
        CompanyWorkOrderMaterialUsageRecovery? confirmed = null;
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await Guard(true); confirmed = await Existing(true); if (confirmed is not null) return;
            await requireCurrentExecution(true); await Guard(true);
            var originals = await MaterialAllocations(connection, context.Company, sourceReview.Query.ItemId);
            var selected = originals.SingleOrDefault(row => row.ALLOCATION_ID == review.Reservation.AllocationId)
                ?? throw new InvalidOperationException("Original selected reservation is unavailable.");
            if (selected.FIELD_ID != review.Start.Permit.Task.FieldId || selected.WORK_ORDER_ID != review.Start.Permit.Task.WorkOrderId ||
                selected.PROCESS_INSTANCE_ID != review.Start.Permit.Task.InstanceId || CompanyWorkOrderMaterialReservationFacts.From(selected) != review.Reservation)
                throw new InvalidOperationException("Original recorded reservation task/source differs from the selected work start.");
            var current = await ReviewMaterialAllocationsCoreAsync(sourceReview, () => Guard(true), context.ActorUserId);
            await RequireMaterialUsagePopulation(connection, current.Source.Inventory, () => Guard(true), true);
            command.RequireCurrent(review with { Material = current, ComparedAtUtc = DateTime.UtcNow });
            var utc = DateTime.UtcNow; var intent = command.Prepare(utc); var movement = command.PrepareMovement(utc);
            var item = CompanyInventorySourceReview.Copy(current.Source.Inventory.Item);
            if (item.QUANTITY_ON_HAND is null || item.QUANTITY_ON_HAND < command.Quantity)
                throw new InvalidOperationException("Confirmed native on-hand stock cannot cover this original physical issue.");
            var updated = CompanyInventorySourceReview.Copy(item); updated.QUANTITY_ON_HAND -= command.Quantity;
            updated.ROW_CHANGED_BY = context.ActorUserId; updated.ROW_CHANGED_DATE = utc;
            var writer = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(INVENTORY_TRANSACTION), connection,
                "INVENTORY_TRANSACTION", rowAccess: new MaterialUsageMutationPolicy(movement, null, () => Guard(true)));
            await writer.InsertAsync(movement, context.ActorUserId); await Guard(true);
            writer = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(WORK_ORDER_MATERIAL_USAGE), connection,
                "WORK_ORDER_MATERIAL_USAGE", rowAccess: new MaterialUsageMutationPolicy(intent, null, () => Guard(true)));
            await writer.InsertAsync(intent, context.ActorUserId); await Guard(true);
            writer = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(INVENTORY_ITEM), connection,
                "INVENTORY_ITEM", rowAccess: new MaterialUsageMutationPolicy(updated, item, () => Guard(true)));
            await writer.UpdateAsync(updated, context.ActorUserId); await Guard(true);
            var stored = await GetCompanyInventorySourceAsync(context.Company, sourceReview.Query.ItemId, context.ActorUserId)
                ?? throw new InvalidOperationException("Original stock readback is unavailable.");
            var history = current.Source.Inventory.Transactions.Append(movement).OrderBy(row => row.INVENTORY_TRANSACTION_ID, StringComparer.Ordinal).ToArray();
            if (JsonSerializer.Serialize(stored.Item) != JsonSerializer.Serialize(updated) || JsonSerializer.Serialize(stored.Transactions) != JsonSerializer.Serialize(history))
                throw new InvalidOperationException("Exact stock decrement or complete physical movement readback differs.");
            var allocations = await MaterialAllocations(connection, context.Company, sourceReview.Query.ItemId);
            var releases = await MaterialReleases(connection, context.Company, sourceReview.Query.ItemId);
            var afterSource = sourceReview with { Inventory = stored, Blockers = CompanyWorkOrderMaterialSourceReview.GetBlockers(sourceReview.Query, stored) };
            var after = CompanyWorkOrderMaterialAllocationReview.Create(afterSource, allocations, releases, DateTime.UtcNow);
            await RequireMaterialUsagePopulation(connection, stored, () => Guard(true));
            if (JsonSerializer.Serialize(after.Allocations) != JsonSerializer.Serialize(current.Allocations) ||
                JsonSerializer.Serialize(after.Releases) != JsonSerializer.Serialize(current.Releases) ||
                after.ReservedQuantity != current.ReservedQuantity - command.Quantity || after.AvailableQuantity != current.AvailableQuantity)
                throw new InvalidOperationException("Competing reservations/releases or remaining physical capacity changed during use.");
            confirmed = await Existing(true) ?? throw new InvalidOperationException("Original private usage and movement write are unconfirmed.");
            await requireCurrentExecution(true); await Guard(true);
        });
        await requireCurrentExecution(false); await Guard(false);
        var durable = await RecoverWorkOrderMaterialUsageAsync(context, reference, requireAuthority);
        if (confirmed is null || JsonSerializer.Serialize(confirmed) != JsonSerializer.Serialize(durable))
            throw new InvalidOperationException("Committed material use is uncertain; recover the retained original reference.");
        return durable;
    }

    private sealed class MaterialUsageMutationPolicy(object original, INVENTORY_ITEM? previousItem, Func<Task> guard) : IPPDMRowAccessPolicy
    {
        private readonly string expected = JsonSerializer.Serialize(original);
        private readonly string? previous = previousItem is null ? null : JsonSerializer.Serialize(previousItem);
        public async Task ValidateMutationAsync(Type type, string table, object entity, object? prior, PPDMRowMutation operation, string? actor)
        {
            await guard();
            var isItem = original is INVENTORY_ITEM;
            if (!ReferenceEquals(original, entity) || type != original.GetType() || table != type.Name ||
                operation != (isItem ? PPDMRowMutation.Update : PPDMRowMutation.Insert) ||
                (isItem ? prior is null || JsonSerializer.Serialize(prior) != previous : prior is not null))
                throw new InvalidOperationException("Only the exact original usage insertion or stock preimage update is authorized.");
            var frozen = JsonSerializer.Deserialize(expected, type)!;
            var expectedActor = (string?)type.GetProperty("ROW_CHANGED_BY")!.GetValue(frozen);
            if (actor != expectedActor) throw new UnauthorizedAccessException("Original physical use actor changed.");
            // The generic repository populates audit defaults; preserve the reviewed operation's exact audit instead.
            foreach (var name in new[] { "PPDM_GUID", "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CHANGED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_DATE" })
                type.GetProperty(name)!.SetValue(entity, type.GetProperty(name)!.GetValue(frozen));
            if (JsonSerializer.Serialize(entity) != expected) throw new InvalidOperationException("Original usage body/stock quantity/audit changed before mutation.");
        }
        private async Task RequireItemRead(Type type, string table)
        {
            await guard(); if (previousItem is null || type != typeof(INVENTORY_ITEM) || table != "INVENTORY_ITEM")
                throw new InvalidOperationException("Only the exact stock preimage can be read for an item mutation.");
        }
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type, string table)
        {
            await RequireItemRead(type, table);
            return new List<IReadOnlyList<AppFilter>> { new List<AppFilter> { new() {
                FieldName = "INVENTORY_ITEM_ID", Operator = "=", FilterValue = previousItem!.INVENTORY_ITEM_ID } } };
        }
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type, string table)
        {
            await RequireItemRead(type, table);
            return new List<AppFilter> { new() { FieldName = "OWNER_TENANT_ID", Operator = "=", FilterValue = previousItem!.OWNER_TENANT_ID },
                new() { FieldName = "OWNER_COMPANY_BA_ID", Operator = "=", FilterValue = previousItem.OWNER_COMPANY_BA_ID } };
        }
        public async Task ValidateRowsAsync(Type type, string table, IReadOnlyList<object> rows)
        {
            await RequireItemRead(type, table);
            if (rows.Count != 1 || JsonSerializer.Serialize(rows[0]) != previous)
                throw new InvalidOperationException("Original stock preimage changed before physical use.");
        }
        public Task<string?> GetEffectiveAccessLevelAsync(Type type, string table, object row, bool inherited = true) => throw new InvalidOperationException("Physical usage mutation grants no reads.");
    }
}
