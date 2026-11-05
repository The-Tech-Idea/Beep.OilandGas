using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.Models.Data.WorkOrder;

public sealed record CompanyMaintenanceReturnPolicyApplication(string FacilityId,string FacilityType,IReadOnlyList<string> PolicyRecordIds);
public sealed record CompanyMaintenanceReturnNativeSource(string Source,string ActiveIndicator,DateTime? EffectiveAtUtc,DateTime? ExpiresAtUtc,bool TemporalEvidence,string NativeRevision)
{
    public bool Covers(DateTime utc)=>ActiveIndicator=="Y" && TemporalEvidence && (EffectiveAtUtc is null || EffectiveAtUtc<=utc) && (ExpiresAtUtc is null || ExpiresAtUtc>utc) && (EffectiveAtUtc is null || ExpiresAtUtc is null || ExpiresAtUtc>EffectiveAtUtc);
}

/// <summary>Current source-to-recorded-policy comparison. It grants no asset WRITE or return authority.</summary>
public sealed record CompanyMaintenanceQualifiedReturnPolicy(string SourcesRevision,CompanyMaintenanceReturnPolicyCatalog Catalog,IReadOnlyList<string> ActorReturnRoles,IReadOnlyList<CompanyMaintenanceReturnPolicyApplication> Applications,IReadOnlyList<CompanyMaintenanceReturnNativeSource> NativeSources,DateTime ComparedAtUtc,IReadOnlyList<string> Blockers)
{
    public static CompanyMaintenanceReturnPolicyContext ContextFor(CompanyMaintenanceQualifiedAssets identity,IReadOnlyList<string> roles)=>new(identity.Context.Company,identity.Context.Database,identity.Context.ActorUserId,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context=identity.Context,ReturnRoles=roles.Order(StringComparer.Ordinal)})));
    private static bool InWindow(CompanyMaintenanceReturnPolicyVersion version,DateTime utc)=>version.Policy.EffectiveFromUtc<=utc && utc<version.Policy.EffectiveToUtc;
    private static bool Period(DateTime? from,DateTime? to,DateTime utc)=>from is {} start && start<=utc && (to is null || to>utc && to>start);
    private static bool Id(string? value)=>LeaseSummaryValidator.Identifier(value);
    public static CompanyMaintenanceReturnPolicyCatalog ScopeCatalog(CompanyMaintenanceQualifiedAssets identity,WorkOrderDetailModel order,CompanyMaintenanceReturnPolicyCatalog catalog)
    {
        catalog.Require();var areas=(identity.Licensing?.Areas??[]).Select(row=>(row.FacilityType,row.AreaId,row.AreaType)).ToHashSet();
        var versions=catalog.Versions.Where(row=>row.Policy.WorkOrderType==order.WoSubType && areas.Contains((row.Policy.FacilityType,row.Policy.AreaId,row.Policy.AreaType))).ToArray();return CompanyMaintenanceReturnPolicyCatalog.Create(catalog.Context,versions,catalog.ComparedAtUtc);
    }
    private static CompanyMaintenanceReturnPolicyApplication[] Select(CompanyMaintenanceQualifiedAssets identity,WorkOrderDetailModel order,CompanyMaintenanceReturnPolicyCatalog catalog,DateTime utc)
    {
        var licensing=identity.Licensing??throw new InvalidOperationException("Complete native licensing evidence is required before policy comparison.");
        return identity.Facilities.OrderBy(row=>row.FacilityId,StringComparer.Ordinal).ThenBy(row=>row.FacilityType,StringComparer.Ordinal).Select(facility=>
        {
            var current=licensing.Licenses.Where(row=>row.FacilityId==facility.FacilityId && row.FacilityType==facility.FacilityType && row.Covers(utc)).Select(row=>row.LicenseId).ToHashSet(StringComparer.Ordinal);
            var areas=(licensing.Areas??[]).Where(row=>row.FacilityId==facility.FacilityId && row.FacilityType==facility.FacilityType && current.Contains(row.LicenseId) && row.Covers(utc)).Select(row=>(row.AreaId,row.AreaType)).ToHashSet();
            var policies=catalog.Versions.Where(row=>InWindow(row,utc) && row.Policy.WorkOrderType==order.WoSubType && row.Policy.FacilityType==facility.FacilityType && areas.Contains((row.Policy.AreaId,row.Policy.AreaType))).Select(row=>row.RecordId).Order(StringComparer.Ordinal).ToArray();return new CompanyMaintenanceReturnPolicyApplication(facility.FacilityId,facility.FacilityType,policies);
        }).ToArray();
    }
    public static string[] RequiredNativeSources(CompanyMaintenanceQualifiedAssets identity,WorkOrderDetailModel order,CompanyMaintenanceReturnPolicyCatalog catalog,DateTime utc)
    {
        var licensing=identity.Licensing??throw new InvalidOperationException("Complete native licensing source evidence is required.");var current=licensing.Licenses.Where(row=>row.Covers(utc)).Select(row=>(row.FacilityId,row.FacilityType,row.LicenseId)).ToHashSet();
        var applications=Select(identity,order,catalog,utc);var applicable=applications.SelectMany(row=>row.PolicyRecordIds).ToHashSet(StringComparer.Ordinal);
        var keys=new List<string?>();keys.AddRange(identity.Operating!.Statuses.Where(row=>row.Covers(utc)).Select(row=>(string?)row.Source));keys.AddRange(identity.Equipment.Select(row=>(string?)row.Installation.Source));keys.AddRange(licensing.Licenses.Where(row=>current.Contains((row.FacilityId,row.FacilityType,row.LicenseId))).Select(row=>row.Source));keys.AddRange(licensing.Statuses.Where(row=>current.Contains((row.FacilityId,row.FacilityType,row.LicenseId)) && row.Covers(utc)).Select(row=>row.Source));
        foreach(var condition in licensing.Conditions.Where(row=>current.Contains((row.FacilityId,row.FacilityType,row.LicenseId)) && row.ActiveIndicator=="Y" && Period(row.EffectiveAtUtc,row.ExpiresAtUtc,utc))){keys.Add(condition.Source);keys.Add(condition.Terms?.ValueUnit?.Source);keys.Add(condition.Terms?.DueTermUnit?.Source);keys.Add(condition.Terms?.Restriction?.Source);}
        keys.AddRange(licensing.Violations.Where(row=>current.Contains((row.FacilityId,row.FacilityType,row.LicenseId)) && row.ActiveIndicator=="Y").Select(row=>row.Source));foreach(var area in (licensing.Areas??[]).Where(row=>current.Contains((row.FacilityId,row.FacilityType,row.LicenseId)) && row.Covers(utc))){keys.Add(area.Source);keys.Add(area.Area?.Source);}
        keys.AddRange(catalog.Versions.Where(row=>applicable.Contains(row.RecordId)).SelectMany(version=>version.Policy.Transitions.Where(transition=>transition.Disposition!="PRESERVE" &&
            applications.Any(application=>application.PolicyRecordIds.Contains(version.RecordId,StringComparer.Ordinal) &&
                (transition.AssetType=="FACILITY"?CompanyMaintenanceQualifiedAssets.IsOriginalFacilityTarget(order,application.FacilityId,application.FacilityType):
                    identity.Equipment.Any(equipment=>equipment.Installation.FacilityId==application.FacilityId && equipment.Installation.FacilityType==application.FacilityType))))
            .Select(transition=>(string?)transition.NativeSource)));return keys.Where(Id).Select(row=>row!).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    }
    public static CompanyMaintenanceQualifiedReturnPolicy Create(CompanyMaintenanceQualifiedAssets identity,WorkOrderDetailModel order,CompanyMaintenanceReturnPolicyCatalog catalog,IReadOnlyList<string> roles,IReadOnlyList<CompanyMaintenanceReturnNativeSource> nativeSources,DateTime utc)
    {
        ArgumentNullException.ThrowIfNull(roles);ArgumentNullException.ThrowIfNull(nativeSources);var baseline=identity with{ReturnPolicy=null};baseline.Require(order);catalog=ScopeCatalog(baseline,order,catalog);var applications=Select(baseline,order,catalog,utc);var result=new CompanyMaintenanceQualifiedReturnPolicy(baseline.SourcesRevision(),catalog,roles.ToArray(),applications,nativeSources.ToArray(),utc,Findings(baseline,order,catalog,roles,applications,nativeSources,utc));result.Require(baseline,order);return result;
    }
    public void Require(CompanyMaintenanceQualifiedAssets identity,WorkOrderDetailModel order)
    {
        var baseline=identity with{ReturnPolicy=null};baseline.Require(order);Catalog.Require();var requiredSources=RequiredNativeSources(baseline,order,Catalog,ComparedAtUtc).ToHashSet(StringComparer.Ordinal);
        if(ActorReturnRoles is null || ActorReturnRoles.Count>1024 || ActorReturnRoles.Any(role=>!Id(role)) || ActorReturnRoles.Distinct(StringComparer.Ordinal).Count()!=ActorReturnRoles.Count || Catalog.Context!=ContextFor(baseline,ActorReturnRoles) || Catalog.Revision!=ScopeCatalog(baseline,order,Catalog).Revision || SourcesRevision!=baseline.SourcesRevision() || ComparedAtUtc.Kind!=DateTimeKind.Utc || ComparedAtUtc<baseline.LatestComparisonAtUtc() || ComparedAtUtc<Catalog.ComparedAtUtc || Applications is null || Applications.Count!=baseline.Facilities.Count || JsonSerializer.Serialize(Applications)!=JsonSerializer.Serialize(Select(baseline,order,Catalog,ComparedAtUtc)) || NativeSources is null || NativeSources.Count>1024 || NativeSources.Any(row=>row is null || !Id(row.Source) || row.ActiveIndicator is not ("Y" or "N") || !ProcessDefinitionRevision.IsValid(row.NativeRevision) || row.EffectiveAtUtc is {} from && from.Kind!=DateTimeKind.Utc || row.ExpiresAtUtc is {} to && to.Kind!=DateTimeKind.Utc) || NativeSources.Select(row=>row.Source).Distinct(StringComparer.Ordinal).Count()!=NativeSources.Count || NativeSources.Any(row=>!requiredSources.Contains(row.Source)) || Blockers is null || !Blockers.SequenceEqual(Findings(baseline,order,Catalog,ActorReturnRoles,Applications,NativeSources,ComparedAtUtc),StringComparer.Ordinal))throw new InvalidOperationException("Recorded policy comparison must retain the complete original native sources, company/actor/return roles, catalog, membership and comparison UTC.");
    }
    public string SourceRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{SourcesRevision,Catalog=Catalog.Revision,ActorReturnRoles=ActorReturnRoles.Order(StringComparer.Ordinal),Applications,NativeSources=NativeSources.OrderBy(row=>row.Source,StringComparer.Ordinal),Blockers}));

    private static string[] Findings(CompanyMaintenanceQualifiedAssets identity,WorkOrderDetailModel order,CompanyMaintenanceReturnPolicyCatalog catalog,IReadOnlyList<string> roles,IReadOnlyList<CompanyMaintenanceReturnPolicyApplication> applications,IReadOnlyList<CompanyMaintenanceReturnNativeSource> nativeSources,DateTime utc)
    {
        var operating=identity.Operating??throw new InvalidOperationException("Complete native operating evidence is required before policy comparison.");var licensing=identity.Licensing??throw new InvalidOperationException("Complete native licensing evidence is required before policy comparison.");
        var result=new List<string>();void Issue(string value)=>result.Add(value);
        bool Reference(string kind,string? key,string? qualifier=null)=>Id(key) && licensing.References.Any(row=>row.Kind==kind && row.Key==key && row.Qualifier==qualifier && row.Covers(utc));
        bool Unit(CompanyMaintenanceLicenseUnitReference? row)=>row is not null && row.Covers(utc) && Id(row.Source) && Reference("PPDM_QUANTITY",row.QuantityType) && Reference("PPDM_MEASUREMENT_SYSTEM",row.MeasurementSystemId) && Reference("R_PPDM_UOM_USAGE",row.UsageType);
        result.AddRange(CompanyMaintenanceQualifiedOperating.GetBlockers(identity,operating.Statuses,operating.References,utc,false));
        foreach(var key in RequiredNativeSources(identity,order,catalog,utc))if(!nativeSources.Any(row=>row.Source==key && row.Covers(utc)))Issue($"Native source {key}: one effective R_SOURCE reference is required for current evidence and proposed status writes.");
        if(!Id(order.WoSubType))Issue("Exact current native work-order type is unconfirmed; no return policy is inferred.");
        if(licensing.Areas is null)Issue("Complete native licence-area evidence is unreviewed; jurisdiction applicability is unconfirmed.");
        foreach(var application in applications)
        {
            var label=$"Facility {application.FacilityId} / {application.FacilityType}";
            var selected=catalog.Versions.Where(row=>application.PolicyRecordIds.Contains(row.RecordId,StringComparer.Ordinal)).OrderBy(row=>row.RecordId,StringComparer.Ordinal).ToArray();
            var current=licensing.Licenses.Where(row=>row.FacilityId==application.FacilityId && row.FacilityType==application.FacilityType && row.Covers(utc)).ToArray();
            foreach(var row in licensing.Licenses.Where(row=>row.FacilityId==application.FacilityId && row.FacilityType==application.FacilityType && row.ActiveIndicator=="Y"))if(!row.TemporalEvidence || row.GrantedAtUtc is null || row.EffectiveAtUtc is null || row.ExpiresAtUtc<=row.EffectiveAtUtc)Issue($"{label}, licence {row.LicenseId}: source grant/effective/expiry evidence is unresolved.");
            if(current.Length==0)Issue($"{label}: no effective dated native licence candidate is established.");
            var currentIds=current.Select(row=>row.LicenseId).ToHashSet(StringComparer.Ordinal);var areas=(licensing.Areas??[]).Where(row=>row.FacilityId==application.FacilityId && row.FacilityType==application.FacilityType && currentIds.Contains(row.LicenseId)).ToArray();
            foreach(var area in areas.Where(row=>row.ActiveIndicator=="Y"))
            {
                if(!area.TemporalEvidence || area.EffectiveAtUtc is null || area.ExpiresAtUtc<=area.EffectiveAtUtc)Issue($"{label}, area {area.AreaId} / {area.AreaType}: native assignment dates are unresolved.");
                if(!area.Covers(utc))continue;
                if(area.Area is null || !area.Area.Covers(utc) || !Id(area.Source) || !Id(area.Area.Source) || !Reference("R_AREA_TYPE",area.AreaType))Issue($"{label}, area {area.AreaId} / {area.AreaType}: current native area/source/type provenance is unconfirmed.");
                var decisions=selected.Where(row=>row.Policy.AreaId==area.AreaId && row.Policy.AreaType==area.AreaType).ToArray();if(decisions.Length==0)Issue($"{label}, area {area.AreaId} / {area.AreaType}: no effective recorded jurisdiction applicability policy is established.");
                if(decisions.Select(row=>row.Policy.AreaApplicability=="NOT_APPLICABLE").Distinct().Count()>1)Issue($"{label}, area {area.AreaId} / {area.AreaType}: recorded area applicability decisions conflict.");
            }
            foreach(var version in selected)if(!version.Policy.AuthorizedReturnRoles.Any(role=>roles.Contains(role,StringComparer.Ordinal)))Issue($"{label}, policy {version.RecordId}: current configured standard return-role assignment is required.");
            var applicable=selected.Where(row=>row.Policy.AreaApplicability!="NOT_APPLICABLE").ToArray();if(applicable.Length==0)Issue($"{label}: no effective applicable company return policy is established; area names and status codes select no default policy.");
            foreach(var version in applicable)
            {
                var policy=version.Policy;
                foreach(var required in policy.Licenses.Where(row=>row.Applicability=="REQUIRED"))if(!current.Any(row=>row.IssuerBaId==required.IssuerBaId && row.LicenseType==required.LicenseType))Issue($"{label}, policy {version.RecordId}: required issuer/type {required.IssuerBaId} / {required.LicenseType} has no current licence.");
                foreach(var license in current)
                {
                    var licenceLabel=$"{label}, licence {license.LicenseId}, policy {version.RecordId}";var rule=policy.Licenses.SingleOrDefault(row=>row.IssuerBaId==license.IssuerBaId && row.LicenseType==license.LicenseType);
                    if(rule is null){Issue($"{licenceLabel}: licence issuer/type has no explicit applicability decision.");continue;}
                    if(license.HolderBaId!=identity.Context.Company.BusinessAssociateId || license.AssignedFieldId!=identity.Context.FieldId || !Id(license.Source) || !Reference("BUSINESS_ASSOCIATE",license.IssuerBaId) || !Reference("BUSINESS_ASSOCIATE",license.HolderBaId) || !Reference("FACILITY_LIC_TYPE",license.IssuerBaId,license.LicenseType))Issue($"{licenceLabel}: current company holder/FIELD/source/issuer/type provenance is unresolved.");
                    if(license.FeesPaidIndicator is not ("Y" or "N") || license.ViolationIndicator is not ("Y" or "N"))Issue($"{licenceLabel}: native fee/violation flags are unconfirmed.");
                    if(!areas.Any(row=>row.LicenseId==license.LicenseId && row.AreaId==policy.AreaId && row.AreaType==policy.AreaType && row.Covers(utc) && row.Area?.Covers(utc)==true))Issue($"{licenceLabel}: exact recorded jurisdiction area applicability is unconfirmed.");
                    var statuses=licensing.Statuses.Where(row=>row.FacilityId==license.FacilityId && row.FacilityType==license.FacilityType && row.LicenseId==license.LicenseId).ToArray();
                    if(statuses.Length==0)Issue($"{licenceLabel}: native licence-status population is absent.");
                    foreach(var group in statuses.GroupBy(row=>row.StatusType,StringComparer.Ordinal))
                    {
                        var active=group.Where(row=>row.ActiveIndicator=="Y").ToArray();if(active.Any(row=>!row.TemporalEvidence || row.EffectiveAtUtc is null || row.EffectiveAtUtc>utc || row.ExpiresAtUtc<=row.EffectiveAtUtc))Issue($"{licenceLabel}, status {group.Key}: native date evidence is unresolved or future.");var assignments=active.Where(row=>row.Covers(utc)).ToArray();
                        if(assignments.Length!=1){Issue($"{licenceLabel}, status {group.Key}: one unambiguous current native assignment is required.");continue;}
                        if(!Reference("R_LIC_STATUS_TYPE",group.Key) || !Reference("R_LICENSE_STATUS",group.Key,assignments[0].Value))Issue($"{licenceLabel}, status {group.Key}: current native status/type references are unconfirmed.");
                        if(rule.Applicability=="REQUIRED" && !rule.Statuses.Any(row=>row.StatusType==group.Key && row.AcceptedValues.Contains(assignments[0].Value,StringComparer.Ordinal)))Issue($"{licenceLabel}, status {group.Key}: current value is not explicitly accepted by the recorded policy.");
                    }
                    if(rule.Applicability=="NOT_APPLICABLE")continue;
                    foreach(var required in rule.Statuses)if(!statuses.Any(row=>row.StatusType==required.StatusType && row.Covers(utc)))Issue($"{licenceLabel}: required status type {required.StatusType} has no current assignment.");
                    if(rule.FeesDecision=="REQUIRED_PAID" && license.FeesPaidIndicator!="Y")Issue($"{licenceLabel}: the recorded policy requires confirmed paid fees.");
                    var conditions=licensing.Conditions.Where(row=>row.FacilityId==license.FacilityId && row.FacilityType==license.FacilityType && row.LicenseId==license.LicenseId && row.ActiveIndicator=="Y").ToArray();
                    foreach(var required in rule.Conditions)if(!conditions.Any(row=>row.ConditionType==required.ConditionType && row.ConditionCode==required.ConditionCode && row.TemporalEvidence && Period(row.EffectiveAtUtc,row.ExpiresAtUtc,utc)))Issue($"{licenceLabel}: required condition {required.ConditionType} / {required.ConditionCode} is not current.");
                    foreach(var condition in conditions)
                    {
                        var conditionLabel=$"{licenceLabel}, condition {condition.ConditionId}";
                        if(!condition.TemporalEvidence || condition.EffectiveAtUtc is null || condition.ExpiresAtUtc<=condition.EffectiveAtUtc){Issue($"{conditionLabel}: native condition date interpretation is unresolved.");continue;}
                        if(!Period(condition.EffectiveAtUtc,condition.ExpiresAtUtc,utc))continue;
                        var decision=rule.Conditions.SingleOrDefault(row=>row.ConditionType==condition.ConditionType && row.ConditionCode==condition.ConditionCode);if(decision is null){Issue($"{conditionLabel}: no explicit current condition applicability decision is recorded.");continue;}
                        if(!Id(condition.Source) || !Reference("R_FAC_LIC_COND",condition.ConditionType) || !Reference("R_FAC_LIC_COND_CODE",condition.ConditionType,condition.ConditionCode) || !Reference("R_FAC_LIC_DUE_CONDITION",condition.DueCondition) || decision.DueCondition!=condition.DueCondition || condition.FulfilledIndicator is not ("Y" or "N") || condition.ExemptIndicator is not ("Y" or "N"))Issue($"{conditionLabel}: native condition/source/flag/due-code provenance differs from the reviewed policy.");
                        var terms=condition.Terms;if(terms is null){Issue($"{conditionLabel}: numeric/unit/restriction source evidence is unreviewed.");continue;}
                        if(terms.DueFrequency!=decision.DueFrequency || terms.ProductId!=decision.ProductId || terms.ProductId is not null && !Reference("SUBSTANCE",terms.ProductId))Issue($"{conditionLabel}: exact recorded frequency/product applicability differs.");
                        void Term(CompanyMaintenanceReturnTermRule expected,decimal? value,string? unitId,CompanyMaintenanceLicenseUnitReference? unit,string name)
                        {
                            if((value is not null || unitId is not null) && (value is null || unitId is null || !Unit(unit)))Issue($"{conditionLabel}: {name} value/unit provenance is unconfirmed.");
                            if(expected.Decision=="EXACT" && (value is null || value!=expected.Value || unitId!=expected.UnitId || !Unit(unit)))Issue($"{conditionLabel}: {name} does not match the exact configured value/unit; null is never zero and units are not converted.");
                        }
                        Term(decision.Value,terms.Value,terms.ValueUnitId,terms.ValueUnit,"numeric term");Term(decision.DueTerm,terms.DueTerm,terms.DueTermUnitId,terms.DueTermUnit,"due term");
                        if(decision.RestrictionDecision=="NO_RECORDED_RESTRICTION")
                        {if(terms.RestrictionId is not null || terms.RestrictionVersion is not null || terms.Restriction is not null)Issue($"{conditionLabel}: a recorded restriction is not covered by the configured no-restriction requirement.");}
                        else if(terms.RestrictionId!=decision.RestrictionId || terms.RestrictionVersion!=decision.RestrictionVersion || terms.Restriction is not {} restriction || !restriction.Covers(utc) || restriction.SourceDocumentId!=decision.RestrictionSourceDocumentId || !Id(restriction.Source) || !Reference("REST_CLASS",restriction.ClassId) || !Reference("R_REST_TYPE",restriction.Type) || !Reference("R_REST_DURATION",restriction.DurationType))Issue($"{conditionLabel}: exact restriction version/source/document/period/type/duration applicability is unresolved.");
                        if(terms.Restriction is {} eventRestriction && (eventRestriction.StartEventAtUtc is not null || eventRestriction.EndEventAtUtc is not null))Issue($"{conditionLabel}: recorded restriction event semantics require additional source-backed review; narrative text is not executable clearance.");
                        var fulfilled=condition.FulfilledIndicator=="Y" && condition.FulfilledAtUtc is {} fulfilledAt && fulfilledAt<=utc && (condition.DueAtUtc is null || fulfilledAt<=condition.DueAtUtc) && Id(condition.FulfilledByBaId) && Reference("BUSINESS_ASSOCIATE",condition.FulfilledByBaId) && decision.AcceptedFulfilledByBaIds is {} accepted && accepted.Contains(condition.FulfilledByBaId!,StringComparer.Ordinal);
                        var exempt=condition.ExemptIndicator=="Y";
                        if(decision.Decision=="FULFILLED_ONLY" && !fulfilled || decision.Decision=="EXEMPT_ONLY" && !exempt || decision.Decision=="FULFILLED_OR_EXEMPT" && !fulfilled && !exempt)Issue($"{conditionLabel}: recorded fulfilment/date/due/accepted business-associate or explicit exemption evidence does not satisfy the policy.");
                    }
                    var violations=licensing.Violations.Where(row=>row.FacilityId==license.FacilityId && row.FacilityType==license.FacilityType && row.LicenseId==license.LicenseId && row.ActiveIndicator=="Y").ToArray();if(license.ViolationIndicator=="Y" && violations.Length==0)Issue($"{licenceLabel}: the recorded violation flag has no complete resolution population.");
                    foreach(var violation in violations)
                    {
                        var accepted=rule.Violations.SingleOrDefault(row=>row.ViolationType==violation.ViolationType);
                        if(!violation.TemporalEvidence || !Id(violation.Source) || violation.OccurredAtUtc is not {} occurred || occurred>utc || violation.ResolvedAtUtc is not {} resolved || resolved<occurred || resolved>utc || accepted is null || !accepted.AcceptedResolutionTypes.Contains(violation.ResolutionType!,StringComparer.Ordinal) || !Reference("R_FAC_LIC_VIOLATION_TYPE",violation.ViolationType) || !Reference("R_FAC_LIC_VIOL_RESOL",violation.ResolutionType) || violation.ConditionId is not null && !licensing.Conditions.Any(row=>row.FacilityId==license.FacilityId && row.FacilityType==license.FacilityType && row.LicenseId==license.LicenseId && row.ConditionId==violation.ConditionId))Issue($"{licenceLabel}, violation {violation.ViolationId}: complete dated source/condition/resolution evidence and an explicit accepted resolution are required.");
                    }
                }
            }
            var assets=identity.Equipment.Where(row=>row.Installation.FacilityId==application.FacilityId && row.Installation.FacilityType==application.FacilityType).Select(row=>(Type:"EQUIPMENT",Id:row.EquipmentId,Qualifier:(string?)null,Affected:true)).Prepend((Type:"FACILITY",Id:application.FacilityId,Qualifier:(string?)application.FacilityType,Affected:CompanyMaintenanceQualifiedAssets.IsOriginalFacilityTarget(order,application.FacilityId,application.FacilityType)));
            foreach(var asset in assets)
            {
                var groups=operating.Statuses.Where(row=>row.AssetType==asset.Type && row.AssetId==asset.Id && row.FacilityType==asset.Qualifier).GroupBy(row=>row.StatusType,StringComparer.Ordinal).ToArray();
                foreach(var group in groups)
                {
                    var decisions=applicable.Select(version=>(Version:version,Rule:version.Policy.Transitions.SingleOrDefault(row=>row.AssetType==asset.Type && row.StatusType==group.Key))).ToArray();
                    if(decisions.Any(row=>row.Rule is null))Issue($"{asset.Type} {asset.Id} / {group.Key}: every applicable policy must explicitly transition or preserve this recorded facet.");
                    var configured=decisions.Where(row=>row.Rule is not null).Select(row=>row.Rule!).ToArray();if(asset.Affected && configured.Select(row=>(Preserve:row.Disposition=="PRESERVE",row.TargetValue,row.NativeSource)).Distinct().Count()>1)Issue($"{asset.Type} {asset.Id} / {group.Key}: recorded policies disagree on preservation or the exact target/source.");
                    var currentStatuses=group.Where(row=>row.Covers(utc)).ToArray();if(currentStatuses.Length!=1)continue;
                    foreach(var decision in configured)
                    {
                        if(!decision.AcceptedSourceValues.Contains(currentStatuses[0].Value,StringComparer.Ordinal))Issue($"{asset.Type} {asset.Id} / {group.Key}: current source code is not explicitly accepted.");
                        if(asset.Affected && decision.Disposition!="PRESERVE" && !operating.References.Any(row=>row.AssetType==asset.Type && row.StatusType==group.Key && row.Value==decision.TargetValue && row.Covers(utc)))Issue($"{asset.Type} {asset.Id} / {group.Key}: the exact configured target lacks a current native status/type reference.");
                    }
                }
                foreach(var version in applicable)foreach(var transition in version.Policy.Transitions.Where(row=>row.AssetType==asset.Type))if(!groups.Any(group=>group.Key==transition.StatusType))Issue($"{asset.Type} {asset.Id}: configured native facet {transition.StatusType} has no reviewed source population.");
                if(asset.Affected && applicable.Length>0 && !applicable.Any(version=>version.Policy.Transitions.Any(row=>row.AssetType==asset.Type && row.Disposition!="PRESERVE")))Issue($"{asset.Type} {asset.Id}: no explicit changed operating facet is established for the controlled return.");
            }
        }
        return result.Distinct(StringComparer.Ordinal).ToArray();
    }
}
