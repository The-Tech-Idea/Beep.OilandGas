using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.Models.Data.WorkOrder;

/// <summary>Original identity/installation scope captured before work; no operating or return clearance.</summary>
public static class CompanyMaintenanceQualifiedWorkScope
{
    public static bool HasTargets(WorkOrderDetailModel order)=>order.Components.Any(row=>
        !string.IsNullOrWhiteSpace(row.EquipmentId) || !string.IsNullOrWhiteSpace(row.FacilityId) || !string.IsNullOrWhiteSpace(row.FacilityType));

    public static void RequireSnapshot(CompanyMaintenanceQualifiedAssets assets,WorkOrderDetailModel order,FieldSelectionReview context,DateTime comparison)
    {
        RequireHeader(assets);assets.Require(order);
        if(!HasTargets(order) || assets.Context!=context || assets.Operating is not null || assets.Licensing is not null || assets.ReturnPolicy is not null ||
            comparison.Kind!=DateTimeKind.Utc || comparison<assets.ComparedAtUtc || comparison>DateTime.UtcNow)
            throw new InvalidOperationException("Original qualified work scope must be an identity-only native snapshot of this executor/company/FIELD/order before review.");
    }

    public static void RequireRecorded(CompanyMaintenanceQualifiedAssets assets,CompanyScope company,CompanyMaintenanceReceivingQuery task,string executor,DateTime started)
    {
        RequireHeader(assets);task.Require();FieldSelectionReviewValidator.RequireReview(assets.Context,company,executor);
        if(assets.Context.Company!=company || assets.Context.ActorUserId!=executor || assets.Context.FieldId!=task.FieldId ||
            assets.Operating is not null || assets.Licensing is not null || assets.ReturnPolicy is not null ||
            !ProcessDefinitionRevision.IsValid(assets.OrderRevision) || assets.Facilities is not {Count:>0 and <=256} || assets.Equipment is null || assets.Equipment.Count>128 ||
            assets.ComparedAtUtc.Kind!=DateTimeKind.Utc || assets.ComparedAtUtc==default || started.Kind!=DateTimeKind.Utc || started<assets.ComparedAtUtc)
            throw new InvalidOperationException("Recorded qualified work scope must retain the original executor/company/FIELD/order, native identity and pre-start UTC.");
        RequireAtStart(assets,started);
    }

    private static void RequireHeader(CompanyMaintenanceQualifiedAssets assets)
    {
        if(assets?.Context is null || assets.Facilities is null || assets.Equipment is null || assets.Facilities.Any(row=>row is null) || assets.Equipment.Any(row=>row is null))
            throw new InvalidOperationException("Complete original qualified work identity headers/populations are required.");
    }

    public const string MissingOriginal="Original qualified identity/installation was not captured with the recorded work start; current installation cannot replace that history.";
    public static string[] GetReturnBlockers(CompanyMaintenanceQualifiedAssets current,CompanyWorkOrderSubmissionFacts report,WorkOrderDetailModel order)
    {
        current.Require(order);report.Require();
        if(report.QualifiedWorkAssets is not {} original || report.WorkStartedAtUtc is not {} started)return [MissingOriginal];
        if(original.OrderRevision!=current.OrderRevision)return ["Original qualified work order/component scope changed; current scope cannot amend the recorded work."];
        RequireSnapshot(original,order,original.Context,started);
        if(original.Context.Company!=current.Context.Company || original.Context.Database!=current.Context.Database || original.Context.FieldId!=current.Context.FieldId)
            throw new InvalidOperationException("Original qualified work/return company, FIELD and CORE differ.");
        if(!current.Facilities.Select(row=>(row.FacilityId,row.FacilityType)).OrderBy(row=>row.FacilityId,StringComparer.Ordinal).ThenBy(row=>row.FacilityType,StringComparer.Ordinal)
                .SequenceEqual(original.Facilities.Select(row=>(row.FacilityId,row.FacilityType)).OrderBy(row=>row.FacilityId,StringComparer.Ordinal).ThenBy(row=>row.FacilityType,StringComparer.Ordinal)) ||
            !current.Equipment.Select(row=>row.EquipmentId).Order(StringComparer.Ordinal).SequenceEqual(original.Equipment.Select(row=>row.EquipmentId).Order(StringComparer.Ordinal),StringComparer.Ordinal))
            return ["Original qualified work population or installed parent changed; current scope cannot amend the recorded work."];
        // Current access/return authority is checked independently; compare physical scope across authorized personas.
        var normalized=current with{Context=original.Context,Operating=null,Licensing=null,ReturnPolicy=null,
            Facilities=current.Facilities.Select(row=>row with{AccessLevel=original.Facilities.Single(value=>value.FacilityId==row.FacilityId && value.FacilityType==row.FacilityType).AccessLevel}).ToArray(),
            Equipment=current.Equipment.Select(row=>row with{AccessLevel=original.Equipment.Single(value=>value.EquipmentId==row.EquipmentId).AccessLevel}).ToArray()};
        return normalized.IdentityRevision()==original.IdentityRevision()?[]:["Original qualified work identity/installation or native source history changed; current scope cannot amend the recorded work."];
    }

    public static void RequireAtStart(CompanyMaintenanceQualifiedAssets assets,DateTime started)
    {
        RequireHeader(assets);if(started.Kind!=DateTimeKind.Utc || started==default || started>DateTime.UtcNow)throw new InvalidOperationException("Actual qualified work start must be declared UTC.");
        // Complete population is validated against the original order by the retained start/report owners.
        foreach(var row in assets.Facilities)
            if(!Covers(row.EffectiveAtUtc,row.ExpiresAtUtc,started))throw new InvalidOperationException("Original qualified facility period does not cover actual work start.");
        foreach(var row in assets.Equipment)
            if(!Covers(row.EffectiveAtUtc,row.ExpiresAtUtc,started) || row.CommissionedAtUtc>started || row.DecommissionedAtUtc<=started ||
                row.Installation is not {} installed || !Covers(installed.EffectiveAtUtc,installed.ExpiresAtUtc,started) ||
                installed.InstalledAtUtc.Kind!=DateTimeKind.Utc || installed.InstalledAtUtc>started || installed.RemovedAtUtc<=started)
                throw new InvalidOperationException("Original equipment/installation period does not cover actual work start.");
        static bool Covers(DateTime from,DateTime? to,DateTime at)=>from.Kind==DateTimeKind.Utc && from!=default && from<=at && (to is null || to.Value.Kind==DateTimeKind.Utc && to>at);
    }
}

public interface ICompanyMaintenanceQualifiedWorkAssetOwner
{
    Task<CompanyMaintenanceQualifiedAssets> ReadMaintenanceQualifiedWorkAssetsAsync(FieldSelectionReview context,WorkOrderDetailModel order,Func<Task> authority,bool owned);
}
