using System.Security.Claims;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyMaintenanceHandoffService
{
    private async Task<(string Revision,IReadOnlyList<string> Capabilities)> AssetReturnOrder(CompanyWorkOrderCompletionQuery query)
    {
        if(workOrders is null)throw new InvalidOperationException("The existing original work-order owner is required.");
        var order=CompanyInventorySourceReview.Copy(await workOrders.GetByIdAsync(query.Task.FieldId,query.Task.WorkOrderId)??throw new InvalidOperationException("Original affected return targets are unavailable."));
        var capabilities=CompanyMaintenanceAssetReturnCapabilities.ForOrder(order);
        if(order.Source!="WORK_ORDER" || order.InstanceId!=query.Task.WorkOrderId || order.FieldId!=query.Task.FieldId)throw new UnauthorizedAccessException("Original company FIELD/order target source differs.");return(CompanyWorkOrderExecutionRequirements.OrderRevision(order),capabilities);
    }
    private async Task RequireAssetReturnOrder(CompanyWorkOrderCompletionQuery query,string revision)
    {if((await AssetReturnOrder(query)).Revision!=revision)throw new InvalidOperationException("The original affected return target population changed; retain its original reference.");}
    private async Task<(FieldSelectionReview Lifecycle,FieldSelectionReview Core,ICompanyMaintenanceClosedReturnOwner Closed,ICompanyMaintenanceAssetReturnOwner Wells,Func<string?,Task> Guard,string OrderRevision)> AssetReturnAuthority(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query)
    {
        var captured=await MaintenanceClosureAuthority(user,query,true);
        if(captured.Owner is not ICompanyMaintenanceClosedReturnOwner closed || maintenanceWells is not ICompanyMaintenanceAssetReturnOwner wells)throw new InvalidOperationException("Existing native closed-return and controlled well owners are required.");
        var order=await AssetReturnOrder(query);var capabilities=order.Capabilities;var revision=order.Revision;
        async Task Guard(string? role)
        {
            using var activity=PermitProofActivities.StartActivity("maintenance.asset_return.authority");
            // The paired guard refreshes the complete permission snapshot; no native read occurs before this check.
            await captured.Guard(role);var access=company.Company;
            if(capabilities.Any(permission=>!access.Permissions.Contains(permission,StringComparer.Ordinal)))throw new UnauthorizedAccessException("Current company capabilities for every original affected return target are required.");
        }
        await Guard(null);return(captured.Lifecycle,captured.Core,closed,wells,Guard,revision);
    }
    public async Task<CompanyMaintenanceAssetReturnReview> ReviewMaintenanceAssetReturnAsync(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query)
    {
        var captured=await AssetReturnAuthority(user,query);
        async Task<CompanyMaintenanceAssetReturnReview> Observe()
        {
            var source=await captured.Closed.ReadClosedReturnSourceAsync(captured.Lifecycle,query,captured.Core.Database.ConnectionName,false,captured.Guard)??throw new InvalidOperationException("Original configured terminal return duty is required.");
            var assets=await ReadMaintenanceAssetEvidenceAsync(user,query);var physical=await ReadReturnEvidenceAsync(user,source.Assignment,false);
            if(assets.Context!=captured.Core || physical.Assignment.Stage.Context!=captured.Lifecycle || CompanyWorkOrderExecutionRequirements.OrderRevision(assets.Order)!=captured.OrderRevision || JsonSerializer.Serialize(source.Closure)!=JsonSerializer.Serialize(assets.Closure))throw new InvalidOperationException("Original closure and current company/CORE/lifecycle asset-return sources differ.");
            var result=new CompanyMaintenanceAssetReturnReview(assets,physical);result.Require();await captured.Guard(source.Assignment.Stage.AssignedRole);return result;
        }
        var first=await Observe();var closing=await Observe();if(first.SourceRevision()!=closing.SourceRevision())throw new InvalidOperationException("Original closure/current physical/native well return sources changed during review.");await RequireAssetReturnOrder(query,captured.OrderRevision);await captured.Guard(closing.Physical.Assignment.Stage.AssignedRole);return closing;
    }
    public async Task<CompanyMaintenanceAssetReturnRecovery> RecordMaintenanceAssetReturnAsync(ClaimsPrincipal user,CompanyMaintenanceAssetReturnCommand original)
    {
        using var activity=PermitProofActivities.StartActivity("maintenance.asset_return.record");
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var captured=await AssetReturnAuthority(user,command.Reference().Query);
        if(command.Review.Assets.Context!=captured.Core || command.Review.Physical.Assignment.Stage.Context!=captured.Lifecycle || CompanyWorkOrderExecutionRequirements.OrderRevision(command.Review.Assets.Order)!=captured.OrderRevision)throw new UnauthorizedAccessException("Original asset-return company/account/FIELD/CORE/lifecycle/target authority differs.");
        async Task<CompanyMaintenanceClosureFacts?> Closure(bool owned)
        {
            var source=await captured.Closed.ReadClosedReturnSourceAsync(captured.Lifecycle,command.Reference().Query,captured.Core.Database.ConnectionName,owned,captured.Guard);
            if(source is not null && JsonSerializer.Serialize(source.Assignment)!=JsonSerializer.Serialize(command.Review.Physical.Assignment))throw new InvalidOperationException("Original configured closed return assignment changed before activation.");return source?.Closure;
        }
        var result=await captured.Wells.RecordMaintenanceAssetReturnAsync(command,captured.Guard,owned=>ReadReturnEvidenceAsync(user,command.Review.Physical.Assignment,owned),Closure);
        result.Require(captured.Core.Company,captured.Core.ActorUserId,command.Reference());await RequireAssetReturnOrder(command.Reference().Query,captured.OrderRevision);await captured.Guard(command.Review.Physical.Assignment.Stage.AssignedRole);return result;
    }
    public async Task<CompanyMaintenanceAssetReturnRecovery> RecoverMaintenanceAssetReturnAsync(ClaimsPrincipal user,CompanyMaintenanceAssetReturnReference reference)
    {
        using var activity=PermitProofActivities.StartActivity("maintenance.asset_return.recovery");
        reference.Require();var captured=await AssetReturnAuthority(user,reference.Query);
        Task<CompanyMaintenanceClosedReturnSource?> Read()=>captured.Closed.ReadClosedReturnSourceAsync(captured.Lifecycle,reference.Query,captured.Core.Database.ConnectionName,false,captured.Guard);
        var first=await Read()??throw new InvalidOperationException("Original terminal return duty is required for recovery.");
        var result=await captured.Wells.RecoverMaintenanceAssetReturnAsync(captured.Core,reference,captured.Guard);result.Require(captured.Core.Company,captured.Core.ActorUserId,reference);
        if(CompanyWorkOrderExecutionRequirements.OrderRevision(result.Command.Review.Assets.Order)!=captured.OrderRevision)throw new UnauthorizedAccessException("Original retained return targets differ from the current authorized work scope.");
        var stage=result.Command.Review.Physical.Assignment.Stage;
        if(stage.Context.Database!=captured.Lifecycle.Database || stage.AssignedRole!=first.Assignment.Stage.AssignedRole || JsonSerializer.Serialize(result.Command.Review.Assets.Closure)!=JsonSerializer.Serialize(first.Closure))throw new InvalidOperationException("Original private return lifecycle/configured role/closure source differs.");
        var closing=await Read();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original terminal return source changed during private recovery.");await RequireAssetReturnOrder(reference.Query,captured.OrderRevision);await captured.Guard(stage.AssignedRole);return result;
    }
}
