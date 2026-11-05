using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Client.App;

namespace Beep.OilandGas.Web.Services;

public sealed class DevelopmentInvestmentRoutingState:IDisposable
{
    private readonly CompanyClientContext company;private readonly IPersonaContextService personas;private readonly IAfeServiceClient client;
    private readonly Func<Exception,string,string,string> explain;private DevelopmentInvestmentRoutingReview? current;
    private string? processId,afeId;private long generation;private bool disposed;
    public DevelopmentInvestmentRoutingReview? Current=>current is null?null:CompanyInventorySourceReview.Copy(current);
    public bool Busy{get;private set;}public string? Error{get;private set;}
    public bool CanView=>company.Selection is {} selected && new[]{"Accounting.View","DevelopmentPlanning.View","Workflow.View","Workflow.Start","Accounting.PrepareDevelopmentBudget"}
        .All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    public bool CanRead=>!Busy && !disposed && CanView && LeaseSummaryValidator.Identifier(processId) && LeaseSummaryValidator.Identifier(afeId);
    public event Action? Changed;
    public DevelopmentInvestmentRoutingState(CompanyClientContext company,IPersonaContextService personas,IAfeServiceClient client,Func<Exception,string,string,string> explain)
    {this.company=company;this.personas=personas;this.client=client;this.explain=explain;company.Changed+=Invalidate;personas.Changed+=Invalidate;}
    private void Invalidate(){++generation;current=null;Error=null;if(!disposed)Changed?.Invoke();}
    public void SetSource(string process,string afe){if(disposed)return;processId=process;afeId=afe;Invalidate();}
    public async Task ReadAsync()
    {
        if(!CanRead)return;Invalidate();var epoch=generation;var selected=company.Snapshot;var process=processId!;var afe=afeId!;
        bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
        Busy=true;Changed?.Invoke();
        try
        {
            var found=await client.ReviewInvestmentRoutingAsync(process,afe);found.Require(selected.Selection!.Scope,selected.Selection.UserId,process,afe);
            if(Active())current=CompanyInventorySourceReview.Copy(found);
        }
        catch(Exception error){var text=explain(error,"reviewing current investment routing","The configured workflow and current independent investment recipients could not be confirmed");if(Active())Error=text;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;Invalidate();}
}
