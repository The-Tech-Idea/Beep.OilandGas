using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

/// <summary>Frozen independently approved journal and exact current native AR/configuration evidence. No posted balance is inferred.</summary>
public sealed record CompanySalesJournalPostingSource(string ActorUserId,CompanySalesJournalPreparationOutcome Reviewed,CompanySalesJournalSourceReview Current,AR_INVOICE Receivable,string Revision)
{
    public static CompanySalesJournalPostingSource Create(string actor,CompanySalesJournalPreparationOutcome reviewed,CompanySalesJournalSourceReview current,AR_INVOICE receivable)
    {
        var value=new CompanySalesJournalPostingSource(actor,reviewed,current,receivable,string.Empty);value=value with{Revision=Digest(value)};value.Require();return CompanyInventorySourceReview.Copy(value);
    }
    public void Require()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("journal-posting-source",this);if(CompanySalesInvoiceValidation.Contains(key))return;BillingPolicyValues.Text(ActorUserId,128);ArgumentNullException.ThrowIfNull(Reviewed);ArgumentNullException.ThrowIfNull(Current);ArgumentNullException.ThrowIfNull(Receivable);
        var preparation=CompanySalesJournalPreparationProof.Require(Reviewed.Header).Command;Reviewed.Require(preparation);var review=CompanySalesJournalReviewProof.Require(Reviewed.Header);var decision=review.Intent.Restore();
        if(Reviewed.Header.STATUS!="APPROVED" || decision.Decision!="APPROVE")throw new InvalidOperationException("The exact independent journal approval is required before posting.");
        preparation.RequireCurrent(Current);if(JsonSerializer.Serialize(decision.Review.Original.Lines)!=JsonSerializer.Serialize(Reviewed.Lines))throw new InvalidOperationException("The independently reviewed native lines changed.");
        _=CompanySalesReceivableDecisionProof.Require(Current.Company,Receivable);
        var contributor=CompanySalesReceivableContributor.Create(Current.Company,Receivable);var header=Current.Receivable.Receivables.Single();
        if(Receivable.STATUS!="APPROVED" || contributor.ReceivableId!=Current.ReceivableId || contributor.Revision!=header.NativeRevision || Receivable.FIELD_ID!=Current.FieldId || Receivable.CURRENCY_CODE!=Current.SourceCurrencyCode || Receivable.PAID_AMOUNT!=0 || Receivable.BALANCE_DUE!=header.ProposedTotal || Revision!=Digest(this))throw new InvalidOperationException("The complete exact unposted native receivable and current journal posting source are unconfirmed.");CompanySalesInvoiceValidation.Remember(key);
    }
    public void RequireCurrent(CompanySalesJournalPostingSource current){Require();current.Require();if(JsonSerializer.Serialize(this)!=JsonSerializer.Serialize(current))throw new InvalidOperationException("The exact original reviewed journal or native posting source changed. Reconcile the original posting request.");}
    private static string Digest(CompanySalesJournalPostingSource source)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(source with{Revision=string.Empty}));
}

/// <summary>One original posting request and explicit native GL identities. Authority is enforced by the actual API/native owner.</summary>
public sealed record CompanySalesJournalPostingCommand(string IntentId,CompanySalesJournalPostingSource Source,IReadOnlyList<string> EntryIds,string Reason)
{
    public const string NativeSource="COMPANY_SALES_JOURNAL_POSTING";
    public void Require()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var key=CompanySalesInvoiceValidation.Key("journal-posting-command",this);if(CompanySalesInvoiceValidation.Contains(key))return;
        ArgumentNullException.ThrowIfNull(Source);Source.Require();BillingPolicyValues.Intent(IntentId);BillingPolicyValues.Text(Reason,1000);var preparation=CompanySalesJournalPreparationProof.Require(Source.Reviewed.Header).Command;var review=CompanySalesJournalReviewProof.Require(Source.Reviewed.Header).Intent;
        var reserved=preparation.LineIds.Concat([preparation.IntentId,preparation.JournalId,review.SALES_JOURNAL_REVIEW_INTENT_ID,Source.Current.ReceivableId,Source.Receivable.SOURCE_INVOICE_ID!]).ToArray();
        if(reserved.Contains(IntentId) || EntryIds is null || EntryIds.Count!=Source.Reviewed.Lines.Count || EntryIds.Distinct(StringComparer.Ordinal).Count()!=EntryIds.Count)throw new InvalidOperationException("One distinct original posting request and complete unique native GL identities are required.");
        foreach(var id in EntryIds){BillingPolicyValues.Intent(id);if(id==IntentId || reserved.Contains(id))throw new InvalidOperationException("Posting cannot borrow a journal/source/reviewer/intent identity.");}
        if(JsonSerializer.Serialize(this).Length>SALES_JOURNAL_POSTING_INTENT.MaximumCommandLength)throw new InvalidOperationException("The complete original native posting request exceeds its bound.");CompanySalesInvoiceValidation.Remember(key);
    }
    public GL_ENTRY CreateEntry(int index)
    {
        Require();var row=Source.Reviewed.Lines[index];return new(){GL_ENTRY_ID=EntryIds[index],JOURNAL_ENTRY_ID=Source.Reviewed.Header.JOURNAL_ENTRY_ID,GL_ACCOUNT_ID=row.GL_ACCOUNT_ID,ENTRY_DATE=Source.Current.PostingDate,DEBIT_AMOUNT=row.DEBIT_AMOUNT,CREDIT_AMOUNT=row.CREDIT_AMOUNT,DESCRIPTION=row.DESCRIPTION,REFERENCE_NUMBER=Source.Reviewed.Header.ENTRY_NUMBER,SOURCE_MODULE="COMPANY_SALES_AR",
            OWNER_TENANT_ID=Source.Current.Company.TenantId,OWNER_COMPANY_BA_ID=Source.Current.Company.BusinessAssociateId,BOOK_ID=Source.Current.BookId,CURRENCY_CODE=Source.Current.CurrencyCode,FIELD_ID=Source.Current.FieldId,SOURCE_AR_INVOICE_ID=Source.Current.ReceivableId,COMPANY_LEDGER_PERIOD_ID=Source.Reviewed.Header.COMPANY_LEDGER_PERIOD_ID,JOURNAL_ENTRY_LINE_ID=row.JOURNAL_ENTRY_LINE_ID,SALES_JOURNAL_POSTING_INTENT_ID=IntentId,MAPPING_KEY=row.MAPPING_KEY,
            ACTIVE_IND="Y",SOURCE=NativeSource,REMARK=Reason,PPDM_GUID=Guid.ParseExact(EntryIds[index],"N").ToString()};
    }
}
