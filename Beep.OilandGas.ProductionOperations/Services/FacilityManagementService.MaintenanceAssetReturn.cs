using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ProductionOperations.Services;

public sealed partial class FacilityManagementService
{
    // A prepared graph is consumed only by the existing complete return coordinator; it records no outcome itself.
    internal sealed record QualifiedAssetReturnRows(FACILITY_STATUS[] BeforeFacilities,EQUIPMENT_STATUS[] BeforeEquipment,
        FACILITY_STATUS[] AfterFacilities,EQUIPMENT_STATUS[] AfterEquipment,R_TIMEZONE UtcReference,DateTime RecordedAtUtc,R_TIMEZONE[] TimeZoneReferences);

    private static void RequireQualifiedReturnArguments(string returnId,string reason)
    {
        if(!LeaseSummaryValidator.Identifier(returnId) || returnId.Length>128 || string.IsNullOrWhiteSpace(reason) || reason.Length>4096 ||
            reason.Any(ch=>char.IsControl(ch) && ch is not ('\r' or '\n' or '\t')))
            throw new ArgumentException("The original retained return identity and explicit observation reason are required.");
    }
    internal async Task<QualifiedAssetReturnRows> PrepareQualifiedAssetReturnRowsAsync(CompanyMaintenanceQualifiedAssets reviewed,
        WorkOrderDetailModel order,IReadOnlyList<CompanyMaintenanceQualifiedReturnObservation> observations,string assignedRole,
        string returnId,DateTime activatedAtUtc,string reason,Func<Task> authority,DateTime? recordedAtUtc=null)
    {
        var transitions=CompanyMaintenanceQualifiedAssetReturn.Resolve(reviewed,order,observations,assignedRole,activatedAtUtc);
        RequireQualifiedReturnArguments(returnId,reason);
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original qualified return source is unavailable.");
        async Task Guard()
        {
            RequireMaintenanceEvidenceLane(source,true);await authority();RequireMaintenanceEvidenceLane(source,true);
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)) || reviewed.Context.Database.ConnectionName!=_connectionName)
                throw new InvalidOperationException("Original qualified return source changed during preparation.");
        }
        await RequireQualifiedReturnSourcesAsync(reviewed,order,observations,assignedRole,Guard);
        var input=await ReadQualifiedReturnInputsAsync(reviewed,transitions,activatedAtUtc,reason,source,Guard);
        await RequireQualifiedReturnSourcesAsync(reviewed,order,observations,assignedRole,Guard);
        var result=BuildQualifiedAssetReturnRows(reviewed,order,observations,assignedRole,returnId,activatedAtUtc,reason,
            input.Facilities,input.Equipment,input.Zones,recordedAtUtc??DateTime.UtcNow);
        await RequireQualifiedReturnClosingZonesAsync(input.Zones,Guard);await Guard();return result;
    }
    private async Task<QualifiedAssetReturnRows> RevalidateQualifiedAssetReturnRowsAsync(CompanyMaintenanceQualifiedAssets reviewed,
        WorkOrderDetailModel order,IReadOnlyList<CompanyMaintenanceQualifiedReturnObservation> observations,string assignedRole,
        string returnId,DateTime activatedAtUtc,string reason,QualifiedAssetReturnRows prepared,Func<Task> Guard)
    {
        RequireQualifiedReturnArguments(returnId,reason);
        var transitions=CompanyMaintenanceQualifiedAssetReturn.Resolve(reviewed,order,observations,assignedRole,activatedAtUtc);
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original qualified return source is unavailable.");
        await Guard();var input=await ReadQualifiedReturnInputsAsync(reviewed,transitions,activatedAtUtc,reason,source,Guard);
        // The caller has checked target authority; close the private input read with current complete sources before mutation.
        await RequireQualifiedReturnSourcesAsync(reviewed,order,observations,assignedRole,Guard);
        var expected=BuildQualifiedAssetReturnRows(reviewed,order,observations,assignedRole,returnId,activatedAtUtc,reason,
            input.Facilities,input.Equipment,input.Zones,prepared.RecordedAtUtc);
        await RequireQualifiedReturnClosingZonesAsync(input.Zones,Guard);await Guard();return expected;
    }
    private async Task RequireQualifiedReturnSourcesAsync(CompanyMaintenanceQualifiedAssets reviewed,WorkOrderDetailModel order,
        IReadOnlyList<CompanyMaintenanceQualifiedReturnObservation> observations,string assignedRole,Func<Task> Guard)
    {
        await Guard();var identity=await ReadMaintenanceQualifiedAssetsAsync(reviewed.Context,order,Guard,true);
        var operating=await ReadMaintenanceQualifiedOperatingAsync(reviewed.Context,order,identity,Guard,true);
        var licensing=await ReadMaintenanceQualifiedLicensingAsync(reviewed.Context,order,identity,Guard,true);
        var current=identity with{Operating=operating,Licensing=licensing};var policy=reviewed.ReturnPolicy!;
        var catalog=await ReadMaintenanceReturnPoliciesAsync(CompanyMaintenanceQualifiedReturnPolicy.ContextFor(current,policy.ActorReturnRoles),Guard,true);
        catalog=CompanyMaintenanceQualifiedReturnPolicy.ScopeCatalog(current,order,catalog);
        var references=await ReadMaintenanceReturnNativeSourcesAsync(reviewed.Context,order,current,catalog,Guard,true);
        var comparison=CompanyMaintenanceQualifiedReturnPolicy.Create(current,order,catalog,policy.ActorReturnRoles,references,DateTime.UtcNow);
        current=current with{ReturnPolicy=comparison};current.Require(order);
        if(current.SourceRevision()!=reviewed.SourceRevision())throw new InvalidOperationException("Original qualified ownership, installation, operating/licensing/policy/source or WRITE evidence changed before return.");
        _=CompanyMaintenanceQualifiedAssetReturn.Resolve(current,order,observations,assignedRole,DateTime.UtcNow);
        await Guard();
    }
    private async Task<(FACILITY_STATUS[] Facilities,EQUIPMENT_STATUS[] Equipment,R_TIMEZONE[] Zones)> ReadQualifiedReturnInputsAsync(
        CompanyMaintenanceQualifiedAssets reviewed,IReadOnlyList<CompanyMaintenanceQualifiedReturnTransition> transitions,
        DateTime activatedAtUtc,string reason,TheTechIdea.Beep.IDataSource source,Func<Task> Guard)
    {
        var facilities=new List<FACILITY_STATUS>();var equipment=new List<EQUIPMENT_STATUS>();
        foreach(var row in reviewed.Facilities)facilities.AddRange(await MaintenanceAssetRowsCoreAsync<FACILITY_STATUS>(Guard,
            ["FACILITY_ID","FACILITY_TYPE","STATUS_ID","STATUS","STATUS_TYPE","SOURCE"],true,
            Equal("FACILITY_ID",row.FacilityId),Equal("FACILITY_TYPE",row.FacilityType)));
        foreach(var row in reviewed.Equipment)equipment.AddRange(await MaintenanceAssetRowsCoreAsync<EQUIPMENT_STATUS>(Guard,
            ["EQUIPMENT_ID","EQUIP_STATUS_ID","EQUIP_STATUS","EQUIP_STATUS_TYPE","SOURCE"],true,Equal("EQUIPMENT_ID",row.EquipmentId)));
        var actualStatus=facilities.Select(row=>(Type:"FACILITY",Id:row.FACILITY_ID,Qualifier:(string?)row.FACILITY_TYPE,Key:row.STATUS_ID,Revision:MaintenanceIdentityRevision(row)))
            .Concat(equipment.Select(row=>(Type:"EQUIPMENT",Id:row.EQUIPMENT_ID,Qualifier:(string?)null,Key:row.EQUIP_STATUS_ID,Revision:MaintenanceIdentityRevision(row)))).ToArray();
        var expectedStatus=reviewed.Operating!.Statuses.Select(row=>(Type:row.AssetType,Id:row.AssetId,Qualifier:row.FacilityType,Key:row.StatusId,Revision:row.NativeRevision)).ToHashSet();
        if(actualStatus.Length!=expectedStatus.Count || actualStatus.Distinct().Count()!=actualStatus.Length || actualStatus.Any(row=>!expectedStatus.Contains(row)))
            throw new InvalidOperationException("The complete raw qualified status population differs from its reviewed native proof.");
        await MaintenanceAssetDefinitionAsync(source,Guard,typeof(R_TIMEZONE),["TIMEZONE"]);
        var zones=await MaintenanceAssetRowsCoreAsync<R_TIMEZONE>(Guard,["TIMEZONE","ACTIVE_IND"],true,Equal("TIMEZONE","UTC"));
        static DateTime? ReferenceUtc(DateTime? value)=>value?.Kind==DateTimeKind.Local?
            throw new InvalidOperationException("The native UTC reference cannot use machine-local dates."):
            value is {} date?DateTime.SpecifyKind(date,DateTimeKind.Utc):null;
        if(zones.Length!=1 || zones[0].ACTIVE_IND!="Y" || ReferenceUtc(zones[0].EFFECTIVE_DATE)>activatedAtUtc || ReferenceUtc(zones[0].EXPIRY_DATE)<=activatedAtUtc)
            throw new InvalidOperationException("One recorded native UTC reference must cover actual qualified activation.");
        foreach(var type in transitions.Select(row=>row.AssetType=="FACILITY"?typeof(FACILITY_STATUS):typeof(EQUIPMENT_STATUS)).Distinct())
        {
            await Guard();var shape=source.GetEntityStructure(new EntityStructure{EntityName=type.Name,DatasourceEntityName=type.Name},true);
            var id=type==typeof(FACILITY_STATUS)?"STATUS_ID":"EQUIP_STATUS_ID";
            foreach(var (name,length) in new[]{(id,35),("REMARK",reason.Length)})
                if(shape?.Fields.SingleOrDefault(row=>row.FieldName==name) is not {} field || field.Size1>0 && field.Size1<length)
                    throw new InvalidOperationException("Native qualified status identity/reason capacity cannot retain this return.");
        }
        var allZones=new List<R_TIMEZONE>();
        foreach(var zone in facilities.Select(row=>row.TIMEZONE).Concat(equipment.Select(row=>row.TIMEZONE)).Where(value=>!string.IsNullOrWhiteSpace(value)).Append("UTC").Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            var values=await MaintenanceAssetRowsCoreAsync<R_TIMEZONE>(Guard,["TIMEZONE","ACTIVE_IND"],true,Equal("TIMEZONE",zone));
            if(values.Length!=1)throw new InvalidOperationException("Every original/new status time zone requires one complete native reference.");allZones.Add(values[0]);
        }
        return(facilities.ToArray(),equipment.ToArray(),allZones.ToArray());
        static AppFilter Equal(string name,string value)=>new(){FieldName=name,Operator="=",FilterValue=value};
    }
    private async Task RequireQualifiedReturnClosingZonesAsync(IReadOnlyList<R_TIMEZONE> references,Func<Task> Guard)
    {
        foreach(var reference in references)
        {
            var closing=await MaintenanceAssetRowsCoreAsync<R_TIMEZONE>(Guard,["TIMEZONE","ACTIVE_IND"],true,Equal("TIMEZONE",reference.TIMEZONE));
            if(closing.Length!=1 || MaintenanceIdentityRevision(closing[0])!=MaintenanceIdentityRevision(reference))throw new InvalidOperationException("Native time-zone reference changed around complete return preparation.");
        }
        static AppFilter Equal(string name,string value)=>new(){FieldName=name,Operator="=",FilterValue=value};
    }
    private static QualifiedAssetReturnRows BuildQualifiedAssetReturnRows(CompanyMaintenanceQualifiedAssets reviewed,WorkOrderDetailModel order,
        IReadOnlyList<CompanyMaintenanceQualifiedReturnObservation> observations,string assignedRole,string returnId,DateTime activatedAtUtc,string reason,
        FACILITY_STATUS[] facilities,EQUIPMENT_STATUS[] equipment,R_TIMEZONE[] zones,DateTime utc)
    {
        var transitions=CompanyMaintenanceQualifiedAssetReturn.Resolve(reviewed,order,observations,assignedRole,activatedAtUtc);
        if(utc.Kind!=DateTimeKind.Utc || utc>DateTime.UtcNow || utc<activatedAtUtc || !LeaseSummaryValidator.Identifier(returnId) || returnId.Length>128 || string.IsNullOrWhiteSpace(reason) || reason.Length>4096)
            throw new InvalidOperationException("Original qualified command and retained native audit UTC are required.");
        var expected=reviewed.Operating!.Statuses.Select(row=>(row.AssetType,row.AssetId,row.FacilityType,row.StatusId,row.NativeRevision)).ToHashSet();
        var actual=facilities.Select(row=>(AssetType:"FACILITY",AssetId:row.FACILITY_ID,FacilityType:(string?)row.FACILITY_TYPE,StatusId:row.STATUS_ID,NativeRevision:MaintenanceIdentityRevision(row)))
            .Concat(equipment.Select(row=>(AssetType:"EQUIPMENT",AssetId:row.EQUIPMENT_ID,FacilityType:(string?)null,StatusId:row.EQUIP_STATUS_ID,NativeRevision:MaintenanceIdentityRevision(row)))).ToArray();
        if(actual.Length!=expected.Count || actual.Distinct().Count()!=actual.Length || actual.Any(row=>!expected.Contains(row)))throw new InvalidOperationException("Original complete qualified preimage differs from retained native source evidence.");
        var originalZones=facilities.Select(row=>row.TIMEZONE).Concat(equipment.Select(row=>row.TIMEZONE)).Where(value=>!string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.Ordinal).ToArray();
        var needed=originalZones.Append("UTC").Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        if(zones.Length!=needed.Length || !zones.Select(row=>row.TIMEZONE).Order(StringComparer.Ordinal).SequenceEqual(needed,StringComparer.Ordinal) ||
            ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(zones.Where(row=>originalZones.Contains(row.TIMEZONE,StringComparer.Ordinal)).Select(MaintenanceIdentityRevision).Order(StringComparer.Ordinal)))!=reviewed.Operating.TimeZoneReferencesRevision)
            throw new InvalidOperationException("Original native time-zone reference proof differs from its complete reviewed population.");
        var utcReference=zones.Single(row=>row.TIMEZONE=="UTC");
        static DateTime? Utc(DateTime? value)=>value?.Kind==DateTimeKind.Local?throw new InvalidOperationException("UTC native reference cannot use machine-local dates."):value is {} at?DateTime.SpecifyKind(at,DateTimeKind.Utc):null;
        if(utcReference.ACTIVE_IND!="Y" || Utc(utcReference.EFFECTIVE_DATE)>activatedAtUtc || Utc(utcReference.EXPIRY_DATE)<=activatedAtUtc)throw new InvalidOperationException("Retained native UTC reference does not cover activation.");
        var beforeFacilities=facilities.OrderBy(row=>row.FACILITY_ID,StringComparer.Ordinal).ThenBy(row=>row.FACILITY_TYPE,StringComparer.Ordinal).ThenBy(row=>row.STATUS_ID,StringComparer.Ordinal).ToArray();
        var beforeEquipment=equipment.OrderBy(row=>row.EQUIPMENT_ID,StringComparer.Ordinal).ThenBy(row=>row.EQUIP_STATUS_ID,StringComparer.Ordinal).ToArray();
        var afterFacilities=beforeFacilities.Select(CompanyInventorySourceReview.Copy).ToList();var afterEquipment=beforeEquipment.Select(CompanyInventorySourceReview.Copy).ToList();
        foreach(var transition in transitions)
        {
            if(transition.AssetType=="FACILITY")Change(afterFacilities,row=>row.FACILITY_ID==transition.AssetId && row.FACILITY_TYPE==transition.FacilityType && row.STATUS_ID==transition.OriginalStatusId,"STATUS_ID","STATUS",transition);
            else Change(afterEquipment,row=>row.EQUIPMENT_ID==transition.AssetId && row.EQUIP_STATUS_ID==transition.OriginalStatusId,"EQUIP_STATUS_ID","EQUIP_STATUS",transition);
        }
        var result=new QualifiedAssetReturnRows(beforeFacilities,beforeEquipment,
            afterFacilities.OrderBy(row=>row.FACILITY_ID,StringComparer.Ordinal).ThenBy(row=>row.FACILITY_TYPE,StringComparer.Ordinal).ThenBy(row=>row.STATUS_ID,StringComparer.Ordinal).ToArray(),
            afterEquipment.OrderBy(row=>row.EQUIPMENT_ID,StringComparer.Ordinal).ThenBy(row=>row.EQUIP_STATUS_ID,StringComparer.Ordinal).ToArray(),utcReference,utc,zones.OrderBy(row=>row.TIMEZONE,StringComparer.Ordinal).ToArray());
        if(JsonSerializer.Serialize(result).Length>2*1024*1024)throw new InvalidOperationException("Complete qualified return rows exceed retained proof capacity.");return result;

        void Change<T>(List<T> rows,Func<T,bool> match,string idColumn,string valueColumn,CompanyMaintenanceQualifiedReturnTransition transition) where T:class
        {
            var before=rows.Where(match).ToArray();if(before.Length!=1 || MaintenanceIdentityRevision(before[0])!=transition.OriginalRevision)
                throw new InvalidOperationException("Exact native qualified status preimage differs from reviewed evidence.");
            var id=transition.StatusId(returnId);
            if(rows.Any(row=>typeof(T).GetProperty(idColumn)!.GetValue(row) as string==id))throw new InvalidOperationException("A qualified successor key already exists; recover the original complete return.");
            var expired=CompanyInventorySourceReview.Copy(before[0]);var active=CompanyInventorySourceReview.Copy(before[0]);
            object? Get(T row,string name)=>typeof(T).GetProperty(name)!.GetValue(row);
            void Set(T row,string name,object? value)=>typeof(T).GetProperty(name)!.SetValue(row,value);
            DateTime Wire(string column)
            {
                var original=(DateTime?)(Get(before[0],column)??Get(before[0],"EFFECTIVE_DATE"));if(original?.Kind==DateTimeKind.Utc)return activatedAtUtc;
                if(original?.Kind==DateTimeKind.Local)throw new InvalidOperationException("Machine-local status dates cannot establish a qualified return.");
                var zone=Get(before[0],"TIMEZONE") as string;zone=string.IsNullOrWhiteSpace(zone)?reviewed.DateTimeZoneId:zone;
                var timeZone=TimeZoneInfo.FindSystemTimeZoneById(zone!);var wire=DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(activatedAtUtc,timeZone),DateTimeKind.Unspecified);
                if(timeZone.IsInvalidTime(wire) || timeZone.IsAmbiguousTime(wire))throw new InvalidOperationException("The native return boundary is ambiguous in its recorded time zone.");return wire;
            }
            Set(expired,"ACTIVE_IND","N");Set(expired,"EXPIRY_DATE",Wire("EXPIRY_DATE"));Set(expired,"END_TIME",Wire("END_TIME"));Set(expired,"ROW_CHANGED_BY",reviewed.Context.ActorUserId);Set(expired,"ROW_CHANGED_DATE",utc);
            Set(active,idColumn,id);Set(active,valueColumn,transition.TargetValue);Set(active,"SOURCE",transition.NativeSource);Set(active,"ACTIVE_IND","Y");Set(active,"TIMEZONE","UTC");
            foreach(var column in new[]{"EFFECTIVE_DATE","START_TIME"})Set(active,column,activatedAtUtc);
            foreach(var column in new[]{"EXPIRY_DATE","END_TIME","ROW_EXPIRY_DATE","ROW_QUALITY"})Set(active,column,null);
            Set(active,"PERCENT_CAPABILITY",transition.ObservedCapabilityPercent);Set(active,"REMARK",reason);
            Set(active,"PPDM_GUID",Guid.ParseExact(ProcessDefinitionRevision.FromJson(returnId+"|"+id)[..32],"N").ToString("D").ToUpperInvariant());
            foreach(var column in new[]{"ROW_CREATED_BY","ROW_CHANGED_BY"})Set(active,column,reviewed.Context.ActorUserId);
            foreach(var column in new[]{"ROW_CREATED_DATE","ROW_CHANGED_DATE","ROW_EFFECTIVE_DATE"})Set(active,column,utc);
            rows[rows.IndexOf(before[0])]=expired;rows.Add(active);
        }
    }
}
