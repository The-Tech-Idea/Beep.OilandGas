using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;

/// <summary>Private account configuration is invalidated on company, account, authority or presentation changes.</summary>
public sealed partial class CompanyLedgerAccountState:IDisposable
{
    private readonly CompanyClientContext company;
    private CompanyGLAccountReview? review;
    private CompanyGLMappingReview? mappings;
    private long version;
    private bool disposed;
    public CompanyLedgerAccountState(CompanyClientContext company){this.company=company;company.Changed+=Invalidate;}
    public event Action? Changed;
    public bool Busy{get;private set;}
    public CompanyGLAccountReview? Review=>review is null?null:CompanyInventorySourceReview.Copy(review);
    public CompanyGLMappingReview? Mappings=>mappings is null?null:CompanyInventorySourceReview.Copy(mappings);
    public void Invalidate(){++version;review=null;mappings=null;configurationReview=null;configurationResult=null;mappingConfigurationReview=null;mappingConfigurationResult=null;mappingTransitionResult=null;periods=null;periodConfigurationReview=null;periodResult=null;periodTransitionReview=null;periodTransitionResult=null;books=null;bookConfigurationReview=null;bookResult=null;fxRates=null;fxConfigurationReview=null;fxResult=null;Changed?.Invoke();}
    public async Task LoadMappingsAsync(string book,string currency,DateTime date,IAccountingServiceClient client)
    {
        if(disposed || Busy || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal))throw new UnauthorizedAccessException();
        _=CompanyGLAccountReview.Create(selected.Scope,selected.UserId,book,currency,[]);CompanyGLMappingReview.RequireDate(date);
        var original=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            var loaded=await client.GetCompanyGLMappingsAsync(book,currency,date) ??throw new InvalidOperationException("The recorded company mappings are unavailable.");
            loaded.Require(selected.Scope,selected.UserId,book,currency,date);
            if(!disposed && generation==version && original==company.Snapshot)mappings=CompanyInventorySourceReview.Copy(loaded);
        }
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task LoadAsync(string book,string currency,IAccountingServiceClient client)
    {
        if(disposed || Busy || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal))
            throw new UnauthorizedAccessException("Current company accounting access is required.");
        _=CompanyGLAccountReview.Create(selected.Scope,selected.UserId,book,currency,[]);
        var original=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            var loaded=await client.GetCompanyGLAccountsAsync(book,currency) ??throw new InvalidOperationException("Recorded company ledger accounts are unavailable.");
            loaded.Require(selected.Scope,selected.UserId,book,currency);
            if(!disposed && generation==version && original==company.Snapshot)review=CompanyInventorySourceReview.Copy(loaded);
        }
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;Invalidate();}
}
