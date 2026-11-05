using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ProductionOperations.Services;

public sealed partial class FacilityManagementService:ICompanyFacilityCreationOwner,ICompanyEquipmentInstallationOwner
{
    /// <summary>Records the exact facility, FIELD link and original in one owned native transaction.</summary>
    public async Task<FACILITY_CREATE_INTENT> RecordFacilityCreationAsync(DevelopmentFacilityCreation input,Func<Task> authority)
    {
        ArgumentNullException.ThrowIfNull(input);ArgumentNullException.ThrowIfNull(authority);var command=CompanyInventorySourceReview.Copy(input);command.Require();
        var existing=await ReadRecordedFacilityCreationAsync(command,authority,true);if(existing is not null)return existing;
        await ReadFacilityCreationPreflightCoreAsync(command,authority,false);
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original facility datasource is unavailable.");
        async Task Guard(bool owned)
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority();
            if(_resolveModuleConnection is null || await _resolveModuleConnection("PPDM_CORE")!=_connectionName || await _resolveModuleConnection("FACILITY")!=_connectionName || !ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original facility datasource changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task<FACILITY_CREATE_INTENT[]> Intents(bool owned)=>await MaintenanceAssetRowsCoreAsync<FACILITY_CREATE_INTENT>(()=>Guard(owned),[],true,new AppFilter{FieldName="FACILITY_CREATE_INTENT_ID",Operator="=",FilterValue=command.IntentId.ToString("N")});
        var draft=command.Review.Draft;FACILITY_CREATE_INTENT? recorded=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await ReadFacilityCreationPreflightCoreAsync(command,authority,true);
            var expected=command.Expected(DateTime.UtcNow);var intent=FACILITY_CREATE_INTENT.Create(command,expected.Facility,expected.Link);
            async Task Insert(Type type,string table,object entity){await new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,type,_connectionName,table,rowAccess:new FacilityCreationInsertPolicy(type,entity,draft.Context.ActorUserId,()=>Guard(true))).InsertAsync(entity,draft.Context.ActorUserId);}
            await Insert(typeof(FACILITY),"FACILITY",expected.Facility);await Insert(typeof(FACILITY_FIELD),"FACILITY_FIELD",expected.Link);await Insert(typeof(FACILITY_CREATE_INTENT),"FACILITY_CREATE_INTENT",intent);
            var facilities=await MaintenanceAssetRowsCoreAsync<FACILITY>(()=>Guard(true),[],true,new AppFilter{FieldName="FACILITY_ID",Operator="=",FilterValue=draft.FacilityId},new AppFilter{FieldName="FACILITY_TYPE",Operator="=",FilterValue=draft.FacilityType});
            var links=await MaintenanceAssetRowsCoreAsync<FACILITY_FIELD>(()=>Guard(true),[],true,new AppFilter{FieldName="FACILITY_ID",Operator="=",FilterValue=draft.FacilityId},new AppFilter{FieldName="FACILITY_TYPE",Operator="=",FilterValue=draft.FacilityType},new AppFilter{FieldName="FIELD_ID",Operator="=",FilterValue=draft.Context.FieldId!});
            var intents=await Intents(true);
            if(facilities.Length!=1 || links.Length!=1 || intents.Length!=1 || JsonSerializer.Serialize(facilities[0])!=JsonSerializer.Serialize(expected.Facility) || JsonSerializer.Serialize(links[0])!=JsonSerializer.Serialize(expected.Link) || JsonSerializer.Serialize(intents[0])!=JsonSerializer.Serialize(intent))throw new InvalidOperationException("The complete original facility creation was not read back exactly.");
            var types=await MaintenanceAssetRowsCoreAsync<R_FACILITY_TYPE>(()=>Guard(true),[],true,new AppFilter{FieldName="FACILITY_TYPE",Operator="=",FilterValue=draft.FacilityType});
            var operators=await MaintenanceAssetRowsCoreAsync<BUSINESS_ASSOCIATE>(()=>Guard(true),[],true,new AppFilter{FieldName="BUSINESS_ASSOCIATE_ID",Operator="=",FilterValue=draft.OperatorBusinessAssociateId});
            if(types.Length!=1 || operators.Length!=1 || JsonSerializer.Serialize(types[0])!=JsonSerializer.Serialize(command.Review.TypeSource) || ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(operators[0]))!=command.Review.OperatorRevision)throw new InvalidOperationException("The reviewed facility type or operator changed before commit.");
            _=intents[0].Restore(draft.Context.Company,draft.Context.ActorUserId);recorded=CompanyInventorySourceReview.Copy(intents[0]);await Guard(true);
        });
        await Guard(false);var durable=await Intents(false);
        if(recorded is null || durable.Length!=1 || JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(durable[0]))throw new InvalidOperationException("The facility creation commit is unconfirmed. Recover the retained original.");
        var restored=durable[0].Restore(draft.Context.Company,draft.Context.ActorUserId);if(restored.Command.Revision()!=command.Revision())throw new InvalidOperationException("The confirmed facility creation differs from the original.");
        await Guard(false);return CompanyInventorySourceReview.Copy(durable[0]);
    }
    /// <summary>Recovers only an exact recorded original using current caller-provided FIELD read authority.</summary>
    public async Task<FACILITY_CREATE_INTENT> RecoverFacilityCreationAsync(DevelopmentFacilityCreation input,Func<Task> readAuthority)=>
        await ReadRecordedFacilityCreationAsync(input,readAuthority,false) ?? throw new InvalidOperationException("The original facility creation outcome is unavailable or ambiguous.");
    private async Task<FACILITY_CREATE_INTENT?> ReadRecordedFacilityCreationAsync(DevelopmentFacilityCreation input,Func<Task> readAuthority,bool allowMissing)
    {
        ArgumentNullException.ThrowIfNull(input);ArgumentNullException.ThrowIfNull(readAuthority);var command=CompanyInventorySourceReview.Copy(input);command.Require();var draft=command.Review.Draft;
        if(_resolveModuleConnection is null || draft.Context.Database.ConnectionName!=_connectionName)throw new InvalidOperationException("The original facility binding is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original facility datasource is unavailable.");
        async Task Guard()
        {
            PPDMDataSourceTransaction.RequireIdle(source);await readAuthority();
            if(await _resolveModuleConnection("PPDM_CORE")!=_connectionName || await _resolveModuleConnection("FACILITY")!=_connectionName || !ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original facility recovery binding changed.");
            PPDMDataSourceTransaction.RequireIdle(source);
        }
        await MaintenanceAssetDefinitionsAsync(source,Guard,[new(typeof(FACILITY_CREATE_INTENT),["FACILITY_CREATE_INTENT_ID"],[])]);
        async Task<FACILITY_CREATE_INTENT?> Read()
        {
            var rows=await MaintenanceAssetRowsCoreAsync<FACILITY_CREATE_INTENT>(Guard,[],true,new AppFilter{FieldName="FACILITY_CREATE_INTENT_ID",Operator="=",FilterValue=command.IntentId.ToString("N")});
            if(rows.Length==0 && allowMissing){await Guard();return null;}
            if(rows.Length!=1)throw new InvalidOperationException("The original facility creation outcome is unavailable or ambiguous.");
            var restored=rows[0].Restore(draft.Context.Company,draft.Context.ActorUserId);if(restored.Command.Revision()!=command.Revision())throw new InvalidOperationException("The original facility creation differs from the retained input.");await Guard();return rows[0];
        }
        var first=await Read();var last=await Read();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(last))throw new InvalidOperationException("The original creation evidence changed during recovery.");await Guard();return last is null?null:CompanyInventorySourceReview.Copy(last);
    }
    public async Task<FACILITY_EQUIPMENT_INSTALL_INTENT> RecordEquipmentInstallationAsync(DevelopmentEquipmentInstallation input,Func<Task> authority)
    {
        ArgumentNullException.ThrowIfNull(input);ArgumentNullException.ThrowIfNull(authority);var command=CompanyInventorySourceReview.Copy(input);command.Require();var proposal=command.Review.Proposal;
        var existing=await ReadRecordedEquipmentInstallationAsync(command,authority,true);if(existing is not null)return existing;
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original equipment datasource is unavailable.");
        async Task Guard(bool owned)
        {
            RequireMaintenanceEvidenceLane(source,owned);await authority();
            if(_resolveModuleConnection is null || proposal.Context.Database.ConnectionName!=_connectionName || await _resolveModuleConnection("PPDM_CORE")!=_connectionName || await _resolveModuleConnection("FACILITY")!=_connectionName || !ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original installation datasource changed.");
            RequireMaintenanceEvidenceLane(source,owned);
        }
        static AppFilter Equal(string column,string value)=>new(){FieldName=column,Operator="=",FilterValue=value};
        Task<FACILITY_EQUIPMENT_INSTALL_INTENT[]> Intents(bool owned)=>MaintenanceAssetRowsCoreAsync<FACILITY_EQUIPMENT_INSTALL_INTENT>(()=>Guard(owned),[],true,Equal("FACILITY_EQUIPMENT_INSTALL_INTENT_ID",command.IntentId.ToString("N")));
        async Task<decimal> Preflight(bool owned)
        {
            var current=await ReviewDevelopmentEquipmentInstallationCoreAsync(proposal,()=>Guard(owned),owned,null);
            if(current!=command.Review.Source)throw new InvalidOperationException("The reviewed equipment installation source changed.");
            await MaintenanceAssetDefinitionsAsync(source,()=>Guard(owned),[new(typeof(FACILITY_EQUIPMENT_INSTALL_INTENT),["FACILITY_EQUIPMENT_INSTALL_INTENT_ID"],[])]);
            var shape=source.GetEntityStructure(new EntityStructure{EntityName="FACILITY_EQUIPMENT_INSTALL_INTENT",DatasourceEntityName="FACILITY_EQUIPMENT_INSTALL_INTENT"},true);
            foreach(var (name,capacity) in new[]{("FACILITY_EQUIPMENT_INSTALL_INTENT_ID",32),("OWNER_TENANT_ID",128),("OWNER_COMPANY_BA_ID",128),("FIELD_ID",128),("FACILITY_ID",128),("FACILITY_TYPE",128),("EQUIPMENT_ID",128),("COMMAND_JSON",262144),("INSTALLATION_JSON",262144),("RECORDED_REVISION",64)})
            {
                var columns=shape?.Fields?.Where(column=>column.FieldName==name).ToArray();if(columns is not {Length:1} || columns[0].AllowDBNull || columns[0].Fieldtype!=typeof(string).FullName || Math.Max(columns[0].Size,columns[0].Size1)<capacity)throw new InvalidOperationException("The complete installation intent storage capacity is unconfirmed.");
            }
            var installationShape=source.GetEntityStructure(new EntityStructure{EntityName="FACILITY_EQUIPMENT",DatasourceEntityName="FACILITY_EQUIPMENT"},true);
            foreach(var (name,value) in new[]{("FACILITY_ID",proposal.FacilityId),("FACILITY_TYPE",proposal.FacilityType),("EQUIPMENT_ID",proposal.EquipmentId),("ACTIVE_IND","Y"),("PPDM_GUID",command.InstallationGuid.ToString("D")),("ROW_CREATED_BY",proposal.Context.ActorUserId),("ROW_CHANGED_BY",proposal.Context.ActorUserId)})
            {
                var columns=installationShape?.Fields?.Where(column=>column.FieldName==name).ToArray();if(columns is not {Length:1} || columns[0].Fieldtype!=typeof(string).FullName || Math.Max(columns[0].Size,columns[0].Size1)<value.Length)throw new InvalidOperationException("Installation storage capacity is unconfirmed.");
            }
            var rows=await MaintenanceAssetRowsCoreAsync<FACILITY_EQUIPMENT>(()=>Guard(owned),[],true,Equal("EQUIPMENT_ID",proposal.EquipmentId));
            if(rows.Any(row=>row.ACTIVE_IND=="Y" || row.INSTALL_DATE is null || row.REMOVE_DATE is null || row.INSTALL_DATE.Value.Kind!=DateTimeKind.Utc || row.REMOVE_DATE.Value.Kind!=DateTimeKind.Utc || row.REMOVE_DATE<=row.INSTALL_DATE || row.REMOVE_DATE>command.InstalledAtUtc))throw new InvalidOperationException("Recorded installations require reconciled, non-overlapping UTC history before adding another.");
            if((await Intents(owned)).Length!=0)throw new InvalidOperationException("The original installation intent is already recorded or ambiguous.");
            decimal next;try{next=rows.Length==0?1:checked(rows.Max(row=>row.INSTALL_OBS_NO)+1);}catch(OverflowException){throw new InvalidOperationException("The next installation observation is unavailable.");}
            await Guard(owned);return next;
        }
        await Preflight(false);FACILITY_EQUIPMENT_INSTALL_INTENT? recorded=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            var observation=await Preflight(true);var expected=command.Expected(observation,DateTime.UtcNow);var intent=FACILITY_EQUIPMENT_INSTALL_INTENT.Create(command,expected);
            async Task Insert(Type type,object row)=>await new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,type,_connectionName,type.Name,rowAccess:new FacilityCreationInsertPolicy(type,row,proposal.Context.ActorUserId,()=>Guard(true))).InsertAsync(row,proposal.Context.ActorUserId);
            await Insert(typeof(FACILITY_EQUIPMENT),expected);await Insert(typeof(FACILITY_EQUIPMENT_INSTALL_INTENT),intent);
            var installed=await MaintenanceAssetRowsCoreAsync<FACILITY_EQUIPMENT>(()=>Guard(true),[],true,Equal("FACILITY_ID",proposal.FacilityId),Equal("FACILITY_TYPE",proposal.FacilityType),Equal("EQUIPMENT_ID",proposal.EquipmentId),Equal("INSTALL_OBS_NO",observation.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            var intents=await Intents(true);if(installed.Length!=1 || intents.Length!=1 || JsonSerializer.Serialize(installed[0])!=JsonSerializer.Serialize(expected) || JsonSerializer.Serialize(intents[0])!=JsonSerializer.Serialize(intent))throw new InvalidOperationException("The complete installation and intent were not read back exactly.");
            var closing=await ReviewDevelopmentEquipmentInstallationCoreAsync(proposal,()=>Guard(true),true,command.InstallationGuid.ToString("D").ToUpperInvariant());if(closing!=command.Review.Source)throw new InvalidOperationException("The reviewed installation source changed before commit.");
            _=intents[0].Restore(proposal.Context.Company,proposal.Context.ActorUserId);recorded=CompanyInventorySourceReview.Copy(intents[0]);await Guard(true);
        });
        await Guard(false);var durable=await Intents(false);if(recorded is null || durable.Length!=1 || JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(durable[0]))throw new InvalidOperationException("The installation commit is unconfirmed. Recover the retained original.");
        if(durable[0].Restore(proposal.Context.Company,proposal.Context.ActorUserId).Command.Revision()!=command.Revision())throw new InvalidOperationException("The confirmed installation differs from the original.");await Guard(false);return CompanyInventorySourceReview.Copy(durable[0]);
    }
    public async Task<FACILITY_EQUIPMENT_INSTALL_INTENT> RecoverEquipmentInstallationAsync(DevelopmentEquipmentInstallation input,Func<Task> readAuthority)=>
        await ReadRecordedEquipmentInstallationAsync(input,readAuthority,false) ?? throw new InvalidOperationException("The original installation outcome is unavailable or ambiguous.");
    private async Task<FACILITY_EQUIPMENT_INSTALL_INTENT?> ReadRecordedEquipmentInstallationAsync(DevelopmentEquipmentInstallation input,Func<Task> authority,bool allowMissing)
    {
        ArgumentNullException.ThrowIfNull(input);ArgumentNullException.ThrowIfNull(authority);var command=CompanyInventorySourceReview.Copy(input);command.Require();var context=command.Review.Proposal.Context;
        if(_resolveModuleConnection is null || context.Database.ConnectionName!=_connectionName)throw new InvalidOperationException("The original installation binding is required.");var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original installation datasource is unavailable.");
        async Task Guard(){RequireMaintenanceEvidenceLane(source,false);await authority();if(await _resolveModuleConnection("PPDM_CORE")!=_connectionName || await _resolveModuleConnection("FACILITY")!=_connectionName || !ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original installation recovery binding changed.");RequireMaintenanceEvidenceLane(source,false);}
        await RequireEquipmentInstallationRecoveryParentsAsync(command.Review.Proposal,Guard);
        await MaintenanceAssetDefinitionsAsync(source,Guard,[new(typeof(FACILITY_EQUIPMENT_INSTALL_INTENT),["FACILITY_EQUIPMENT_INSTALL_INTENT_ID"],[])]);
        async Task<FACILITY_EQUIPMENT_INSTALL_INTENT?> Read()
        {
            var rows=await MaintenanceAssetRowsCoreAsync<FACILITY_EQUIPMENT_INSTALL_INTENT>(Guard,[],true,new AppFilter{FieldName="FACILITY_EQUIPMENT_INSTALL_INTENT_ID",Operator="=",FilterValue=command.IntentId.ToString("N")});
            if(rows.Length==0 && allowMissing){await Guard();return null;}if(rows.Length!=1 || rows[0].Restore(context.Company,context.ActorUserId).Command.Revision()!=command.Revision())throw new InvalidOperationException("The original installation outcome is unavailable or differs from the retained original.");await Guard();return rows[0];
        }
        var first=await Read();var last=await Read();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(last))throw new InvalidOperationException("The installation outcome changed during recovery.");await RequireEquipmentInstallationRecoveryParentsAsync(command.Review.Proposal,Guard);await Guard();return last is null?null:CompanyInventorySourceReview.Copy(last);
    }

    private sealed class FacilityCreationInsertPolicy(Type expectedType,object original,string actor,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private readonly string expected=JsonSerializer.Serialize(original,expectedType);
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Creation policy grants no reads.");
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Creation policy grants no reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Creation policy grants no reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool includeInherited=true)=>throw new InvalidOperationException("Creation policy grants no general asset access.");
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actualActor)
        {
            await guard();if(type!=expectedType || table!=expectedType.Name || operation!=PPDMRowMutation.Insert || previous is not null || actualActor!=actor || entity.GetType()!=expectedType)throw new InvalidOperationException("Only the exact original facility creation rows may be inserted.");
            var copy=JsonSerializer.Deserialize(expected,expectedType)??throw new InvalidOperationException("The expected facility creation row is unavailable.");
            foreach(var name in new[]{"ACTIVE_IND","PPDM_GUID","ROW_CREATED_BY","ROW_CHANGED_BY","ROW_CREATED_DATE","ROW_CHANGED_DATE"}){var property=expectedType.GetProperty(name)!;property.SetValue(entity,property.GetValue(copy));}
            if(JsonSerializer.Serialize(entity,expectedType)!=expected)throw new InvalidOperationException("The proposed facility creation row changed before insertion.");await guard();
        }
    }
}
