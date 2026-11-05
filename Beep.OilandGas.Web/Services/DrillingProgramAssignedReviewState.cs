using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class DrillingProgramAssignedReviewState:IDisposable
{
    private readonly CompanyClientContext company;private readonly IPersonaContextService personas;private readonly IDrillingServiceClient client;private readonly IDevelopmentDraftIntentStore journal;private readonly Func<Exception,string,string,string> explain;private bool disposed;private long generation,request;private DrillingProgramAssignedReview? current;private DrillingProgramReviewProgress? progress;
    public DrillingProgramAssignedReview? Current=>current is null?null:CompanyInventorySourceReview.Copy(current);
    public DrillingProgramReviewProgress? Progress=>progress is null?null:CompanyInventorySourceReview.Copy(progress);
    public bool Loading{get;private set;}public string? Error{get;private set;}
    public bool CanRead=>company.Selection is {} selected && new[]{"Drilling.View","Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    public bool CanView=>company.Selection is {} selected && new[]{"Drilling.View","Drilling.Approve","Accounting.View","DevelopmentPlanning.View","Workflow.View","Workflow.Approve"}.All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    public event Action? Changed;
    public DrillingProgramAssignedReviewState(CompanyClientContext company,IPersonaContextService personas,IDrillingServiceClient client,IDevelopmentDraftIntentStore journal,Func<Exception,string,string,string> explain)
    {this.company=company;this.personas=personas;this.client=client;this.journal=journal;this.explain=explain;company.Changed+=Invalidate;personas.Changed+=Invalidate;}
    private void Invalidate(){++generation;current=null;progress=null;ResetDecision();Error=null;Loading=false;if(!disposed)Changed?.Invoke();}
    public async Task LoadAsync(string? processId)
    {
        if(disposed)return;RequestedProcessId=processId;var sequence=++request;Invalidate();var selected=company.Snapshot;if(!CanRead || selected.Selection is null)return;var epoch=generation;string? persona=null;bool profileReady=false;
        bool Active()=>!disposed && sequence==request && epoch==generation && selected==company.Snapshot && (!profileReady || personas.CurrentPersona?.Code==persona);
        Loading=true;Changed?.Invoke();
        try
        {
            if(!LeaseSummaryValidator.Identifier(processId) || !processId.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Choose an exact assigned program review from the company inbox.");
            var retained=await journal.ReadProgramDecisionAsync(selected.Selection.Scope,selected.Selection.UserId);if(!Active())return;
            if(retained is not null){retained.Require();if(retained.Scope().Company!=selected.Selection.Scope || retained.Scope().ActorUserId!=selected.Selection.UserId)throw new InvalidOperationException("The retained program decision belongs to another company/account.");pendingDecision=CompanyInventorySourceReview.Copy(retained);return;}
            var outcome=await client.GetProgramReviewProgressAsync(processId)??throw new InvalidOperationException("The complete original program outcome is unavailable.");if(!Active())return;outcome.Require(selected.Selection.Scope,selected.Selection.UserId,processId);progress=CompanyInventorySourceReview.Copy(outcome);
            if(!CanView || outcome.State!="IN_REVIEW")return;
            await personas.EnsureLoadedAsync();if(disposed || sequence!=request || selected!=company.Snapshot)return;epoch=generation;persona=personas.CurrentPersona?.Code;profileReady=true;Loading=true;Changed?.Invoke();
            progress=CompanyInventorySourceReview.Copy(outcome);
            if(persona is null || personas.CurrentProfile?.UserId!=selected.Selection.UserId || personas.CurrentProfile.PersonaCode!=persona)return;
            var activeStage=outcome.Stages.Single(row=>row.State=="PENDING");var assignedStage=outcome.Original.Workflow.Stages.Single(stage=>stage.Definition.StepId==activeStage.StepId);
            if(!selected.Selection.Roles.Contains(activeStage.AssignedRole,StringComparer.Ordinal) || !assignedStage.Authority.Recipients.Any(row=>row.UserId==selected.Selection.UserId && row.PersonaCode==persona))return;
            var result=await client.GetAssignedProgramReviewAsync(processId);if(!Active())return;result.Require(selected.Selection.Scope,selected.Selection.UserId,processId);if(result.NativeRevision!=outcome.NativeRevision || result.Source.Revision!=outcome.Source.Revision || result.Original.Revision()!=outcome.Original.Revision() || result.PersonaCode!=persona || !selected.Selection.Roles.Contains(result.AssignedRole,StringComparer.Ordinal))throw new InvalidOperationException("The program task belongs to another current role or workspace.");current=CompanyInventorySourceReview.Copy(result);
        }
        catch(Exception error){var text=explain(error,"opening assigned program review","The exact assigned task, saved program and original funding/well could not be confirmed");if(Active()){current=null;progress=null;Error=text;}}
        finally{if(Active()){Loading=false;Changed?.Invoke();}}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;++request;Invalidate();}
}
