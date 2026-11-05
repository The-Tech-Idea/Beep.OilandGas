using System.Text.Json;
using System.Text.Json.Serialization;
namespace TheTechIdea.Data.OilGas;

public sealed record CompanyMaintenanceLicenseAreaReference(string AreaId,string AreaType,string? PreferredName,string ActiveIndicator,DateTime? EffectiveAtUtc,DateTime? ExpiresAtUtc,string? Source,string? SourceDocumentId,bool TemporalEvidence,string NativeRevision)
{
    public bool Covers(DateTime utc)=>ActiveIndicator=="Y" && TemporalEvidence && EffectiveAtUtc is {} from && from<=utc && (ExpiresAtUtc is null || ExpiresAtUtc>utc && ExpiresAtUtc>from);
}
public sealed record CompanyMaintenanceLicenseArea(string FacilityId,string FacilityType,string LicenseId,string AreaId,string AreaType,string? Description,string ActiveIndicator,DateTime? EffectiveAtUtc,DateTime? ExpiresAtUtc,string? Source,bool TemporalEvidence,string NativeRevision,CompanyMaintenanceLicenseAreaReference? Area)
{
    public bool Covers(DateTime utc)=>ActiveIndicator=="Y" && TemporalEvidence && EffectiveAtUtc is {} from && from<=utc && (ExpiresAtUtc is null || ExpiresAtUtc>utc && ExpiresAtUtc>from);
}
public sealed partial record CompanyMaintenanceQualifiedLicensing
{
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<CompanyMaintenanceLicenseArea>? Areas{get;init;}

    private static string[] AreaBlockers(IReadOnlyList<CompanyMaintenanceFacilityLicense> licenses,IReadOnlyList<CompanyMaintenanceLicenseReference> references,IReadOnlyList<CompanyMaintenanceLicenseArea>? areas,int population,DateTime utc)
    {
        // Older evidence remains a read-only policy-blocked review with the original body/digest.
        if(areas is null)return [];
        if(population+areas.Count+areas.Count(row=>row?.Area is not null)>2048 || areas.Any(row=>row is null))throw new InvalidOperationException("Complete bounded licence-area populations are required; no assignments are truncated.");
        var licenseKeys=licenses.Select(row=>(row.FacilityId,row.FacilityType,row.LicenseId)).ToHashSet();
        if(areas.Select(row=>(row.FacilityId,row.FacilityType,row.LicenseId,row.AreaId,row.AreaType)).Distinct().Count()!=areas.Count || areas.Any(row=>!licenseKeys.Contains((row.FacilityId,row.FacilityType,row.LicenseId)) || !Id(row.AreaId) || !Id(row.AreaType) || row.Description?.Length>16384 || row.ActiveIndicator is not ("Y" or "N") || !Utc(row.EffectiveAtUtc,row.ExpiresAtUtc) || !ProcessDefinitionRevision.IsValid(row.NativeRevision)))throw new InvalidOperationException("Every native licence-area assignment requires its exact qualified licence and complete unique area keys/periods.");
        foreach(var row in areas)
        {
            if(row.Area is {} area && (area.AreaId!=row.AreaId || area.AreaType!=row.AreaType || area.PreferredName?.Length>16384 || area.SourceDocumentId?.Length>256 || area.ActiveIndicator is not ("Y" or "N") || !Utc(area.EffectiveAtUtc,area.ExpiresAtUtc) || !ProcessDefinitionRevision.IsValid(area.NativeRevision)))throw new InvalidOperationException("Referenced native areas must retain both original area keys and source/period provenance.");
        }
        var findings=new List<string>();
        foreach(var license in licenses.Where(row=>row.ActiveIndicator=="Y"))
        {
            var assigned=areas.Where(row=>row.FacilityId==license.FacilityId && row.FacilityType==license.FacilityType && row.LicenseId==license.LicenseId).ToArray();
            if(!assigned.Any(row=>row.Covers(utc)))findings.Add($"Licence {license.FacilityId} / {license.FacilityType} / {license.LicenseId}: no effective dated native licence-area assignment is established; explicit jurisdiction applicability is required.");
            foreach(var row in assigned.Where(row=>row.ActiveIndicator=="Y"))
            {
                if(!row.Covers(utc) || row.Area is not {} area || !area.Covers(utc) || !Id(row.Source) || !Id(area.Source) || !references.Any(reference=>reference.Kind=="R_AREA_TYPE" && reference.Key==row.AreaType && reference.Qualifier is null && reference.Covers(utc)))findings.Add($"Licence {row.LicenseId}, area {row.AreaId} / {row.AreaType}: assignment/area dates, sources or area-type reference are unconfirmed.");
            }
        }
        return findings.ToArray();
    }
    private string WithAreaRevision(object original)=>Areas is null?ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(original)):ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Licensing=original,Areas=Areas.OrderBy(row=>row.FacilityId,StringComparer.Ordinal).ThenBy(row=>row.FacilityType,StringComparer.Ordinal).ThenBy(row=>row.LicenseId,StringComparer.Ordinal).ThenBy(row=>row.AreaId,StringComparer.Ordinal).ThenBy(row=>row.AreaType,StringComparer.Ordinal)}));
}
