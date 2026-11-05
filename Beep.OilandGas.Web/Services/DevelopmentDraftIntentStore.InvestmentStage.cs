using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;
public sealed partial class DevelopmentDraftIntentStore
{
    public async Task<DevelopmentInvestmentStageDecision?> ReadInvestmentStageAsync(CompanyScope company,string actor)
    {
        var saved=await storage.GetAsync<DevelopmentInvestmentStageDecision>(Key(company,actor)+".investment-stage");if(!saved.Success)return null;
        var original=saved.Value??throw new InvalidOperationException("The retained original investment stage is unavailable.");original.Require();
        if(original.Scope().Company!=company || original.Scope().ActorUserId!=actor)throw new InvalidOperationException("The retained investment stage belongs to another original company or actor.");return original;
    }
    public async Task RetainInvestmentStageAsync(DevelopmentInvestmentStageDecision original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadInvestmentStageAsync(scope.Company,scope.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original investment stage before preparing another decision.");
            await storage.SetAsync(Key(scope.Company,scope.ActorUserId)+".investment-stage",original);
            if((await ReadInvestmentStageAsync(scope.Company,scope.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original stage recovery input was not retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearInvestmentStageAsync(DevelopmentInvestmentStageDecision original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadInvestmentStageAsync(scope.Company,scope.ActorUserId);
            if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The retained stage input changed. Its original recovery data was preserved.");
            await storage.DeleteAsync(Key(scope.Company,scope.ActorUserId)+".investment-stage");
        }
        finally{gate.Release();}
    }
}
