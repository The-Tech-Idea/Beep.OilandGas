using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;
public sealed partial class DevelopmentDraftIntentStore
{
    public async Task<CreateDrillingProgramDraft?> ReadProgramAsync(CompanyScope company,string actor)
    {
        var saved=await storage.GetAsync<CreateDrillingProgramDraft>(Key(company,actor)+".drilling-program");if(!saved.Success)return null;var value=saved.Value??throw new InvalidOperationException("The original drilling program input is unavailable.");value.Require();
        if(value.Basis.Scope().Company!=company || value.Basis.Scope().ActorUserId!=actor)throw new InvalidOperationException("The retained program belongs to another company or account.");return value;
    }
    public async Task RetainProgramAsync(CreateDrillingProgramDraft original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Basis.Scope();var previous=await ReadProgramAsync(scope.Company,scope.ActorUserId);if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original drilling program before preparing another.");
            await storage.SetAsync(Key(scope.Company,scope.ActorUserId)+".drilling-program",original);if((await ReadProgramAsync(scope.Company,scope.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original program input was not retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearProgramAsync(CreateDrillingProgramDraft original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Basis.Scope();var previous=await ReadProgramAsync(scope.Company,scope.ActorUserId);if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The original program input changed and was preserved.");await storage.DeleteAsync(Key(scope.Company,scope.ActorUserId)+".drilling-program");
        }
        finally{gate.Release();}
    }
}
