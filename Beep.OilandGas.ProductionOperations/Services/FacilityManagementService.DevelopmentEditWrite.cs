using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ProductionOperations.Services;

public sealed partial class FacilityManagementService:ICompanyFacilityEditOwner
{
    /// <summary>Recovers a recorded original using current read authority without repeating the update.</summary>
    public async Task<FACILITY_EDIT_INTENT> RecoverFacilityEditAsync(DevelopmentFacilityEdit input,Func<Task> readAuthority)
        => await ReadRecordedFacilityEditAsync(input,readAuthority,false,false)
            ?? throw new InvalidOperationException("The original facility edit outcome is unavailable or ambiguous.");

    private async Task<FACILITY_EDIT_INTENT?> ReadRecordedFacilityEditAsync(DevelopmentFacilityEdit input,Func<Task> authority,bool allowMissing,bool requireWrite)
    {
        ArgumentNullException.ThrowIfNull(input);ArgumentNullException.ThrowIfNull(authority);
        var command=CompanyInventorySourceReview.Copy(input);command.Require();var context=command.Review.Context;
        if(_maintenanceAssetAccess is null || _resolveModuleConnection is null || context.Database.ConnectionName!=_connectionName)
            throw new InvalidOperationException("The original facility binding and row access are required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original facility datasource is unavailable.");
        async Task Guard()
        {
            PPDMDataSourceTransaction.RequireIdle(source);await authority();
            if(await _resolveModuleConnection("PPDM_CORE")!=_connectionName || await _resolveModuleConnection("FACILITY")!=_connectionName || !ReferenceEquals(source,_editor.GetDataSource(_connectionName)))
                throw new InvalidOperationException("The original facility recovery binding changed.");
            var level=await _maintenanceAssetAccess.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",command.Review.Original,false);
            if(!AssetAccessLevelChecks.Meets(level,requireWrite?"WRITE":"READ"))throw new UnauthorizedAccessException(requireWrite?"Current direct facility WRITE access is required for an edit retry.":"Current direct facility READ access is required for original recovery.");
            PPDMDataSourceTransaction.RequireIdle(source);
        }
        await RequireFacilityEditSchemaAsync(source,Guard);
        async Task<FACILITY_EDIT_INTENT?> Read()
        {
            var rows=await MaintenanceAssetRowsCoreAsync<FACILITY_EDIT_INTENT>(Guard,[],true,new AppFilter{FieldName="FACILITY_EDIT_INTENT_ID",Operator="=",FilterValue=command.IntentId.ToString("N")});
            if(rows.Length==0 && allowMissing){await Guard();return null;}
            if(rows.Length!=1)throw new InvalidOperationException("The original facility edit outcome is unavailable or ambiguous.");
            var saved=rows[0].Restore(context.Company,context.ActorUserId);
            if(saved.Command.Revision()!=command.Revision())throw new InvalidOperationException("The recorded facility edit differs from the retained original.");
            await Guard();return rows[0];
        }
        var first=await Read();var last=await Read();
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(last))throw new InvalidOperationException("The original facility edit evidence changed during recovery.");
        await Guard();return last is null?null:CompanyInventorySourceReview.Copy(last);
    }

    /// <summary>Records one reviewed edit and original intent in a single owned Beep transaction.</summary>
    public async Task<FACILITY_EDIT_INTENT> RecordFacilityEditAsync(DevelopmentFacilityEdit input,Func<Task> authority)
    {
        ArgumentNullException.ThrowIfNull(input);ArgumentNullException.ThrowIfNull(authority);
        var command=CompanyInventorySourceReview.Copy(input);command.Require();
        var existing=await ReadRecordedFacilityEditAsync(command,authority,true,true);
        if(existing is not null)return existing;
        var before=await ReadFacilityEditPreflightCoreAsync(command,authority,false);
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original facility datasource is unavailable.");
        async Task Guard(bool owned)
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();
            if(_resolveModuleConnection is null || await _resolveModuleConnection("PPDM_CORE")!=_connectionName || await _resolveModuleConnection("FACILITY")!=_connectionName || !ReferenceEquals(source,_editor.GetDataSource(_connectionName)))
                throw new InvalidOperationException("The facility edit datasource changed.");
            var level=await _maintenanceAssetAccess!.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",before,false);
            if(!AssetAccessLevelChecks.Meets(level,"WRITE"))throw new UnauthorizedAccessException("Current direct facility WRITE access is required.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task<FACILITY_EDIT_INTENT[]> ReadIntent(bool owned)=>await MaintenanceAssetRowsCoreAsync<FACILITY_EDIT_INTENT>(()=>Guard(owned),[],true,
            new AppFilter{FieldName="FACILITY_EDIT_INTENT_ID",Operator="=",FilterValue=command.IntentId.ToString("N")});
        async Task<FACILITY[]> ReadFacility(bool owned)=>await MaintenanceAssetRowsCoreAsync<FACILITY>(()=>Guard(owned),[],true,
            new(){FieldName="FACILITY_ID",Operator="=",FilterValue=before.FACILITY_ID},new(){FieldName="FACILITY_TYPE",Operator="=",FilterValue=before.FACILITY_TYPE});
        FACILITY_EDIT_INTENT? recorded=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            var current=await ReadFacilityEditPreflightCoreAsync(command,authority,true);
            if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(before))throw new InvalidOperationException("The original facility changed before the owned transaction.");
            if((await ReadIntent(true)).Length!=0)throw new InvalidOperationException("This facility edit intent is already used. Recover the original.");
            var update=CompanyInventorySourceReview.Copy(before);update.FACILITY_LONG_NAME=command.LongName;update.DESCRIPTION=command.Description!;
            var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(FACILITY),_connectionName,"FACILITY",rowAccess:new FacilityEditRowPolicy(command,before,()=>Guard(true),_maintenanceAssetAccess!));
            await writer.UpdateAsync(update,command.Review.Context.ActorUserId);await Guard(true);
            var rows=await ReadFacility(true);if(rows.Length!=1 || JsonSerializer.Serialize(rows[0])!=JsonSerializer.Serialize(update))throw new InvalidOperationException("The exact approved facility result was not read back.");
            var intent=FACILITY_EDIT_INTENT.Create(command,rows[0]);
            var journal=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(FACILITY_EDIT_INTENT),_connectionName,"FACILITY_EDIT_INTENT",rowAccess:new FacilityEditIntentPolicy(intent,()=>Guard(true)));
            await journal.InsertAsync(intent,command.Review.Context.ActorUserId);await Guard(true);
            var saved=await ReadIntent(true);if(saved.Length!=1 || JsonSerializer.Serialize(saved[0])!=JsonSerializer.Serialize(intent))throw new InvalidOperationException("The exact original facility edit was not retained.");
            _=saved[0].Restore(command.Review.Context.Company,command.Review.Context.ActorUserId);recorded=CompanyInventorySourceReview.Copy(saved[0]);
            var final=await ReadFacility(true);if(final.Length!=1 || JsonSerializer.Serialize(final[0])!=JsonSerializer.Serialize(rows[0]))throw new InvalidOperationException("The facility changed before commit.");
            await Guard(true);
        });
        await Guard(false);var durable=await ReadIntent(false);
        if(recorded is null || durable.Length!=1 || JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(durable[0]))throw new InvalidOperationException("The facility edit commit is unconfirmed. Recover the retained original.");
        var recovered=durable[0].Restore(command.Review.Context.Company,command.Review.Context.ActorUserId);
        if(recovered.Command.Revision()!=command.Revision())throw new InvalidOperationException("The confirmed facility edit differs from the original.");
        await Guard(false);return CompanyInventorySourceReview.Copy(durable[0]);
    }

    private sealed class FacilityEditRowPolicy(DevelopmentFacilityEdit command,FACILITY before,Func<Task> guard,IPPDMRowAccessPolicy access):IPPDMRowAccessPolicy
    {
        private async Task Require(Type type,string table){await guard();if(type!=typeof(FACILITY) || table!="FACILITY")throw new InvalidOperationException("This policy grants only the original facility update.");}
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table){await Require(type,table);return [new List<AppFilter>{new(){FieldName="FACILITY_ID",Operator="=",FilterValue=before.FACILITY_ID},new(){FieldName="FACILITY_TYPE",Operator="=",FilterValue=before.FACILITY_TYPE}}];}
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table){await Require(type,table);return [new(){FieldName="PRIMARY_FIELD_ID",Operator="=",FilterValue=before.PRIMARY_FIELD_ID}];}
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows){await Require(type,table);if(rows.Count!=1 || JsonSerializer.Serialize(rows[0])!=JsonSerializer.Serialize(before))throw new InvalidOperationException("The reviewed facility preimage changed.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool includeInherited=true)=>throw new InvalidOperationException("This policy grants no general asset access.");
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await Require(type,table);if(operation!=PPDMRowMutation.Update || actor!=command.Review.Context.ActorUserId || entity is not FACILITY row || previous is not FACILITY prior || JsonSerializer.Serialize(prior)!=JsonSerializer.Serialize(before))throw new InvalidOperationException("Only the exact reviewed facility can be edited.");
            if(!AssetAccessLevelChecks.Meets(await access.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",prior,false),"WRITE"))throw new UnauthorizedAccessException("Direct facility WRITE access changed.");
            var expected=CompanyInventorySourceReview.Copy(before);expected.FACILITY_LONG_NAME=command.LongName;expected.DESCRIPTION=command.Description!;expected.ROW_CHANGED_BY=actor;expected.ROW_CHANGED_DATE=row.ROW_CHANGED_DATE;
            if(row.ROW_CHANGED_DATE is null || row.ROW_CHANGED_DATE.Value.Kind==DateTimeKind.Local || JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("Only the approved descriptive fields and actor audit may change.");
            await guard();
        }
    }
    private sealed class FacilityEditIntentPolicy(FACILITY_EDIT_INTENT original,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private readonly FACILITY_EDIT_INTENT expected=CompanyInventorySourceReview.Copy(original);
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Intent policy grants no reads.");
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Intent policy grants no reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Intent policy grants no reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool includeInherited=true)=>throw new InvalidOperationException("Intent policy grants no asset access.");
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(FACILITY_EDIT_INTENT) || table!="FACILITY_EDIT_INTENT" || operation!=PPDMRowMutation.Insert || previous is not null || actor!=expected.ROW_CREATED_BY || entity is not FACILITY_EDIT_INTENT row)throw new InvalidOperationException("Only the exact reviewed facility edit intent can be inserted.");
            row.ROW_CREATED_BY=expected.ROW_CREATED_BY;row.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;row.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;row.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;row.ACTIVE_IND=expected.ACTIVE_IND;
            if(JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The original facility edit intent changed before insertion.");
            await guard();
        }
    }
}
