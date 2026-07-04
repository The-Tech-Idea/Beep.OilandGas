using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// Tracks workflow version history for process definitions.
/// Enables in-flight instance migration when process definitions are updated.
/// Part of Phase 2 workflow engine enhancement.
/// </summary>
public class WORKFLOW_VERSION : ModelEntityBase
{
    [Key]
    public string VERSION_ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>FK → PROCESS_DEFINITION.PROCESS_DEFINITION_ID.</summary>
    public string PROCESS_DEFINITION_ID { get; set; } = string.Empty;

    /// <summary>Semantic version: "1.0", "1.1", "2.0".</summary>
    public string VERSION_NUMBER { get; set; } = "1.0";

    /// <summary>Human-readable description of changes in this version.</summary>
    public string CHANGE_DESCRIPTION { get; set; } = string.Empty;

    /// <summary>FK → WORKFLOW_VERSION.VERSION_ID of the previous version.</summary>
    public string? PREVIOUS_VERSION_ID { get; set; }

    /// <summary>Full JSON snapshot of the process definition at this version.</summary>
    public string? PROCESS_CONFIG_SNAPSHOT { get; set; }

    /// <summary>When this version became effective.</summary>
    public DateTime EFFECTIVE_DATE { get; set; } = DateTime.UtcNow;

    /// <summary>JSON array of step IDs removed in this version.</summary>
    public string? DEPRECATED_STEP_IDS { get; set; }

    /// <summary>JSON object mapping old_step_id → new_step_id for renamed/merged steps.</summary>
    public string? STEP_REMAPPING_JSON { get; set; }

    /// <summary>Who created this version.</summary>
    public string? CREATED_BY { get; set; }
}
