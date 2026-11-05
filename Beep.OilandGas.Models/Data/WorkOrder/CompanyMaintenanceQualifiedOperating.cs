using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.Models.Data.WorkOrder;

public sealed record CompanyMaintenanceQualifiedStatus(string AssetType,string AssetId,string? FacilityType,string StatusId,string StatusType,string Value,string Source,string ActiveIndicator,DateTime? EffectiveAtUtc,DateTime? StartedAtUtc,DateTime? ExpiresAtUtc,DateTime? EndedAtUtc,string? NativeTimeZone,bool TemporalEvidence,string NativeRevision)
{
    public bool Covers(DateTime utc)=>ActiveIndicator=="Y" && TemporalEvidence && EffectiveAtUtc is {} from && from<=utc && (StartedAtUtc is null || StartedAtUtc<=utc) && (ExpiresAtUtc is null || ExpiresAtUtc>utc) && (EndedAtUtc is null || EndedAtUtc>utc);
}
public sealed record CompanyMaintenanceQualifiedStatusReference(string AssetType,string StatusType,string Value,string ActiveIndicator,DateTime? EffectiveAtUtc,DateTime? ExpiresAtUtc,bool TemporalEvidence,bool TypeReferenceCurrent,string NativeRevision,string TypeReferenceRevision)
{
    public bool Covers(DateTime utc)=>ActiveIndicator=="Y" && TemporalEvidence && TypeReferenceCurrent && (EffectiveAtUtc is null || EffectiveAtUtc<=utc) && (ExpiresAtUtc is null || ExpiresAtUtc>utc) && (EffectiveAtUtc is null || ExpiresAtUtc is null || ExpiresAtUtc>EffectiveAtUtc);
}
/// <summary>Recorded native status codes and date/reference evidence; no inferred operational meaning or transition authority.</summary>
public sealed record CompanyMaintenanceQualifiedOperating(FieldSelectionReview Context,string IdentityRevision,IReadOnlyList<CompanyMaintenanceQualifiedStatus> Statuses,IReadOnlyList<CompanyMaintenanceQualifiedStatusReference> References,string TimeZoneReferencesRevision,DateTime ComparedAtUtc,IReadOnlyList<string> Blockers)
{
    public static string[] GetBlockers(CompanyMaintenanceQualifiedAssets identity,IReadOnlyList<CompanyMaintenanceQualifiedStatus> statuses,IReadOnlyList<CompanyMaintenanceQualifiedStatusReference> references,DateTime utc,bool includePolicyNotice=true)
    {
        if(statuses is null || references is null || statuses.Count>1024 || references.Count>1024 || statuses.Any(row=>row is null) || references.Any(row=>row is null) || utc==default || utc.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("Bounded complete qualified status/reference populations and comparison UTC are required.");
        var targets=identity.Facilities.Select(row=>(Type:"FACILITY",Id:row.FacilityId,Qualifier:(string?)row.FacilityType)).Concat(identity.Equipment.Select(row=>(Type:"EQUIPMENT",Id:row.EquipmentId,Qualifier:(string?)null))).ToArray();
        foreach(var row in statuses)
        {
            if(row is null || !targets.Contains((row.AssetType,row.AssetId,row.FacilityType)) || new[]{row.StatusId,row.StatusType,row.Value,row.Source}.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128) || row.ActiveIndicator is not ("Y" or "N") || !ProcessDefinitionRevision.IsValid(row.NativeRevision) || new[]{row.EffectiveAtUtc,row.StartedAtUtc,row.ExpiresAtUtc,row.EndedAtUtc}.Any(value=>value is {} date && date.Kind!=DateTimeKind.Utc))throw new InvalidOperationException("Exact qualified status source identity, complete native keys and UTC projections are required.");
        }
        if(statuses.Select(row=>(row.AssetType,row.AssetId,row.FacilityType,row.StatusId)).Distinct().Count()!=statuses.Count || references.Select(row=>(row.AssetType,row.StatusType,row.Value)).Distinct().Count()!=references.Count)throw new InvalidOperationException("Qualified native status/reference keys are ambiguous.");
        foreach(var row in references)if(row is null || row.AssetType is not ("FACILITY" or "EQUIPMENT") || new[]{row.StatusType,row.Value}.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128) || row.ActiveIndicator is not ("Y" or "N") || !ProcessDefinitionRevision.IsValid(row.NativeRevision) || !ProcessDefinitionRevision.IsValid(row.TypeReferenceRevision) || row.EffectiveAtUtc is {} from && from.Kind!=DateTimeKind.Utc || row.ExpiresAtUtc is {} to && to.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("Recorded native status and type reference provenance are required.");
        var result=new List<string>();
        foreach(var target in targets)
        {
            var population=statuses.Where(row=>row.AssetType==target.Type && row.AssetId==target.Id && row.FacilityType==target.Qualifier).ToArray();var label=$"{target.Type} {target.Id} {target.Qualifier}";
            if(population.Length==0){result.Add($"{label}: no recorded native status population is available.");continue;}
            foreach(var group in population.GroupBy(row=>row.StatusType,StringComparer.Ordinal))
            {
                var active=group.Where(row=>row.ActiveIndicator=="Y").ToArray();
                if(active.Any(row=>!row.TemporalEvidence || row.EffectiveAtUtc is null || row.ExpiresAtUtc<=row.EffectiveAtUtc || row.EndedAtUtc<=row.EffectiveAtUtc || row.EndedAtUtc<=row.StartedAtUtc))result.Add($"{label} / {group.Key}: native status dates or time-zone interpretation are unresolved.");
                if(active.Any(row=>row.TemporalEvidence && (row.EffectiveAtUtc>utc || row.StartedAtUtc>utc)))result.Add($"{label} / {group.Key}: a future status assignment requires reconciliation.");
                var current=active.Where(row=>row.Covers(utc)).ToArray();if(current.Length!=1){result.Add($"{label} / {group.Key}: one unambiguous current native assignment is required.");continue;}
                if(!references.Any(row=>row.AssetType==target.Type && row.StatusType==group.Key && row.Value==current[0].Value && row.Covers(utc)))result.Add($"{label} / {group.Key}: the current value has no effective native status/type reference.");
            }
        }
        if(includePolicyNotice)result.Add("Native status codes have no assumed operational meaning. Explicit recorded transition policy, licensing and independent return authority remain required.");return result.ToArray();
    }
    public void Require(CompanyMaintenanceQualifiedAssets identity)
    {
        if(Context!=identity.Context || IdentityRevision!=identity.IdentityRevision() || !ProcessDefinitionRevision.IsValid(TimeZoneReferencesRevision) || ComparedAtUtc<identity.ComparedAtUtc || Blockers is null || !GetBlockers(identity,Statuses,References,ComparedAtUtc).SequenceEqual(Blockers,StringComparer.Ordinal))throw new InvalidOperationException("Current qualified operating evidence must match the exact reviewed native identities and source comparison.");
    }
    public string SourceRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context,IdentityRevision,Statuses=Statuses.OrderBy(row=>row.AssetType,StringComparer.Ordinal).ThenBy(row=>row.AssetId,StringComparer.Ordinal).ThenBy(row=>row.FacilityType,StringComparer.Ordinal).ThenBy(row=>row.StatusId,StringComparer.Ordinal),References=References.OrderBy(row=>row.AssetType,StringComparer.Ordinal).ThenBy(row=>row.StatusType,StringComparer.Ordinal).ThenBy(row=>row.Value,StringComparer.Ordinal),TimeZoneReferencesRevision,Blockers}));
}
