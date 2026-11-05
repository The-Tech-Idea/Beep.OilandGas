using System.Data;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using NativeWell=Beep.OilandGas.PPDM39.Models.WELL;
namespace Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;

public partial class WellServices : ICompanyMaintenanceWellEvidenceOwner
{
    private static string MaintenanceWellRowRevision(object row)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(row.GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(property=>property.CanRead && property.CanWrite).OrderBy(property=>property.Name,StringComparer.Ordinal).Select(property=>new object?[]{property.Name,property.GetValue(row)})));
    private PPDMGenericRepository MaintenanceWellProofRepository<T>() where T:class=>new(
        _editor,_wellRepository.CommonColumnHandler,_defaults,_metadata,typeof(T),_connectionName,typeof(T).Name,null,
        typeof(T)==typeof(NativeWell)?_companyWellAccess:null,null,
        typeof(T).GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly)
            .Where(property=>property.CanRead && property.CanWrite).Select(property=>property.Name).ToArray());
    private async Task RequireMaintenanceWellDefinition(Type type,string[] keys,params (string Column,string Table,string Key)[] relationships)
    {
        var definition=await _metadata.GetTableMetadataAsync(type.Name);
        if(definition?.EntityTypeName!=type.Name || !keys.Order(StringComparer.Ordinal).SequenceEqual((definition.PrimaryKeyColumn??"").Split(',').Select(value=>value.Trim()).Order(StringComparer.Ordinal)) || relationships.Any(expected=>definition.ForeignKeys.Count(key=>key.ForeignKeyColumn==expected.Column && key.ReferencedTable==expected.Table && key.ReferencedPrimaryKey==expected.Key)!=1))throw new InvalidOperationException("Canonical native well-return compound keys and parent/reference metadata are required.");
    }
    private static DateTime? MaintenanceWellUtc(DateTime? value,string? zone,ref bool confirmed)
    {
        if(value is null)return null;if(value.Value.Kind==DateTimeKind.Utc)return value;if(value.Value.Kind==DateTimeKind.Local || string.IsNullOrWhiteSpace(zone)){confirmed=false;return null;}
        try{var timezone=TimeZoneInfo.FindSystemTimeZoneById(zone);if(timezone.IsInvalidTime(value.Value) || timezone.IsAmbiguousTime(value.Value)){confirmed=false;return null;}return TimeZoneInfo.ConvertTimeToUtc(value.Value,timezone);}
        catch(Exception error) when(error is TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException){confirmed=false;return null;}
    }
    private async Task<CompanyMaintenanceWellEvidence> ReadMaintenanceWellEvidenceCore(FieldSelectionReview context,string wellId,Func<Task> authority,bool owned)
    {
        FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);
        if(!LeaseSummaryValidator.Identifier(wellId) || wellId.Length>128 || context.Database.ConnectionName!=_connectionName || _companyWellAccess is null)throw new UnauthorizedAccessException("Original company/FIELD/CORE well and reviewed parent row policy are required before native reads.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original native well source is unavailable.");
        async Task Guard(){if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority();if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original native well evidence binding changed.");}
        async Task<T[]> Rows<T>(List<AppFilter> filters) where T:class
        {
            await Guard();var raw=(await MaintenanceWellProofRepository<T>().GetAsync(filters)).ToArray();
            if(raw.Length>256 || raw.Any(row=>row is not T || filters.Any(filter=>typeof(T).GetProperty(filter.FieldName)?.GetValue(row)?.ToString()!=filter.FilterValue)))throw new InvalidOperationException("Complete bounded native well evidence must honor every exact source predicate.");
            var result=raw.Cast<T>().Select(CompanyInventorySourceReview.Copy).ToArray();await Guard();return result;
        }
        async Task<CompanyMaintenanceWellEvidence> Observe()
        {
            await Guard();await RequireMaintenanceWellDefinition(typeof(NativeWell),["UWI"],("ASSIGNED_FIELD","FIELD","FIELD_ID"),("OPERATOR","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID"));
            await RequireMaintenanceWellDefinition(typeof(WELL_STATUS),["UWI","SOURCE","STATUS_ID"],("UWI","WELL","UWI"),("STATUS_TYPE","R_WELL_STATUS","STATUS_TYPE"),("TIMEZONE","R_TIMEZONE","TIMEZONE"));await RequireMaintenanceWellDefinition(typeof(R_WELL_STATUS),["STATUS_TYPE","STATUS"]);
            var wells=await Rows<NativeWell>([new(){FieldName="UWI",Operator="=",FilterValue=wellId},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}]);
            if(wells.Length!=1 || wells[0].ASSIGNED_FIELD!=context.FieldId || wells[0].OPERATOR!=context.Company.BusinessAssociateId)throw new UnauthorizedAccessException("The original well is not uniquely visible in this company and FIELD.");var well=wells[0];var wellRevision=MaintenanceWellRowRevision(well);var access=await _companyWellAccess.GetEffectiveAccessLevelAsync(typeof(NativeWell),"WELL",well)??throw new UnauthorizedAccessException("Current company well authority is unavailable.");
            var wellTime=true;var wellFrom=MaintenanceWellUtc(well.EFFECTIVE_DATE,_wellEvidenceTimeZoneId,ref wellTime);var wellTo=MaintenanceWellUtc(well.EXPIRY_DATE,_wellEvidenceTimeZoneId,ref wellTime);var wellNow=DateTime.UtcNow;if(!wellTime || wellFrom is null || wellFrom>wellNow || wellTo<=wellNow || wellTo<=wellFrom)throw new InvalidOperationException("Original native well effective period is not established in the recorded evidence time zone.");
            var statuses=new List<WELL_STATUS>();var references=new List<R_WELL_STATUS>();foreach(var facet in CompanyMaintenanceWellEvidence.RequiredFacets)
            {
                statuses.AddRange(await Rows<WELL_STATUS>([new(){FieldName="UWI",Operator="=",FilterValue=wellId},new(){FieldName="STATUS_TYPE",Operator="=",FilterValue=facet}]));references.AddRange(await Rows<R_WELL_STATUS>([new(){FieldName="STATUS_TYPE",Operator="=",FilterValue=facet}]));
            }
            var temporalZones=new HashSet<string>(StringComparer.Ordinal);var zoneRows=new List<R_TIMEZONE>();foreach(var zone in statuses.Select(row=>row.TIMEZONE).Where(value=>!string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.Ordinal))
            {
                await RequireMaintenanceWellDefinition(typeof(R_TIMEZONE),["TIMEZONE"]);var found=await Rows<R_TIMEZONE>([new(){FieldName="TIMEZONE",Operator="=",FilterValue=zone}]);zoneRows.AddRange(found);if(found.Length>1)throw new InvalidOperationException("Native time-zone reference identity is ambiguous.");if(found.Length==1 && found[0].ACTIVE_IND=="Y"){var confirmed=true;var from=MaintenanceWellUtc(found[0].EFFECTIVE_DATE,zone,ref confirmed);var to=MaintenanceWellUtc(found[0].EXPIRY_DATE,zone,ref confirmed);var now=DateTime.UtcNow;if(confirmed && (from is null || from<=now) && (to is null || to>now) && (from is null || to is null || to>from))temporalZones.Add(zone);}
            }
            var observations=statuses.Select(row=>
            {
                var confirmed=true;var zone=string.IsNullOrWhiteSpace(row.TIMEZONE)?_wellEvidenceTimeZoneId:temporalZones.Contains(row.TIMEZONE)?row.TIMEZONE:null;
                if(!string.IsNullOrWhiteSpace(row.TIMEZONE) && zone is null)confirmed=false;var effective=MaintenanceWellUtc(row.EFFECTIVE_DATE,zone,ref confirmed);var expiry=MaintenanceWellUtc(row.EXPIRY_DATE,zone,ref confirmed);var end=MaintenanceWellUtc(row.END_TIME,zone,ref confirmed);
                return new CompanyWellReturnFacet(row.UWI,row.SOURCE,row.STATUS_ID,row.STATUS_TYPE,row.STATUS,row.STATUS_QUALIFIER,row.STATUS_QUALIFIER_VALUE,row.ACTIVE_IND,effective,expiry,end,row.TIMEZONE,confirmed,MaintenanceWellRowRevision(row));
            }).OrderBy(row=>row.Source,StringComparer.Ordinal).ThenBy(row=>row.StatusId,StringComparer.Ordinal).ToArray();
            var values=references.Select(row=>{var confirmed=true;var effective=MaintenanceWellUtc(row.EFFECTIVE_DATE,_wellEvidenceTimeZoneId,ref confirmed);var expiry=MaintenanceWellUtc(row.EXPIRY_DATE,_wellEvidenceTimeZoneId,ref confirmed);return new CompanyWellReturnReference(row.STATUS_TYPE,row.STATUS,row.ACTIVE_IND,effective,expiry,confirmed,MaintenanceWellRowRevision(row));}).OrderBy(row=>row.Facet,StringComparer.Ordinal).ThenBy(row=>row.Value,StringComparer.Ordinal).ToArray();
            var closingWell=await Rows<NativeWell>([new(){FieldName="UWI",Operator="=",FilterValue=wellId},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}]);if(closingWell.Length!=1 || MaintenanceWellRowRevision(closingWell[0])!=wellRevision || await _companyWellAccess.GetEffectiveAccessLevelAsync(typeof(NativeWell),"WELL",closingWell[0])!=access)throw new InvalidOperationException("Original native well/parent rights changed around facet reads.");
            var utc=DateTime.UtcNow;var timeZoneRevision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(zoneRows.Select(MaintenanceWellRowRevision).Order(StringComparer.Ordinal)));var result=new CompanyMaintenanceWellEvidence(context,wellId,well.ASSIGNED_FIELD,well.OPERATOR,access,wellRevision,_wellEvidenceTimeZoneId,timeZoneRevision,observations,values,utc,CompanyMaintenanceWellEvidence.GetBlockers(context,wellId,well.ASSIGNED_FIELD,well.OPERATOR,access,wellRevision,_wellEvidenceTimeZoneId,timeZoneRevision,observations,values,utc));result.Require();await Guard();return result;
        }
        var first=await Observe();var closing=await Observe();if(first.SourceRevision()!=closing.SourceRevision())throw new InvalidOperationException("Original complete well facets/references/company/rights/date interpretation changed during return review.");await Guard();return closing;
    }
    public Task<CompanyMaintenanceWellEvidence> ReadMaintenanceWellEvidenceAsync(FieldSelectionReview context,string wellId,Func<Task> authority)=>ReadMaintenanceWellEvidenceCore(context,wellId,authority,false);
}
