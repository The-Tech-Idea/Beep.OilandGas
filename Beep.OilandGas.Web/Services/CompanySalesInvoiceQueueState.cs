using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;

public sealed class CompanySalesInvoiceQueueState:IDisposable
{
    private readonly CompanyClientContext company;
    private CompanySalesInvoiceQueueReview? review;
    private bool disposed;
    public int Version{get;private set;}
    public bool Busy{get;private set;}
    public bool NeedsField{get;private set;}
    public CompanySalesInvoiceQueueReview? Review=>review is null?null:CompanyInventorySourceReview.Copy(review);
    public CompanySalesInvoiceQueueState(CompanyClientContext company){this.company=company;company.Changed+=Clear;}
    public void Clear(){++Version;review=null;Busy=false;NeedsField=false;}
    public async Task LoadAsync(Func<Task<string?>> currentField,Func<string,Task<CompanySalesInvoiceQueueReview>> read)
    {
        Clear();var version=Version;var snapshot=company.Snapshot;var selected=snapshot.Selection;
        bool Current()=>!disposed && version==Version && snapshot==company.Snapshot;
        if(selected is null || !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current company Accounting.View is required.");
        Busy=true;
        try
        {
            var field=await currentField();if(!Current())return;if(string.IsNullOrWhiteSpace(field)){NeedsField=true;return;}
            var result=CompanyInventorySourceReview.Copy(await read(field));if(!Current())return;
            result.Require(selected.Scope,selected.UserId,field);if(!Current())return;review=result;
        }
        finally{if(Current())Busy=false;}
    }
    public void Dispose(){disposed=true;company.Changed-=Clear;Clear();}
}
