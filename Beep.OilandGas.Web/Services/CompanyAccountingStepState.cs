using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Processes;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>One circuit's retained step command; company/process changes invalidate visible evidence.</summary>
public sealed class CompanyAccountingStepState : IDisposable
{
    private readonly CompanyClientContext company;
    private string? id,revision;
    private long version;
    private CancellationTokenSource changes=new();
    private bool disposed,confirmed;
    private ProcessInstance? recorded;
    private CompanyAccountingStepReceipt? receiptEvidence;
    private static T Copy<T>(T value)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
    public CompanyAccountingStepState(CompanyClientContext company){this.company=company;company.Changed+=ClearVisible;}
    public event Action? Changed;
    public bool Busy {get;private set;}
    public CompanyAccountingStepReview? Review {get;private set;}
    public CompanyAccountingStepCommand? Intent {get;private set;}
    public CompanyAccountingStepReceipt? Receipt {get=>receiptEvidence is null?null:Copy(receiptEvidence);private set=>receiptEvidence=value;}
    public bool HasUnconfirmedIntent=>Intent is not null && !confirmed;
    public bool CanReconcile=>HasUnconfirmedIntent && company.Selection is {} selected && Intent!.Review.Company==selected.Scope &&
        Intent.Review.ActorUserId==selected.UserId && Intent.Review.InstanceId==id && revision is not null;
    public void AttachProcess(string? process,string? displayedRevision)
    {
        if(id==process && revision==displayedRevision)return;
        var keep=confirmed && Intent is {} command && company.Selection is {} selected && command.Review.Company==selected.Scope &&
            command.Review.ActorUserId==selected.UserId && command.Review.InstanceId==process && recorded is not null &&
            ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(recorded))==displayedRevision;
        ClearVisible(keep);id=process;revision=displayedRevision;
    }
    private void ClearVisible(){id=null;revision=null;ClearVisible(false);}
    private void ClearVisible(bool keepConfirmed)
    {
        version++;var previous=changes;changes=new();previous.Cancel();previous.Dispose();Review=null;
        if(!keepConfirmed){Receipt=null;recorded=null;if(confirmed){Intent=null;confirmed=false;}}
        Changed?.Invoke();
    }
    public void AcceptReview(CompanyAccountingStepReview review)
    {
        if(disposed || Busy || company.Selection is not {} selected || id is null)throw new InvalidOperationException("Reload the original company process first.");
        review.Require(selected.Scope,selected.UserId,id);
        if(review.ProcessRevision!=revision)throw new InvalidOperationException("The displayed process changed before step review.");
        if(HasUnconfirmedIntent && review!=Intent!.Review)throw new InvalidOperationException("Reconcile the original completion before reviewing another step.");
        if(confirmed && Intent?.Review.StepInstanceId!=review.StepInstanceId){Intent=null;Receipt=null;recorded=null;confirmed=false;}
        Review=review;Changed?.Invoke();
    }
    public async Task SubmitAsync(IBusinessProcessServiceClient client,string notes,string? action=null)
    {
        if(disposed || Busy)throw new InvalidOperationException("Wait for the current completion outcome.");
        if(confirmed)throw new InvalidOperationException("This completion is confirmed. Review the next assigned step to continue.");
        if(Intent is null)
        {
            if(Review is not {} review || company.Selection is not {} selected || id!=review.InstanceId || revision!=review.ProcessRevision || selected.Scope!=review.Company || selected.UserId!=review.ActorUserId)
                throw new InvalidOperationException("Review the current original company step before completion.");
            var command=new CompanyAccountingStepCommand(review,Guid.NewGuid().ToString("N"),notes,action);command.Require();Intent=command;
        }
        if(!CanReconcile)throw new InvalidOperationException("Return to the original company/account/process before reconciling the retained completion.");
        var intent=Intent;var original=company.Snapshot;var generation=version;var token=changes.Token;Busy=true;Receipt=null;Changed?.Invoke();
        bool Current()=>!disposed && original==company.Snapshot && generation==version;
        try
        {
            var receipt=await client.CompleteCompanyAccountingStepAsync(intent,token);receipt.Require(intent);
            if(!Current())return;
            // Reuse the existing full company process/step/history validation owner for authoritative reload.
            using var detail=new ProcessInstanceDetailState(company,_=>throw new InvalidOperationException(),_=>throw new InvalidOperationException());
            await detail.LoadAccountingAsync(intent.Review.InstanceId,value=>client.GetCompanyAccountingInstanceAsync(value,token));
            if(!Current())return;
            var process=detail.Instance ?? throw new InvalidOperationException("The completed process read-back is unavailable.");
            var step=process.StepInstances.SingleOrDefault(row=>row.StepInstanceId==intent.Review.StepInstanceId);
            var history=process.History.SingleOrDefault(row=>row.HistoryId==receipt.HistoryId);
            if(process.ProcessId!=intent.Review.ProcessId || process.EntityType!=intent.Review.EntityType || process.EntityId!=intent.Review.EntityId ||
                step?.StepId!=intent.Review.StepId || (intent.Action=="RETURN_CORRECTION_WORK"?step.Status!=StepStatus.BLOCKED:step.Status!=StepStatus.COMPLETED || step.CompletedBy!=intent.Review.ActorUserId || step.Notes!=intent.Notes) ||
                history?.Action!=(CompanyAccountingSourceTypes.IsNativeDecision(intent.Review.EntityType) && intent.Action==CompanyAccountingSourceTypes.RejectionRoute(intent.Review.EntityType)?CompanyAccountingSourceTypes.RejectionHistory(intent.Review.EntityType):CompanyAccountingSourceTypes.IsNativeDecision(intent.Review.EntityType) && !intent.Review.RequiresIndependentActor?CompanyAccountingSourceTypes.RejectionReviewHistory(intent.Review.EntityType):intent.Action=="RETURN_CORRECTION_WORK"?"CORRECTION_RETURNED":"STEP_COMPLETED") || history.PerformedBy!=intent.Review.ActorUserId || history.StepInstanceId!=intent.Review.StepInstanceId || history.Notes!=intent.Notes ||
                receipt.NextStepInstanceId is {} next && (process.Status!=ProcessStatus.IN_PROGRESS || !process.StepInstances.Any(row=>row.StepInstanceId==next && row.StepId==process.CurrentStepId && row.Status==StepStatus.PENDING)) ||
                receipt.NextStepInstanceId is null && process.Status!=ProcessStatus.COMPLETED)
                throw new InvalidOperationException("The recorded completion and next recipient could not be confirmed.");
            recorded=Copy(process);Receipt=Copy(receipt);confirmed=true;Review=null;
        }
        catch{Review=null;throw;}
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=ClearVisible;changes.Cancel();changes.Dispose();}
}
