#if ACCOUNTING_WORKFLOW_SEED_FOCUSED
using Beep.OilandGas.Models.Processes;
namespace Beep.OilandGas.LifeCycle.Services.Processes;

// Test-only partial boundary for the complete actual operational seed file.
// It does not implement the production definition service or claim persistence/skip-existing behavior.
public partial class ProcessDefinitionInitializer
{
    public List<(ProcessDefinition Definition,string Actor)> RecordedDefinitions {get;}=[];
    private Task CreateProcessDefinitionIfNotExistsAsync(ProcessDefinition definition,string userId)
    {RecordedDefinitions.Add((definition,userId));return Task.CompletedTask;}
}
#endif
