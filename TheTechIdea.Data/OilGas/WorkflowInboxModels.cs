namespace TheTechIdea.Data.OilGas;

public sealed class UnifiedTask
{
    public string TaskId { get; set; } = "";
    public string TaskType { get; set; } = "REVIEW";
    public string WorkflowName { get; set; } = "";
    public string StepName { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string EntityDescription { get; set; } = "";
    public string FromPersona { get; set; } = "";
    public string FromUserName { get; set; } = "";
    public int Priority { get; set; } = 3;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public string SlaStatus { get; set; } = "ON_TRACK";
    public string Status { get; set; } = "PENDING";
    public string Route { get; set; } = "";
}

public sealed class UnifiedInbox
{
    public List<UnifiedTask> CriticalTasks { get; set; } = new();
    public List<UnifiedTask> HighPriorityTasks { get; set; } = new();
    public List<UnifiedTask> NormalTasks { get; set; } = new();
    public InboxCounts Counts { get; set; } = new();
}

public sealed class InboxCounts
{
    public int TotalPending { get; set; }
    public int Critical { get; set; }
    public int Overdue { get; set; }
    public int Approvals { get; set; }
    public int Reviews { get; set; }
    public int DataEntry { get; set; }
}

public sealed class InboxFilter
{
    public string? TaskType { get; set; }
    public int? MinPriority { get; set; }
    public DateTime? DueBefore { get; set; }
    public string? WorkflowName { get; set; }
    public string? SortBy { get; set; } = "priority";
    public int PageSize { get; set; } = 20;
    public int PageNumber { get; set; }
}
