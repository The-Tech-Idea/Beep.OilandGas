using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
namespace TheTechIdea.Data.OilGas;
public sealed class SALES_JOURNAL_POSTING_INTENT:ModelEntityBase
{
    public const int MaximumCommandLength=33554432;
    [Key][Required][MaxLength(32)]public string SALES_JOURNAL_POSTING_INTENT_ID{get;set;}=string.Empty;
    [Required][MaxLength(128)]public string OWNER_TENANT_ID{get;set;}=string.Empty;
    [Required][MaxLength(128)]public string OWNER_COMPANY_BA_ID{get;set;}=string.Empty;
    [Required][MaxLength(128)]public string BOOK_ID{get;set;}=string.Empty;
    [Required][MaxLength(16)]public string CURRENCY_CODE{get;set;}=string.Empty;
    [Required][MaxLength(128)]public string FIELD_ID{get;set;}=string.Empty;
    [Required][MaxLength(32)]public string JOURNAL_ENTRY_ID{get;set;}=string.Empty;
    [Required][MaxLength(32)]public string SOURCE_AR_INVOICE_ID{get;set;}=string.Empty;
    [Required][MaxLength(64)]public string SOURCE_REVISION{get;set;}=string.Empty;
    [Required][MaxLength(MaximumCommandLength)]public string COMMAND_JSON{get;set;}=string.Empty;
    public CompanySalesJournalPostingCommand Restore()
    {
        if(string.IsNullOrWhiteSpace(COMMAND_JSON) || COMMAND_JSON.Length>MaximumCommandLength)throw new InvalidOperationException("The complete original posting request is unavailable.");var command=JsonSerializer.Deserialize<CompanySalesJournalPostingCommand>(COMMAND_JSON)??throw new InvalidOperationException("The original posting request is malformed.");command.Require();CompanyGLAccountReview.RequireAudit(this);var source=command.Source;
        if(SALES_JOURNAL_POSTING_INTENT_ID!=command.IntentId || OWNER_TENANT_ID!=source.Current.Company.TenantId || OWNER_COMPANY_BA_ID!=source.Current.Company.BusinessAssociateId || BOOK_ID!=source.Current.BookId || CURRENCY_CODE!=source.Current.CurrencyCode || FIELD_ID!=source.Current.FieldId || JOURNAL_ENTRY_ID!=source.Reviewed.Header.JOURNAL_ENTRY_ID || SOURCE_AR_INVOICE_ID!=source.Current.ReceivableId || SOURCE_REVISION!=source.Revision || ROW_CREATED_BY!=source.ActorUserId || ACTIVE_IND!="Y" || PPDM_GUID!=Guid.ParseExact(command.IntentId,"N").ToString())throw new InvalidOperationException("The complete original posting intent scope, identity, source or native audit changed.");return command;
    }
    public static SALES_JOURNAL_POSTING_INTENT Create(CompanySalesJournalPostingCommand command)
    {
        command.Require();var source=command.Source;return new(){SALES_JOURNAL_POSTING_INTENT_ID=command.IntentId,OWNER_TENANT_ID=source.Current.Company.TenantId,OWNER_COMPANY_BA_ID=source.Current.Company.BusinessAssociateId,BOOK_ID=source.Current.BookId,CURRENCY_CODE=source.Current.CurrencyCode,FIELD_ID=source.Current.FieldId,JOURNAL_ENTRY_ID=source.Reviewed.Header.JOURNAL_ENTRY_ID,SOURCE_AR_INVOICE_ID=source.Current.ReceivableId,SOURCE_REVISION=source.Revision,COMMAND_JSON=JsonSerializer.Serialize(command),ACTIVE_IND="Y",PPDM_GUID=Guid.ParseExact(command.IntentId,"N").ToString()};
    }
}
