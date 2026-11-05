using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.Models.Data.WorkOrder;

public sealed record CompanyMaintenanceFacilityLicense(string FacilityId,string FacilityType,string LicenseId,string? LicenseNumber,string? LicenseType,string? IssuerBaId,string? HolderBaId,string? AssignedFieldId,string ActiveIndicator,DateTime? GrantedAtUtc,DateTime? EffectiveAtUtc,DateTime? ExpiresAtUtc,string? ViolationIndicator,string? FeesPaidIndicator,string? Source,bool TemporalEvidence,string NativeRevision)
{
    public bool Covers(DateTime utc)=>ActiveIndicator=="Y" && TemporalEvidence && GrantedAtUtc is {} granted && granted<=utc && EffectiveAtUtc is {} from && from<=utc && (ExpiresAtUtc is null || ExpiresAtUtc>utc && ExpiresAtUtc>from);
}
public sealed record CompanyMaintenanceLicenseStatus(string FacilityId,string FacilityType,string LicenseId,string StatusId,string StatusType,string Value,string ActiveIndicator,DateTime? EffectiveAtUtc,DateTime? ExpiresAtUtc,string? Source,bool TemporalEvidence,string NativeRevision)
{public bool Covers(DateTime utc)=>ActiveIndicator=="Y" && TemporalEvidence && EffectiveAtUtc is {} from && from<=utc && (ExpiresAtUtc is null || ExpiresAtUtc>utc && ExpiresAtUtc>from);}
public sealed partial record CompanyMaintenanceLicenseCondition(string FacilityId,string FacilityType,string LicenseId,string ConditionId,string? ConditionType,string? ConditionCode,string? Description,string? DueCondition,string? FulfilledIndicator,string? ExemptIndicator,DateTime? DueAtUtc,DateTime? FulfilledAtUtc,DateTime? EffectiveAtUtc,DateTime? ExpiresAtUtc,string ActiveIndicator,string? Source,bool TemporalEvidence,string NativeRevision);
public sealed record CompanyMaintenanceLicenseViolation(string FacilityId,string FacilityType,string LicenseId,string ViolationId,string? ConditionId,string? ViolationType,string? ResolutionType,DateTime? OccurredAtUtc,DateTime? ResolvedAtUtc,DateTime? EffectiveAtUtc,DateTime? ExpiresAtUtc,string ActiveIndicator,string? Source,bool TemporalEvidence,string NativeRevision);
public sealed record CompanyMaintenanceLicenseReference(string Kind,string Key,string? Qualifier,string ActiveIndicator,DateTime? EffectiveAtUtc,DateTime? ExpiresAtUtc,bool TemporalEvidence,string NativeRevision)
{public bool Covers(DateTime utc)=>ActiveIndicator=="Y" && TemporalEvidence && (EffectiveAtUtc is null || EffectiveAtUtc<=utc) && (ExpiresAtUtc is null || ExpiresAtUtc>utc) && (EffectiveAtUtc is null || ExpiresAtUtc is null || ExpiresAtUtc>EffectiveAtUtc);}

/// <summary>Recorded qualified licence sources and applicability findings, without legal/operating approval inference.</summary>
public sealed partial record CompanyMaintenanceQualifiedLicensing(FieldSelectionReview Context,string IdentityRevision,IReadOnlyList<CompanyMaintenanceFacilityLicense> Licenses,IReadOnlyList<CompanyMaintenanceLicenseStatus> Statuses,IReadOnlyList<CompanyMaintenanceLicenseCondition> Conditions,IReadOnlyList<CompanyMaintenanceLicenseViolation> Violations,IReadOnlyList<CompanyMaintenanceLicenseReference> References,DateTime ComparedAtUtc,IReadOnlyList<string> Blockers)
{
    private static bool Id(string? value)=>LeaseSummaryValidator.Identifier(value) && value!.Length<=128;
    private static bool Utc(params DateTime?[] values)=>values.All(value=>value is null || value.Value.Kind==DateTimeKind.Utc);
    public static string[] GetBlockers(CompanyMaintenanceQualifiedAssets identity,IReadOnlyList<CompanyMaintenanceFacilityLicense> licenses,IReadOnlyList<CompanyMaintenanceLicenseStatus> statuses,IReadOnlyList<CompanyMaintenanceLicenseCondition> conditions,IReadOnlyList<CompanyMaintenanceLicenseViolation> violations,IReadOnlyList<CompanyMaintenanceLicenseReference> references,DateTime utc,IReadOnlyList<CompanyMaintenanceLicenseArea>? areas=null)
    {
        if(licenses is null || statuses is null || conditions is null || violations is null || references is null || licenses.Count+statuses.Count+conditions.Count+violations.Count+references.Count>2048 || licenses.Any(row=>row is null) || statuses.Any(row=>row is null) || conditions.Any(row=>row is null) || violations.Any(row=>row is null) || references.Any(row=>row is null) || utc==default || utc.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("Complete bounded qualified licensing populations and comparison UTC are required.");
        var facilities=identity.Facilities.Select(row=>(row.FacilityId,row.FacilityType)).ToHashSet();var keys=licenses.Select(row=>(row.FacilityId,row.FacilityType,row.LicenseId)).ToHashSet();
        if(keys.Count!=licenses.Count || licenses.Any(row=>!facilities.Contains((row.FacilityId,row.FacilityType)) || !Id(row.LicenseId) || row.ActiveIndicator is not ("Y" or "N") || !Utc(row.GrantedAtUtc,row.EffectiveAtUtc,row.ExpiresAtUtc) || !ProcessDefinitionRevision.IsValid(row.NativeRevision)))throw new InvalidOperationException("Every licence must retain its exact authorized qualified facility and native key/period.");
        if(statuses.Any(row=>!keys.Contains((row.FacilityId,row.FacilityType,row.LicenseId)) || !Id(row.StatusId) || !Id(row.StatusType) || !Id(row.Value) || row.ActiveIndicator is not ("Y" or "N") || !Utc(row.EffectiveAtUtc,row.ExpiresAtUtc) || !ProcessDefinitionRevision.IsValid(row.NativeRevision)) || statuses.Select(row=>(row.FacilityId,row.FacilityType,row.LicenseId,row.StatusId)).Distinct().Count()!=statuses.Count)throw new InvalidOperationException("Complete unique licence status membership and native periods are required.");
        if(conditions.Any(row=>!keys.Contains((row.FacilityId,row.FacilityType,row.LicenseId)) || !Id(row.ConditionId) || row.FulfilledByBaId is not null && !Id(row.FulfilledByBaId) || row.Description?.Length>16384 || row.ActiveIndicator is not ("Y" or "N") || !Utc(row.DueAtUtc,row.FulfilledAtUtc,row.EffectiveAtUtc,row.ExpiresAtUtc) || !ProcessDefinitionRevision.IsValid(row.NativeRevision)) || conditions.Select(row=>(row.FacilityId,row.FacilityType,row.LicenseId,row.ConditionId)).Distinct().Count()!=conditions.Count)throw new InvalidOperationException("Complete unique licence condition membership and source text/periods are required.");
        if(violations.Any(row=>!keys.Contains((row.FacilityId,row.FacilityType,row.LicenseId)) || !Id(row.ViolationId) || row.ActiveIndicator is not ("Y" or "N") || !Utc(row.OccurredAtUtc,row.ResolvedAtUtc,row.EffectiveAtUtc,row.ExpiresAtUtc) || !ProcessDefinitionRevision.IsValid(row.NativeRevision)) || violations.Select(row=>(row.FacilityId,row.FacilityType,row.LicenseId,row.ViolationId)).Distinct().Count()!=violations.Count)throw new InvalidOperationException("Complete unique licence violation membership and native dates are required.");
        if(references.Any(row=>!Id(row.Kind) || !Id(row.Key) || row.Qualifier is not null && !Id(row.Qualifier) || row.ActiveIndicator is not ("Y" or "N") || !Utc(row.EffectiveAtUtc,row.ExpiresAtUtc) || !ProcessDefinitionRevision.IsValid(row.NativeRevision)) || references.Select(row=>(row.Kind,row.Key,row.Qualifier)).Distinct().Count()!=references.Count)throw new InvalidOperationException("Complete unique licensing reference identities and native provenance are required.");
        bool Reference(string kind,string? key,string? qualifier=null)=>Id(key) && references.Any(row=>row.Kind==kind && row.Key==key && row.Qualifier==qualifier && row.Covers(utc));
        var result=new List<string>();foreach(var facility in identity.Facilities)if(!licenses.Any(row=>row.FacilityId==facility.FacilityId && row.FacilityType==facility.FacilityType && row.Covers(utc)))result.Add($"Facility {facility.FacilityId} / {facility.FacilityType}: no effective dated native licence candidate is established.");
        foreach(var license in licenses.Where(row=>row.ActiveIndicator=="Y"))
        {
            var label=$"Licence {license.FacilityId} / {license.FacilityType} / {license.LicenseId}";
            if(!license.Covers(utc))result.Add($"{label}: grant/effective/expiry dates are missing, future, expired or unresolved.");
            if(license.HolderBaId!=identity.Context.Company.BusinessAssociateId || license.AssignedFieldId!=identity.Context.FieldId)result.Add($"{label}: current company holder and original FIELD applicability require reconciliation.");
            if(!Reference("BUSINESS_ASSOCIATE",license.IssuerBaId) || !Reference("BUSINESS_ASSOCIATE",license.HolderBaId) || !Reference("FACILITY_LIC_TYPE",license.IssuerBaId,license.LicenseType))result.Add($"{label}: effective issuer/holder identity and issuer-specific licence-type references are required.");
            if(!Id(license.Source))result.Add($"{label}: native source provenance is unconfirmed.");
            if(license.ViolationIndicator is not "N")result.Add($"{label}: recorded violation indicator requires explicit review; no absence is assumed.");
            if(license.FeesPaidIndicator is not "Y")result.Add($"{label}: recorded fee indicator requires explicit applicability review.");
            var sourceStatuses=statuses.Where(row=>row.FacilityId==license.FacilityId && row.FacilityType==license.FacilityType && row.LicenseId==license.LicenseId).ToArray();
            if(sourceStatuses.Length==0)result.Add($"{label}: no native licence-status history is established.");
            foreach(var group in sourceStatuses.GroupBy(row=>row.StatusType,StringComparer.Ordinal))
            {
                if(group.Any(row=>row.ActiveIndicator=="Y" && (!row.TemporalEvidence || row.EffectiveAtUtc is null || row.EffectiveAtUtc>utc || row.ExpiresAtUtc<=row.EffectiveAtUtc)))result.Add($"{label} / {group.Key}: missing, future or inconsistent native status dates require reconciliation.");
                var current=group.Where(row=>row.Covers(utc)).ToArray();if(current.Length!=1){result.Add($"{label} / {group.Key}: one unambiguous dated current status assignment is required.");continue;}
                if(!Reference("R_LIC_STATUS_TYPE",group.Key) || !Reference("R_LICENSE_STATUS",group.Key,current[0].Value))result.Add($"{label} / {group.Key}: effective native status/type references are unconfirmed.");
            }
        }
        foreach(var row in conditions.Where(row=>row.ActiveIndicator=="Y"))
        {
            var label=$"Licence {row.LicenseId}, condition {row.ConditionId}";
            if(!row.TemporalEvidence || row.EffectiveAtUtc is null || row.EffectiveAtUtc>utc || row.ExpiresAtUtc<=utc || row.ExpiresAtUtc<=row.EffectiveAtUtc || !Reference("R_FAC_LIC_COND",row.ConditionType) || !Reference("R_FAC_LIC_COND_CODE",row.ConditionType,row.ConditionCode) || !Reference("R_FAC_LIC_DUE_CONDITION",row.DueCondition))result.Add($"{label}: native condition/date/reference applicability is unresolved.");
            result.Add($"{label}: recorded fulfilment/exemption, due dates, numeric terms/units and restrictions require explicit applicability policy.");
        }
        foreach(var row in violations.Where(row=>row.ActiveIndicator=="Y"))
        {
            if(!row.TemporalEvidence || !Reference("R_FAC_LIC_VIOLATION_TYPE",row.ViolationType) || row.ResolutionType is not null && !Reference("R_FAC_LIC_VIOL_RESOL",row.ResolutionType))result.Add($"Licence {row.LicenseId}, violation {row.ViolationId}: native date/reference evidence is unresolved.");
            result.Add($"Licence {row.LicenseId}, violation {row.ViolationId}: recorded violation/resolution requires explicit applicability policy; a date alone is not clearance.");
        }
        var population=licenses.Count+statuses.Count+conditions.Count+violations.Count+references.Count;
        result.AddRange(ConditionTermsBlockers(conditions,references,population+(areas?.Count??0)+(areas?.Count(row=>row?.Area is not null)??0),utc));
        result.AddRange(AreaBlockers(licenses,references,areas,population+ConditionTermsPopulation(conditions),utc));
        result.Add("Recorded licence/type/status codes and flags do not establish legal or operating approval. Explicit company applicability/transition policy and independent return authority remain required.");return result.ToArray();
    }
    public void Require(CompanyMaintenanceQualifiedAssets identity)
    {if(Context!=identity.Context || IdentityRevision!=identity.IdentityRevision() || ComparedAtUtc<identity.ComparedAtUtc || Blockers is null || !GetBlockers(identity,Licenses,Statuses,Conditions,Violations,References,ComparedAtUtc,Areas).SequenceEqual(Blockers,StringComparer.Ordinal))throw new InvalidOperationException("Qualified licensing evidence differs from the original native identities and reviewed source population.");}
    public string SourceRevision()=>WithAreaRevision(new{Context,IdentityRevision,Licenses=Licenses.OrderBy(row=>row.FacilityId,StringComparer.Ordinal).ThenBy(row=>row.FacilityType,StringComparer.Ordinal).ThenBy(row=>row.LicenseId,StringComparer.Ordinal),Statuses=Statuses.OrderBy(row=>row.FacilityId,StringComparer.Ordinal).ThenBy(row=>row.FacilityType,StringComparer.Ordinal).ThenBy(row=>row.LicenseId,StringComparer.Ordinal).ThenBy(row=>row.StatusId,StringComparer.Ordinal),Conditions=Conditions.OrderBy(row=>row.FacilityId,StringComparer.Ordinal).ThenBy(row=>row.FacilityType,StringComparer.Ordinal).ThenBy(row=>row.LicenseId,StringComparer.Ordinal).ThenBy(row=>row.ConditionId,StringComparer.Ordinal),Violations=Violations.OrderBy(row=>row.FacilityId,StringComparer.Ordinal).ThenBy(row=>row.FacilityType,StringComparer.Ordinal).ThenBy(row=>row.LicenseId,StringComparer.Ordinal).ThenBy(row=>row.ViolationId,StringComparer.Ordinal),References=References.OrderBy(row=>row.Kind,StringComparer.Ordinal).ThenBy(row=>row.Key,StringComparer.Ordinal).ThenBy(row=>row.Qualifier,StringComparer.Ordinal),Blockers});
}
