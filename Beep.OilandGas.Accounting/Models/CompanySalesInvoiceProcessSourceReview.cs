using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Accounting.Models;
public sealed record CompanySalesInvoiceProcessSourceReview(CompanyScope Company,string ActorUserId,string InstanceId,string InvoiceId,string SourceRevision,string ProcessRevision,CompanySalesInvoiceReviewSource Invoice)
{
    public void Require(CompanyScope company,string actor,string instance)
    {
        PurchaseOrderOwnership.RequireScope(company);BillingPolicyValues.Text(actor,128);BillingPolicyValues.Text(instance,128);BillingPolicyValues.Intent(InvoiceId);BillingPolicyValues.Revision(SourceRevision);BillingPolicyValues.Revision(ProcessRevision);
        if(Invoice is null || Company!=company || ActorUserId!=actor || InstanceId!=instance)throw new InvalidOperationException("The complete owned invoice process source is required.");
        Invoice.Require(company,actor,InvoiceId);
        if(SourceRevision!=CompanyAccountingSourceTypes.InvoicePreparationRevision(Invoice.Original))throw new InvalidOperationException("The invoice process no longer matches its original preparation receipt.");
    }
}
