using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
/// <summary>Book reads supplied by the existing accounting HTTP client.</summary>
public interface ICompanyLedgerBookClient
{
    Task<CompanyLedgerBookReview> GetCompanyLedgerBooksAsync();
}
