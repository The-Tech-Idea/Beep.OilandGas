using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using static Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview;

namespace Beep.OilandGas.LifeCycle.Services.Production;

public partial class PPDMProductionService
{
    public async Task<FieldSelectionReview> GetInterventionSelectionReviewAsync(string fieldId)
    {
        await RequireCurrentBindingAsync(fieldId, null, true);
        return _review!;
    }
    public async Task<ProductionInterventionMaintenanceLink> ReadInterventionMaintenanceLinkAsync(string well,Guid id,string order,string actor,CompanyScope assertedCompany)
    {
        using var activity=ProductionActivities.StartActivity("production.intervention.maintenance_link");
        if(id==Guid.Empty || !LeaseSummaryValidator.Identifier(well) || !LeaseSummaryValidator.Identifier(order))throw RefusalException.Invalid("The original decision/order/well identities are required.");
        var connection=await RequireCurrentBindingAsync(null,actor,true);var context=_review!;
        if(context.Company!=assertedCompany)throw RefusalException.Forbidden("The receiving company differs from the authoritative FIELD owner.");
        var source=_editor.GetDataSource(connection)??throw new InvalidOperationException("The original receiving datasource is unavailable.");
        async Task Guard()
        {
            if(await RequireCurrentBindingAsync(context.FieldId,actor,true)!=connection || !ReferenceEquals(source,_editor.GetDataSource(connection)))throw RefusalException.Conflict("The original receiving binding changed.");
            await NativeDecisionSchemaAsync(source);
        }
        async Task<ProductionInterventionMaintenanceLink> Read()
        {
            var current=await CaptureNativeInterventionAsync(context,well,requireWrite:false,order);
            var intent=await ReadNativeDecisionRowAsync<WELL_INTERVENTION_INTENT>(source,connection,id,Guard)??throw RefusalException.NotFound("The original recorded maintenance decision is unavailable.");
            var outcome=await ReadNativeDecisionRowAsync<WELL_INTERVENTION_RESULT>(source,connection,id,Guard)??throw RefusalException.NotFound("An original reservation alone does not establish a maintenance link.");
            var command=intent.Restore();var result=outcome.Restore(intent);
            if(command.Review.Context.Company!=context.Company || command.Review.Context.Database!=context.Database || command.Review.Context.FieldId!=context.FieldId ||
                command.Review.Well.UWI!=well || command.Decision.WorkOrderId!=order || result.Decision!="APPROVED" || result.WorkOrderId!=order)
                throw RefusalException.Forbidden("This decision does not establish the requested original company/FIELD/order/well link.");
            await RequireNativeDecisionActivityAsync(intent,outcome,current);
            var value=new ProductionInterventionMaintenanceLink(context.Company,actor,context.FieldId!,order,well,id,command.Review.Context.ActorUserId,
                command.Decision.Note!,System.Text.Json.JsonSerializer.Deserialize<WELL_ACTIVITY>(outcome.ACTIVITY_JSON)!.ROW_CREATED_DATE!.Value,
                result.NativeReceiptRevision,command.Review.WorkOrder!.Revision,DateTimeOffset.UtcNow);
            value.Require(context.Company,actor,context.FieldId!,order,well,id);await Guard();return value;
        }
        var first=await Read();var closing=await Read();
        if(first with{RetrievedAtUtc=closing.RetrievedAtUtc}!=closing)throw RefusalException.Conflict("The recorded receiving source changed during read-back.");
        return closing;
    }
    public async Task<ProductionInterventionDecisionResult> RecordNativeInterventionDecisionAsync(string well,
        ProductionInterventionDecisionRequest request, string actor)
    {
        request = Copy(request);
        if (request.IntentId == Guid.Empty || request.Context is null || !LeaseSummaryValidator.Identifier(well) || actor != request.Context.ActorUserId)
            throw RefusalException.Invalid("The original native decision identity, actor and FIELD review are required.");
        _ = ProductionInterventionDecisionChecks.Require(request, request.Context.FieldId ?? string.Empty);
        var context = request.Context;
        var connection = await RequireCurrentBindingAsync(request.FieldId, actor, true);
        if (_review != context) throw RefusalException.Conflict("Return to the exact original FIELD review before recording its command.");
        var source = _editor.GetDataSource(connection) ?? throw new InvalidOperationException("The original intervention datasource is unavailable.");
        if (_editor.OpenDataSource(connection) != ConnectionState.Open) throw new InvalidOperationException("The original intervention datasource could not be opened.");

        async Task Guard(bool owned)
        {
            if (await RequireCurrentBindingAsync(request.FieldId, actor, true) != connection || _review != context || !ReferenceEquals(_editor.GetDataSource(connection), source))
                throw RefusalException.Conflict("The original intervention account/company/FIELD/datasource changed.");
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
            await NativeDecisionSchemaAsync(source);
        }
        async Task<ProductionInterventionNativeReview> Current(bool owned)
        {
            await Guard(owned);
            var value = await CaptureNativeInterventionAsync(context, well, requireWrite: true,request.WorkOrderId);
            await Guard(owned); return value;
        }
        Task<WELL_INTERVENTION_INTENT?> Intent(bool owned) => ReadNativeDecisionRowAsync<WELL_INTERVENTION_INTENT>(source, connection, request.IntentId, () => Guard(owned));
        Task<WELL_INTERVENTION_RESULT?> Outcome(bool owned) => ReadNativeDecisionRowAsync<WELL_INTERVENTION_RESULT>(source, connection, request.IntentId, () => Guard(owned));
        void SameRequest(WELL_INTERVENTION_INTENT intent)
        {
            var retained = intent.Restore();
            if (retained.Review.Context != context || retained.Review.Well.UWI != well || JsonSerializer.Serialize(retained.Decision) != JsonSerializer.Serialize(request))
                throw RefusalException.Conflict("This original native identity is already reserved for a different decision.");
        }
        async Task<ProductionInterventionDecisionResult> Confirm(WELL_INTERVENTION_INTENT intent, WELL_INTERVENTION_RESULT outcome, bool owned)
        {
            SameRequest(intent); var result = outcome.Restore(intent);
            await RequireNativeDecisionActivityAsync(intent, outcome, await Current(owned));
            await Guard(owned); return result;
        }

        await Guard(false);
        var retained = await Intent(false);
        if (retained is not null)
        {
            SameRequest(retained);
            if (await Outcome(false) is { } recorded) return await Confirm(retained, recorded, false);
        }
        else if (await Outcome(false) is not null) throw RefusalException.Conflict("A recorded result has no original native intent.");

        var command = retained?.Restore();
        if (command is null)
        {
            var review = await Current(false);
            var maximum = review.History.Max(row => row.ACTIVITY_OBS_NO);
            if (maximum == decimal.MaxValue) throw RefusalException.Conflict("The native intervention observation range is exhausted.");
            command = new(request.IntentId, review, Copy(request), maximum + 1); command.Require();
        }
        var original = command;
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await Guard(true);
            if (await Intent(true) is { } existing)
            {
                SameRequest(existing);
                if (JsonSerializer.Serialize(existing.Restore()) != JsonSerializer.Serialize(original))
                    throw RefusalException.Conflict("The original native reservation has different complete preparation evidence.");
                return;
            }
            if (await Outcome(true) is not null) throw RefusalException.Conflict("A result already exists without the original reservation.");
            original.Review.RequireCurrent(await Current(true));
            var entity = WELL_INTERVENTION_INTENT.Create(original);
            var policy = new NativeDecisionMutation(source, () => Guard(true), original, null);
            await NativeDecisionRepository(typeof(WELL_INTERVENTION_INTENT), connection, policy).InsertAsync(entity, actor);
            var saved = await Intent(true) ?? throw RefusalException.Conflict("The original native reservation was not read back.");
            if (JsonSerializer.Serialize(saved) != JsonSerializer.Serialize(entity)) throw RefusalException.Conflict("The complete retained native intent differs from its insertion.");
            original.Review.RequireCurrent(await Current(true)); await Guard(true);
        });
        retained = await Intent(false) ?? throw RefusalException.Conflict("The committed original reservation is unconfirmed. Recover it before another write.");
        SameRequest(retained);
        if (JsonSerializer.Serialize(retained.Restore()) != JsonSerializer.Serialize(original)) throw RefusalException.Conflict("The original durable reservation changed.");

        WELL_INTERVENTION_RESULT? confirmed = null;
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await Guard(true);
            var intent = await Intent(true) ?? throw RefusalException.Conflict("The original native reservation disappeared.");
            if (JsonSerializer.Serialize(intent) != JsonSerializer.Serialize(retained)) throw RefusalException.Conflict("The complete original reservation changed before disposition.");
            if (await Outcome(true) is { } recorded) { await Confirm(intent, recorded, true); confirmed = recorded; return; }
            original.Review.RequireCurrent(await Current(true));
            var activity = original.NewActivity();
            var policy = new NativeDecisionMutation(source, () => Guard(true), original, intent);
            await NativeDecisionRepository(typeof(WELL_ACTIVITY), connection, policy).InsertAsync(activity, actor);
            original.RequireActivity(activity);
            var outcome = WELL_INTERVENTION_RESULT.Create(intent, activity);
            await NativeDecisionRepository(typeof(WELL_INTERVENTION_RESULT), connection, policy).InsertAsync(outcome, actor);
            var saved = await Outcome(true) ?? throw RefusalException.Conflict("The native result was not read back.");
            if (JsonSerializer.Serialize(saved) != JsonSerializer.Serialize(outcome)) throw RefusalException.Conflict("The complete native result differs from its insertion.");
            await Confirm(intent, saved, true); confirmed = saved; await Guard(true);
        });
        var first = await Outcome(false); var closing = await Outcome(false);
        if (confirmed is null || first is null || closing is null || JsonSerializer.Serialize(first) != JsonSerializer.Serialize(confirmed) ||
            JsonSerializer.Serialize(first) != JsonSerializer.Serialize(closing))
            throw RefusalException.Conflict("The committed native decision is unconfirmed. Recover its original identity without another submission.");
        return await Confirm(retained, closing, false);
    }

    public async Task<ProductionInterventionNativeRecovery> RecoverNativeInterventionDecisionAsync(string well, Guid intentId, string actor, CompanyScope? assertedCompany = null)
    {
        if (intentId == Guid.Empty || !LeaseSummaryValidator.Identifier(well) || !LeaseSummaryValidator.Identifier(actor))
            throw RefusalException.Invalid("An exact original native recovery identity is required.");
        var connection = await RequireCurrentBindingAsync(null, actor, true); var context = _review!;
        if (assertedCompany is not null && assertedCompany != context.Company) throw RefusalException.Forbidden("The requested native recovery company differs from the authoritative FIELD owner.");
        var source = _editor.GetDataSource(connection) ?? throw new InvalidOperationException("The original intervention datasource is unavailable.");
        if (_editor.OpenDataSource(connection) != ConnectionState.Open) throw new InvalidOperationException("The original recovery datasource could not be opened.");
        async Task Guard()
        {
            if (await RequireCurrentBindingAsync(context.FieldId, actor, true) != connection || !ReferenceEquals(_editor.GetDataSource(connection), source))
                throw RefusalException.Conflict("The original native recovery binding changed.");
            await NativeDecisionSchemaAsync(source);
        }
        async Task<ProductionInterventionNativeRecovery> Capture()
        {
            await Guard();
            var current = await CaptureNativeInterventionAsync(context, well, requireWrite: false);
            var intent = await ReadNativeDecisionRowAsync<WELL_INTERVENTION_INTENT>(source, connection, intentId, Guard);
            var outcome = await ReadNativeDecisionRowAsync<WELL_INTERVENTION_RESULT>(source, connection, intentId, Guard);
            var value = new ProductionInterventionNativeRecovery(context.Company, actor, intentId, intent, outcome); value.Require();
            if (intent is not null)
            {
                var original = intent.Restore();
                if (original.Review.Well.UWI != well || original.Review.Context.Database != context.Database || original.Review.Context.FieldId != context.FieldId)
                    throw RefusalException.Forbidden("Return to the original company/FIELD/WELL/database to recover its private decision.");
                if(!string.IsNullOrWhiteSpace(original.Decision.WorkOrderId))current=await CaptureNativeInterventionAsync(context,well,requireWrite:false,original.Decision.WorkOrderId);
                if (outcome is not null) await RequireNativeDecisionActivityAsync(intent, outcome, current);
            }
            await Guard(); return value;
        }
        var first = await Capture(); var closing = await Capture();
        if (JsonSerializer.Serialize(first) != JsonSerializer.Serialize(closing)) throw RefusalException.Conflict("The exact original native recovery changed during read-back.");
        return closing;
    }

    private async Task<ProductionInterventionNativeReview> CaptureNativeInterventionAsync(FieldSelectionReview context, string well, bool requireWrite,string? order=null)
    {
        using var activity=ProductionActivities.StartActivity("production.intervention.capture");
        await RequireCurrentBindingAsync(context.FieldId, context.ActorUserId, true);
        var repo = NativeDecisionRepository(typeof(WELL), context.Database.ConnectionName, _rowAccess);
        async Task<WELL> Header()
        {
            var rows = (await repo.GetAsync([Equal("UWI", well), Equal("ASSIGNED_FIELD", context.FieldId!), Equal("ACTIVE_IND", "Y")])).ToArray();
            if (rows is not [WELL value]) throw RefusalException.Conflict("The unique original active WELL header is unavailable.");
            if (requireWrite) await _rowAccess.ValidateMutationAsync(typeof(WELL), "WELL", value, value, PPDMRowMutation.Update, context.ActorUserId);
            return Copy(value);
        }
        var initial = await Header();
        WELL_ACTIVITY[] history;
        using(ProductionActivities.StartActivity("production.intervention.history"))history = (await ReadWellActivitiesForScopeAsync(context.FieldId!,well)).Select(Copy).ToArray();
        var closing = await Header();
        if (JsonSerializer.Serialize(initial) != JsonSerializer.Serialize(closing)) throw RefusalException.Conflict("The original WELL header changed during native decision capture.");
        await RequireCurrentBindingAsync(context.FieldId, context.ActorUserId, true);
        ProductionInterventionWorkOrderReview? maintenance=null;
        if(!string.IsNullOrWhiteSpace(order))
        {
            if(_rowAccess is not IProductionInterventionWorkOrderReader owner)throw RefusalException.Conflict("The canonical company maintenance source owner is unavailable.");
            using(ProductionActivities.StartActivity("production.intervention.work_order"))maintenance=await owner.ReviewInterventionWorkOrderAsync(context,order,well);
            await RequireCurrentBindingAsync(context.FieldId,context.ActorUserId,true);
        }
        return ProductionInterventionNativeReview.Create(context, closing, history,maintenance);
    }

    private static Task RequireNativeDecisionActivityAsync(WELL_INTERVENTION_INTENT intent, WELL_INTERVENTION_RESULT outcome, ProductionInterventionNativeReview current)
    {
        var command = intent.Restore(); _ = outcome.Restore(intent);
        var activity = JsonSerializer.Deserialize<WELL_ACTIVITY>(outcome.ACTIVITY_JSON)!;
        var saved = current.History.Where(row => row.SOURCE == activity.SOURCE && row.ACTIVITY_OBS_NO == activity.ACTIVITY_OBS_NO).ToArray();
        if (current.Context.Company != command.Review.Context.Company || current.Context.Database != command.Review.Context.Database ||
            current.Context.FieldId != command.Review.Context.FieldId || current.Well.UWI != command.Review.Well.UWI || saved.Length != 1 ||
            JsonSerializer.Serialize(saved[0]) != JsonSerializer.Serialize(activity) || command.Review.History.Any(original =>
                !current.History.Any(row => JsonSerializer.Serialize(row) == JsonSerializer.Serialize(original))) ||
            JsonSerializer.Serialize(current.WorkOrder)!=JsonSerializer.Serialize(command.Review.WorkOrder))
            throw RefusalException.Conflict("The original native activity and complete preparation history do not confirm their retained result.");
        return Task.CompletedTask;
    }

    private PPDMGenericRepository NativeDecisionRepository(Type type, string connection, IPPDMRowAccessPolicy policy) =>
        new(_editor, _commonColumnHandler, _defaults, _metadata, type, connection, type.Name, rowAccess: policy);

    private async Task<T?> ReadNativeDecisionRowAsync<T>(IDataSource source, string connection, Guid id, Func<Task> guard) where T : class
    {
        using var activity=ProductionActivities.StartActivity("production.intervention.journal");
        await guard();
        var rows = (await NativeDecisionRepository(typeof(T), connection, new NativeDecisionRead(guard, id)).GetAsync([Equal("INTENT_ID", id.ToString("N"))])).ToArray();
        await guard();
        if (rows.Length > 1 || rows.Any(row => row is not T)) throw RefusalException.Conflict("The original native journal key is ambiguous.");
        return rows.SingleOrDefault() is T value ? Copy(value) : null;
    }

    private async Task NativeDecisionSchemaAsync(IDataSource source)
    {
        using var activity=ProductionActivities.StartActivity("production.intervention.schema");
        foreach (var type in ProductionInterventionNativeSchema.EntityTypes)
        {
            var definition = await _metadata.GetTableMetadataAsync(type.Name);
            if (definition is null || definition.EntityTypeName != type.FullName || definition.PrimaryKeyColumn != "INTENT_ID" || definition.Module != "PPDM_CORE")
                throw RefusalException.Conflict("Confirm the original native intervention metadata through its PPDM_CORE owner.");
            var physical = source.GetEntityStructure(new EntityStructure { EntityName = type.Name, DatasourceEntityName = type.Name }, true);
            var keys = physical?.Fields?.Where(field => field.IsKey).ToArray();
            if (physical?.EntityName != type.Name || keys is not { Length: 1 } || keys[0].FieldName != "INTENT_ID" || keys[0].AllowDBNull)
                throw RefusalException.Conflict("Apply the original native intervention journal schema with Beep Migration before recording decisions.");
            foreach (var property in type.GetProperties().Where(property => property.IsDefined(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), true)))
            {
                var fields = physical.Fields.Where(field => field.FieldName == property.Name).ToArray();
                var size = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.MaxLengthAttribute), true).Cast<System.ComponentModel.DataAnnotations.MaxLengthAttribute>().Single().Length;
                if (fields is not { Length: 1 } || fields[0].Fieldtype != typeof(string).FullName || fields[0].AllowDBNull || Math.Max(fields[0].Size, fields[0].Size1) < size)
                    throw RefusalException.Conflict("The bounded original native journal columns are unconfirmed.");
            }
        }
    }

    private static AppFilter Equal(string name, string value) => new() { FieldName = name, Operator = "=", FilterValue = value };

    private class NativeDecisionRead(Func<Task> guard, Guid id) : IPPDMRowAccessPolicy
    {
        protected Task RequireCurrentAsync() => guard();
        public virtual Task ValidateMutationAsync(Type type, string table, object entity, object? previous, PPDMRowMutation operation, string? actor) => throw RefusalException.Forbidden("Private decision recovery grants no general mutation authority.");
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type, string table)
        {
            await guard(); if (!ProductionInterventionNativeSchema.EntityTypes.Contains(type) || table != type.Name) throw RefusalException.Forbidden("Only the original native journal identity can be read.");
            return [Equal("INTENT_ID", id.ToString("N"))];
        }
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type, string table) => [await GetReadConstraintsAsync(type, table)];
        public async Task ValidateRowsAsync(Type type, string table, IReadOnlyList<object> rows)
        {
            await guard();
            if (!ProductionInterventionNativeSchema.EntityTypes.Contains(type) || table != type.Name || rows.Count > 1 ||
                rows.Any(row => row.GetType() != type || type.GetProperty("INTENT_ID")!.GetValue(row) as string != id.ToString("N")))
                throw RefusalException.Conflict("The provider returned another or ambiguous private native journal identity.");
        }
        public Task<string?> GetEffectiveAccessLevelAsync(Type type, string table, object row, bool inherited = true) => throw RefusalException.Forbidden("Native journal recovery grants no general reads.");
    }

    private sealed class NativeDecisionMutation(IDataSource source, Func<Task> guard, ProductionInterventionNativeCommand command,
        WELL_INTERVENTION_INTENT? intent) : NativeDecisionRead(guard, command.IntentId)
    {
        public override async Task ValidateMutationAsync(Type type, string table, object entity, object? previous, PPDMRowMutation operation, string? actor)
        {
            await RequireCurrentAsync(); PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
            if (previous is not null || operation != PPDMRowMutation.Insert || actor != command.Review.Context.ActorUserId || table != type.Name)
                throw RefusalException.Forbidden("Only the original immutable native decision insertion is authorized.");
            if (intent is null && type == typeof(WELL_INTERVENTION_INTENT) && entity is WELL_INTERVENTION_INTENT journal &&
                JsonSerializer.Serialize(journal.Restore()) == JsonSerializer.Serialize(command)) return;
            if (intent is not null && type == typeof(WELL_ACTIVITY) && entity is WELL_ACTIVITY activity) { command.RequireActivity(activity); return; }
            if (intent is not null && type == typeof(WELL_INTERVENTION_RESULT) && entity is WELL_INTERVENTION_RESULT result) { _ = result.Restore(intent); return; }
            throw RefusalException.Forbidden("The native command grants no unrelated mutation authority.");
        }
    }
}
