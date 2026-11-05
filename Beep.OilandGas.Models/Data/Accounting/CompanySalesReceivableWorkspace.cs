using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Accounting;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

public sealed record CompanySalesReceivableStatusRow(string Id,string Number,string Maker,string Status,string Currency,decimal ProposedTotal,
    DateTime InvoiceDate,DateTime DueDate,DateTime PreparedAtUtc,string NativeRevision,string ReceiptRevision,
    [property:System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]string? ReviewerId=null,
    [property:System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]DateTime? ReviewedUtc=null);

/// <summary>Public recorded headers; original private AR commands are excluded.</summary>
public sealed record CompanySalesReceivableStatusReview(CompanyScope Company,string ActorUserId,string BindingRevision,CompanySalesInvoiceQueueRow Invoice,
    string FieldId,IReadOnlyList<CompanySalesReceivableStatusRow> Receivables,string Revision)
{
    public static CompanySalesReceivableStatusReview Create(CompanyScope company,string actor,string binding,CompanySalesInvoiceDraftOutcome invoice,IEnumerable<CompanySalesReceivableOutcome> outcomes)
    {
        var header=CompanySalesInvoiceQueueReview.Create(company,actor,invoice.Invoice.FIELD_ID!,binding,[invoice]).Rows.Single();
        var rows=outcomes.Select(outcome=>{
            var command=outcome.Intent.Restore(company,outcome.Intent.ROW_CREATED_BY);outcome.Require(command);var row=outcome.Receivable;
            if(command.Source.InvoiceId!=header.InvoiceId || command.Source.InvoicePreparationRevision!=header.PreparationRevision || row.FIELD_ID!=invoice.Invoice.FIELD_ID || row.CUSTOMER_BA_ID!=header.BuyerId)
                throw new InvalidOperationException("The recorded receivable does not belong to the original invoice receipt and FIELD.");
            var review=row.REVIEW_WORKFLOW_JSON is null?null:CompanySalesReceivableDecisionProof.Require(company,row);
            return new CompanySalesReceivableStatusRow(row.AR_INVOICE_ID,row.INVOICE_NUMBER,row.ROW_CREATED_BY,row.STATUS,row.CURRENCY_CODE!,row.TOTAL_AMOUNT!.Value,
                row.INVOICE_DATE!.Value,row.DUE_DATE!.Value,row.ROW_CREATED_DATE!.Value,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(row)),outcome.Receipt.RECORDED_REVISION,review?.Intent.ROW_CREATED_BY,review?.DecidedAtUtc);
        }).OrderBy(row=>row.Id,StringComparer.Ordinal).ToArray();
        var result=new CompanySalesReceivableStatusReview(company,actor,binding,header,invoice.Invoice.FIELD_ID!,rows,string.Empty);result=result with{Revision=Digest(result)};result.Require(company,actor,header.InvoiceId);return result;
    }
    public void Require(CompanyScope company,string actor,string invoice)
    {
        PurchaseOrderOwnership.RequireScope(company);BillingPolicyValues.Text(actor,128);BillingPolicyValues.Text(FieldId,128);
        if(Company!=company || ActorUserId!=actor || Invoice is null || Invoice.InvoiceId!=invoice || !ProcessDefinitionRevision.IsValid(BindingRevision) || Receivables is null || Receivables.Count>CompanySalesReceivableSource.MaximumReceivables ||
            Receivables.Any(row=>row is null) || Receivables.Select(row=>row.Id).Distinct(StringComparer.Ordinal).Count()!=Receivables.Count || !Receivables.Select(row=>row.Id).SequenceEqual(Receivables.Select(row=>row.Id).Order(StringComparer.Ordinal)))throw new InvalidOperationException("The complete recorded invoice/receivable status is unconfirmed.");
        var queue=new CompanySalesInvoiceQueueReview(company,actor,FieldId,BindingRevision,[Invoice],string.Empty);
        queue=queue with{Revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(queue))};queue.Require(company,actor,FieldId);
        foreach(var row in Receivables)
        {
            BillingPolicyValues.Intent(row.Id);BillingPolicyValues.Text(row.Number,128);BillingPolicyValues.Text(row.Maker,128);
            if(row.Status is not ("DRAFT" or "APPROVED" or "REJECTED") || row.Currency!=Invoice.Currency || row.ProposedTotal!=Invoice.ProposedTotal || row.InvoiceDate!=Invoice.InvoiceDate || row.InvoiceDate.Kind!=DateTimeKind.Unspecified || row.DueDate!=Invoice.DueDate || row.DueDate.Kind!=DateTimeKind.Unspecified ||
                row.PreparedAtUtc==default || row.PreparedAtUtc.Kind!=DateTimeKind.Utc || !ProcessDefinitionRevision.IsValid(row.NativeRevision) || !ProcessDefinitionRevision.IsValid(row.ReceiptRevision))throw new InvalidOperationException("Recorded receivable financial/source facts disagree.");
            if(row.Status=="DRAFT" ? row.ReviewerId is not null || row.ReviewedUtc is not null : string.IsNullOrWhiteSpace(row.ReviewerId) || row.ReviewerId==row.Maker || row.ReviewedUtc is not {} reviewed || reviewed.Kind!=DateTimeKind.Utc || reviewed<row.PreparedAtUtc)throw new InvalidOperationException("The recorded independent receivable reviewer audit is unconfirmed.");
        }
        if(Revision!=Digest(this))throw new InvalidOperationException("The complete transported receivable status revision changed.");
    }
    private static string Digest(CompanySalesReceivableStatusReview review)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(review with{Revision=string.Empty}));
}

public sealed record CompanySalesReceivableRecovery(CompanyScope Company,string ActorUserId,string InvoiceId,SALES_RECEIVABLE_INTENT? Retained,CompanySalesReceivableOutcome? Outcome)
{
    public void Require(CompanyScope company,string actor,string invoice)
    {
        PurchaseOrderOwnership.RequireScope(company);BillingPolicyValues.Text(actor,128);BillingPolicyValues.Text(invoice,128);
        if(Company!=company || ActorUserId!=actor || InvoiceId!=invoice || Retained is null && Outcome is not null)throw new InvalidOperationException("The original receivable recovery scope is unconfirmed.");
        if(Retained is null)return;var command=Retained.Restore(company,actor);
        if(command.Source.InvoiceId!=invoice)throw new InvalidOperationException("The original receivable intent belongs to another invoice.");
        if(Outcome is not null){Outcome.Require(command);if(JsonSerializer.Serialize(Outcome.Intent)!=JsonSerializer.Serialize(Retained))throw new InvalidOperationException("The original receivable journal differs from its committed outcome.");}
    }
}
public sealed record CompanySalesReceivableWorkspaceReview(CompanySalesReceivableSource Source,CompanySalesReceivableRecovery Recovery)
{
    public void Require(CompanyScope company,string actor,string invoice)
    {
        if(Source is null || Recovery is null)throw new InvalidOperationException("Complete receivable source and original recovery evidence are required.");Source.Require(company,actor,invoice);Recovery.Require(company,actor,invoice);
        if(Recovery.Outcome is {} outcome && (Source.Receivables.Count!=1 || JsonSerializer.Serialize(Source.Receivables[0])!=JsonSerializer.Serialize(CompanySalesReceivableContributor.Create(company,outcome.Receivable))))
            throw new InvalidOperationException("The committed original receivable differs from the current complete source.");
        if(Recovery.Outcome is null && Source.Receivables.Count!=0)throw new InvalidOperationException("An existing receivable belongs to another original command. Review its recorded status rather than preparing again.");
    }
}
