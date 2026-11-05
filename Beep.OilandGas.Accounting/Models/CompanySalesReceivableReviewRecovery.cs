using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.Accounting.Models;

/// <summary>Current reviewer's exact retained command and proven native disposition, independent of advanced physical source.</summary>
public sealed record CompanySalesReceivableReviewRecovery(CompanyScope Company,string ActorUserId,string ReceivableId,SALES_RECEIVABLE_REVIEW_INTENT? Retained,CompanySalesReceivableOutcome? Outcome)
{
    public void Require(CompanyScope company,string actor,string receivable)
    {
        PurchaseOrderOwnership.RequireScope(company);BillingPolicyValues.Text(actor,128);BillingPolicyValues.Intent(receivable);
        if(Company!=company || ActorUserId!=actor || ReceivableId!=receivable || Retained is null && Outcome is not null)throw new InvalidOperationException("Original reviewer recovery context is unconfirmed.");
        if(Retained is null)return;
        var command=Retained.Restore(company,actor);
        if(command.ReceivableId!=receivable)throw new InvalidOperationException("The retained reviewer command belongs to another receivable.");
        if(Outcome is null)return;
        Outcome.Require(command.Review.Original.Intent.Restore(company,command.Review.Original.Intent.ROW_CREATED_BY));
        var proof=CompanySalesReceivableDecisionProof.Require(company,Outcome.Receivable);
        if(JsonSerializer.Serialize(proof.Intent)!=JsonSerializer.Serialize(Retained) || JsonSerializer.Serialize(proof.Intent.Restore(company,actor))!=JsonSerializer.Serialize(command))throw new InvalidOperationException("The native disposition differs from its exact original reviewer command.");
    }
}
