using System.Net.Http.Json;

namespace Beep.OilandGas.Web.Services;

/// <summary>
/// Provides a unified view of all pending tasks across workflows, approvals,
/// access reviews, and cross-persona tasks for the current user.
/// Aggregates from multiple backend sources into a single inbox.
/// Part of Phase 5 experience & integration.
/// </summary>
public interface IUnifiedTaskInboxService
{
    Task<UnifiedInbox> GetInboxAsync(string personaCode);
    Task<InboxCounts> GetInboxCountsAsync(string personaCode);
    Task<List<UnifiedTask>> GetFilteredTasksAsync(InboxFilter filter, string personaCode);
}

public class UnifiedTask
{
    public string TaskId { get; set; } = string.Empty;
    public string TaskType { get; set; } = "REVIEW";       // APPROVAL, REVIEW, DATA_ENTRY, NOTIFICATION
    public string WorkflowName { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string EntityDescription { get; set; } = string.Empty;
    public string FromPersona { get; set; } = string.Empty;
    public string FromUserName { get; set; } = string.Empty;
    public int Priority { get; set; } = 3;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string SlaStatus { get; set; } = "ON_TRACK";    // ON_TRACK, AT_RISK, BREACHED
    public string Status { get; set; } = "PENDING";
    public string Route { get; set; } = string.Empty;
}

public class UnifiedInbox
{
    public List<UnifiedTask> CriticalTasks { get; set; } = new();
    public List<UnifiedTask> HighPriorityTasks { get; set; } = new();
    public List<UnifiedTask> NormalTasks { get; set; } = new();
    public InboxCounts Counts { get; set; } = new();
}

public class InboxCounts
{
    public int TotalPending { get; set; }
    public int Critical { get; set; }
    public int Overdue { get; set; }
    public int Approvals { get; set; }
    public int Reviews { get; set; }
    public int DataEntry { get; set; }
}

public class InboxFilter
{
    public string? TaskType { get; set; }
    public int? MinPriority { get; set; }
    public DateTime? DueBefore { get; set; }
    public string? WorkflowName { get; set; }
    public string? SortBy { get; set; } = "priority";
    public int PageSize { get; set; } = 20;
    public int PageNumber { get; set; } = 0;
}

public class UnifiedTaskInboxService : IUnifiedTaskInboxService
{
    private readonly HttpClient _http;

    public UnifiedTaskInboxService(HttpClient http)
    {
        _http = http;
    }

    public async Task<UnifiedInbox> GetInboxAsync(string personaCode)
    {
        try
        {
            var response = await _http.GetAsync($"/api/workflow/tasks/inbox?personaCode={Uri.EscapeDataString(personaCode)}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UnifiedInbox>()
                    ?? new UnifiedInbox();
            }
        }
        catch { /* API unavailable — return empty */ }

        return new UnifiedInbox();
    }

    public async Task<InboxCounts> GetInboxCountsAsync(string personaCode)
    {
        try
        {
            var response = await _http.GetAsync($"/api/workflow/tasks/counts?personaCode={Uri.EscapeDataString(personaCode)}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<InboxCounts>()
                    ?? new InboxCounts();
            }
        }
        catch { }

        return new InboxCounts();
    }

    public async Task<List<UnifiedTask>> GetFilteredTasksAsync(InboxFilter filter, string personaCode)
    {
        try
        {
            var query = $"/api/workflow/tasks/inbox?personaCode={Uri.EscapeDataString(personaCode)}"
                + $"&taskType={filter.TaskType}&minPriority={filter.MinPriority}"
                + $"&sortBy={filter.SortBy}&pageSize={filter.PageSize}&page={filter.PageNumber}";

            if (filter.DueBefore.HasValue)
                query += $"&dueBefore={filter.DueBefore.Value:O}";
            if (!string.IsNullOrWhiteSpace(filter.WorkflowName))
                query += $"&workflow={Uri.EscapeDataString(filter.WorkflowName)}";

            var response = await _http.GetAsync(query);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<UnifiedTask>>()
                    ?? new List<UnifiedTask>();
            }
        }
        catch { }

        return new List<UnifiedTask>();
    }
}
