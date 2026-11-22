using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// One recorded configuration change (WFL-06, AC-2): what was changed, by whom, why, and what it replaced.
/// </summary>
/// <remarks>
/// The owner's configuration tables carry no change history - a version has a change description for its own creation, but a rule
/// that is switched on or a dependency that is added leaves no trace of its previous value. The configuration affects every
/// approval in the product, so the plan's own contract (actor, reason and previous value) is implemented here rather than
/// appended to the audit log of a session.
/// </remarks>
public partial class WORKFLOW_CONFIGURATION_CHANGE : ModelEntityBase
{
    [Key, MaxLength(128)]
    public string WORKFLOW_CONFIGURATION_CHANGE_ID { get; set; } = string.Empty;

    /// <summary>The area changed: VERSION, DEPENDENCY, TRIGGER, SOD_RULE, DELEGATION.</summary>
    [MaxLength(32)]
    public string AREA { get; set; } = string.Empty;

    /// <summary>The row changed, so the history can be read per record.</summary>
    [MaxLength(128)]
    public string TARGET_ID { get; set; } = string.Empty;

    /// <summary>A second identifier when the target needs two (a dependency's other definition, a delegation's delegate).</summary>
    [MaxLength(256)]
    public string? TARGET_DETAIL { get; set; }

    [MaxLength(128)]
    public string ACTOR { get; set; } = string.Empty;

    /// <summary>Why the change was made. A change without one is refused.</summary>
    [MaxLength(1000)]
    public string REASON { get; set; } = string.Empty;

    /// <summary>The value that was replaced, as JSON, so the change can be understood and undone.</summary>
    [MaxLength(4000)]
    public string? PREVIOUS_VALUE { get; set; }

    /// <summary>The value written, as JSON.</summary>
    [MaxLength(4000)]
    public string? NEW_VALUE { get; set; }

    public DateTime CHANGED_AT_UTC { get; set; }
}
