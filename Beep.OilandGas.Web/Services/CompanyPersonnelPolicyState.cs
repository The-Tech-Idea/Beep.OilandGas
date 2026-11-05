using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed class CompanyPersonnelPolicyState:IDisposable
{
    private readonly CompanyClientContext company;private CompanyClientSelection? owner;private CompanyPersonnelPolicyCommand? command;private CompanyPersonnelPolicyReference? reference;private WORK_ORDER_PERSONNEL_POLICY? receipt;private long version;private bool confirmed,disposed;
    public event Action? Changed;public bool Busy {get;private set;}public CompanyPersonnelPolicyCatalog? Catalog {get;private set;}
    private bool SameOwner=>owner is {} original && company.Selection is {} current && original.Scope==current.Scope && original.UserId==current.UserId;
    public CompanyPersonnelPolicyReference? VisibleReference=>SameOwner?reference:null;
    public CompanyPersonnelPolicyCommand? VisibleCommand=>SameOwner && command is not null?CompanyInventorySourceReview.Copy(command):null;
    public WORK_ORDER_PERSONNEL_POLICY? Receipt=>SameOwner && receipt is not null?CompanyInventorySourceReview.Copy(receipt):null;
    public bool CanReview=>!Busy && reference is null;public bool CanAppend=>CanReview && Catalog is not null;
    public CompanyPersonnelPolicyState(CompanyClientContext company){this.company=company;company.Changed+=Invalidate;}
    private void Invalidate(){version++;Catalog=null;receipt=null;if(confirmed){command=null;reference=null;owner=null;confirmed=false;}Changed?.Invoke();}
    private CompanyClientSelection Current()
    {
        if(disposed || company.Selection is not {} selected || !selected.Roles.Contains("Administrator",StringComparer.Ordinal) || !selected.Permissions.Contains("HSE.ConfigurePersonnelPolicy",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original company Administrator and personnel policy authority are required.");return selected;
    }
    public async Task ReviewAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(!CanReview)throw new InvalidOperationException("Reconcile the original retained policy intent before another review.");var selected=Current();var snapshot=company.Snapshot;var generation=version;Busy=true;Catalog=null;Changed?.Invoke();
        try{var result=await client.ReviewCompanyPersonnelPoliciesAsync();result.Require();if(result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original policy administrator company/account differs.");if(!disposed && generation==version && snapshot==company.Snapshot)Catalog=CompanyInventorySourceReview.Copy(result);}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task AppendAsync(ICompanyMaintenanceHandoffClient client,CompanyPersonnelPolicyDraft policy,int nextVersion,string reason,Func<CompanyPersonnelPolicyReference,Task> retainBeforeTransport)
    {
        ArgumentNullException.ThrowIfNull(retainBeforeTransport);if(!CanAppend)throw new InvalidOperationException("Review the complete current policy catalog before appending another version.");var selected=Current();var catalog=Catalog!;
        if(catalog.Context.Company!=selected.Scope || catalog.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Return to the original policy administrator.");
        policy.RequireCurrentEligibility();
        var original=new CompanyPersonnelPolicyCommand(catalog.Context,catalog.Revision,CompanyInventorySourceReview.Copy(policy),nextVersion,Guid.NewGuid(),reason);original.RequireCatalog(catalog);
        command=original;reference=new(original.RecordId(),original.IntentId);owner=selected;receipt=null;confirmed=false;Catalog=null;var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try{await retainBeforeTransport(reference);if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("Original policy context changed before transport.");var result=await client.RecordCompanyPersonnelPolicyAsync(CompanyInventorySourceReview.Copy(original));original.RequireRecorded(result);if(!disposed && generation==version && snapshot==company.Snapshot){receipt=CompanyInventorySourceReview.Copy(result);confirmed=true;}}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverReferenceAsync(ICompanyMaintenanceHandoffClient client,CompanyPersonnelPolicyReference original,Func<CompanyPersonnelPolicyReference,Task>? retainBeforeTransport=null)
    {
        original.Require();if(Busy || reference is {} retained && retained!=original)throw new InvalidOperationException("Recover the same original retained policy reference.");var selected=Current();if(owner is not null && !SameOwner)throw new InvalidOperationException("Return to the same original policy maker/company.");
        owner=selected;reference=original;Catalog=null;receipt=null;confirmed=false;var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try{if(retainBeforeTransport is not null)await retainBeforeTransport(original);if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("Original policy reference context changed before transport.");var result=await client.RecoverCompanyPersonnelPolicyReferenceAsync(original);result.Require(selected.Scope,selected.UserId,original);if(command is not null && System.Text.Json.JsonSerializer.Serialize(command)!=System.Text.Json.JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Original retained policy body changed during recovery.");if(!disposed && generation==version && snapshot==company.Snapshot){command=CompanyInventorySourceReview.Copy(result.Command);receipt=CompanyInventorySourceReview.Copy(result.Recorded);confirmed=true;}}
        finally{Busy=false;Changed?.Invoke();}
    }
    public void BeginNextReview()
    {
        if(Busy || !SameOwner || !confirmed || command is null || receipt is null)throw new InvalidOperationException("Confirm the exact original policy record before another version review.");Current();command.RequireRecorded(receipt);command=null;reference=null;owner=null;receipt=null;Catalog=null;confirmed=false;version++;Changed?.Invoke();
    }
    public void ClearDisplay()=>Invalidate();
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;Invalidate();}
}
