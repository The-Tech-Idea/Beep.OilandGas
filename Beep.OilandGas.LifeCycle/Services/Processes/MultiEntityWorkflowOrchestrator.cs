using System.Text.Json;
using Beep.OilandGas.Models.Processes;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Orchestrates multi-entity workflow chains where each step may operate on a different entity type.
/// Example: AFE approved → Cost transactions created → Journal entries posted → Revenue recognized.
/// Each entity's workflow runs as a sub-process linked to the parent chain.
/// Part of Phase 3 cross-role orchestration.
/// </summary>
public interface IMultiEntityWorkflowOrchestrator
{
    /// <summary>
    /// Execute a multi-entity workflow chain. Each link in the chain starts a sub-process
    /// for a different entity type. Links execute sequentially unless configured otherwise.
    /// </summary>
    Task<ChainExecutionResult> ExecuteChainAsync(
        string chainDefinitionId,
        string initiatingEntityType,
        string initiatingEntityId,
        string fieldId,
        Dictionary<string, object> chainContext,
        string userId);

    /// <summary>
    /// Get the current state of all entities in a running chain.
    /// </summary>
    Task<ChainState> GetChainStateAsync(string chainInstanceId);
}

public class ChainExecutionResult
{
    public string? ChainInstanceId { get; set; }
    public bool Success { get; set; }
    public List<ChainLinkResult> LinkResults { get; set; } = new();
    public bool AllLinksCompleted { get; set; }
    public string? CurrentLinkId { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ChainLinkResult
{
    public int Sequence { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string? ProcessInstanceId { get; set; }
    public string Status { get; set; } = "PENDING";
    public string? Error { get; set; }
}

public class ChainState
{
    public string ChainInstanceId { get; set; } = string.Empty;
    public Dictionary<string, string> EntityStates { get; set; } = new();
    public string OverallStatus { get; set; } = "IN_PROGRESS";
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public List<ChainLinkResult> Links { get; set; } = new();
}

/// <summary>
/// Pre-defined multi-entity chains that model real O&G process flows.
/// </summary>
public static class ChainDefinitions
{
    /// <summary>AFE → Cost Tracking → Journal Entry → Revenue Recognition</summary>
    public static readonly ChainTemplate AfeToRevenue = new()
    {
        ChainId = "CHAIN_AFE_TO_REVENUE",
        ChainName = "AFE → Cost → Journal → Revenue",
        Description = "Full financial lifecycle: approve AFE, track costs, post journals, recognize revenue, calculate royalties",
        Links = new List<ChainLinkTemplate>
        {
            new() { Sequence = 1, EntityType = "AFE", ProcessDefId = "AFE_DOA_APPROVAL", WaitForCompletion = true },
            new() { Sequence = 2, EntityType = "COST_TRANSACTION", ProcessDefId = "CRW_AFE_COST_TRACKING", WaitForCompletion = false },
            new() { Sequence = 3, EntityType = "JOURNAL_ENTRY", ProcessDefId = "CRW_PERIOD_CLOSE", WaitForCompletion = true },
            new() { Sequence = 4, EntityType = "REVENUE_TRANSACTION", ProcessDefId = "CRW_PRODUCTION_TO_REVENUE", WaitForCompletion = true },
            new() { Sequence = 5, EntityType = "ROYALTY_CALCULATION", ProcessDefId = "CRW_ROYALTY_CALCULATION", WaitForCompletion = true },
        },
    };

    /// <summary>Production → Revenue → Royalty</summary>
    public static readonly ChainTemplate ProductionToRoyalty = new()
    {
        ChainId = "CHAIN_PRODUCTION_TO_ROYALTY",
        ChainName = "Production → Revenue → Royalty",
        Description = "Monthly production posted → revenue recognized → royalties calculated → owner payments",
        Links = new List<ChainLinkTemplate>
        {
            new() { Sequence = 1, EntityType = "PDEN_VOL_SUMMARY", ProcessDefId = "COMPLIANCE_PRODUCTION_REPORTING", WaitForCompletion = true },
            new() { Sequence = 2, EntityType = "REVENUE_TRANSACTION", ProcessDefId = "CRW_PRODUCTION_TO_REVENUE", WaitForCompletion = true },
            new() { Sequence = 3, EntityType = "ROYALTY_CALCULATION", ProcessDefId = "CRW_ROYALTY_CALCULATION", WaitForCompletion = true },
        },
    };

    /// <summary>Incident → Investigation → Corrective Action</summary>
    public static readonly ChainTemplate IncidentToCorrectiveAction = new()
    {
        ChainId = "CHAIN_INCIDENT_TO_CORRECTIVE",
        ChainName = "Incident → Investigation → Corrective Action",
        Description = "HSE incident reported → investigation → root cause analysis → corrective action → verification",
        Links = new List<ChainLinkTemplate>
        {
            new() { Sequence = 1, EntityType = "HSE_INCIDENT", ProcessDefId = "HSE_INCIDENT_REPORTING", WaitForCompletion = true },
            new() { Sequence = 2, EntityType = "HSE_INCIDENT", ProcessDefId = "HSE_HAZID", WaitForCompletion = true },
            new() { Sequence = 3, EntityType = "HSE_INCIDENT", ProcessDefId = "HSE_SAFETY_DRILL", WaitForCompletion = true },
            new() { Sequence = 4, EntityType = "HSE_INCIDENT", ProcessDefId = "HSE_AUDIT", WaitForCompletion = true },
        },
    };
}

public class ChainTemplate
{
    public string ChainId { get; set; } = string.Empty;
    public string ChainName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ChainLinkTemplate> Links { get; set; } = new();
}

public class ChainLinkTemplate
{
    public int Sequence { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string ProcessDefId { get; set; } = string.Empty;
    public bool WaitForCompletion { get; set; } = true;
}

public class MultiEntityWorkflowOrchestrator : IMultiEntityWorkflowOrchestrator
{
    private readonly IProcessService _processService;
    private readonly ILogger<MultiEntityWorkflowOrchestrator> _logger;

    private static readonly Dictionary<string, ChainTemplate> BuiltInChains = new(StringComparer.OrdinalIgnoreCase)
    {
        [ChainDefinitions.AfeToRevenue.ChainId] = ChainDefinitions.AfeToRevenue,
        [ChainDefinitions.ProductionToRoyalty.ChainId] = ChainDefinitions.ProductionToRoyalty,
        [ChainDefinitions.IncidentToCorrectiveAction.ChainId] = ChainDefinitions.IncidentToCorrectiveAction,
    };

    public MultiEntityWorkflowOrchestrator(
        IProcessService processService,
        ILogger<MultiEntityWorkflowOrchestrator>? logger = null)
    {
        _processService = processService;
        _logger = logger;
    }

    public async Task<ChainExecutionResult> ExecuteChainAsync(
        string chainDefinitionId,
        string initiatingEntityType,
        string initiatingEntityId,
        string fieldId,
        Dictionary<string, object> chainContext,
        string userId)
    {
        var result = new ChainExecutionResult { Success = true };

        if (!BuiltInChains.TryGetValue(chainDefinitionId, out var chain))
        {
            result.Success = false;
            result.Errors.Add($"Chain definition '{chainDefinitionId}' not found.");
            return result;
        }

        _logger?.LogInformation(
            "Starting chain {ChainName} ({ChainId}) for {EntityType}/{EntityId}",
            chain.ChainName, chain.ChainId, initiatingEntityType, initiatingEntityId);

        // Start the first link using the initiating entity
        ProcessInstance? previousInstance = null;

        foreach (var link in chain.Links.OrderBy(l => l.Sequence))
        {
            try
            {
                var entityId = link.Sequence == 1
                    ? initiatingEntityId
                    : chainContext.GetValueOrDefault($"{link.EntityType}_ID")?.ToString() ?? initiatingEntityId;

                var instance = await _processService.StartProcessAsync(
                    link.ProcessDefId, entityId, link.EntityType, fieldId, userId);

                var linkResult = new ChainLinkResult
                {
                    Sequence = link.Sequence,
                    EntityType = link.EntityType,
                    EntityId = entityId,
                    ProcessDefinitionId = link.ProcessDefId,
                    ProcessInstanceId = instance.InstanceId,
                    Status = "STARTED",
                };

                result.LinkResults.Add(linkResult);
                result.CurrentLinkId = linkResult.ProcessInstanceId;

                // If this is a child of a previous link, spawn as sub-process
                if (previousInstance is not null && _processService is ProcessServiceBase baseService)
                {
                    await baseService.SpawnSubProcessAsync(
                        previousInstance.InstanceId,
                        link.ProcessDefId,
                        link.EntityType,
                        entityId,
                        fieldId,
                        userId,
                        chainContext);
                }

                if (link.WaitForCompletion)
                {
                    // Poll for completion (in production, this would be event-driven via SignalR)
                    var completed = await WaitForProcessCompletionAsync(instance.InstanceId, TimeSpan.FromMinutes(5));
                    linkResult.Status = completed ? "COMPLETED" : "TIMED_OUT_WAITING";
                }

                previousInstance = instance;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Link {link.Sequence} ({link.EntityType}) failed: {ex.Message}");
                result.LinkResults.Add(new ChainLinkResult
                {
                    Sequence = link.Sequence,
                    EntityType = link.EntityType,
                    ProcessDefinitionId = link.ProcessDefId,
                    Status = "FAILED",
                    Error = ex.Message,
                });
                _logger?.LogError(ex, "Chain link {Sequence} failed for {ChainId}", link.Sequence, chainDefinitionId);
            }
        }

        result.AllLinksCompleted = result.LinkResults.All(l => l.Status is "COMPLETED" or "STARTED");
        result.Success = result.Errors.Count == 0;

        _logger?.LogInformation(
            "Chain {ChainName} completed: {CompletedLinks}/{TotalLinks} links, Success={Success}",
            chain.ChainName, result.LinkResults.Count(l => l.Status == "COMPLETED"),
            chain.Links.Count, result.Success);

        return result;
    }

    public async Task<ChainState> GetChainStateAsync(string chainInstanceId)
    {
        // Chain state is reconstructed from the sub-process instances
        var state = new ChainState { ChainInstanceId = chainInstanceId };

        try
        {
            var parentInstance = await _processService.GetProcessInstanceAsync(chainInstanceId);
            if (parentInstance is not null)
            {
                state.OverallStatus = parentInstance.Status.ToString();
                state.LastUpdated = parentInstance.CompletionDate ?? DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to get chain state for {ChainId}", chainInstanceId);
        }

        return state;
    }

    private async Task<bool> WaitForProcessCompletionAsync(string instanceId, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            var instance = await _processService.GetProcessInstanceAsync(instanceId);
            if (instance?.Status is ProcessStatus.COMPLETED or ProcessStatus.CANCELLED)
                return instance.Status == ProcessStatus.COMPLETED;
            await Task.Delay(1000);
        }
        return false;
    }
}
