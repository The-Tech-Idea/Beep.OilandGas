using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;

namespace Beep.OilandGas.Web.Services;
public sealed partial class DevelopmentDraftIntentStore
{
    public async Task<DrillingProgramReviewDecision?> ReadProgramDecisionAsync(CompanyScope company,string actor)
    {
        var saved=await storage.GetAsync<DrillingProgramReviewDecision>(Key(company,actor)+".program-decision");if(!saved.Success)return null;var value=saved.Value??throw new InvalidOperationException("The original program decision input is unavailable.");value.Require();
        if(value.Scope().Company!=company || value.Scope().ActorUserId!=actor)throw new InvalidOperationException("The retained program belongs to another company or account.");return value;
    }
    public async Task RetainProgramDecisionAsync(DrillingProgramReviewDecision original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadProgramDecisionAsync(scope.Company,scope.ActorUserId);if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original program decision before preparing another.");
            await storage.SetAsync(Key(scope.Company,scope.ActorUserId)+".program-decision",original);if((await ReadProgramDecisionAsync(scope.Company,scope.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original program decision input was not retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearProgramDecisionAsync(DrillingProgramReviewDecision original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadProgramDecisionAsync(scope.Company,scope.ActorUserId);if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The original program decision input changed and was preserved.");await storage.DeleteAsync(Key(scope.Company,scope.ActorUserId)+".program-decision");
        }
        finally{gate.Release();}
    }
}
