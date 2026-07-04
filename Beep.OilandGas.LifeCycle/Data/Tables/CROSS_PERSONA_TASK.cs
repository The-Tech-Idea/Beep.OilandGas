using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// Represents a task routed to a specific persona's inbox from a cross-role workflow.
/// Created when a workflow step is assigned to a role that maps to one or more personas.
/// Part of Phase 3 cross-role orchestration.
/// </summary>
public class CROSS_PERSONA_TASK : ModelEntityBase
{
    [Key]
    public string CROSS_TASK_ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>FK → PROCESS_INSTANCE.PROCESS_INSTANCE_ID.</summary>
    public string PROCESS_INSTANCE_ID { get; set; } = string.Empty;

    /// <summary>FK → PROCESS_STEP_INSTANCE.PROCESS_STEP_INSTANCE_ID.</summary>
    public string PROCESS_STEP_INSTANCE_ID { get; set; } = string.Empty;

    /// <summary>The persona this task is routed to.</summary>
    public string TARGET_PERSONA_CODE { get; set; } = string.Empty;

    /// <summary>The role that needs to act.</summary>
    public string ASSIGNED_ROLE { get; set; } = string.Empty;

    /// <summary>Task type: APPROVAL, REVIEW, DATA_ENTRY, NOTIFICATION.</summary>
    public string TASK_TYPE { get; set; } = "REVIEW";

    /// <summary>1=Critical, 2=High, 3=Normal, 4=Low.</summary>
    public int PRIORITY { get; set; } = 3;

    /// <summary>Task status: PENDING, IN_PROGRESS, COMPLETED, DISMISSED.</summary>
    public string TASK_STATUS { get; set; } = "PENDING";

    /// <summary>When the task is due (from SLA).</summary>
    public DateTime? DUE_DATE { get; set; }

    /// <summary>Entity context: what this task is about.</summary>
    public string? ENTITY_TYPE { get; set; }
    public string? ENTITY_ID { get; set; }
    public string? ENTITY_DESCRIPTION { get; set; }

    /// <summary>Deep-link route to the task detail page.</summary>
    public string? ROUTE { get; set; }

    /// <summary>Additional context as JSON.</summary>
    public string? TASK_CONTEXT_JSON { get; set; }
}
