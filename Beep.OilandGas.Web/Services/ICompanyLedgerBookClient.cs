using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Web.Services;
/// <summary>Book reads supplied by the existing accounting HTTP client.</summary>
public interface ICompanyLedgerBookClient
{
    Task<CompanyLedgerBookReview> GetCompanyLedgerBooksAsync();
}
