using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public sealed partial class DevelopmentInvestmentAssignedState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly IPersonaContextService personas;
    private readonly IAfeServiceClient client;
    private readonly IDevelopmentDraftIntentStore journal;
    private readonly Func<Exception,string,string,string> explain;
    private DevelopmentInvestmentAssignedReview? current;
    private long generation,requestVersion;
    private bool disposed;
    public DevelopmentInvestmentAssignedReview? Current=>current is null?null:CompanyInventorySourceReview.Copy(current);
    public bool Loading{get;private set;}
    public string? Error{get;private set;}
    public bool CanReview=>company.Selection is {} selected && new[]{"Accounting.View","DevelopmentPlanning.View","Workflow.View","Workflow.Approve","Accounting.ReviewDevelopmentBudget"}
        .All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    public bool CanView=>company.Selection is {} selected && new[]{"Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    public event Action? Changed;
    public DevelopmentInvestmentAssignedState(CompanyClientContext company,IPersonaContextService personas,IAfeServiceClient client,IDevelopmentDraftIntentStore journal,Func<Exception,string,string,string> explain)
    {
        this.company=company;this.personas=personas;this.client=client;this.journal=journal;this.explain=explain;
        company.Changed+=Invalidate;personas.Changed+=Invalidate;
    }
    private void Invalidate(){++generation;current=null;Loading=false;Error=null;ResetDecision();if(!disposed)Changed?.Invoke();}
    public async Task LoadAsync(string? instanceId)
    {
        if(disposed)return;var request=++requestVersion;Invalidate();var selection=company.Snapshot;
        RequestedInstanceId=instanceId;if(!CanView || deciding)return;
        var epoch=generation;string? persona=null;var loaded=false;
        bool Active()=>!disposed && request==requestVersion && selection==company.Snapshot && (!loaded || epoch==generation && persona==personas.CurrentPersona?.Code);
        Loading=true;Changed?.Invoke();
        try
        {
            if(!LeaseSummaryValidator.Identifier(instanceId) || !instanceId.StartsWith(DevelopmentInvestmentHandoff.InstancePrefix,StringComparison.Ordinal))
                throw new InvalidOperationException("Choose an assigned investment review from the company inbox.");
            var retained=await journal.ReadInvestmentStageAsync(selection.Selection!.Scope,selection.Selection.UserId);if(!Active())return;
            if(retained is not null){retained.Require();if(retained.Scope().Company!=selection.Selection.Scope || retained.Scope().ActorUserId!=selection.Selection.UserId)throw new InvalidOperationException("The retained stage belongs to another original company or actor.");pending=CompanyInventorySourceReview.Copy(retained);}
            journalReady=true;if(pending?.Review.InstanceId==instanceId || !CanReview)return;
            await personas.EnsureLoadedAsync();if(!Active())return;loaded=true;epoch=generation;persona=personas.CurrentPersona?.Code;Loading=true;pending=retained is null?null:CompanyInventorySourceReview.Copy(retained);journalReady=true;
            if(persona is null || personas.CurrentProfile?.UserId!=selection.Selection!.UserId || personas.CurrentProfile.PersonaCode!=persona)
                throw new InvalidOperationException("Load the current company receiving workspace before opening this investment review.");
            var result=CompanyInventorySourceReview.Copy(await client.ReadAssignedInvestmentAsync(instanceId!));if(!Active())return;
            result.Require(selection.Selection!.Scope,selection.Selection.UserId,instanceId!);
            if(result.PersonaCode!=persona || !selection.Selection.Roles.Contains(result.AssignedRole,StringComparer.Ordinal))
                throw new InvalidOperationException("The investment review belongs to another role or workspace.");
            current=result;
        }
        catch(Exception error){var text=explain(error,"opening assigned investment review","The assigned investment task and exact AFE source could not be confirmed");if(Active())Error=text;}
        finally{if(Active()){Loading=false;Changed?.Invoke();}}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;++requestVersion;Invalidate();}
}
