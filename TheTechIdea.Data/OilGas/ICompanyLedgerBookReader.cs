namespace TheTechIdea.Data.OilGas;
/// <summary>Read-only access to the existing configured native company-book owner.</summary>
public interface ICompanyLedgerBookReader
{
    Task<CompanyLedgerBookReview> GetCompanyBooksAsync(CompanyScope company,string actor,Func<Task> requireAuthority);
}
