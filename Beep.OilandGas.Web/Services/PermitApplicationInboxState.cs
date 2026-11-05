using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.Security;
using Microsoft.AspNetCore.Components.Authorization;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>Returned-record queues under the current app roles and company/FIELD. Commands always review their own fresh source.</summary>
public sealed class PermitApplicationInboxState:IDisposable
{
    private readonly CompanyClientContext company;private readonly IFieldSelectionContext fields;private readonly AuthenticationStateProvider authentication;private readonly IPermitServiceClient client;
    private long generation;private bool disposed;private CancellationTokenSource? active;
    public PermitApplicationInboxState(CompanyClientContext company,IFieldSelectionContext fields,AuthenticationStateProvider authentication,IPermitServiceClient client)
    {this.company=company;this.fields=fields;this.authentication=authentication;this.client=client;company.Changed+=Clear;fields.CurrentFieldChanged+=FieldChanged;authentication.AuthenticationStateChanged+=AuthChanged;}
    public long Epoch=>generation;public event Action? Changed;public bool Busy{get;private set;}public PermitApplicationInboxReview? Review{get;private set;}
    public IReadOnlyList<PermitApplicationSummary> Applications{get;private set;}=[];
    public string SelectedQueue{get;private set;}="All returned";public string Search{get;private set;}="";public int Page{get;private set;}
    public bool CanAuthor=>Review?.Authority.CanAuthor==true && company.Selection?.Permissions.Contains(PermissionConstants.HSE.IssuePermit)==true;
    public bool CanReview=>Review?.Authority.CanReview==true && company.Selection?.Permissions.Contains(PermissionConstants.HSE.ApprovePermit)==true;
    public string Category(PermitApplicationSummary row)=>Review is {} review?PermitApplicationInboxReview.Queue(row,review.Authority.Context.Field.ActorUserId,CanAuthor,CanReview):"Review current FIELD";
    public int Count(string queue)=>queue=="All returned"?Applications.Count:Applications.Count(row=>Category(row)==queue);
    private IEnumerable<PermitApplicationSummary> Filtered=>Applications.Where(row=>(SelectedQueue=="All returned" || Category(row)==SelectedQueue) &&
        (string.IsNullOrWhiteSpace(Search) || new[]{row.PermitApplicationId,row.ApplicantName,row.ApplicationType,row.RegulatoryAuthority,row.Country,row.Status}.Any(value=>value?.Contains(Search.Trim(),StringComparison.OrdinalIgnoreCase)==true)));
    public int FilteredCount=>Filtered.Count();public IReadOnlyList<PermitApplicationSummary> PageItems=>Filtered.Skip(Page*10).Take(10).ToArray();
    public void Select(string queue){if(!new[]{"All returned","Drafts","Corrections","Independent review","Other returned records"}.Contains(queue,StringComparer.Ordinal))throw new InvalidOperationException("Choose a recorded permit group.");SelectedQueue=queue;Page=0;Changed?.Invoke();}
    public void SetSearch(string value){Search=value??"";Page=0;Changed?.Invoke();}
    public void Previous(){Page=Math.Max(0,Page-1);Changed?.Invoke();}public void Next(){if((Page+1)*10<FilteredCount)Page++;Changed?.Invoke();}
    private void FieldChanged(string _)=>Clear();private void AuthChanged(Task<AuthenticationState> _)=>Clear();
    private void Clear(){generation++;active?.Cancel();Review=null;Applications=[];Busy=false;SelectedQueue="All returned";Search="";Page=0;Changed?.Invoke();}
    public async Task LoadAsync()
    {
        Clear();var epoch=generation;var selection=company.Snapshot;using var cancel=new CancellationTokenSource();active=cancel;Busy=true;Changed?.Invoke();
        void Current(){if(disposed || epoch!=generation || company.Snapshot!=selection)throw new OperationCanceledException("The permit inbox selection changed.");}
        async Task<FieldSelectionReview> Authority()
        {
            Current();var selected=selection.Selection;var user=(await authentication.GetAuthenticationStateAsync()).User;Current();
            if(selected is null || user.Identity?.IsAuthenticated!=true || user.FindFirst("party_id")?.Value!=selected.UserId || !selected.Roles.Any(user.IsInRole) || !selected.Permissions.Contains(PermissionConstants.HSE.ViewPermits))
                throw new InvalidOperationException("Current standard app roles and permit-view authority are required.");
            var field=await fields.GetCurrentFieldSelectionAsync(cancel.Token);Current();FieldSelectionReviewValidator.RequireCurrent(field,selected.Scope,selected.UserId);
            if(!field.SelectionAvailable)throw new InvalidOperationException("Choose an available company FIELD.");return field.SelectionContext!;
        }
        try
        {
            var field=await Authority();var reviewed=await client.ReadInboxAsync(field,cancel.Token);reviewed.RequireFor(field);Current();
            if(await Authority()!=field)throw new InvalidOperationException("The permit inbox FIELD changed while reading its records.");
            Review=reviewed;Applications=reviewed.ReadApplications().OrderBy(row=>row.PermitApplicationId,StringComparer.Ordinal).ToArray();
        }
        finally{if(ReferenceEquals(active,cancel))active=null;if(!disposed && epoch==generation){Busy=false;Changed?.Invoke();}}
    }
    public void Dispose(){if(disposed)return;disposed=true;generation++;active?.Cancel();company.Changed-=Clear;fields.CurrentFieldChanged-=FieldChanged;authentication.AuthenticationStateChanged-=AuthChanged;Changed=null;}
}
