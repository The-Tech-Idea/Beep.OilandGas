namespace Beep.OilandGas.ProspectIdentification;

/// <summary>
/// Configuration for lead → prospect workflow persistence (see <c>LeadExplorationService</c> in LifeCycle).
/// Bind from <c>Exploration:LeadWorkflow</c> (see <see cref="SectionName"/>).
/// </summary>
public sealed class LeadExplorationWorkflowOptions
{
    public const string SectionName = "Exploration:LeadWorkflow";

    /// <summary>
    /// Value written to <c>LEAD.LEAD_STATUS</c> after a new <c>PROSPECT</c> is created; must exist in <c>R_LEAD_STATUS</c> when FK is enforced.
    /// </summary>
    public string PromotedLeadStatusCode { get; set; } = ExplorationReferenceCodes.LeadStatusPromotedToProspect;
}
