using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// Defines dependencies between workflow steps.
/// Workflow B Step Y cannot start until Workflow A Step X completes.
/// Supports BLOCKING (hard gate), ADVISORY (warning only), and CONDITIONAL (expression-based).
/// Part of Phase 2 workflow engine enhancement.
/// </summary>
public class WORKFLOW_DEPENDENCY : ModelEntityBase
{
    [Key]
    public string DEPENDENCY_ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>The process definition that DEPENDS ON something else.</summary>
    public string DEPENDENT_PROCESS_DEF_ID { get; set; } = string.Empty;

    /// <summary>Specific step within the dependent process (null = entire process).</summary>
    public string? DEPENDENT_STEP_ID { get; set; }

    /// <summary>The process definition that must complete FIRST.</summary>
    public string PREREQUISITE_PROCESS_DEF_ID { get; set; } = string.Empty;

    /// <summary>Specific step within the prerequisite process (null = entire process).</summary>
    public string? PREREQUISITE_STEP_ID { get; set; }

    /// <summary>Dependency type: BLOCKING (hard gate), ADVISORY (warning), CONDITIONAL (expression).</summary>
    public string DEPENDENCY_TYPE { get; set; } = "BLOCKING";

    /// <summary>For CONDITIONAL type: expression to evaluate, e.g. "ESTIMATED_COST > 500000".</summary>
    public string? CONDITION_EXPRESSION { get; set; }

    /// <summary>Optional description of why this dependency exists.</summary>
    public string? DESCRIPTION { get; set; }
}
