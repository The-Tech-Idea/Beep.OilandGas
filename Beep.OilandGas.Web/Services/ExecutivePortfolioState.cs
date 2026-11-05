using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed class ExecutivePortfolioState:IDisposable
{
    private readonly CompanyClientContext company;private long version;
    public ExecutivePortfolioState(CompanyClientContext company){this.company=company;company.Changed+=Clear;}
    public long Version=>version;
    public ExecutivePortfolioReview? Current { get; private set; }
    public ExecutiveAsset? Selected { get; private set; }
    public void Clear(){++version;Current=null;Selected=null;}
    public void Select(string id)=>Selected=Current?.Assets.SingleOrDefault(x=>x.FieldId==id);
    public async Task LoadAsync(Func<Task<ExecutivePortfolioReview>> read)
    {
        var scope=company.Snapshot;Clear();var generation=version;if(scope.Selection is null)return;
        var review=await read();if(generation!=version || scope!=company.Snapshot)return;
        ExecutivePortfolioChecks.Require(review,scope.Selection.Scope,scope.Selection.UserId);Current=review;
    }
    public void Dispose(){company.Changed-=Clear;Clear();}
}
