using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.ProductionOperations.Services;

public sealed partial class FacilityManagementService
{
    public async Task<CompanyMaintenanceQualifiedAssets> ReadMaintenanceQualifiedCurrentStatusAsync(FieldSelectionReview context,WorkOrderDetailModel order,Func<Task> authority)
    {
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Current qualified status source is unavailable.");
        var orderRevision=CompanyWorkOrderExecutionRequirements.OrderRevision(order);
        async Task Guard()
        {
            RequireMaintenanceEvidenceLane(source,false);await authority();RequireMaintenanceEvidenceLane(source,false);
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)) || CompanyWorkOrderExecutionRequirements.OrderRevision(order)!=orderRevision)
                throw new InvalidOperationException("Current qualified status source or original component scope changed.");
        }
        await Guard();var identity=await ReadMaintenanceQualifiedAssetsAsync(context,order,Guard);
        var operating=await ReadMaintenanceQualifiedOperatingAsync(context,order,identity,Guard);
        var result=identity with{Operating=operating};result.Require(order);await Guard();return result;
    }
    public Task<CompanyMaintenanceQualifiedOperating> ReadMaintenanceQualifiedOperatingAsync(FieldSelectionReview context,WorkOrderDetailModel order,CompanyMaintenanceQualifiedAssets identity,Func<Task> authority) =>
        ReadMaintenanceQualifiedOperatingAsync(context,order,identity,authority,false);

    internal async Task<CompanyMaintenanceQualifiedOperating> ReadMaintenanceQualifiedOperatingAsync(FieldSelectionReview context,WorkOrderDetailModel order,CompanyMaintenanceQualifiedAssets identity,Func<Task> authority,bool owned)
    {
        identity.Require(order);if(identity.Context!=context || context.Database.ConnectionName!=_connectionName || _maintenanceAssetAccess is null)throw new UnauthorizedAccessException("Original qualified identity, company/FIELD/source and parent authority are required before native status reads.");
        var baseline=identity with{Operating=null};var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original native operating evidence source is unavailable.");
        async Task Guard(){RequireMaintenanceEvidenceLane(source,owned);await authority();RequireMaintenanceEvidenceLane(source,owned);if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)) || CompanyWorkOrderExecutionRequirements.OrderRevision(order)!=identity.OrderRevision)throw new InvalidOperationException("Original native status source/component scope changed.");}
        async Task Parents(){await Guard();var current=await ReadMaintenanceQualifiedAssetsAsync(context,order,authority,owned);if(current.IdentityRevision()!=baseline.IdentityRevision())throw new InvalidOperationException("Original qualified parent ownership, installation or rights changed around native status evidence.");await Guard();}
        async Task<CompanyMaintenanceQualifiedOperating> Observe()
        {
            await Parents();var definitions=new Dictionary<Type,(string[] Keys,(string Column,string Table,string Key)[] Relationships)>();
            async Task Definition(Type type,string[] keys,params (string Column,string Table,string Key)[] relationships){await MaintenanceAssetDefinitionAsync(source,Guard,type,keys,relationships);definitions[type]=(keys,relationships);}
            Task<T[]> Rows<T>(string[] required,params AppFilter[] filters) where T:class=>MaintenanceAssetRowsCoreAsync<T>(Guard,required,owned,filters);
            static AppFilter Equal(string name,string value)=>new(){FieldName=name,Operator="=",FilterValue=value};
            var facilityRows=new List<FACILITY_STATUS>();var equipmentRows=new List<EQUIPMENT_STATUS>();
            if(baseline.Facilities.Count>0)await Definition(typeof(FACILITY_STATUS),["FACILITY_ID","FACILITY_TYPE","STATUS_ID"],("FACILITY_ID","FACILITY","FACILITY_ID"),("STATUS_TYPE","R_FACILITY_STATUS","STATUS_TYPE"),("TIMEZONE","R_TIMEZONE","TIMEZONE"));
            foreach(var facility in baseline.Facilities){var parent=await Rows<FACILITY>(["FACILITY_ID","FACILITY_TYPE","CURRENT_OPERATOR","PRIMARY_FIELD_ID"],Equal("FACILITY_ID",facility.FacilityId),Equal("FACILITY_TYPE",facility.FacilityType),Equal("ACTIVE_IND","Y"));if(parent.Length!=1 || MaintenanceIdentityRevision(parent[0])!=facility.NativeRevision || await _maintenanceAssetAccess.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",parent[0],parent[0].CURRENT_OPERATOR==context.Company.BusinessAssociateId)!=facility.AccessLevel)throw new UnauthorizedAccessException("Original qualified facility source/READ authority changed before status reads.");await Guard();facilityRows.AddRange(await Rows<FACILITY_STATUS>(["FACILITY_ID","FACILITY_TYPE","STATUS_ID","STATUS_TYPE","STATUS","SOURCE","ACTIVE_IND"],Equal("FACILITY_ID",facility.FacilityId),Equal("FACILITY_TYPE",facility.FacilityType)));}
            if(baseline.Equipment.Count>0)await Definition(typeof(EQUIPMENT_STATUS),["EQUIPMENT_ID","EQUIP_STATUS_ID"],("EQUIPMENT_ID","EQUIPMENT","EQUIPMENT_ID"),("EQUIP_STATUS_TYPE","R_EQUIP_STATUS","STATUS_TYPE"),("TIMEZONE","R_TIMEZONE","TIMEZONE"));
            foreach(var equipment in baseline.Equipment){var parent=await Rows<EQUIPMENT>(["EQUIPMENT_ID","CURRENT_OWNER_BA_ID"],Equal("EQUIPMENT_ID",equipment.EquipmentId),Equal("ACTIVE_IND","Y"));if(parent.Length!=1 || MaintenanceIdentityRevision(parent[0])!=equipment.NativeRevision || await _maintenanceAssetAccess.GetEffectiveAccessLevelAsync(typeof(EQUIPMENT),"EQUIPMENT",parent[0],false)!=equipment.AccessLevel)throw new UnauthorizedAccessException("Original equipment source/direct READ authority changed before status reads.");await Guard();equipmentRows.AddRange(await Rows<EQUIPMENT_STATUS>(["EQUIPMENT_ID","EQUIP_STATUS_ID","EQUIP_STATUS_TYPE","EQUIP_STATUS","SOURCE","ACTIVE_IND"],Equal("EQUIPMENT_ID",equipment.EquipmentId)));}
            if(facilityRows.Count+equipmentRows.Count>1024)throw new InvalidOperationException("Qualified status population exceeds this complete bounded review.");
            DateTime? Utc(DateTime? value,string? zone,ref bool confirmed)
            {
                if(value is null)return null;if(value.Value.Kind==DateTimeKind.Utc)return value;
                if(value.Value.Kind==DateTimeKind.Local || string.IsNullOrWhiteSpace(zone)){confirmed=false;return null;}
                try{var selected=TimeZoneInfo.FindSystemTimeZoneById(zone);if(selected.IsInvalidTime(value.Value) || selected.IsAmbiguousTime(value.Value)){confirmed=false;return null;}return TimeZoneInfo.ConvertTimeToUtc(value.Value,selected);}catch(Exception error) when(error is TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException){confirmed=false;return null;}
            }
            static bool Period(DateTime? from,DateTime? to,DateTime at)=> (from is null || from<=at) && (to is null || to>at) && (from is null || to is null || to>from);
            var zones=new Dictionary<string,bool>(StringComparer.Ordinal);var zoneRevisions=new List<string>();
            foreach(var zone in facilityRows.Select(row=>row.TIMEZONE).Concat(equipmentRows.Select(row=>row.TIMEZONE)).Where(value=>!string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
            {
                await Definition(typeof(R_TIMEZONE),["TIMEZONE"]);var rows=await Rows<R_TIMEZONE>(["TIMEZONE","ACTIVE_IND"],Equal("TIMEZONE",zone));if(rows.Length>1)throw new InvalidOperationException("Native status time-zone identity is ambiguous.");zoneRevisions.AddRange(rows.Select(MaintenanceIdentityRevision));var valid=rows.Length==1 && rows[0].ACTIVE_IND=="Y";
                if(valid){try{_=TimeZoneInfo.FindSystemTimeZoneById(zone);}catch(Exception error) when(error is TimeZoneNotFoundException or InvalidTimeZoneException){valid=false;}var from=Utc(rows[0].EFFECTIVE_DATE,zone,ref valid);var to=Utc(rows[0].EXPIRY_DATE,zone,ref valid);valid=valid && Period(from,to,DateTime.UtcNow);}zones[zone]=valid;
            }
            CompanyMaintenanceQualifiedStatus Status(string asset,string id,string? qualifier,string key,string type,string value,string stream,string active,DateTime? from,DateTime? start,DateTime? expiry,DateTime? end,string? nativeZone,object row)
            {
                var confirmed=string.IsNullOrWhiteSpace(nativeZone) || zones.GetValueOrDefault(nativeZone);var zone=string.IsNullOrWhiteSpace(nativeZone)?_maintenanceEvidenceZone:confirmed?nativeZone:null;
                var effective=Utc(from,zone,ref confirmed);var started=Utc(start,zone,ref confirmed);var expires=Utc(expiry,zone,ref confirmed);var ended=Utc(end,zone,ref confirmed);
                return new(asset,id,qualifier,key,type,value,stream,active,effective,started,expires,ended,nativeZone,confirmed,MaintenanceIdentityRevision(row));
            }
            var statuses=facilityRows.Select(row=>Status("FACILITY",row.FACILITY_ID,row.FACILITY_TYPE,row.STATUS_ID,row.STATUS_TYPE,row.STATUS,row.SOURCE,row.ACTIVE_IND,row.EFFECTIVE_DATE,row.START_TIME,row.EXPIRY_DATE,row.END_TIME,row.TIMEZONE,row)).Concat(equipmentRows.Select(row=>Status("EQUIPMENT",row.EQUIPMENT_ID,null,row.EQUIP_STATUS_ID,row.EQUIP_STATUS_TYPE,row.EQUIP_STATUS,row.SOURCE,row.ACTIVE_IND,row.EFFECTIVE_DATE,row.START_TIME,row.EXPIRY_DATE,row.END_TIME,row.TIMEZONE,row))).OrderBy(row=>row.AssetType,StringComparer.Ordinal).ThenBy(row=>row.AssetId,StringComparer.Ordinal).ThenBy(row=>row.FacilityType,StringComparer.Ordinal).ThenBy(row=>row.StatusId,StringComparer.Ordinal).ToArray();
            var references=new List<CompanyMaintenanceQualifiedStatusReference>();
            foreach(var family in new[]{"FACILITY","EQUIPMENT"})foreach(var statusType in statuses.Where(row=>row.AssetType==family).Select(row=>row.StatusType).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
            {
                bool typeCurrent;string typeRevision;
                if(family=="FACILITY")
                {
                    await Definition(typeof(R_FAC_STATUS_TYPE),["STATUS_TYPE"]);var types=await Rows<R_FAC_STATUS_TYPE>(["STATUS_TYPE","ACTIVE_IND"],Equal("STATUS_TYPE",statusType));if(types.Length>1)throw new InvalidOperationException("Native facility status-type identity is ambiguous.");var temporal=true;var from=types.Length==1?Utc(types[0].EFFECTIVE_DATE,_maintenanceEvidenceZone,ref temporal):null;var to=types.Length==1?Utc(types[0].EXPIRY_DATE,_maintenanceEvidenceZone,ref temporal):null;typeCurrent=types.Length==1 && types[0].ACTIVE_IND=="Y" && temporal && Period(from,to,DateTime.UtcNow);typeRevision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(types.Select(MaintenanceIdentityRevision)));
                    await Definition(typeof(R_FACILITY_STATUS),["STATUS_TYPE","STATUS"],("STATUS_TYPE","R_FAC_STATUS_TYPE","STATUS_TYPE"));foreach(var row in await Rows<R_FACILITY_STATUS>(["STATUS_TYPE","STATUS","ACTIVE_IND"],Equal("STATUS_TYPE",statusType))){var confirmed=true;var effective=Utc(row.EFFECTIVE_DATE,_maintenanceEvidenceZone,ref confirmed);var expires=Utc(row.EXPIRY_DATE,_maintenanceEvidenceZone,ref confirmed);references.Add(new(family,row.STATUS_TYPE,row.STATUS,row.ACTIVE_IND,effective,expires,confirmed,typeCurrent,MaintenanceIdentityRevision(row),typeRevision));}
                }
                else
                {
                    await Definition(typeof(R_EQUIP_STATUS_TYPE),["STATUS_TYPE"]);var types=await Rows<R_EQUIP_STATUS_TYPE>(["STATUS_TYPE","ACTIVE_IND"],Equal("STATUS_TYPE",statusType));if(types.Length>1)throw new InvalidOperationException("Native equipment status-type identity is ambiguous.");var temporal=true;var from=types.Length==1?Utc(types[0].EFFECTIVE_DATE,_maintenanceEvidenceZone,ref temporal):null;var to=types.Length==1?Utc(types[0].EXPIRY_DATE,_maintenanceEvidenceZone,ref temporal):null;typeCurrent=types.Length==1 && types[0].ACTIVE_IND=="Y" && temporal && Period(from,to,DateTime.UtcNow);typeRevision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(types.Select(MaintenanceIdentityRevision)));
                    await Definition(typeof(R_EQUIP_STATUS),["STATUS_TYPE","EQUIP_STATUS"],("STATUS_TYPE","R_EQUIP_STATUS_TYPE","STATUS_TYPE"));foreach(var row in await Rows<R_EQUIP_STATUS>(["STATUS_TYPE","EQUIP_STATUS","ACTIVE_IND"],Equal("STATUS_TYPE",statusType))){var confirmed=true;var effective=Utc(row.EFFECTIVE_DATE,_maintenanceEvidenceZone,ref confirmed);var expires=Utc(row.EXPIRY_DATE,_maintenanceEvidenceZone,ref confirmed);references.Add(new(family,row.STATUS_TYPE,row.EQUIP_STATUS,row.ACTIVE_IND,effective,expires,confirmed,typeCurrent,MaintenanceIdentityRevision(row),typeRevision));}
                }
                if(references.Count>1024)throw new InvalidOperationException("Qualified status-reference membership exceeds this complete bounded review.");
            }
            // The next observation's opening Parents(), or the final Parents(), closes this complete observation.
            await MaintenanceAssetDefinitionsAsync(source,Guard,definitions.Select(definition=>new MaintenanceAssetDefinition(definition.Key,definition.Value.Keys,definition.Value.Relationships)).ToArray());
            var utc=DateTime.UtcNow;var result=new CompanyMaintenanceQualifiedOperating(context,baseline.IdentityRevision(),statuses,references,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(zoneRevisions.Order(StringComparer.Ordinal))),utc,CompanyMaintenanceQualifiedOperating.GetBlockers(baseline,statuses,references,utc));result.Require(baseline);await Guard();return result;
        }
        var first=await Observe();var closing=await Observe();if(first.SourceRevision()!=closing.SourceRevision())throw new InvalidOperationException("Complete qualified native status/reference/date/parent evidence changed during review.");await Parents();await Guard();return closing;
    }
}
