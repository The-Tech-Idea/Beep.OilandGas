using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

public sealed record CompanySalesReceivableContributor(string ReceivableId,string InvoiceId,string FieldId,string Currency,string Status,decimal ProposedTotal,string Revision)
{
    public static CompanySalesReceivableContributor Create(CompanyScope company,AR_INVOICE row)
    {_=CompanySalesReceivableProof.Require(company,row);return new(row.AR_INVOICE_ID,row.SOURCE_INVOICE_ID!,row.FIELD_ID!,row.CURRENCY_CODE!,row.STATUS,row.TOTAL_AMOUNT!.Value,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(row)));}
    public void Require(string invoice,string field,string currency)
    {
        BillingPolicyValues.Intent(ReceivableId);
        if(InvoiceId!=invoice || FieldId!=field || Currency!=currency || Status is not ("DRAFT" or "APPROVED" or "REJECTED") || ProposedTotal<=0 || !ProcessDefinitionRevision.IsValid(Revision))throw new InvalidOperationException("The owned receivable contributor is unconfirmed.");
    }
}

/// <summary>Approved native invoice snapshot and complete current physical/policy revision; no posted receivable is implied.</summary>
public sealed record CompanySalesReceivableSource(CompanyScope Company,string ActorUserId,string FieldId,string BindingRevision,INVOICE Invoice,
    string InvoicePreparationRevision,string CurrentInvoiceReviewRevision,IReadOnlyList<CompanySalesReceivableContributor> Receivables,string Revision)
{
    [JsonIgnore]public string InvoiceId=>Invoice.INVOICE_ID;
    [JsonIgnore]public bool CanPrepare=>Receivables.Count==0;
    public const int MaximumReceivables=32;
    public static CompanySalesReceivableSource Create(CompanySalesInvoiceReviewSource source,IEnumerable<AR_INVOICE> rows)
    {
        source.Require(source.Current.Company,source.Current.ActorUserId,source.InvoiceId);
        if(source.Original.Invoice.STATUS!="APPROVED" || !source.HasCurrentOriginalSource)throw new InvalidOperationException("An independently approved invoice with unchanged current receiving and billing policy evidence is required.");
        var result=new CompanySalesReceivableSource(source.Current.Company,source.Current.ActorUserId,source.Current.FieldId,source.Current.BindingRevision,
            CompanyInventorySourceReview.Copy(source.Original.Invoice),source.Original.Receipt.RECORDED_REVISION,source.Revision,
            rows.Select(row=>CompanySalesReceivableContributor.Create(source.Current.Company,row)).OrderBy(row=>row.ReceivableId,StringComparer.Ordinal).ToArray(),string.Empty);
        result=result with{Revision=Digest(result)};result.Require(result.Company,result.ActorUserId,result.InvoiceId);return result;
    }
    public void Require(CompanyScope company,string actor,string invoice)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("receivable-source",this,new{company,actor,invoice});if(CompanySalesInvoiceValidation.Contains(key))return;
        PurchaseOrderOwnership.RequireScope(company);BillingPolicyValues.Text(actor,128);BillingPolicyValues.Text(FieldId,128);
        if(Company!=company || ActorUserId!=actor || Invoice is null || InvoiceId!=invoice || Invoice.FIELD_ID!=FieldId || Invoice.STATUS!="APPROVED" ||
            !ProcessDefinitionRevision.IsValid(BindingRevision) || !ProcessDefinitionRevision.IsValid(InvoicePreparationRevision) || !ProcessDefinitionRevision.IsValid(CurrentInvoiceReviewRevision) ||
            Receivables is null || Receivables.Count>MaximumReceivables || Receivables.Any(row=>row is null) || Receivables.Select(row=>row.ReceivableId).Distinct(StringComparer.Ordinal).Count()!=Receivables.Count ||
            !Receivables.Select(row=>row.ReceivableId).SequenceEqual(Receivables.Select(row=>row.ReceivableId).Order(StringComparer.Ordinal)))throw new InvalidOperationException("The complete company/account approved invoice receivable source is unconfirmed.");
        var proof=CompanySalesInvoiceDecisionProof.Require(company,Invoice);if(proof.Intent.Restore(company,proof.Intent.ROW_CREATED_BY).Decision!="APPROVE")throw new InvalidOperationException("The original independent invoice approval is required.");
        foreach(var row in Receivables)row.Require(invoice,FieldId,Invoice.CURRENCY_CODE);
        if(Revision!=Digest(this))throw new InvalidOperationException("The complete current receivable source revision changed.");CompanySalesInvoiceValidation.Remember(key);
    }
    internal static string Digest(CompanySalesReceivableSource source)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(source with{Revision=string.Empty}));
}

public sealed record CompanySalesReceivableCommand(string IntentId,string ReceivableId,CompanySalesReceivableSource Source,string Number,string Reason)
{
    public void Require()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("receivable-command",this);if(CompanySalesInvoiceValidation.Contains(key))return;
        BillingPolicyValues.Intent(IntentId);BillingPolicyValues.Intent(ReceivableId);BillingPolicyValues.Text(Number,128);BillingPolicyValues.Text(Reason,1000);
        if(Source is null || IntentId==ReceivableId || IntentId==Source.InvoiceId || ReceivableId==Source.InvoiceId)throw new InvalidOperationException("Distinct original receivable intent, document and invoice identities are required.");
        Source.Require(Source.Company,Source.ActorUserId,Source.InvoiceId);if(!Source.CanPrepare)throw new InvalidOperationException("An existing receivable must be reconciled before another proposal.");
        if(JsonSerializer.Serialize(this).Length>SALES_RECEIVABLE_INTENT.MaximumCommandLength)throw new InvalidOperationException("The complete original receivable preparation exceeds its journal bound.");CompanySalesInvoiceValidation.Remember(key);
    }
    public void RequireCurrent(CompanySalesReceivableSource current)
    {Require();current.Require(Source.Company,Source.ActorUserId,Source.InvoiceId);if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(Source))throw new InvalidOperationException("The complete original invoice/receivable source changed.");}
    public void RequireAfterInsertion(CompanySalesReceivableSource current,AR_INVOICE row)
    {
        Require();current.Require(Source.Company,Source.ActorUserId,Source.InvoiceId);
        if(current.Receivables.Count!=1 || JsonSerializer.Serialize(current.Receivables[0])!=JsonSerializer.Serialize(CompanySalesReceivableContributor.Create(Source.Company,row)))throw new InvalidOperationException("The unique inserted receivable contributor is unconfirmed.");
        var normalized=current with{Receivables=[],Revision=string.Empty};normalized=normalized with{Revision=CompanySalesReceivableSource.Digest(normalized)};RequireCurrent(normalized);
    }
}

public sealed record CompanySalesReceivableRecord(CompanySalesReceivableCommand Command,DateTime PreparedAtUtc)
{
    public void Require()
    {
        Command.Require();var decision=CompanySalesInvoiceDecisionProof.Require(Command.Source.Company,Command.Source.Invoice);
        if(PreparedAtUtc==default || PreparedAtUtc.Kind!=DateTimeKind.Utc || PreparedAtUtc<decision.DecidedAtUtc)throw new InvalidOperationException("Ordered UTC receivable preparation after independent sales invoice approval is required.");
    }
}
public sealed record CompanySalesReceivableProof(CompanySalesReceivableRecord Record,string RowRevision)
{
    public const int MaximumLength=134217728;
    private static string Digest(AR_INVOICE row){var copy=CompanyInventorySourceReview.Copy(row);copy.PREPARATION_WORKFLOW_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static AR_INVOICE Prepare(CompanySalesReceivableCommand command,DateTime at)
    {
        command=CompanyInventorySourceReview.Copy(command);var record=new CompanySalesReceivableRecord(command,at);record.Require();var invoice=command.Source.Invoice;
        var row=new AR_INVOICE{AR_INVOICE_ID=command.ReceivableId,INVOICE_NUMBER=command.Number,CUSTOMER_BA_ID=invoice.CUSTOMER_BA_ID,
            OWNER_TENANT_ID=command.Source.Company.TenantId,OWNER_COMPANY_BA_ID=command.Source.Company.BusinessAssociateId,FIELD_ID=command.Source.FieldId,SOURCE_INVOICE_ID=invoice.INVOICE_ID,CURRENCY_CODE=invoice.CURRENCY_CODE,
            INVOICE_DATE=invoice.INVOICE_DATE,DUE_DATE=invoice.DUE_DATE,TOTAL_AMOUNT=invoice.TOTAL_AMOUNT,PAID_AMOUNT=0m,BALANCE_DUE=invoice.TOTAL_AMOUNT,STATUS="DRAFT",ACTIVE_IND="Y",PPDM_GUID=command.IntentId,
            ROW_CREATED_BY=command.Source.ActorUserId,ROW_CHANGED_BY=command.Source.ActorUserId,ROW_CREATED_DATE=at,ROW_CHANGED_DATE=at};
        row.PREPARATION_WORKFLOW_JSON=Create(record,row);_=Require(command.Source.Company,row);return row;
    }
    public static string Create(CompanySalesReceivableRecord record,AR_INVOICE row)
    {record.Require();var json=JsonSerializer.Serialize(new CompanySalesReceivableProof(record,Digest(row)));if(json.Length>MaximumLength)throw new InvalidOperationException("The complete native receivable preparation proof exceeds its column bound.");return json;}
    public static CompanySalesReceivableProof Require(CompanyScope company,AR_INVOICE row)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();PurchaseOrderOwnership.RequireScope(company);
        var native=row;
        if(row.REVIEW_WORKFLOW_JSON is not null)row=CompanySalesReceivableDecisionProof.OriginalDraft(company,row);
        if(string.IsNullOrWhiteSpace(row.PREPARATION_WORKFLOW_JSON) || row.PREPARATION_WORKFLOW_JSON.Length>MaximumLength)throw new InvalidOperationException("The complete native receivable preparation proof is required.");
        CompanySalesReceivableProof proof;
        try{proof=JsonSerializer.Deserialize<CompanySalesReceivableProof>(row.PREPARATION_WORKFLOW_JSON,new JsonSerializerOptions{UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow}) ??throw new InvalidOperationException("Receivable preparation proof cannot be null.");}
        catch(JsonException){throw new InvalidOperationException("Receivable preparation proof is malformed.");}
        var key=CompanySalesInvoiceValidation.Key("receivable-row",native,company);if(CompanySalesInvoiceValidation.Contains(key))return proof;
        if(proof.Record is null)throw new InvalidOperationException("The exact original receivable record is required.");proof.Record.Require();var command=proof.Record.Command;var source=command.Source;var invoice=source.Invoice;
        if(source.Company!=company || row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.FIELD_ID!=source.FieldId || row.SOURCE_INVOICE_ID!=source.InvoiceId || row.CURRENCY_CODE!=invoice.CURRENCY_CODE ||
            row.AR_INVOICE_ID!=command.ReceivableId || row.INVOICE_NUMBER!=command.Number || row.CUSTOMER_BA_ID!=invoice.CUSTOMER_BA_ID || row.INVOICE_DATE!=invoice.INVOICE_DATE || row.INVOICE_DATE?.Kind!=DateTimeKind.Unspecified || row.DUE_DATE!=invoice.DUE_DATE || row.DUE_DATE?.Kind!=DateTimeKind.Unspecified ||
            row.TOTAL_AMOUNT!=invoice.TOTAL_AMOUNT || row.PAID_AMOUNT!=0m || row.BALANCE_DUE!=invoice.TOTAL_AMOUNT || row.STATUS!="DRAFT" || row.ACTIVE_IND!="Y" || row.PPDM_GUID!=command.IntentId ||
            row.ROW_CREATED_BY!=source.ActorUserId || row.ROW_CHANGED_BY!=source.ActorUserId || row.ROW_CREATED_DATE!=proof.Record.PreparedAtUtc || row.ROW_CHANGED_DATE is not {} changed || changed.Kind!=DateTimeKind.Utc || changed<proof.Record.PreparedAtUtc || proof.RowRevision!=Digest(row))
            throw new InvalidOperationException("Native receivable financial, source, company and original preparation facts disagree.");CompanySalesInvoiceValidation.Remember(key);return proof;
    }
}

public sealed record CompanySalesReceivableOutcome(SALES_RECEIVABLE_INTENT Intent,AR_INVOICE Receivable,SALES_RECEIVABLE_OUTCOME Receipt)
{
    public void Require(CompanySalesReceivableCommand command)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("receivable-outcome",this,command);if(CompanySalesInvoiceValidation.Contains(key))return;
        command.Require();if(Intent is null || Receivable is null || Receipt is null)throw new InvalidOperationException("The complete original receivable intent, native draft and outcome receipt are required.");
        var retained=Intent.Restore(command.Source.Company,command.Source.ActorUserId);var proof=CompanySalesReceivableProof.Require(command.Source.Company,Receivable);
        if(JsonSerializer.Serialize(retained)!=JsonSerializer.Serialize(command) || JsonSerializer.Serialize(proof.Record.Command)!=JsonSerializer.Serialize(command) || proof.Record.PreparedAtUtc<Intent.ROW_CREATED_DATE || JsonSerializer.Serialize(Receipt.Restore(Intent))!=JsonSerializer.Serialize(CompanySalesReceivableDecisionProof.OriginalDraft(command.Source.Company,Receivable)))throw new InvalidOperationException("The native receivable differs from its original journal or receipt.");
        CompanySalesInvoiceValidation.Remember(key);
    }
}
