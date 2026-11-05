using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.Security;
using Microsoft.AspNetCore.Components.Authorization;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>One page's reviewed original. Browser input is never authorization or a recorded business outcome.</summary>
public sealed partial class PermitApplicationCommandState:IDisposable
{
    private readonly CompanyClientContext company;private readonly IFieldSelectionContext fields;private readonly AuthenticationStateProvider authentication;
    private readonly IPermitServiceClient client;private readonly IPermitApplicationIntentStore originals;
    private readonly RetainedOriginalFeedback feedback=new();private CancellationTokenSource? active;
    private PermitApplicationCommand? pending;private FieldSelectionReview? currentField;private bool readable,disposed,savingOriginal,savingPrior;private long version;
    public PermitApplicationCommandState(CompanyClientContext company,IFieldSelectionContext fields,AuthenticationStateProvider authentication,IPermitServiceClient client,IPermitApplicationIntentStore originals)
    {this.company=company;this.fields=fields;this.authentication=authentication;this.client=client;this.originals=originals;company.Changed+=Invalidate;fields.CurrentFieldChanged+=FieldChanged;authentication.AuthenticationStateChanged+=AuthChanged;}
    public event Action? Changed;
    public string? ApplicationId{get;private set;}public long Epoch=>version;public bool Ready{get;private set;}public bool Busy{get;private set;}
    public bool CreationMode{get;private set;}private Guid creationIntent;
    public PermitApplicationCreationReview? CreationReview{get;private set;}
    public PermitApplicationSourceReview? Review{get;private set;}public PermitApplicationCommandOutcome? Outcome{get;private set;}
    public Guid? PendingReference=>pending?.IntentId;public bool HasPending=>pending is not null || pendingDocument is not null;
    public PermitApplicationCommand? VisibleOriginal=>pending is not null && readable && pending.Review.Context.Field==currentField?pending:null;
    public bool AbsenceObserved{get;private set;}
    public string OriginalTitle=>feedback.Title("permit command");public string OriginalExplanation=>savingOriginal && savingPrior?"Saving the same original again. An earlier request may already have reached the server.":feedback.Explanation;
    public bool CanReview=>!disposed && Ready && readable && !Busy && !HasPending;
    public bool CanRecover=>!disposed && Ready && readable && !Busy && VisibleOriginal is not null;
    private bool Permission(string action)=>company.Selection?.Permissions.Contains(PermitApplicationCommand.IsAuthorAction(action)?PermissionConstants.HSE.IssuePermit:PermissionConstants.HSE.ApprovePermit)==true;
    public bool CanReviewCreation=>CreationMode && CanReview && Permission("CREATE_DRAFT");
    public bool CanPrepare(string action)=>CanReview && Review is not null && Permission(action) && (CreationMode?
        action=="CREATE_DRAFT" && CreationReview is not null:PermitApplicationCommand.AllowedActions(Review).Contains(action,StringComparer.Ordinal));
    public bool CanResend=>CanRecover && AbsenceObserved && pending is not null && Permission(pending.Action);
    public void SetApplication(string? id){if(!CreationMode && ApplicationId==id)return;CreationMode=false;ApplicationId=id;Invalidate();}
    public void SetCreation(){if(CreationMode)return;CreationMode=true;ApplicationId="new-permit";Invalidate();}
    private void FieldChanged(string _)=>Invalidate();private void AuthChanged(Task<AuthenticationState> _)=>Invalidate();
    private void Invalidate(){version++;active?.Cancel();currentField=null;readable=false;Ready=false;Busy=false;Review=null;CreationReview=null;creationIntent=Guid.Empty;Outcome=null;pending=null;pendingDocument=null;StagedDocument=null;DocumentOutcome=null;DocumentAbsenceObserved=false;AbsenceObserved=false;feedback.Reset();documentFeedback.Reset();savingOriginal=false;savingPrior=false;Changed?.Invoke();}
    private Operation Begin()
    {
        if(disposed || Busy || !LeaseSummaryValidator.Identifier(ApplicationId))throw new InvalidOperationException("Wait for the current operation and choose an exact application.");
        var op=new Operation(this,version,company.Snapshot,ApplicationId!);active=op.Cancel;Busy=true;Changed?.Invoke();return op;
    }
    private sealed class Operation(PermitApplicationCommandState owner,long epoch,(long Version,CompanyClientSelection? Selection) snapshot,string id):IDisposable
    {
        public CancellationTokenSource Cancel{get;}=new();public CancellationToken Token=>Cancel.Token;
        public bool IsCurrent=>!owner.disposed && owner.version==epoch && owner.company.Snapshot==snapshot && owner.ApplicationId==id;
        public void Check(){if(!IsCurrent)throw new OperationCanceledException("The original permit context changed.");}
        public async Task<FieldSelectionReview> Authority(string? action=null)
        {
            Check();var user=(await owner.authentication.GetAuthenticationStateAsync()).User;Check();
            var selected=snapshot.Selection;
            if(selected is null || user.Identity?.IsAuthenticated!=true || user.FindFirst("party_id")?.Value!=selected.UserId ||
                !selected.Roles.Any(user.IsInRole) || !selected.Permissions.Contains(PermissionConstants.HSE.ViewPermits) || action is not null && !owner.Permission(action))
                throw new UnauthorizedAccessException("Current company application roles and permit authority are required.");
            var current=await owner.fields.GetCurrentFieldSelectionAsync(Token);Check();
            FieldSelectionReviewValidator.RequireCurrent(current,selected.Scope,selected.UserId);
            if(!current.SelectionAvailable)throw new InvalidOperationException("Choose an available company FIELD.");
            return current.SelectionContext!;
        }
        public void Dispose(){if(ReferenceEquals(owner.active,Cancel))owner.active=null;Cancel.Dispose();if(!owner.disposed && owner.version==epoch){owner.Busy=false;owner.Changed?.Invoke();}}
    }
    public Task AttachAsync(string id)=>AttachOriginalAsync(id,false);
    public Task AttachCreationAsync()=>AttachOriginalAsync("new-permit",true);
    private async Task AttachOriginalAsync(string id,bool creation)
    {
        if(creation)SetCreation();else SetApplication(id);using var op=Begin();var field=await op.Authority();
        var saved=creation?await originals.ReadCreationAsync(field):await originals.ReadAsync(field,id);op.Check();var after=await op.Authority();
        if(after!=field)throw new InvalidOperationException("The selected FIELD changed while reading the retained permit original.");
        if(pending is not null && feedback.SubmissionMayHaveBeenSent && saved is not null && pending!=saved)throw new InvalidOperationException("A different protected original was preserved.");
        if(saved is not null){pending=saved;feedback.ReadFromTab();}
        if(Outcome is not null && pending==Outcome.Original)feedback.ConfirmReceiptAndBeginClear(true);else Outcome=null;
        var document=creation?null:await originals.ReadDocumentAsync(field,id);var candidate=creation?null:await originals.ReadStagedDocumentAsync(field,id);op.Check();
        if(saved is not null && document is not null)throw new InvalidOperationException("Two retained workflow originals require reconciliation.");
        if(await op.Authority()!=field)throw new InvalidOperationException("The selected FIELD changed while reading upload input.");
        pendingDocument=document;StagedDocument=candidate;if(document is not null)documentFeedback.ReadFromTab();DocumentAbsenceObserved=false;
        currentField=field;readable=true;Ready=true;Review=null;CreationReview=null;creationIntent=Guid.Empty;AbsenceObserved=false;
    }
    public async Task ReviewAsync()
    {
        if(CreationMode || !CanReview)throw new InvalidOperationException("Read retained input and recover any pending original before reviewing another command.");
        using var op=Begin();var field=await op.Authority();
        var review=await client.ReviewSourceAsync(ApplicationId!,field,op.Token);review.RequireFor(ApplicationId!,field);op.Check();
        if(await op.Authority()!=field)throw new InvalidOperationException("The selected FIELD changed during permit review.");
        currentField=field;readable=true;Review=review;Outcome=null;
    }
    public async Task ReviewCreationAsync(PermitApplicationCreationProposal proposal)
    {
        if(!CanReviewCreation)throw new InvalidOperationException("Read retained creation input and recover a pending original before reviewing a new draft under author authority.");
        proposal.Require();using var op=Begin();var field=await op.Authority("CREATE_DRAFT");
        var reviewed=await client.ReviewCreationAsync(proposal,field,op.Token);reviewed.RequireFor(proposal,field);op.Check();
        if(await op.Authority("CREATE_DRAFT")!=field)throw new InvalidOperationException("The selected FIELD changed during creation review.");
        currentField=field;readable=true;CreationReview=reviewed;creationIntent=Guid.NewGuid();
        Review=PermitApplicationSourceReview.Capture(reviewed.Context,reviewed.BuildDraft(creationIntent),[]);Outcome=null;
    }
    public async Task RecordAsync(string action,string reason,bool confirmed,PermitApplicationDraftChanges? draftChanges=null,PermitDrillingDraftChanges? drillingChanges=null,PermitEnvironmentalDraftChanges? environmentalChanges=null,PermitInjectionDraftChanges? injectionChanges=null,PermitAttachmentProposal? attachment=null)
    {
        if(!CanPrepare(action))throw new InvalidOperationException("Review the current source and use a permitted independent action.");
        if(!confirmed || string.IsNullOrWhiteSpace(reason) || reason.Trim().Length is <20 or >2000 || reason.Any(c=>char.IsControl(c) && c is not ('\r' or '\n')))
            throw new InvalidOperationException("Confirm the exact source and enter a reason between 20 and 2000 characters.");
        var review=Review!;using var op=Begin();
        if(await op.Authority(action)!=review.Context.Field)throw new InvalidOperationException("The reviewed permit selection changed.");
        var original=action=="CREATE_DRAFT"?CreationReview!.PrepareOriginal(creationIntent,reason.Trim()):
            new PermitApplicationCommand(Guid.NewGuid(),review,action,reason.Trim()){DraftChanges=draftChanges,DrillingChanges=drillingChanges,EnvironmentalChanges=environmentalChanges,InjectionChanges=injectionChanges,Attachment=attachment};
        if(original.Review!=review || action=="CREATE_DRAFT" && draftChanges is not null)throw new InvalidOperationException("The creation proposal changed before retention.");
        original.Require();pending=original;Review=null;CreationReview=null;Outcome=null;AbsenceObserved=false;
        await Send(op,original);
    }
    private async Task Send(Operation op,PermitApplicationCommand original)
    {
        savingPrior=feedback.SubmissionMayHaveBeenSent;savingOriginal=true;feedback.BeginSaving();if(savingPrior)feedback.AttemptSubmission();Changed?.Invoke();
        try
        {
            await originals.RetainAsync(original);op.Check();
            if(await op.Authority(original.Action)!=original.Review.Context.Field)throw new InvalidOperationException("Return to the original permit context before sending.");
            savingOriginal=false;feedback.ConfirmSaved();feedback.AttemptSubmission();Changed?.Invoke();
            var outcome=await client.RecordCommandAsync(original,op.Token);await Accept(op,original,outcome);
        }
        catch{if(op.IsCurrent){savingOriginal=false;feedback.Failed();}throw;}
    }
    private async Task Accept(Operation op,PermitApplicationCommand original,PermitApplicationCommandOutcome outcome)
    {
        outcome.Require();if(outcome.Original!=original)throw new InvalidOperationException("The permit outcome differs from the retained original.");
        op.Check();if(await op.Authority()!=original.Review.Context.Field)throw new InvalidOperationException("The permit recovery context changed.");
        Outcome=outcome;feedback.ConfirmReceiptAndBeginClear(true);Changed?.Invoke();
        if(original.Action=="ATTACH_DOCUMENT")
        {
            await originals.ClearStagedDocumentAsync(original.Review.Context.Field,original.Attachment!.Document);op.Check();StagedDocument=null;DocumentOutcome=null;
        }
        await originals.ClearAsync(original);op.Check();pending=null;Review=null;CreationReview=null;creationIntent=Guid.Empty;AbsenceObserved=false;
    }
    public async Task<PermitAttachmentContent> ReadAttachmentAsync(string attachmentId)
    {
        if(!CanReview || Review is not {} review)throw new InvalidOperationException("Review the current application before reading attachment content.");
        using var op=Begin();if(await op.Authority()!=review.Context.Field)throw new InvalidOperationException("The attachment review context changed.");
        var query=new PermitAttachmentContentQuery(review,attachmentId);query.Require();
        var result=await client.ReadAttachmentContentAsync(query,op.Token);result.Require(query);op.Check();
        if(await op.Authority()!=review.Context.Field)throw new InvalidOperationException("Attachment read authority changed before delivery.");return result;
    }
    public async Task RecoverAsync()
    {
        if(!CanRecover)throw new InvalidOperationException("Return to the original company/account/FIELD selection to recover its retained command.");
        var original=pending!;using var op=Begin();
        if(await op.Authority()!=original.Review.Context.Field)throw new InvalidOperationException("The retained permit selection differs from the current selection.");
        var result=await client.RecoverCommandAsync(original,op.Token);result.Require(original);op.Check();
        if(await op.Authority()!=original.Review.Context.Field)throw new InvalidOperationException("The permit selection changed during recovery.");
        if(result.Outcome is null)
        {
            if(Outcome is not null)throw new InvalidOperationException("A previously recorded receipt is now absent. Keep the original for investigation.");
            AbsenceObserved=true;return; // An absent read cannot prove that an earlier in-flight request will never commit.
        }
        await Accept(op,original,result.Outcome);
    }
    public async Task ResendAsync()
    {
        if(!CanResend)throw new InvalidOperationException("Recover first and use only the exact retained original under current write authority.");
        var original=pending!;using var op=Begin();
        if(await op.Authority(original.Action)!=original.Review.Context.Field)throw new InvalidOperationException("The retained permit context changed.");
        AbsenceObserved=false;await Send(op,original);
    }
    public void Dispose(){if(disposed)return;disposed=true;version++;active?.Cancel();company.Changed-=Invalidate;fields.CurrentFieldChanged-=FieldChanged;authentication.AuthenticationStateChanged-=AuthChanged;Changed=null;}
}
