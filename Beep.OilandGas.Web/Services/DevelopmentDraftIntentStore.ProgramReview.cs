using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;
public sealed partial class DevelopmentDraftIntentStore
{
    public async Task<DrillingProgramReviewStart?> ReadProgramReviewAsync(CompanyScope company,string actor)
    {
        var saved=await storage.GetAsync<DrillingProgramReviewStart>(Key(company,actor)+".program-review");if(!saved.Success)return null;var value=saved.Value??throw new InvalidOperationException("The original program review input is unavailable.");value.Require();
        if(value.Scope().Company!=company || value.Scope().ActorUserId!=actor)throw new InvalidOperationException("The retained program belongs to another company or account.");return value;
    }
    public async Task RetainProgramReviewAsync(DrillingProgramReviewStart original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadProgramReviewAsync(scope.Company,scope.ActorUserId);if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original program review before preparing another.");
            await storage.SetAsync(Key(scope.Company,scope.ActorUserId)+".program-review",original);if((await ReadProgramReviewAsync(scope.Company,scope.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original program review input was not retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearProgramReviewAsync(DrillingProgramReviewStart original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadProgramReviewAsync(scope.Company,scope.ActorUserId);if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The original program review input changed and was preserved.");await storage.DeleteAsync(Key(scope.Company,scope.ActorUserId)+".program-review");
        }
        finally{gate.Release();}
    }
}
