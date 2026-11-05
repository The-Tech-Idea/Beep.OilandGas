using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;

/// <summary>Circuit-owned intent retention; context invalidation clears displayed evidence, never an uncertain command.</summary>
public sealed class CommercialTermsWorkspaceState:IDisposable
{
    private readonly CompanyClientContext company;
    private record Selection(string Field,DateTime From,DateTime Through,string Transaction);
    private Selection? selected;
    private SalesContractTermsPreparation? pendingPreparation;
    private SalesContractTermsDecisionSubmission? pendingDecision;
    private SalesContractTermsReadReview? review;
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
    public bool HasPendingIntent=>pendingPreparation is not null || pendingDecision is not null;
    public bool CanRetryPending=>!Busy && review is not null && HasPendingIntent && PendingMatchesSelection();
    public SalesContractTermsPreparation? PendingPreparation=>pendingPreparation is not null && PendingMatchesSelection()?CompanyInventorySourceReview.Copy(pendingPreparation):null;
    public SalesContractTermsDecisionCommand? PendingDecision=>pendingDecision is not null && PendingMatchesSelection()?CompanyInventorySourceReview.Copy(pendingDecision.Command):null;
    public SalesContractTermsReadReview? Review=>review is null?null:CompanyInventorySourceReview.Copy(review);
    public event Action? Changed;
    public CommercialTermsWorkspaceState(CompanyClientContext company){this.company=company;company.Changed+=Invalidate;}
    public void Attach(string? field,DateTime from,DateTime through,string? transaction)
    {
        var next=field is null || transaction is null?null:new Selection(field,from,through,transaction);
        if(next is not null){FieldRevenueSourceReview.RequireWindow(next.Field,next.From,next.Through);if(string.IsNullOrWhiteSpace(transaction) || transaction.Length>128 || transaction.Any(char.IsControl))throw new ArgumentException("An exact revenue transaction is required.");}
        if(selected==next)return;selected=next;Invalidate();
    }
    public void Invalidate(){++version;review=null;Changed?.Invoke();}
    private void Ready(){if(disposed || Busy || selected is null)throw new InvalidOperationException("Select the original source and wait for the current operation.");}
    private static bool Same(object first,object second)=>JsonSerializer.Serialize(first)==JsonSerializer.Serialize(second);
    private bool PendingMatchesSelection()
    {
        var original=pendingPreparation ??pendingDecision?.Preparation.Preparation;
        var actor=pendingDecision?.Command.ActorUserId ??original?.ActorUserId;
        var selection=company.Selection;
        return original is not null && selected is not null && selection is not null && selection.Scope==original.Company && selection.UserId==actor &&
            original.Source.Source.FieldId==selected.Field && original.Source.Source.From==selected.From && original.Source.Source.Through==selected.Through && original.Source.TransactionId==selected.Transaction;
    }
    private async Task Refresh(IAccountingServiceClient client,Selection query,(long Version,CompanyClientSelection? Selection) snapshot,long generation)
    {
        var selection=snapshot.Selection ??throw new InvalidOperationException("Select a company and account.");
        var result=await client.GetSalesContractTermsAsync(query.Field,query.From,query.Through,query.Transaction);
        result.Require(selection.Scope,selection.UserId,query.Field,query.From,query.Through,query.Transaction);
        if(disposed || generation!=version || snapshot!=company.Snapshot || query!=selected)return;
        if(HasPendingIntent && PendingMatchesSelection())
        {
            if(pendingPreparation is {} preparation)
            {
                if(result.Recorded is {} recorded)
                {if(!Same(recorded.Preparation,preparation))throw new InvalidOperationException("A different preparation is recorded. Retain the original intent.");pendingPreparation=null;}
                else preparation.RequireCurrentSource(result.Source);
            }
            else if(pendingDecision is {} submission)
            {
                var recorded=result.Recorded ??throw new InvalidOperationException("The original preparation is unavailable. Retain the decision intent.");
                if(!Same(recorded.Preparation,submission.Preparation.Preparation) || recorded.PreparedAtUtc!=submission.Preparation.PreparedAtUtc)throw new InvalidOperationException("The original decision preparation changed.");
                if(recorded.Decision is {} decision)
                {if(decision!=submission.Command)throw new InvalidOperationException("A different decision is recorded. Retain the original intent.");pendingDecision=null;}
                else if(recorded.Revision!=submission.Preparation.Revision)throw new InvalidOperationException("The original undecided preparation changed.");
            }
        }
        review=CompanyInventorySourceReview.Copy(result);
    }
    public async Task LoadAsync(IAccountingServiceClient client)
    {
        Ready();var query=selected!;var snapshot=company.Snapshot;var generation=++version;review=null;Busy=true;Changed?.Invoke();
        try{await Refresh(client,query,snapshot,generation);}finally{Busy=false;Changed?.Invoke();}
    }
    public async Task PrepareAsync(IAccountingServiceClient client,SalesContractEffectiveTerms terms)
    {
        Ready();if(HasPendingIntent)throw new InvalidOperationException("Reconcile the original pending intent before creating another.");
        var current=review ??throw new InvalidOperationException("Reload the exact commercial source before preparing terms.");
        if(current.Recorded is not null)throw new InvalidOperationException("The contract already has recorded terms. Review its outcome.");
        var actor=company.Selection?.UserId ??throw new InvalidOperationException("Select an account.");
        var intent=new SalesContractTermsPreparation(Guid.NewGuid().ToString("N"),current.Source.Source.Company,actor,CompanyInventorySourceReview.Copy(current.Source),CompanyInventorySourceReview.Copy(terms));intent.Require();
        pendingPreparation=intent;await SubmitPendingAsync(client);
    }
    public async Task DecideAsync(IAccountingServiceClient client,SalesContractTermsDecision decision,string reason)
    {
        Ready();if(HasPendingIntent)throw new InvalidOperationException("Reconcile the original pending intent before creating another.");
        var record=review?.Recorded ??throw new InvalidOperationException("Review the original recorded preparation first.");
        var actor=company.Selection?.UserId ??throw new InvalidOperationException("Select an account.");
        var command=new SalesContractTermsDecisionCommand(Guid.NewGuid().ToString("N"),record.Preparation.Company,actor,record.Revision,decision,reason);command.Require(record);
        pendingDecision=new(CompanyInventorySourceReview.Copy(record),command);await SubmitPendingAsync(client);
    }
    public async Task SubmitPendingAsync(IAccountingServiceClient client)
    {
        Ready();if(!HasPendingIntent || !PendingMatchesSelection() || review is null)throw new InvalidOperationException("Reload the original company/account/FIELD/transaction/window before reconciling its intent.");
        if(pendingPreparation is {} preparation){if(review.Recorded is not null)throw new InvalidOperationException("Reload the recorded preparation to reconcile.");preparation.RequireCurrentSource(review.Source);}
        if(pendingDecision is {} submission){if(review.Recorded is null || !Same(review.Recorded,submission.Preparation))throw new InvalidOperationException("Reload the exact original undecided preparation.");submission.Require();}
        var query=selected!;var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            SalesContractTermsRecord result;
            if(pendingPreparation is {} prep)
            {result=await client.PrepareSalesContractTermsAsync(CompanyInventorySourceReview.Copy(prep));result.Require();if(!Same(result.Preparation,prep))throw new InvalidOperationException("Preparation acknowledgement differs from the original intent.");}
            else
            {var decision=pendingDecision!;result=await client.DecideSalesContractTermsAsync(CompanyInventorySourceReview.Copy(decision));result.Require();if(result.Decision!=decision.Command || !Same(result.Preparation,decision.Preparation.Preparation) || result.PreparedAtUtc!=decision.Preparation.PreparedAtUtc)throw new InvalidOperationException("Decision acknowledgement differs from the original intent.");}
            if(disposed || generation!=version || snapshot!=company.Snapshot || query!=selected)return;
            review=null;await Refresh(client,query,snapshot,generation);
            if(HasPendingIntent)throw new InvalidOperationException("Persisted outcome is still unconfirmed. Retain and reconcile the original intent.");
        }
        catch{if(!disposed && generation==version)review=null;throw;}
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;Invalidate();}
}
