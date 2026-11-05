using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Accounting.Services;
public partial class InvoiceService
{
    /// <summary>Caller must enforce distinct current reviewer authority; physical FIELD access precedes restoration of the private proposal.</summary>
    public async Task<CompanySalesInvoiceDraftOutcome> GetCompanyInvoiceDraftForReviewAsync(CompanyScope company,string invoiceId,Func<Task> requireAuthority,Func<string,Task> requireFieldAccess)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();
        PurchaseOrderOwnership.RequireScope(company);RequireInvoiceSourceId(invoiceId);ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(requireFieldAccess);
        await requireAuthority();var binding=await InvoiceIntentBinding();string? field=null;
        async Task Guard()
        {await requireAuthority();var current=await InvoiceIntentBinding();if(current.Connection!=binding.Connection || !ReferenceEquals(current.Source,binding.Source))throw new InvalidOperationException("The original reviewer production binding changed.");if(field is not null)await requireFieldAccess(field);}
        async Task<CompanySalesInvoiceDraftOutcome> Capture()
        {
            await Guard();RequireCompanyInvoiceSchema(binding.Source);var repo=await GetRepoAsync<INVOICE>("INVOICE",binding.Connection);
            var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="INVOICE_ID",Operator="=",FilterValue=invoiceId}]))?.ToArray();
            await Guard();if(raw is not {Length:1} || raw[0] is not INVOICE row || row.INVOICE_ID!=invoiceId)throw new InvalidOperationException("One exact owned native invoice is required for review.");
            RequireInvoiceSourceId(row.FIELD_ID!);if(field is not null && field!=row.FIELD_ID)throw new InvalidOperationException("The original review FIELD changed.");field=row.FIELD_ID;await Guard();
            var command=CompanySalesInvoiceDraftProof.Require(company,row).Record.Command;
            var outcome=await GetCompanyInvoiceDraftOutcomeAsync(company,command.Review.ActorUserId,command.IntentId,Guard);await Guard();
            if(outcome is null || JsonSerializer.Serialize(outcome.Invoice)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("The original invoice command or complete native outcome receipt is unconfirmed.");return outcome;
        }
        var first=await Capture();var closing=await Capture();await Guard();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete original native invoice review evidence changed.");return CompanyInventorySourceReview.Copy(closing);
    }
}
