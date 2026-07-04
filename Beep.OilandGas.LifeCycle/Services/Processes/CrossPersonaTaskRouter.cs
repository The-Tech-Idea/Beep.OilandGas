using Beep.OilandGas.PPDM39.Core;
using System.Text.Json;
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
/// Routes workflow tasks to the correct persona inboxes based on role-to-persona mappings.
/// When a step is assigned to a role, this service determines which persona(s) should see it
/// and creates CROSS_PERSONA_TASK records for each.
/// Part of Phase 3 cross-role orchestration.
/// </summary>
public interface ICrossPersonaTaskRouter
{
    /// <summary>
    /// When a step becomes active and is assigned to a role, create task records
    /// for all personas that map to that role.
    /// </summary>
    Task<List<CROSS_PERSONA_TASK>> RouteTaskAsync(
        string processInstanceId,
        string stepInstanceId,
        string assignedRole,
        string entityType,
        string entityId,
        string? entityDescription,
        string userId);

    /// <summary>
    /// Get all pending tasks for a given persona.
    /// </summary>
    Task<List<CROSS_PERSONA_TASK>> GetTasksForPersonaAsync(string personaCode);

    /// <summary>
    /// Get task counts by persona (for dashboard badges).
    /// </summary>
    Task<Dictionary<string, int>> GetTaskCountsByPersonaAsync();
}

public class CrossPersonaTaskRouter : ICrossPersonaTaskRouter
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<CrossPersonaTaskRouter> _logger;

    public CrossPersonaTaskRouter(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName = "PPDM39",
        ILogger<CrossPersonaTaskRouter>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    public async Task<List<CROSS_PERSONA_TASK>> RouteTaskAsync(
        string processInstanceId,
        string stepInstanceId,
        string assignedRole,
        string entityType,
        string entityId,
        string? entityDescription,
        string userId)
    {
        var tasks = new List<CROSS_PERSONA_TASK>();

        // Resolve which personas map to this role via PERSONA_ROLE table
        var personaRoles = await GetPersonaRolesForRoleAsync(assignedRole);
        if (personaRoles.Count == 0)
        {
            _logger?.LogWarning("No personas found for role {Role} — task not routed", assignedRole);
            return tasks;
        }

        // Get step info for SLA/due date
        var step = await GetStepInstanceAsync(stepInstanceId);
        var dueDate = step?.SLA_HOURS.HasValue == true && step.STARTED_DATE.HasValue
            ? step.STARTED_DATE.Value.AddHours(step.SLA_HOURS.Value)
            : (DateTime?)null;

        // Determine task type from step info
        var taskType = step?.APPROVAL_REQUIRED == true ? "APPROVAL" : "REVIEW";
        var priority = DeterminePriority(step, dueDate);

        var repo = GetTaskRepo();

        foreach (var pr in personaRoles)
        {
            var task = new CROSS_PERSONA_TASK
            {
                PROCESS_INSTANCE_ID = processInstanceId,
                PROCESS_STEP_INSTANCE_ID = stepInstanceId,
                TARGET_PERSONA_CODE = pr.PERSONA_CODE,
                ASSIGNED_ROLE = assignedRole,
                TASK_TYPE = taskType,
                PRIORITY = priority,
                TASK_STATUS = "PENDING",
                DUE_DATE = dueDate,
                ENTITY_TYPE = entityType,
                ENTITY_ID = entityId,
                ENTITY_DESCRIPTION = entityDescription,
                ROUTE = BuildRoute(taskType, processInstanceId, stepInstanceId),
                TASK_CONTEXT_JSON = JsonSerializer.Serialize(new
                {
                    assignedRole,
                    entityType,
                    entityId,
                    processInstanceId,
                    stepInstanceId,
                }),
            };

            await repo.InsertAsync(task, userId);
            tasks.Add(task);
        }

        _logger?.LogInformation(
            "Routed task to {PersonaCount} personas: Role={Role}, Process={ProcessId}, Step={StepId}",
            tasks.Count, assignedRole, processInstanceId, stepInstanceId);

        return tasks;
    }

    public async Task<List<CROSS_PERSONA_TASK>> GetTasksForPersonaAsync(string personaCode)
    {
        var repo = GetTaskRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "TARGET_PERSONA_CODE", FilterValue = personaCode },
            new() { FieldName = "TASK_STATUS", FilterValue = "PENDING" },
        };
        var results = await repo.GetAsync(filters);
        return results.OfType<CROSS_PERSONA_TASK>()
            .OrderBy(t => t.PRIORITY)
            .ThenBy(t => t.DUE_DATE ?? DateTime.MaxValue)
            .ToList();
    }

    public async Task<Dictionary<string, int>> GetTaskCountsByPersonaAsync()
    {
        var repo = GetTaskRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "TASK_STATUS", FilterValue = "PENDING" },
        };
        var results = await repo.GetAsync(filters);
        return results.OfType<CROSS_PERSONA_TASK>()
            .GroupBy(t => t.TARGET_PERSONA_CODE)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private async Task<List<dynamic>> GetPersonaRolesForRoleAsync(string roleName)
    {
        var repo = new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(Beep.OilandGas.UserManagement.Models.Identity.PERSONA_ROLE),
            _connectionName, "PERSONA_ROLE", null);

        var filters = new List<AppFilter>
        {
            new() { FieldName = "ROLE_NAME", FilterValue = roleName },
            new() { FieldName = "ACTIVE_IND", FilterValue = "Y" },
        };

        var results = await repo.GetAsync(filters);
        return results.OfType<Beep.OilandGas.UserManagement.Models.Identity.PERSONA_ROLE>()
            .Select(pr => (dynamic)new { pr.PERSONA_CODE, pr.IS_PRIMARY })
            .ToList();
    }

    private async Task<PROCESS_STEP_INSTANCE?> GetStepInstanceAsync(string stepInstanceId)
    {
        var repo = new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROCESS_STEP_INSTANCE), _connectionName, "PROCESS_STEP_INSTANCE", null);

        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROCESS_STEP_INSTANCE_ID", FilterValue = stepInstanceId },
        };
        var results = await repo.GetAsync(filters);
        return results.OfType<PROCESS_STEP_INSTANCE>().FirstOrDefault();
    }

    private static int DeterminePriority(PROCESS_STEP_INSTANCE? step, DateTime? dueDate)
    {
        if (dueDate.HasValue && dueDate.Value < DateTime.UtcNow) return 1; // Overdue = Critical
        if (step?.SLA_HOURS is > 0 && step.STARTED_DATE.HasValue)
        {
            var elapsed = DateTime.UtcNow - step.STARTED_DATE.Value;
            var sla = TimeSpan.FromHours(step.SLA_HOURS.Value);
            if (elapsed > sla * 0.75) return 1;  // > 75% of SLA elapsed
            if (elapsed > sla * 0.5) return 2;   // > 50% of SLA elapsed
        }
        return 3; // Normal
    }

    private static string BuildRoute(string taskType, string processInstanceId, string stepInstanceId)
    {
        return taskType switch
        {
            "APPROVAL" => $"/ppdm39/process/approve/{processInstanceId}/{stepInstanceId}",
            _ => $"/ppdm39/process/task/{processInstanceId}/{stepInstanceId}",
        };
    }

    private PPDMGenericRepository GetTaskRepo() =>
        new(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(CROSS_PERSONA_TASK), _connectionName, "CROSS_PERSONA_TASK", null);
}
