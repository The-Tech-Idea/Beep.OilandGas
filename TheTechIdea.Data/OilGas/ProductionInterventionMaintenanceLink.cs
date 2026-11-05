namespace TheTechIdea.Data.OilGas;

/// <summary>Authorized receiving-role source facts, not task assignment or acceptance.</summary>
public sealed record ProductionInterventionMaintenanceLink(CompanyScope Company,string Actor,string FieldId,string WorkOrderId,
    string WellId,Guid DecisionId,string Reviewer,string Reason,DateTime ReviewedAtUtc,string ReceiptRevision,string WorkOrderRevision,DateTimeOffset RetrievedAtUtc)
{
    public void Require(CompanyScope company,string actor,string field,string order,string well,Guid decisionId)
    {
        if(Company!=company || Actor!=actor || FieldId!=field || WorkOrderId!=order || WellId!=well || DecisionId!=decisionId || decisionId==Guid.Empty ||
            !LeaseSummaryValidator.Identifier(Reviewer) || string.IsNullOrWhiteSpace(Reason) || ReviewedAtUtc==default || ReviewedAtUtc.Kind!=DateTimeKind.Utc ||
            !ProductionInterventionDecisionChecks.IsRevision(ReceiptRevision) || !ProductionInterventionDecisionChecks.IsRevision(WorkOrderRevision) || RetrievedAtUtc==default || RetrievedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The recorded maintenance link must match its current company/account/FIELD/order/well and original decision.");
    }
}
