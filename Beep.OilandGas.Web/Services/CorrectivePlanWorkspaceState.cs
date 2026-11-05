using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.HSE.Models;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.App.Services.Field;
using Beep.OilandGas.Models.Data.Security;
namespace Beep.OilandGas.Web.Services;

public sealed class CorrectivePlanWorkspaceState(CompanyClientContext company,IFieldSelectionContext fields,IFieldService api,ICorrectivePlanIntentStore store,Func<Exception,string,string,string> explain):IDisposable
{
    private long version;private bool disposed,started;private CAPlanCreationReview? review;private FieldSelectionReview? context;private CorrectivePlanOriginal? pending;private RecordedIncidentParticipant? participant;
    public string IncidentId { get; private set; }="";
    public CAPlanCreationReview? Review=>review;
    public CorrectivePlanOriginal? Pending=>pending;
    public RecordedIncidentParticipant? Participant=>participant;
    public bool Busy { get; private set; }
    public bool NeedsCompany=>company.Selection is null;
    public bool NeedsField { get; private set; }
    public string? Error { get; private set; }
    public string? Message { get; private set; }
    public event Action? Changed;
    public bool CanPrepare=>!disposed && !Busy && pending is null && review is not null && participant is not null && company.Selection is {} selected && selected.Scope==review.Context.Company && selected.UserId==review.Context.ActorUserId && selected.Permissions.Contains(PermissionConstants.HSE.ManageIncidents,StringComparer.Ordinal);
    public bool CanRetry=>!disposed && !Busy && pending is not null && context==pending.Submission.Context && company.Selection is {} selected && selected.Scope==context.Company && selected.UserId==context.ActorUserId && selected.Permissions.Contains(PermissionConstants.HSE.ManageIncidents,StringComparer.Ordinal);
    public void Start(){if(disposed || started)return;started=true;company.Changed+=Invalidate;fields.CurrentFieldChanged+=FieldChanged;}
    private void FieldChanged(string _) => Invalidate();
    private void Invalidate(){++version;review=null;context=null;pending=null;participant=null;NeedsField=false;Busy=false;Error=null;Message=null;if(!disposed)Changed?.Invoke();}
    public void Select(RecordedIncidentParticipant choice){if(!Busy && pending is null && review is not null && review.Participants.Any(p=>ReferenceEquals(p,choice))){participant=choice;Changed?.Invoke();}}
    public async Task LoadAsync(string incident)
    {
        if(disposed)return;Invalidate();IncidentId=incident;var captured=company.Snapshot;var current=version;if(captured.Selection is null){Changed?.Invoke();return;}Busy=true;Changed?.Invoke();
        bool Current()=>!disposed && version==current && company.Snapshot==captured;
        try
        {
            if(!LeaseSummaryValidator.Identifier(incident))throw new InvalidOperationException("Select an exact incident.");
            var selected=await fields.GetCurrentFieldSelectionAsync();if(!Current())return;FieldSelectionReviewValidator.RequireCurrent(selected,captured.Selection.Scope,captured.Selection.UserId);if(!selected.SelectionAvailable){NeedsField=true;return;}
            context=selected.SelectionContext!;pending=await store.ReadAsync(context,incident);if(!Current())return;
            if(pending is not null)
            {
                var afterRetention=await fields.GetCurrentFieldSelectionAsync();if(!Current())return;FieldSelectionReviewValidator.RequireCurrent(afterRetention,captured.Selection.Scope,captured.Selection.UserId);
                if(!afterRetention.SelectionAvailable || afterRetention.SelectionContext!=context)throw new InvalidOperationException("The FIELD changed during original recovery.");
                Message="The original plan submission is retained. Review its current link or retry the same original.";return;
            }
            var source=await api.ReviewCorrectiveActionPlanCreationAsync(incident,context.FieldId!);if(!Current())return;
            InvestigatorAssignmentReviewValidator.Require(new(source.Context,source.IncidentId,source.Revision,source.Participants,null),captured.Selection.Scope,captured.Selection.UserId,incident,context.FieldId!);
            if(source.Context!=context || source.ResponseTypes is null || source.ResponseTypes.Count is < 1 or >256 || source.ResponseTypes.Any(x=>!LeaseSummaryValidator.Identifier(x) || x=="STATUS") || source.ResponseTypes.Distinct(StringComparer.Ordinal).Count()!=source.ResponseTypes.Count)throw new InvalidOperationException("The plan review choices or FIELD changed.");
            var after=await fields.GetCurrentFieldSelectionAsync();if(!Current())return;FieldSelectionReviewValidator.RequireCurrent(after,captured.Selection.Scope,captured.Selection.UserId);if(!after.SelectionAvailable || after.SelectionContext!=context)throw new InvalidOperationException("The FIELD changed during review.");
            review=source with{Participants=Array.AsReadOnly(source.Participants.Select(x=>x with{}).ToArray()),ResponseTypes=Array.AsReadOnly(source.ResponseTypes.ToArray())};
        }
        catch(Exception error){if(Current()){review=null;pending=null;context=null;Error=explain(error,"reviewing a corrective action plan","Corrective action evidence could not be confirmed");}}
        finally{if(Current()){Busy=false;Changed?.Invoke();}}
    }
    public async Task SubmitAsync(string name,string number,string responseType,bool confirmed)
    {
        if(!CanPrepare || !confirmed)return;
        try{var intent=new CreateCAPlanRequest(Guid.NewGuid(),name?.Trim()??"",number?.Trim()??"",responseType,participant!.PartyObservation,participant.RoleObservation,participant.BusinessAssociateId,review!.Revision);var original=new CorrectivePlanOriginal(review,new(review.Context,intent));original.Require();pending=original;review=null;participant=null;await SendAsync();}
        catch(Exception error){Error=explain(error,"preparing a corrective action plan","The original plan could not be prepared");Changed?.Invoke();}
    }
    public Task RetryAsync()=>SendAsync();
    private async Task SendAsync()
    {
        if(!CanRetry || pending is null)return;var original=pending;var captured=company.Snapshot;var current=version;Busy=true;Error=null;Message=null;Changed?.Invoke();bool Current()=>!disposed && version==current && company.Snapshot==captured;
        try
        {
            var selected=await fields.GetCurrentFieldSelectionAsync();if(!Current())return;FieldSelectionReviewValidator.RequireCurrent(selected,captured.Selection!.Scope,captured.Selection.UserId);if(!selected.SelectionAvailable || selected.SelectionContext!=original.Submission.Context)throw new InvalidOperationException("The reviewed FIELD changed.");
            await store.RetainAsync(original);if(!Current())return;
            var result=await api.CreateCorrectiveActionPlanAsync(IncidentId,original.Submission);if(!Current())return;
            if(result.Context!=original.Submission.Context || result.IncidentId!=IncidentId || result.PlanId!=original.Submission.Intent.PlanId.ToString("N"))throw new InvalidOperationException("The saved plan identity was not confirmed.");
            await store.ClearAsync(original);if(!Current())return;pending=null;Message="The original corrective action plan was confirmed. Open the incident to review its recorded plan.";
        }
        catch(Exception error){if(Current())Error=explain(error,"submitting the corrective action plan","The original plan outcome is unconfirmed. Review or retry the same original");}
        finally{if(Current()){Busy=false;Changed?.Invoke();}}
    }
    public async Task ReviewLinkAsync()
    {
        if(disposed || Busy || pending is null || context is null)return;var captured=company.Snapshot;var current=version;var original=pending;Busy=true;Error=null;Changed?.Invoke();bool Current()=>!disposed && current==version && captured==company.Snapshot;
        try{var selected=await fields.GetCurrentFieldSelectionAsync();if(!Current())return;FieldSelectionReviewValidator.RequireCurrent(selected,captured.Selection!.Scope,captured.Selection.UserId);if(!selected.SelectionAvailable || selected.SelectionContext!=context)throw new InvalidOperationException("The current FIELD changed.");var result=await api.GetCorrectiveActionPlanAsync(IncidentId,context);if(!Current())return;if(result.Context!=context || result.IncidentId!=IncidentId || result.PlanId!=original.Submission.Intent.PlanId.ToString("N"))throw new InvalidOperationException("The current plan link differs from the original.");var after=await fields.GetCurrentFieldSelectionAsync();if(!Current())return;FieldSelectionReviewValidator.RequireCurrent(after,captured.Selection.Scope,captured.Selection.UserId);if(!after.SelectionAvailable || after.SelectionContext!=context)throw new InvalidOperationException("The FIELD changed during plan link review.");Message="The current incident links to this plan ID. This alone does not confirm the original submission; retry the same original for confirmation.";}
        catch(Exception error){if(Current())Error=explain(error,"reviewing the corrective action plan link","The original plan link could not be confirmed");}
        finally{if(Current()){Busy=false;Changed?.Invoke();}}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;fields.CurrentFieldChanged-=FieldChanged;Invalidate();}
}
