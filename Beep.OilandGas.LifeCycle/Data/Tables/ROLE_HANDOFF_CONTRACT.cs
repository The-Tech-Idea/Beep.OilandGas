using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// Defines a formal contract for handing off work between roles in a cross-role workflow.
/// Specifies required data fields, documents, SLA context, and validation rules
/// that must be satisfied before a handoff can proceed.
/// Part of Phase 3 cross-role orchestration.
/// </summary>
public class ROLE_HANDOFF_CONTRACT : ModelEntityBase
{
    [Key]
    public string HANDOFF_CONTRACT_ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>FK → PROCESS_DEFINITION.PROCESS_DEFINITION_ID.</summary>
    public string PROCESS_DEFINITION_ID { get; set; } = string.Empty;

    /// <summary>The step after which the handoff occurs.</summary>
    public string FROM_STEP_ID { get; set; } = string.Empty;

    /// <summary>The role handing off the work.</summary>
    public string FROM_ROLE { get; set; } = string.Empty;

    /// <summary>The step receiving the handoff.</summary>
    public string TO_STEP_ID { get; set; } = string.Empty;

    /// <summary>The role receiving the work.</summary>
    public string TO_ROLE { get; set; } = string.Empty;

    /// <summary>JSON array of required field names that must be populated on the entity.</summary>
    public string? REQUIRED_DATA_FIELDS_JSON { get; set; }

    /// <summary>JSON array of required document types, e.g. ["Completion Report", "Well Test Data"].</summary>
    public string? REQUIRED_DOCUMENTS_JSON { get; set; }

    /// <summary>JSON object with SLA context: maxResponseHours, priority level.</summary>
    public string? SLA_CONTEXT_JSON { get; set; }

    /// <summary>JSON object with approval context passed to the receiving role.</summary>
    public string? APPROVAL_CONTEXT_JSON { get; set; }

    /// <summary>JSON array of validation rule expressions for handoff validity.</summary>
    public string? VALIDATION_RULES_JSON { get; set; }

    /// <summary>Optional description of what this handoff accomplishes.</summary>
    public string? DESCRIPTION { get; set; }
}
