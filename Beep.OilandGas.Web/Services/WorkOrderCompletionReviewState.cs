using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;
public sealed class WorkOrderCompletionReviewState(CompanyClientContext company):IDisposable
{
    private CompanyWorkOrderCompletionQuery? query;
    private bool subscribed,disposed;
    private int version;
    public event Action? Changed;
    public CompanyWorkOrderCompletionReview? Current{get;private set;}
    public CompanyWorkOrderCompletionDecisionStatus? DecisionStatus{get;private set;}
    public CompanyMaintenanceReturnHandoffReview? ReturnReview{get;private set;}
    public bool Busy{get;private set;}
    public void Attach(CompanyWorkOrderCompletionQuery? next){if(!subscribed){company.Changed+=Clear;subscribed=true;}next?.Require();if(query==next)return;Clear();query=next;}
    public void Clear(){version++;query=null;Current=null;DecisionStatus=null;ReturnReview=null;Changed?.Invoke();}
    public async Task ReadReturnReviewAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(disposed || Busy || query is null || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Maintenance.VerifyCompletion",StringComparer.Ordinal) || !selected.Permissions.Contains("Workflow.Approve",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current completion reviewer role, verification and approval capabilities are required.");
        var original=query;var snapshot=company.Snapshot;var generation=version;Busy=true;ReturnReview=null;Changed?.Invoke();
        try
        {
            var result=await client.ReviewReturnHandoffAsync(original)??throw new InvalidOperationException("Configured return review is unavailable.");result.Require();
            if(result.Completion.Report.Query!=original || result.Stage.Context.Company!=selected.Scope || result.Stage.Context.ActorUserId!=selected.UserId || !selected.Roles.Contains(result.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original company/account/report/configured reviewer role differs.");
            if(!disposed && generation==version && snapshot==company.Snapshot)ReturnReview=CompanyInventorySourceReview.Copy(result);
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task ReadDecisionStatusAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(disposed || Busy || query is null || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Maintenance.VerifyCompletion",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current standard company role, original report and completion read capability are required.");
        var original=query;var snapshot=company.Snapshot;var generation=version;Busy=true;DecisionStatus=null;Changed?.Invoke();
        try
        {
            var result=await client.ReadCompletionDecisionStatusAsync(original)??throw new InvalidOperationException("Recorded decision status is unavailable; a failed read cannot establish absence.");result.Require();
            if(result.Query!=original || result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original company/account/report decision status differs.");
            if(!disposed && generation==version && snapshot==company.Snapshot)DecisionStatus=CompanyInventorySourceReview.Copy(result);
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task ReadAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(disposed || Busy || query is null || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Maintenance.VerifyCompletion",StringComparer.Ordinal))throw new UnauthorizedAccessException("Original standard completion reviewer company/account/query and capability are required.");
        var original=query;var snapshot=company.Snapshot;var generation=version;Busy=true;Current=null;Changed?.Invoke();
        try
        {
            var result=await client.ReviewWorkOrderCompletionAsync(original);result.Require();
            if(result.Report.Query!=original || result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId || result.Report.ExecutorUserId==selected.UserId)throw new UnauthorizedAccessException("Original independent completion reviewer/company/task/report differs.");
            if(!disposed && generation==version && snapshot==company.Snapshot)Current=CompanyInventorySourceReview.Copy(result);
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;version++;if(subscribed)company.Changed-=Clear;Changed=null;}
}
