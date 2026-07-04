using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Manages cross-workflow dependency gates.
/// Workflow B Step Y cannot start until Workflow A Step X completes.
/// Supports BLOCKING (hard gate), ADVISORY (warning), and CONDITIONAL (expression-based).
/// Part of Phase 3 cross-role orchestration.
/// </summary>
public interface IWorkflowDependencyGraphService
{
    /// <summary>
    /// Define a dependency: dependent process step cannot start until prerequisite process step completes.
    /// </summary>
    Task<WORKFLOW_DEPENDENCY> AddDependencyAsync(
        string dependentProcessDefId, string? dependentStepId,
        string prerequisiteProcessDefId, string? prerequisiteStepId,
        string dependencyType, string? conditionExpression, string userId);

    /// <summary>
    /// Check if all prerequisites for a step are satisfied.
    /// Returns whether the step can proceed and details of any unsatisfied prerequisites.
    /// </summary>
    Task<DependencyCheckResult> CheckPrerequisitesAsync(
        string processInstanceId, string? stepId);

    /// <summary>
    /// Get all dependencies where the given process definition is the dependent.
    /// </summary>
    Task<List<WORKFLOW_DEPENDENCY>> GetDependenciesAsync(string processDefinitionId);

    /// <summary>
    /// Get the full dependency graph for visualization.
    /// </summary>
    Task<List<WORKFLOW_DEPENDENCY>> GetFullGraphAsync();
}

public class DependencyCheckResult
{
    public bool CanProceed { get; set; } = true;
    public List<string> SatisfiedPrerequisites { get; set; } = new();
    public List<string> UnsatisfiedPrerequisites { get; set; } = new();
    public List<string> AdvisoryWarnings { get; set; } = new();
    public string? BlockingReason { get; set; }
}

public class WorkflowDependencyGraphService : IWorkflowDependencyGraphService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<WorkflowDependencyGraphService> _logger;

    public WorkflowDependencyGraphService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName = "PPDM39",
        ILogger<WorkflowDependencyGraphService>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    public async Task<WORKFLOW_DEPENDENCY> AddDependencyAsync(
        string dependentProcessDefId, string? dependentStepId,
        string prerequisiteProcessDefId, string? prerequisiteStepId,
        string dependencyType, string? conditionExpression, string userId)
    {
        var dep = new WORKFLOW_DEPENDENCY
        {
            DEPENDENT_PROCESS_DEF_ID = dependentProcessDefId,
            DEPENDENT_STEP_ID = dependentStepId,
            PREREQUISITE_PROCESS_DEF_ID = prerequisiteProcessDefId,
            PREREQUISITE_STEP_ID = prerequisiteStepId,
            DEPENDENCY_TYPE = dependencyType,
            CONDITION_EXPRESSION = conditionExpression,
            DESCRIPTION = $"{dependentProcessDefId} depends on {prerequisiteProcessDefId}",
        };

        var repo = GetRepo();
        await repo.InsertAsync(dep, userId);

        _logger?.LogInformation(
            "Dependency created: {Dependent}[{DependentStep}] → depends on → {Prerequisite}[{PrerequisiteStep}] ({Type})",
            dependentProcessDefId, dependentStepId, prerequisiteProcessDefId, prerequisiteStepId, dependencyType);

        return dep;
    }

    public async Task<DependencyCheckResult> CheckPrerequisitesAsync(
        string processInstanceId, string? stepId)
    {
        var result = new DependencyCheckResult();

        // Get process instance to find the process definition
        var instanceRepo = new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROCESS_INSTANCE), _connectionName, "PROCESS_INSTANCE", null);

        var instanceFilters = new List<AppFilter>
        {
            new() { FieldName = "PROCESS_INSTANCE_ID", FilterValue = processInstanceId },
        };
        var instances = (await instanceRepo.GetAsync(instanceFilters))
            .OfType<PROCESS_INSTANCE>().ToList();

        if (instances.Count == 0)
            return result;

        var processDefId = instances[0].PROCESS_DEFINITION_ID;

        // Find all dependencies where this process/step is the dependent
        var deps = await GetDependenciesAsync(processDefId);
        if (!string.IsNullOrWhiteSpace(stepId))
        {
            deps = deps.Where(d =>
                string.IsNullOrWhiteSpace(d.DEPENDENT_STEP_ID) ||
                string.Equals(d.DEPENDENT_STEP_ID, stepId, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (deps.Count == 0)
            return result;

        // Check each prerequisite
        var processRepo = new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROCESS_INSTANCE), _connectionName, "PROCESS_INSTANCE", null);

        foreach (var dep in deps)
        {
            var prereqFilters = new List<AppFilter>
            {
                new() { FieldName = "PROCESS_DEFINITION_ID", FilterValue = dep.PREREQUISITE_PROCESS_DEF_ID },
                new() { FieldName = "ENTITY_TYPE", FilterValue = instances[0].ENTITY_TYPE },
                new() { FieldName = "ENTITY_ID", FilterValue = instances[0].ENTITY_ID },
            };

            var prereqInstances = (await processRepo.GetAsync(prereqFilters))
                .OfType<PROCESS_INSTANCE>().ToList();

            var prereqComplete = prereqInstances.Any(p =>
                p.STATUS == "COMPLETED" &&
                (string.IsNullOrWhiteSpace(dep.PREREQUISITE_STEP_ID) ||
                 p.CURRENT_STEP_ID == dep.PREREQUISITE_STEP_ID));

            if (prereqComplete)
            {
                result.SatisfiedPrerequisites.Add(
                    $"{dep.PREREQUISITE_PROCESS_DEF_ID}/{dep.PREREQUISITE_STEP_ID ?? "*"}");
            }
            else
            {
                if (dep.DEPENDENCY_TYPE == "BLOCKING")
                {
                    result.UnsatisfiedPrerequisites.Add(
                        $"{dep.PREREQUISITE_PROCESS_DEF_ID}/{dep.PREREQUISITE_STEP_ID ?? "*"}");
                    result.CanProceed = false;
                }
                else if (dep.DEPENDENCY_TYPE == "ADVISORY")
                {
                    result.AdvisoryWarnings.Add(
                        $"Advisory: {dep.PREREQUISITE_PROCESS_DEF_ID} not yet complete");
                }
            }
        }

        if (!result.CanProceed)
        {
            result.BlockingReason = $"Unsatisfied prerequisites: {string.Join(", ", result.UnsatisfiedPrerequisites)}";
        }

        return result;
    }

    public async Task<List<WORKFLOW_DEPENDENCY>> GetDependenciesAsync(string processDefinitionId)
    {
        var repo = GetRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "DEPENDENT_PROCESS_DEF_ID", FilterValue = processDefinitionId },
            new() { FieldName = "ACTIVE_IND", FilterValue = "Y" },
        };
        var results = await repo.GetAsync(filters);
        return results.OfType<WORKFLOW_DEPENDENCY>().ToList();
    }

    public async Task<List<WORKFLOW_DEPENDENCY>> GetFullGraphAsync()
    {
        var repo = GetRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "ACTIVE_IND", FilterValue = "Y" },
        };
        var results = await repo.GetAsync(filters);
        return results.OfType<WORKFLOW_DEPENDENCY>().ToList();
    }

    private PPDMGenericRepository GetRepo() =>
        new(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(WORKFLOW_DEPENDENCY), _connectionName, "WORKFLOW_DEPENDENCY", null);
}
