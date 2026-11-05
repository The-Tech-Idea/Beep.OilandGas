using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.Accounting.Models;
/// <summary>Actual native journal/line state and stable original preparation identity for existing accounting workflow correlation.</summary>
public sealed record CompanySalesJournalWorkflowSource(CompanyScope Company,string ActorUserId,string BindingRevision,string JournalId,string SourceRevision,CompanySalesJournalPreparationOutcome Journal,string Revision)
{
    public static CompanySalesJournalWorkflowSource Create(CompanyScope company,string actor,string binding,CompanySalesJournalPreparationOutcome journal)
    {
        var value=new CompanySalesJournalWorkflowSource(company,actor,binding,journal.Header.JOURNAL_ENTRY_ID,CompanyAccountingSourceTypes.JournalPreparationRevision(journal),journal,string.Empty);value=value with{Revision=Digest(value)};value.Require(company,actor,value.JournalId);return CompanyInventorySourceReview.Copy(value);
    }
    public void Require(CompanyScope company,string actor,string id)
    {
        PurchaseOrderOwnership.RequireScope(company);BillingPolicyValues.Text(actor,128);BillingPolicyValues.Intent(id);BillingPolicyValues.Revision(BindingRevision);BillingPolicyValues.Revision(SourceRevision);ArgumentNullException.ThrowIfNull(Journal);var preparation=CompanySalesJournalPreparationProof.Require(Journal.Header).Command;Journal.Require(preparation);
        if(Company!=company || ActorUserId!=actor || JournalId!=id || Journal.Header.JOURNAL_ENTRY_ID!=id || preparation.Source.Company!=company || preparation.Source.BindingRevision!=BindingRevision || Journal.Header.STATUS is not ("DRAFT" or "APPROVED" or "REJECTED" or "POSTED") || SourceRevision!=CompanyAccountingSourceTypes.JournalPreparationRevision(Journal) || Revision!=Digest(this))throw new InvalidOperationException("The exact native journal workflow source/preparation/context changed.");
    }
    private static string Digest(CompanySalesJournalWorkflowSource value)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value with{Revision=string.Empty}));
}
