using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class DrillingExecutionState:IDisposable
{
    private readonly RetainedOriginalFeedback originalFeedback=new();
    public bool PendingTabCopyConfirmed=>pending is not null && originalFeedback.TabCopyConfirmed;
    public bool PendingSubmissionMayHaveBeenSent=>pending is not null && originalFeedback.SubmissionMayHaveBeenSent;
    public string PendingTitle=>originalFeedback.Title("execution start");
    public string? PendingExplanation=>pending is null?null:ConflictAcknowledged && conflict is not null?ConflictCleanupExplanation:originalFeedback.Explanation;
    private readonly CompanyClientContext company;private readonly IPersonaContextService personas;private readonly IDrillingServiceClient client;private readonly IDevelopmentDraftIntentStore journal;private readonly Func<Exception,string,string,string> explain;private bool disposed,ready;private long generation,request;private string? reviewId;private DrillingExecutionStatus? status;private DrillingExecutionBasis? basis;private DrillingExecutionStart? prepared,pending;private DrillingExecutionStartReceipt? receipt;
    public DrillingExecutionStatus? Status=>Copy(status);public DrillingExecutionBasis? Basis=>Copy(basis);public DrillingExecutionStart? Prepared=>Copy(prepared);public DrillingExecutionStart? Pending=>pending?.Basis.ReviewInstanceId==reviewId?Copy(pending):null;public string? RetainedReviewId=>pending?.Basis.ReviewInstanceId;public DrillingExecutionStartReceipt? Receipt=>Copy(receipt);
    private bool reading,writing;public bool Busy=>reading||writing;public string? Error{get;private set;}public string? Confirmation{get;private set;}
    public bool CanView=>company.Selection is {} selected && new[]{"Drilling.View","Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    private bool HasRecorderRights=>CanView && new[]{"Drilling.Create","Drilling.ManageWellProgram"}.All(permission=>company.Selection!.Permissions.Contains(permission,StringComparer.Ordinal));
    private bool ProfileMatches(DrillingExecutionBasis source)=>source.CanRecord && personas.CurrentProfile?.UserId==company.Selection?.UserId && personas.CurrentPersona?.Code==source.PersonaCode && personas.CurrentProfile.PersonaCode==source.PersonaCode && source.RecorderRoles.All(role=>company.Selection!.Roles.Contains(role,StringComparer.Ordinal));
    public bool CanInspect=>!Busy && ready && HasRecorderRights && pending is null && receipt is null && status is {Recorded:null} && ProfileMatches(status.Basis);
    public bool CanPrepare=>CanInspect && basis is not null && ProfileMatches(basis);
    public bool CanRecover=>!Busy && ready && CanView && pending is not null && pending.Basis.ReviewInstanceId==reviewId && pending.Basis.Scope().Company==company.Selection!.Scope && pending.Basis.Scope().ActorUserId==company.Selection.UserId;
    public event Action? Changed;
    public DrillingExecutionState(CompanyClientContext company,IPersonaContextService personas,IDrillingServiceClient client,IDevelopmentDraftIntentStore journal,Func<Exception,string,string,string> explain)
    {this.company=company;this.personas=personas;this.client=client;this.journal=journal;this.explain=explain;company.Changed+=Invalidate;personas.Changed+=Invalidate;}
    private static T? Copy<T>(T? value) where T:class=>value is null?null:CompanyInventorySourceReview.Copy(value);
    private void Invalidate(){++generation;reading=false;ready=false;status=null;basis=null;prepared=null;pending=null;receipt=null;conflict=null;ConflictAcknowledged=false;originalFeedback.Reset();Error=null;Confirmation=null;if(!disposed)Changed?.Invoke();}
    private bool Active((long Version,CompanyClientSelection? Selection) selected,long epoch)=>!disposed && company.Snapshot==selected && generation==epoch;
    private void RequireBasis(DrillingExecutionBasis value)
    {value.Require();var selected=company.Selection??throw new InvalidOperationException("Choose the current execution company.");if(value.Scope().Company!=selected.Scope || value.Scope().ActorUserId!=selected.UserId || value.ReviewInstanceId!=reviewId)throw new InvalidOperationException("The execution source belongs to another company/account/program review.");}
    public async Task LoadAsync(string? requestedReview)
    {
        if(disposed)return;var sequence=++request;reviewId=requestedReview;Invalidate();var selected=company.Snapshot;if(!CanView || selected.Selection is null)return;var epoch=generation;bool Current()=>sequence==request && Active(selected,epoch);reading=true;Changed?.Invoke();
        try
        {
            if(!LeaseSummaryValidator.Identifier(reviewId) || !reviewId.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Open the exact approved program's recorded execution link.");
            var original=await journal.ReadExecutionAsync(selected.Selection.Scope,selected.Selection.UserId);if(!Current())return;if(original is not null){original.Require();if(original.Basis.Scope().Company!=selected.Selection.Scope || original.Basis.Scope().ActorUserId!=selected.Selection.UserId)throw new InvalidOperationException("The retained execution belongs to another company/account.");pending=Copy(original);originalFeedback.ReadFromTab();ready=true;return;}
            var found=await client.GetExecutionStatusAsync(reviewId);if(!Current())return;found.Require();RequireBasis(found.Basis);
            if(HasRecorderRights){await personas.EnsureLoadedAsync();if(disposed || selected!=company.Snapshot || sequence!=request)return;epoch=generation;}
            if(Current()){status=Copy(found);ready=true;}
        }
        catch(Exception error){var text=explain(error,"loading original program execution","The exact approved program and recorded execution or absence could not be confirmed");if(Current()){status=null;basis=null;Error=text;}}
        finally{if(Current()){reading=false;Changed?.Invoke();}}
    }
    public async Task InspectAsync()
    {
        if(!CanInspect)return;var selected=company.Snapshot;var epoch=generation;reading=true;Error=null;basis=null;prepared=null;Changed?.Invoke();
        try{var found=await client.GetExecutionBasisAsync(reviewId!);if(!Active(selected,epoch))return;RequireBasis(found);status!.Basis.RequireSource(found,true);if(!ProfileMatches(found))throw new InvalidOperationException("Your current execution role and mapped workspace must match this source.");basis=Copy(found);}
        catch(Exception error){var text=explain(error,"reviewing approved execution start source","The approved program/current active well and execution recorder could not be confirmed");if(Active(selected,epoch))Error=text;}
        finally{if(Active(selected,epoch)){reading=false;Changed?.Invoke();}}
    }
    public void Prepare(string? role,string? spudUtc,string? rationale)
    {
        if(!CanPrepare)return;Error=null;prepared=null;try{if(!ExplicitUtcInput.TryParse(spudUtc,out var utc))throw new InvalidOperationException("Enter an explicit actual UTC spud timestamp ending in Z or +00:00.");if(utc>DateTime.UtcNow)throw new InvalidOperationException("Actual spud UTC cannot be in the future.");var original=new DrillingExecutionStart(Copy(basis)!,Guid.NewGuid(),role??"",new DateTimeOffset(utc),rationale?.Trim()??"");original.Require();prepared=original;}catch(Exception error){Error=explain(error,"preparing actual execution start","Choose your current role and report actual spud UTC with its recorded rationale");}Changed?.Invoke();
    }
    public void Cancel(){if(Busy || pending is not null)return;prepared=null;Changed?.Invoke();}
    public Task SubmitAsync()=>Record(false);public Task RecoverAsync()=>Record(true);
    private async Task Record(bool recover)
    {
        if(disposed || (recover?!CanRecover:!CanPrepare || prepared is null))return;var original=Copy(recover?pending:prepared)!;var selected=company.Snapshot;var epoch=generation;bool Current()=>Active(selected,epoch);writing=true;Error=null;Confirmation=null;conflict=null;ConflictAcknowledged=false;if(!recover){receipt=null;pending=original;prepared=null;originalFeedback.BeginSaving();}var failureExplanation=originalFeedback.FailureExplanation;Changed?.Invoke();
        try
        {
            if(!recover){if(!Current())return;await journal.RetainExecutionAsync(original);if(!Current())return;originalFeedback.ConfirmSaved();originalFeedback.AttemptSubmission();failureExplanation=originalFeedback.FailureExplanation;Changed?.Invoke();if(!Current())return;}
            var result=recover?await client.RecoverExecutionStartAsync(original):await client.StartExecutionAsync(original);result.Require(original);if(!recover && result.Recorded is null)throw new InvalidOperationException("The original execution start is not confirmed recorded.");if(!Current())return;
            receipt=Copy(result);basis=null;status=null;originalFeedback.ConfirmReceiptAndBeginClear(result.Recorded is not null);failureExplanation=originalFeedback.FailureExplanation;Changed?.Invoke();if(!Current())return;
            await journal.ClearExecutionAsync(original);if(!Current())return;pending=null;originalFeedback.Reset();
            Confirmation=result.Recorded is null?"The original execution start was confirmed absent. Reload the current source before another confirmation.":"The exact original execution start was confirmed recorded. WELL state, financial actuals, completion and production handover remain separate.";
        }
        catch(Exception error){var text=explain(error,recover?"recovering original execution start":"recording actual execution start",failureExplanation);if(Current()){originalFeedback.Failed();Error=text;}}
        finally{writing=false;if(!disposed)Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;++request;Invalidate();}
}
