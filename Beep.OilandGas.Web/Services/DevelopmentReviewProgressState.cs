using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

/// <summary>Current read-only review history. Never depends on a retained command or pending assignment.</summary>
public sealed class DevelopmentReviewProgressState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly IPersonaContextService personas;
    private readonly IDevelopmentServiceClient client;
    private readonly Func<Exception,string,string,string> explain;
    private DevelopmentReviewProgress? current;
    private long generation;
    private bool disposed;
    public DevelopmentReviewProgress? Current=>current is null?null:JsonSerializer.Deserialize<DevelopmentReviewProgress>(JsonSerializer.Serialize(current));
    public bool NeedsCompany=>company.Selection is null;
    public bool CanView=>company.Selection is {} selected && new[]{"DevelopmentPlanning.View","Workflow.View"}.All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    public bool Loading{get;private set;}
    public string? Error{get;private set;}
    public event Action? Changed;
    public DevelopmentReviewProgressState(CompanyClientContext company,IPersonaContextService personas,IDevelopmentServiceClient client,Func<Exception,string,string,string> explain)
    {this.company=company;this.personas=personas;this.client=client;this.explain=explain;company.Changed+=Invalidate;personas.Changed+=Invalidate;}
    private void Invalidate(){++generation;current=null;Loading=false;Error=null;Changed?.Invoke();}
    public async Task LoadAsync(string? processId)
    {
        if(disposed)return;Invalidate();var epoch=generation;var selected=company.Snapshot;
        bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
        if(selected.Selection is null || !CanView)return;Loading=true;Changed?.Invoke();
        try
        {
            if(!LeaseSummaryValidator.Identifier(processId))throw new InvalidOperationException("Choose a recorded development review from its source or assignment.");
            var result=await client.GetDraftReviewProgressAsync(processId!);if(!Active())return;
            result.Require(selected.Selection.Scope,selected.Selection.UserId,processId);current=JsonSerializer.Deserialize<DevelopmentReviewProgress>(JsonSerializer.Serialize(result));
        }
        catch(Exception error){var text=explain(error,"reading recorded development review history","The recorded review outcome and audit could not be confirmed");if(Active())Error=text;}
        finally{if(Active()){Loading=false;Changed?.Invoke();}}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;Invalidate();}
}
