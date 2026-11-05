using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;
public sealed class DrillingCompletionProgressState:IDisposable
{
    private readonly CompanyClientContext company;private readonly IPersonaContextService personas;private readonly IDrillingServiceClient client;private readonly Func<Exception,string,string,string> explain;
    private bool disposed;private long generation,request;private DrillingCompletionReviewProgress? current;
    public DrillingCompletionReviewProgress? Current=>current is null?null:CompanyInventorySourceReview.Copy(current);
    public bool Busy{get;private set;}public string? Error{get;private set;}public event Action? Changed;
    public DrillingCompletionProgressState(CompanyClientContext company,IPersonaContextService personas,IDrillingServiceClient client,Func<Exception,string,string,string> explain)
    {this.company=company;this.personas=personas;this.client=client;this.explain=explain;company.Changed+=Invalidate;personas.Changed+=Invalidate;}
    private void Invalidate(){++generation;current=null;Error=null;Busy=false;if(!disposed)Changed?.Invoke();}
    public async Task LoadAsync(string? processId)
    {
        if(disposed)return;var sequence=++request;Invalidate();var captured=company.Snapshot;var epoch=generation;
        if(captured.Selection is null)return;bool Active()=>!disposed && generation==epoch && sequence==request && company.Snapshot==captured;
        Busy=true;Changed?.Invoke();
        try
        {
            if(!LeaseSummaryValidator.Identifier(processId) || !processId.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Open an exact recorded completion review history.");
            var found=await client.GetCompletionReviewProgressAsync(processId);if(!Active())return;found.Require(captured.Selection.Scope,captured.Selection.UserId,processId);current=CompanyInventorySourceReview.Copy(found);
        }
        catch(Exception error){var message=explain(error,"loading complete completion review history","The original claim, complete ordered decisions and current source could not be confirmed");if(Active()){Error=message;current=null;}}
        finally{if(Active()){Busy=false;Changed?.Invoke();}}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;++request;Invalidate();}
}
