using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;
public sealed class CompanyPersonnelIdentityState:IDisposable
{
    private readonly CompanyClientContext company;private CompanyClientSelection? owner;private CompanyPersonnelIdentityCommand? command;private CompanyPersonnelIdentityReference? reference;private COMPANY_PERSONNEL_IDENTITY? receipt;private long generation;private bool confirmed,disposed;
    public event Action? Changed;public bool Busy{get;private set;}public CompanyPersonnelIdentityReview? Review{get;private set;}
    private bool SameOwner=>owner is {} original && company.Selection is {} current && original.Scope==current.Scope && original.UserId==current.UserId;
    public CompanyPersonnelIdentityReference? VisibleReference=>SameOwner?reference:null;
    public CompanyPersonnelIdentityCommand? VisibleCommand=>SameOwner && command is not null?CompanyInventorySourceReview.Copy(command):null;
    public COMPANY_PERSONNEL_IDENTITY? Receipt=>SameOwner && receipt is not null?CompanyInventorySourceReview.Copy(receipt):null;
    public bool CanReview=>!Busy && reference is null;public bool CanRecord=>CanReview && Review is not null;
    public CompanyPersonnelIdentityState(CompanyClientContext company){this.company=company;company.Changed+=ClearDisplay;}
    private CompanyClientSelection Current()
    {if(disposed || company.Selection is not {} selected || !selected.Roles.Contains("Administrator",StringComparer.Ordinal) || !selected.Permissions.Contains("HSE.ConfigurePersonnelIdentity",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original company Administrator and identity configuration permission are required.");return selected;}
    public async Task ReviewAsync(ICompanyMaintenanceHandoffClient client,CompanyPersonnelIdentityQuery query)
    {
        if(!CanReview)throw new InvalidOperationException("Recover original identity-link outcome before replacing the intent.");query.Require();var selected=Current();var version=++generation;var snapshot=company.Snapshot;Busy=true;Review=null;Changed?.Invoke();
        try{var result=await client.ReviewCompanyPersonnelIdentityAsync(query);result.Require();if(result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId || result.Query!=query)throw new UnauthorizedAccessException("Original identity review differs.");if(!disposed && version==generation && snapshot==company.Snapshot)Review=CompanyInventorySourceReview.Copy(result);}finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecordAsync(ICompanyMaintenanceHandoffClient client,DateTime from,DateTime to,string source,string reason,bool identityVerified,Func<CompanyPersonnelIdentityReference,Task> retainBeforeTransport)
    {
        ArgumentNullException.ThrowIfNull(retainBeforeTransport);if(!CanRecord)throw new InvalidOperationException("Review the complete original account/person first.");var selected=Current();var review=CompanyInventorySourceReview.Copy(Review!);if(review.Context.Company!=selected.Scope || review.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Return to the original identity administrator.");
        var original=new CompanyPersonnelIdentityCommand(review,review.Catalog.Versions.Count(row=>row.UserId==review.Query.UserId)+1,Guid.NewGuid(),from,to,source,reason,identityVerified);original.RequireCatalog(review.Catalog);command=original;reference=new(original.RecordId(),original.IntentId);owner=selected;receipt=null;confirmed=false;Review=null;var version=generation;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try{await retainBeforeTransport(reference);if(disposed || version!=generation || snapshot!=company.Snapshot)throw new OperationCanceledException("Original identity context changed before transport.");var result=await client.RecordCompanyPersonnelIdentityAsync(CompanyInventorySourceReview.Copy(original));original.RequireRecorded(result);if(!disposed && version==generation && snapshot==company.Snapshot){receipt=CompanyInventorySourceReview.Copy(result);confirmed=true;}}finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverAsync(ICompanyMaintenanceHandoffClient client,CompanyPersonnelIdentityReference original,Func<CompanyPersonnelIdentityReference,Task> retainBeforeTransport)
    {
        original.Require();if(Busy || reference is {} retained && retained!=original)throw new InvalidOperationException("Recover the exact retained identity-link reference.");var selected=Current();if(owner is not null && !SameOwner)throw new InvalidOperationException("Return to the original identity-link recorder/company.");owner=selected;reference=original;receipt=null;confirmed=false;Review=null;var version=generation;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try{await retainBeforeTransport(original);if(disposed || version!=generation || snapshot!=company.Snapshot)throw new OperationCanceledException("Original identity recovery context changed before transport.");var result=await client.RecoverCompanyPersonnelIdentityReferenceAsync(original);result.Require(selected.Scope,selected.UserId,original);if(command is not null && JsonSerializer.Serialize(command)!=JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Retained original identity body changed during recovery.");if(!disposed && version==generation && snapshot==company.Snapshot){command=CompanyInventorySourceReview.Copy(result.Command);receipt=CompanyInventorySourceReview.Copy(result.Recorded);confirmed=true;}}finally{Busy=false;Changed?.Invoke();}
    }
    public void BeginNextReview()
    {if(Busy || !SameOwner || !confirmed || command is null || receipt is null)throw new InvalidOperationException("Confirm original identity link before beginning another review.");Current();command.RequireRecorded(receipt);command=null;reference=null;owner=null;receipt=null;Review=null;confirmed=false;generation++;Changed?.Invoke();}
    public void ClearDisplay(){generation++;Review=null;receipt=null;if(confirmed){command=null;reference=null;owner=null;confirmed=false;}Changed?.Invoke();}
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=ClearDisplay;ClearDisplay();}
}
