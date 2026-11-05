using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public interface IExecutiveServiceClient { Task<ExecutivePortfolioReview> GetPortfolioAsync(); }
public sealed class ExecutiveServiceClient(ApiClient api):IExecutiveServiceClient
{
    public async Task<ExecutivePortfolioReview> GetPortfolioAsync()
    {
        var snapshot=api.CompanySnapshot;
        if(snapshot.Selection is not {} selected || selected.Roles.Count==0)throw new UnauthorizedAccessException("A current company/account and app role are required for executive evidence.");
        var result=await api.GetAsync<ExecutivePortfolioReview>("/api/executive/portfolio");
        if(snapshot!=api.CompanySnapshot)throw new UnauthorizedAccessException("The executive evidence company or account changed during transport.");
        ExecutivePortfolioChecks.Require(result,selected.Scope,selected.UserId);return result!;
    }
}
