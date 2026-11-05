using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Accounting;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Recorded native invoice headers. Amounts do not establish posted receivables or settlement.</summary>
public sealed record CompanySalesInvoiceQueueRow(string InvoiceId,string Number,string BuyerId,string TicketId,string Currency,string Status,
    DateTime InvoiceDate,DateTime DueDate,decimal ProposedTotal,string MakerId,string? ReviewerId,DateTime? ReviewedUtc,string NativeRevision,string PreparationRevision);

public sealed record CompanySalesInvoiceQueueTotal(string Currency,string Status,int Count,decimal ProposedTotal);

public sealed record CompanySalesInvoiceQueueReview(CompanyScope Company,string ActorUserId,string FieldId,string BindingRevision,
    IReadOnlyList<CompanySalesInvoiceQueueRow> Rows,string Revision)
{
    public const int MaximumRows=128;
    [JsonIgnore] public IReadOnlyList<CompanySalesInvoiceQueueTotal> Totals=>Rows.GroupBy(row=>(row.Currency,row.Status))
        .OrderBy(group=>group.Key.Currency,StringComparer.Ordinal).ThenBy(group=>group.Key.Status,StringComparer.Ordinal)
        .Select(group=>new CompanySalesInvoiceQueueTotal(group.Key.Currency,group.Key.Status,group.Count(),group.Sum(row=>row.ProposedTotal))).ToArray();
    public static CompanySalesInvoiceQueueReview Create(CompanyScope company,string actor,string field,string binding,IEnumerable<CompanySalesInvoiceDraftOutcome> outcomes)
    {
        var rows=outcomes.Select(outcome=>
        {
            outcome.Require(outcome.Intent.Restore(company,outcome.Intent.ROW_CREATED_BY));var row=outcome.Invoice;
            if(row.FIELD_ID!=field)throw new InvalidOperationException("The invoice belongs to another FIELD.");
            var decision=row.REVIEW_WORKFLOW_JSON is null?null:CompanySalesInvoiceDecisionProof.Require(company,row);
            return new CompanySalesInvoiceQueueRow(row.INVOICE_ID,row.INVOICE_NUMBER,row.CUSTOMER_BA_ID,row.SOURCE_RUN_TICKET_ID!,row.CURRENCY_CODE,row.STATUS,
                row.INVOICE_DATE!.Value,row.DUE_DATE!.Value,row.TOTAL_AMOUNT,outcome.Intent.ROW_CREATED_BY,decision?.Intent.ROW_CREATED_BY,decision?.DecidedAtUtc,
                ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(row)),outcome.Receipt.RECORDED_REVISION);
        }).OrderBy(row=>row.InvoiceId,StringComparer.Ordinal).ToArray();
        var result=new CompanySalesInvoiceQueueReview(company,actor,field,binding,rows,string.Empty);result=result with{Revision=Digest(result)};
        result.Require(company,actor,field);return result;
    }
    public void Require(CompanyScope company,string actor,string field)
    {
        PurchaseOrderOwnership.RequireScope(company);
        static bool Id(string? value,int size=128)=>!string.IsNullOrWhiteSpace(value)&&value.Length<=size&&!value.Any(char.IsControl);
        if(Company!=company || ActorUserId!=actor || FieldId!=field || !Id(actor) || !Id(field) || !ProcessDefinitionRevision.IsValid(BindingRevision) ||
            Rows is null || Rows.Count>MaximumRows || Rows.Any(row=>row is null) || Rows.Select(row=>row.InvoiceId).Distinct(StringComparer.Ordinal).Count()!=Rows.Count ||
            !Rows.Select(row=>row.InvoiceId).SequenceEqual(Rows.Select(row=>row.InvoiceId).Order(StringComparer.Ordinal)))throw new InvalidOperationException("The complete current company/account/FIELD invoice queue is unconfirmed.");
        foreach(var row in Rows)
            if(!Guid.TryParseExact(row.InvoiceId,"N",out var id) || id==Guid.Empty || !Id(row.Number) || !Id(row.BuyerId) || !Id(row.TicketId) || !Id(row.Currency,16) ||
                !Id(row.MakerId) || row.Status is not ("DRAFT" or "APPROVED" or "REJECTED") || row.ProposedTotal<=0 ||
                row.InvoiceDate.Kind!=DateTimeKind.Unspecified || row.InvoiceDate.TimeOfDay!=TimeSpan.Zero || row.DueDate.Kind!=DateTimeKind.Unspecified || row.DueDate.TimeOfDay!=TimeSpan.Zero || row.DueDate<row.InvoiceDate ||
                !ProcessDefinitionRevision.IsValid(row.NativeRevision) || !ProcessDefinitionRevision.IsValid(row.PreparationRevision) ||
                (row.Status=="DRAFT" ? row.ReviewerId is not null || row.ReviewedUtc is not null : !Id(row.ReviewerId) || row.ReviewerId==row.MakerId || row.ReviewedUtc?.Kind!=DateTimeKind.Utc))
                throw new InvalidOperationException("Recorded native invoice queue facts are incomplete or inconsistent.");
        try{_=Totals;}catch(OverflowException){throw new InvalidOperationException("The recorded currency/status amount exceeds supported precision.");}
        if(Revision!=Digest(this))throw new InvalidOperationException("The transported invoice queue differs from its recorded revision.");
    }
    private static string Digest(CompanySalesInvoiceQueueReview review)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(review with{Revision=string.Empty}));
}
