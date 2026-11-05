using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    public async Task<CompanySalesJournalSourceReview> GetCompanySalesJournalSourceAsync(string receivable,string book,string currency,string calendar,DateTime date,CompanySalesJournalMappings selection)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();ArgumentNullException.ThrowIfNull(selection);if(!Guid.TryParseExact(receivable,"N",out var identity) || identity==Guid.Empty || identity.ToString("N")!=receivable)throw new ArgumentException("The original native receivable identity is required.");
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original journal company/account.");_=CompanyLedgerPeriodReview.Create(original.Selection.Scope,original.Selection.UserId,book,currency,calendar,[]);CompanyGLMappingReview.RequireDate(date);
        var query=PeriodQuery(book,currency,calendar)+"&date="+Uri.EscapeDataString(date.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture))+"&receivableKey="+Uri.EscapeDataString(selection.ReceivableKey)+"&revenueKey="+Uri.EscapeDataString(selection.RevenueKey);
        if(selection.ChargesKey is not null)query+="&chargesKey="+Uri.EscapeDataString(selection.ChargesKey);if(selection.TaxKey is not null)query+="&taxKey="+Uri.EscapeDataString(selection.TaxKey);
        var source=await _apiClient.GetAsync<CompanySalesJournalSourceReview>(CompanySalesReceivableRoute+"/journal-source/"+Uri.EscapeDataString(receivable)+query) ??throw new InvalidOperationException("The complete original sales journal source is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during sales journal source review.");source.Require(original.Selection.Scope,original.Selection.UserId,receivable,book,currency,calendar,date,selection);return CompanyInventorySourceReview.Copy(source);
    }
}
