using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

public sealed class DrillingExecutionAssignedState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly IPersonaContextService personas;
    private readonly IDrillingServiceClient client;
    private readonly Func<Exception,string,string,string> explain;
    private bool disposed;private long generation,request;
    private DrillingExecutionAssignedWork? current;
    public DrillingExecutionAssignedWork? Current=>current is null?null:CompanyInventorySourceReview.Copy(current);
    public bool Busy{get;private set;}
    public string? Error{get;private set;}
    public event Action? Changed;
    public DrillingExecutionAssignedState(CompanyClientContext company,IPersonaContextService personas,IDrillingServiceClient client,Func<Exception,string,string,string> explain)
    {this.company=company;this.personas=personas;this.client=client;this.explain=explain;company.Changed+=Invalidate;personas.Changed+=Invalidate;}
    private void Invalidate(){++generation;current=null;Error=null;Busy=false;if(!disposed)Changed?.Invoke();}
    public async Task LoadAsync(string? processId)
    {
        if(disposed)return;var sequence=++request;Invalidate();var selected=company.Snapshot;var epoch=generation;
        if(selected.Selection is null)return;bool Active()=>!disposed && sequence==request && generation==epoch && selected==company.Snapshot;
        Busy=true;Changed?.Invoke();
        try
        {
            if(!LeaseSummaryValidator.Identifier(processId) || !processId.StartsWith(DrillingExecutionAssignment.InstancePrefix,StringComparison.Ordinal))
                throw new InvalidOperationException("Open an exact reporting responsibility from the company inbox.");
            await personas.EnsureLoadedAsync();if(disposed || sequence!=request || selected!=company.Snapshot)return;epoch=generation;
            var found=await client.GetAssignedExecutionAsync(processId);if(!Active())return;found.Require(selected.Selection.Scope,selected.Selection.UserId,processId);
            if(!selected.Selection.Roles.Contains(found.AssignedRole,StringComparer.Ordinal) || personas.CurrentProfile?.UserId!=selected.Selection.UserId ||
                personas.CurrentProfile?.PersonaCode!=found.PersonaCode || personas.CurrentPersona?.Code!=found.PersonaCode)
                throw new InvalidOperationException("The original reporting task must match your current configured role and workspace.");
            current=CompanyInventorySourceReview.Copy(found);
        }
        catch(Exception error){var text=explain(error,"loading assigned execution reporting","The individual reporting responsibility and original execution could not be confirmed");if(Active()){current=null;Error=text;}}
        finally{if(Active()){Busy=false;Changed?.Invoke();}}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;++request;Invalidate();}
}
