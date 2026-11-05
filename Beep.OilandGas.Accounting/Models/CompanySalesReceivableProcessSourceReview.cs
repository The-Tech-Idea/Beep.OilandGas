using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Accounting.Models;
public sealed record CompanySalesReceivableProcessSourceReview(CompanyScope Company,string ActorUserId,string InstanceId,string ReceivableId,string SourceRevision,string ProcessRevision,CompanySalesReceivableReviewSource Receivable)
{
    public void Require(CompanyScope company,string actor,string instance)
    {
        PurchaseOrderOwnership.RequireScope(company);BillingPolicyValues.Text(actor,128);BillingPolicyValues.Text(instance,128);BillingPolicyValues.Intent(ReceivableId);BillingPolicyValues.Revision(SourceRevision);BillingPolicyValues.Revision(ProcessRevision);
        if(Receivable is null || Company!=company || ActorUserId!=actor || InstanceId!=instance)throw new InvalidOperationException("The complete owned receivable process source is required.");
        Receivable.Require(company,actor,ReceivableId);
        if(SourceRevision!=CompanyAccountingSourceTypes.ReceivablePreparationRevision(Receivable.Original))throw new InvalidOperationException("The receivable process no longer matches its original immutable draft receipt.");
    }
}
