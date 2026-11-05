using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>Read-only financial evidence; decisions belong to the assigned investment stage.</summary>
public sealed class DevelopmentInvestmentState:IDisposable
{
    private readonly CompanyClientContext company;private readonly IPersonaContextService personas;private readonly IAfeServiceClient client;private readonly Func<Exception,string,string,string> explain;
    private string? processId,afeId;private DevelopmentInvestmentReview? review;private long generation;private bool disposed;
    public bool Busy{get;private set;}public string? Error{get;private set;}
    public DevelopmentInvestmentReview? Review=>review is null?null:CompanyInventorySourceReview.Copy(review);
    public bool CanView=>company.Selection is {} selected && new[]{"Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    public bool CanInspect=>!Busy && CanView && LeaseSummaryValidator.Identifier(processId) && LeaseSummaryValidator.Identifier(afeId);
    public event Action? Changed;
    public DevelopmentInvestmentState(CompanyClientContext company,IPersonaContextService personas,IAfeServiceClient client,Func<Exception,string,string,string> explain)
    {this.company=company;this.personas=personas;this.client=client;this.explain=explain;company.Changed+=Invalidate;personas.Changed+=Invalidate;}
    private void Invalidate(){++generation;review=null;Error=null;if(!disposed)Changed?.Invoke();}
    public Task LoadAsync(string process,string afe)
    {if(disposed)return Task.CompletedTask;Invalidate();processId=process;afeId=afe;return Task.CompletedTask;}
    public async Task InspectAsync()
    {
        if(disposed || !CanInspect)return;var epoch=generation;var selected=company.Snapshot;var source=processId!;var afe=afeId!;Busy=true;review=null;Error=null;Changed?.Invoke();bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
        try
        {
            var found=await client.ReviewInvestmentAsync(source,afe);found.Require();var scope=found.Current.Source.Review.Scope;
            if(scope.Company!=selected.Selection!.Scope || scope.ActorUserId!=selected.Selection.UserId || found.Current.Source.Review.InstanceId!=source || found.AfeId!=afe)throw new InvalidOperationException("The investment evidence belongs to another original account, company, source or AFE.");if(Active())review=CompanyInventorySourceReview.Copy(found);
        }
        catch(Exception error){var text=explain(error,"inspecting recorded investment decision","The original investment source or recorded outcome could not be confirmed");if(Active())Error=text;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;Invalidate();}
}
