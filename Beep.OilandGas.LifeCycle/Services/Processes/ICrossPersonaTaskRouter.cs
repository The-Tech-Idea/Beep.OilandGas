using Beep.OilandGas.LifeCycle.Data.Tables;
namespace Beep.OilandGas.LifeCycle.Services.Processes;

public interface ICrossPersonaTaskRouter
{
    /// <summary>
    /// When a step becomes active and is assigned to a role, create task records
    /// for the active personas of active users who hold that role.
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

