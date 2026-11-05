using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
namespace Beep.OilandGas.Accounting.Models;
/// <summary>Read-only access to the existing configured native company-book owner.</summary>
public interface ICompanyLedgerBookReader
{
    Task<CompanyLedgerBookReview> GetCompanyBooksAsync(CompanyScope company,string actor,Func<Task> requireAuthority);
}
