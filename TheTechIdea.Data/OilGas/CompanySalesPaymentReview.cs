using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

public sealed record CompanySalesPaymentReviewSummary(string IntentId,string Reviewer,DateTime ReviewedAtUtc,string Decision,string FinancialDisposition,string FinancialEvidence,string ReceiptDisposition,string ReceiptEvidence,string Reason);
public sealed record CompanySalesPaymentReviewSource(AR_PAYMENT Original,CompanySalesPaymentSource Current,string Revision)
{
    public static CompanySalesPaymentReviewSource Create(AR_PAYMENT original,CompanySalesPaymentSource current)
    {var value=new CompanySalesPaymentReviewSource(CompanyInventorySourceReview.Copy(original),CompanyInventorySourceReview.Copy(current),string.Empty);value=value with{Revision=Digest(value)};value.Require();return value;}
    public void Require()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("payment-review-source",this);if(CompanySalesInvoiceValidation.Contains(key))return;
        ArgumentNullException.ThrowIfNull(Original);ArgumentNullException.ThrowIfNull(Current);var maker=CompanySalesPaymentProof.Require(Original).Command;var context=Current.Ledger;Current.Require(context.Company,context.ActorUserId,context.ReceivableId,context.BookId,context.CurrencyCode);
        if(Original.STATUS!="DRAFT" || Original.REVIEW_WORKFLOW_JSON is not null || context.ActorUserId==maker.Source.Ledger.ActorUserId || context.Company!=maker.Source.Ledger.Company || context.BindingRevision!=maker.Source.Ledger.BindingRevision || context.ReceivableId!=Original.AR_INVOICE_ID || context.BookId!=Original.BOOK_ID || context.FieldId!=Original.FIELD_ID || Current.Currency!=Original.CURRENCY_CODE || Current.CustomerId!=maker.Source.CustomerId || Current.OriginalAmount!=maker.Source.OriginalAmount || Current.CurrencyDecimals!=maker.Source.CurrencyDecimals || Revision!=Digest(this))throw new InvalidOperationException("An independent controller, exact original DRAFT and current original posted AR/payment source are required.");
        var fact=Current.Payments.SingleOrDefault(row=>row.Id==Original.AR_PAYMENT_ID);if(fact is null || fact.NativeRevision!=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(Original)) || fact.Status!="DRAFT")throw new InvalidOperationException("Current native payment facts do not confirm this original draft.");CompanySalesInvoiceValidation.Remember(key);
    }
    public void RequireCurrent(CompanySalesPaymentReviewSource current){Require();current.Require();if(JsonSerializer.Serialize(this)!=JsonSerializer.Serialize(current))throw new InvalidOperationException("The original complete payment/ledger/receipt review changed. Retain the original decision.");}
    private static string Digest(CompanySalesPaymentReviewSource value)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value with{Revision=string.Empty}));
}
public sealed record CompanySalesPaymentReviewCommand(string IntentId,CompanySalesPaymentReviewSource Review,string Decision,string FinancialDisposition,string FinancialEvidence,string ReceiptDisposition,string ReceiptEvidence,string Reason)
{
    public void Require()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("payment-review-command",this);if(CompanySalesInvoiceValidation.Contains(key))return;
        ArgumentNullException.ThrowIfNull(Review);Review.Require();BillingPolicyValues.Intent(IntentId);var maker=CompanySalesPaymentProof.Require(Review.Original).Command;
        if(IntentId==maker.IntentId || IntentId==maker.Id || Decision is not ("APPROVE" or "REJECT"))throw new InvalidOperationException("A distinct original reviewer intent and explicit decision are required.");
        foreach(var value in new[]{FinancialDisposition,ReceiptDisposition})if(value is not ("CONFIRMED" or "ISSUE"))throw new InvalidOperationException("Explicit financial and receipt dispositions are required.");
        BillingPolicyValues.Text(FinancialEvidence,2000);BillingPolicyValues.Text(ReceiptEvidence,2000);BillingPolicyValues.Text(Reason,1000);if(Decision=="APPROVE" && (FinancialDisposition!="CONFIRMED" || ReceiptDisposition!="CONFIRMED"))throw new InvalidOperationException("Approval requires confirmed financial and receipt evidence.");
        if(JsonSerializer.Serialize(this).Length>SALES_PAYMENT_REVIEW_INTENT.MaximumCommandLength)throw new InvalidOperationException("The original reviewer request exceeds native capacity.");CompanySalesInvoiceValidation.Remember(key);
    }
}
public sealed class SALES_PAYMENT_REVIEW_INTENT:ModelEntityBase
{
    public const int MaximumCommandLength=4194304;
    [Key,Required,MaxLength(32)]public string SALES_PAYMENT_REVIEW_INTENT_ID{get;set;}=string.Empty;
    [Required,MaxLength(128)]public string OWNER_TENANT_ID{get;set;}=string.Empty;
    [Required,MaxLength(128)]public string OWNER_COMPANY_BA_ID{get;set;}=string.Empty;
    [Required,MaxLength(128)]public string BOOK_ID{get;set;}=string.Empty;
    [Required,MaxLength(16)]public string CURRENCY_CODE{get;set;}=string.Empty;
    [Required,MaxLength(128)]public string FIELD_ID{get;set;}=string.Empty;
    [Required,MaxLength(32)]public string AR_PAYMENT_ID{get;set;}=string.Empty;
    [Required,MaxLength(32)]public string AR_INVOICE_ID{get;set;}=string.Empty;
    [Required,MaxLength(64)]public string SOURCE_REVISION{get;set;}=string.Empty;
    [Required,MaxLength(MaximumCommandLength)]public string COMMAND_JSON{get;set;}=string.Empty;
    public static SALES_PAYMENT_REVIEW_INTENT Create(CompanySalesPaymentReviewCommand command)
    {command.Require();var source=command.Review.Current;return new(){SALES_PAYMENT_REVIEW_INTENT_ID=command.IntentId,OWNER_TENANT_ID=source.Ledger.Company.TenantId,OWNER_COMPANY_BA_ID=source.Ledger.Company.BusinessAssociateId,BOOK_ID=source.Ledger.BookId,CURRENCY_CODE=source.Currency,FIELD_ID=source.Ledger.FieldId,AR_PAYMENT_ID=command.Review.Original.AR_PAYMENT_ID,AR_INVOICE_ID=source.Ledger.ReceivableId,SOURCE_REVISION=command.Review.Revision,COMMAND_JSON=JsonSerializer.Serialize(command),ACTIVE_IND="Y",PPDM_GUID=Guid.ParseExact(command.IntentId,"N").ToString()};}
    public CompanySalesPaymentReviewCommand Restore()
    {
        if(string.IsNullOrWhiteSpace(COMMAND_JSON) || COMMAND_JSON.Length>MaximumCommandLength)throw new InvalidOperationException("The original payment reviewer command is unavailable.");var command=JsonSerializer.Deserialize<CompanySalesPaymentReviewCommand>(COMMAND_JSON)??throw new InvalidOperationException("The original payment reviewer command is malformed.");command.Require();CompanyGLAccountReview.RequireAudit(this);var expected=Create(command);expected.ROW_CREATED_BY=ROW_CREATED_BY;expected.ROW_CREATED_DATE=ROW_CREATED_DATE;expected.ROW_CHANGED_BY=ROW_CHANGED_BY;expected.ROW_CHANGED_DATE=ROW_CHANGED_DATE;
        if(ROW_CREATED_BY!=command.Review.Current.Ledger.ActorUserId || ROW_CHANGED_DATE is not null && ROW_CHANGED_BY!=ROW_CREATED_BY || JsonSerializer.Serialize(this)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The complete original native payment reviewer journal or audit changed.");return command;
    }
}
public sealed record CompanySalesPaymentReviewProof(SALES_PAYMENT_REVIEW_INTENT Intent,DateTime ReviewedAtUtc,string RowRevision)
{
    public const int MaximumLength=8388608;
    public static string Create(SALES_PAYMENT_REVIEW_INTENT intent,AR_PAYMENT row)
    {var value=new CompanySalesPaymentReviewProof(CompanyInventorySourceReview.Copy(intent),row.ROW_CHANGED_DATE??throw new InvalidOperationException("Native reviewer UTC audit is required."),Digest(row));value.RequireFacts(row);var json=JsonSerializer.Serialize(value);if(json.Length>MaximumLength)throw new InvalidOperationException("The original payment review proof exceeds capacity.");return json;}
    public static CompanySalesPaymentReviewProof Require(AR_PAYMENT row)
    {if(string.IsNullOrWhiteSpace(row.REVIEW_WORKFLOW_JSON) || row.REVIEW_WORKFLOW_JSON.Length>MaximumLength)throw new InvalidOperationException("The original payment review proof is unavailable.");var value=JsonSerializer.Deserialize<CompanySalesPaymentReviewProof>(row.REVIEW_WORKFLOW_JSON)??throw new InvalidOperationException("The original review proof is malformed.");value.RequireFacts(row);if(value.RowRevision!=Digest(row))throw new InvalidOperationException("The complete reviewed payment changed.");return value;}
    private void RequireFacts(AR_PAYMENT row)
    {
        var command=Intent.Restore();var original=command.Review.Original;CompanyGLAccountReview.RequireAudit(row);
        if(row.STATUS!=(command.Decision=="APPROVE"?"APPROVED":"REJECTED") || row.ROW_CHANGED_BY!=command.Review.Current.Ledger.ActorUserId || row.ROW_CHANGED_DATE!=ReviewedAtUtc || ReviewedAtUtc.Kind!=DateTimeKind.Utc || ReviewedAtUtc==default || ReviewedAtUtc<Intent.ROW_CREATED_DATE || ReviewedAtUtc<(original.ROW_CHANGED_DATE??original.ROW_CREATED_DATE))throw new InvalidOperationException("Independent payment decision and ordered original native UTC audit are unconfirmed.");
        var normalized=CompanyInventorySourceReview.Copy(row);normalized.REVIEW_WORKFLOW_JSON=null!;normalized.STATUS=original.STATUS;normalized.ROW_CHANGED_BY=original.ROW_CHANGED_BY;normalized.ROW_CHANGED_DATE=original.ROW_CHANGED_DATE;if(JsonSerializer.Serialize(normalized)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Payment review changed original money, source, maker, reference or preparation receipt.");
    }
    public CompanySalesPaymentReviewSummary Summary(){var command=Intent.Restore();return new(command.IntentId,command.Review.Current.Ledger.ActorUserId,ReviewedAtUtc,command.Decision,command.FinancialDisposition,command.FinancialEvidence,command.ReceiptDisposition,command.ReceiptEvidence,command.Reason);}
    public static AR_PAYMENT OriginalDraft(AR_PAYMENT row)=>CompanyInventorySourceReview.Copy(Require(row).Intent.Restore().Review.Original);
    private static string Digest(AR_PAYMENT row){var copy=CompanyInventorySourceReview.Copy(row);copy.REVIEW_WORKFLOW_JSON=null!;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
}
public sealed record CompanySalesPaymentReviewRecovery(CompanyScope Company,string ActorUserId,string PaymentId,SALES_PAYMENT_REVIEW_INTENT? Intent,AR_PAYMENT? Outcome)
{
    public void Require()
    {PurchaseOrderOwnership.RequireScope(Company);BillingPolicyValues.Text(ActorUserId,128);BillingPolicyValues.Intent(PaymentId);if(Intent is null){if(Outcome is not null)throw new InvalidOperationException("A reviewed payment needs its original reviewer journal.");return;}var command=Intent.Restore();if(command.Review.Current.Ledger.Company!=Company || command.Review.Current.Ledger.ActorUserId!=ActorUserId || command.Review.Original.AR_PAYMENT_ID!=PaymentId)throw new UnauthorizedAccessException("Only the original reviewer can recover this private payment decision.");if(Outcome is not null && JsonSerializer.Serialize(CompanySalesPaymentReviewProof.Require(Outcome).Intent)!=JsonSerializer.Serialize(Intent))throw new InvalidOperationException("The reviewed payment does not confirm its retained original decision.");}
}
