using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed class DevelopmentInvestmentQueueState:IDisposable
{
    private readonly CompanyClientContext company;private readonly IPersonaContextService personas;private readonly IAfeServiceClient client;private readonly Func<Exception,string,string,string> explain;
    private DevelopmentInvestmentQueue? current;private long generation;private bool disposed,reloadRequested;
    public DevelopmentInvestmentQueue? Current=>current is null?null:JsonSerializer.Deserialize<DevelopmentInvestmentQueue>(JsonSerializer.Serialize(current));
    public bool Busy{get;private set;}public string? Error{get;private set;}
    public bool CanView=>company.Selection is {} selected && new[]{"Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    public event Action? Changed;
    public DevelopmentInvestmentQueueState(CompanyClientContext company,IPersonaContextService personas,IAfeServiceClient client,Func<Exception,string,string,string> explain)
    {this.company=company;this.personas=personas;this.client=client;this.explain=explain;company.Changed+=Invalidate;personas.Changed+=Invalidate;}
    private void Invalidate(){++generation;current=null;Error=null;Changed?.Invoke();}
    public async Task LoadAsync()
    {
        if(disposed)return;Invalidate();if(!CanView)return;if(Busy){reloadRequested=true;return;}Busy=true;Changed?.Invoke();
        try
        {
            do
            {
                reloadRequested=false;if(!CanView || disposed)break;var epoch=generation;var selected=company.Snapshot;Error=null;
                bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
                try{var found=await client.QueueAsync();found.Require(selected.Selection!.Scope,selected.Selection.UserId);if(Active())current=JsonSerializer.Deserialize<DevelopmentInvestmentQueue>(JsonSerializer.Serialize(found));}
                catch(Exception error){var text=explain(error,"loading company development investment queue","The current source-bound investment queue could not be confirmed");if(Active())Error=text;}
            }while(reloadRequested && !disposed);
        }
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;Invalidate();}
}
