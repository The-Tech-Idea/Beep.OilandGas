using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyMaintenanceHandoffService
{
    public async Task<CompanyMaintenanceAssetReturnStatus> ReadMaintenanceAssetReturnStatusAsync(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query)
    {
        query.Require();var boundary=await Boundary(user,query.Task.WorkOrderId,query.Task.FieldId,"READ");
        if(boundary.Process is not ICompanyMaintenanceClosureOwner closureOwner || maintenanceWells is not ICompanyMaintenanceAssetReturnOwner wells || executionPermits is null || workOrders is null)throw new InvalidOperationException("Existing native report/closure/work-order/well owners are required for recorded return status.");
        async Task<FieldSelectionReview> Core()
        {
            var access=await company.RefreshCompanyAsync();
            if(!access.Permissions.Contains("Maintenance.ReturnToService",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current company return evidence capability is required.");
            var authority=await Authority(user,boundary.Preference,query.Task.WorkOrderId,"READ");var name=await modules.ResolveAsync("PPDM_CORE");var database=new AssetDatabaseScope(name,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],name));
            if(authority!=boundary.Context)throw new UnauthorizedAccessException("Original selected company/account/FIELD/order read authority changed.");
            return boundary.Preference with{Database=database,Revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Authority=authority,Permission="Maintenance.ReturnToService",Access="READ",Database=database}))};
        }
        var context=await Core();async Task Guard(){if(await Core()!=context)throw new UnauthorizedAccessException("Original recorded return company/CORE/read authority changed.");}
        async Task<(CompanyMaintenanceAssetReturnStatus Status,string Source)> Observe()
        {
            await Guard();var report=await executionPermits.ReadSubmissionFactsAsync(context,query,Guard);
            var closure=await closureOwner.ReadMaintenanceClosureFactsAsync(boundary.Context,query,Guard)??throw new InvalidOperationException("Original recorded maintenance closure is required.");
            var order=CompanyInventorySourceReview.Copy(await workOrders.GetByIdAsync(query.Task.FieldId,query.Task.WorkOrderId)??throw new InvalidOperationException("Original current component population is unavailable."));RecordedWorkOrderComponentChecks.Require(order);
            if(order.Components.Count>128 || CompanyWorkOrderExecutionRequirements.OrderRevision(order)!=report.OriginalOrderRevision || closure.CoreDatabase!=context.Database || closure.ReportRevision!=report.Revision)throw new InvalidOperationException("Original closed report/CORE/component scope changed.");
            if(order.Components.Any(row=>!ProcessDefinitionRevision.IsValid(row.NativeRevision) || row.UnresolvedReferenceKinds is {Count:>0} || string.IsNullOrWhiteSpace(row.WellId) && string.IsNullOrWhiteSpace(row.EquipmentId) && string.IsNullOrWhiteSpace(row.FacilityId) && !(row.InformationItemSubtype=="DOCUMENT" && !string.IsNullOrWhiteSpace(row.InformationItemId))))throw new InvalidOperationException("Every original affected component requires its complete supported native relationship.");
            var hasQualified=CompanyMaintenanceQualifiedWorkScope.HasTargets(order);
            var ids=order.Components.Where(row=>!string.IsNullOrWhiteSpace(row.WellId)).Select(row=>row.WellId!).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();if(ids.Length==0 && !hasQualified)throw new InvalidOperationException("Original affected asset scope is unavailable.");
            var current=new List<CompanyMaintenanceWellEvidence>();foreach(var id in ids)current.Add(await wells.ReadMaintenanceWellEvidenceAsync(context,id,Guard));
            CompanyMaintenanceQualifiedAssets? qualified=null;
            if(hasQualified)qualified=await (qualifiedAssets??throw new InvalidOperationException("The complete current qualified READ evidence owner is required.")).ReadMaintenanceQualifiedCurrentStatusAsync(context,order,Guard);
            var recorded=await wells.ReadMaintenanceAssetReturnFactsAsync(context,query,Guard);
            if(recorded is not null && (recorded.ReportRevision!=report.Revision || recorded.ClosureRevision!=closure.Revision || recorded.PhysicalSubjectRevision!=closure.PhysicalSubjectRevision || !recorded.Wells.Select(well=>well.WellId).SequenceEqual(ids,StringComparer.Ordinal)))throw new InvalidOperationException("Recorded native asset return differs from the original closed report and complete well population.");
            var result=new CompanyMaintenanceAssetReturnStatus(context,query,recorded,current){OriginalComponents=hasQualified?CompanyInventorySourceReview.Copy(order.Components.ToArray()):null,CurrentQualifiedAssets=qualified};result.Require();await Guard();return(result,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{report,closure,order,Wells=current.Select(well=>well.SourceRevision()),Qualified=qualified?.SourceRevision(),recorded})));
        }
        var first=await Observe();var closing=await Observe();if(first.Source!=closing.Source)throw new InvalidOperationException("Original public return/report/closure/current native read evidence changed.");await Guard();return closing.Status;
    }
}
