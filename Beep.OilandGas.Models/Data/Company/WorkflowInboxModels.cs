using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
namespace Beep.OilandGas.Models.Data.Company;

public sealed class UnifiedTask
{
    public string TaskId { get; set; } = "";
    public string TaskType { get; set; } = "REVIEW";
    public string WorkflowName { get; set; } = "";
    public string StepName { get; set; } = "";
    public string? StepDisplayName { get; set; }
    public CompanyAccountingStepReview? AccountingStepReview { get; set; }
    public string? ProgramReviewRevision { get; set; }
    public string? CompletionReviewRevision { get; set; }
    public string? ExecutionAssignmentRevision { get; set; }
    public string? InvestmentReviewRevision { get; set; }
    public string? DevelopmentReviewRevision { get; set; }
    public string? CreatedByUserId { get; set; }
    public string ScopeKind { get; set; } = "";
    public string AssignedRole { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string EntityDescription { get; set; } = "";
    public string FromPersona { get; set; } = "";
    public string FromUserName { get; set; } = "";
    public int Priority { get; set; } = 3;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public string SlaStatus { get; set; } = "UNSCHEDULED";
    public string Status { get; set; } = "PENDING";
    public string Route { get; set; } = "";
}

public sealed class UnifiedInbox
{
    public CompanyScope Company { get; set; } = null!;
    public string ActorUserId { get; set; } = "";
    public string PersonaCode { get; set; } = "";
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

/// <summary>Classifies only recorded UTC deadlines; local or unspecified dates have no confirmed time zone.</summary>
public static class WorkflowTaskSchedule
{
    public static string Classify(string? entityType, DateTime? due, DateTime nowUtc)
    {
        if (nowUtc.Kind != DateTimeKind.Utc) throw new ArgumentException("The comparison instant must be UTC.");
        if (entityType == "DRILLING_OPERATION" || due is null) return "UNSCHEDULED";
        if (due.Value.Kind != DateTimeKind.Utc) return "UNCONFIRMED";
        return due.Value < nowUtc ? "BREACHED" : "ON_TRACK";
    }
}
