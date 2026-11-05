using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;
public sealed partial class DevelopmentDraftIntentStore
{
    public async Task<DrillingCompletionReviewStart?> ReadCompletionReviewAsync(CompanyScope company,string actor)
    {
        var saved=await storage.GetAsync<DrillingCompletionReviewStart>(Key(company,actor)+".drilling-completion-review");if(!saved.Success)return null;var value=saved.Value??throw new InvalidOperationException("The original completion review input is unavailable.");value.Require();
        if(value.Scope().Company!=company || value.Scope().ActorUserId!=actor)throw new InvalidOperationException("The retained completion review belongs to another company or account.");return value;
    }
    public async Task RetainCompletionReviewAsync(DrillingCompletionReviewStart original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadCompletionReviewAsync(scope.Company,scope.ActorUserId);if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original completion review before preparing another.");
            await storage.SetAsync(Key(scope.Company,scope.ActorUserId)+".drilling-completion-review",original);if((await ReadCompletionReviewAsync(scope.Company,scope.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original completion review input was not retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearCompletionReviewAsync(DrillingCompletionReviewStart original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadCompletionReviewAsync(scope.Company,scope.ActorUserId);if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The original completion review input changed and was preserved.");await storage.DeleteAsync(Key(scope.Company,scope.ActorUserId)+".drilling-completion-review");
        }
        finally{gate.Release();}
    }
}
