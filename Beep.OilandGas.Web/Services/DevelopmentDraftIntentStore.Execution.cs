using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;
public sealed partial class DevelopmentDraftIntentStore
{
    public async Task<DrillingExecutionStart?> ReadExecutionAsync(CompanyScope company,string actor)
    {
        var saved=await storage.GetAsync<DrillingExecutionStart>(Key(company,actor)+".drilling-execution");if(!saved.Success)return null;var value=saved.Value??throw new InvalidOperationException("The original execution start input is unavailable.");value.Require();
        if(value.Basis.Scope().Company!=company || value.Basis.Scope().ActorUserId!=actor)throw new InvalidOperationException("The retained execution belongs to another company or account.");return value;
    }
    public async Task RetainExecutionAsync(DrillingExecutionStart original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Basis.Scope();var previous=await ReadExecutionAsync(scope.Company,scope.ActorUserId);if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original execution start before preparing another.");
            await storage.SetAsync(Key(scope.Company,scope.ActorUserId)+".drilling-execution",original);if((await ReadExecutionAsync(scope.Company,scope.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original execution start input was not retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearExecutionAsync(DrillingExecutionStart original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Basis.Scope();var previous=await ReadExecutionAsync(scope.Company,scope.ActorUserId);if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The original execution start input changed and was preserved.");await storage.DeleteAsync(Key(scope.Company,scope.ActorUserId)+".drilling-execution");
        }
        finally{gate.Release();}
    }
}
