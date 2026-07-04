using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// Defines an automatic workflow trigger based on business events.
/// When entity X is created/updated with status Y, automatically start workflow Z.
/// Example: PDEN_VOL_SUMMARY posted → start Production→Revenue recognition workflow.
/// Part of Phase 3 cross-role orchestration.
/// </summary>
public class BUSINESS_EVENT_TRIGGER : ModelEntityBase
{
    [Key]
    public string TRIGGER_ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Human-readable trigger name.</summary>
    public string TRIGGER_NAME { get; set; } = string.Empty;

    /// <summary>Event type: ENTITY_CREATED, ENTITY_UPDATED, STATUS_CHANGED, FIELD_CHANGED.</summary>
    public string EVENT_TYPE { get; set; } = "STATUS_CHANGED";

    /// <summary>The entity type to watch, e.g. "PDEN_VOL_SUMMARY", "AFE", "HSE_INCIDENT".</summary>
    public string ENTITY_TYPE { get; set; } = string.Empty;

    /// <summary>Optional: only fire when this field changes.</summary>
    public string? WATCH_FIELD { get; set; }

    /// <summary>Condition expression: "NewStatus == 'POSTED'" or "AMOUNT > 100000".</summary>
    public string? CONDITION_EXPRESSION { get; set; }

    /// <summary>FK → PROCESS_DEFINITION.PROCESS_DEFINITION_ID — workflow to start.</summary>
    public string TARGET_PROCESS_DEF_ID { get; set; } = string.Empty;

    /// <summary>Whether this trigger is active.</summary>
    public string IS_ACTIVE { get; set; } = "Y";

    /// <summary>Execution priority (lower = fires first).</summary>
    public int PRIORITY { get; set; } = 10;

    /// <summary>Optional description.</summary>
    public string? DESCRIPTION { get; set; }
}
