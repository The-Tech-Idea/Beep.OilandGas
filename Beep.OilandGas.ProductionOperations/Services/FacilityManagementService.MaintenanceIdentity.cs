using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.ProductionOperations.Services;

public sealed partial class FacilityManagementService:ICompanyMaintenanceQualifiedAssetEvidenceOwner,ICompanyMaintenanceQualifiedWorkAssetOwner,ICompanyFacilityEquipmentReader,ICompanyEquipmentInstallationReader
{
    private static void RequireMaintenanceEvidenceLane(TheTechIdea.Beep.IDataSource source,bool owned)
    {
        if(owned)PPDMDataSourceTransaction.RequireOwned(source,System.Data.IsolationLevel.Serializable);
        else PPDMDataSourceTransaction.RequireIdle(source);
    }
    private static string MaintenanceIdentityRevision(object row)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(row.GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(property=>property.CanRead && property.CanWrite).OrderBy(property=>property.Name,StringComparer.Ordinal).Select(property=>new object?[]{property.Name,property.GetValue(row)})));
    private DateTime? MaintenanceIdentityUtc(DateTime? value)
    {
        if(value is null)return null;if(value.Value.Kind==DateTimeKind.Utc)return value;
        if(value.Value.Kind==DateTimeKind.Local || string.IsNullOrWhiteSpace(_maintenanceEvidenceZone))throw new InvalidOperationException("Native asset dates require UTC or the explicit recorded company date zone.");
        try{var zone=TimeZoneInfo.FindSystemTimeZoneById(_maintenanceEvidenceZone);if(zone.IsInvalidTime(value.Value) || zone.IsAmbiguousTime(value.Value))throw new InvalidOperationException("Native asset dates have an unresolved daylight-saving boundary.");return TimeZoneInfo.ConvertTimeToUtc(value.Value,zone);}
        catch(Exception error) when(error is TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException){throw new InvalidOperationException("Native asset date interpretation is unavailable.",error);}
    }
    private sealed record MaintenanceAssetDefinition(Type Type,string[] Keys,(string Column,string Table,string Key)[] Relationships);
    private Task MaintenanceAssetDefinitionAsync(TheTechIdea.Beep.IDataSource source,Func<Task> Guard,Type type,string[] keys,params (string Column,string Table,string Key)[] relationships)=>
        MaintenanceAssetDefinitionsAsync(source,Guard,[new(type,keys,relationships)]);
    private async Task MaintenanceAssetDefinitionsAsync(TheTechIdea.Beep.IDataSource source,Func<Task> Guard,IReadOnlyList<MaintenanceAssetDefinition> definitions)
    {
        if(definitions.Count==0)return;
        await Guard();
        foreach(var definition in definitions)
        {
            var type=definition.Type;var keys=definition.Keys;var relationships=definition.Relationships;
            var metadata=await _metadata.GetTableMetadataAsync(type.Name);
            if(metadata?.EntityTypeName!=type.Name || !keys.Order(StringComparer.Ordinal).SequenceEqual((metadata.PrimaryKeyColumn??"").Split(',').Select(key=>key.Trim()).Order(StringComparer.Ordinal)) || relationships.Any(expected=>metadata.ForeignKeys.Count(key=>key.ForeignKeyColumn==expected.Column && key.ReferencedTable==expected.Table && key.ReferencedPrimaryKey==expected.Key)!=1))throw new InvalidOperationException("Canonical qualified asset compound keys and parent relationships are required.");
            var exists=source.CheckEntityExist(type.Name);
            if(source.ErrorObject?.Flag==Errors.Failed)throw new InvalidOperationException("The native schema existence check failed.");
            if(!exists)throw new NativeSchemaCompatibilityException("Review the explicit native Beep schema before qualified asset evidence.");
            var actual=source.GetEntityStructure(new EntityStructure{EntityName=type.Name,DatasourceEntityName=type.Name},true);var expected=_editor.classCreator.ConvertToEntityStructure(type);
            if(actual?.Fields is null || expected?.Fields is null || actual.Fields.GroupBy(field=>field.FieldName,StringComparer.OrdinalIgnoreCase).Any(group=>group.Count()!=1) || !keys.Order(StringComparer.Ordinal).SequenceEqual(actual.Fields.Where(field=>field.IsKey).Select(field=>field.FieldName).Order(StringComparer.Ordinal)) || expected.Fields.Any(field=>actual.Fields.Count(found=>found.FieldName==field.FieldName && found.Fieldtype==field.Fieldtype)!=1) || source.ErrorObject?.Flag==Errors.Failed)throw new NativeSchemaCompatibilityException("Complete physical native asset fields and compound keys are required.");
            // This closing check also opens the next schema validation: no read/publication lies between them.
            await Guard();
        }
    }
    private Task<T[]> MaintenanceAssetRowsAsync<T>(Func<Task> Guard,IReadOnlyList<string> requiredColumns,params AppFilter[] filters) where T:class =>
        MaintenanceAssetRowsCoreAsync<T>(Guard,requiredColumns,false,filters);
    private async Task<T[]> MaintenanceAssetRowsCoreAsync<T>(Func<Task> Guard,IReadOnlyList<string> requiredColumns,bool completeProjection,params AppFilter[] filters) where T:class
        {
            await Guard();var repo=completeProjection?
                new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(T),_connectionName,typeof(T).Name,null,null,requiredColumns,typeof(T).GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(property=>property.CanRead && property.CanWrite).Select(property=>property.Name).ToArray()):
                new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(T),_connectionName,typeof(T).Name,requiredReadColumns:requiredColumns);
            var rows=(await repo.GetAsync(filters.ToList())).ToArray();
            bool Matches(T row,AppFilter filter)
            {
                var value=typeof(T).GetProperty(filter.FieldName)!.GetValue(row);
                return value is decimal number?decimal.TryParse(filter.FilterValue,NumberStyles.Number,CultureInfo.InvariantCulture,out var expected) && number==expected:Convert.ToString(value,CultureInfo.InvariantCulture)==filter.FilterValue;
            }
            if(rows.Length>256 || rows.Any(row=>row is not T typed || filters.Any(filter=>!Matches(typed,filter))))throw new InvalidOperationException("Complete bounded native asset membership must honor all exact predicates.");
            var copy=rows.Cast<T>().Select(CompanyInventorySourceReview.Copy).ToArray();await Guard();return copy;
        }

    // Facility-centric read for the protected equipment screen. These are recorded rows,
    // not a claim that an installation is current or approved for operation.
    public async Task<IReadOnlyList<FACILITY_EQUIPMENT>> ReadDevelopmentFacilityEquipmentAsync(FieldSelectionReview context,string facilityId,string facilityType,Func<Task> authority)
    {
        FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);
        if(_maintenanceAssetAccess is null || context.Database.ConnectionName!=_connectionName || !LeaseSummaryValidator.Identifier(facilityId) || !LeaseSummaryValidator.Identifier(facilityType))
            throw new UnauthorizedAccessException("An exact company, FIELD and facility identity is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The facility source is unavailable.");
        async Task Guard()
        {
            RequireMaintenanceEvidenceLane(source,false);await authority();RequireMaintenanceEvidenceLane(source,false);
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)) || _resolveModuleConnection is not null && await _resolveModuleConnection("PPDM_CORE")!=_connectionName)
                throw new InvalidOperationException("The original facility source binding changed.");
        }
        static AppFilter Equal(string column,string value)=>new(){FieldName=column,Operator="=",FilterValue=value};
        async Task<FACILITY_EQUIPMENT[]> Observe()
        {
            await MaintenanceAssetDefinitionsAsync(source,Guard,[
                new(typeof(FACILITY),["FACILITY_ID","FACILITY_TYPE"],[("CURRENT_OPERATOR","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID"),("PRIMARY_FIELD_ID","FIELD","FIELD_ID")]),
                new(typeof(FACILITY_EQUIPMENT),["FACILITY_ID","FACILITY_TYPE","EQUIPMENT_ID","INSTALL_OBS_NO"],[("EQUIPMENT_ID","EQUIPMENT","EQUIPMENT_ID"),("FACILITY_ID","FACILITY","FACILITY_ID")]),
                new(typeof(EQUIPMENT),["EQUIPMENT_ID"],[("CURRENT_OWNER_BA_ID","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID")])]);
            var parent=await MaintenanceAssetRowsAsync<FACILITY>(Guard,[],Equal("FACILITY_ID",facilityId),Equal("FACILITY_TYPE",facilityType),Equal("ACTIVE_IND","Y"));
            if(parent.Length!=1 || parent[0].PRIMARY_FIELD_ID!=context.FieldId || !LeaseSummaryValidator.Identifier(parent[0].CURRENT_OPERATOR))throw new UnauthorizedAccessException("The qualified facility is not in the original FIELD.");
            var level=await _maintenanceAssetAccess.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",parent[0],parent[0].CURRENT_OPERATOR==context.Company.BusinessAssociateId);
            if(!AssetAccessLevelChecks.Meets(level,"READ"))throw new UnauthorizedAccessException("Current qualified facility access is required.");
            var rows=await MaintenanceAssetRowsAsync<FACILITY_EQUIPMENT>(Guard,[],Equal("FACILITY_ID",facilityId),Equal("FACILITY_TYPE",facilityType),Equal("ACTIVE_IND","Y"));
            if(rows.Any(row=>!LeaseSummaryValidator.Identifier(row.EQUIPMENT_ID) || row.INSTALL_OBS_NO<0 || decimal.Truncate(row.INSTALL_OBS_NO)!=row.INSTALL_OBS_NO) || rows.Select(row=>(row.FACILITY_ID,row.FACILITY_TYPE,row.EQUIPMENT_ID,row.INSTALL_OBS_NO)).Distinct().Count()!=rows.Length)
                throw new InvalidOperationException("Facility equipment membership is incomplete or ambiguous.");
            foreach(var id in rows.Select(row=>row.EQUIPMENT_ID).Distinct(StringComparer.Ordinal))
            {
                var equipment=await MaintenanceAssetRowsAsync<EQUIPMENT>(Guard,[],Equal("EQUIPMENT_ID",id),Equal("ACTIVE_IND","Y"));
                if(equipment.Length!=1 || equipment[0].CURRENT_OWNER_BA_ID!=context.Company.BusinessAssociateId || !AssetAccessLevelChecks.Meets(await _maintenanceAssetAccess.GetEffectiveAccessLevelAsync(typeof(EQUIPMENT),"EQUIPMENT",equipment[0],false),"READ"))
                    throw new UnauthorizedAccessException("Current selected-company equipment and direct access are required.");
            }
            await Guard();return rows.OrderBy(row=>row.EQUIPMENT_ID,StringComparer.Ordinal).ThenBy(row=>row.INSTALL_OBS_NO).ToArray();
        }
        var original=await Observe();var closing=await Observe();
        if(!original.Select(MaintenanceIdentityRevision).SequenceEqual(closing.Select(MaintenanceIdentityRevision),StringComparer.Ordinal))
            throw new InvalidOperationException("The original facility equipment membership changed during review.");
        await Guard();return closing;
    }

    private async Task RequireEquipmentInstallationRecoveryParentsAsync(DevelopmentEquipmentInstallationProposal proposal,Func<Task> authority)
    {
        var context=proposal.Context;proposal.Require();if(_maintenanceAssetAccess is null || context.Database.ConnectionName!=_connectionName)throw new UnauthorizedAccessException("Current original asset authority is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original equipment datasource is unavailable.");
        async Task Guard(){RequireMaintenanceEvidenceLane(source,false);await authority();if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)) || _resolveModuleConnection is null || await _resolveModuleConnection("PPDM_CORE")!=_connectionName || await _resolveModuleConnection("FACILITY")!=_connectionName)throw new InvalidOperationException("The original equipment binding changed.");RequireMaintenanceEvidenceLane(source,false);}
        static AppFilter Equal(string column,string value)=>new(){FieldName=column,Operator="=",FilterValue=value};
        async Task<string> Observe()
        {
            await MaintenanceAssetDefinitionsAsync(source,Guard,[new(typeof(FACILITY),["FACILITY_ID","FACILITY_TYPE"],[("CURRENT_OPERATOR","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID"),("PRIMARY_FIELD_ID","FIELD","FIELD_ID")]),new(typeof(EQUIPMENT),["EQUIPMENT_ID"],[("CURRENT_OWNER_BA_ID","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID")])]);
            var facilities=await MaintenanceAssetRowsCoreAsync<FACILITY>(Guard,[],true,Equal("FACILITY_ID",proposal.FacilityId),Equal("FACILITY_TYPE",proposal.FacilityType),Equal("ACTIVE_IND","Y"));
            if(facilities.Length!=1 || facilities[0].PRIMARY_FIELD_ID!=context.FieldId || !LeaseSummaryValidator.Identifier(facilities[0].CURRENT_OPERATOR) || !AssetAccessLevelChecks.Meets(await _maintenanceAssetAccess.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",facilities[0],facilities[0].CURRENT_OPERATOR==context.Company.BusinessAssociateId),"READ"))throw new UnauthorizedAccessException("Current original facility READ access is required.");
            var equipment=await MaintenanceAssetRowsCoreAsync<EQUIPMENT>(Guard,[],true,Equal("EQUIPMENT_ID",proposal.EquipmentId),Equal("ACTIVE_IND","Y"));
            if(equipment.Length!=1 || equipment[0].CURRENT_OWNER_BA_ID!=context.Company.BusinessAssociateId || !AssetAccessLevelChecks.Meets(await _maintenanceAssetAccess.GetEffectiveAccessLevelAsync(typeof(EQUIPMENT),"EQUIPMENT",equipment[0],false),"READ"))throw new UnauthorizedAccessException("Current original equipment READ access is required.");
            await Guard();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Facility=MaintenanceIdentityRevision(facilities[0]),Equipment=MaintenanceIdentityRevision(equipment[0])}));
        }
        var first=await Observe();var last=await Observe();if(first!=last)throw new InvalidOperationException("The original equipment parents changed during recovery authorization.");await Guard();
    }

    public Task<DevelopmentEquipmentInstallationSource> ReviewDevelopmentEquipmentInstallationAsync(DevelopmentEquipmentInstallationProposal proposal,Func<Task> authority)=>ReviewDevelopmentEquipmentInstallationCoreAsync(proposal,authority,false,null);
    private async Task<DevelopmentEquipmentInstallationSource> ReviewDevelopmentEquipmentInstallationCoreAsync(DevelopmentEquipmentInstallationProposal proposal,Func<Task> authority,bool owned,string? ignoreInstallationGuid)
    {
        proposal.Require();var context=proposal.Context;
        if(_maintenanceAssetAccess is null || context.Database.ConnectionName!=_connectionName)throw new UnauthorizedAccessException("The original company and datasource are required.");
        var original=JsonSerializer.Serialize(proposal);var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The facility source is unavailable.");
        async Task Guard()
        {
            RequireMaintenanceEvidenceLane(source,owned);await authority();RequireMaintenanceEvidenceLane(source,owned);
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)) || _resolveModuleConnection is not null && await _resolveModuleConnection("PPDM_CORE")!=_connectionName || original!=JsonSerializer.Serialize(proposal))throw new InvalidOperationException("The original proposal or datasource changed.");
        }
        static AppFilter Equal(string column,string value)=>new(){FieldName=column,Operator="=",FilterValue=value};
        async Task<DevelopmentEquipmentInstallationSource> Observe()
        {
            await MaintenanceAssetDefinitionsAsync(source,Guard,[new(typeof(FACILITY),["FACILITY_ID","FACILITY_TYPE"],[("CURRENT_OPERATOR","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID"),("PRIMARY_FIELD_ID","FIELD","FIELD_ID")]),new(typeof(EQUIPMENT),["EQUIPMENT_ID"],[("CURRENT_OWNER_BA_ID","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID")]),new(typeof(FACILITY_EQUIPMENT),["FACILITY_ID","FACILITY_TYPE","EQUIPMENT_ID","INSTALL_OBS_NO"],[("EQUIPMENT_ID","EQUIPMENT","EQUIPMENT_ID"),("FACILITY_ID","FACILITY","FACILITY_ID")])]);
            var facilities=await MaintenanceAssetRowsCoreAsync<FACILITY>(Guard,[],true,Equal("FACILITY_ID",proposal.FacilityId),Equal("FACILITY_TYPE",proposal.FacilityType),Equal("ACTIVE_IND","Y"));
            if(facilities.Length!=1 || facilities[0].PRIMARY_FIELD_ID!=context.FieldId || !LeaseSummaryValidator.Identifier(facilities[0].CURRENT_OPERATOR) || !AssetAccessLevelChecks.Meets(await _maintenanceAssetAccess.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",facilities[0],facilities[0].CURRENT_OPERATOR==context.Company.BusinessAssociateId),"WRITE"))throw new UnauthorizedAccessException("Current qualified facility WRITE access is required.");
            var equipment=await MaintenanceAssetRowsCoreAsync<EQUIPMENT>(Guard,[],true,Equal("EQUIPMENT_ID",proposal.EquipmentId),Equal("ACTIVE_IND","Y"));
            if(equipment.Length!=1 || equipment[0].CURRENT_OWNER_BA_ID!=context.Company.BusinessAssociateId || !AssetAccessLevelChecks.Meets(await _maintenanceAssetAccess.GetEffectiveAccessLevelAsync(typeof(EQUIPMENT),"EQUIPMENT",equipment[0],false),"WRITE"))throw new UnauthorizedAccessException("Current selected-company equipment WRITE access is required.");
            var rows=await MaintenanceAssetRowsCoreAsync<FACILITY_EQUIPMENT>(Guard,[],true,Equal("EQUIPMENT_ID",proposal.EquipmentId));
            if(rows.Any(row=>!LeaseSummaryValidator.Identifier(row.FACILITY_ID) || !LeaseSummaryValidator.Identifier(row.FACILITY_TYPE) || row.ACTIVE_IND is not ("Y" or "N") || row.INSTALL_OBS_NO<0 || decimal.Truncate(row.INSTALL_OBS_NO)!=row.INSTALL_OBS_NO) || rows.Select(row=>(row.FACILITY_ID,row.FACILITY_TYPE,row.EQUIPMENT_ID,row.INSTALL_OBS_NO)).Distinct().Count()!=rows.Length)throw new InvalidOperationException("Recorded equipment installation membership is incomplete or ambiguous.");
            if(ignoreInstallationGuid is not null)
            {
                var added=rows.Where(row=>row.PPDM_GUID==ignoreInstallationGuid).ToArray();if(added.Length!=1)throw new InvalidOperationException("The inserted installation is unavailable or ambiguous.");rows=rows.Where(row=>row.PPDM_GUID!=ignoreInstallationGuid).ToArray();
            }
            var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Facility=MaintenanceIdentityRevision(facilities[0]),Equipment=MaintenanceIdentityRevision(equipment[0]),Installations=rows.Select(MaintenanceIdentityRevision).Order(StringComparer.Ordinal)}));
            await Guard();return new(equipment[0].CURRENT_OWNER_BA_ID,rows.Length,rows.Count(row=>row.ACTIVE_IND=="Y"),revision);
        }
        var first=await Observe();var second=await Observe();if(first!=second)throw new InvalidOperationException("Recorded installation evidence changed during review.");await Guard();return second;
    }

    public Task<CompanyMaintenanceQualifiedAssets> ReadMaintenanceQualifiedAssetsAsync(FieldSelectionReview context,WorkOrderDetailModel order,Func<Task> authority) =>
        ReadMaintenanceQualifiedAssetsAsync(context,order,authority,false);

    public Task<CompanyMaintenanceQualifiedAssets> ReadMaintenanceQualifiedWorkAssetsAsync(FieldSelectionReview context,WorkOrderDetailModel order,Func<Task> authority,bool owned)=>
        ReadMaintenanceQualifiedAssetsCoreAsync(context,order,authority,owned,true);

    internal Task<CompanyMaintenanceQualifiedAssets> ReadMaintenanceQualifiedAssetsAsync(FieldSelectionReview context,WorkOrderDetailModel order,Func<Task> authority,bool owned)=>
        ReadMaintenanceQualifiedAssetsCoreAsync(context,order,authority,owned,owned);

    private async Task<CompanyMaintenanceQualifiedAssets> ReadMaintenanceQualifiedAssetsCoreAsync(FieldSelectionReview context,WorkOrderDetailModel order,Func<Task> authority,bool owned,bool completeProjection)
    {
        FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);RecordedWorkOrderComponentChecks.Require(order);
        if(_maintenanceAssetAccess is null || string.IsNullOrWhiteSpace(_maintenanceEvidenceZone) || context.Database.ConnectionName!=_connectionName || order.Source!="WORK_ORDER" || order.FieldId!=context.FieldId || order.Components.Count>128)throw new UnauthorizedAccessException("Original company/FIELD/CORE component scope, date zone and canonical parent policy are required.");
        try{_=TimeZoneInfo.FindSystemTimeZoneById(_maintenanceEvidenceZone);}catch(Exception error) when(error is TimeZoneNotFoundException or InvalidTimeZoneException){throw new InvalidOperationException("Explicit company native asset date interpretation is unavailable.",error);}
        foreach(var component in order.Components)if((!string.IsNullOrWhiteSpace(component.FacilityId) || !string.IsNullOrWhiteSpace(component.FacilityType)) && (!LeaseSummaryValidator.Identifier(component.FacilityId) || !LeaseSummaryValidator.Identifier(component.FacilityType)))throw new InvalidOperationException("Every original facility component requires both native identity keys.");
        var original=CompanyInventorySourceReview.Copy(order);var orderRevision=CompanyWorkOrderExecutionRequirements.OrderRevision(original);
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original qualified-asset CORE source is unavailable.");
        async Task Guard()
        {
            RequireMaintenanceEvidenceLane(source,owned);await authority();RequireMaintenanceEvidenceLane(source,owned);
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)) || _resolveModuleConnection is not null && await _resolveModuleConnection("PPDM_CORE")!=_connectionName || CompanyWorkOrderExecutionRequirements.OrderRevision(order)!=orderRevision)throw new InvalidOperationException("Original qualified component/source binding changed during review.");
        }
        Task Definition(Type type,string[] keys,params (string Column,string Table,string Key)[] relationships)=>MaintenanceAssetDefinitionAsync(source,Guard,type,keys,relationships);
        Task<T[]> Rows<T>(params AppFilter[] filters) where T:class=>MaintenanceAssetRowsCoreAsync<T>(Guard,Array.Empty<string>(),completeProjection,filters);
        static AppFilter Equal(string name,string value)=>new(){FieldName=name,Operator="=",FilterValue=value};
        static bool Current(DateTime? from,DateTime? to,DateTime at)=>from is {} start && start<=at && (to is null || to>at && to>start);
        async Task<CompanyMaintenanceQualifiedAssets> Observe()
        {
            await Definition(typeof(FACILITY),["FACILITY_ID","FACILITY_TYPE"],("CURRENT_OPERATOR","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID"),("PRIMARY_FIELD_ID","FIELD","FIELD_ID"));
            var equipment=new List<CompanyMaintenanceEquipmentIdentity>();var facilities=new List<CompanyMaintenanceFacilityIdentity>();
            foreach(var id in original.Components.Where(row=>!string.IsNullOrWhiteSpace(row.EquipmentId)).Select(row=>row.EquipmentId!).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
            {
                await Definition(typeof(EQUIPMENT),["EQUIPMENT_ID"],("CURRENT_OWNER_BA_ID","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID"));
                var equipmentRows=await Rows<EQUIPMENT>(Equal("EQUIPMENT_ID",id),Equal("ACTIVE_IND","Y"));
                if(equipmentRows.Length!=1 || equipmentRows[0].CURRENT_OWNER_BA_ID!=context.Company.BusinessAssociateId)throw new UnauthorizedAccessException("Original equipment must have one active current selected-company owner.");var row=equipmentRows[0];
                var access=await _maintenanceAssetAccess.GetEffectiveAccessLevelAsync(typeof(EQUIPMENT),"EQUIPMENT",row,false);
                if(!AssetAccessLevelChecks.Meets(access,"READ"))throw new UnauthorizedAccessException("Current direct equipment READ authority is required before installation evidence.");
                var now=DateTime.UtcNow;var from=MaintenanceIdentityUtc(row.EFFECTIVE_DATE);var to=MaintenanceIdentityUtc(row.EXPIRY_DATE);var commissioned=MaintenanceIdentityUtc(row.COMMISSION_DATE);var decommissioned=MaintenanceIdentityUtc(row.DECOMMISSION_DATE);
                if(!Current(from,to,now) || commissioned>now || decommissioned<=now)throw new InvalidOperationException("Current equipment effective and commissioning periods are unresolved.");
                await Definition(typeof(FACILITY_EQUIPMENT),["FACILITY_ID","FACILITY_TYPE","EQUIPMENT_ID","INSTALL_OBS_NO"],("EQUIPMENT_ID","EQUIPMENT","EQUIPMENT_ID"),("FACILITY_ID","FACILITY","FACILITY_ID"));
                var population=await Rows<FACILITY_EQUIPMENT>(Equal("EQUIPMENT_ID",id));
                if(population.Select(item=>(item.FACILITY_ID,item.FACILITY_TYPE,item.EQUIPMENT_ID,item.INSTALL_OBS_NO)).Distinct().Count()!=population.Length)throw new InvalidOperationException("Native equipment installation compound membership is ambiguous.");
                var current=new List<CompanyMaintenanceEquipmentInstallation>();
                foreach(var item in population)
                {
                    if(!LeaseSummaryValidator.Identifier(item.FACILITY_ID) || !LeaseSummaryValidator.Identifier(item.FACILITY_TYPE) || item.INSTALL_OBS_NO<0 || decimal.Truncate(item.INSTALL_OBS_NO)!=item.INSTALL_OBS_NO || item.ACTIVE_IND is not ("Y" or "N"))throw new InvalidOperationException("Complete qualified installation keys and active indicators are required.");
                    var effective=MaintenanceIdentityUtc(item.EFFECTIVE_DATE);var expiry=MaintenanceIdentityUtc(item.EXPIRY_DATE);var installed=MaintenanceIdentityUtc(item.INSTALL_DATE);var removed=MaintenanceIdentityUtc(item.REMOVE_DATE);
                    if(effective is null || installed is null || expiry<=effective || removed<=installed)throw new InvalidOperationException("Every native installation period must be explicitly established before current membership is selected.");
                    if(item.ACTIVE_IND!="Y")continue;if(effective>now || installed>now)throw new InvalidOperationException("A future native equipment installation requires reconciliation before maintenance return.");
                    if(Current(effective,expiry,now) && Current(installed,removed,now))current.Add(new(id,item.FACILITY_ID,item.FACILITY_TYPE,item.INSTALL_OBS_NO,effective.Value,installed.Value,expiry,removed,item.SOURCE,MaintenanceIdentityRevision(item),population.Length,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(population.Select(MaintenanceIdentityRevision).Order(StringComparer.Ordinal)))));
                }
                if(current.Count!=1)throw new InvalidOperationException("One complete unambiguous current qualified equipment installation is required.");
                equipment.Add(new(id,row.CURRENT_OWNER_BA_ID,access!,from!.Value,to,commissioned,decommissioned,MaintenanceIdentityRevision(row),current[0]));await Guard();
            }
            var targets=original.Components.Where(row=>!string.IsNullOrWhiteSpace(row.FacilityId)).Select(row=>(Id:row.FacilityId!,Type:row.FacilityType!)).Concat(equipment.Select(row=>(row.Installation.FacilityId,row.Installation.FacilityType))).Distinct().OrderBy(row=>row.Item1,StringComparer.Ordinal).ThenBy(row=>row.Item2,StringComparer.Ordinal).ToArray();
            foreach(var target in targets)
            {
                var rows=await Rows<FACILITY>(Equal("FACILITY_ID",target.Item1),Equal("FACILITY_TYPE",target.Item2),Equal("ACTIVE_IND","Y"));
                if(rows.Length!=1 || !LeaseSummaryValidator.Identifier(rows[0].CURRENT_OPERATOR) || rows[0].PRIMARY_FIELD_ID!=context.FieldId)throw new UnauthorizedAccessException("Qualified facility must have one recorded operator and the original FIELD.");var row=rows[0];
                // A partner-operated parent requires explicit direct facility access in addition to the company FIELD path.
                var access=await _maintenanceAssetAccess.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",row,row.CURRENT_OPERATOR==context.Company.BusinessAssociateId);if(!AssetAccessLevelChecks.Meets(access,"READ"))throw new UnauthorizedAccessException("Current qualified facility READ authority is required before its native operating evidence.");
                var from=MaintenanceIdentityUtc(row.EFFECTIVE_DATE);var to=MaintenanceIdentityUtc(row.EXPIRY_DATE);if(!Current(from,to,DateTime.UtcNow))throw new InvalidOperationException("Qualified facility effective period is unresolved.");
                facilities.Add(new(row.FACILITY_ID,row.FACILITY_TYPE,row.PRIMARY_FIELD_ID,row.CURRENT_OPERATOR,access!,from!.Value,to,MaintenanceIdentityRevision(row)));await Guard();
            }
            var closingDefinitions=new List<MaintenanceAssetDefinition>{new(typeof(FACILITY),["FACILITY_ID","FACILITY_TYPE"],[("CURRENT_OPERATOR","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID"),("PRIMARY_FIELD_ID","FIELD","FIELD_ID")])};
            if(equipment.Count>0)closingDefinitions.AddRange([
                new(typeof(EQUIPMENT),["EQUIPMENT_ID"],[("CURRENT_OWNER_BA_ID","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID")]),
                new(typeof(FACILITY_EQUIPMENT),["FACILITY_ID","FACILITY_TYPE","EQUIPMENT_ID","INSTALL_OBS_NO"],[("EQUIPMENT_ID","EQUIPMENT","EQUIPMENT_ID"),("FACILITY_ID","FACILITY","FACILITY_ID")])]);
            await MaintenanceAssetDefinitionsAsync(source,Guard,closingDefinitions);
            var result=new CompanyMaintenanceQualifiedAssets(context,orderRevision,_maintenanceEvidenceZone!,facilities,equipment,DateTime.UtcNow);result.Require(original);await Guard();return result;
        }
        var first=await Observe();var closing=await Observe();if(first.SourceRevision()!=closing.SourceRevision())throw new InvalidOperationException("Original qualified native ownership, installation membership, parent authority or date interpretation changed during review.");await Guard();return closing;
    }
}
