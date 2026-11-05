using System.Security.Claims;
using TheTechIdea.Data.OilGas;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyMaintenanceHandoffService
{
    public async Task<CompanyMaintenanceAssetEvidence> ReadMaintenanceAssetEvidenceAsync(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query)
    {
        using var requestActivity=PermitProofActivities.StartActivity("maintenance.asset_return.evidence");
        var captured=await MaintenanceClosureAuthority(user,query,false);if(maintenanceWells is null || workOrders is null)throw new InvalidOperationException("Existing canonical well and original work-order owners are required for maintenance asset evidence.");
        async Task Guard(){using var activity=PermitProofActivities.StartActivity("maintenance.asset_return.authority");await captured.Guard(null);}
        async Task<CompanyMaintenanceAssetEvidence> Observe()
        {
            await Guard();CompanyWorkOrderSubmissionFacts report;using(PermitProofActivities.StartActivity("maintenance.asset_return.report"))report=await executionPermits!.ReadSubmissionFactsAsync(captured.Core,query,Guard);
            CompanyMaintenanceClosureFacts closure;using(PermitProofActivities.StartActivity("maintenance.asset_return.closure"))closure=await captured.Owner.ReadMaintenanceClosureFactsAsync(captured.Lifecycle,query,Guard)??throw new InvalidOperationException("Original recorded maintenance closure is required before asset-return review.");
            var order=CompanyInventorySourceReview.Copy(await workOrders.GetByIdAsync(query.Task.FieldId,query.Task.WorkOrderId)??throw new InvalidOperationException("Original current component population is unavailable."));RecordedWorkOrderComponentChecks.Require(order);if(order.Components.Count>128)throw new InvalidOperationException("The original affected component population exceeds this bounded asset-return review.");
            var wells=new List<CompanyMaintenanceWellEvidence>();foreach(var well in order.Components.Where(row=>!string.IsNullOrWhiteSpace(row.WellId)).Select(row=>row.WellId!).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)){await Guard();wells.Add(await maintenanceWells.ReadMaintenanceWellEvidenceAsync(captured.Core,well,Guard));}
            CompanyMaintenanceQualifiedAssets? qualified=null;
            if(order.Components.Any(row=>!string.IsNullOrWhiteSpace(row.EquipmentId) || !string.IsNullOrWhiteSpace(row.FacilityId) || !string.IsNullOrWhiteSpace(row.FacilityType)))
            {using var activity=PermitProofActivities.StartActivity("maintenance.asset_return.qualified");qualified=await (qualifiedAssets??throw new InvalidOperationException("The active qualified facility/equipment evidence owner is required for every original mixed component.")).ReadMaintenanceQualifiedAssetsAsync(captured.Core,order,Guard);}
            var utc=DateTime.UtcNow;var result=new CompanyMaintenanceAssetEvidence(captured.Core,report,closure,order,wells,utc,CompanyMaintenanceAssetEvidence.GetBlockers(captured.Core,report,closure,order,wells,utc,qualified)){QualifiedAssets=qualified};result.Require();await Guard();return result;
        }
        var first=await Observe();var closing=await Observe();if(first.SourceRevision()!=closing.SourceRevision())throw new InvalidOperationException("Original closure/report/component/native asset evidence changed during review.");await Guard();return closing;
    }
}
