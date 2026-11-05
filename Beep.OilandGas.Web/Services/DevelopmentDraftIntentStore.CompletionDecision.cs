using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;

namespace Beep.OilandGas.Web.Services;
public sealed partial class DevelopmentDraftIntentStore
{
    public async Task<DrillingCompletionReviewDecision?> ReadCompletionDecisionAsync(CompanyScope company,string actor)
    {
        var saved=await storage.GetAsync<DrillingCompletionReviewDecision>(Key(company,actor)+".drilling-completion-decision");if(!saved.Success)return null;var value=saved.Value??throw new InvalidOperationException("The original completion decision input is unavailable.");value.Require();
        if(value.Scope().Company!=company || value.Scope().ActorUserId!=actor)throw new InvalidOperationException("The retained completion decision belongs to another company or account.");return value;
    }
    public async Task RetainCompletionDecisionAsync(DrillingCompletionReviewDecision original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadCompletionDecisionAsync(scope.Company,scope.ActorUserId);if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original completion decision before preparing another.");
            await storage.SetAsync(Key(scope.Company,scope.ActorUserId)+".drilling-completion-decision",original);if((await ReadCompletionDecisionAsync(scope.Company,scope.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original completion decision input was not retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearCompletionDecisionAsync(DrillingCompletionReviewDecision original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadCompletionDecisionAsync(scope.Company,scope.ActorUserId);if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The original completion decision input changed and was preserved.");await storage.DeleteAsync(Key(scope.Company,scope.ActorUserId)+".drilling-completion-decision");
        }
        finally{gate.Release();}
    }
}
