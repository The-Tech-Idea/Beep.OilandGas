using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Accounting.Models;
/// <summary>Durable native AR-to-journal/GL posting correlation, written by the existing AR owner in the journal's owned transaction.</summary>
public sealed class SALES_JOURNAL_POSTING_OUTCOME:ModelEntityBase
{
    public const int MaximumOutcomeLength=134217728;
    [Key][Required][MaxLength(32)]public string SALES_JOURNAL_POSTING_OUTCOME_ID{get;set;}=string.Empty;
    [Required][MaxLength(32)]public string SALES_JOURNAL_POSTING_INTENT_ID{get;set;}=string.Empty;
    [Required][MaxLength(128)]public string OWNER_TENANT_ID{get;set;}=string.Empty;
    [Required][MaxLength(128)]public string OWNER_COMPANY_BA_ID{get;set;}=string.Empty;
    [Required][MaxLength(128)]public string BOOK_ID{get;set;}=string.Empty;
    [Required][MaxLength(16)]public string CURRENCY_CODE{get;set;}=string.Empty;
    [Required][MaxLength(128)]public string FIELD_ID{get;set;}=string.Empty;
    [Required][MaxLength(32)]public string JOURNAL_ENTRY_ID{get;set;}=string.Empty;
    [Required][MaxLength(32)]public string SOURCE_AR_INVOICE_ID{get;set;}=string.Empty;
    [Required][MaxLength(64)]public string SOURCE_REVISION{get;set;}=string.Empty;
    [Required][MaxLength(64)]public string RECORDED_REVISION{get;set;}=string.Empty;
    [Required][MaxLength(MaximumOutcomeLength)]public string OUTCOME_JSON{get;set;}=string.Empty;
    public CompanySalesJournalPostingOutcome Restore()
    {
        if(string.IsNullOrWhiteSpace(OUTCOME_JSON) || OUTCOME_JSON.Length>MaximumOutcomeLength)throw new InvalidOperationException("The complete native posting correlation is unavailable.");var outcome=JsonSerializer.Deserialize<CompanySalesJournalPostingOutcome>(OUTCOME_JSON)??throw new InvalidOperationException("The native posting outcome is malformed.");var command=outcome.Intent.Restore();outcome.Require(command);CompanyGLAccountReview.RequireAudit(this);var source=command.Source;
        if(SALES_JOURNAL_POSTING_OUTCOME_ID!=command.IntentId || SALES_JOURNAL_POSTING_INTENT_ID!=command.IntentId || OWNER_TENANT_ID!=source.Current.Company.TenantId || OWNER_COMPANY_BA_ID!=source.Current.Company.BusinessAssociateId || BOOK_ID!=source.Current.BookId || CURRENCY_CODE!=source.Current.CurrencyCode || FIELD_ID!=source.Current.FieldId || JOURNAL_ENTRY_ID!=source.Reviewed.Header.JOURNAL_ENTRY_ID || SOURCE_AR_INVOICE_ID!=source.Current.ReceivableId || SOURCE_REVISION!=source.Revision || RECORDED_REVISION!=ProcessDefinitionRevision.FromJson(OUTCOME_JSON) || ROW_CREATED_BY!=source.ActorUserId || ROW_CREATED_DATE<outcome.Journal.Header.ROW_CHANGED_DATE || ACTIVE_IND!="Y" || PPDM_GUID!=Guid.ParseExact(command.IntentId,"N").ToString())throw new InvalidOperationException("The complete native AR posting correlation scope, identity, audit or receipt changed.");return outcome;
    }
    public static SALES_JOURNAL_POSTING_OUTCOME Create(CompanySalesJournalPostingOutcome outcome)
    {
        var command=outcome.Intent.Restore();outcome.Require(command);var source=command.Source;var json=JsonSerializer.Serialize(outcome);if(json.Length>MaximumOutcomeLength)throw new InvalidOperationException("The complete native posting correlation exceeds its bound.");return new(){SALES_JOURNAL_POSTING_OUTCOME_ID=command.IntentId,SALES_JOURNAL_POSTING_INTENT_ID=command.IntentId,OWNER_TENANT_ID=source.Current.Company.TenantId,OWNER_COMPANY_BA_ID=source.Current.Company.BusinessAssociateId,BOOK_ID=source.Current.BookId,CURRENCY_CODE=source.Current.CurrencyCode,FIELD_ID=source.Current.FieldId,JOURNAL_ENTRY_ID=source.Reviewed.Header.JOURNAL_ENTRY_ID,SOURCE_AR_INVOICE_ID=source.Current.ReceivableId,SOURCE_REVISION=source.Revision,RECORDED_REVISION=ProcessDefinitionRevision.FromJson(json),OUTCOME_JSON=json,ACTIVE_IND="Y",PPDM_GUID=Guid.ParseExact(command.IntentId,"N").ToString()};
    }
}
