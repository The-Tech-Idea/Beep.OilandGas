using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.Accounting.Models;
public sealed record CompanySalesPostedLedgerLine(string EntryId,string JournalLineId,string AccountId,string AccountNumber,string MappingKey,decimal Debit,decimal Credit);
public sealed record CompanySalesPostedLedgerJournal(string JournalId,string Number,DateTime PostingDate,DateTime PostedAtUtc,string PostedBy,string Maker,string Reviewer,decimal Debit,decimal Credit,string OutcomeRevision,IReadOnlyList<CompanySalesPostedLedgerLine> Lines,[property:JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]CompanyFxConversion? Conversion=null);
/// <summary>Public confirmed native GL posting facts for one explicit AR/book/currency. Private original request/receipt bodies are excluded.</summary>
public sealed record CompanySalesPostedLedgerReview(CompanyScope Company,string ActorUserId,string BindingRevision,string BookId,string CurrencyCode,string ReceivableId,string FieldId,string ReceivableRevision,IReadOnlyList<CompanySalesPostedLedgerJournal> Journals,string Revision)
{
    public static CompanySalesPostedLedgerReview Create(CompanyScope company,string actor,string binding,string book,string currency,string receivable,string field,string receivableRevision,CompanySalesJournalPostingOutcome? outcome)
    {
        _=CompanyGLAccountReview.Create(company,actor,book,currency,[]);IReadOnlyList<CompanySalesPostedLedgerJournal> journals=[];
        if(outcome is not null)
        {
            var command=outcome.Intent.Restore();outcome.Require(command);var source=command.Source.Current;var header=outcome.Journal.Header;
            if(source.Company!=company || source.BindingRevision!=binding || source.BookId!=book || source.CurrencyCode!=currency || source.ReceivableId!=receivable || source.FieldId!=field || ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(outcome.Receivable))!=receivableRevision)throw new InvalidOperationException("The confirmed posting belongs to another native ledger context.");
            var lines=outcome.Entries.Select((row,index)=>new CompanySalesPostedLedgerLine(row.GL_ENTRY_ID,row.JOURNAL_ENTRY_LINE_ID!,row.GL_ACCOUNT_ID,source.Lines[index].AccountNumber,row.MAPPING_KEY!,row.DEBIT_AMOUNT!.Value,row.CREDIT_AMOUNT!.Value)).ToArray();
            journals=[new(header.JOURNAL_ENTRY_ID,header.ENTRY_NUMBER,header.ENTRY_DATE!.Value,header.ROW_CHANGED_DATE!.Value,header.ROW_CHANGED_BY,header.ROW_CREATED_BY,CompanySalesJournalReviewProof.Require(header).Intent.ROW_CREATED_BY,header.TOTAL_DEBIT!.Value,header.TOTAL_CREDIT!.Value,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(outcome)),lines,source.Conversion is null?null:CompanyInventorySourceReview.Copy(source.Conversion))];
        }
        var value=new CompanySalesPostedLedgerReview(company,actor,binding,book,currency,receivable,field,receivableRevision,journals,string.Empty);value=value with{Revision=Digest(value)};value.Require(company,actor,book,currency,receivable);return CompanyInventorySourceReview.Copy(value);
    }
    public void Require(CompanyScope company,string actor,string book,string currency,string receivable)
    {
        _=CompanyGLAccountReview.Create(company,actor,book,currency,[]);BillingPolicyValues.Intent(receivable);BillingPolicyValues.Text(FieldId,128);BillingPolicyValues.Revision(BindingRevision);BillingPolicyValues.Revision(ReceivableRevision);
        if(Company!=company || ActorUserId!=actor || BookId!=book || CurrencyCode!=currency || ReceivableId!=receivable || Journals is null || Journals.Count>1 || Revision!=Digest(this))throw new InvalidOperationException("The complete original public ledger projection context changed.");
        foreach(var row in Journals)
        {
            BillingPolicyValues.Intent(row.JournalId);BillingPolicyValues.Text(row.Number,128);BillingPolicyValues.Text(row.PostedBy,128);BillingPolicyValues.Text(row.Maker,128);BillingPolicyValues.Text(row.Reviewer,128);BillingPolicyValues.Revision(row.OutcomeRevision);CompanyGLMappingReview.RequireDate(row.PostingDate);
            if(row.Conversion is {} conversion){if(conversion.Rates.Books.Company!=company || conversion.Rates.BookId!=book || conversion.Rates.ReviewDate!=row.PostingDate || conversion.Rates.RequireApplicable().TargetCurrency!=currency || conversion.TargetTotal!=row.Debit || conversion.Components is not {Count:3})throw new InvalidOperationException("The confirmed public ledger conversion context changed.");conversion.Require(conversion.Rates,conversion.SourceCurrencyDecimals,conversion.SourceTotal,conversion.Components[0].SourceAmount,conversion.Components[1].SourceAmount,conversion.Components[2].SourceAmount);}
            if(row.Maker==row.Reviewer || row.PostedAtUtc==default || row.PostedAtUtc.Kind!=DateTimeKind.Utc || row.Debit<=0 || row.Debit!=row.Credit || row.Lines is not {Count:>=2 and <=4} || row.Lines.Select(line=>line.EntryId).Distinct(StringComparer.Ordinal).Count()!=row.Lines.Count || row.Lines.Select(line=>line.JournalLineId).Distinct(StringComparer.Ordinal).Count()!=row.Lines.Count || row.Lines.Sum(line=>line.Debit)!=row.Debit || row.Lines.Sum(line=>line.Credit)!=row.Credit)throw new InvalidOperationException("Complete independently reviewed, balanced native posted ledger facts are required.");
            foreach(var line in row.Lines){BillingPolicyValues.Intent(line.EntryId);BillingPolicyValues.Intent(line.JournalLineId);BillingPolicyValues.Text(line.AccountId,128);BillingPolicyValues.Text(line.AccountNumber,128);BillingPolicyValues.Text(line.MappingKey,128);if(line.Debit<0 || line.Credit<0 || (line.Debit==0)==(line.Credit==0))throw new InvalidOperationException("Each exact posted GL line requires one strictly positive side.");}
        }
    }
    private static string Digest(CompanySalesPostedLedgerReview value)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value with{Revision=string.Empty}));
}
