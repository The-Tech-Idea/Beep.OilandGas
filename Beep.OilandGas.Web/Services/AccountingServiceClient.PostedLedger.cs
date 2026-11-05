using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    public async Task<CompanySalesPostedLedgerReview> GetCompanySalesPostedLedgerAsync(string receivable,string book,string currency)
    {
        var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original ledger company/account.");_=CompanyGLAccountReview.Create(selected.Scope,selected.UserId,book,currency,[]);if(!Guid.TryParseExact(receivable,"N",out var id) || id==Guid.Empty || id.ToString("N")!=receivable)throw new ArgumentException("The original receivable identity is required.");
        var result=await _apiClient.GetAsync<CompanySalesPostedLedgerReview>(CompanySalesReceivableRoute+"/posted-ledger/"+Uri.EscapeDataString(receivable)+"?book="+Uri.EscapeDataString(book)+"&currency="+Uri.EscapeDataString(currency))??throw new InvalidOperationException("The complete original posted ledger is unavailable.");if(snapshot!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during posted ledger read.");result.Require(selected.Scope,selected.UserId,book,currency,receivable);return CompanyInventorySourceReview.Copy(result);
    }
}
