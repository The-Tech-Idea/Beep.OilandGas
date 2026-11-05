using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Client.App;

namespace Beep.OilandGas.Web.Services;

/// <summary>Task-bound reviewer evidence. The active FIELD preference is deliberately independent.</summary>
public sealed partial class DevelopmentAssignedReviewState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly IPersonaContextService personas;
    private readonly IDevelopmentServiceClient client;
    private readonly Func<Exception,string,string,string> explain;
    private readonly IDevelopmentDraftIntentStore journal;
    private long requestVersion,generation;
    private bool disposed;
    private DevelopmentAssignedReview? current;
    public DevelopmentAssignedReview? Current=>current is null?null:Copy(current);
    public bool Loading{get;private set;}
    public string? Error{get;private set;}
    public bool NeedsCompany=>company.Selection is null;
    public bool CanView=>company.Selection is {} selected && new[]{"DevelopmentPlanning.View","DevelopmentPlanning.Approve","Workflow.View","Workflow.Approve"}
        .All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    public event Action? Changed;
    private static T Copy<T>(T value)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
    public DevelopmentAssignedReviewState(CompanyClientContext company,IPersonaContextService personas,IDevelopmentServiceClient client,IDevelopmentDraftIntentStore journal,Func<Exception,string,string,string> explain)
    {
        this.company=company;this.personas=personas;this.client=client;this.journal=journal;this.explain=explain;
        company.Changed+=CompanyChanged;personas.Changed+=PersonaChanged;
    }
    private void Invalidate(){++generation;current=null;Error=null;Loading=false;ResetDecision();Changed?.Invoke();}
    private void CompanyChanged(){++requestVersion;Invalidate();}
    private void PersonaChanged()=>Invalidate();
    public async Task LoadAsync(string? processId)
    {
        if(disposed)return;var request=++requestVersion;Invalidate();RequestedProcessId=processId;
        if(deciding)return;
        var selected=company.Snapshot;var epoch=generation;string? persona=null;var profileReady=false;
        bool CurrentRequest()=>!disposed && request==requestVersion && selected==company.Snapshot && (!profileReady || epoch==generation && (persona is null || personas.CurrentPersona?.Code==persona));
        if(selected.Selection is null || !CanView)return;
        Loading=true;Changed?.Invoke();
        try
        {
            if(!LeaseSummaryValidator.Identifier(processId))throw new InvalidOperationException("Choose an assigned development review from the company inbox.");
            await personas.EnsureLoadedAsync();
            if(disposed || request!=requestVersion || selected!=company.Snapshot)return;
            profileReady=true;epoch=generation;persona=personas.CurrentPersona?.Code;Loading=true;Changed?.Invoke();
            if(persona is null || personas.CurrentProfile?.UserId!=selected.Selection.UserId || personas.CurrentProfile.PersonaCode!=persona)
                throw new InvalidOperationException("Load the current assigned company workspace before opening this review.");
            var retained=await journal.ReadDecisionAsync(selected.Selection.Scope,selected.Selection.UserId);if(!CurrentRequest())return;
            if(retained is not null)
            {
                retained.Require();if(retained.Review.Scope.Company!=selected.Selection.Scope || retained.Review.Scope.ActorUserId!=selected.Selection.UserId)throw new InvalidOperationException("The retained decision belongs to another original actor or company.");
                pendingDecision=retained;
                // A recorded decision can close this task; original-outcome recovery must not depend on a still-pending read.
                if(retained.Review.InstanceId==processId)return;
            }
            var result=Copy(await client.GetAssignedDraftReviewAsync(processId));if(!CurrentRequest())return;
            result.Require(selected.Selection.Scope,selected.Selection.UserId,processId!);
            if(result.PersonaCode!=persona || !selected.Selection.Roles.Contains(result.AssignedRole,StringComparer.Ordinal))
                throw new InvalidOperationException("The development review belongs to another current role or workspace.");
            current=result;
        }
        catch(Exception error){var text=explain(error,"opening assigned development review","The assigned development task and original source could not be confirmed");if(CurrentRequest())Error=text;}
        finally{if(CurrentRequest()){Loading=false;Changed?.Invoke();}}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=CompanyChanged;personas.Changed-=PersonaChanged;++requestVersion;Invalidate();}
}
