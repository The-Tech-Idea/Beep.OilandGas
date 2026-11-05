using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.Models.Data.WorkOrder;

public sealed record CompanyMaintenanceLicenseUnitReference(string UnitId,string? Name,string? QuantityType,string? MeasurementSystemId,string? UsageType,string ActiveIndicator,DateTime? EffectiveAtUtc,DateTime? ExpiresAtUtc,string? Source,string? SourceDocumentId,bool TemporalEvidence,string NativeRevision)
{
    public bool Covers(DateTime utc)=>ActiveIndicator=="Y" && TemporalEvidence && (EffectiveAtUtc is null || EffectiveAtUtc<=utc) && (ExpiresAtUtc is null || ExpiresAtUtc>utc) && (EffectiveAtUtc is null || ExpiresAtUtc is null || ExpiresAtUtc>EffectiveAtUtc);
}
public sealed record CompanyMaintenanceLicenseRestriction(string RestrictionId,decimal Version,string? Name,string? Description,string? RegulatoryAct,string? ClassId,string? Type,string? DurationType,string ActiveIndicator,DateTime? EffectiveAtUtc,DateTime? ExpiresAtUtc,DateTime? StartedAtUtc,DateTime? EndedAtUtc,DateTime? StartEventAtUtc,DateTime? EndEventAtUtc,string? Source,string? SourceDocumentId,bool TemporalEvidence,string NativeRevision)
{
    public bool Covers(DateTime utc)=>ActiveIndicator=="Y" && TemporalEvidence && EffectiveAtUtc is {} from && from<=utc && (ExpiresAtUtc is null || ExpiresAtUtc>utc && ExpiresAtUtc>from) && (StartedAtUtc is null || StartedAtUtc<=utc) && (EndedAtUtc is null || EndedAtUtc>utc && (StartedAtUtc is null || EndedAtUtc>StartedAtUtc));
}
public sealed record CompanyMaintenanceLicenseConditionTerms(decimal? Value,string? ValueUnitId,decimal? DueTerm,string? DueTermUnitId,string? DueFrequency,string? ProductId,string? RestrictionId,decimal? RestrictionVersion,CompanyMaintenanceLicenseUnitReference? ValueUnit,CompanyMaintenanceLicenseUnitReference? DueTermUnit,CompanyMaintenanceLicenseRestriction? Restriction);

public sealed partial record CompanyMaintenanceLicenseCondition
{
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]
    public string? FulfilledByBaId{get;init;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]
    public CompanyMaintenanceLicenseConditionTerms? Terms{get;init;}
}
public sealed partial record CompanyMaintenanceQualifiedLicensing
{
    private static int ConditionTermsPopulation(IReadOnlyList<CompanyMaintenanceLicenseCondition> conditions)=>conditions.Sum(row=>(row.Terms?.ValueUnit is null?0:1)+(row.Terms?.DueTermUnit is null?0:1)+(row.Terms?.Restriction is null?0:1));
    private static string[] ConditionTermsBlockers(IReadOnlyList<CompanyMaintenanceLicenseCondition> conditions,IReadOnlyList<CompanyMaintenanceLicenseReference> references,int population,DateTime utc)
    {
        if(population+ConditionTermsPopulation(conditions)>2048)throw new InvalidOperationException("Complete bounded condition term/unit/restriction source populations are required.");
        static bool OptionalId(string? value)=>value is null || Id(value);
        static bool Text(params string?[] values)=>values.All(value=>value?.Length is not >16384);
        bool Reference(string kind,string? key)=>Id(key) && references.Any(row=>row.Kind==kind && row.Key==key && row.Qualifier is null && row.Covers(utc));
        void Unit(CompanyMaintenanceLicenseUnitReference? unit,string? key)
        {
            if(unit is not null && (unit.UnitId!=key || !Id(unit.UnitId) || !Text(unit.Name) || !OptionalId(unit.QuantityType) || !OptionalId(unit.MeasurementSystemId) || !OptionalId(unit.UsageType) || unit.SourceDocumentId?.Length>256 || unit.ActiveIndicator is not ("Y" or "N") || !Utc(unit.EffectiveAtUtc,unit.ExpiresAtUtc) || !ProcessDefinitionRevision.IsValid(unit.NativeRevision)))throw new InvalidOperationException("Native condition units must retain the exact recorded unit key and complete source/period provenance.");
        }
        var findings=new List<string>();
        foreach(var condition in conditions)
        {
            // Earlier evidence has no terms projection and remains subject to its existing policy blockers.
            if(condition.Terms is not {} terms)continue;
            if(!OptionalId(terms.ValueUnitId) || !OptionalId(terms.DueTermUnitId) || !OptionalId(terms.ProductId) || !OptionalId(terms.RestrictionId) || !Text(terms.DueFrequency))throw new InvalidOperationException("Recorded condition term/reference identifiers are malformed.");
            Unit(terms.ValueUnit,terms.ValueUnitId);Unit(terms.DueTermUnit,terms.DueTermUnitId);
            if(terms.Restriction is {} restriction && (restriction.RestrictionId!=terms.RestrictionId || restriction.Version!=terms.RestrictionVersion || !Id(restriction.RestrictionId) || !Text(restriction.Name,restriction.Description,restriction.RegulatoryAct) || !OptionalId(restriction.ClassId) || !OptionalId(restriction.Type) || !OptionalId(restriction.DurationType) || restriction.SourceDocumentId?.Length>256 || restriction.ActiveIndicator is not ("Y" or "N") || !Utc(restriction.EffectiveAtUtc,restriction.ExpiresAtUtc,restriction.StartedAtUtc,restriction.EndedAtUtc,restriction.StartEventAtUtc,restriction.EndEventAtUtc) || !ProcessDefinitionRevision.IsValid(restriction.NativeRevision)))throw new InvalidOperationException("Native condition restrictions must retain both exact recorded keys and complete source text/periods.");
            if(condition.ActiveIndicator!="Y")continue;
            var label=$"Licence {condition.LicenseId}, condition {condition.ConditionId}";
            bool ProvenUnit(CompanyMaintenanceLicenseUnitReference? unit)=>unit is not null && unit.Covers(utc) && Id(unit.Source) && Reference("PPDM_QUANTITY",unit.QuantityType) && Reference("PPDM_MEASUREMENT_SYSTEM",unit.MeasurementSystemId) && Reference("R_PPDM_UOM_USAGE",unit.UsageType);
            if((terms.Value is not null || terms.ValueUnitId is not null) && (terms.Value is null || !ProvenUnit(terms.ValueUnit)))findings.Add($"{label}: recorded numeric value/unit/quantity/system/usage provenance is unconfirmed.");
            if((terms.DueTerm is not null || terms.DueTermUnitId is not null) && (terms.DueTerm is null || !ProvenUnit(terms.DueTermUnit)))findings.Add($"{label}: recorded due term/unit/quantity/system/usage provenance is unconfirmed.");
            if(terms.ProductId is not null && !Reference("SUBSTANCE",terms.ProductId))findings.Add($"{label}: recorded product/substance reference is unconfirmed.");
            if(terms.RestrictionId is not null || terms.RestrictionVersion is not null)
            {
                if(terms.Restriction is not {} current || !current.Covers(utc) || !Id(current.Source) || !Reference("REST_CLASS",current.ClassId) || !Reference("R_REST_TYPE",current.Type) || !Reference("R_REST_DURATION",current.DurationType) || current.StartEventAtUtc is not null || current.EndEventAtUtc is not null)findings.Add($"{label}: exact restriction version/date/source/type/duration applicability is unresolved; event boundaries require explicit policy.");
            }
        }
        return findings.ToArray();
    }
}
