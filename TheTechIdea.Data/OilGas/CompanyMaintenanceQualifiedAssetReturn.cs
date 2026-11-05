using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace TheTechIdea.Data.OilGas;

/// <summary>Explicit observed capability for every original qualified facility and equipment target.</summary>
public sealed record CompanyMaintenanceQualifiedReturnObservation(string AssetType,string AssetId,string? FacilityType,decimal? ObservedCapabilityPercent)
{
    public void Require()
    {
        if(AssetType is not ("FACILITY" or "EQUIPMENT") || !LeaseSummaryValidator.Identifier(AssetId) || AssetId.Length>128 ||
            AssetType=="FACILITY" && (!LeaseSummaryValidator.Identifier(FacilityType) || FacilityType!.Length>128) ||
            AssetType=="EQUIPMENT" && FacilityType is not null || ObservedCapabilityPercent is null or <0 or >100)
            throw new ArgumentException("Each original qualified asset requires its complete identity and explicitly observed capability between 0 and 100.");
    }
}
public sealed record CompanyMaintenanceQualifiedReturnTransition(string AssetType,string AssetId,string? FacilityType,string StatusType,string OriginalStatusId,string OriginalSource,string OriginalRevision,string TargetValue,string NativeSource,decimal ObservedCapabilityPercent)
{
    public string StatusId(string returnId)=>"mr-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Return=returnId,AssetType,AssetId,FacilityType,StatusType}))[..32];
}

public static class CompanyMaintenanceQualifiedAssetReturn
{
    public static void RequireReview(CompanyMaintenanceQualifiedAssets assets,WorkOrderDetailModel order,string assignedRole)
    {
        assets.Require(order);
        if(assets.Operating is null || assets.Licensing is null || assets.ReturnPolicy is not {Blockers.Count:0} policy ||
            assets.Facilities.Count+assets.Equipment.Count==0 || !policy.ActorReturnRoles.Contains(assignedRole,StringComparer.Ordinal) ||
            assets.Facilities.Any(row=>CompanyMaintenanceQualifiedAssets.IsOriginalFacilityTarget(order,row.FacilityId,row.FacilityType) && row.AccessLevel is not ("WRITE" or "DELETE")) || assets.Equipment.Any(row=>row.AccessLevel is not ("WRITE" or "DELETE")))
            throw new UnauthorizedAccessException("Every qualified asset requires WRITE authority, complete matched source policy and the configured independent return role.");
        foreach(var application in policy.Applications)
        {
            var versions=policy.Catalog.Versions.Where(row=>application.PolicyRecordIds.Contains(row.RecordId,StringComparer.Ordinal) && row.Policy.AreaApplicability!="NOT_APPLICABLE").ToArray();
            if(versions.Length==0 || versions.Any(row=>!row.Policy.AuthorizedReturnRoles.Contains(assignedRole,StringComparer.Ordinal)))
                throw new UnauthorizedAccessException("The original independent return duty must be authorized by every applicable recorded policy.");
        }
    }
    public static IReadOnlyList<CompanyMaintenanceQualifiedReturnTransition> Resolve(CompanyMaintenanceQualifiedAssets assets,
        WorkOrderDetailModel order,IReadOnlyList<CompanyMaintenanceQualifiedReturnObservation> observations,
        string assignedRole,DateTime activatedAtUtc)
    {
        RequireReview(assets,order,assignedRole);
        if(assets.Operating is not {} operating || assets.Licensing is null || assets.ReturnPolicy is not {Blockers.Count:0} policy ||
            assets.Facilities.Count+assets.Equipment.Count==0 || observations is null || observations.Count>384 ||
            activatedAtUtc.Kind!=DateTimeKind.Utc || activatedAtUtc<assets.LatestComparisonAtUtc() || activatedAtUtc>DateTime.UtcNow ||
            !policy.ActorReturnRoles.Contains(assignedRole,StringComparer.Ordinal))
            throw new InvalidOperationException("Complete qualified native sources, matched policy, current configured return role and actual activation after review are required.");
        var expected=assets.Facilities.Where(row=>CompanyMaintenanceQualifiedAssets.IsOriginalFacilityTarget(order,row.FacilityId,row.FacilityType)).Select(row=>(Type:"FACILITY",Id:row.FacilityId,Qualifier:(string?)row.FacilityType))
            .Concat(assets.Equipment.Select(row=>(Type:"EQUIPMENT",Id:row.EquipmentId,Qualifier:(string?)null))).ToHashSet();
        foreach(var row in observations)row.Require();
        if(observations.Count!=expected.Count || observations.Any(row=>!expected.Contains((row.AssetType,row.AssetId,row.FacilityType))) ||
            observations.Select(row=>(row.AssetType,row.AssetId,row.FacilityType)).Distinct().Count()!=observations.Count ||
            assets.Facilities.Any(row=>CompanyMaintenanceQualifiedAssets.IsOriginalFacilityTarget(order,row.FacilityId,row.FacilityType) && row.AccessLevel is not ("WRITE" or "DELETE") || row.EffectiveAtUtc>activatedAtUtc || row.ExpiresAtUtc<=activatedAtUtc) ||
            assets.Equipment.Any(row=>row.AccessLevel is not ("WRITE" or "DELETE") || row.EffectiveAtUtc>activatedAtUtc || row.ExpiresAtUtc<=activatedAtUtc ||
                row.CommissionedAtUtc>activatedAtUtc || row.DecommissionedAtUtc<=activatedAtUtc || row.Installation.EffectiveAtUtc>activatedAtUtc ||
                row.Installation.InstalledAtUtc>activatedAtUtc || row.Installation.ExpiresAtUtc<=activatedAtUtc || row.Installation.RemovedAtUtc<=activatedAtUtc))
            throw new InvalidOperationException("Every original qualified target requires one observation, current WRITE rights and native identity/installation coverage at actual activation.");
        // Re-evaluate every policy at the declared activation; a match at a different instant is not authority.
        var checkedPolicy=CompanyMaintenanceQualifiedReturnPolicy.Create(assets with{ReturnPolicy=null},order,policy.Catalog,
            policy.ActorReturnRoles,policy.NativeSources,activatedAtUtc);
        if(checkedPolicy.Blockers.Count!=0)throw new InvalidOperationException("Native qualified policy/source evidence does not cover actual activation.");
        var result=new List<CompanyMaintenanceQualifiedReturnTransition>();
        foreach(var observation in observations)
        {
            var facility=observation.AssetType=="FACILITY"?(observation.AssetId,observation.FacilityType!):
                assets.Equipment.Where(row=>row.EquipmentId==observation.AssetId).Select(row=>(row.Installation.FacilityId,row.Installation.FacilityType)).Single();
            var application=checkedPolicy.Applications.Single(row=>row.FacilityId==facility.Item1 && row.FacilityType==facility.Item2);
            var versions=checkedPolicy.Catalog.Versions.Where(row=>application.PolicyRecordIds.Contains(row.RecordId,StringComparer.Ordinal) && row.Policy.AreaApplicability!="NOT_APPLICABLE").ToArray();
            if(versions.Length==0 || versions.Any(row=>!row.Policy.AuthorizedReturnRoles.Contains(assignedRole,StringComparer.Ordinal)))
                throw new UnauthorizedAccessException("The original independent return duty must be authorized by every applicable recorded policy.");
            foreach(var group in operating.Statuses.Where(row=>row.AssetType==observation.AssetType && row.AssetId==observation.AssetId && row.FacilityType==observation.FacilityType).GroupBy(row=>row.StatusType,StringComparer.Ordinal))
            {
                var decisions=versions.Select(version=>version.Policy.Transitions.Single(row=>row.AssetType==observation.AssetType && row.StatusType==group.Key)).ToArray();
                if(decisions.All(row=>row.Disposition=="PRESERVE"))continue;
                var targets=decisions.Select(row=>(row.TargetValue,row.NativeSource)).Distinct().ToArray();
                if(targets.Length!=1 || decisions.Any(row=>row.Disposition=="PRESERVE"))throw new InvalidOperationException("All applicable policies must agree on the changed native facet.");
                var original=group.Single(row=>row.Covers(activatedAtUtc));var target=targets[0];
                result.Add(new(observation.AssetType,observation.AssetId,observation.FacilityType,group.Key,original.StatusId,original.Source,original.NativeRevision,target.TargetValue,target.NativeSource,observation.ObservedCapabilityPercent!.Value));
            }
            if(!result.Any(row=>row.AssetType==observation.AssetType && row.AssetId==observation.AssetId && row.FacilityType==observation.FacilityType))
                throw new InvalidOperationException("Each qualified target requires an explicitly changed operating facet; preservation alone is not a new return.");
        }
        return result.OrderBy(row=>row.AssetType,StringComparer.Ordinal).ThenBy(row=>row.AssetId,StringComparer.Ordinal).ThenBy(row=>row.FacilityType,StringComparer.Ordinal).ThenBy(row=>row.StatusType,StringComparer.Ordinal).ToArray();
    }
}
