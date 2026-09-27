using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Controllers;

[ApiController]
[Authorize]
[Route("api/workflow/tasks")]
public sealed class WorkflowTasksController(RepositoryDbContext repository, ICrossPersonaTaskRouter tasks) : ControllerBase
{
    [HttpGet("inbox")]
    public async Task<IActionResult> Inbox(string? personaCode, CancellationToken token)
    {
        var visible = await VisibleTasksAsync(personaCode, token);
        if (visible is null) return Forbid();
        return Ok(new UnifiedInbox { CriticalTasks = visible.Where(x => x.Priority <= 1).ToList(),
            HighPriorityTasks = visible.Where(x => x.Priority == 2).ToList(),
            NormalTasks = visible.Where(x => x.Priority > 2).ToList(), Counts = Count(visible) });
    }

    [HttpGet("counts")]
    public async Task<IActionResult> Counts(string? personaCode, CancellationToken token)
    {
        var visible = await VisibleTasksAsync(personaCode, token);
        return visible is null ? Forbid() : Ok(Count(visible));
    }

    [HttpGet]
    public async Task<IActionResult> List(string? personaCode, string? taskType, int? minPriority,
        DateTime? dueBefore, string? workflow, string? sortBy, int page = 0, int pageSize = 20, CancellationToken token = default)
    {
        if (page < 0 || page > 100000 || pageSize is < 1 or > 100 || minPriority is < 1 or > 4 ||
            (sortBy is not null && sortBy != "priority" && sortBy != "dueDate" && sortBy != "createdDate")) return BadRequest();
        var visible = await VisibleTasksAsync(personaCode, token);
        if (visible is null) return Forbid();
        IEnumerable<UnifiedTask> filtered = visible;
        if (!string.IsNullOrWhiteSpace(taskType)) filtered = filtered.Where(x => x.TaskType == taskType);
        if (minPriority.HasValue) filtered = filtered.Where(x => x.Priority <= minPriority.Value);
        if (dueBefore.HasValue) filtered = filtered.Where(x => x.DueDate.HasValue && x.DueDate <= dueBefore);
        if (!string.IsNullOrWhiteSpace(workflow)) filtered = filtered.Where(x => x.WorkflowName == workflow);
        filtered = sortBy switch { "dueDate" => filtered.OrderBy(x => x.DueDate ?? DateTime.MaxValue),
            "createdDate" => filtered.OrderByDescending(x => x.CreatedDate), _ => filtered.OrderBy(x => x.Priority) };
        return Ok(filtered.Skip(page * pageSize).Take(pageSize).ToList());
    }

    private async Task<List<UnifiedTask>?> VisibleTasksAsync(string? requestedPersona, CancellationToken token)
    {
        var userId = User.ActingUserId();
        var persona = await (from user in repository.Users.AsNoTracking()
                             join profile in repository.Set<AppUserPersona>().AsNoTracking() on user.Id equals profile.UserId
                             join catalog in repository.Set<AppPersona>().AsNoTracking() on profile.PersonaCode equals catalog.Code
                             where user.Id == userId && user.IsActive && catalog.IsActive
                             select catalog.Code).SingleOrDefaultAsync(token);
        if (persona is null || (!string.IsNullOrWhiteSpace(requestedPersona) && requestedPersona != persona)) return null;
        var roles = (await (from membership in repository.UserRoles.AsNoTracking()
                           join role in repository.Roles.AsNoTracking() on membership.RoleId equals role.Id
                           where membership.UserId == userId select role.Name).ToListAsync(token)).OfType<string>().ToHashSet(StringComparer.Ordinal);
        if (roles.Count == 0) return new();
        var now = DateTime.UtcNow;
        return (await tasks.GetTasksForPersonaAsync(persona))
            .Where(x => x.TARGET_PERSONA_CODE == persona && x.TASK_STATUS == "PENDING" && roles.Contains(x.ASSIGNED_ROLE))
            .Select(x => new UnifiedTask { TaskId = x.CROSS_TASK_ID, TaskType = x.TASK_TYPE,
                WorkflowName = x.PROCESS_INSTANCE_ID, StepName = x.PROCESS_STEP_INSTANCE_ID,
                EntityType = x.ENTITY_TYPE ?? "", EntityId = x.ENTITY_ID ?? "", EntityDescription = x.ENTITY_DESCRIPTION ?? "",
                Priority = x.PRIORITY, DueDate = x.DUE_DATE, CreatedDate = x.ROW_CREATED_DATE ?? DateTime.MinValue,
                SlaStatus = x.DUE_DATE < now ? "BREACHED" : "ON_TRACK", Status = x.TASK_STATUS, Route = x.ROUTE ?? "" }).ToList();
    }

    private static InboxCounts Count(List<UnifiedTask> tasks) => new() { TotalPending = tasks.Count,
        Critical = tasks.Count(x => x.Priority <= 1), Overdue = tasks.Count(x => x.SlaStatus == "BREACHED"),
        Approvals = tasks.Count(x => x.TaskType == "APPROVAL"), Reviews = tasks.Count(x => x.TaskType == "REVIEW"),
        DataEntry = tasks.Count(x => x.TaskType == "DATA_ENTRY") };
}
