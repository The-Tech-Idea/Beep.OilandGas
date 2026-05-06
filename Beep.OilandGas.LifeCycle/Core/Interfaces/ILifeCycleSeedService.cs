using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.LifeCycle.Core.Interfaces;

public interface ILifeCycleSeedService
{
    Task<LifeCycleSeedResult> SeedAllAsync(
        string connectionName,
        string userId,
        CancellationToken cancellationToken = default);
}

public sealed class LifeCycleSeedResult
{
    public bool Success { get; set; }
    public int LifecycleStatesInserted { get; set; }
    public int ProcessDefinitionsInserted { get; set; }
    public int ProcessStepsInserted { get; set; }
    public int SlaTemplatesInserted { get; set; }
    public int ApprovalChainsInserted { get; set; }
    public int TotalRecordsInserted =>
        LifecycleStatesInserted + ProcessDefinitionsInserted + ProcessStepsInserted +
        SlaTemplatesInserted + ApprovalChainsInserted;
    public int TablesSeeded { get; set; }
    public List<string> Errors { get; set; } = new();
}
