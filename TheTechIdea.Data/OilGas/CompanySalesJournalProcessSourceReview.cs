namespace TheTechIdea.Data.OilGas;

/// <summary>The exact native journal correlated with its separately owned company workflow.</summary>
public sealed record CompanySalesJournalProcessSourceReview(CompanyScope Company,string ActorUserId,string InstanceId,string JournalId,string SourceRevision,string ProcessRevision,CompanySalesJournalWorkflowSource Journal)
{
    public void Require(CompanyScope company,string actor,string instance)
    {
        PurchaseOrderOwnership.RequireScope(company);BillingPolicyValues.Text(actor,128);BillingPolicyValues.Text(instance,128);BillingPolicyValues.Intent(JournalId);BillingPolicyValues.Revision(SourceRevision);BillingPolicyValues.Revision(ProcessRevision);
        if(Journal is null || Company!=company || ActorUserId!=actor || InstanceId!=instance)throw new InvalidOperationException("The exact owned journal process source is required.");
        Journal.Require(company,actor,JournalId);
        if(SourceRevision!=Journal.SourceRevision)throw new InvalidOperationException("The journal process differs from its original immutable preparation.");
    }
}
