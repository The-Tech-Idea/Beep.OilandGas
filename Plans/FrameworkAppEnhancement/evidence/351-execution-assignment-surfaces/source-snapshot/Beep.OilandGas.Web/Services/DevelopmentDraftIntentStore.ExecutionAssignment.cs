using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;
public sealed partial class DevelopmentDraftIntentStore
{
    public async Task<DrillingExecutionAssignment?> ReadExecutionAssignmentAsync(CompanyScope company,string actor)
    {
        var saved=await storage.GetAsync<DrillingExecutionAssignment>(Key(company,actor)+".drilling-execution-assignment");if(!saved.Success)return null;var value=saved.Value??throw new InvalidOperationException("The original execution reporting assignment input is unavailable.");value.Require();
        if(value.Scope().Company!=company || value.Scope().ActorUserId!=actor)throw new InvalidOperationException("The retained execution assignment belongs to another company or account.");return value;
    }
    public async Task RetainExecutionAssignmentAsync(DrillingExecutionAssignment original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadExecutionAssignmentAsync(scope.Company,scope.ActorUserId);if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("Recover the original execution reporting assignment before preparing another.");
            await storage.SetAsync(Key(scope.Company,scope.ActorUserId)+".drilling-execution-assignment",original);if((await ReadExecutionAssignmentAsync(scope.Company,scope.ActorUserId))?.Revision()!=original.Revision())throw new InvalidOperationException("The original execution reporting assignment input was not retained.");
        }
        finally{gate.Release();}
    }
    public async Task ClearExecutionAssignmentAsync(DrillingExecutionAssignment original)
    {
        original.Require();await gate.WaitAsync();
        try
        {
            var scope=original.Scope();var previous=await ReadExecutionAssignmentAsync(scope.Company,scope.ActorUserId);if(previous is not null && previous.Revision()!=original.Revision())throw new InvalidOperationException("The original execution reporting assignment input changed and was preserved.");await storage.DeleteAsync(Key(scope.Company,scope.ActorUserId)+".drilling-execution-assignment");
        }
        finally{gate.Release();}
    }
}
