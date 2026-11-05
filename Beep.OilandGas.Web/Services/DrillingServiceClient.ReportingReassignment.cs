using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public partial class DrillingServiceClient
{
    public async Task<DrillingReportingReassignmentCatalog> GetReportingReassignmentCatalogAsync(string assignmentId,CancellationToken cancellationToken=default)
    {
        ReportingIdentity(assignmentId);var context=_apiClient.CompanySnapshot;
        var result=await _apiClient.GetAsync<DrillingReportingReassignmentCatalog>("/api/drilling/programs/execution/reporting/transfer/catalog?assignmentId="+Uri.EscapeDataString(assignmentId),cancellationToken)
            ??throw new InvalidOperationException("The current configured reporting transfer catalog is unavailable.");
        if(context!=_apiClient.CompanySnapshot || context.Selection is not {} selected)throw new InvalidOperationException("The reporting transfer company/account changed during loading.");
        result.Require(selected.Scope,selected.UserId,assignmentId);return result;
    }
    public async Task<DrillingReportingReassignmentReview> ReviewReportingReassignmentAsync(DrillingReportingReassignmentSelection selection,CancellationToken cancellationToken=default)
    {
        selection.Require();var context=_apiClient.CompanySnapshot;var result=await _apiClient.PostAsync<DrillingReportingReassignmentSelection,DrillingReportingReassignmentReview>("/api/drilling/programs/execution/reporting/transfer/review",selection,cancellationToken)
            ??throw new InvalidOperationException("The complete current reporting transfer review is unavailable.");
        result.Require();if(context!=_apiClient.CompanySnapshot || context.Selection is not {} selected || result.Scope().Company!=selected.Scope || result.Scope().ActorUserId!=selected.UserId || result.Responsibility.Root.InstanceId()!=selection.AssignmentId || result.Destination.ProcessId!=selection.ProcessId || result.ManagerRole!=selection.ManagerRole)
            throw new InvalidOperationException("The transfer review belongs to another reporting selection.");return result;
    }
    public Task<DrillingReportingReassignmentReceipt> ReassignReportingAsync(DrillingReportingReassignment original,CancellationToken cancellationToken=default)=>ReportingReassignment(original,false,cancellationToken);
    public Task<DrillingReportingReassignmentReceipt> RecoverReportingReassignmentAsync(DrillingReportingReassignment original,CancellationToken cancellationToken=default)=>ReportingReassignment(original,true,cancellationToken);
    private async Task<DrillingReportingReassignmentReceipt> ReportingReassignment(DrillingReportingReassignment original,bool recover,CancellationToken cancellationToken)
    {
        original.Require();var context=_apiClient.CompanySnapshot;
        if(context.Selection is not {} selected || original.Scope().Company!=selected.Scope || original.Scope().ActorUserId!=selected.UserId)throw new InvalidOperationException("The retained reporting transfer belongs to another company/account.");
        var result=await _apiClient.PostAsync<DrillingReportingReassignment,DrillingReportingReassignmentReceipt>("/api/drilling/programs/execution/reporting/transfer"+(recover?"/recover":""),original,cancellationToken)
            ??throw new InvalidOperationException("The exact original reporting transfer is unconfirmed.");result.Require(original);
        if(context!=_apiClient.CompanySnapshot)throw new InvalidOperationException("The reporting transfer company/account changed before acknowledgement.");return result;
    }
}
