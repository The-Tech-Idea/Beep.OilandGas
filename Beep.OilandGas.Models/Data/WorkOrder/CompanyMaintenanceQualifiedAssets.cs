using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.Models.Data.WorkOrder;

public sealed record CompanyMaintenanceFacilityIdentity(string FacilityId,string FacilityType,string FieldId,string OperatorBaId,string AccessLevel,DateTime EffectiveAtUtc,DateTime? ExpiresAtUtc,string NativeRevision);
public sealed record CompanyMaintenanceEquipmentInstallation(string EquipmentId,string FacilityId,string FacilityType,decimal InstallationObservation,DateTime EffectiveAtUtc,DateTime InstalledAtUtc,DateTime? ExpiresAtUtc,DateTime? RemovedAtUtc,string Source,string NativeRevision,int ReviewedPopulation,string PopulationRevision);
public sealed record CompanyMaintenanceEquipmentIdentity(string EquipmentId,string OwnerBaId,string AccessLevel,DateTime EffectiveAtUtc,DateTime? ExpiresAtUtc,DateTime? CommissionedAtUtc,DateTime? DecommissionedAtUtc,string NativeRevision,CompanyMaintenanceEquipmentInstallation Installation);

/// <summary>Current native identity and installation proof. This does not interpret status codes or authorize return.</summary>
public sealed record CompanyMaintenanceQualifiedAssets(FieldSelectionReview Context,string OrderRevision,string DateTimeZoneId,IReadOnlyList<CompanyMaintenanceFacilityIdentity> Facilities,IReadOnlyList<CompanyMaintenanceEquipmentIdentity> Equipment,DateTime ComparedAtUtc)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public CompanyMaintenanceQualifiedOperating? Operating{get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public CompanyMaintenanceQualifiedLicensing? Licensing{get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public CompanyMaintenanceQualifiedReturnPolicy? ReturnPolicy{get;init;}
    public static bool IsOriginalFacilityTarget(WorkOrderDetailModel order,string facilityId,string facilityType)=>
        order.Components.Any(row=>row.FacilityId==facilityId && row.FacilityType==facilityType);
    private static bool Identifier(string? value)=>LeaseSummaryValidator.Identifier(value) && value!.Length<=128;
    private static bool Level(string level)=>level is "READ" or "WRITE" or "DELETE";
    private static bool Period(DateTime from,DateTime? to,DateTime at)=>from!=default && from.Kind==DateTimeKind.Utc && from<=at && (to is null || to.Value.Kind==DateTimeKind.Utc && to>at && to>from);
    public void Require(WorkOrderDetailModel order)
    {
        RequireIdentity(order,CompanyWorkOrderExecutionRequirements.OrderRevision(order));
        Operating?.Require(this);Licensing?.Require(this);ReturnPolicy?.Require(this,order);
    }
    public void RequireCurrentStatus(CompanyMaintenanceReceivingQuery task,IReadOnlyList<WorkOrderComponentReference> components)
    {
        task.Require();
        if(components is null || components.Count is 0 or >128 || components.Any(row=>row is null || !ProcessDefinitionRevision.IsValid(row.NativeRevision) || row.UnresolvedReferenceKinds is {Count:>0}) || Operating is null || Licensing is not null || ReturnPolicy is not null)
            throw new InvalidOperationException("Complete original component identities and qualified current READ status are required.");
        // The API binds OrderRevision to its full native order/report. Public status carries only component identities.
        var scope=new WorkOrderDetailModel{InstanceId=task.WorkOrderId,FieldId=task.FieldId,Source="WORK_ORDER",Components=components.ToList()};
        RequireIdentity(scope,OrderRevision);Operating.Require(this);
    }
    private void RequireIdentity(WorkOrderDetailModel order,string expectedOrderRevision)
    {
        FieldSelectionReviewValidator.RequireReview(Context,Context.Company,Context.ActorUserId);RecordedWorkOrderComponentChecks.Require(order);
        if(order.Source!="WORK_ORDER" || order.FieldId!=Context.FieldId || order.Components.Count>128 || !ProcessDefinitionRevision.IsValid(expectedOrderRevision) || OrderRevision!=expectedOrderRevision || string.IsNullOrWhiteSpace(DateTimeZoneId) || ComparedAtUtc.Kind!=DateTimeKind.Utc || ComparedAtUtc==default || Facilities is null || Equipment is null || Facilities.Count>256 || Equipment.Count>128)throw new InvalidOperationException("Complete original qualified component scope, date zone and current native evidence are required.");
        try{_=TimeZoneInfo.FindSystemTimeZoneById(DateTimeZoneId);}catch(Exception error) when(error is TimeZoneNotFoundException or InvalidTimeZoneException){throw new InvalidOperationException("Explicit company evidence time zone is unavailable.",error);}
        var ids=order.Components.Where(row=>!string.IsNullOrWhiteSpace(row.EquipmentId)).Select(row=>row.EquipmentId!).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);
        if(!ids.SequenceEqual(Equipment.Select(row=>row.EquipmentId).Order(StringComparer.Ordinal),StringComparer.Ordinal) || Equipment.Select(row=>row.EquipmentId).Distinct(StringComparer.Ordinal).Count()!=Equipment.Count)throw new InvalidOperationException("Every original equipment component requires one complete current identity and installation.");
        foreach(var row in Equipment)
        {
            if(!Identifier(row.EquipmentId) || row.OwnerBaId!=Context.Company.BusinessAssociateId || !Level(row.AccessLevel) || !Period(row.EffectiveAtUtc,row.ExpiresAtUtc,ComparedAtUtc) || !ProcessDefinitionRevision.IsValid(row.NativeRevision) || row.CommissionedAtUtc is {} commissioned && (commissioned.Kind!=DateTimeKind.Utc || commissioned>ComparedAtUtc) || row.DecommissionedAtUtc is {} decommissioned && (decommissioned.Kind!=DateTimeKind.Utc || decommissioned<=ComparedAtUtc))throw new InvalidOperationException("Current native equipment owner, authority and effective/commissioning periods must agree.");
            var installed=row.Installation;if(installed is null || installed.EquipmentId!=row.EquipmentId || !Identifier(installed.FacilityId) || !Identifier(installed.FacilityType) || installed.InstallationObservation<0 || decimal.Truncate(installed.InstallationObservation)!=installed.InstallationObservation || !Period(installed.EffectiveAtUtc,installed.ExpiresAtUtc,ComparedAtUtc) || installed.InstalledAtUtc==default || installed.InstalledAtUtc.Kind!=DateTimeKind.Utc || installed.InstalledAtUtc>ComparedAtUtc || installed.RemovedAtUtc is {} removed && (removed.Kind!=DateTimeKind.Utc || removed<=ComparedAtUtc || removed<=installed.InstalledAtUtc) || !Identifier(installed.Source) || !ProcessDefinitionRevision.IsValid(installed.NativeRevision) || installed.ReviewedPopulation is <1 or >256 || !ProcessDefinitionRevision.IsValid(installed.PopulationRevision))throw new InvalidOperationException("Exact current equipment installation keys, complete membership and native periods are required.");
            foreach(var component in order.Components.Where(component=>component.EquipmentId==row.EquipmentId))
                if(!string.IsNullOrWhiteSpace(component.FacilityId) && (component.FacilityId!=installed.FacilityId || component.FacilityType!=installed.FacilityType))throw new InvalidOperationException("Original component qualification differs from the current equipment installation.");
        }
        var expected=order.Components.Where(row=>!string.IsNullOrWhiteSpace(row.FacilityId) || !string.IsNullOrWhiteSpace(row.FacilityType)).Select(row=>(row.FacilityId,row.FacilityType)).Concat(Equipment.Select(row=>((string?)row.Installation.FacilityId,(string?)row.Installation.FacilityType))).Distinct().OrderBy(row=>row.Item1,StringComparer.Ordinal).ThenBy(row=>row.Item2,StringComparer.Ordinal).ToArray();
        var actual=Facilities.Select(row=>((string?)row.FacilityId,(string?)row.FacilityType)).OrderBy(row=>row.Item1,StringComparer.Ordinal).ThenBy(row=>row.Item2,StringComparer.Ordinal).ToArray();
        if(!expected.SequenceEqual(actual) || actual.Distinct().Count()!=actual.Length)throw new InvalidOperationException("Every original or installed qualified facility requires its complete parent evidence.");
        foreach(var row in Facilities)if(!Identifier(row.FacilityId) || !Identifier(row.FacilityType) || row.FieldId!=Context.FieldId || !Identifier(row.OperatorBaId) || !Level(row.AccessLevel) || !Period(row.EffectiveAtUtc,row.ExpiresAtUtc,ComparedAtUtc) || !ProcessDefinitionRevision.IsValid(row.NativeRevision))throw new InvalidOperationException("Current qualified facility, recorded operator, FIELD, native periods and READ authority are required.");
    }
    public string IdentityRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context,OrderRevision,DateTimeZoneId,Facilities=Facilities.OrderBy(row=>row.FacilityId,StringComparer.Ordinal).ThenBy(row=>row.FacilityType,StringComparer.Ordinal),Equipment=Equipment.OrderBy(row=>row.EquipmentId,StringComparer.Ordinal)}));
    public DateTime LatestComparisonAtUtc()=>new[]{ComparedAtUtc,Operating?.ComparedAtUtc??ComparedAtUtc,Licensing?.ComparedAtUtc??ComparedAtUtc,ReturnPolicy?.ComparedAtUtc??ComparedAtUtc}.Max();
    public string SourcesRevision(){var previous=Operating is null?IdentityRevision():ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Identity=IdentityRevision(),Operating=Operating.SourceRevision()}));return Licensing is null?previous:ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Previous=previous,Licensing=Licensing.SourceRevision()}));}
    public string SourceRevision(){var sources=SourcesRevision();return ReturnPolicy is null?sources:ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Sources=sources,ReturnPolicy=ReturnPolicy.SourceRevision()}));}
}
public interface ICompanyMaintenanceQualifiedAssetEvidenceOwner
{
    Task<CompanyMaintenanceQualifiedAssets> ReadMaintenanceQualifiedAssetsAsync(FieldSelectionReview context,WorkOrderDetailModel order,Func<Task> authority);
    Task<CompanyMaintenanceQualifiedAssets> ReadMaintenanceQualifiedCurrentStatusAsync(FieldSelectionReview context,WorkOrderDetailModel order,Func<Task> authority);
}
