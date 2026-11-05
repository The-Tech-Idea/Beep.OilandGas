using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Text.Json;
namespace Beep.OilandGas.Accounting.Services;
public partial class ARService
{
    public async Task<CompanySalesPaymentWorkflowSource> GetCompanySalesPaymentWorkflowSourceAsync(CompanyScope company,string actor,string bindingRevision,string id,JournalEntryService journal,Func<Task> authority,Func<string,Task> fieldRead)
        =>await ReadCompanySalesPaymentWorkflowSourceAsync(company,actor,bindingRevision,id,journal,authority,fieldRead,true);
    /// <summary>One fresh observation inside the enclosing process/source initial and closing comparison; no native reads or authority are cached.</summary>
    public Task<CompanySalesPaymentWorkflowSource> CaptureCompanySalesPaymentWorkflowSourceAsync(CompanyScope company,string actor,string bindingRevision,string id,JournalEntryService journal,Func<Task> authority,Func<string,Task> fieldRead)
        =>ReadCompanySalesPaymentWorkflowSourceAsync(company,actor,bindingRevision,id,journal,authority,fieldRead,false);
    private async Task<CompanySalesPaymentWorkflowSource> ReadCompanySalesPaymentWorkflowSourceAsync(CompanyScope company,string actor,string bindingRevision,string id,JournalEntryService journal,Func<Task> authority,Func<string,Task> fieldRead,bool compareCaptures)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();BillingPaymentId(id);BillingPaymentRevision(bindingRevision);PurchaseOrderOwnership.RequireScope(company);await authority();var binding=await ReceivableBinding();
        async Task Guard(){await authority();await RequireJournalSourceBindingAsync(binding.Connection,binding.Source);await journal.RequirePaymentSourceBindingAsync(binding.Connection,binding.Source);PaymentReviewSchema(binding.Source);}
        async Task<CompanySalesPaymentWorkflowSource> Capture(){var row=await ReadPaymentForReview(company,id,binding.Connection,binding.Source,Guard,fieldRead);var maker=CompanySalesPaymentProof.Require(row).Command;var source=await ReadCompanySalesPaymentSourceAsync(company,actor,bindingRevision,row.BOOK_ID,maker.Source.Ledger.CurrencyCode,row.AR_INVOICE_ID,journal,Guard,fieldRead,false);var value=CompanySalesPaymentWorkflowSource.Create(source,id);if(value.Payment.NativeRevision!=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(row)))throw new InvalidOperationException("The complete current payment and public workflow observation differ.");await Guard();return value;}
        var first=await Capture();await Guard();if(!compareCaptures)return first;var closing=await Capture();await Guard();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete original native payment workflow source changed.");return closing;
    }
}
