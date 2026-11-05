using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;

public sealed partial class DrillingCompletionAssignedState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly IPersonaContextService personas;
    private readonly IDrillingServiceClient client;
    private readonly Func<Exception,string,string,string> explain;
    private bool disposed;private long generation,request;
    private DrillingCompletionAssignedReview? current;
    public DrillingCompletionAssignedReview? Current=>current is null?null:CompanyInventorySourceReview.Copy(current);
    private bool loading,writing;
    public bool Busy=>loading || writing || inspectingTest;
    public string? Error{get;private set;}
    public event Action? Changed;
    public DrillingCompletionAssignedState(CompanyClientContext company,IPersonaContextService personas,IDrillingServiceClient client,IDevelopmentDraftIntentStore journal,Func<Exception,string,string,string> explain)
    {this.company=company;this.personas=personas;this.client=client;this.journal=journal;this.explain=explain;company.Changed+=Invalidate;personas.Changed+=Invalidate;}
    private void Invalidate(){++generation;current=null;Error=null;loading=false;ResetDecision();ResetTestInspection();ResetAcceptanceDraft();if(!disposed)Changed?.Invoke();}
    public async Task LoadAsync(string? processId)
    {
        if(disposed)return;var sequence=++request;Invalidate();RequestedProcessId=processId;var selected=company.Snapshot;var epoch=generation;
        if(selected.Selection is null || !CanRead || writing)return;bool Active()=>!disposed && sequence==request && generation==epoch && selected==company.Snapshot;
        loading=true;Changed?.Invoke();
        try
        {
            if(!LeaseSummaryValidator.Identifier(processId) || !processId.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal))
                throw new InvalidOperationException("Open an exact completion review from the company inbox.");
            var saved=await journal.ReadCompletionDecisionAsync(selected.Selection.Scope,selected.Selection.UserId);if(!Active())return;
            if(saved is not null){pendingDecision=CompanyInventorySourceReview.Copy(saved);feedback.ReadFromTab();return;}
            await personas.EnsureLoadedAsync();if(disposed || sequence!=request || selected!=company.Snapshot)return;epoch=generation;
            var found=await client.GetAssignedCompletionReviewAsync(processId);if(!Active())return;found.Require(selected.Selection.Scope,selected.Selection.UserId,processId);
            if(!selected.Selection.Roles.Contains(found.AssignedRole,StringComparer.Ordinal) || personas.CurrentProfile?.UserId!=selected.Selection.UserId ||
                personas.CurrentProfile?.PersonaCode!=found.PersonaCode || personas.CurrentPersona?.Code!=found.PersonaCode)
                throw new InvalidOperationException("The original completion task must match your current configured role and workspace.");
            current=CompanyInventorySourceReview.Copy(found);
        }
        catch(Exception error){var text=explain(error,"loading assigned independent completion review","The individual completion review and original execution could not be confirmed");if(Active()){current=null;Error=text;}}
        finally{if(Active()){loading=false;Changed?.Invoke();}}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;++request;Invalidate();}
}
