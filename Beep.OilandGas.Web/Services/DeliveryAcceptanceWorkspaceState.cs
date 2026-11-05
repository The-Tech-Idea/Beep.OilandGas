using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>Retains and recovers the original native journal command; invalidation never discards uncertain work.</summary>
public sealed class DeliveryAcceptanceWorkspaceState:IDisposable
{
    private readonly CompanyClientContext company;
    private record Selection(string Field,DateTime From,DateTime Through,string Transaction);
    private Selection? selected;
    private RevenueDeliveryAcceptancePreparation? pendingPreparation;
    private RevenueDeliveryAcceptanceSubmission? pendingDecision;
    private CompanyDeliveryCorrectionSubmission? pendingCorrection;
    private RevenueDeliveryAcceptanceReadReview? review;
    private RevenueDeliveryAcceptanceIntent? confirmed;
    private DELIVERY_ACCEPTANCE_INTENT? saved;
    private string? savedIntentId;
    private long version;
    private bool disposed;
    private object? presentationOwner;
    public void AttachOwner(object owner,string field,DateTime from,DateTime through,string transaction)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if(!ReferenceEquals(presentationOwner,owner)){presentationOwner=owner;Invalidate();}
        Attach(field,from,through,transaction);
    }
    public void DetachOwner(object owner)
    {
        if(!ReferenceEquals(presentationOwner,owner))return;
        presentationOwner=null;Attach(null,default,default,null);
    }
    public bool Busy{get;private set;}
    public bool HasPendingIntent=>pendingPreparation is not null || pendingDecision is not null || pendingCorrection is not null;
    public bool CanRetryPending
    {get{if(Busy || review is null || !HasPendingIntent || !PendingMatchesSelection())return false;try{var original=PendingEnvelope();if(original.Correction is not null && (company.Selection?.Permissions.Contains("Accounting.View",StringComparer.Ordinal)!=true || !company.Selection.Permissions.Contains("ProductionAccounting.Adjust",StringComparer.Ordinal)))return false;original.RequireExecution();original.RequireCurrent(review);return true;}catch(InvalidOperationException){return false;}}}
    public DELIVERY_ACCEPTANCE_INTENT? SavedIntent=>saved is null?null:CompanyInventorySourceReview.Copy(saved);
    public RevenueDeliveryAcceptanceIntent? PendingOriginal
    {get{if(!HasPendingIntent)return null;var original=PendingEnvelope();return company.Selection is {} selection && selection.Scope==original.Company && selection.UserId==original.ActorUserId?original:null;}}
    public RevenueDeliveryAcceptancePreparation? PendingPreparation=>pendingPreparation is not null && PendingMatchesSelection()?CompanyInventorySourceReview.Copy(pendingPreparation):null;
    public RevenueDeliveryAcceptanceDecisionCommand? PendingDecision=>pendingDecision is not null && PendingMatchesSelection()?CompanyInventorySourceReview.Copy(pendingDecision.Command):null;
    public RevenueDeliveryAcceptanceReadReview? Review=>review is null?null:CompanyInventorySourceReview.Copy(review);
    public event Action? Changed;
    public DeliveryAcceptanceWorkspaceState(CompanyClientContext company){this.company=company;company.Changed+=Invalidate;}
    public void Attach(string? field,DateTime from,DateTime through,string? transaction)
    {
        var next=field is null || transaction is null?null:new Selection(field,from,through,transaction);
        if(next is not null){FieldRevenueSourceReview.RequireWindow(next.Field,next.From,next.Through);if(string.IsNullOrWhiteSpace(transaction) || transaction.Length>128 || transaction.Any(char.IsControl))throw new ArgumentException("An exact revenue transaction is required.");}
        if(selected==next)return;if(!HasPendingIntent){confirmed=null;savedIntentId=null;}selected=next;Invalidate();
    }
    public void Invalidate()
    {
        ++version;review=null;saved=null;
        if(!HasPendingIntent && confirmed is not null && (company.Selection?.Scope!=confirmed.Company || company.Selection.UserId!=confirmed.ActorUserId)){confirmed=null;savedIntentId=null;}
        Changed?.Invoke();
    }
    private void Ready(){if(disposed || Busy || selected is null)throw new InvalidOperationException("Select the original source and wait for the current operation.");}
    private static bool Same(object first,object second)=>JsonSerializer.Serialize(first)==JsonSerializer.Serialize(second);
    private RevenueDeliveryAcceptanceIntent PendingEnvelope()=>new(CompanyInventorySourceReview.Copy(pendingPreparation),CompanyInventorySourceReview.Copy(pendingDecision)){Correction=CompanyInventorySourceReview.Copy(pendingCorrection)};
    public void RecoverSaved(DELIVERY_ACCEPTANCE_INTENT entry)
    {
        if(disposed || Busy || selected is null || company.Selection is not {} selection)throw new InvalidOperationException("Return to the original saved delivery scope and wait for the current operation.");
        var original=entry.Restore(selection.Scope,selection.UserId,entry.SCOPE_KEY);var source=original.Original.Source.Source.Source;
        if(source.FieldId!=selected.Field || source.From!=selected.From || source.Through!=selected.Through || original.Original.Source.Source.TransactionId!=selected.Transaction)throw new InvalidOperationException("The saved delivery recovery coordinates differ from the selected source.");
        var retained=HasPendingIntent?PendingEnvelope():confirmed;
        if(retained is not null && !Same(retained,original))throw new InvalidOperationException("Reconcile the retained original delivery command before opening another.");
        if(!HasPendingIntent && confirmed is null){pendingPreparation=CompanyInventorySourceReview.Copy(original.Preparation);pendingDecision=CompanyInventorySourceReview.Copy(original.Submission);pendingCorrection=CompanyInventorySourceReview.Copy(original.Correction);}
        saved=CompanyInventorySourceReview.Copy(entry);savedIntentId=entry.DELIVERY_ACCEPTANCE_INTENT_ID;review=null;Changed?.Invoke();
    }
    private bool PendingMatchesSelection()
    {
        var original=pendingCorrection?.Correction.Preparation ??pendingPreparation ??pendingDecision?.Preparation.Preparation;
        var actor=pendingDecision?.Command.ActorUserId ??original?.ActorUserId;
        var selection=company.Selection;
        return original is not null && selected is not null && selection is not null && selection.Scope==original.Company && selection.UserId==actor &&
            original.Source.Source.Source.FieldId==selected.Field && original.Source.Source.Source.From==selected.From && original.Source.Source.Source.Through==selected.Through && original.Source.Source.TransactionId==selected.Transaction;
    }
    private async Task Refresh(IAccountingServiceClient client,Selection query,(long Version,CompanyClientSelection? Selection) snapshot,long generation)
    {
        var selection=snapshot.Selection ??throw new InvalidOperationException("Select a company and account.");
        var recovered=await client.GetDeliverySavedReviewAsync(query.Field,query.From,query.Through,query.Transaction);
        recovered.Require(selection.Scope,selection.UserId,query.Field,query.From,query.Through,query.Transaction);var result=recovered.Current;
        result.Source.Source.Require(selection.Scope,selection.UserId,query.Field,query.From,query.Through,query.Transaction,result.Source.Source.Source.Revision);result.Require();
        if(disposed || generation!=version || snapshot!=company.Snapshot || query!=selected)return;
        var candidates=recovered.Entries.ToArray();
        var expected=HasPendingIntent?PendingEnvelope():confirmed;
        if(expected is not null)candidates=candidates.Where(row=>row.SCOPE_KEY==expected.ScopeKey).ToArray();
        if(candidates.Length>1)throw new InvalidOperationException("Multiple original saved delivery commands require explicit recovery before another action.");
        if(candidates.SingleOrDefault() is {} entry)
        {
            var original=entry.Restore(selection.Scope,selection.UserId,entry.SCOPE_KEY);var local=HasPendingIntent?PendingEnvelope():confirmed;
            if(local is not null && !Same(local,original))throw new InvalidOperationException("The saved delivery command differs from the retained original intent.");
            saved=CompanyInventorySourceReview.Copy(entry);savedIntentId=entry.DELIVERY_ACCEPTANCE_INTENT_ID;
            if(!HasPendingIntent && confirmed is null){pendingPreparation=CompanyInventorySourceReview.Copy(original.Preparation);pendingDecision=CompanyInventorySourceReview.Copy(original.Submission);pendingCorrection=CompanyInventorySourceReview.Copy(original.Correction);}
        }
        else if(savedIntentId is not null && (HasPendingIntent && PendingMatchesSelection() || confirmed is not null))
            throw new InvalidOperationException("The previously confirmed saved delivery command is unavailable. Retain its original intent.");
        if(HasPendingIntent && !PendingMatchesSelection())throw new InvalidOperationException("Return to the saved command's original company/account/FIELD/window/transaction before another action.");
        if(HasPendingIntent && PendingMatchesSelection())
        {
            var original=PendingEnvelope();original.RequireCurrent(result);
            if(pendingCorrection is {} correction)
            {
                if(result.Recorded is {} corrected && Same(corrected.Preparation,correction.Correction.Preparation)){confirmed=original;pendingCorrection=null;}
            }
            else if(pendingPreparation is {} preparation)
            {
                if(result.Recorded is {} recorded)
                {if(!Same(recorded.Preparation,preparation))throw new InvalidOperationException("A different preparation is recorded. Retain the original intent.");confirmed=original;pendingPreparation=null;}
                else preparation.RequireCurrent(result.Source);
            }
            else if(pendingDecision is {} submission)
            {
                var recorded=result.Recorded ??throw new InvalidOperationException("The original preparation is unavailable. Retain the decision intent.");
                if(!Same(recorded.Preparation,submission.Preparation.Preparation) || recorded.PreparedAtUtc!=submission.Preparation.PreparedAtUtc)throw new InvalidOperationException("The original decision preparation changed.");
                if(recorded.Decision is {} decision)
                {if(decision!=submission.Command)throw new InvalidOperationException("A different decision is recorded. Retain the original intent.");confirmed=original;pendingDecision=null;}
                else if(recorded.Revision!=submission.Preparation.Revision)throw new InvalidOperationException("The original undecided preparation changed.");
            }
        }
        review=CompanyInventorySourceReview.Copy(result);
    }
    public async Task LoadAsync(IAccountingServiceClient client)
    {
        Ready();var query=selected!;var snapshot=company.Snapshot;var generation=++version;review=null;saved=null;Busy=true;Changed?.Invoke();
        try{await Refresh(client,query,snapshot,generation);}finally{Busy=false;Changed?.Invoke();}
    }
    public async Task PrepareAsync(IAccountingServiceClient client,RevenueDeliveryAcceptanceFacts facts)
    {
        Ready();if(HasPendingIntent)throw new InvalidOperationException("Reconcile the original pending intent before creating another.");
        var current=review ??throw new InvalidOperationException("Reload the exact delivery source before preparing acceptance.");
        if(current.Recorded is not null)throw new InvalidOperationException("The ticket already has recorded acceptance. Review its outcome.");
        var actor=company.Selection?.UserId ??throw new InvalidOperationException("Select an account.");
        var intent=new RevenueDeliveryAcceptancePreparation(Guid.NewGuid().ToString("N"),current.Source.Source.Source.Company,actor,CompanyInventorySourceReview.Copy(current.Source),CompanyInventorySourceReview.Copy(facts));intent.Require();intent.Facts.RequireQuantity(intent.Source);
        pendingPreparation=intent;await SubmitPendingAsync(client);
    }
    public async Task DecideAsync(IAccountingServiceClient client,RevenueDeliveryAcceptanceDecision decision,string reason)
    {
        Ready();if(HasPendingIntent)throw new InvalidOperationException("Reconcile the original pending intent before creating another.");
        var record=review?.Recorded ??throw new InvalidOperationException("Review the original recorded preparation first.");
        var actor=company.Selection?.UserId ??throw new InvalidOperationException("Select an account.");
        var command=new RevenueDeliveryAcceptanceDecisionCommand(Guid.NewGuid().ToString("N"),record.Preparation.Company,actor,record.Revision,decision,reason);command.RequireExecution(record);
        pendingDecision=new(CompanyInventorySourceReview.Copy(record),command);await SubmitPendingAsync(client);
    }
    public async Task SubmitPendingAsync(IAccountingServiceClient client)
    {
        Ready();if(!HasPendingIntent || !PendingMatchesSelection() || review is null)throw new InvalidOperationException("Reload the original company/account/FIELD/transaction/window before reconciling its intent.");
        if(pendingCorrection is not null)PendingEnvelope().RequireCurrent(review);
        if(pendingPreparation is {} preparation){if(review.Recorded is not null)throw new InvalidOperationException("Reload the recorded preparation to reconcile.");preparation.RequireCurrent(review.Source);preparation.Facts.RequireQuantity(preparation.Source);}
        if(pendingDecision is {} submission){if(review.Recorded is null || !Same(review.Recorded,submission.Preparation))throw new InvalidOperationException("Reload the exact original undecided preparation.");submission.Command.RequireExecution(submission.Preparation);}
        var query=selected!;var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            var original=PendingEnvelope();original.RequireExecution();
            var entry=await client.SaveDeliveryIntentAsync(original);
            if(!Same(entry.Restore(original.Company,original.ActorUserId,original.ScopeKey),original))throw new InvalidOperationException("Saved delivery acknowledgement differs from the original intent.");
            if(disposed || generation!=version || snapshot!=company.Snapshot || query!=selected)return;
            saved=CompanyInventorySourceReview.Copy(entry);savedIntentId=entry.DELIVERY_ACCEPTANCE_INTENT_ID;
            RevenueDeliveryAcceptanceRecord result;
            if(pendingCorrection is {} correction)
            {result=await client.CorrectDeliveryAcceptanceAsync(CompanyInventorySourceReview.Copy(correction));result.Require();if(!Same(result.Preparation,correction.Correction.Preparation))throw new InvalidOperationException("Correction acknowledgement differs from the original intent.");}
            else if(pendingPreparation is {} prep)
            {result=await client.PrepareDeliveryAcceptanceAsync(CompanyInventorySourceReview.Copy(prep));result.Require();if(!Same(result.Preparation,prep))throw new InvalidOperationException("Preparation acknowledgement differs from the original intent.");}
            else
            {var decision=pendingDecision!;result=await client.DecideDeliveryAcceptanceAsync(CompanyInventorySourceReview.Copy(decision));result.Require();if(result.Decision!=decision.Command || !Same(result.Preparation,decision.Preparation.Preparation) || result.PreparedAtUtc!=decision.Preparation.PreparedAtUtc)throw new InvalidOperationException("Decision acknowledgement differs from the original intent.");}
            if(disposed || generation!=version || snapshot!=company.Snapshot || query!=selected)return;
            review=null;await Refresh(client,query,snapshot,generation);
            if(HasPendingIntent)throw new InvalidOperationException("Persisted outcome is still unconfirmed. Retain and reconcile the original intent.");
        }
        catch{if(!disposed && generation==version)review=null;throw;}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task CorrectAsync(IAccountingServiceClient client,CompanyDeliveryCorrectionReview correctionReview,RevenueDeliveryAcceptanceFacts facts,string reason)
    {
        Ready();if(HasPendingIntent)throw new InvalidOperationException("Reconcile the retained original command before another correction.");
        if(company.Selection is not {} selection)throw new InvalidOperationException("Select the original company/account.");correctionReview=CompanyInventorySourceReview.Copy(correctionReview);correctionReview.Require(selection.Scope,selection.UserId,correctionReview.InstanceId);
        var command=new CompanyDeliveryCorrectionSubmission(correctionReview,correctionReview.CreateIntent(Guid.NewGuid().ToString("N"),CompanyInventorySourceReview.Copy(facts),reason));command.Require();
        var envelope=new RevenueDeliveryAcceptanceIntent(null,null){Correction=command};envelope.RequireCurrent(review ??throw new InvalidOperationException("Reload the original receiving evidence before correcting it."));
        pendingCorrection=command;if(!PendingMatchesSelection()){pendingCorrection=null;throw new InvalidOperationException("The reviewed correction source differs from this workbench.");}confirmed=null;saved=null;savedIntentId=null;await SubmitPendingAsync(client);
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;Invalidate();}
}
