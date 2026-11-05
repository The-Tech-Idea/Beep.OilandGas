using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Accounting.Services;

public partial class InvoiceService
{
    /// <summary>Bounded complete selected-FIELD queue; original journals and receipts are checked by the native invoice owner.</summary>
    public async Task<IReadOnlyList<CompanySalesInvoiceDraftOutcome>> GetCompanySalesInvoiceQueueAsync(CompanyScope company,string field,Func<Task> requireAuthority)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();PurchaseOrderOwnership.RequireScope(company);RequireInvoiceSourceId(field);ArgumentNullException.ThrowIfNull(requireAuthority);
        await requireAuthority();var binding=await InvoiceIntentBinding();
        async Task Guard(){await requireAuthority();var current=await InvoiceIntentBinding();if(current.Connection!=binding.Connection || !ReferenceEquals(current.Source,binding.Source))throw new InvalidOperationException("The original invoice queue binding changed.");}
        async Task<INVOICE[]> Read()
        {
            await Guard();RequireCompanyInvoiceSchema(binding.Source);var repo=await GetRepoAsync<INVOICE>("INVOICE",binding.Connection);
            var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="FIELD_ID",Operator="=",FilterValue=field}]))?.ToArray();
            await Guard();if(raw is null || raw.Length>CompanySalesInvoiceQueueReview.MaximumRows || raw.Any(row=>row is not INVOICE))throw new InvalidOperationException("The complete bounded invoice queue is unavailable; no partial total can be shown.");
            var rows=raw.Cast<INVOICE>().OrderBy(row=>row.INVOICE_ID,StringComparer.Ordinal).ToArray();
            if(rows.Any(row=>row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.FIELD_ID!=field) || rows.Select(row=>row.INVOICE_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Length)
                throw new InvalidOperationException("The provider returned foreign or duplicate invoice queue records.");
            return rows.Select(CompanyInventorySourceReview.Copy).ToArray();
        }
        var initial=await Read();var outcomes=new List<CompanySalesInvoiceDraftOutcome>();
        foreach(var row in initial)
        {
            var original=await GetCompanyInvoiceDraftForReviewAsync(company,row.INVOICE_ID,Guard,discovered=>discovered==field?Guard():throw new InvalidOperationException("The queue invoice FIELD changed."));
            if(JsonSerializer.Serialize(original.Invoice)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("The complete native invoice queue record changed.");outcomes.Add(original);
        }
        var closing=await Read();await Guard();if(JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete invoice queue changed during read-back.");return outcomes;
    }
}
