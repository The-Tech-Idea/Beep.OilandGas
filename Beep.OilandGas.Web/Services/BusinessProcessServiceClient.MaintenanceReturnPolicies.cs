using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.Web.Services;
public partial class BusinessProcessServiceClient
{
    private Beep.OilandGas.Client.App.CompanyClientSelection MaintenanceReturnPolicyAdministrator()
    {
        var selected=_apiClient.CompanySnapshot.Selection??throw new UnauthorizedAccessException("Select the original policy administrator company/account.");
        if(!selected.Roles.Contains("Administrator",StringComparer.Ordinal) || !selected.Permissions.Contains("Maintenance.ConfigureReturnPolicy",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current app Administrator and maintenance return policy permission are required.");return selected;
    }
    public async Task<CompanyMaintenanceReturnPolicyCatalog> ReviewCompanyMaintenanceReturnPoliciesAsync(CancellationToken cancellationToken=default)
    {
        var snapshot=_apiClient.CompanySnapshot;var selected=MaintenanceReturnPolicyAdministrator();var result=await _apiClient.GetAsync<CompanyMaintenanceReturnPolicyCatalog>("/api/company/maintenance-return-policies/review",cancellationToken)??throw new InvalidOperationException("Complete company maintenance return policy catalog is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original policy administrator context changed.");result.Require();if(result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original maintenance return policy company/account differs.");return CompanyInventorySourceReview.Copy(result);
    }
    public Task<WORK_ORDER_MAINTENANCE_RETURN_POLICY> RecordCompanyMaintenanceReturnPolicyAsync(CompanyMaintenanceReturnPolicyCommand command,CancellationToken cancellationToken=default)=>MaintenanceReturnPolicyAsync(command,false,cancellationToken);
    public Task<WORK_ORDER_MAINTENANCE_RETURN_POLICY> RecoverCompanyMaintenanceReturnPolicyAsync(CompanyMaintenanceReturnPolicyCommand command,CancellationToken cancellationToken=default)=>MaintenanceReturnPolicyAsync(command,true,cancellationToken);
    private async Task<WORK_ORDER_MAINTENANCE_RETURN_POLICY> MaintenanceReturnPolicyAsync(CompanyMaintenanceReturnPolicyCommand command,bool readOnly,CancellationToken token)
    {
        command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=MaintenanceReturnPolicyAdministrator();if(command.Context.Company!=selected.Scope || command.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original policy maker/company differs.");
        var result=await _apiClient.PostAsync<CompanyMaintenanceReturnPolicyCommand,WORK_ORDER_MAINTENANCE_RETURN_POLICY>("/api/company/maintenance-return-policies/"+(readOnly?"recovery":"record"),command,token)??throw new InvalidOperationException("Original maintenance return policy append is unconfirmed; recover its retained intent.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original policy maker context changed.");command.RequireRecorded(result);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceReturnPolicyRecovery> RecoverCompanyMaintenanceReturnPolicyReferenceAsync(CompanyMaintenanceReturnPolicyReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=MaintenanceReturnPolicyAdministrator();var result=await _apiClient.PostAsync<CompanyMaintenanceReturnPolicyReference,CompanyMaintenanceReturnPolicyRecovery>("/api/company/maintenance-return-policies/recovery/reference",reference,cancellationToken)??throw new InvalidOperationException("Original private maintenance return policy reference is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original policy reference context changed.");result.Require(selected.Scope,selected.UserId,reference);return CompanyInventorySourceReview.Copy(result);
    }
}
