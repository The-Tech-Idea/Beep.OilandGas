using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>Retains one reviewed amendment in the circuit until its native history and original process confirm it.</summary>
public sealed class CommercialAmendmentWorkspaceState:IDisposable
{
    private readonly CompanyClientContext company;
    private string? process;
    private object? owner;
    private long generation;
    private bool disposed;
    private CompanyCommercialAmendmentSubmission? pending,confirmed;
    private CompanyCommercialAmendmentReview? review;
    private SalesContractTermsRecord? recorded;
    private CompanyCommercialHandoffReview? handoff;
    private COMMERCIAL_TERMS_INTENT? saved;
    private string? savedIntentId;
    public CommercialAmendmentWorkspaceState(CompanyClientContext company){this.company=company;company.Changed+=Invalidate;}
    public event Action? Changed;
    public bool Busy{get;private set;}
    public bool HasPendingIntent=>pending is not null;
    private bool Matches(CompanyCommercialAmendmentSubmission command)=>company.Selection is {} selection && selection.Scope==command.Review.Company && selection.UserId==command.Review.ActorUserId && process==command.Review.InstanceId;
    public CompanyCommercialAmendmentSubmission? Pending=>pending is not null && Matches(pending)?CompanyInventorySourceReview.Copy(pending):null;
    public CompanyCommercialAmendmentReview? Review=>review is null?null:CompanyInventorySourceReview.Copy(review);
    public SalesContractTermsRecord? Recorded=>recorded is null?null:CompanyInventorySourceReview.Copy(recorded);
    public CompanyCommercialHandoffReview? Handoff=>handoff is null?null:CompanyInventorySourceReview.Copy(handoff);
    public COMMERCIAL_TERMS_INTENT? SavedIntent=>saved is null?null:CompanyInventorySourceReview.Copy(saved);
    public bool CanRetry=>!Busy && pending is not null && Matches(pending) && review is not null && Same(review,pending.Review);
    private static bool Same(object? a,object? b)=>JsonSerializer.Serialize(a)==JsonSerializer.Serialize(b);
    public void AttachOwner(object value,string id)
    {
        if(string.IsNullOrWhiteSpace(id) || id.Length>128 || id.Any(char.IsControl))throw new ArgumentException("The original process is required.");
        if(ReferenceEquals(owner,value) && process==id)return;
        if(pending is null && process!=id){savedIntentId=null;if(confirmed is not null && confirmed.Review.InstanceId!=id)confirmed=null;}
        owner=value;process=id;Invalidate();
    }
    public void DetachOwner(object value){if(!ReferenceEquals(owner,value))return;owner=null;process=null;Invalidate();}
    public void Invalidate(){generation++;review=null;recorded=null;handoff=null;saved=null;Changed?.Invoke();}
    private void Ready(){if(disposed || Busy || process is null || company.Selection is null)throw new InvalidOperationException("Return to the original process/company/account and wait for the current operation.");}
    private async Task Refresh(IBusinessProcessServiceClient client,string id,(long Version,CompanyClientSelection? Selection) snapshot,long epoch)
    {
        bool Current()=>!disposed && generation==epoch && snapshot==company.Snapshot && process==id;
        async Task Discover(SalesContractTermsRecord record)
        {
            var selection=snapshot.Selection!;
            var query=new CompanyAccountingProcessStartCatalogQuery("SALES_CONTRACT",record.Preparation.Source.Contract.SALES_CONTRACT_ID,CompanyAccountingSourceTypes.CommercialPreparationRevision(record));
            var result=await client.DiscoverCommercialHandoffAsync(query) ?? throw new InvalidOperationException("Commercial handoff discovery is unavailable.");result.Require(selection.Scope,selection.UserId,query);
            if(Current())handoff=CompanyInventorySourceReview.Copy(result);
        }
        var selectionForIntent=snapshot.Selection!;
        var durable=await client.GetCommercialAmendmentIntentAsync(id);
        if(!Current())return;
        if(durable is not null)
        {
            var original=durable.Restore(selectionForIntent.Scope,selectionForIntent.UserId,id);
            if((pending ?? confirmed) is {} local && !Same(local,original))throw new InvalidOperationException("Reconcile the different original saved amendment before another command.");
            saved=CompanyInventorySourceReview.Copy(durable);
            savedIntentId=durable.COMMERCIAL_TERMS_INTENT_ID;
            if(pending is null && confirmed is null)pending=CompanyInventorySourceReview.Copy(original);
        }
        else if(savedIntentId is not null && savedIntentId==(pending ?? confirmed)?.Amendment.Preparation.IntentId)throw new InvalidOperationException("The previously saved amendment intent is unavailable. Retain its original command.");
        var retained=pending ?? confirmed;
        if(retained is null)
        {
            var selection=snapshot.Selection!;
            var source=await client.GetCompanyCommercialSourceAsync(id);source.Require(selection.Scope,selection.UserId,id);
            if(!Current())return;
            if(source.CurrentNativeContract is null || source.Terms.Recorded?.Preparation.ActorUserId!=selection.UserId)throw new InvalidOperationException("The original closed preparer process is required.");
            var proof=SalesContractTermsNativeProof.Require(selection.Scope,source.CurrentNativeContract);
            var versions=(proof.PriorVersions ?? []).Select(row=>(row.Record,row.Amendment)).Append((proof.Record,proof.Amendment));
            var linked=versions.Where(row=>row.Amendment?.Process?.InstanceId==id && row.Amendment.Process.ProcessRevision==source.ProcessRevision && row.Amendment.PreviousRecordRevision==source.Terms.Recorded.Revision).ToArray();
            if(linked.Length>1)throw new InvalidOperationException("The original process has ambiguous amendment evidence.");
            if(linked.Length==1){recorded=CompanyInventorySourceReview.Copy(linked[0].Record);await Discover(linked[0].Record);return;}
        }
        if(retained is not null)
        {
            if(!Matches(retained))throw new InvalidOperationException("Reconcile the retained amendment in its original company/account/process.");
            var source=await client.GetCompanyCommercialSourceAsync(id);source.Require(retained.Review.Company,retained.Review.ActorUserId,id);
            if(!Current())return;
            if(source.ProcessRevision!=retained.Review.ProcessRevision || !Same(source.Terms.Recorded,retained.Review.Previous) || source.CurrentNativeContract is null)
                throw new InvalidOperationException("The original closed process/predecessor changed. Retain the intent.");
            var proof=SalesContractTermsNativeProof.Require(retained.Review.Company,source.CurrentNativeContract);
            var versions=(proof.PriorVersions ?? []).Select(row=>(row.Record,row.Amendment)).Append((proof.Record,proof.Amendment));
            var matching=versions.Where(row=>Same(row.Record.Preparation,retained.Amendment.Preparation)).ToArray();
            if(matching.Length==1)
            {
                var expected=new SalesContractTermsAmendmentOrigin(retained.Amendment.Previous.Revision,retained.Amendment.PreviousNativeRevision,retained.Amendment.Reason,retained.Amendment.Process);
                if(matching[0].Amendment!=expected)throw new InvalidOperationException("The native amendment origin differs from its retained intent.");
                recorded=CompanyInventorySourceReview.Copy(matching[0].Record);confirmed=CompanyInventorySourceReview.Copy(retained);pending=null;review=null;await Discover(matching[0].Record);return;
            }
            if(confirmed is not null || proof.Revision!=retained.Amendment.PreviousNativeRevision || !Same(proof.Record,retained.Review.Previous))
                throw new InvalidOperationException("The retained amendment cannot be reconciled against current native history.");
        }
        var fresh=await client.ReviewCommercialAmendmentAsync(id);
        var selected=snapshot.Selection!;fresh.Require(selected.Scope,selected.UserId,id);
        if(!Current())return;
        if(pending is not null && !Same(fresh,pending.Review))throw new InvalidOperationException("The original reviewed amendment source changed. Retain the intent.");
        review=CompanyInventorySourceReview.Copy(fresh);
    }
    public async Task LoadAsync(IBusinessProcessServiceClient client)
    {
        Ready();var id=process!;var snapshot=company.Snapshot;var epoch=++generation;review=null;recorded=null;handoff=null;saved=null;Busy=true;Changed?.Invoke();
        try{await Refresh(client,id,snapshot,epoch);}finally{Busy=false;Changed?.Invoke();}
    }
    public async Task PrepareAsync(IBusinessProcessServiceClient client,SalesContractEffectiveTerms terms,string reason)
    {
        Ready();if(pending is not null || confirmed is not null || recorded is not null)throw new InvalidOperationException("Reconcile the original amendment before proposing another version.");
        var current=review ?? throw new InvalidOperationException("Review the fresh source and closed original process first.");
        var selection=company.Selection!;current.Require(selection.Scope,selection.UserId,process!);
        pending=new(CompanyInventorySourceReview.Copy(current),current.CreateIntent(Guid.NewGuid().ToString("N"),terms,reason));
        await SubmitPendingAsync(client);
    }
    public async Task SubmitPendingAsync(IBusinessProcessServiceClient client)
    {
        Ready();if(!CanRetry)throw new InvalidOperationException("Reload the original reviewed source before retrying its frozen intent.");
        var command=CompanyInventorySourceReview.Copy(pending!);command.Require();var id=process!;var snapshot=company.Snapshot;var epoch=generation;Busy=true;Changed?.Invoke();
        try
        {
            var entry=await client.SaveCommercialAmendmentIntentAsync(command);
            if(!Same(entry.Restore(command.Review.Company,command.Review.ActorUserId,id),command))throw new InvalidOperationException("The durable saved proposal differs from the original intent.");
            if(disposed || epoch!=generation || snapshot!=company.Snapshot || process!=id)return;
            saved=CompanyInventorySourceReview.Copy(entry);
            savedIntentId=entry.COMMERCIAL_TERMS_INTENT_ID;
            var result=await client.SubmitCommercialAmendmentAsync(command);result.Require();
            if(!Same(result.Preparation,command.Amendment.Preparation))throw new InvalidOperationException("The amendment acknowledgement differs from its original proposal.");
            if(disposed || epoch!=generation || snapshot!=company.Snapshot || process!=id)return;
            review=null;recorded=null;await Refresh(client,id,snapshot,epoch);
            if(pending is not null)throw new InvalidOperationException("The native amendment remains unconfirmed. Retain the original intent.");
        }
        catch{if(!disposed && epoch==generation){review=null;recorded=null;saved=null;}throw;}
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;Invalidate();}
}
