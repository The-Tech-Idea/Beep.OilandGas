using System.Text.Json;
using System.Data;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ProductionOperations.Services;

public sealed partial class FacilityManagementService:ICompanyFacilityCreationReader
{
    /// <summary>Rechecks a confirmed creation original and its complete native storage without writing.</summary>
    public Task ReadFacilityCreationPreflightAsync(DevelopmentFacilityCreation input,Func<Task> authority)=>ReadFacilityCreationPreflightCoreAsync(input,authority,false);
    private async Task ReadFacilityCreationPreflightCoreAsync(DevelopmentFacilityCreation input,Func<Task> authority,bool owned)
    {
        ArgumentNullException.ThrowIfNull(input);ArgumentNullException.ThrowIfNull(authority);var original=CompanyInventorySourceReview.Copy(input);original.Require();
        var draft=original.Review.Draft;if(_resolveModuleConnection is null || draft.Context.Database.ConnectionName!=_connectionName)throw new InvalidOperationException("The original CORE binding is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original facility datasource is unavailable.");
        async Task Guard()
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority();
            if(await _resolveModuleConnection("PPDM_CORE")!=_connectionName || await _resolveModuleConnection("FACILITY")!=_connectionName || !ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original facility datasource changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task CompareSources()
        {
            var current=await ReadFacilityCreationSourceCoreAsync(draft,Guard,owned);
            if(current.OperatorRevision!=original.Review.OperatorRevision || JsonSerializer.Serialize(current.TypeSource)!=JsonSerializer.Serialize(original.Review.TypeSource))throw new InvalidOperationException("The reviewed facility type or operator source changed.");
        }
        await CompareSources();
        await MaintenanceAssetDefinitionsAsync(source,Guard,[new(typeof(FACILITY),["FACILITY_ID","FACILITY_TYPE"],[("PRIMARY_FIELD_ID","FIELD","FIELD_ID"),("CURRENT_OPERATOR","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID")]),
            new(typeof(FACILITY_FIELD),["FACILITY_ID","FACILITY_TYPE","FIELD_ID"],[("FACILITY_ID","FACILITY","FACILITY_ID"),("FIELD_ID","FIELD","FIELD_ID")]),new(typeof(FACILITY_CREATE_INTENT),["FACILITY_CREATE_INTENT_ID"],[])]);
        async Task Capacity(string table,IEnumerable<(string Name,string? Value)> values)
        {
            await Guard();var physical=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);
            foreach(var (name,value) in values)
            {
                var columns=physical?.Fields?.Where(column=>column.FieldName==name).ToArray();
                if(columns is not {Length:1} || columns[0].Fieldtype!=typeof(string).FullName || (value is null?!columns[0].AllowDBNull:Math.Max(columns[0].Size,columns[0].Size1)<value.Length))throw new InvalidOperationException("The proposed facility or FIELD-link storage capacity is unconfirmed.");
            }
            await Guard();
        }
        var common=new[]{("FACILITY_ID",(string?)draft.FacilityId),("FACILITY_TYPE",draft.FacilityType),("ACTIVE_IND","Y"),("PPDM_GUID",original.FacilityGuid.ToString("D")),("ROW_CREATED_BY",draft.Context.ActorUserId),("ROW_CHANGED_BY",draft.Context.ActorUserId)};
        await Capacity("FACILITY",common.Concat(new[]{("PRIMARY_FIELD_ID",draft.Context.FieldId),("CURRENT_OPERATOR",draft.OperatorBusinessAssociateId),("FACILITY_LONG_NAME",draft.LongName),("DESCRIPTION",draft.Description)}));
        await Capacity("FACILITY_FIELD",common.Select(value=>value.Item1=="PPDM_GUID"?(value.Item1,(string?)original.FieldLinkGuid.ToString("D")):value).Concat(new[]{("FIELD_ID",draft.Context.FieldId)}));
        var shape=source.GetEntityStructure(new EntityStructure{EntityName="FACILITY_CREATE_INTENT",DatasourceEntityName="FACILITY_CREATE_INTENT"},true);
        foreach(var (name,capacity) in new[]{("FACILITY_CREATE_INTENT_ID",32),("OWNER_TENANT_ID",128),("OWNER_COMPANY_BA_ID",128),("FIELD_ID",128),("FACILITY_ID",128),("FACILITY_TYPE",128),("COMMAND_JSON",262144),("FACILITY_JSON",262144),("FIELD_LINK_JSON",262144),("RECORDED_REVISION",64)})
        {
            var columns=shape?.Fields?.Where(column=>column.FieldName==name).ToArray();
            if(columns is not {Length:1} || columns[0].AllowDBNull || columns[0].Fieldtype!=typeof(string).FullName || Math.Max(columns[0].Size,columns[0].Size1)<capacity)throw new InvalidOperationException("The complete original facility creation storage capacity is unconfirmed.");
        }
        async Task CheckAbsence()
        {
            var links=await MaintenanceAssetRowsCoreAsync<FACILITY_FIELD>(Guard,[],true,new AppFilter{FieldName="FACILITY_ID",Operator="=",FilterValue=draft.FacilityId},new AppFilter{FieldName="FACILITY_TYPE",Operator="=",FilterValue=draft.FacilityType},new AppFilter{FieldName="FIELD_ID",Operator="=",FilterValue=draft.Context.FieldId!});
            var intents=await MaintenanceAssetRowsCoreAsync<FACILITY_CREATE_INTENT>(Guard,[],true,new AppFilter{FieldName="FACILITY_CREATE_INTENT_ID",Operator="=",FilterValue=original.IntentId.ToString("N")});
            if(links.Length!=0 || intents.Length!=0)throw new InvalidOperationException("The original FIELD link or creation intent is unavailable or already recorded.");
        }
        await CheckAbsence();await CompareSources();await CheckAbsence();await Guard();
    }
    /// <summary>Checks the exact proposed identity and recorded type without creating business data.</summary>
    public Task<DevelopmentFacilityCreationSource> ReadFacilityCreationSourceAsync(DevelopmentFacilityCreationDraft input,Func<Task> authority)=>ReadFacilityCreationSourceCoreAsync(input,authority,false);
    private async Task<DevelopmentFacilityCreationSource> ReadFacilityCreationSourceCoreAsync(DevelopmentFacilityCreationDraft input,Func<Task> authority,bool owned)
    {
        ArgumentNullException.ThrowIfNull(input);ArgumentNullException.ThrowIfNull(authority);var draft=CompanyInventorySourceReview.Copy(input);draft.Require();
        if(_resolveModuleConnection is null || draft.Context.Database.ConnectionName!=_connectionName)throw new InvalidOperationException("The reviewed CORE and facility binding is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The reviewed facility datasource is unavailable.");
        async Task Guard()
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority();
            if(await _resolveModuleConnection("PPDM_CORE")!=_connectionName || await _resolveModuleConnection("FACILITY")!=_connectionName || !ReferenceEquals(source,_editor.GetDataSource(_connectionName)))
                throw new InvalidOperationException("The reviewed facility datasource changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        await MaintenanceAssetDefinitionsAsync(source,Guard,[new(typeof(FACILITY),["FACILITY_ID","FACILITY_TYPE"],[("PRIMARY_FIELD_ID","FIELD","FIELD_ID")]),new(typeof(R_FACILITY_TYPE),["FACILITY_TYPE"],[]),new(typeof(BUSINESS_ASSOCIATE),["BUSINESS_ASSOCIATE_ID"],[])]);
        async Task<DevelopmentFacilityCreationSource> Read()
        {
            var identities=await MaintenanceAssetRowsCoreAsync<FACILITY>(Guard,[],true,new AppFilter{FieldName="FACILITY_ID",Operator="=",FilterValue=draft.FacilityId},new AppFilter{FieldName="FACILITY_TYPE",Operator="=",FilterValue=draft.FacilityType});
            if(identities.Length!=0)throw new InvalidOperationException("The proposed facility identity is unavailable or ambiguous.");
            var types=await MaintenanceAssetRowsCoreAsync<R_FACILITY_TYPE>(Guard,[],true,new AppFilter{FieldName="FACILITY_TYPE",Operator="=",FilterValue=draft.FacilityType});
            if(types.Length!=1 || types[0].FACILITY_TYPE!=draft.FacilityType || types[0].ACTIVE_IND!="Y")throw new InvalidOperationException("The exact recorded active facility type is unavailable or ambiguous.");
            var operators=await MaintenanceAssetRowsCoreAsync<BUSINESS_ASSOCIATE>(Guard,[],true,new AppFilter{FieldName="BUSINESS_ASSOCIATE_ID",Operator="=",FilterValue=draft.OperatorBusinessAssociateId});
            if(operators.Length!=1 || operators[0].BUSINESS_ASSOCIATE_ID!=draft.OperatorBusinessAssociateId || operators[0].ACTIVE_IND!="Y")throw new InvalidOperationException("The exact recorded active operator is unavailable or ambiguous.");
            await Guard();return new(types[0],ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(operators[0])));
        }
        var first=CompanyInventorySourceReview.Copy(await Read());var last=await Read();
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(last))throw new InvalidOperationException("The recorded facility type changed during review.");
        await Guard();return CompanyInventorySourceReview.Copy(last);
    }
}
